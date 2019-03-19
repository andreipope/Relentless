using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using log4net;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using NUnit.Framework;
using UnityEngine;

namespace Loom.ZombieBattleground.Test
{
    public static class PvPTestUtility
    {
        private static readonly ILog Log = Logging.GetLog(nameof(PvPTestUtility));

        private static TestHelper TestHelper => TestHelper.Instance;

        public static async Task GenericPvPTest(
            PvpTestContext pvpTestContext,
            IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns,
            Action validateEndStateAction,
            bool enableReverseMatch = true,
            bool enableBackendGameLogicMatch = false,
            bool enableClientGameLogicMatch = true,
            bool onlyReverseMatch = false,
            bool ignoreGooRequirements = true
            )
        {
            void LogTestMode()
            {
                Log.Info($"= RUNNING INTEGRATION TEST [{TestContext.CurrentTestExecutionContext.CurrentTest.Name}] Reverse: {pvpTestContext.IsReversed}, UseBackendLogic: {pvpTestContext.UseBackendLogic}");
            }

            async Task ExecuteTest()
            {
                LogTestMode();
                await GenericPvPTest(
                    turns,
                    pvpTestContext.Player1Deck,
                    () =>
                    {
                        bool player1HasFirstTurn = pvpTestContext.IsReversed ?
                            !pvpTestContext.Player1HasFirstTurn :
                            pvpTestContext.Player1HasFirstTurn;
                        TestHelper.DebugCheats.ForceFirstTurnUserId = player1HasFirstTurn ?
                            TestHelper.BackendDataControlMediator.UserDataModel.UserId :
                            TestHelper.GetOpponentDebugClient().UserDataModel.UserId;
                        TestHelper.DebugCheats.UseCustomDeck = true;
                        TestHelper.DebugCheats.CustomDeck = pvpTestContext.IsReversed ? pvpTestContext.Player2Deck : pvpTestContext.Player1Deck;
                        TestHelper.DebugCheats.DisableDeckShuffle = true;
                        TestHelper.DebugCheats.IgnoreGooRequirements = ignoreGooRequirements;
                        TestHelper.DebugCheats.CustomRandomSeed = 1337;
                        GameClient.Get<IPvPManager>().UseBackendGameLogic = pvpTestContext.UseBackendLogic;
                    },
                    cheats =>
                    {
                        cheats.UseCustomDeck = true;
                        cheats.CustomDeck = pvpTestContext.IsReversed ? pvpTestContext.Player1Deck : pvpTestContext.Player2Deck;
                        },
                    validateEndStateAction);
            }

            async Task ExecuteTestWithReverse()
            {
                if (!onlyReverseMatch)
                {
                    pvpTestContext.IsReversed = false;
                    await ExecuteTest();
                }

                if (enableReverseMatch)
                {
                    pvpTestContext.IsReversed = true;
                    await ExecuteTest();
                }
            }

#if !ENABLE_BACKEND_INTEGRATION_TESTS
            enableBackendGameLogicMatch = false;
#endif
            if (!enableClientGameLogicMatch && !enableBackendGameLogicMatch)
                throw new Exception("At least one tests must be run");

            if (!enableReverseMatch && onlyReverseMatch)
                throw new Exception("!enableReverseMatch && onlyReverseMatch");

            if (enableClientGameLogicMatch)
            {
                pvpTestContext.UseBackendLogic = false;
                await ExecuteTestWithReverse();
            }

            if (enableBackendGameLogicMatch)
            {
                pvpTestContext.UseBackendLogic = true;
                await ExecuteTestWithReverse();
            }
        }

