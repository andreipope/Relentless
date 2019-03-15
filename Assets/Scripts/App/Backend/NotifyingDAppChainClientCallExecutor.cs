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
        public delegate void CallStartingHandler(int callNumber, CallContext callContext);

        public delegate void CallFinishedHandler(int callNumber, CallContext callContext, Exception exception);

        public event CallStartingHandler CallStarting;

        public event CallFinishedHandler CallFinished;

        private int _callCounter;

        public NotifyingDAppChainClientCallExecutor(DAppChainClientConfiguration configuration)
            : base(configuration)
        {
        }

        public override Task<T> Call<T>(Func<Task<T>> taskProducer, CallContext callContext)
        {
            Func<Task<T>> originalProducer = taskProducer;
            taskProducer = () => SendEventWrapper(callContext, originalProducer);
            return base.Call(taskProducer, callContext);
        }

        public override Task Call(Func<Task> taskProducer, CallContext callContext)
        {
            Func<Task> originalProducer = taskProducer;
            taskProducer = () => SendEventWrapper(callContext, originalProducer);
            return base.Call(taskProducer, callContext);
        }

        public override Task<T> StaticCall<T>(Func<Task<T>> taskProducer, CallContext callContext)
        {
            Func<Task<T>> originalProducer = taskProducer;
            taskProducer = () => SendEventWrapper(callContext, originalProducer);
            return base.StaticCall(taskProducer, callContext);
        }

        public override Task StaticCall(Func<Task> taskProducer, CallContext callContext)
        {
            Func<Task> originalProducer = taskProducer;
            taskProducer = () => SendEventWrapper(callContext, originalProducer);
            return base.StaticCall(taskProducer, callContext);
        }

        private async Task SendEventWrapper(CallContext callContext, Func<Task> taskProducer)
        {
            int callNumber = Interlocked.Increment(ref _callCounter);
            try
            {
                CallStarting?.Invoke(callNumber, callContext);
                await taskProducer();
                CallFinished?.Invoke(callNumber, callContext, null);
            }
            catch (Exception e)
            {
                CallFinished?.Invoke(callNumber, callContext, e);
                throw;
            }
        }

        private async Task<TResult> SendEventWrapper<TResult>(CallContext callContext, Func<Task<TResult>> taskProducer)
        {
            int callNumber = Interlocked.Increment(ref _callCounter);
            try
            {
                CallStarting?.Invoke(callNumber, callContext);
                TResult result = await taskProducer();
                CallFinished?.Invoke(callNumber, callContext, null);
                return result;
            }
            catch (Exception e)
            {
                CallFinished?.Invoke(callNumber, callContext, e);
                throw;
            }
        }
    }
}
