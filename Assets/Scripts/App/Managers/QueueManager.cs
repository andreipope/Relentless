using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Protobuf;
using System;
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

        private volatile Queue<PlayerActionRequest> _networkThreadActions;

        private Thread _networkThread;

        private bool _networkThreadAlive;

        public void Init()
        {
            _mainThreadActions = new Queue<Action>();
            _networkThreadActions = new Queue<PlayerActionRequest>();
        }

        public void StartNetworkThread()
        {
            _networkThreadAlive = true;
            _networkThread = new Thread(NetworkThread);
            _networkThread.Start();
        }

        public void StopNetworkThread()
        {
            _networkThreadAlive = false;
            _networkThread.Abort();
            _networkThread = null;
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
            UnityEngine.Debug.Log("NEW Action _ " + action.PlayerAction);
            UnityEngine.Debug.Log("STATUS _ " + _networkThread.ThreadState);
            _networkThreadActions.Enqueue(action);
            UnityEngine.Debug.Log("NEW COUNT _ " + _networkThreadActions.Count);
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
                Thread.Sleep(5000);
                while (_networkThreadActions.Count > 0)
                {
                    UnityEngine.Debug.Log("Start Action _ " + _networkThreadActions.Count);
                    await GameClient.Get<BackendFacade>().SendAction(_networkThreadActions.Dequeue());
                    UnityEngine.Debug.Log("End Action _ " + _networkThreadActions.Count);
                }
              //  Thread.Sleep(1000);
            }
            //_networkThread.Abort();
        }

        public void Dispose()
        {
            _networkThread.Interrupt();
            _mainThreadActions.Clear();
            _networkThreadActions.Clear();
        }
    }
}
