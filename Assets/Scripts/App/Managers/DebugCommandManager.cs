using Loom.ZombieBattleground;
using UnityEngine;

public class DebugCommandsManager : IService
{
    public void Init()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        ILoadObjectsManager loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
        Object.Instantiate(loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/Plugin/DebugConsole"));

        GeneralCommandsHandler.Initialize();
        QuickPlayCommandsHandler.Initialize();
        BattleCommandsHandler.Initialize();
        DecksCommandHandler.Initialize();
        PvPCommandsHandler.Initialize();
        TutorialRewardCommandsHandler.Initialize();
        IapCommandsHandler.Initialize();
#endif
    }

    public void Update() { }

    void IService.Dispose() { }
}
