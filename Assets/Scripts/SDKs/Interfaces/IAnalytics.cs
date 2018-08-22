
public interface IAnalytics
{
    void StartSession();
    void LogScreen(string title);
    void LogEvent(string eventCategory, string eventAction, string eventLabel, long value);
    void Dispose();
}
