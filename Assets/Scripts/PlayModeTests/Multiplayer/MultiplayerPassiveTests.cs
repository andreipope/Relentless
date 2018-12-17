using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Loom.ZombieBattleground.Test
{
    public class MultiplayerPassiveTests : BaseIntegrationTest
    {
        [UnityTest]
        [Timeout(180 * 1000 * TestHelper.TestTimeScale)]
        public IEnumerator Test_A1_MatchmakeAndPlay()
        {
            return AsyncTest(async () =>
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
                        () => TestHelper.PlayAMatch(),
                        TestHelper.PressOK);

                    await TestHelper.LetsThink();
                }
            });
        }
    }
}
