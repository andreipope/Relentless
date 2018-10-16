using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Protobuf;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Loom.ZombieBattleground
{
    public class QueueManager : IService, IQueueManager
    {

        private volatile Queue<Func<Task>> _mainThreadActions;

        private BlockingCollection<PlayerActionRequest> _networkThreadActions;

        private Thread _networkThread;

        private bool _networkThreadAlive;

        public void Init()
        {
            _mainThreadActions = new Queue<Func<Task>>();
            _networkThreadActions = new BlockingCollection<PlayerActionRequest>();
        }

        public void StartNetworkThread()
        {
            _networkThreadAlive = true;
            _networkThread = new Thread(NetworkThread);
            _networkThread.Start();
        }

        public void StopNetworkThread()
        {
            if (_networkThread != null)
            {
                _networkThreadAlive = false;
                _networkThread.Abort();
                _networkThread = null;
            }
        }

        //Main Gameplay Thread
        public async void Update()
        {
            await MainThread();
        }

        public void AddAction(Func<Task> action)
        {
            _mainThreadActions.Enqueue(action);
        }

        public void AddAction(PlayerActionRequest action)
        {
            _networkThreadActions.Add(action);
        }

        private async Task MainThread()
        {
            if (_mainThreadActions.Count > 0)
            {
                await _mainThreadActions.Dequeue().Invoke();
            }
        }

        private async void NetworkThread()
        {
            while (_networkThreadAlive)
            {
                while (_networkThreadActions.Count > 0)
                {
                    await GameClient.Get<BackendFacade>().SendAction(_networkThreadActions.Take());
                }
            }
        }

        public void Dispose()
        {
            if(_networkThread != null)
            {
                _networkThreadAlive = false;
                _networkThread.Interrupt();
                _networkThreadActions.Dispose();
            }
            _mainThreadActions.Clear();
        }
    }
}
