using Loom.Client;
using Loom.ZombieBattleground.BackendCommunication;
using Opencoding.CommandHandlerSystem;

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

        [CommandHandler(Description = "Logs with a fake random user")]
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
    }
}
