using Loom.ZombieBattleground;
using UnityEngine;

public class DebugCommandsManager : IService
{
    public void Init()
    {
        #if UNITY_EDITOR || DEVELOPMENT
        ILoadObjectsManager loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
        Object.Instantiate(loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/Plugin/DebugConsole"));

        QuickPlayCommandsHandler.Initialize();
        BattleCommandsHandler.Initialize();
        DecksCommandHandler.Initialize();
        #endif
    }

    public void Update()
    {
    }

    void IService.Dispose()
    {
    }
}
