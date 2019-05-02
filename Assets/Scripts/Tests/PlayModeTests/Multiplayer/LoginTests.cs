using System;
using UnityEngine;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using UnityEditor;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Loom.ZombieBattleground.Test
{
    [Category("PlayQuickSubset")]
    public class LoginTests : BaseIntegrationTest
    {
        [UnityTest]
        [Timeout(int.MaxValue)]
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

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator ForgotPasswordFlow()
        {
            return AsyncTest(async () =>
            {
                LoginPopup loginPopup = GameClient.Get<IUIManager>().GetPopup<LoginPopup>();

                HandleLogut();

                await TestHelper.LetsThink();

                await TestHelper.ClickGenericButton("Button_Login");

                await TestHelper.ClickGenericButton("Login_Group/Button_ForgotPassword");

                string email = UnityEngine.Random.Range(0, 2 ^ 1024) + UnityEngine.Random.Range(0, 2 ^ 1024) + "_Test@test.com";

                PopulateLoginPopupTextField(email, "Forgot_Group/Email_BG/Email_InputField");

                await TestHelper.LetsThink();

                await TestHelper.ClickGenericButton("Forgot_Group/Button_Send_BG/Button_Send");

                await TestHelper.LetsThink(5, true);

                await TestHelper.ClickGenericButton("SuccessForgot_Group/Button_Confirm_BG/Button_Confirm");

                await TestHelper.LetsThink();

                Assert.IsTrue(loginPopup.Self.transform.Find("Login_Group").gameObject.activeSelf);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator LoginWithWrongPasswordFlow()
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

                password = "wrongPassword";

                loginPopup.SetLoginFieldsData(email, password);

                await TestHelper.LetsThink();

                await TestHelper.ClickGenericButton("Login_Group/Button_Login_BG/Button_Login");

                await TestHelper.LetsThink(10, true);

                Assert.IsTrue(GameClient.Get<IUIManager>().GetPopup<WarningPopup>().Self != null);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator LoginWithWrongEmailFlow()
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

                email = "wrongemail@test.com";

                loginPopup.SetLoginFieldsData(email, password);

                await TestHelper.LetsThink();

                await TestHelper.ClickGenericButton("Login_Group/Button_Login_BG/Button_Login");

                await TestHelper.LetsThink(10, true);

                Assert.IsTrue(GameClient.Get<IUIManager>().GetPopup<WarningPopup>().Self != null);
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

            await new WaitUntil(() => {
                AsyncTestRunner.Instance.ThrowIfCancellationRequested();
                return loginPopup.Self == null;
            });
        }

        private void PopulateLoginPopupTextField(string text, string fieldName)
        {
            LoginPopup loginPopup = GameClient.Get<IUIManager>().GetPopup<LoginPopup>();

            if (loginPopup.Self != null)
            {
                InputField field = loginPopup.Self.transform.Find(fieldName).GetComponent<InputField>();
                field.text = text;
            }
        }
    }
}