        private static async Task GenericPvPTest(
            IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns,
            Deck deck,
            Action setupAction,
            Action<DebugCheatsConfiguration> modifyOpponentDebugCheats,
            Action validateEndStateAction)
        {
            MatchScenarioPlayer matchScenarioPlayer = null;

            bool canceled = false;
            await TestHelper.CreateAndConnectOpponentDebugClient(
                async exception =>
                {
                    await GameClient.Get<IPvPManager>().StopMatchmaking();
                    matchScenarioPlayer?.AbortNextMoves();
                    canceled = true;
                }
                );
            setupAction?.Invoke();

            await StartOnlineMatch(null, createOpponent: false);

            GameClient.Get<IUIManager>().GetPage<GameplayPage>().CurrentDeckId = (int) deck.Id;
            GameClient.Get<IGameplayManager>().CurrentPlayerDeck = deck;
            await TestHelper.MainMenuTransition("Button_Battle");
            GameClient.Get<IPvPManager>().MatchMakingFlowController.ActionWaitingTime = 1;

            await TestHelper.MatchmakeOpponentDebugClient(modifyOpponentDebugCheats);

            Assert.IsFalse(canceled, "canceled");
            await TestHelper.WaitUntilPlayerOrderIsDecided();
            Assert.IsFalse(canceled, "canceled");

            GameClient.Get<IGameplayManager>().OpponentHasDoneMulligan = true;

            using (matchScenarioPlayer = new MatchScenarioPlayer(TestHelper, turns))
            {
                await matchScenarioPlayer.Play();
            }

            validateEndStateAction?.Invoke();
            await TestHelper.GoBackToMainScreen();
        }

        public static async Task StartOnlineMatch(IReadOnlyList<string> tags = null, bool createOpponent = true)
        {
            await TestHelper.MainMenuTransition("Panel_Battle_Mode");
            await TestHelper.MainMenuTransition("Button_PvPMode");

            if (tags == null)
            {
                tags = new List<string>
                {
                    "onlineTest",
                    TestHelper.GetTestName(),
                    Guid.NewGuid().ToString()
                };
            }
            TestHelper.SetPvPTags(tags);
            TestHelper.DebugCheats.Enabled = true;
            TestHelper.DebugCheats.CustomRandomSeed = 0;

            await TestHelper.LetsThink();

            if (createOpponent)
            {
                await TestHelper.CreateAndConnectOpponentDebugClient();
            }
        }

        public static BoardUnitModel GetCardOnBoard(Player player, string name)
        {
            BoardUnitModel boardUnitModel =
                player
                .CardsOnBoard
                .Concat(player.CardsOnBoard)
                .FirstOrDefault(card => CardNameEqual(name, card.Card.Prototype.Name));

            if (boardUnitModel == null)
            {
                throw new Exception($"No '{name}' cards found on board for player {player}");
            }

            return boardUnitModel;
        }

        public static BoardUnitModel GetCardInHand(Player player, string name)
        {
            BoardUnitModel boardUnitModel =
                player
                    .CardsInHand
                    .FirstOrDefault(card => CardNameEqual(name, card.Card.Prototype.Name));

            if (boardUnitModel == null)
            {
                throw new Exception($"No '{name}' cards found in hand of player {player}");
            }

            return boardUnitModel;
        }

        public static bool CardNameEqual(string name1, string name2)
        {
            return String.Equals(name1, name2, StringComparison.InvariantCultureIgnoreCase);
        }

        public static Deck GetDeckWithCards(string name, int heroId = 0, params DeckCardData[] cards)
        {
            Deck deck = new Deck(
                 0,
                 heroId,
                 name,
                 cards.ToList(),
                 Enumerators.OverlordSkill.NONE,
                 Enumerators.OverlordSkill.NONE
             );

            return deck;
        }

        public static Deck GetDeckWithCards(string name,
                                    int heroId = 0,
                                    Enumerators.OverlordSkill primaryskill = Enumerators.OverlordSkill.NONE,
                                    Enumerators.OverlordSkill secondarySkill = Enumerators.OverlordSkill.NONE,
                                    params DeckCardData[] cards)
        {
            Deck deck = GetDeckWithCards(name, heroId, cards);
            deck.PrimarySkill = primaryskill;
            deck.SecondarySkill = secondarySkill;

            return deck;
        }
    }
}
