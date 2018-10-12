using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Protobuf;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Loom.ZombieBattleground
{
    public class QueueManager : IService, IQueueManager
    {

        private volatile Queue<Action> _mainThreadActions;

        private BlockingCollection<PlayerActionRequest> _networkThreadActions;

        private Thread _networkThread;

        private bool _networkThreadAlive;

        public void Init()
        {
            _mainThreadActions = new Queue<Action>();
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
        public void Update()
        {
            MainThread();
        }

        public void AddAction(Action action)
        {
            _mainThreadActions.Enqueue(action);
        }

        public void AddAction(PlayerActionRequest action)
        {
            _networkThreadActions.Add(action);
        }

        private void MainThread()
        {
            if (_mainThreadActions.Count > 0)
            {
                _mainThreadActions.Dequeue().Invoke();
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
