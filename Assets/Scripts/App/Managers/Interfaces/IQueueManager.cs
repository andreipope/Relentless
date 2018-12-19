using System;
using System.Threading.Tasks;
using Loom.Google.Protobuf;

namespace Loom.ZombieBattleground
{
    public interface IQueueManager
    {
        bool Active { get; set; }
        void AddTask(Func<Task> taskFunc);
        void AddAction(IMessage request);
        void Clear();
    }
}
