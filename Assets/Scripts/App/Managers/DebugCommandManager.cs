using Loom.ZombieBattleground;

public class DebugCommandsManager : IService
{
    public void Init()
    {
        BattleCommandHandlers.Initialize();
    }

    public void Update()
    {
    }

    void IService.Dispose()
    {
    }
}
