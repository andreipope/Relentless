using System;
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
    public class MultiplayerTests : BaseIntegrationTest
    {
        [UnityTest]
        [Timeout(150 * 1000 * TestHelper.TestTimeScale)]
        public IEnumerator Test_A0_PlayScenarioGame1()
        {
            return AsyncTest(async () =>
            {
                TestHelper.SetTestName("PvP - Scenario");
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
                TestHelper.SetPvPTags(new[]
                {
                    "pvpTest", "scenario1"
                });
                TestHelper.DebugCheatsConfiguration.Enabled = true;
                TestHelper.DebugCheatsConfiguration.CustomRandomSeed = 0;

                await TestHelper.LetsThink();

                await TestHelper.MainMenuTransition("Button_Battle");

                await TestHelper.CreateAndConnectOpponentDebugClient();

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                    opponent => {},
                    player => {},
                    opponent => {},
                    player => {},
                    opponent => {},
                    player => {},
                    opponent => {},
                    player => {},
                    opponent => {},
                    player => player.CardPlay(new InstanceId(38), 0),
                    opponent => opponent.CardPlay(new InstanceId(2), 0),
                    player => player.CardAttack(new InstanceId(38), Enumerators.AffectObjectType.Player, TestHelper.GetOpponentPlayer().InstanceId),
                    opponent => opponent.CardAttack(new InstanceId(2), Enumerators.AffectObjectType.Player, TestHelper.GetCurrentPlayer().InstanceId),
                    player => player.CardAttack(new InstanceId(38), Enumerators.AffectObjectType.Player, TestHelper.GetOpponentPlayer().InstanceId),
                    opponent => opponent.CardAttack(new InstanceId(2), Enumerators.AffectObjectType.Player, TestHelper.GetCurrentPlayer().InstanceId),
                    player => player.CardAttack(new InstanceId(38), Enumerators.AffectObjectType.Player,TestHelper.GetOpponentPlayer().InstanceId),
                    opponent => {},
                    player => player.CardAttack(new InstanceId(38), Enumerators.AffectObjectType.Player, TestHelper.GetOpponentPlayer().InstanceId),
                    opponent => {},
                    player => player.CardAttack(new InstanceId(38), Enumerators.AffectObjectType.Player, TestHelper.GetOpponentPlayer().InstanceId),
                    opponent => {},
                    player => player.CardAttack(new InstanceId(38), Enumerators.AffectObjectType.Player, TestHelper.GetOpponentPlayer().InstanceId),
                    opponent => {},
                    player => player.CardAttack(new InstanceId(38), Enumerators.AffectObjectType.Player, TestHelper.GetOpponentPlayer().InstanceId),
                    opponent => {},
                    player => player.CardAttack(new InstanceId(38), Enumerators.AffectObjectType.Player, TestHelper.GetOpponentPlayer().InstanceId),
                    opponent => {},
                    player => player.CardAttack(new InstanceId(38), Enumerators.AffectObjectType.Player, TestHelper.GetOpponentPlayer().InstanceId),
                    opponent => {},
                    player => player.CardAttack(new InstanceId(38), Enumerators.AffectObjectType.Player, TestHelper.GetOpponentPlayer().InstanceId),
                };

                MatchScenarioPlayer matchScenarioPlayer = new MatchScenarioPlayer(TestHelper, turns);
                await TestHelper.MatchmakeOpponentDebugClient();

                await matchScenarioPlayer.Play();
            });
        }

        [UnityTest]
        [Timeout(50 * 1000 * TestHelper.TestTimeScale)]
        public IEnumerator Test_A1_MatchmakingCancel()
        {
            return AsyncTest(async () =>
            {
                TestHelper.SetTestName("PvP - Matchmaking Cancel");
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
                TestHelper.SetPvPTags(new[]
                {
                    "pvpTest", "NoOpponentCancel"
                });

                await TestHelper.LetsThink();

                await TestHelper.MainMenuTransition("Button_Battle");

                await TestHelper.LetsThink(10);

                await TestHelper.ClickGenericButton("Button_Cancel");

                await TestHelper.LetsThink();

                TestHelper.TestEndHandler();
            });
        }

        [UnityTest]
        [Timeout(50 * 1000 * TestHelper.TestTimeScale)]
        public IEnumerator Test_A2_MatchmakingTimeout()
        {
            return AsyncTest(async () =>
            {
                TestHelper.SetTestName("PvP - Matchmaking Cancel");
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
                TestHelper.SetPvPTags(new[]
                {
                    "pvpTest", "NoOpponentTimeout"
                });

                await TestHelper.LetsThink();

                await TestHelper.MainMenuTransition("Button_Battle");
                await TestHelper.AssertPvPStartedOrMatchmakingFailed(
                    () => TestHelper.PlayAMatch(),
                    () => TestHelper.ClickGenericButton("Button_Cancel"));

                await TestHelper.LetsThink();

                TestHelper.TestEndHandler();
            });
        }

        [UnityTest]
        [Timeout(50 * 1000 * TestHelper.TestTimeScale)]
        public IEnumerator Test_A3_MatchmakeAndQuit()
        {
            return AsyncTest(async () =>
            {
                TestHelper.SetTestName("PvP - Matchmaking And Quit");
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
                TestHelper.SetPvPTags(new[]
                {
                    "pvpTest"
                });

                await TestHelper.LetsThink();

                await TestHelper.MainMenuTransition("Button_Battle");

                await TestHelper.CreateAndConnectOpponentDebugClient();
                await TestHelper.MatchmakeOpponentDebugClient();

                await TestHelper.AssertCurrentPageName("GameplayPage");
                await TestHelper.WaitUntilPlayerOrderIsDecided();
                TestHelper.AssertOverlordName();

                await TestHelper.ClickGenericButton("Button_Settings");
                await TestHelper.ClickGenericButton("Button_QuitToMainMenu");
                await TestHelper.RespondToYesNoOverlay(true);

                await TestHelper.LetsThink();

                TestHelper.TestEndHandler();
            });
        }

        [UnityTest]
        [Timeout(50 * 1000 * TestHelper.TestTimeScale)]
        public IEnumerator Test_A4_MatchmakeWaitForOurTurnAndQuit()
        {
            return AsyncTest(async () =>
            {
                TestHelper.SetTestName("PvP - Matchmake, Wait for Our Turn and Quit");

                await TestHelper.ClickGenericButton("Button_Play");

                await TestHelper.AssertIfWentDirectlyToTutorial(
                    TestHelper.GoBackToMainAndPressPlay);

                await TestHelper.AssertCurrentPageName("PlaySelectionPage");
                await TestHelper.ClickGenericButton("Button_PvPMode");
                await TestHelper.AssertCurrentPageName("PvPSelectionPage");
                await TestHelper.ClickGenericButton("Button_CasualType");
                await TestHelper.AssertCurrentPageName("HordeSelectionPage");

                int selectedHordeIndex = 0;

                await TestHelper.SelectAHordeByIndex(selectedHordeIndex);
                TestHelper.RecordExpectedOverlordName(selectedHordeIndex);
                TestHelper.SetPvPTags(new[]
                {
                    "pvpTest"
                });

                await TestHelper.LetsThink();

                await TestHelper.ClickGenericButton("Button_Battle");
                await TestHelper.AssertCurrentPageName("GameplayPage");
                await TestHelper.WaitUntilPlayerOrderIsDecided();
                await TestHelper.AssertMulliganPopupCameUp(
                    () => TestHelper.ClickGenericButton("Button_Keep"),
                    null);
                await TestHelper.WaitUntilOurFirstTurn();
                await TestHelper.ClickGenericButton("Button_Settings");
                await TestHelper.ClickGenericButton("Button_QuitToMainMenu");
                await TestHelper.RespondToYesNoOverlay(true);

                TestHelper.TestEndHandler();
            });
        }

        [UnityTest]
        [Timeout(500000)]
        public IEnumerator Test_A5_MatchmakeMakeOneMoveAndQuit()
        {
            return AsyncTest(async () =>
            {
                TestHelper.SetTestName("PvP - Matchmake, Make One Move And Quit");

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
                TestHelper.SetPvPTags(new[]
                {
                    "pvpTest"
                });

                await TestHelper.LetsThink();

                await TestHelper.MainMenuTransition("Button_Battle");

                await TestHelper.CreateAndConnectOpponentDebugClient();
                TestHelper.SetupOpponentDebugClientToEndTurns();
                await TestHelper.MatchmakeOpponentDebugClient();

                await TestHelper.PlayAMatch(1);
                await TestHelper.ClickGenericButton("Button_Settings");
                await TestHelper.ClickGenericButton("Button_QuitToMainMenu");
                await TestHelper.RespondToYesNoOverlay(true);

                TestHelper.TestEndHandler();
            });
        }

        [UnityTest]
        [Timeout(300 * 1000 * TestHelper.TestTimeScale)]
        public IEnumerator Test_A6_MatchmakeAndPlay()
        {
            return AsyncTest(async () =>
            {
                TestHelper.SetTestName("PvP - Matchmaking And Play");
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
                TestHelper.SetPvPTags(new[]
                {
                    "pvpTest"
                });

                await TestHelper.LetsThink();

                await TestHelper.MainMenuTransition("Button_Battle");

                await TestHelper.CreateAndConnectOpponentDebugClient();
                TestHelper.SetupOpponentDebugClientToEndTurns();
                await TestHelper.MatchmakeOpponentDebugClient();

                await TestHelper.PlayAMatch();

                TestHelper.TestEndHandler();
            });
        }

        [UnityTest]
        [Timeout(50 * 1000 * TestHelper.TestTimeScale)]
        public IEnumerator Test_A7_MatchmakingCancelAndMatchmake()
        {
            return AsyncTest(async () =>
            {
                TestHelper.SetTestName("PvP - Create a Horde and save");
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

                #region Matchmaking Cancel

                TestHelper.SetPvPTags(new[]
                {
                    "pvpTestNoOpponentCancel"
                });

                await TestHelper.LetsThink();
                await TestHelper.MainMenuTransition("Button_Battle");

                await TestHelper.LetsThink();
                await TestHelper.LetsThink();
                await TestHelper.LetsThink();

                await TestHelper.ClickGenericButton("Button_Cancel");

                #endregion

                await TestHelper.LetsThink();
                await TestHelper.LetsThink();

                #region Matchmake and Quit

                TestHelper.SetPvPTags(new[]
                {
                    "pvpTest"
                });

                await TestHelper.LetsThink();

                await TestHelper.MainMenuTransition("Button_Battle");
                await TestHelper.AssertCurrentPageName("GameplayPage");
                await TestHelper.WaitUntilPlayerOrderIsDecided();
                TestHelper.AssertOverlordName();
                await TestHelper.ClickGenericButton("Button_Settings");

                await TestHelper.LetsThink();

                await TestHelper.ClickGenericButton("Button_QuitToMainMenu");

                await TestHelper.LetsThink();

                await TestHelper.RespondToYesNoOverlay(true);

                await TestHelper.LetsThink();

                #endregion

                TestHelper.TestEndHandler();
            });
        }
    }
}
