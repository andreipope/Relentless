using System;
using System.Collections;
using System.Collections.Generic;
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
    public class IntegrationTestRunner
    {
        private const string LogTag = "[" + nameof(IntegrationTestRunner) + "] ";

        public static IntegrationTestRunner Instance { get; } = new IntegrationTestRunner();

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

        public IEnumerator AsyncTest(Func<Task> taskFunc, int timeout = Timeout.Infinite)
        {
            return TaskAsIEnumeratorInternal(async () =>
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
            Debug.Log(LogTag + nameof(GameTearDown));


            await Task.Delay(1000);
            await TestHelper.Instance.TearDown_Cleanup();

            await new WaitForUpdate();
            GameClient.ClearInstance();
            await new WaitForUpdate();

            Application.logMessageReceivedThreaded -= IgnoreAssertsLogMessageReceivedHandler;
        }

        private async Task GameSetUp()
        {
            Debug.Log(LogTag + nameof(GameSetUp));

            Application.logMessageReceivedThreaded -= IgnoreAssertsLogMessageReceivedHandler;
            Application.logMessageReceivedThreaded += IgnoreAssertsLogMessageReceivedHandler;

            TestHelper.DestroyInstance();
            await TestHelper.Instance.PerTestSetupInternal();
        }

        private IEnumerator TaskAsIEnumeratorInternal(Func<Task> taskFunc, int timeout = Timeout.Infinite)
        {
            int testTimeout = (int) TestContext.CurrentContext.Test.Properties["Timeout"][0];
            Assert.AreEqual(int.MaxValue, testTimeout, "Integration test timeout must be int.MaxValue");

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
                    _currentRunningTestTask = null;
                    _currentTestCancellationTokenSource = null;
                }
            }

            _currentTestCancellationTokenSource = new CancellationTokenSource();
            Task currentTask = taskFunc();
            _currentRunningTestTask = currentTask;

            double lastTimestamp = Utilites.GetTimestamp();
            double timeElapsed = 0;
            double timeoutSeconds = timeout / 1000.0;
            while (!currentTask.IsCompleted)
            {
                if (timeout != Timeout.Infinite)
                {
                    double currentTimestamp = Utilites.GetTimestamp();
                    timeElapsed += currentTimestamp - lastTimestamp;
                    lastTimestamp = currentTimestamp;

                    if (timeElapsed > timeoutSeconds)
                    {
                        CancelTestWithReason(new TimeoutException($"Test execution time exceeded {timeout} ms"));
                    }
                }

                yield return null;
            }

            try
            {
                currentTask.Wait();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                throw;
            }
            finally
            {
                _currentRunningTestTask = null;
                _currentTestCancellationTokenSource = null;
            }
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
