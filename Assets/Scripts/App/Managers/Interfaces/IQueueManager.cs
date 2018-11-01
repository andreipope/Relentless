using Loom.ZombieBattleground.Protobuf;
using System;
using UnityEngine;
using static Loom.ZombieBattleground.QueueManager;
using Loom.Google.Protobuf;


namespace Loom.ZombieBattleground
{
    public interface IQueueManager
    {
        void AddAction(Action action);
        void AddAction(IMessage action);
        bool Active { get; set; }
        void StartNetworkThread();
        void StopNetworkThread();
        void Clear();
    }
}
