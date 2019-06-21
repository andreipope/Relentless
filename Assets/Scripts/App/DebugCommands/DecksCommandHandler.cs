using Loom.ZombieBattleground;
using Opencoding.CommandHandlerSystem;

static class DecksCommandHandler
{
    private static IMatchManager _matchManager;

    public static void Initialize()
    {
        CommandHandlers.RegisterCommandHandlers(typeof(DecksCommandHandler));

        _matchManager = GameClient.Get<IMatchManager>();
    }
}
