using System;
using UnityEngine;

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

    public string GetTimeDiffrence()
    {
        var time = String.Format("{0}:{1}:{2}", _timeDiff.Hours,_timeDiff.Minutes,_timeDiff.Seconds);
        return time;
    }
}
