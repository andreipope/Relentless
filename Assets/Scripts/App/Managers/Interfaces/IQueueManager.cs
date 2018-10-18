using Loom.ZombieBattleground.Protobuf;
using System;
using UnityEngine;
using static Loom.ZombieBattleground.QueueManager;

namespace Loom.ZombieBattleground
{
    public interface IQueueManager
    {
        void AddAction(Action action);
        void AddAction(PlayerActionRequest action);
        void StartNetworkThread();
        void StopNetworkThread();
    }
}
