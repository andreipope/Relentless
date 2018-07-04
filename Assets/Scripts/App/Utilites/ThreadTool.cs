using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace LoomNetwork.CZB
{
    public class ThreadTool : MonoBehaviour
    {
        private static ThreadTool _Instance;
        public static ThreadTool Instance
        {
            get
            {
                if(_Instance == null)
                    _Instance = new GameObject("ThreadTool").AddComponent<ThreadTool>();

                return _Instance;
            }
        }

        private List<OneTimeCustomThread> _threads;
        private ulong _freeId = 0;
        private volatile Queue<Action> _actionsMainThread;

        private void Awake()
        {
            _threads = new List<OneTimeCustomThread>();
            _actionsMainThread = new Queue<Action>();
        }

        private void Update()
        {
            while (_actionsMainThread.Count > 0)
            {
                var action = _actionsMainThread.Dequeue();
                if (action != null)
                    action.Invoke();
            }
        }

        private void OnDestroy()
        {
            foreach (var item in _threads)
                item.Dispose();
        }


        public ulong StartOneTimeThread(Action action, Action onComplete, object root = null)
        {
            ulong id = _freeId++;

            OneTimeCustomThread thread = new OneTimeCustomThread();
            thread.id = id;
            thread.action = action;
            thread.onComplete = onComplete;
            thread.ThreadEndEvent += ThreadEndEventHandler;
            thread.root = root;
            thread.Start();

            _threads.Add(thread);

            return id;
        }

        public void AbortThread(ulong id)
        {
            var thread = _threads.Find(x => x.id == id);
            if (thread != null)
                _threads.Remove(thread);
        }

        public void AbortAllThreads(object root = null)
        {
            if (root == null)
            {
                foreach (var thread in _threads)
                    thread.Dispose();
                _threads.Clear();
            }
            else
            {
                var foundThreads = _threads.FindAll(x => x.root == root);
                foreach (var thread in foundThreads)
                {
                    thread.Dispose();
                    _threads.Remove(thread);
                }
                foundThreads.Clear();
            }
        }


        private void ThreadEndEventHandler(ulong id)
        {
            AbortThread(id);
        }

        public void RunInMainThread(Action action)
        {
            _actionsMainThread.Enqueue(action);
        }


        public class OneTimeCustomThread
        {
            public event Action<ulong> ThreadEndEvent;

            public Thread thread;
            public ulong id;
            public Action action;
            public Action onComplete;
            public object root;

            public void Start()
            {
                Action newAction = () =>
                {
                    if(action != null)
                        action();

                    if (ThreadEndEvent != null)
                        ThreadEndEvent(id);

                    if(onComplete != null)
                        ThreadTool.Instance.RunInMainThread(onComplete);

                    Dispose();
                };

                thread = new Thread(new ThreadStart(newAction));
                thread.Start();
            }

            public void Dispose()
            {
                try
                {
#if UNITY_IOS
                    thread.Interrupt();
#else
                    thread.Abort();
#endif
                }
                catch { }
            }

        }

    }
}
