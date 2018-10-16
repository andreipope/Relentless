using Loom.ZombieBattleground.Protobuf;
using System;
using System.Threading.Tasks;

namespace Loom.ZombieBattleground
{
    public interface IQueueManager
    {
        void AddAction(Func<Task> action);
        void AddAction(PlayerActionRequest action);
        void StartNetworkThread();
        void StopNetworkThread();
    }
}
