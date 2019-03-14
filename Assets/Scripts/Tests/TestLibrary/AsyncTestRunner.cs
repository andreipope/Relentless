using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using NUnit.Framework;
using UnityEngine;

namespace Loom.ZombieBattleground.Test
{
    public class AsyncTestRunner
    {
        private static readonly ILog Log = Logging.GetLog(nameof(AsyncTestRunner));

        private const int FlappyErrorMaxRetryCount = 5;

        private static readonly string[] KnownErrors =
        {
            "Sub-emitters must be children of the system that spawns them",
            "Invalid SortingGroup index set in Renderer"
        };

        private static readonly string[] FlappyTestErrorSubstrings =
        {
            "RpcClientException",
            "WebSocketException",
            "Call took longer than",
            "invalid player"
        };

        public static AsyncTestRunner Instance { get; } = new AsyncTestRunner();

        private Task _currentRunningTestTask;
        private CancellationTokenSource _currentTestCancellationTokenSource;
        private bool _shouldPauseOnErrorInsteadOfFailing;

        private Exception _cancellationReason;

        public CancellationToken CurrentTestCancellationToken
        {
            get
            {
                if (_currentTestCancellationTokenSource == null)
                    throw new Exception("No test running");

                return _currentTestCancellationTokenSource.Token;
            }
        }

        public void ThrowIfCancellationRequested()
        {
            if (_currentTestCancellationTokenSource.IsCancellationRequested)
            {
                ExceptionDispatchInfo.Capture(_cancellationReason).Throw();
            }
        }

        public IEnumerator RunAsyncTest(Func<Task> taskFunc, float timeout)
        {
            if (timeout <= 0)
                throw new ArgumentOutOfRangeException(nameof(timeout));

            Assert.AreEqual(int.MaxValue, TestContext.CurrentTestExecutionContext.TestCaseTimeout, "Integration test timeout must have [Timeout(int.MaxValue)] attribute");
            Log.Info("=== RUNNING TEST: " + TestContext.CurrentTestExecutionContext.CurrentTest.Name);

            IEnumerator enumerator =
                RunAsyncTestInternal(async () =>
                    {
                        try
                        {
                            await GameSetUp();
                            await taskFunc();
                        }
                        catch
                        {
                            _shouldPauseOnErrorInsteadOfFailing = _cancellationReason != null && ShouldPauseOnErrorInsteadOfFailing();
                        }
                        finally
                        {
                            if (!_shouldPauseOnErrorInsteadOfFailing)
                            {
                                await GameTearDown();
                            }
                            else
                            {
#if UNITY_EDITOR
                                UnityEditor.EditorApplication.isPaused = true;
#endif
                            }
                        }
                    },
                    timeout,
                    0);

            while (enumerator.MoveNext())
            {
                yield return enumerator.Current;
            }
        }

        private async Task GameTearDown()
        {
            if (!Application.isPlaying)
                return;

            Log.Info(nameof(GameTearDown));
            await new WaitForSecondsRealtime(0.5f);
            GameClient.Get<IAppStateManager>()?.Dispose();
            await TestHelper.Instance.TearDown_Cleanup();

            await new WaitForUpdate();
            GameClient.ClearInstance();
            await new WaitForSecondsRealtime(1);

            Application.logMessageReceivedThreaded -= IgnoreAssertsLogMessageReceivedHandler;
        }

        private async Task GameSetUp()
        {
            if (!Application.isPlaying)
                return;

            Log.Info(nameof(GameSetUp));

            Application.logMessageReceivedThreaded -= IgnoreAssertsLogMessageReceivedHandler;
            Application.logMessageReceivedThreaded += IgnoreAssertsLogMessageReceivedHandler;

            await TestHelper.Instance.Dispose();
            TestHelper.DestroyInstance();
            await TestHelper.Instance.PerTestSetupInternal();
        }

