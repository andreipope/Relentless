using System;
using System.Collections;
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
                        await TestHelper.Instance.TearDown_Cleanup();
                        GameClient.Instance.Dispose();
                        GameClient.ClearInstance();

                        await new WaitForUpdate();
                    }

                    //TestHelper.Instance.DebugCheats.CopyFrom(new DebugCheatsConfiguration());
                },
                timeout);
        }

        private IEnumerator TaskAsIEnumeratorInternal(Func<Task> taskFunc, int timeout = Timeout.Infinite)
        {
            /*if (_currentRunningTestTask != null)
            {
                try
                {
                    Debug.Log("Previous test still running, waiting for it to end... " + random);
                    _currentRunningTestTask.Wait();
                }
                finally
                {
                    _currentRunningTestTask = null;
                    _currentTestCancellationTokenSource = null;
                }
            }*/

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
    }
}
