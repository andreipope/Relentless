using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Test;
using NUnit.Framework;
using UnityEngine;
using Stopwatch = System.Diagnostics.Stopwatch;

namespace Loom.ZombieBattleground.Test
{
    public class AsyncTestRunner
    {
        private const string LogTag = "[" + nameof(AsyncTestRunner) + "] ";

        public static AsyncTestRunner Instance { get; } = new AsyncTestRunner();

        private Task _currentRunningTestTask;
        private CancellationTokenSource _currentTestCancellationTokenSource;

        private readonly List<LogMessage> _errorMessages = new List<LogMessage>();
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

            Assert.AreEqual(int.MaxValue, TestContext.CurrentTestExecutionContext.TestCaseTimeout, "Integration test timeout must be int.MaxValue");
            Debug.Log("=== RUNNING TEST: " + TestContext.CurrentTestExecutionContext.CurrentTest.Name);

            return RunAsyncTestInternal(async () =>
                {
                    await GameSetUp();
                    try
                    {
                        await taskFunc();
                    }
                    finally
                    {
                        await GameTearDown();
                    }
                },
                timeout);
        }

        private async Task GameTearDown()
        {
            if (!Application.isPlaying)
                return;

            Debug.Log(LogTag + nameof(GameTearDown));
            await Task.Delay(500);
            await TestHelper.Instance.TearDown_Cleanup();

            await new WaitForUpdate();
            GameClient.ClearInstance();
            await new WaitForUpdate();

            Application.logMessageReceivedThreaded -= IgnoreAssertsLogMessageReceivedHandler;
        }

        private async Task GameSetUp()
        {
            if (!Application.isPlaying)
                return;

            Debug.Log(LogTag + nameof(GameSetUp));

            Application.logMessageReceivedThreaded -= IgnoreAssertsLogMessageReceivedHandler;
            Application.logMessageReceivedThreaded += IgnoreAssertsLogMessageReceivedHandler;

            TestHelper.DestroyInstance();
            await TestHelper.Instance.PerTestSetupInternal();
        }

        private IEnumerator RunAsyncTestInternal(Func<Task> taskFunc, float timeout)
        {
            if (_currentRunningTestTask != null)
            {
                try
                {
                    Debug.Log("Previous test still running, tearing down the world");
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
            while (!currentTask.IsCompleted)
            {
                double currentTimestamp = Utilites.GetTimestamp();
                timeElapsed += currentTimestamp - lastTimestamp;
                lastTimestamp = currentTimestamp;

                if (timeElapsed > timeout)
                {
                    CancelTestWithReason(new TimeoutException($"Test execution time exceeded {timeout} s"));
                }

                yield return null;
            }

            try
            {
                currentTask.Wait();
            }
            catch (Exception e)
            {
                if (e is OperationCanceledException || e is TaskCanceledException)
                {
                    Debug.LogException(_cancellationReason);
                    ExceptionDispatchInfo.Capture(_cancellationReason).Throw();
                }
                else
                {
                    Debug.LogException(e);
                    throw;
                }
            }
            finally
            {
                FinishCurrentTest();
            }
        }

        private void FinishCurrentTest()
        {
            _currentRunningTestTask = null;
            _currentTestCancellationTokenSource = null;
            _cancellationReason = null;
        }

        private void CancelTestWithReason(Exception reason)
        {
            _cancellationReason = reason;
            _currentTestCancellationTokenSource.Cancel();
        }

        private void IgnoreAssertsLogMessageReceivedHandler(string condition, string stacktrace, LogType type)
        {
            switch (type)
            {
                case LogType.Error:
                case LogType.Exception:
                    _errorMessages.Add(new LogMessage(condition, stacktrace, type));
                    string[] knownErrors = new []{
                        "Sub-emitters must be children of the system that spawns them"
                    }.Select(s => s.ToLowerInvariant()).ToArray();

                    if (knownErrors.Any(error => condition.ToLowerInvariant().Contains(error)))
                        break;

                    CancelTestWithReason(new Exception(condition + "\r\n" + stacktrace));
                    break;
                case LogType.Assert:
                case LogType.Warning:
                case LogType.Log:
                    break;
            }
        }

        private struct LogMessage
        {
            public string Message { get; }

            public string StackTrace { get; }

            public LogType LogType { get; }

            public LogMessage(string message, string stackTrace, LogType logType)
            {
                Message = message;
                StackTrace = stackTrace;
                LogType = logType;
            }

            public override string ToString()
            {
                return $"[{LogType}] {Message}";
            }
        }
    }
}
