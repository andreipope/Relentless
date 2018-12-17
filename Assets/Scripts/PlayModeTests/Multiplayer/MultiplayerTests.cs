using System;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using UnityEngine.TestTools;

namespace Loom.ZombieBattleground.Test
{
    public class MultiplayerTests : BaseIntegrationTest
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
        [Timeout(150 * 1000 * TestHelper.TestTimeScale)]
        public IEnumerator Test_A0_PlayScenarioGame1()
        {
            _testHelper.SetTestName("PvP - Scenario");
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
            _testHelper.SetPvPTags(new[]
            {
                "pvpTest", "scenario1"
            });
            _testHelper.DebugCheatsConfiguration.Enabled = true;
            _testHelper.DebugCheatsConfiguration.CustomRandomSeed = 0;

            yield return _testHelper.LetsThink();

            yield return _testHelper.MainMenuTransition("Button_Battle");

            yield return TestHelper.TaskAsIEnumerator(_testHelper.CreateAndConnectOpponentDebugClient());

            IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
            {
                opponent =>
                {
                },
                player =>
                {
                },
                opponent =>
                {
                },
                player =>
                {
                },
                opponent =>
                {
                },
                player =>
                {
                },
                opponent =>
                {
                },
                player =>
                {
                },
                opponent =>
                {
                },
                player => player.CardPlay(new InstanceId(38), 0), opponent => opponent.CardPlay(new InstanceId(2), 0),
                player => player.CardAttack(new InstanceId(38), Enumerators.AffectObjectType.Player, _testHelper.GetOpponentPlayer().InstanceId),
                opponent => opponent.CardAttack(new InstanceId(2),
                    Enumerators.AffectObjectType.Player,
                    _testHelper.GetCurrentPlayer().InstanceId),
                player => player.CardAttack(new InstanceId(38), Enumerators.AffectObjectType.Player, _testHelper.GetOpponentPlayer().InstanceId),
                opponent => opponent.CardAttack(new InstanceId(2),
                    Enumerators.AffectObjectType.Player,
                    _testHelper.GetCurrentPlayer().InstanceId),
                player => player.CardAttack(new InstanceId(38), Enumerators.AffectObjectType.Player, _testHelper.GetOpponentPlayer().InstanceId),
                opponent =>
                {
                },
                player => player.CardAttack(new InstanceId(38), Enumerators.AffectObjectType.Player, _testHelper.GetOpponentPlayer().InstanceId),
                opponent =>
                {
                },
                player => player.CardAttack(new InstanceId(38), Enumerators.AffectObjectType.Player, _testHelper.GetOpponentPlayer().InstanceId),
                opponent =>
                {
                },
                player => player.CardAttack(new InstanceId(38), Enumerators.AffectObjectType.Player, _testHelper.GetOpponentPlayer().InstanceId),
                opponent =>
                {
                },
                player => player.CardAttack(new InstanceId(38), Enumerators.AffectObjectType.Player, _testHelper.GetOpponentPlayer().InstanceId),
                opponent =>
                {
                },
                player => player.CardAttack(new InstanceId(38), Enumerators.AffectObjectType.Player, _testHelper.GetOpponentPlayer().InstanceId),
                opponent =>
                {
                },
                player => player.CardAttack(new InstanceId(38), Enumerators.AffectObjectType.Player, _testHelper.GetOpponentPlayer().InstanceId),
                opponent =>
                {
                },
                player => player.CardAttack(new InstanceId(38), Enumerators.AffectObjectType.Player, _testHelper.GetOpponentPlayer().InstanceId),
            };

            MatchScenarioPlayer matchScenarioPlayer = new MatchScenarioPlayer(_testHelper, turns);
            yield return TestHelper.TaskAsIEnumerator(_testHelper.MatchmakeOpponentDebugClient());

            yield return matchScenarioPlayer.Play();
        }

