using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Test;
using UnityEngine;
using Stopwatch = System.Diagnostics.Stopwatch;

namespace Loom.ZombieBattleground.Test
{
    public class IntegrationTestRunner
    {
        public static IntegrationTestRunner Instance { get; } = new IntegrationTestRunner();

        private Task _currentRunningTestTask;
        private CancellationTokenSource _currentTestCancellationTokenSource;

        private readonly List<LogMessage> _errorMessages = new List<LogMessage>();

        public CancellationToken CurrentTestCancellationToken
        {
            get
            {
                if (_currentTestCancellationTokenSource == null)
                    throw new Exception("No test running");

                return _currentTestCancellationTokenSource.Token;
            }
        }

        public IEnumerator AsyncTest(Func<Task> taskFunc, int timeout = Timeout.Infinite)
        {
            return TaskAsIEnumeratorInternal(async () =>
                {
                    TestHelper.DestroyInstance();

                    //TestHelper.Instance.DebugCheats.CopyFrom(new DebugCheatsConfiguration());
                    await TestHelper.Instance.PerTestSetupInternal();
                    try
                    {
                        await taskFunc();
                    }
                    finally
                    {
                        await GameTearDown();
                    }

                    //TestHelper.Instance.DebugCheats.CopyFrom(new DebugCheatsConfiguration());
                },
                timeout);
        }

        private async Task GameTearDown()
        {
            await TestHelper.Instance.TearDown_Cleanup();

            await new WaitForUpdate();
            GameClient.ClearInstance();
            await new WaitForUpdate();

            Application.logMessageReceivedThreaded -= IgnoreAssertsLogMessageReceivedHandler;
        }

        private async Task GameSetUp()
        {
            Application.logMessageReceivedThreaded -= IgnoreAssertsLogMessageReceivedHandler;
            Application.logMessageReceivedThreaded += IgnoreAssertsLogMessageReceivedHandler;

            await TestHelper.Instance.PerTestSetupInternal();
        }

        private IEnumerator TaskAsIEnumeratorInternal(Func<Task> taskFunc, int timeout = Timeout.Infinite)
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
                    _currentRunningTestTask = null;
                    _currentTestCancellationTokenSource = null;
                }
            }

            _currentTestCancellationTokenSource = new CancellationTokenSource();
            Task currentTask = taskFunc();
            _currentRunningTestTask = currentTask;

            Stopwatch timeoutStopwatch = timeout != Timeout.Infinite ? Stopwatch.StartNew() : null;
            while (!currentTask.IsCompleted)
            {
                if (timeoutStopwatch != null && timeoutStopwatch.ElapsedMilliseconds > timeout)
                {
                    _currentTestCancellationTokenSource.Cancel();
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

        private void IgnoreAssertsLogMessageReceivedHandler(string condition, string stacktrace, LogType type)
        {
            switch (type)
            {
                case LogType.Error:
                case LogType.Exception:
                    _currentTestCancellationTokenSource?.Cancel();
                    //_errorMessages.Add(new LogMessage(condition, stacktrace, type));
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
