using System;
using System.Threading.Tasks;
using Loom.Google.Protobuf;

namespace Loom.ZombieBattleground
{
    public interface INetworkActionManager
    {
        int QueuedTaskCount { get; }

        /// <summary>
        /// Puts a task to send a message to the backend into the queue.
        /// This method will still throw in case an exception happens to give a chance for implementing special handling.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task EnqueueMessage(IMessage request);

        /// <summary>
        /// Puts a network-related task into the queue, handling network-related exceptions. Queued actions will be executed in order.
        /// This method will still throw in case an exception happens to give a chance for implementing special handling.
        /// </summary>
        /// <param name="taskFunc">Task to execute</param>
        /// <param name="onUnknownExceptionCallbackFunc">Invoked when a non network-related exception is detected.</param>
        /// <param name="onNetworkExceptionCallbackFunc">Invoked when a network-related exception is detected.</param>
        /// <param name="keepCurrentAppState"></param>
        /// <param name="drawErrorMessage"></param>
        /// /// <param name="ignoreConnectionState"></param>
        /// <returns></returns>
        Task EnqueueNetworkTask(
            Func<Task> taskFunc,
            Func<Exception, Task> onUnknownExceptionCallbackFunc = null,
            Func<Exception, Task> onNetworkExceptionCallbackFunc = null,
            bool keepCurrentAppState = false,
            bool drawErrorMessage = true,
            bool ignoreConnectionState = false
        );

        /// <summary>
        /// Execute a network-related task immediately without putting it into a queue, handling network-related exceptions.
        /// This method will still throw in case an exception happens to give a chance for implementing special handling.
        /// </summary>
        /// <param name="taskFunc">Task to execute</param>
        /// <param name="onUnknownExceptionCallbackFunc">Invoked when a non network-related exception is detected.</param>
        /// <param name="onNetworkExceptionCallbackFunc">Invoked when a network-related exception is detected.</param>
        /// <param name="keepCurrentAppState"></param>
        /// <param name="drawErrorMessage"></param>
        /// <param name="ignoreConnectionState"></param>
        /// <returns></returns>
        Task ExecuteNetworkTask(
            Func<Task> taskFunc,
            Func<Exception, Task> onUnknownExceptionCallbackFunc = null,
            Func<Exception, Task> onNetworkExceptionCallbackFunc = null,
            bool keepCurrentAppState = false,
            bool drawErrorMessage = true,
            bool ignoreConnectionState = false
        );

        void Clear();
    }
}
