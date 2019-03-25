using System;
using System.Threading.Tasks;
using Loom.Client;

namespace Loom.ZombieBattleground.BackendCommunication
{
    /// <summary>
    /// No-op call executor for testing purposes.
    /// </summary>
    public class DumbDAppChainClientCallExecutor : DefaultDAppChainClientCallExecutor
    {
        public DumbDAppChainClientCallExecutor(DAppChainClientConfiguration configuration)
            : base(configuration)
        {
        }

        protected override async Task<Task> ExecuteTaskWithTimeout(Func<Task> taskProducer, int timeoutMs)
        {
            Task task = taskProducer();
            await task;
            return task;
        }

        protected override async Task<Task> ExecuteTaskWaitForOtherTasks(Func<Task<Task>> taskProducer)
        {
            Task<Task> task = taskProducer();
            await task;
            return await task;
        }

        protected override async Task<Task> ExecuteTaskWithRetryOnInvalidTxNonceException(Func<Task<Task>> taskTaskProducer)
        {
            Task<Task> task = taskTaskProducer();
            await task;
            return await task;
        }
    }
}
