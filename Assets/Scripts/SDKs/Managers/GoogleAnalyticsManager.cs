using LoomNetwork.CZB;
using UnityEngine;

public class GoogleAnalyticsManager : IAnalytics, IService
{
    private GoogleAnalyticsV4 _googleAnalytics;
    private const string OtherTrackingCode = "UA-124069166-1";
    /*private const string ProductName = "Zombie Battleground";
    private const string BundleIdentifier = "com.loomx.czb";
    private const string BundleVersion = "0.0.2";*/
    
    public void StartSession()
    {
        if (_googleAnalytics == null)
            return;
        
        _googleAnalytics.StartSession();
    }

    public void LogScreen(string title)
    {
        if (_googleAnalytics == null)
            return;
        
        Debug.Log("=== Log screen = " + title);
        _googleAnalytics.LogScreen(title);
    }

    public void LogEvent(string eventCategory, string eventAction, string eventLabel, long value)
    {
        if (_googleAnalytics == null)
            return;
        
        Debug.Log("=== Log Event = " + eventCategory);
        _googleAnalytics.LogEvent(eventCategory, eventAction, eventLabel, value);
    }

    public void Init()
    {
        _googleAnalytics = GameObject.FindObjectOfType<GoogleAnalyticsV4>();
        /*var googleAnalyticsObj = new GameObject("GAv3");
        _googleAnalytics = googleAnalyticsObj.AddComponent<GoogleAnalyticsV3>();
        _googleAnalytics.otherTrackingCode = OtherTrackingCode;
        _googleAnalytics.productName = ProductName;
        _googleAnalytics.bundleIdentifier = BundleIdentifier;
        _googleAnalytics.bundleVersion = BundleVersion;
        _googleAnalytics.logLevel = GoogleAnalyticsV3.DebugMode.VERBOSE;*/
    }

    public void Update()
    {
        //throw new System.NotImplementedException();
    }

    void IAnalytics.Dispose()
    {
        if (_googleAnalytics == null)
            return;
        
        _googleAnalytics.Dispose();
    }

    void IService.Dispose()
    {
        
    }
}
