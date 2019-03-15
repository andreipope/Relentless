using System;
using System.Threading;
using System.Threading.Tasks;
using Loom.Client;

namespace Loom.ZombieBattleground.BackendCommunication
{
    /// <summary>
    /// A variant of <see cref="DefaultDAppChainClientCallExecutor"/> with events for before and after each call attempt is made.
    /// </summary>
    public class NotifyingDAppChainClientCallExecutor : DefaultDAppChainClientCallExecutor
    {
        public delegate void CallStartingHandler(int callNumber, CallDescription callDescription);

        public delegate void CallFinishedHandler(int callNumber, CallDescription callDescription, Exception exception);

        public event CallStartingHandler CallStarting;

        public event CallFinishedHandler CallFinished;

        private int _callCounter;

        public NotifyingDAppChainClientCallExecutor(DAppChainClientConfiguration configuration)
            : base(configuration)
        {
        }

        public override Task<T> Call<T>(Func<Task<T>> taskProducer, CallDescription callDescription)
        {
            Func<Task<T>> originalProducer = taskProducer;
            taskProducer = () => SendEventWrapper(callDescription, originalProducer);
            return base.Call(taskProducer, callDescription);
        }

        public override Task Call(Func<Task> taskProducer, CallDescription callDescription)
        {
            Func<Task> originalProducer = taskProducer;
            taskProducer = () => SendEventWrapper(callDescription, originalProducer);
            return base.Call(taskProducer, callDescription);
        }

        public override Task<T> StaticCall<T>(Func<Task<T>> taskProducer, CallDescription callDescription)
        {
            Func<Task<T>> originalProducer = taskProducer;
            taskProducer = () => SendEventWrapper(callDescription, originalProducer);
            return base.StaticCall(taskProducer, callDescription);
        }

        public override Task StaticCall(Func<Task> taskProducer, CallDescription callDescription)
        {
            Func<Task> originalProducer = taskProducer;
            taskProducer = () => SendEventWrapper(callDescription, originalProducer);
            return base.StaticCall(taskProducer, callDescription);
        }

        private async Task SendEventWrapper(CallDescription callDescription, Func<Task> taskProducer)
        {
            int callNumber = Interlocked.Increment(ref _callCounter);
            try
            {
                CallStarting?.Invoke(callNumber, callDescription);
                await taskProducer();
                CallFinished?.Invoke(callNumber, callDescription, null);
            }
            catch (Exception e)
            {
                CallFinished?.Invoke(callNumber, callDescription, e);
                throw;
            }
        }

        private async Task<TResult> SendEventWrapper<TResult>(CallDescription callDescription, Func<Task<TResult>> taskProducer)
        {
            int callNumber = Interlocked.Increment(ref _callCounter);
            try
            {
                CallStarting?.Invoke(callNumber, callDescription);
                TResult result = await taskProducer();
                CallFinished?.Invoke(callNumber, callDescription, null);
                return result;
            }
            catch (Exception e)
            {
                CallFinished?.Invoke(callNumber, callDescription, e);
                throw;
            }
        }
    }
}
