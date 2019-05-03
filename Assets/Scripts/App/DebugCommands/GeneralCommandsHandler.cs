using Loom.Client;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Protobuf;
using Opencoding.CommandHandlerSystem;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public static class GeneralCommandsHandler
    {
        private static BackendDataControlMediator _backendDataControlMediator;

        public static void Initialize()
        {
            CommandHandlers.RegisterCommandHandlers(typeof(GeneralCommandsHandler));

            _backendDataControlMediator = GameClient.Get<BackendDataControlMediator>();
        }

        [CommandHandler(Description = "Logs in into the game with a fake random user")]
        private static async void FakeLogin()
        {
            UserDataModel userDataModel = new UserDataModel(
                "ZombieSlayer_Fake_" + UnityEngine.Random.Range(int.MinValue, int.MaxValue).ToString().Replace("-", "0"),
                CryptoUtils.GeneratePrivateKey()
            );

            _backendDataControlMediator.SetUserDataModel(userDataModel);
            await _backendDataControlMediator.LoginAndLoadData();

            userDataModel.IsValid = true;
            _backendDataControlMediator.SetUserDataModel(userDataModel);

            GameClient.Get<IUIManager>().GetPopup<LoginPopup>().Hide();

        }

        [CommandHandler(Description = "Skips tutorial if you inside it")]
        public static void SkipTutorialFlow()
        {
            if (!GameClient.Get<ITutorialManager>().IsTutorial)
                return;

            if (GameClient.Get<IAppStateManager>().AppState == Common.Enumerators.AppState.GAMEPLAY)
            {
                GameClient.Get<IGameplayManager>().EndGame(Common.Enumerators.EndGameType.CANCEL);
                GameClient.Get<IMatchManager>().FinishMatch(Common.Enumerators.AppState.MAIN_MENU);
            }
            else
            {
                GameClient.Get<IAppStateManager>().ChangeAppState(Common.Enumerators.AppState.MAIN_MENU, true);
            }

            GameClient.Get<IDataManager>().CachedUserLocalData.Tutorial = false;
            GameClient.Get<IDataManager>().CachedUserLocalData.CurrentTutorialId = 0;
            GameClient.Get<ITutorialManager>().StopTutorial(true);
            GameClient.Get<IGameplayManager>().IsTutorial = false;
            GameClient.Get<IGameplayManager>().IsSpecificGameplayBattleground = false;
        }

        [CommandHandler(Description = "Get notifications from server")]
        public static async void GetNotifications()
        {
            GetNotificationsResponse response = await GameClient.Get<BackendFacade>().GetNotifications(_backendDataControlMediator.UserDataModel.UserId);
            Debug.Log(JsonUtility.PrettyPrint(response.ToString()));
        }

        [CommandHandler(Description = "Clear notification on server")]
        public static async void ClearNotification(int notificationId)
        {
            ClearNotificationsResponse response =
                await GameClient.Get<BackendFacade>().ClearNotifications(_backendDataControlMediator.UserDataModel.UserId, new []{ notificationId });
            Debug.Log(JsonUtility.PrettyPrint(response.ToString()));
        }
    }
}
