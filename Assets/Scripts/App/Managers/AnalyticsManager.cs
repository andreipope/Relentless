using System;
using System.CodeDom;
using System.Collections.Generic;
using Loom.ZombieBattleground;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Common;
using UnityEngine;
using UnityEngine.Analytics;
using Object = UnityEngine.Object;
using mixpanel;

public class AnalyticsManager : IAnalyticsManager, IService
{
    private const string MatchesInPreviousSittingKey = "Analytics_MatchesPerSitting";
    private const string FirstTimeInstallKey = "Analytics_FirstTimeInstall";

    private GoogleAnalyticsV4 _googleAnalytics;

    private int _startedMatchCounter;

    private int _finishedMatchCounter;

    public const string EventLogIn = "Log In";
    public const string EventStartedTutorial = "Started Tutorial";
    public const string EventCompletedTutorial = "Completed Tutorial";
    public const string EventStartedMatch = "Started Match";
    public const string EventEndedMatch = "Ended Match";
    public const string EventDeckCreated = "Create Deck";
    public const string EventDeckDeleted = "Delete Deck";
    public const string EventDeckEdited = "Edit Deck";

    public const string PropertyTesterKey = "Tester Key";
    public const string PropertyDAppChainWalletAddress = "DAppChainWallet Address";

    private BackendFacade _backendFacade;
    private BackendDataControlMediator _backendDataControlMediator;


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

        // first Install
        bool isFirstTimeInstall = PlayerPrefs.GetInt(FirstTimeInstallKey, 0) == 0;
        if (isFirstTimeInstall)
        {
            LogEvent("NewInstallation", Application.platform.ToString(), 0);
            PlayerPrefs.SetInt(FirstTimeInstallKey, 1);
        }
    }

    public void LogScreen(string title)
    {
        Debug.Log("=== Log screen = " + title);
        _googleAnalytics.LogScreen(title);
        AnalyticsEvent.ScreenVisit(title);

        Mixpanel.Track(title);
    }

    public void LogEvent(string eventAction, string eventLabel, long value)
    {
        Debug.Log("=== Log Event = " + eventAction);
        _googleAnalytics.LogEvent("Game Event", eventAction, eventLabel, value);
        AnalyticsEvent.Custom(
            eventAction,
            new Dictionary<string, object>
            {
                {
                    "label", eventLabel
                },
                {
                    "value", value
                }
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

    void IAnalyticsManager.Dispose()
    {
        if (_googleAnalytics == null)
            return;

        _googleAnalytics.Dispose();
    }

    public void Init()
    {
        _googleAnalytics = Object.FindObjectOfType<GoogleAnalyticsV4>();
        if (_googleAnalytics == null)
            throw new Exception("GoogleAnalyticsV4 object not found");

        ILoadObjectsManager loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
        _backendFacade = GameClient.Get<BackendFacade>();
        _backendDataControlMediator = GameClient.Get<BackendDataControlMediator>();
        Object.Instantiate(loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/Plugin/Mixpanel"));
    }

    public void Update()
    {
    }

    void IService.Dispose()
    {
    }

    public void SetEvent(string eventName)
    {
        Value props = new Value();
        props[PropertyTesterKey] = _backendDataControlMediator.UserDataModel.UserId;
        props[PropertyDAppChainWalletAddress] = _backendFacade.DAppChainWalletAddress;

        Mixpanel.Identify(_backendDataControlMediator.UserDataModel.UserId);
        Mixpanel.Track(eventName, props);
    }

    public void SetPoepleProperty(string identityId, string property, string value)
    {
        if (string.IsNullOrEmpty(identityId))
            return;

        Mixpanel.Identify(identityId);
        Mixpanel.people.Set(property, value);
    }

    public void SetSuperProperty(string property, string value)
    {
        Mixpanel.Register(property, value);
    }

    public void SetPoepleIncrement(string property, int value)
    {
        Mixpanel.people.Increment(property, value);
    }
}
