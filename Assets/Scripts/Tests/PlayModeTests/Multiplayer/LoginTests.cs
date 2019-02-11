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
        [Timeout(90000)]
        public IEnumerator RegisterAndLoginFlow()
        {
            return AsyncTest(async () =>
            {
                LoginPopup loginPoup = GameClient.Get<IUIManager>().GetPopup<LoginPopup>();

                HandleLogut();

                await TestHelper.LetsThink();

                await TestHelper.ClickGenericButton("Button_Login");

                await TestHelper.ClickGenericButton("Login_Group/Button_Register_BG/Button_Register");

                string email = UnityEngine.Random.Range(0, 2 ^ 1024) + UnityEngine.Random.Range(0, 2 ^ 1024) + "_Test@test.com";
                string password = "testing";

                loginPoup.SetRegistrationFieldsData(email, password);

                await TestHelper.LetsThink();

                await TestHelper.ClickGenericButton("Register_Group/Button_Register_BG/Button_Register");

                await TestHelper.LetsThink();

                Assert.IsTrue(IsUserLoggedIn());

                HandleLogut();

                await TestHelper.LetsThink();

                await TestHelper.ClickGenericButton("Button_Login");

                loginPoup.SetLoginFieldsData(email, password);

                await TestHelper.LetsThink();

                await TestHelper.ClickGenericButton("Login_Group/Button_Login_BG/Button_Login");

                await TestHelper.LetsThink();

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
    }
}