        [UnityTest]
        [Timeout(50 * 1000 * TestHelper.TestTimeScale)]
        public IEnumerator Test_A1_MatchmakingCancel()
        {
            _testHelper.SetTestName("PvP - Matchmaking Cancel");
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
            _testHelper.SetPvPTags(new[]
            {
                "pvpTest", "NoOpponentCancel"
            });

            yield return _testHelper.LetsThink();

            yield return _testHelper.MainMenuTransition("Button_Battle");

            yield return _testHelper.LetsThink(10);

            yield return _testHelper.ClickGenericButton("Button_Cancel");

            yield return _testHelper.LetsThink();

            _testHelper.TestEndHandler();
        }

        [UnityTest]
        [Timeout(50 * 1000 * TestHelper.TestTimeScale)]
        public IEnumerator Test_A2_MatchmakingTimeout()
        {
            _testHelper.SetTestName("PvP - Matchmaking Cancel");
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
            _testHelper.SetPvPTags(new[]
            {
                "pvpTest", "NoOpponentTimeout"
            });

            yield return _testHelper.LetsThink();

            yield return _testHelper.MainMenuTransition("Button_Battle");
            yield return _testHelper.AssertPvPStartedOrMatchmakingFailed(
                _testHelper.PlayAMatch(),
                _testHelper.ClickGenericButton("Button_Cancel"));

            yield return _testHelper.LetsThink();

            _testHelper.TestEndHandler();
        }

        [UnityTest]
        [Timeout(50 * 1000 * TestHelper.TestTimeScale)]
        public IEnumerator Test_A3_MatchmakeAndQuit()
        {
            _testHelper.SetTestName("PvP - Matchmaking And Quit");
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
            _testHelper.SetPvPTags(new[]
            {
                "pvpTest"
            });

            yield return _testHelper.LetsThink();

            yield return _testHelper.MainMenuTransition("Button_Battle");

            yield return TestHelper.TaskAsIEnumerator(_testHelper.CreateAndConnectOpponentDebugClient());
            yield return TestHelper.TaskAsIEnumerator(_testHelper.MatchmakeOpponentDebugClient());

            yield return _testHelper.AssertCurrentPageName("GameplayPage");
            yield return _testHelper.WaitUntilPlayerOrderIsDecided();
            _testHelper.AssertOverlordName();

            yield return _testHelper.ClickGenericButton("Button_Settings");
            yield return _testHelper.ClickGenericButton("Button_QuitToMainMenu");
            yield return _testHelper.RespondToYesNoOverlay(true);

            yield return _testHelper.LetsThink();

            _testHelper.TestEndHandler();
        }

        [UnityTest]
        [Timeout(50 * 1000 * TestHelper.TestTimeScale)]
        public IEnumerator Test_A4_MatchmakeWaitForOurTurnAndQuit()
        {
            _testHelper.SetTestName("PvP - Matchmake, Wait for Our Turn and Quit");

            yield return _testHelper.ClickGenericButton("Button_Play");

            yield return _testHelper.AssertIfWentDirectlyToTutorial(
                _testHelper.GoBackToMainAndPressPlay());

            yield return _testHelper.AssertCurrentPageName("PlaySelectionPage");
            yield return _testHelper.ClickGenericButton("Button_PvPMode");
            yield return _testHelper.AssertCurrentPageName("PvPSelectionPage");
            yield return _testHelper.ClickGenericButton("Button_CasualType");
            yield return _testHelper.AssertCurrentPageName("HordeSelectionPage");

            int selectedHordeIndex = 0;

            yield return _testHelper.SelectAHordeByIndex(selectedHordeIndex);
            _testHelper.RecordExpectedOverlordName(selectedHordeIndex);
            _testHelper.SetPvPTags(new[]
            {
                "pvpTest"
            });

            yield return _testHelper.LetsThink();

            yield return _testHelper.ClickGenericButton("Button_Battle");
            yield return _testHelper.AssertCurrentPageName("GameplayPage");
            yield return _testHelper.WaitUntilPlayerOrderIsDecided();
            yield return _testHelper.AssertMulliganPopupCameUp(
                _testHelper.ClickGenericButton("Button_Keep"),
                null);
            yield return _testHelper.WaitUntilOurFirstTurn();
            yield return _testHelper.ClickGenericButton("Button_Settings");
            yield return _testHelper.ClickGenericButton("Button_QuitToMainMenu");
            yield return _testHelper.RespondToYesNoOverlay(true);

            _testHelper.TestEndHandler();
        }

