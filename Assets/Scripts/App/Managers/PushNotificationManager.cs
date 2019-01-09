using Loom.ZombieBattleground;
using UnityEngine;

public class PushNotificationManager : IService
{
    public void Init()
    {
        ILoadObjectsManager loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
        GameObject oneSignalObj = Object.Instantiate(loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/Plugin/OneSignal"));
        Object.DontDestroyOnLoad(oneSignalObj);
    }

    public void Update()
    {

    }

    public void Dispose()
    {

    }
}
