using System.Collections;
using Loom.ZombieBattleground.BackendCommunication;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Loom.ZombieBattleground.Test
{
    public class MultiplayerPassiveTests : BaseIntegrationTest
    {
        #region Setup & TearDown

        [UnityTearDown]
        public override IEnumerator PerTestTearDown()
        {
            TestHelper.DebugCheatsConfiguration = new DebugCheatsConfiguration();

            if (TestContext.CurrentContext.Test.Name == "TestN_Cleanup")
            {
                await TestHelper.TearDown_Cleanup();
            }
            else
            {
                await TestHelper.TearDown_GoBackToMainScreen();
            }

            TestHelper.ReportTestTime();
        }

        #endregion

        [UnityTest]
        [Timeout(180 * 1000 * TestHelper.TestTimeScale)]
        public async Task Test_A1_MatchmakeAndPlay()
        {
            TestHelper.SetTestName("PvP - Passive Matchmake And Play");
            TestHelper.SetPvPTags(new[]
            {
                "pvpTest"
            });

            await TestHelper.LetsThink();

            await TestHelper.MainMenuTransition("Button_Play");
            await TestHelper.AssertIfWentDirectlyToTutorial(
                TestHelper.GoBackToMainAndPressPlay);

            await TestHelper.AssertCurrentPageName("PlaySelectionPage");
            await TestHelper.MainMenuTransition("Button_PvPMode");
            await TestHelper.AssertCurrentPageName("PvPSelectionPage");
            await TestHelper.MainMenuTransition("Button_CasualType");
            await TestHelper.AssertCurrentPageName("HordeSelectionPage");

            int selectedHordeIndex = 0;

            await TestHelper.SelectAHordeByIndex(selectedHordeIndex);
            TestHelper.RecordExpectedOverlordName(selectedHordeIndex);

            while (true)
            {
                await TestHelper.ClickGenericButton("Button_Battle");
                await TestHelper.AssertPvPStartedOrMatchmakingFailed(
                    TestHelper.PlayAMatch(),
                    TestHelper.PressOK());

                await TestHelper.LetsThink();
            }
        }
    }
}