        private IEnumerator RunAsyncTestInternal(Func<Task> taskFunc, float timeout, int retry)
        {
            if (_currentRunningTestTask != null)
            {
                try
                {
                    Log.Info("Previous test still running, tearing down the world");
                    yield return TestHelper.TaskAsIEnumerator(GameTearDown);

                    while (!_currentRunningTestTask.IsCompleted)
                    {
                        yield return null;
                    }

                    yield return TestHelper.TaskAsIEnumerator(GameSetUp);
                }
                finally
                {
                    FinishCurrentTest();
                }
            }

            _currentTestCancellationTokenSource = new CancellationTokenSource();
            Task currentTask = taskFunc();
            _currentRunningTestTask = currentTask;

            double lastTimestamp = Utilites.GetTimestamp();
            double timeElapsed = 0;
            bool isTimedOut = false;
            while (!currentTask.IsCompleted)
            {
                if (!isTimedOut)
                {
                    double currentTimestamp = Utilites.GetTimestamp();
                    timeElapsed += currentTimestamp - lastTimestamp;
                    lastTimestamp = currentTimestamp;

                    if (timeElapsed > timeout)
                    {
                        isTimedOut = true;
                        string message = $"Test execution time exceeded {timeout} s";
                        Log.Info(message);
                        CancelTestWithReason(new TimeoutException(message));
                    }
                }

                yield return null;
            }

            bool mustRetry = false;
            try
            {
                currentTask.Wait();
            }
            catch (Exception e)
            {
                if (e is AggregateException aggregateException)
                {
                    Assert.AreEqual(1, aggregateException.InnerExceptions.Count);
                    e = aggregateException.InnerException;
                }

                Exception flappyException = null;
                if (IsFlappyException(e))
                {
                    flappyException = e;
                } else if (_cancellationReason != null && IsFlappyException(_cancellationReason))
                {
                    flappyException = _cancellationReason;
                }

                if (flappyException != null && retry <= FlappyErrorMaxRetryCount)
                {
                    mustRetry = true;
                    Log.Warn($"Test had flappy error, retrying (retry {retry + 1} out of {FlappyErrorMaxRetryCount})");
                }
                else
                {
                    Exception rethrownException = e is OperationCanceledException ? _cancellationReason : e;
                    Log.Error("", rethrownException);
                    if (!_shouldPauseOnErrorInsteadOfFailing)
                    {
                        ExceptionDispatchInfo.Capture(rethrownException).Throw();
                    }
                }
            }
            finally
            {
                FinishCurrentTest();
            }

            if (mustRetry)
            {
                IEnumerator retryEnumerator = RunAsyncTestInternal(taskFunc, timeout, retry + 1);
                while (retryEnumerator.MoveNext())
                {
                    yield return retryEnumerator.Current;
                }
            }
        }

        private void FinishCurrentTest()
        {
            _currentRunningTestTask = null;
            _currentTestCancellationTokenSource = null;
            _cancellationReason = null;
            _shouldPauseOnErrorInsteadOfFailing = false;
        }

        private void CancelTestWithReason(Exception reason)
        {
            if (_cancellationReason != null)
                return;

            _cancellationReason = reason;
            _currentTestCancellationTokenSource.Cancel();
            Log.Warn("=== CANCELING TEST WITH REASON: " + reason);
        }

        private void IgnoreAssertsLogMessageReceivedHandler(string condition, string stacktrace, LogType type)
        {
            switch (type)
            {
                case LogType.Error:
                case LogType.Exception:
                    if (IsKnownError(condition))
                        break;

                    CancelTestWithReason(new Exception(condition + "\r\n" + stacktrace));
                    break;
                case LogType.Assert:
                case LogType.Warning:
                case LogType.Log:
                    break;
            }
        }

        private static bool IsFlappyException(Exception e)
        {
            string exceptionString = e.ToString();
            return FlappyTestErrorSubstrings.Any(s => exceptionString.Contains(s));
        }

        private static bool IsKnownError(string condition)
        {
            return KnownErrors.Any(knownError => condition.IndexOf(knownError, StringComparison.InvariantCultureIgnoreCase) != -1);
        }

        private static bool ShouldPauseOnErrorInsteadOfFailing()
        {
#if UNITY_EDITOR
            if (Application.isBatchMode)
                return false;

            PropertyInfo consoleFlagsProperty =
                typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.LogEntries").GetProperty("consoleFlags", BindingFlags.Public | BindingFlags.Static);
            int consoleFlagValue = (int) consoleFlagsProperty.GetValue(null, null);
            const int errorPauseFlag = 4;
            return (consoleFlagValue & errorPauseFlag) != 0;
#else
            return false;
#endif
        }
    }
}
