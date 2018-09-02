using LoomNetwork.CZB.Common;

public interface IAnalyticsManager
{
    void StartSession();

    void LogScreen(string title);

    void LogEvent(string eventAction, string eventLabel, long value);

    void NotifyStartedMatch();

    void NotifyFinishedMatch(Enumerators.EndGameType endGameType);

    void Dispose();
}
