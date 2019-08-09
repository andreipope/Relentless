using System;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class AnalyticsTimer
    {
        private DateTime _startTime;
        private TimeSpan _timeDiff;

        public void StartTimer()
        {
            _startTime = DateTime.Now;

            //Debug.LogError("Start Time = " + _startTime);
        }

        public void FinishTimer()
        {
            DateTime currentTime = DateTime.Now;
            _timeDiff = currentTime - _startTime;
        }

        public string GetTimeDifference()
        {
            return $"{_timeDiff.Hours}:{_timeDiff.Minutes}:{_timeDiff.Seconds}";
        }
    }
}
