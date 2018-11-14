using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Protobuf;
using Loom.Google.Protobuf;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class QueueManager : IService, IQueueManager
    {
        private Queue<Func<Task>> _tasks;
        private BackendFacade _backendFacade;

        public bool Active { get; set; }

        public void Init()
        {
            _backendFacade = GameClient.Get<BackendFacade>();
            _tasks = new Queue<Func<Task>>();
        }

        public void Clear()
        {
            _tasks.Clear();
        }

        public async void Update()
        {
            if (!Active)
                return;

            while (_tasks.Count > 0)
            {
                await _tasks.Dequeue()();
            }
        }

        public void AddTask(Func<Task> taskFunc)
        {
            _tasks.Enqueue(taskFunc);
        }

        public void AddAction(IMessage action)
        {
            AddTask(async () => await _backendFacade.SendAction(action));
        }

        public void Dispose()
        {
            Clear();
        }
    }
}
