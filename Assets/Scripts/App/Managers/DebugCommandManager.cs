using Loom.ZombieBattleground;

public class DebugCommandsManager : IService
{
    public void Init()
    {
        QuickPlayCommandsHandler.Initialize();
        BattleCommandsHandler.Initialize();
    }

    public void Update()
    {
    }

    void IService.Dispose()
    {
    }
}
