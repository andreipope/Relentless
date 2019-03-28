using UnityEngine;

public class OneSignalSystem : MonoBehaviour
{
	private static string AppId = "4bb0a2ed-ef2a-44fd-902f-0b451063e655";
	void Start ()
	{
	    #if USE_PRODUCTION_BACKEND
            AppId = "3636eb04-8b07-450b-9f46-d619d11823e4";
        #else
	        AppId = "aa0ebe27-ad5b-43b0-96a6-9b706f06a50f";
        #endif

		// Enable line below to enable logging if you are having issues setting up OneSignal. (logLevel, visualLogLevel)
        //OneSignal.SetLogLevel(OneSignal.LOG_LEVEL.INFO, OneSignal.LOG_LEVEL.INFO);
        OneSignal.SetLocationShared(false);
		OneSignal.StartInit(AppId).HandleNotificationOpened(HandleNotificationOpened).EndInit();
		OneSignal.inFocusDisplayType = OneSignal.OSInFocusDisplayOption.Notification;
	}

	// Gets called when the player opens the notification.
	private static void HandleNotificationOpened(OSNotificationOpenedResult result)
	{
	}
}
