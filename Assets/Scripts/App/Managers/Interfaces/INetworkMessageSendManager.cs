using System;
using System.Threading.Tasks;
using Loom.Google.Protobuf;

namespace Loom.ZombieBattleground
{
    public interface INetworkMessageSendManager
    {
        bool Active { get; set; }
        void EnqueueMessage(IMessage request);
        void Clear();
    }
}
