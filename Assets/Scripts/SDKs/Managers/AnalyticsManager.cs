using System;
using System.Collections.Generic;
using LoomNetwork.CZB;
using LoomNetwork.CZB.Common;
using UnityEngine;
using UnityEngine.Analytics;
using Object = UnityEngine.Object;

public class AnalyticsManager : IAnalyticsManager, IService
{
    private const string MatchesInPreviousSittingKey = "Analytics_MatchesPerSitting";
    private GoogleAnalyticsV4 _googleAnalytics;
    private int _startedMatchCounter;
    private int _finishedMatchCounter;
    
    public void StartSession()
    {
        AnalyticsEvent.GameStart();
        _googleAnalytics.StartSession();
        
        int matchesInPreviousSittingKey = PlayerPrefs.GetInt(MatchesInPreviousSittingKey, -1);
        if (matchesInPreviousSittingKey != -1)
        {
            PlayerPrefs.DeleteKey(MatchesInPreviousSittingKey);
            Debug.Log("Sending previousMatchesPerSitting = " + matchesInPreviousSittingKey);
            LogEvent("MatchesInPreviousSitting", "", matchesInPreviousSittingKey);
        }
    }

    public void LogScreen(string title)
    {
        Debug.Log("=== Log screen = " + title);
        _googleAnalytics.LogScreen(title);
        AnalyticsEvent.ScreenVisit(title);
    }

    public void LogEvent(string eventAction, string eventLabel, long value)
    {
        Debug.Log("=== Log Event = " + eventAction);
        _googleAnalytics.LogEvent("Game Event", eventAction, eventLabel, value);
        AnalyticsEvent.Custom(eventAction, new Dictionary<string, object>
        {
            { "label", eventLabel },
            { "value", value}
        });
    }
    
    public void NotifyStartedMatch()
    {
        _startedMatchCounter++;
        LogEvent("MatchStarted", "", _startedMatchCounter);
        PlayerPrefs.SetInt(MatchesInPreviousSittingKey, _startedMatchCounter);
    }

    public void NotifyFinishedMatch(Enumerators.EndGameType endGameType)
    {
        _finishedMatchCounter++;
        LogEvent("MatchFinished", "", _finishedMatchCounter);
    }

    public void Init()
    {
        _googleAnalytics = Object.FindObjectOfType<GoogleAnalyticsV4>();
        if (_googleAnalytics == null)
            throw new Exception("GoogleAnalyticsV4 object not found");
    }

    public void Update()
    {
    }

    void IAnalyticsManager.Dispose()
    {
        if (_googleAnalytics == null)
            return;
        
        _googleAnalytics.Dispose();
    }

    void IService.Dispose()
    {
        
    }
}
