using System;
using UnityEngine;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using UnityEngine.TestTools;

namespace Loom.ZombieBattleground.Test
{
    public class LoginTests : BaseIntegrationTest
    {
        [UnityTest]
        [Timeout(250000)]
        public IEnumerator RegisterAndLoginFlow()
        {
            return AsyncTest(async () =>
            {
                LoginPopup loginPopup = GameClient.Get<IUIManager>().GetPopup<LoginPopup>();

                DeactivateTutorialFlag();
                HandleLogut();

                await ConfirmLoginPopupHide();

                await TestHelper.ClickGenericButton("Button_Login");

                await TestHelper.ClickGenericButton("Login_Group/Button_Register_BG/Button_Register");

                string email = UnityEngine.Random.Range(0, 2 ^ 4096) + UnityEngine.Random.Range(0, 2 ^ 4096) + "_Test@test.com";
                string password = "testing";

                loginPopup.SetRegistrationFieldsData(email, password);

                await TestHelper.LetsThink();

                await TestHelper.ClickGenericButton("Register_Group/Button_Register_BG/Button_Register");

                await ConfirmLoginPopupHide();

                Assert.IsTrue(IsUserLoggedIn());

                HandleLogut();

                await ConfirmLoginPopupHide();

                await TestHelper.ClickGenericButton("Button_Login");

                loginPopup.SetLoginFieldsData(email, password);

                await TestHelper.LetsThink();

                await TestHelper.ClickGenericButton("Login_Group/Button_Login_BG/Button_Login");

                await ConfirmLoginPopupHide();

                Assert.IsTrue(IsUserLoggedIn());
            });
        }

        private void HandleLogut()
        {
            BackendDataControlMediator backendDataControlMediator = GameClient.Get<BackendDataControlMediator>();
            if (backendDataControlMediator.LoadUserDataModel() && backendDataControlMediator.UserDataModel.IsValid) 
            {
                GameClient.Get<IUIManager>().GetPopup<LoginPopup>().Logout();
            }
        }

        private bool IsUserLoggedIn()
        {
            BackendDataControlMediator backendDataControlMediator = GameClient.Get<BackendDataControlMediator>();
            if (backendDataControlMediator.LoadUserDataModel() && backendDataControlMediator.UserDataModel.IsValid)
            {
                return true;
            }
            return false;
        }

        private void DeactivateTutorialFlag()
        {
            GameClient.Get<IDataManager>().CachedUserLocalData.Tutorial = false;
        }

        private async Task ConfirmLoginPopupHide()
        {
            LoginPopup loginPopup = GameClient.Get<IUIManager>().GetPopup<LoginPopup>();

            await new WaitUntil(()=>{
                return loginPopup.Self == null;
            });
        }
    }
}
