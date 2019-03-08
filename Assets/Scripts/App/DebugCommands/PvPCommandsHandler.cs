using log4net;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Protobuf;
using Opencoding.CommandHandlerSystem;
using InstanceId = Loom.ZombieBattleground.Data.InstanceId;

namespace Loom.ZombieBattleground
{
    public static class PvPCommandsHandler
    {
        private static readonly ILog Log = Logging.GetLog(nameof(PvPCommandsHandler));

        private static IPvPManager _pvpManager;
        private static IQueueManager _queueManager;
        private static BackendDataControlMediator _backendDataControlMediator;

        public static void Initialize()
        {
            CommandHandlers.RegisterCommandHandlers(typeof(PvPCommandsHandler));

            _pvpManager = GameClient.Get<IPvPManager>();
            _queueManager = GameClient.Get<IQueueManager>();
            _backendDataControlMediator = GameClient.Get<BackendDataControlMediator>();
        }

        [CommandHandler(Description = "Whether to use backend-based game logic, or trust the clients")]
        private static void UseBackendGameLogic(bool useBackendGameLogic)
        {
            _pvpManager.UseBackendGameLogic = useBackendGameLogic;
        }

        [CommandHandler]
        private static void EnableCheats(bool enableCheats)
        {
            _pvpManager.DebugCheats.Enabled = enableCheats;
        }

        [CommandHandler]
        private static void DestroyCardOnBoard(int cardId)
        {
            if (!_pvpManager.DebugCheats.Enabled)
            {
                Log.Error("Cheat must be enabled, use EnableCheats before match");
                return;
            }

            MatchRequestFactory matchRequestFactory = new MatchRequestFactory(_pvpManager.MatchMetadata.Id);
            PlayerActionFactory playerActionFactory = new PlayerActionFactory(_backendDataControlMediator.UserDataModel.UserId);
            PlayerAction action = playerActionFactory.CheatDestroyCardsOnBoard(new[] { new InstanceId(cardId) });
            _queueManager.AddAction(matchRequestFactory.CreateAction(action));
        }
    }
}
