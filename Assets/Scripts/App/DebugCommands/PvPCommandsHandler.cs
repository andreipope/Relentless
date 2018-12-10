using Opencoding.CommandHandlerSystem;

namespace Loom.ZombieBattleground
{
    public static class PvPCommandsHandler
    {
        private static IPvPManager _pvpManager;

        public static void Initialize()
        {
            CommandHandlers.RegisterCommandHandlers(typeof(PvPCommandsHandler));

            _pvpManager = GameClient.Get<IPvPManager>();
        }

        [CommandHandler(Description = "Whether to use backend-based logic, or trust the clients")]
        private static void SetUseBackendLogic(bool useBackendLogic)
        {
            _pvpManager.UseBackendLogic = useBackendLogic;
        }
    }
}
