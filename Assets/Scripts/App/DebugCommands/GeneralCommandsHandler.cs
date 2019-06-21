using Loom.Client;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.Protobuf;
using Opencoding.CommandHandlerSystem;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public static class GeneralCommandsHandler
    {
        private static BackendDataControlMediator _backendDataControlMediator;
        private static BackendFacade _backendFacade;

        public static void Initialize()
        {
            CommandHandlers.RegisterCommandHandlers(typeof(GeneralCommandsHandler));

            _backendDataControlMediator = GameClient.Get<BackendDataControlMediator>();
            _backendFacade = GameClient.Get<BackendFacade>();
        }

        [CommandHandler(Description = "Logs in into the game with a fake random user")]
        private static async void FakeLogin()
        {
            long userId = UnityEngine.Random.Range(100000, int.MaxValue);
            UserDataModel userDataModel = new UserDataModel(
                "ZombieSlayer_Fake_" + userId,
                userId,
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

        [CommandHandler(Description = "Add experience to an overlord")]
        public static async void AddSoloExperience(int overlordId, int experience, int deckId = 1, bool isWin = true)
        {
            await GameClient.Get<BackendFacade>()
                .AddSoloExperience(
                    _backendDataControlMediator.UserDataModel.UserId,
                    new OverlordId(overlordId),
                    new DeckId(deckId),
                    experience,
                    isWin
                );

            Debug.Log("Added experience");
        }

        [CommandHandler(Description = "Get overlord user instance from server")]
        public static async void GetOverlordUserInstance(int overlordId)
        {
            GetOverlordUserInstanceResponse getOverlordUserInstanceResponse =
                await GameClient.Get<BackendFacade>()
                .GetOverlordUserInstance(
                    _backendDataControlMediator.UserDataModel.UserId,
                    new OverlordId(overlordId)
                );

            Debug.Log(JsonUtility.PrettyPrint(getOverlordUserInstanceResponse.Overlord.ToString()));
        }

        [CommandHandler]
        public static void ShowYouWonYouLostPopup(bool win = true)
        {
            GameClient.Get<IUIManager>().DrawPopup<YouWonYouLostPopup>(new object[] { win });
        }

        [CommandHandler]
        public static void HideYouWonYouLostPopup()
        {
            GameClient.Get<IUIManager>().HidePopup<YouWonYouLostPopup>();
        }

        [CommandHandler]
        public static void ShowPackOpenerV1()
        {
            GameClient.Get<IUIManager>().SetPage<PackOpenerPageWithNavigationBar>();
        }

        [CommandHandler]
        public static void ShowPackOpenerV2()
        {
            GameClient.Get<IUIManager>().SetPage<PackOpenerPageWithNavigationBarV2>();
        }

        [CommandHandler]
        public static async void DebugGetUserIdByAddress()
        {
            string userId = await _backendFacade.DebugGetUserIdByAddress(_backendDataControlMediator.UserDataModel.Address);
            Debug.Log("User Id: " + userId);
        }

        [CommandHandler]
        public static async void DebugGetPendingCardAmountChangeItems()
        {
            DebugGetPendingCardAmountChangeItemsResponse response =
                await _backendFacade.DebugGetPendingCardAmountChangeItems(_backendDataControlMediator.UserDataModel.Address);
            Debug.Log(JsonUtility.PrettyPrint(response.ToString()));
        }
    }
}
