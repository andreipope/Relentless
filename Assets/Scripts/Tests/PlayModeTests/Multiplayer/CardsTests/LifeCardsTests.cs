using System;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using UnityEngine.TestTools;
using System.Linq;

namespace Loom.ZombieBattleground.Test.MultiplayerTests
{
    public class LifeCardsTests : BaseIntegrationTest
    {
        [UnityTest]
        [Timeout(150 * 1000 * TestHelper.TestTimeScale)]
        public IEnumerator Cactuz()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0,
                    new DeckCardData("Cactuz", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    new DeckCardData("Cactuz", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Cactuz", 1);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Cactuz", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                       player => {},
                       opponent => {},
                       player => {},
                       opponent => {},
                       player =>
                       {
                           player.CardPlay(playerCardId, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentCardId, ItemPosition.Start);
                           opponent.CardAbilityUsed(opponentCardId, Enumerators.AbilityType.REANIMATE_UNIT, new List<ParametrizedAbilityInstanceId>());
                           opponent.CardAbilityUsed(opponentCardId, Enumerators.AbilityType.ADD_CARD_BY_NAME_TO_HAND, new List<ParametrizedAbilityInstanceId>());
                       },
                       player => player.CardAttack(playerCardId, opponentCardId),
                       player => {},
                       opponent => {},
                };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(1, TestHelper.BattlegroundController.PlayerBoardCards.Count);
                    Assert.AreEqual(1, TestHelper.BattlegroundController.OpponentBoardCards.Count);
                    Assert.AreEqual(156, (TestHelper.BattlegroundController.PlayerHandCards.Select(card => card.LibraryCard.MouldId)));
                    Assert.AreEqual(156, (TestHelper.BattlegroundController.OpponentHandCards.Select(card => card.WorkingCard.LibraryCard.MouldId)));
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }

        [UnityTest]
        [Timeout(150 * 1000 * TestHelper.TestTimeScale)]
        public IEnumerator Huzk()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0,
                    new DeckCardData("Huzk", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    new DeckCardData("Huzk", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Huzk", 1);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Huzk", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                       player => {},
                       opponent => {},
                       player => {},
                       opponent => {},
                       player =>
                       {
                           player.CardPlay(playerCardId, ItemPosition.Start);
                           player.CardAbilityUsed(playerCardId, Enumerators.AbilityType.REANIMATE_UNIT, new List<ParametrizedAbilityInstanceId>());
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentCardId, ItemPosition.Start);
                           opponent.CardAbilityUsed(opponentCardId, Enumerators.AbilityType.REANIMATE_UNIT, new List<ParametrizedAbilityInstanceId>());
                       },
                       player => player.CardAttack(playerCardId, opponentCardId),
                       player => {},
                       opponent => {},
                };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(1, TestHelper.BattlegroundController.PlayerBoardCards.Count);
                    Assert.AreEqual(1, TestHelper.BattlegroundController.OpponentBoardCards.Count);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }

