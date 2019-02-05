using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Common;
using UnityEngine;

namespace Loom.ZombieBattleground.Test
{
    public static class PvPTestUtility
    {
        private static readonly TestHelper TestHelper = TestHelper.Instance;

        public static async Task GenericPvPTest(
            PvpTestContext pvpTestContext,
            IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns,
            Action validateEndStateAction,
            bool enableReverseMatch = true)
        {
            Func<Task> callTest = async () =>
            {
                await GenericPvPTest(
                    turns,
                    () =>
                    {
                        bool player1HasFirstTurn = pvpTestContext.IsReversed ?
                            !pvpTestContext.Player1HasFirstTurn :
                            pvpTestContext.Player1HasFirstTurn;
                        TestHelper.DebugCheats.ForceFirstTurnUserId =
                            player1HasFirstTurn ?
                                TestHelper.BackendDataControlMediator.UserDataModel.UserId :
                                TestHelper.GetOpponentDebugClient().UserDataModel.UserId;
                        TestHelper.DebugCheats.UseCustomDeck = true;
                        TestHelper.DebugCheats.CustomDeck = pvpTestContext.IsReversed ? pvpTestContext.Player2Deck : pvpTestContext.Player1Deck;
                        TestHelper.DebugCheats.DisableDeckShuffle = true;
                        TestHelper.DebugCheats.IgnoreGooRequirements = true;
                    },
                    cheats =>
                    {
                        cheats.UseCustomDeck = true;
                        cheats.CustomDeck = pvpTestContext.IsReversed ? pvpTestContext.Player1Deck : pvpTestContext.Player2Deck;
                    },
                    validateEndStateAction
                );
            };

            pvpTestContext.IsReversed = false;
            await callTest();

            if (enableReverseMatch)
            {
                Debug.Log("Starting reversed Pvp test");
                pvpTestContext.IsReversed = true;
                await callTest();
            }
        }

        private static async Task GenericPvPTest(
            IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns,
            Action setupAction,
            Action<DebugCheatsConfiguration> modifyOpponentDebugCheats,
            Action validateEndStateAction)
        {
            await TestHelper.CreateAndConnectOpponentDebugClient();
            setupAction?.Invoke();

            await StartOnlineMatch(createOpponent: false);

            MatchScenarioPlayer matchScenarioPlayer = new MatchScenarioPlayer(TestHelper, turns);
            await TestHelper.MatchmakeOpponentDebugClient(modifyOpponentDebugCheats);
            await TestHelper.WaitUntilPlayerOrderIsDecided();

            await matchScenarioPlayer.Play();
            validateEndStateAction?.Invoke();

            await TestHelper.GoBackToMainScreen();
        }

        public static async Task StartOnlineMatch(int selectedHordeIndex = 0, bool createOpponent = true, IList<string> tags = null)
        {
            await TestHelper.MainMenuTransition("Button_Play");
            await TestHelper.AssertIfWentDirectlyToTutorial(TestHelper.GoBackToMainAndPressPlay);

            await TestHelper.AssertCurrentPageName(Enumerators.AppState.PlaySelection);
            await TestHelper.MainMenuTransition("Button_PvPMode");
            await TestHelper.AssertCurrentPageName(Enumerators.AppState.PvPSelection);
            await TestHelper.MainMenuTransition("Button_CasualType");
            await TestHelper.AssertCurrentPageName(Enumerators.AppState.HordeSelection);

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

        public static WorkingCard GetCardOnBoard(Player player, string name)
        {
            WorkingCard workingCard =
                player
                .BoardCards
                .Select(boardCard => boardCard.Model.Card)
                .Concat(player.CardsOnBoard)
                .FirstOrDefault(card => CardNameEqual(name, card));

            if (workingCard == null)
            {
                throw new Exception($"No '{name}' cards found on board for player {player}");
            }

            return workingCard;
        }

        public static WorkingCard GetCardInHand(Player player, string name)
        {
            WorkingCard workingCard =
                player
                    .CardsInHand
                    .FirstOrDefault(card => CardNameEqual(name, card));

            if (workingCard == null)
            {
                throw new Exception($"No '{name}' cards found in hand of player {player}");
            }

            return workingCard;
        }

        public static bool CardNameEqual(string name1, string name2)
        {
            return String.Equals(name1, name2, StringComparison.InvariantCultureIgnoreCase);
        }

        public static bool CardNameEqual(string name, WorkingCard card)
        {
            return CardNameEqual(name, card.LibraryCard.Name);
        }
    }
}