        [UnityTest]
        [Timeout(500000)]
        public IEnumerator Test_A5_MatchmakeMakeOneMoveAndQuit()
        {
            _testHelper.SetTestName("PvP - Matchmake, Make One Move And Quit");

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
            _testHelper.SetPvPTags(new[]
            {
                "pvpTest"
            });

            yield return _testHelper.LetsThink();

            yield return _testHelper.MainMenuTransition("Button_Battle");

            yield return TestHelper.TaskAsIEnumerator(_testHelper.CreateAndConnectOpponentDebugClient());
            _testHelper.SetupOpponentDebugClientToEndTurns();
            yield return TestHelper.TaskAsIEnumerator(_testHelper.MatchmakeOpponentDebugClient());

            yield return _testHelper.PlayAMatch(1);
            yield return _testHelper.ClickGenericButton("Button_Settings");
            yield return _testHelper.ClickGenericButton("Button_QuitToMainMenu");
            yield return _testHelper.RespondToYesNoOverlay(true);

            _testHelper.TestEndHandler();
        }

        [UnityTest]
        [Timeout(300 * 1000 * TestHelper.TestTimeScale)]
        public IEnumerator Test_A6_MatchmakeAndPlay()
        {
            _testHelper.SetTestName("PvP - Matchmaking And Play");
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
            _testHelper.SetPvPTags(new[]
            {
                "pvpTest"
            });

            yield return _testHelper.LetsThink();

            yield return _testHelper.MainMenuTransition("Button_Battle");

            yield return TestHelper.TaskAsIEnumerator(_testHelper.CreateAndConnectOpponentDebugClient());
            _testHelper.SetupOpponentDebugClientToEndTurns();
            yield return TestHelper.TaskAsIEnumerator(_testHelper.MatchmakeOpponentDebugClient());

            yield return _testHelper.PlayAMatch();

            _testHelper.TestEndHandler();
        }

        [UnityTest]
        [Timeout(50 * 1000 * TestHelper.TestTimeScale)]
        public IEnumerator Test_A7_MatchmakingCancelAndMatchmake()
        {
            _testHelper.SetTestName("PvP - Create a Horde and save");
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

            #region Matchmaking Cancel

            _testHelper.SetPvPTags(new[]
            {
                "pvpTestNoOpponentCancel"
            });

            yield return _testHelper.LetsThink();
            yield return _testHelper.MainMenuTransition("Button_Battle");

            yield return _testHelper.LetsThink();
            yield return _testHelper.LetsThink();
            yield return _testHelper.LetsThink();

            yield return _testHelper.ClickGenericButton("Button_Cancel");

            #endregion

            yield return _testHelper.LetsThink();
            yield return _testHelper.LetsThink();

            #region Matchmake and Quit

            _testHelper.SetPvPTags(new[]
            {
                "pvpTest"
            });

            yield return _testHelper.LetsThink();

            yield return _testHelper.MainMenuTransition("Button_Battle");
            yield return _testHelper.AssertCurrentPageName("GameplayPage");
            yield return _testHelper.WaitUntilPlayerOrderIsDecided();
            _testHelper.AssertOverlordName();
            yield return _testHelper.ClickGenericButton("Button_Settings");

            yield return _testHelper.LetsThink();

            yield return _testHelper.ClickGenericButton("Button_QuitToMainMenu");

            yield return _testHelper.LetsThink();

            yield return _testHelper.RespondToYesNoOverlay(true);

            yield return _testHelper.LetsThink();

            #endregion

            _testHelper.TestEndHandler();
        }
    }
}
