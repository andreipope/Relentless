using System;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using UnityEngine;
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

                InstanceId playerSlabId = new InstanceId(36);
                InstanceId opponentSlabId = new InstanceId(2);
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
                       player => player.CardPlay(playerSlabId, ItemPosition.Start),
                       opponent => opponent.CardPlay(opponentSlabId, ItemPosition.Start),
                       player => player.CardAttack(playerSlabId, TestHelper.GetOpponentPlayer().InstanceId),
                       opponent => opponent.CardAttack(opponentSlabId, TestHelper.GetCurrentPlayer().InstanceId),
                       player => player.CardAttack(playerSlabId, TestHelper.GetOpponentPlayer().InstanceId),
                       opponent => opponent.CardAttack(opponentSlabId, TestHelper.GetCurrentPlayer().InstanceId),
                       player => player.CardAttack(playerSlabId, TestHelper.GetOpponentPlayer().InstanceId),
                       opponent => {},
                       player => player.CardAttack(playerSlabId, TestHelper.GetOpponentPlayer().InstanceId),
                       opponent => {},
                       player => player.CardAttack(playerSlabId, TestHelper.GetOpponentPlayer().InstanceId),
                       opponent => {},
                       player => player.CardAttack(playerSlabId, TestHelper.GetOpponentPlayer().InstanceId),
                       opponent => {},
                       player => player.CardAttack(playerSlabId, TestHelper.GetOpponentPlayer().InstanceId),
                       opponent => {},
                       player => player.CardAttack(playerSlabId, TestHelper.GetOpponentPlayer().InstanceId),
                       opponent => {},
                       player => player.CardAttack(playerSlabId, TestHelper.GetOpponentPlayer().InstanceId),
                       opponent => {},
                       player => player.CardAttack(playerSlabId, TestHelper.GetOpponentPlayer().InstanceId),
                   };

                await GenericPvPTest(
                    turns,
                    () =>
                    {
                        TestHelper.DebugCheats.ForceFirstTurnUserId = TestHelper.GetOpponentDebugClient().UserDataModel.UserId;
                        TestHelper.DebugCheats.UseCustomDeck = true;
                        TestHelper.DebugCheats.CustomDeck = deck;
                        TestHelper.DebugCheats.DisableDeckShuffle = true;
                    },
                    cheats =>
                    {
                        cheats.UseCustomDeck = true;
                        cheats.CustomDeck = deck;
                    },
                    null
                );
            });
        }

        [UnityTest]
        [Timeout(150 * 1000 * TestHelper.TestTimeScale)]
        public IEnumerator Cynderman()
        {
            return AsyncTest(async () =>
            {
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

                Deck playerDeck = new Deck(
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

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck) {
                    Player1HasFirstTurn = true
                };

                InstanceId playerSlabId = pvpTestContext.GetCardInstanceIdByIndex(playerDeck, 2);
                InstanceId opponentSlabId = pvpTestContext.GetCardInstanceIdByIndex(opponentDeck, 2);
                InstanceId playerCyndermanId = pvpTestContext.GetCardInstanceIdByIndex(playerDeck, 0);
                InstanceId opponentCyndermanId = pvpTestContext.GetCardInstanceIdByIndex(opponentDeck, 0);
                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                   {
                       player => {},
                       opponent => {},
                       player => {},
                       opponent => {},
                       player => player.CardPlay(playerSlabId, ItemPosition.Start),
                       opponent =>
                       {
                           opponent.CardPlay(opponentSlabId, ItemPosition.Start);
                           opponent.CardPlay(opponentCyndermanId, ItemPosition.Start, playerSlabId);
                       },
                       player =>
                       {
                           player.CardPlay(playerCyndermanId, ItemPosition.Start, opponentCyndermanId);
                       },
                   };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(2, ((BoardUnitModel) TestHelper.BattlegroundController.GetBoardObjectById(playerSlabId)).CurrentHp);
                    Assert.AreEqual(2, ((BoardUnitModel) TestHelper.BattlegroundController.GetBoardObjectById(opponentCyndermanId)).CurrentHp);
                };

                await GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }

        private async Task GenericPvPTest(
            PvpTestContext pvpTestContext,
            IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns,
            Action validateEndStateAction)
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

            Debug.Log("Starting reversed Pvp test");
            pvpTestContext.IsReversed = true;
            await callTest();
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
                    player =>
                    {
                        Assert.AreEqual(4, TestHelper.GetCurrentPlayer().CardsInHand.Count);
                        Assert.AreEqual(3, TestHelper.GetOpponentPlayer().CardsInHand.Count);
                    },
                    opponent =>
                    {
                        Assert.AreEqual(4, TestHelper.GetCurrentPlayer().CardsInHand.Count);
                        Assert.AreEqual(5, TestHelper.GetOpponentPlayer().CardsInHand.Count);
                    },
                    player =>
                    {
                        Assert.AreEqual(5, TestHelper.GetCurrentPlayer().CardsInHand.Count);
                        Assert.AreEqual(5, TestHelper.GetOpponentPlayer().CardsInHand.Count);
                    },
                };

                MatchScenarioPlayer matchScenarioPlayer = new MatchScenarioPlayer(TestHelper, turns);
                await TestHelper.MatchmakeOpponentDebugClient();

                await matchScenarioPlayer.Play();
            });
        }

        private async Task GenericPvPTest(
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

        private class PvpTestContext {
            public readonly Deck Player1Deck;
            public readonly Deck Player2Deck;
            public bool Player1HasFirstTurn;
            public bool IsReversed;

            public PvpTestContext(Deck player1Deck, Deck player2Deck) {
                if (player1Deck == null)
                    throw new ArgumentNullException(nameof(player1Deck));

                if (player2Deck == null)
                    throw new ArgumentNullException(nameof(player2Deck));

                if (player1Deck == player2Deck)
                    throw new Exception("player1Deck == player2Deck");

                Player1Deck = player1Deck;
                Player2Deck = player2Deck;
            }

            public InstanceId GetCardIdByName(Deck deck, string name) {
                AssertKnownDeck(deck);

                int count = 0;
                foreach (DeckCardData deckCard in deck.Cards)
                {
                    if (PvPTestUtility.CardNameEqual(name, deckCard.CardName))
                    {
                        if (deckCard.Amount > 1)
                            throw new Exception($"deckCard.Amount > 1 for card {name}");

                        return GetCardInstanceIdByIndex(deck, count);
                    }
                    count += deckCard.Amount;
                }

                throw new Exception($"card with name {name} not found in deck");
            }

            public InstanceId GetCardInstanceIdByIndex(Deck deck, int indexInDeck) {
                return new InstanceId(GetDeckStartingInstanceId(deck).Id + indexInDeck);
            }

            public InstanceId GetDeckStartingInstanceId(Deck deck) {
                AssertKnownDeck(deck);

                bool isPlayer1Deck = deck == Player1Deck;
                isPlayer1Deck = IsReversed ? !isPlayer1Deck : isPlayer1Deck;
                Deck otherDeck = isPlayer1Deck ? Player2Deck : Player1Deck;

                bool condition = Player1HasFirstTurn && isPlayer1Deck || !Player1HasFirstTurn && !isPlayer1Deck;
                condition = IsReversed ? !condition : condition;
                if (condition)
                {
                    return new InstanceId(2);
                } else
                {
                    return new InstanceId(2 + GetTotalCardCount(otherDeck));
                }
            }

            public int GetTotalCardCount(Deck deck) {
                AssertKnownDeck(deck);

                int count = 0;
                foreach (DeckCardData deckCard in deck.Cards)
                {
                    count += deckCard.Amount;
                }

                return count;
            }

            private void AssertKnownDeck(Deck deck) {
                if (deck != Player1Deck && deck != Player2Deck)
                    throw new Exception("deck != Player1Deck && deck != Player2Deck");
            }
        }
    }
}
