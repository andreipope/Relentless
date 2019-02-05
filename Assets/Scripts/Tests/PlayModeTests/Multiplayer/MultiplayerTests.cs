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
        public IEnumerator Slab()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = new Deck(
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

                Deck opponentDeck = new Deck(
                    0,
                    0,
                    "test deck2",
                    new List<DeckCardData>
                    {
                        new DeckCardData("Slab", 30)
                    },
                    Enumerators.OverlordSkill.NONE,
                    Enumerators.OverlordSkill.NONE
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck) {
                    Player1HasFirstTurn = true
                };

                InstanceId playerSlabId = pvpTestContext.GetCardInstanceIdByIndex(playerDeck, 0);
                InstanceId opponentSlabId = pvpTestContext.GetCardInstanceIdByIndex(opponentDeck, 0);
                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                   {
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
                       player => player.CardAttack(playerSlabId, pvpTestContext.GetOpponentPlayer().InstanceId),
                       opponent => opponent.CardAttack(opponentSlabId, pvpTestContext.GetCurrentPlayer().InstanceId),
                       player => player.CardAttack(playerSlabId, pvpTestContext.GetOpponentPlayer().InstanceId),
                       opponent => opponent.CardAttack(opponentSlabId, pvpTestContext.GetCurrentPlayer().InstanceId),
                       player => player.CardAttack(playerSlabId, pvpTestContext.GetOpponentPlayer().InstanceId),
                       opponent => {},
                       player => player.CardAttack(playerSlabId, pvpTestContext.GetOpponentPlayer().InstanceId),
                       opponent => {},
                       player => player.CardAttack(playerSlabId, pvpTestContext.GetOpponentPlayer().InstanceId),
                       opponent => {},
                       player => player.CardAttack(playerSlabId, pvpTestContext.GetOpponentPlayer().InstanceId),
                       opponent => {},
                       player => player.CardAttack(playerSlabId, pvpTestContext.GetOpponentPlayer().InstanceId),
                       opponent => {},
                       player => player.CardAttack(playerSlabId, pvpTestContext.GetOpponentPlayer().InstanceId),
                       opponent => {},
                       player => player.CardAttack(playerSlabId, pvpTestContext.GetOpponentPlayer().InstanceId),
                       opponent => {},
                       player => player.CardAttack(playerSlabId, pvpTestContext.GetOpponentPlayer().InstanceId),
                   };

                await PvPTestUtility.GenericPvPTest(
                    pvpTestContext,
                    turns,
                    () =>
                    {
                        // FIXME: references to the players are nulled immediately after the game ends,
                        // so we can't assert the state at that moment?
                        //Assert.AreEqual(0, pvpTestContext.GetOpponentPlayer().Defense);
                    }
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

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }

        [UnityTest]
        [Timeout(150 * 1000 * TestHelper.TestTimeScale)]
        public IEnumerator CorrectCardDraw()
        {
            return AsyncTest(async () =>
            {
                await PvPTestUtility.StartOnlineMatch();
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
    }
}
