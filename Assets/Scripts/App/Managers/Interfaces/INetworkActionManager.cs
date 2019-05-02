using System;
using System.Threading.Tasks;
using Loom.Google.Protobuf;

namespace Loom.ZombieBattleground
{
    public interface INetworkActionManager
    {
        bool Active { get; set; }

        int QueuedTaskCount { get; }

        void EnqueueMessage(IMessage request);

        void Clear();
    }
}
