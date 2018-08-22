
using LoomNetwork.CZB;

public class AnalyticsManager : IService
{
    private IAnalytics _googleAnalytics;
    
    public void Init()
    {
        _googleAnalytics = GameClient.Get<GoogleAnalyticsManager>();
        _googleAnalytics.StartSession();
    }

    public void Update()
    {
    
    }

    public void Dispose()
    {
        _googleAnalytics.Dispose();
    }

    public void LogScreen(string title)
    {
        _googleAnalytics.LogScreen(title);
    }

    public void LogEvent(string eventCategory, string eventAction, string eventLabel, long value)
    {
        _googleAnalytics.LogEvent(eventCategory, eventAction, eventLabel, value);
    }
}
