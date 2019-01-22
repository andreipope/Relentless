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
        public IEnumerator PlayScenarioGame1()
        {
            return AsyncTest(async () =>
            {
                Deck deck = new Deck(
                    0,
                    0,
                    "test deck",
                    new List<DeckCardData>
                    {
                        new DeckCardData("Slab", 30)
                    },
                    Enumerators.OverlordSkill.NONE,
                    Enumerators.OverlordSkill.NONE
                );

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
                       player => player.CardPlay(new InstanceId(36), 0),
                       opponent => opponent.CardPlay(new InstanceId(2), 0),
                       player => player.CardAttack(new InstanceId(36), Enumerators.AffectObjectType.Player, TestHelper.GetOpponentPlayer().InstanceId),
                       opponent => opponent.CardAttack(new InstanceId(2), Enumerators.AffectObjectType.Player, TestHelper.GetCurrentPlayer().InstanceId),
                       player => player.CardAttack(new InstanceId(36), Enumerators.AffectObjectType.Player, TestHelper.GetOpponentPlayer().InstanceId),
                       opponent => opponent.CardAttack(new InstanceId(2), Enumerators.AffectObjectType.Player, TestHelper.GetCurrentPlayer().InstanceId),
                       player => player.CardAttack(new InstanceId(36), Enumerators.AffectObjectType.Player,TestHelper.GetOpponentPlayer().InstanceId),
                       opponent => {},
                       player => player.CardAttack(new InstanceId(36), Enumerators.AffectObjectType.Player, TestHelper.GetOpponentPlayer().InstanceId),
                       opponent => {},
                       player => player.CardAttack(new InstanceId(36), Enumerators.AffectObjectType.Player, TestHelper.GetOpponentPlayer().InstanceId),
                       opponent => {},
                       player => player.CardAttack(new InstanceId(36), Enumerators.AffectObjectType.Player, TestHelper.GetOpponentPlayer().InstanceId),
                       opponent => {},
                       player => player.CardAttack(new InstanceId(36), Enumerators.AffectObjectType.Player, TestHelper.GetOpponentPlayer().InstanceId),
                       opponent => {},
                       player => player.CardAttack(new InstanceId(36), Enumerators.AffectObjectType.Player, TestHelper.GetOpponentPlayer().InstanceId),
                       opponent => {},
                       player => player.CardAttack(new InstanceId(36), Enumerators.AffectObjectType.Player, TestHelper.GetOpponentPlayer().InstanceId),
                       opponent => {},
                       player => player.CardAttack(new InstanceId(36), Enumerators.AffectObjectType.Player, TestHelper.GetOpponentPlayer().InstanceId),
                   };

                await GenericPvPTest(
                    turns,
                    () =>
                    {
                        TestHelper.DebugCheats.ForceFirstTurnUserId = TestHelper.GetOpponentDebugClient().UserDataModel.UserId;
                        TestHelper.DebugCheats.CustomDeck = deck;
                        TestHelper.DebugCheats.DisableDeckShuffle = true;
                    },
                    cheats => cheats.CustomDeck = deck
                );

                await TestHelper.ClickGenericButton("Button_Continue");
                await TestHelper.AssertCurrentPageName("HordeSelectionPage");
            });
        }

        [UnityTest]
        [Timeout(150 * 1000 * TestHelper.TestTimeScale)]
        public IEnumerator Cynderman()
        {
            return AsyncTest(async () =>
            {
                PvPTestHelper pvpTestHelper = new PvPTestHelper();

                Deck opponentDeck = new Deck(
                    0,
                    0,
                    "test deck",
                    new List<DeckCardData>
                    {
                        new DeckCardData("Cynderman", 2),
                        new DeckCardData("Slab", 2)
                    },
                    Enumerators.OverlordSkill.NONE,
                    Enumerators.OverlordSkill.NONE
                );

                Deck localDeck = new Deck(
                    0,
                    0,
                    "test deck2",
                    new List<DeckCardData>
                    {
                        new DeckCardData("Cynderman", 2),
                        new DeckCardData("Slab", 2)
                    },
                    Enumerators.OverlordSkill.NONE,
                    Enumerators.OverlordSkill.NONE
                );

                InstanceId slabId = new InstanceId(4);
                InstanceId cyndermanId = new InstanceId(6);
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
                       player => {},
                       opponent => opponent.CardPlay(slabId, 0),
                       player =>
                       {
                           //player.CardAbilityUsed();
                           player.CardPlay(new InstanceId(32), 0);
                       },
                       // slab
                       opponent => opponent.CardPlay(new InstanceId(5), 0),
                       // znowman attacks slab
                       player => player.CardAttack(new InstanceId(32), Enumerators.AffectObjectType.Character, new InstanceId(5)),
                   };

                await GenericPvPTest(
                    turns,
                    () =>
                    {
                        TestHelper.DebugCheats.ForceFirstTurnUserId = TestHelper.GetOpponentDebugClient().UserDataModel.UserId;
                        TestHelper.DebugCheats.CustomDeck = localDeck;
                        TestHelper.DebugCheats.DisableDeckShuffle = true;
                    },
                    cheats => cheats.CustomDeck = opponentDeck
                );

                await Task.Delay(5000);

                await TestHelper.ClickGenericButton("Button_Continue");
                await TestHelper.AssertCurrentPageName("HordeSelectionPage");
            });
        }

        [UnityTest]
        [Timeout(150 * 1000 * TestHelper.TestTimeScale)]
        public IEnumerator CorrectCardDraw()
        {
            return AsyncTest(async () =>
            {
                await StartOnlineMatch();
                TestHelper.DebugCheats.ForceFirstTurnUserId = TestHelper.BackendDataControlMediator.UserDataModel.UserId;

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                    player => {},
                    opponent => {},
                    player => {},
                };

                MatchScenarioPlayer matchScenarioPlayer = new MatchScenarioPlayer(TestHelper, turns);
                await TestHelper.MatchmakeOpponentDebugClient();

                await matchScenarioPlayer.Play();

                await TestHelper.ClickGenericButton("Button_Settings");
                await TestHelper.ClickGenericButton("Button_QuitToMainMenu");
                await TestHelper.RespondToYesNoOverlay(true);
            });
        }

        [UnityTest]
        [Timeout(50 * 1000 * TestHelper.TestTimeScale)]
        public IEnumerator MatchmakingCancel()
        {
            return AsyncTest(async () =>
            {
                await StartOnlineMatch(createOpponent: false);

                await TestHelper.LetsThink(5, true);

                await TestHelper.ClickGenericButton("Button_Cancel");

                await TestHelper.AssertPvPStartedOrMatchmakingFailed(
                    () =>
                    {
                        Assert.Fail("It shouldn't have been matched.");
                        return Task.CompletedTask;
                    },
                    () => TestHelper.ClickGenericButton ("Button_Cancel"));
            });
        }

        [UnityTest]
        [Timeout(50 * 1000 * TestHelper.TestTimeScale)]
        public IEnumerator MatchmakingTimeout()
        {
            return AsyncTest(async () =>
            {
                await StartOnlineMatch(createOpponent: false);

                await TestHelper.AssertPvPStartedOrMatchmakingFailed(
                    () =>
                    {
                        Assert.Fail("It shouldn't have been matched.");
                        return Task.CompletedTask;
                    },
                    () => TestHelper.ClickGenericButton("Button_Cancel"));
            });
        }

        [UnityTest]
        [Timeout(50 * 1000 * TestHelper.TestTimeScale)]
        public IEnumerator MatchmakeAndQuit()
        {
            return AsyncTest(async () =>
            {
                await StartOnlineMatch();
                await TestHelper.AssertPvPStartedOrMatchmakingFailed(
                    null,
                    () =>
                    {
                        Assert.Fail("Didn't match, so couldn't check.");
                        return Task.CompletedTask;
                    });

                await TestHelper.MatchmakeOpponentDebugClient();

                await TestHelper.AssertCurrentPageName("GameplayPage");
                await TestHelper.WaitUntilPlayerOrderIsDecided();
                TestHelper.AssertOverlordName();

                await TestHelper.ClickGenericButton("Button_Settings");
                await TestHelper.ClickGenericButton("Button_QuitToMainMenu");
                await TestHelper.RespondToYesNoOverlay(true);
            });
        }

        [UnityTest]
        [Timeout(50 * 1000 * TestHelper.TestTimeScale)]
        public IEnumerator MatchmakeWaitForOurTurnAndQuit()
        {
            return AsyncTest(async () =>
            {
                await StartOnlineMatch();

                await TestHelper.MatchmakeOpponentDebugClient();

                await TestHelper.AssertCurrentPageName("GameplayPage");
                await TestHelper.WaitUntilPlayerOrderIsDecided();
                await TestHelper.AssertMulliganPopupCameUp(
                    () => TestHelper.ClickGenericButton("Button_Keep"),
                    null);
                await TestHelper.WaitUntilOurFirstTurn();
                await TestHelper.ClickGenericButton("Button_Settings");
                await TestHelper.ClickGenericButton("Button_QuitToMainMenu");
                await TestHelper.RespondToYesNoOverlay(true);
            });
        }

        [UnityTest]
        [Timeout(500000)]
        public IEnumerator MatchmakeMakeOneMoveAndQuit()
        {
            return AsyncTest(async () =>
            {
                await StartOnlineMatch();
                await TestHelper.AssertPvPStartedOrMatchmakingFailed(
                    null,
                    () =>
                    {
                        Assert.Fail("Didn't match, so couldn't check.");
                        return Task.CompletedTask;
                    });

                await TestHelper.MatchmakeOpponentDebugClient();

                await TestHelper.PlayAMatch(1);
                await TestHelper.ClickGenericButton("Button_Settings");
                await TestHelper.ClickGenericButton("Button_QuitToMainMenu");
                await TestHelper.RespondToYesNoOverlay(true);
            });
        }

        [UnityTest]
        [Timeout(300 * 1000 * TestHelper.TestTimeScale)]
        public IEnumerator MatchmakeAndPlay()
        {
            return AsyncTest(async () =>
            {
                await StartOnlineMatch();
                await TestHelper.AssertPvPStartedOrMatchmakingFailed(
                    null,
                    () =>
                    {
                        Assert.Fail("Didn't match, so couldn't check.");
                        return Task.CompletedTask;
                    });

                TestHelper.SetupOpponentDebugClientToEndTurns();
                await TestHelper.MatchmakeOpponentDebugClient();

                await TestHelper.PlayAMatch();
            });
        }

        [UnityTest]
        [Timeout(50 * 1000 * TestHelper.TestTimeScale)]
        public IEnumerator MatchmakingCancelAndMatchmake()
        {
            return AsyncTest(async () =>
            {
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

                // Matchmaking Cancel

                TestHelper.SetPvPTags(new[]
                {
                    "pvpTestNoOpponentCancel"
                });

                await TestHelper.LetsThink();
                await TestHelper.MainMenuTransition("Button_Battle");
                await TestHelper.AssertPvPStartedOrMatchmakingFailed(
                    () =>
                    {
                        Assert.Fail("It shouldn't have been matched.");
                        return Task.CompletedTask;
                    },
                    () => TestHelper.ClickGenericButton ("Button_Cancel"));

                await TestHelper.LetsThink();
                await TestHelper.LetsThink();
                await TestHelper.LetsThink();

                await TestHelper.ClickGenericButton("Button_Cancel");

                await TestHelper.LetsThink();
                await TestHelper.LetsThink();

                // Matchmake and Quit

                TestHelper.SetPvPTags(new[]
                {
                    "pvpTest"
                });

                await TestHelper.LetsThink();

                await TestHelper.MainMenuTransition("Button_Battle");
                await TestHelper.AssertPvPStartedOrMatchmakingFailed(
                    null,
                    () =>
                    {
                        Assert.Fail("Didn't match, so couldn't check.");
                        return Task.CompletedTask;
                    });

                await TestHelper.AssertCurrentPageName("GameplayPage");
                await TestHelper.WaitUntilPlayerOrderIsDecided();
                TestHelper.AssertOverlordName();
                await TestHelper.ClickGenericButton("Button_Settings");

                await TestHelper.LetsThink();

                await TestHelper.ClickGenericButton("Button_QuitToMainMenu");

                await TestHelper.LetsThink();

                await TestHelper.RespondToYesNoOverlay(true);

                await TestHelper.LetsThink();
            });
        }

        private async Task GenericPvPTest(IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns, Action setupAction, Action<DebugCheatsConfiguration> modifyOpponentDebugCheats)
        {
            await TestHelper.CreateAndConnectOpponentDebugClient();

            setupAction?.Invoke();

            await StartOnlineMatch(createOpponent: false);

            MatchScenarioPlayer matchScenarioPlayer = new MatchScenarioPlayer(TestHelper, turns);
            await TestHelper.MatchmakeOpponentDebugClient(modifyOpponentDebugCheats);
            await TestHelper.WaitUntilPlayerOrderIsDecided();

            await matchScenarioPlayer.Play();
        }

        private async Task StartOnlineMatch(int selectedHordeIndex = 0, bool createOpponent = true, IList<string> tags = null)
        {
            await TestHelper.HandleLogin();

            await TestHelper.MainMenuTransition("Button_Play");
            await TestHelper.AssertIfWentDirectlyToTutorial(TestHelper.GoBackToMainAndPressPlay);

            await TestHelper.AssertCurrentPageName("PlaySelectionPage");
            await TestHelper.MainMenuTransition("Button_PvPMode");
            await TestHelper.AssertCurrentPageName("PvPSelectionPage");
            await TestHelper.MainMenuTransition("Button_CasualType");
            await TestHelper.AssertCurrentPageName("HordeSelectionPage");

            await TestHelper.SelectAHordeByIndex(selectedHordeIndex);
            TestHelper.RecordExpectedOverlordName(selectedHordeIndex);

            if (tags == null)
            {
                tags = new List<string>();
            }

            tags.Insert(0, "pvpTest");
            tags.Insert(1, TestHelper.GetTestName());

            TestHelper.SetPvPTags(tags);
            TestHelper.DebugCheats.Enabled = true;
            TestHelper.DebugCheats.CustomRandomSeed = 0;

            await TestHelper.LetsThink();

            await TestHelper.MainMenuTransition("Button_Battle");

            if (createOpponent)
            {
                await TestHelper.CreateAndConnectOpponentDebugClient();
            }
        }
    }
}
