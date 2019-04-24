using System;
using System.Threading.Tasks;
using Loom.Google.Protobuf;

namespace Loom.ZombieBattleground
{
    public interface INetworkActionManager
    {
        bool Active { get; set; }

        int QueuedTaskCount { get; }

        Task EnqueueMessage(IMessage request);

        Task EnqueueNetworkTask(
            Func<Task> taskFunc,
            Func<Exception, Task> onUnknownExceptionCallbackFunc = null,
            Func<Exception, Task> onNetworkExceptionCallbackFunc = null,
            bool leaveCurrentAppState = false,
            bool drawErrorMessage = true
        );

        void Clear();
    }
}
