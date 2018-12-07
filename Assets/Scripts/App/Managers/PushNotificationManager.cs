using Loom.ZombieBattleground;
using UnityEngine;

public class PushNotificationManager : IService
{
    public void Init()
    {
        ILoadObjectsManager loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
        Object.Instantiate(loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/Plugin/OneSignal"));
    }

    public void Update()
    {

    }

    public void Dispose()
    {

    }
}