        [UnityTest]
        [Timeout(150 * 1000 * TestHelper.TestTimeScale)]
        public IEnumerator Yggdrazil()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0,
                    new DeckCardData("Yggdrazil", 1),
                    new DeckCardData("Azuraz", 1),
                    new DeckCardData("MonZoon", 2),
                    new DeckCardData("Pyromaz", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    new DeckCardData("Yggdrazil", 1),
                    new DeckCardData("Azuraz", 1),
                    new DeckCardData("MonZoon", 2),
                    new DeckCardData("Pyromaz", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerYggdrazildId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Yggdrazil", 1);
                InstanceId playerAzurazdId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Azuraz", 1);
                InstanceId playerMonZoonId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "MonZoon", 1);
                InstanceId playerMonZoon2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "MonZoon", 2);
                InstanceId playerPyromazId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Pyromaz", 1);
                InstanceId playerPyromaz2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Pyromaz", 2);
                InstanceId opponentYggdrazildId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Yggdrazil", 1);
                InstanceId opponentAzurazdId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Azuraz", 1);
                InstanceId opponentMonZoonId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "MonZoon", 1);
                InstanceId opponentMonZoon2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "MonZoon", 2);
                InstanceId opponentPyromazId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Pyromaz", 1);
                InstanceId opponentPyromaz2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Pyromaz", 2);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                       player => {},
                       opponent => {},
                       player => {},
                       opponent => {},
                       player =>
                       {
                           player.CardPlay(playerAzurazdId, ItemPosition.Start);
                           player.CardPlay(playerMonZoonId, ItemPosition.Start);
                           player.CardPlay(playerMonZoon2Id, ItemPosition.Start);
                           player.CardPlay(playerPyromazId, ItemPosition.Start);
                           player.CardPlay(playerPyromaz2Id, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentAzurazdId, ItemPosition.Start);
                           opponent.CardPlay(opponentMonZoonId, ItemPosition.Start);
                           opponent.CardPlay(opponentMonZoon2Id, ItemPosition.Start);
                           opponent.CardPlay(opponentPyromazId, ItemPosition.Start);
                           opponent.CardPlay(opponentPyromaz2Id, ItemPosition.Start);
                       },
                       player =>
                       {
                           player.CardAttack(playerAzurazdId, opponentAzurazdId);
                       },
                       opponent => {
                           opponent.CardAttack(opponentMonZoonId, playerMonZoonId);
                           opponent.CardAttack(opponentMonZoon2Id, playerMonZoon2Id);
                       },
                       player => {
                           player.CardAttack(playerPyromazId, opponentPyromazId);
                           player.CardAttack(playerPyromaz2Id, opponentPyromaz2Id);
                       },
                       opponent => {},
                       player => player.CardPlay(playerYggdrazildId, ItemPosition.Start),
                       opponent => {},
                       player => {},
                       opponent => opponent.CardPlay(opponentYggdrazildId, ItemPosition.Start),
                       player => {},
                };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(4, TestHelper.BattlegroundController.PlayerBoardCards.Count);
                    Assert.AreEqual(4, TestHelper.BattlegroundController.OpponentBoardCards.Count);

                    Assert.NotNull(TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerYggdrazildId));
                    Assert.NotNull(TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerAzurazdId));
                    Assert.NotNull(TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerMonZoonId));
                    Assert.NotNull(TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerMonZoon2Id));
                    Assert.IsNull(TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerPyromazId));
                    Assert.IsNull(TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerPyromaz2Id));

                    Assert.NotNull(TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentYggdrazildId));
                    Assert.NotNull(TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentAzurazdId));
                    Assert.NotNull(TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentMonZoonId));
                    Assert.NotNull(TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentMonZoon2Id));
                    Assert.IsNull(TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentPyromazId));
                    Assert.IsNull(TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentPyromaz2Id));
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }

        [UnityTest]
        [Timeout(150 * 1000 * TestHelper.TestTimeScale)]
        public IEnumerator Puffer()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0,
                    new DeckCardData("Puffer", 1),
                    new DeckCardData("Azuraz", 1),
                    new DeckCardData("Bark", 1),
                    new DeckCardData("Pyromaz", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    new DeckCardData("Puffer", 1),
                    new DeckCardData("Azuraz", 1),
                    new DeckCardData("Bark", 1),
                    new DeckCardData("Pyromaz", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerPufferId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Puffer", 1);
                InstanceId playerAzurazdId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Azuraz", 1);
                InstanceId playerBarkId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Bark", 1);
                InstanceId playerPyromazId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Pyromaz", 1);
                InstanceId opponentPufferId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Puffer", 1);
                InstanceId opponentAzurazdId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Azuraz", 1);
                InstanceId opponentBarkId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Bark", 1);
                InstanceId opponentPyromazId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Pyromaz", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                       player => {},
                       opponent => {},
                       player => {},
                       opponent => {},
                       player =>
                       {
                           player.CardPlay(playerAzurazdId, ItemPosition.Start);
                           player.CardPlay(playerBarkId, ItemPosition.Start);
                           player.CardPlay(playerPyromazId, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentAzurazdId, ItemPosition.Start);
                           opponent.CardPlay(opponentBarkId, ItemPosition.Start);
                           opponent.CardPlay(opponentPyromazId, ItemPosition.Start);
                       },
                       player =>
                       {
                           player.CardPlay(playerPufferId, ItemPosition.Start);
                           player.CardAbilityUsed(playerPufferId, Enumerators.AbilityType.CHANGE_STAT_OF_CREATURES_BY_TYPE, new List<ParametrizedAbilityInstanceId>());
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentPufferId, ItemPosition.Start);
                           opponent.CardAbilityUsed(opponentPufferId, Enumerators.AbilityType.CHANGE_STAT_OF_CREATURES_BY_TYPE, new List<ParametrizedAbilityInstanceId>());
                       },
                       player => {},
                       opponent => {},
                };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(2, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerPufferId)).CurrentDamage);
                    Assert.AreEqual(2, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerAzurazdId)).CurrentDamage);
                    Assert.AreEqual(1, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerBarkId)).CurrentDamage);
                    Assert.AreEqual(1, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerPyromazId)).CurrentDamage);
                    Assert.AreEqual(2, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentPufferId)).CurrentDamage);
                    Assert.AreEqual(2, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentAzurazdId)).CurrentDamage);
                    Assert.AreEqual(1, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentBarkId)).CurrentDamage);
                    Assert.AreEqual(1, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentPyromazId)).CurrentDamage);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }

        [UnityTest]
        [Timeout(150 * 1000 * TestHelper.TestTimeScale)]
        public IEnumerator Azuraz()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0,
                    new DeckCardData("Azuraz", 1),
                    new DeckCardData("Tiny", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    new DeckCardData("Azuraz", 1),
                    new DeckCardData("Tiny", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerAzurazId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Azuraz", 1);
                InstanceId playerTinyId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Tiny", 1);
                InstanceId opponentAzurazId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Azuraz", 1);
                InstanceId opponentTinyId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Tiny", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                       player => {},
                       opponent => {},
                       player =>
                       {
                           player.CardPlay(playerAzurazId, ItemPosition.Start);
                           player.CardPlay(playerTinyId, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentAzurazId, ItemPosition.Start);
                           opponent.CardPlay(opponentTinyId, ItemPosition.Start);
                       },
                       player =>
                       {
                           player.CardAttack(playerAzurazId, opponentTinyId);
                       },
                       opponent =>
                       {
                           opponent.CardAttack(opponentAzurazId, playerTinyId);
                       },
                       player => {},
                       opponent => {}
                };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(2, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerTinyId)).CurrentHp);
                    Assert.AreEqual(2, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentTinyId)).CurrentHp);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }

        [UnityTest]
        [Timeout(150 * 1000 * TestHelper.TestTimeScale)]
        public IEnumerator Bloomer()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0,
                    new DeckCardData("Bloomer", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    new DeckCardData("Bloomer", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerBloomer1Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Bloomer", 1);
                InstanceId playerBloomer2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Bloomer", 2);
                InstanceId opponentBloomer1Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Bloomer", 1);
                InstanceId opponentBloomer2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Bloomer", 2);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                       player => {},
                       opponent => {},
                       player =>
                       {
                           player.CardPlay(playerBloomer1Id, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentBloomer1Id, ItemPosition.Start);
                       },
                       player =>
                       {
                           player.CardPlay(playerBloomer2Id, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentBloomer2Id, ItemPosition.Start);
                       },
                       player => {}
                };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(7, TestHelper.BattlegroundController.PlayerHandCards.Count);
                    Assert.AreEqual(7, TestHelper.BattlegroundController.OpponentHandCards.Count);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }

        [UnityTest]
        [Timeout(150 * 1000 * TestHelper.TestTimeScale)]
        public IEnumerator Zap()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 5,
                    new DeckCardData("Zap", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 5,
                    new DeckCardData("Zap", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerZapId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zap", 1);
                InstanceId opponentZapId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zap", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                       player => {},
                       opponent => {},
                       player =>
                       {
                           player.CardPlay(playerZapId, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentZapId, ItemPosition.Start);
                       },
                       player => {}
                };

                Action validateEndState = () =>
                {
                    bool playerHasTaintendGoo = false;
                    bool opponentHasTaintedGoo = false;

                    foreach (BoardCard card in TestHelper.BattlegroundController.PlayerHandCards)
                    {
                        if (card.WorkingCard.LibraryCard.Name == "Tainted Goo") 
                        {
                            playerHasTaintendGoo = true;
                            break;
                        }
                    }

                    foreach (OpponentHandCard card in TestHelper.BattlegroundController.OpponentHandCards)
                    {
                        if (card.WorkingCard.LibraryCard.Name == "Tainted Goo") 
                        {
                            opponentHasTaintedGoo = true;
                            break;
                        }
                    }
                    Assert.IsTrue(playerHasTaintendGoo);
                    Assert.IsTrue(opponentHasTaintedGoo);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }
    }
}
