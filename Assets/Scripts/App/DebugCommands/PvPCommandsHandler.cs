using log4net;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Protobuf;
using InstanceId = Loom.ZombieBattleground.Data.InstanceId;

namespace Loom.ZombieBattleground
{
    public static class PvPCommandsHandler
    {
        private static readonly ILog Log = Logging.GetLog(nameof(PvPCommandsHandler));

        private static IPvPManager _pvpManager;
        private static INetworkActionManager _networkActionManager;
        private static BackendDataControlMediator _backendDataControlMediator;

        public static void Initialize()
        {
            _pvpManager = GameClient.Get<IPvPManager>();
            _networkActionManager = GameClient.Get<INetworkActionManager>();
            _backendDataControlMediator = GameClient.Get<BackendDataControlMediator>();
        }

        private static void UseBackendGameLogic(bool useBackendGameLogic)
        {
            _pvpManager.UseBackendGameLogic = useBackendGameLogic;
        }

        private static void EnableCheats(bool enableCheats)
        {
            _pvpManager.DebugCheats.Enabled = enableCheats;
        }

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
            _networkActionManager.EnqueueMessage(matchRequestFactory.CreateAction(action));
        }
    }
}
