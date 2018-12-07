using UnityEngine;

public class OneSignalSystem : MonoBehaviour
{
	private const string AppId = "4bb0a2ed-ef2a-44fd-902f-0b451063e655";
	void Start ()
	{
		// Enable line below to enable logging if you are having issues setting up OneSignal. (logLevel, visualLogLevel)
        //OneSignal.SetLogLevel(OneSignal.LOG_LEVEL.INFO, OneSignal.LOG_LEVEL.INFO);

		OneSignal.StartInit(AppId).HandleNotificationOpened(HandleNotificationOpened).EndInit();
		OneSignal.inFocusDisplayType = OneSignal.OSInFocusDisplayOption.Notification;
	}

	// Gets called when the player opens the notification.
	private static void HandleNotificationOpened(OSNotificationOpenedResult result)
	{
	}
}
