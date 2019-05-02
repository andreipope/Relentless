using System;
using System.Collections.Generic;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class TimerManager : IService, ITimerManager
    {
        private static readonly object Sync = new object();

        private int _timersCount;

        private List<Timer> _timers = new List<Timer>();

        public void Dispose()
        {
            _timers.Clear();
        }

        public void Init()
        {
        }

        public void Update()
        {
            lock (Sync)
            {
                for (int i = _timers.Count - 1; i > -1; i--)
                {
                    if (_timers.Count <= 0)
                    {
                        break;
                    }

                    if (i < _timers.Count && i > -1)
                    {
                        if (_timers[i] != null)
                        {
                            if (_timers[i].Finished)
                            {
                                _timers.RemoveAt(i);
                            }
                            else
                            {
                                _timers[i].Update();
                            }
                        }
                    }
                }
            }
        }

        public void StopTimer(Action<object[]> handler)
        {
            for (int i = 0; i < _timers.Count; i++)
            {
                if (_timers[i].Handler == handler)
                {
                    _timers.RemoveAt(i);
                }
            }
        }

        public void AddTimer(
            Action<object[]> handler, object[] parameters = null, float time = 1, bool loop = false,
            bool storeTimer = false)
        {
            Timer timer = new Timer(handler, parameters, time, loop);
            timer.Index = _timersCount++;

            if (storeTimer)
            {
                if (parameters != null)
                {
                    object[] newParams = new object[parameters.Length + 1];

                    for (int i = 0; i < parameters.Length; i++)
                    {
                        newParams[i] = parameters[i];
                    }

                    newParams[newParams.Length - 1] = timer.Index;

                    parameters = newParams;
                }
                else
                {
                    parameters = new object[1];
                    parameters[0] = timer.Index;
                }

                timer.Parameters = parameters;
            }

            _timers.Add(timer);
        }

        public void StopTimer(int index)
        {
            Timer timer = _timers.Find(x => x.Index == index);

            if (_timers.Contains(timer))
            {
                _timers.Remove(timer);
            }
        }
    }

    public class Timer
    {
        public Action<object[]> Handler;

        public object[] Parameters;

        public bool Finished;

        public int Index;

        private readonly float _time;

        private readonly bool _loop;

        private float _currentTime;

        public Timer(Action<object[]> handler, object[] parameters, float time, bool loop)
        {
            Handler = handler;
            Parameters = parameters;
            _time = time;
            _currentTime = time;
            _loop = loop;
            Finished = false;
        }

        public void Update()
        {
            _currentTime -= Time.deltaTime;
            if (_currentTime < 0)
            {
                try
                {
                    Handler(Parameters);
                } catch (Exception)
                {
                    Finished = true;
                    throw;
                }

                if (_loop)
                {
                    _currentTime = _time;
                }
                else
                {
                    Finished = true;
                }
            }
        }
    }
}
