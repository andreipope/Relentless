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

namespace Loom.ZombieBattleground
{
    public class QueueManager : IService, IQueueManager
    {
        private Queue<Action> _mainThreadActions;

        private BlockingCollection<IMessage> _networkThreadActions;

        private Thread _networkThread;

        private bool _networkThreadAlive;

        public void Init()
        {
            _mainThreadActions = new Queue<Action>();
            _networkThreadActions = new BlockingCollection<IMessage>();
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

        public void Clear()
        {
            _mainThreadActions.Clear();
            while (_networkThreadActions.Count > 0)
            {
                _networkThreadActions.Take();
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

        public void AddAction(IMessage action)
        {
            _networkThreadActions.Add(action);
        }

        public bool Active { get; set; }

        private void MainThread()
        {
            if (Active && _mainThreadActions.Count > 0)
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
                    IMessage request = _networkThreadActions.Take();
                    await GameClient.Get<BackendFacade>().SendAction(request);
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
