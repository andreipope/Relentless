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
            _testHelper.DebugCheatsConfiguration = new DebugCheatsConfiguration();

            if (TestContext.CurrentContext.Test.Name == "TestN_Cleanup")
            {
                yield return _testHelper.TearDown_Cleanup();
            }
            else
            {
                yield return _testHelper.TearDown_GoBackToMainScreen();
            }

            _testHelper.ReportTestTime();
        }

        #endregion

        [UnityTest]
        [Timeout(180 * 1000 * TestHelper.TestTimeScale)]
        public IEnumerator Test_A1_MatchmakeAndPlay()
        {
            _testHelper.SetTestName("PvP - Passive Matchmake And Play");
            _testHelper.SetPvPTags(new[]
            {
                "pvpTest"
            });

            yield return _testHelper.LetsThink();

            yield return _testHelper.MainMenuTransition("Button_Play");
            yield return _testHelper.AssertIfWentDirectlyToTutorial(
                _testHelper.GoBackToMainAndPressPlay());

            yield return _testHelper.AssertCurrentPageName("PlaySelectionPage");
            yield return _testHelper.MainMenuTransition("Button_PvPMode");
            yield return _testHelper.AssertCurrentPageName("PvPSelectionPage");
            yield return _testHelper.MainMenuTransition("Button_CasualType");
            yield return _testHelper.AssertCurrentPageName("HordeSelectionPage");

            int selectedHordeIndex = 0;

            yield return _testHelper.SelectAHordeByIndex(selectedHordeIndex);
            _testHelper.RecordExpectedOverlordName(selectedHordeIndex);

            while (true)
            {
                yield return _testHelper.ClickGenericButton("Button_Battle");
                yield return _testHelper.AssertPvPStartedOrMatchmakingFailed(
                    _testHelper.PlayAMatch(),
                    _testHelper.PressOK());

                yield return _testHelper.LetsThink();
            }
        }
    }
}
