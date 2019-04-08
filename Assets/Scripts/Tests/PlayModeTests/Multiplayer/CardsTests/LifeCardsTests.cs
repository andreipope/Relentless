using System;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using UnityEngine.TestTools;
using System.Linq;

namespace Loom.ZombieBattleground.Test.MultiplayerTests
{
    public class LifeCardsTests : BaseIntegrationTest
    {
        [UnityTest]
        [Timeout(int.MaxValue)]
        [Category("QuickSubset")]
        public IEnumerator Cactuz()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 5,
                    new DeckCardData("Cactuz", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 5,
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
                           player.CardAbilityUsed(playerCardId, Enumerators.AbilityType.REANIMATE_UNIT, new List<ParametrizedAbilityInstanceId>());
                           player.CardAbilityUsed(playerCardId, Enumerators.AbilityType.ADD_CARD_BY_NAME_TO_HAND, new List<ParametrizedAbilityInstanceId>());
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentCardId, ItemPosition.Start);
                           opponent.CardAbilityUsed(opponentCardId, Enumerators.AbilityType.REANIMATE_UNIT, new List<ParametrizedAbilityInstanceId>());
                           opponent.CardAbilityUsed(opponentCardId, Enumerators.AbilityType.ADD_CARD_BY_NAME_TO_HAND, new List<ParametrizedAbilityInstanceId>());
                       },
                       player => player.CardAttack(playerCardId, opponentCardId),
                       opponent => {},
                       player => {},
                };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(1, TestHelper.GameplayManager.CurrentPlayer.CardsOnBoard.Count);
                    Assert.AreEqual(1, TestHelper.GameplayManager.OpponentPlayer.CardsOnBoard.Count);
                    Assert.AreEqual(149, (TestHelper.GameplayManager.CurrentPlayer.CardsInHand.Select(card => card.Card.Prototype.MouldId)).ToList()[0]);
                    Assert.AreEqual(149, (TestHelper.GameplayManager.OpponentPlayer.CardsInHand.Select(card => card.Card.Prototype.MouldId)).ToList()[0]);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, enableBackendGameLogicMatch: true);
            }, 300);
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        [Category("QuickSubset")]
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
                    Assert.AreEqual(1, TestHelper.GameplayManager.CurrentPlayer.CardsOnBoard.Count);
                    Assert.AreEqual(1, TestHelper.GameplayManager.OpponentPlayer.CardsOnBoard.Count);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            }, 300);
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        [Category("QuickSubset")]
        public IEnumerator Yggdrazil()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0,
                    new DeckCardData("Yggdrazil", 1),
                    new DeckCardData("Azuraz", 1),
                    new DeckCardData("MonZoon", 2),
                    new DeckCardData("Pyromaz", 20)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    new DeckCardData("Yggdrazil", 1),
                    new DeckCardData("Azuraz", 1),
                    new DeckCardData("MonZoon", 2),
                    new DeckCardData("Pyromaz", 20)
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
                       player => {},
                       opponent => {},
                       player => {
                           player.CardPlay(playerYggdrazildId, ItemPosition.Start);
                       },
                       opponent => {},
                       player => {},
                       opponent => {},
                       player => {},
                       opponent => {
                           opponent.CardPlay(opponentYggdrazildId, ItemPosition.Start);
                       },
                       player => {},
                       opponent=> {},
                       player => {}
                };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(2, TestHelper.GameplayManager.CurrentPlayer.CardsOnBoard.Count);
                    Assert.AreEqual(2, TestHelper.GameplayManager.OpponentPlayer.CardsOnBoard.Count);

                    Assert.IsNull(TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerMonZoonId));
                    Assert.IsNull(TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerMonZoon2Id));
                    Assert.IsNull(TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerPyromazId));
                    Assert.IsNull(TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerPyromaz2Id));

                    Assert.IsNull(TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentMonZoonId));
                    Assert.IsNull(TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentMonZoon2Id));
                    Assert.IsNull(TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentPyromazId));
                    Assert.IsNull(TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentPyromaz2Id));
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            }, 400);
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        [Category("QuickSubset")]
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
                    Assert.AreEqual(2, ((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerPufferId)).CurrentDamage);
                    Assert.AreEqual(2, ((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerAzurazdId)).CurrentDamage);
                    Assert.AreEqual(1, ((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerBarkId)).CurrentDamage);
                    Assert.AreEqual(1, ((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerPyromazId)).CurrentDamage);
                    Assert.AreEqual(2, ((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentPufferId)).CurrentDamage);
                    Assert.AreEqual(2, ((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentAzurazdId)).CurrentDamage);
                    Assert.AreEqual(1, ((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentBarkId)).CurrentDamage);
                    Assert.AreEqual(1, ((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentPyromazId)).CurrentDamage);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            }, 300);
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        [Category("QuickSubset")]
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
                           player.CardAbilityUsed(playerAzurazId, Enumerators.AbilityType.MODIFICATOR_STATS, new List<ParametrizedAbilityInstanceId>());
                           player.CardPlay(playerTinyId, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentAzurazId, ItemPosition.Start);
                           opponent.CardAbilityUsed(opponentAzurazId, Enumerators.AbilityType.MODIFICATOR_STATS, new List<ParametrizedAbilityInstanceId>());
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
                    Assert.AreEqual(2, ((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerTinyId)).CurrentDefense);
                    Assert.AreEqual(2, ((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentTinyId)).CurrentDefense);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            }, 300);
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        [Category("QuickSubset")]
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
                    Assert.AreEqual(6, pvpTestContext.GetCurrentPlayer().CardsInHand.Count);
                    Assert.AreEqual(7, pvpTestContext.GetOpponentPlayer().CardsInHand.Count);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            }, 300);
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        [Category("QuickSubset")]
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
                    string cardToFind = "Tainted Goo";

                    Assert.NotNull(TestHelper.GameplayManager.CurrentPlayer.CardsInHand.Select(card => card.Card.Prototype.Name == cardToFind));
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            }, 500);
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        [Category("QuickSubset")]
        public IEnumerator Amber()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 5,
                    new DeckCardData("Amber", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 5,
                    new DeckCardData("Amber", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerAmberId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Amber", 1);
                InstanceId opponentAmberId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Amber", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                       player => {},
                       opponent => {},
                       player =>
                       {
                           player.CardPlay(playerAmberId, ItemPosition.Start);
                           player.CardAbilityUsed(playerAmberId, Enumerators.AbilityType.DELAYED_GAIN_ATTACK, new List<ParametrizedAbilityInstanceId>());
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentAmberId, ItemPosition.Start);
                           opponent.CardAbilityUsed(opponentAmberId, Enumerators.AbilityType.DELAYED_GAIN_ATTACK, new List<ParametrizedAbilityInstanceId>());
                       },
                       player => {},
                       opponent => {}
                };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(3, ((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerAmberId)).CurrentDamage);
                    Assert.AreEqual(3, ((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentAmberId)).CurrentDamage);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            }, 300);
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        [Category("QuickSubset")]
        public IEnumerator Bark()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 5,
                    new DeckCardData("Bark", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 5,
                    new DeckCardData("Bark", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerBarkId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Bark", 1);
                InstanceId opponentBarkId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Bark", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                       player => {},
                       opponent => {},
                       player =>
                       {
                           player.CardPlay(playerBarkId, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentBarkId, ItemPosition.Start);
                       },
                       player => {},
                       opponent => {}
                };

                Action validateEndState = () =>
                {
                    Assert.IsTrue(((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerBarkId)).HasHeavy);
                    Assert.IsTrue(((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentBarkId)).HasHeavy);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            }, 300);
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        [Category("QuickSubset")]
        public IEnumerator PreZerver()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 5,
                    new DeckCardData("PreZerver", 1),
                    new DeckCardData("Igloo", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 5,
                    new DeckCardData("PreZerver", 1),
                    new DeckCardData("Igloo", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerPreZerverId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "PreZerver", 1);
                InstanceId playerIglooId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Igloo", 1);
                InstanceId opponentPreZerverId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "PreZerver", 1);
                InstanceId opponentIglooId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Igloo", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                       player => {},
                       opponent => {},
                       player =>
                       {
                           player.CardPlay(playerIglooId, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentIglooId, ItemPosition.Start);
                       },
                       player =>
                       {
                           player.CardAttack(playerIglooId, opponentIglooId);
                           player.CardPlay(playerPreZerverId, ItemPosition.Start, playerIglooId);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentPreZerverId, ItemPosition.Start, opponentIglooId);
                       },
                       player => {},
                       opponent => {}
                };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(3, ((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerIglooId)).CurrentDefense);
                    Assert.AreEqual(3, ((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentIglooId)).CurrentDefense);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            }, 300);
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        [Category("QuickSubset")]
        public IEnumerator Grower()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 5,
                    new DeckCardData("Grower", 1),
                    new DeckCardData("Enrager", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 5,
                    new DeckCardData("Grower", 1),
                    new DeckCardData("Enrager", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerGrowerId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Grower", 1);
                InstanceId playerEnragerId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Enrager", 1);
                InstanceId opponentGrowerId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Grower", 1);
                InstanceId opponentEnragerId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Enrager", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                       player => {},
                       opponent => {},
                       player =>
                       {
                           player.CardPlay(playerEnragerId, ItemPosition.Start);
                           player.CardAttack(playerEnragerId, pvpTestContext.GetOpponentPlayer().InstanceId);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentEnragerId, ItemPosition.Start);
                           opponent.CardAttack(opponentEnragerId, pvpTestContext.GetCurrentPlayer().InstanceId);
                       },
                       player => 
                       {
                           player.CardPlay(playerGrowerId, ItemPosition.Start);
                       },
                       opponent => 
                       {
                           opponent.CardPlay(opponentGrowerId, ItemPosition.Start);
                       },
                       player => {},
                       opponent => {}
                };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(pvpTestContext.GetCurrentPlayer().InitialDefense-3, pvpTestContext.GetCurrentPlayer().Defense);
                    Assert.AreEqual(pvpTestContext.GetOpponentPlayer().InitialDefense-3, pvpTestContext.GetOpponentPlayer().Defense);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            }, 300);
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        [Category("QuickSubset")]
        public IEnumerator Keeper()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 5,
                    new DeckCardData("Keeper", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 5,
                    new DeckCardData("Keeper", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerKeeperId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Keeper", 1);
                InstanceId opponentKeeperId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Keeper", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                       player => {},
                       opponent => {},
                       player =>
                       {
                           player.CardPlay(playerKeeperId, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentKeeperId, ItemPosition.Start);
                       },
                       player => {},
                       opponent => {}
                };

                Action validateEndState = () =>
                {
                    string cardToFind = "Ember";

                    Assert.NotNull(TestHelper.GameplayManager.CurrentPlayer.CardsOnBoard.Select(card => card.Card.Prototype.Name == cardToFind));
                    Assert.NotNull(TestHelper.GameplayManager.OpponentPlayer.CardsOnBoard.Select(card => card.Card.Prototype.Name == cardToFind));
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            }, 500);
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator WiZp()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 5,
                    new DeckCardData("WiZp", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 5,
                    new DeckCardData("WiZp", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerWiZpId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "WiZp", 1);
                InstanceId opponentWiZpId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "WiZp", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                       player => {},
                       opponent => {},
                       player =>
                       {
                           player.CardPlay(playerWiZpId, ItemPosition.Start);
                           player.CardAbilityUsed(playerWiZpId, Enumerators.AbilityType.REANIMATE_UNIT, new List<ParametrizedAbilityInstanceId>());

                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentWiZpId, ItemPosition.Start);
                           opponent.CardAbilityUsed(opponentWiZpId, Enumerators.AbilityType.REANIMATE_UNIT, new List<ParametrizedAbilityInstanceId>());
                       },
                       player =>
                       {
                           player.CardAttack(playerWiZpId, opponentWiZpId);
                       },
                       opponent => {},
                       player => {}
                };

                Action validateEndState = () =>
                {
                    string cardToFind = "WiZp";

                    Assert.NotNull(TestHelper.GameplayManager.CurrentPlayer.CardsOnBoard.Select(card => card.Card.Prototype.Name == cardToFind));
                    Assert.NotNull(TestHelper.GameplayManager.OpponentPlayer.CardsOnBoard.Select(card => card.Card.Prototype.Name == cardToFind));
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            }, 500);
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        [Category("QuickSubset")]
        public IEnumerator Shroom()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 5,
                    new DeckCardData("Shroom", 1),
                    new DeckCardData("Igloo", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 5,
                    new DeckCardData("Shroom", 1),
                    new DeckCardData("Igloo", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerShroomId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Shroom", 1);
                InstanceId opponentShroomId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Shroom", 1);
                InstanceId opponentIglooId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Igloo", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                       player => {},
                       opponent =>
                       {
                           opponent.CardPlay(opponentIglooId, ItemPosition.Start);
                       },
                       player =>
                       {
                           player.CardPlay(playerShroomId, ItemPosition.Start, opponentIglooId);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentShroomId, ItemPosition.Start, playerShroomId);
                       },
                       player => {},
                       opponent => {},
                       player => {}
                };

                Action validateEndState = () =>
                {
                   Assert.AreEqual(2, ((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerShroomId)).CurrentDefense);
                   Assert.AreEqual(3, ((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentIglooId)).CurrentDefense);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            }, 300);
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        [Category("QuickSubset")]
        public IEnumerator Sapper()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 5,
                    new DeckCardData("Sapper", 1),
                    new DeckCardData("Spiker", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 5,
                    new DeckCardData("Sapper", 1),
                    new DeckCardData("Spiker", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerSapperId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Sapper", 1);
                InstanceId playerSpikerId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Spiker", 1);
                InstanceId opponentSapperId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Sapper", 1);
                InstanceId opponentSpikerId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Spiker", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                       player => {},
                       opponent =>
                       {
                           opponent.CardPlay(opponentSpikerId, ItemPosition.Start);
                           opponent.CardPlay(opponentSapperId, ItemPosition.Start);
                           opponent.CardAbilityUsed(opponentSapperId, Enumerators.AbilityType.GAIN_NUMBER_OF_LIFE_FOR_EACH_DAMAGE_THIS_DEALS, new List<ParametrizedAbilityInstanceId>());
                       },
                       player =>
                       {
                           player.CardPlay(playerSpikerId, ItemPosition.Start);
                           player.CardPlay(playerSapperId, ItemPosition.Start);
                           player.CardAbilityUsed(playerSapperId, Enumerators.AbilityType.GAIN_NUMBER_OF_LIFE_FOR_EACH_DAMAGE_THIS_DEALS, new List<ParametrizedAbilityInstanceId>());
                       },
                       opponent =>
                       {
                           opponent.CardAttack(opponentSapperId, playerSpikerId);
                       },
                       player => {
                           player.CardAttack(playerSapperId, opponentSpikerId);
                       },
                       opponent => {},
                       player => {}
                };

                Action validateEndState = () =>
                {
                   Assert.AreEqual(3, ((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerSapperId)).CurrentDefense);
                   Assert.AreEqual(3, ((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentSapperId)).CurrentDefense);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            }, 300);
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        [Category("QuickSubset")]
        public IEnumerator EverlaZting()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 5,
                    new DeckCardData("EverlaZting", 1),
                    new DeckCardData("Igloo", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 5,
                    new DeckCardData("EverlaZting", 1),
                    new DeckCardData("Igloo", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerEverlaztingId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "EverlaZting", 1);
                InstanceId playerIglooId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Igloo", 1);
                InstanceId opponentEverlaztingId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "EverlaZting", 1);
                InstanceId opponentIglooId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Igloo", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                       player => {},
                       opponent =>
                       {
                           opponent.CardPlay(opponentIglooId, ItemPosition.Start);
                           opponent.CardPlay(opponentEverlaztingId, ItemPosition.Start);
                           opponent.CardAbilityUsed(opponentEverlaztingId, Enumerators.AbilityType.SHUFFLE_THIS_CARD_TO_DECK, new List<ParametrizedAbilityInstanceId>());
                       },
                       player =>
                       {
                           player.CardPlay(playerIglooId, ItemPosition.Start);
                           player.CardPlay(playerEverlaztingId, ItemPosition.Start);
                           player.CardAbilityUsed(playerEverlaztingId, Enumerators.AbilityType.SHUFFLE_THIS_CARD_TO_DECK, new List<ParametrizedAbilityInstanceId>());
                       },
                       opponent =>
                       {
                           opponent.CardAttack(opponentEverlaztingId, playerIglooId);
                       },
                       player => {
                           player.CardAttack(playerEverlaztingId, opponentIglooId);
                       },
                       opponent => {},
                       player => {}
                };

                Action validateEndState = () =>
                {
                    const string cardToFind = "EverlaZting";
                    Assert.IsTrue(TestHelper.GameplayManager.CurrentPlayer.CardsInHand.Any(model => model.Card.Prototype.Name == cardToFind));
                    Assert.IsTrue(TestHelper.GameplayManager.OpponentPlayer.CardsInHand.Any(model => model.Card.Prototype.Name == cardToFind));
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            }, 300);
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        [Category("QuickSubset")]
        public IEnumerator Healz()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 5,
                    new DeckCardData("Healz", 2),
                    new DeckCardData("Enrager", 20)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 5,
                    new DeckCardData("Healz", 2),
                    new DeckCardData("Enrager", 20)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerHealz1Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Healz", 1);
                InstanceId playerHealz2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Healz", 2);
                InstanceId playerEnragerId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Enrager", 1);
                InstanceId opponentHealz1Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Healz", 1);
                InstanceId opponentHealz2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Healz", 2);
                InstanceId opponentEnragerId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Enrager", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                       player => {},
                       opponent =>
                       {
                           opponent.CardPlay(opponentEnragerId, ItemPosition.Start);
                           opponent.CardAttack(opponentEnragerId, pvpTestContext.GetCurrentPlayer().InstanceId);
                       },
                       player =>
                       {
                           player.CardPlay(playerEnragerId, ItemPosition.Start);
                           player.CardAttack(playerEnragerId, pvpTestContext.GetOpponentPlayer().InstanceId);
                       },
                       opponent =>
                       {
                           opponent.CardAttack(opponentEnragerId, pvpTestContext.GetCurrentPlayer().InstanceId);
                       },
                       player =>
                       {
                           player.CardAttack(playerEnragerId, pvpTestContext.GetOpponentPlayer().InstanceId);
                        },
                       opponent =>
                       {
                           opponent.CardPlay(opponentHealz1Id, ItemPosition.Start);
                           opponent.CardPlay(opponentHealz2Id, ItemPosition.Start);
                       },
                       player => {
                           player.CardPlay(playerHealz1Id, ItemPosition.Start);
                           player.CardPlay(playerHealz2Id, ItemPosition.Start);
                       },
                       opponent => {}
                };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(18, pvpTestContext.GetCurrentPlayer().Defense);
                    Assert.AreEqual(18, pvpTestContext.GetOpponentPlayer().Defense);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            }, 300);
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Zeeder()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 5,
                    new DeckCardData("Zeeder", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 5,
                    new DeckCardData("Zeeder", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerZeederId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zeeder", 1);
                InstanceId opponentZeederId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zeeder", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                       player => {},
                       opponent => {},
                       player =>
                       {
                           player.CardPlay(playerZeederId, ItemPosition.Start);
                           player.CardAbilityUsed(playerZeederId, Enumerators.AbilityType.SUMMON, new List<ParametrizedAbilityInstanceId>());
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentZeederId, ItemPosition.Start);
                           opponent.CardAbilityUsed(opponentZeederId, Enumerators.AbilityType.SUMMON, new List<ParametrizedAbilityInstanceId>());
                       },
                       player => {},
                       opponent => {}
                };

                Action validateEndState = () =>
                {
                    string cardToFind = "Bark";

                    Assert.NotNull(TestHelper.GameplayManager.CurrentPlayer.CardsOnBoard.Select(card => card.Card.Prototype.Name == cardToFind));
                    Assert.NotNull(TestHelper.GameplayManager.OpponentPlayer.CardsOnBoard.Select(card => card.Card.Prototype.Name == cardToFind));
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            }, 500);
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator ZVirus()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 5,
                    new DeckCardData("Z-Virus", 1),
                    new DeckCardData("Igloo", 3)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 5,
                    new DeckCardData("Z-Virus", 1),
                    new DeckCardData("Igloo", 3)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerZVirusId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Z-Virus", 1);
                InstanceId playerIgloo1Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Igloo", 1);
                InstanceId playerIgloo2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Igloo", 2);
                InstanceId playerIgloo3Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Igloo", 3);
                InstanceId opponentZVirusId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Z-Virus", 1);
                InstanceId opponentIgloo1Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Igloo", 1);
                InstanceId opponentIgloo2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Igloo", 2);
                InstanceId opponentIgloo3Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Igloo", 3);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                       player => {},
                       opponent => {},
                       player =>
                       {
                           player.CardPlay(playerIgloo1Id, ItemPosition.Start);
                           player.CardPlay(playerIgloo2Id, ItemPosition.Start);
                           player.CardPlay(playerIgloo3Id, ItemPosition.Start);
                           player.CardPlay(playerZVirusId, ItemPosition.Start, null, true);
                           player.CardAbilityUsed(playerZVirusId, Enumerators.AbilityType.DEVOUR_ZOMBIES_AND_COMBINE_STATS, new List<ParametrizedAbilityInstanceId>()
                           {
                               new ParametrizedAbilityInstanceId(playerIgloo1Id),
                               new ParametrizedAbilityInstanceId(playerIgloo2Id),
                               new ParametrizedAbilityInstanceId(playerIgloo3Id)
                           });
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentIgloo1Id, ItemPosition.Start);
                           opponent.CardPlay(opponentIgloo2Id, ItemPosition.Start);
                           opponent.CardPlay(opponentIgloo3Id, ItemPosition.Start);
                        },
                       player => {},
                       opponent => {
                           opponent.CardPlay(opponentZVirusId, ItemPosition.Start, null, true);
                           opponent.CardAbilityUsed(opponentZVirusId, Enumerators.AbilityType.DEVOUR_ZOMBIES_AND_COMBINE_STATS, new List<ParametrizedAbilityInstanceId>()
                           {
                               new ParametrizedAbilityInstanceId(opponentIgloo1Id),
                               new ParametrizedAbilityInstanceId(opponentIgloo2Id),
                               new ParametrizedAbilityInstanceId(opponentIgloo3Id)
                           });
                       },
                       player => {},
                       opponent => {}
                };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(13, ((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerZVirusId)).CurrentDamage);
                    Assert.AreEqual(16, ((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerZVirusId)).CurrentDefense);
                    Assert.AreEqual(1, TestHelper.GameplayManager.CurrentPlayer.CardsOnBoard.Count);
                    Assert.AreEqual(13, ((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentZVirusId)).CurrentDamage);
                    Assert.AreEqual(16, ((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentZVirusId)).CurrentDefense);
                    Assert.AreEqual(1, TestHelper.GameplayManager.OpponentPlayer.CardsOnBoard.Count);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            }, 300);
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Shammann()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 5,
                    new DeckCardData("Shammann", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 5,
                    new DeckCardData("Shammann", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerShammannId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Shammann", 1);
                InstanceId opponentShammannId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Shammann", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                       player => {},
                       opponent => {},
                       player =>
                       {
                           player.CardPlay(playerShammannId, ItemPosition.Start);
                           player.CardAbilityUsed(playerShammannId, Enumerators.AbilityType.SUMMON, new List<ParametrizedAbilityInstanceId>());
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentShammannId, ItemPosition.Start);
                           opponent.CardAbilityUsed(opponentShammannId, Enumerators.AbilityType.SUMMON, new List<ParametrizedAbilityInstanceId>());
                       },
                       player => {},
                       opponent => {},
                       player => {},
                       opponent => {}
                };

                Action validateEndState = () =>
                {
                    string cardToFind = "Azuraz";

                    Assert.NotNull(TestHelper.GameplayManager.CurrentPlayer.CardsOnBoard.Select(card => card.Card.Prototype.Name == cardToFind));
                    Assert.NotNull(TestHelper.GameplayManager.OpponentPlayer.CardsOnBoard.Select(card => card.Card.Prototype.Name == cardToFind));
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            }, 300);
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Zplitter()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 5,
                    new DeckCardData("Zplitter", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 5,
                    new DeckCardData("Zplitter", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerZplitterId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zplitter", 1);
                InstanceId opponentZplitterId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zplitter", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                       player => {},
                       opponent => {},
                       player =>
                       {
                           player.CardPlay(playerZplitterId, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentZplitterId, ItemPosition.Start);
                       },
                       player => {},
                       opponent => {}
                };

                Action validateEndState = () =>
                {
                    string cardToFind = "Zplitter";

                    Assert.AreEqual(2, TestHelper.GameplayManager.CurrentPlayer.CardsOnBoard.Select(card => card.Card.Prototype.Name == cardToFind).ToList().Count);
                    Assert.AreEqual(2, TestHelper.GameplayManager.OpponentPlayer.CardsOnBoard.Select(card => card.Card.Prototype.Name == cardToFind).ToList().Count);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            }, 300);
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Blight()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 5,
                    new DeckCardData("Blight", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 5,
                    new DeckCardData("Blight", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerBlightId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Blight", 1);
                InstanceId opponentBlightId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Blight", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                       player => {},
                       opponent => {},
                       player =>
                       {
                           player.CardPlay(playerBlightId, ItemPosition.Start);
                           player.CardAbilityUsed(playerBlightId, Enumerators.AbilityType.DELAYED_PLACE_COPIES_IN_PLAY_DESTROY_UNIT, new List<ParametrizedAbilityInstanceId>());
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentBlightId, ItemPosition.Start);
                           opponent.CardAbilityUsed(opponentBlightId, Enumerators.AbilityType.DELAYED_PLACE_COPIES_IN_PLAY_DESTROY_UNIT, new List<ParametrizedAbilityInstanceId>());
                       },
                       player => {},
                       opponent => {},
                       player => {},
                       opponent => {},
                       player => {},
                       opponent => {}
                };

                Action validateEndState = () =>
                {
                    string cardToFind = "Blightling";

                    Assert.AreEqual(2, TestHelper.GameplayManager.CurrentPlayer.CardsOnBoard.Where(card => card.Card.Prototype.Name == cardToFind).ToList().Count);
                    Assert.AreEqual(2, TestHelper.GameplayManager.OpponentPlayer.CardsOnBoard.Where(card => card.Card.Prototype.Name == cardToFind).ToList().Count);

                    cardToFind = "Blight";

                    Assert.IsFalse(TestHelper.GameplayManager.CurrentPlayer.CardsOnBoard.Any(card => card.Card.Prototype.Name == cardToFind));
                    Assert.IsFalse(TestHelper.GameplayManager.OpponentPlayer.CardsOnBoard.Any(card => card.Card.Prototype.Name == cardToFind));
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            }, 500);
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Rainz()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 5,
                    new DeckCardData("Rainz", 1),
                    new DeckCardData("Igloo", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 5,
                    new DeckCardData("Rainz", 1),
                    new DeckCardData("Igloo", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerRainzId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Rainz", 1);
                InstanceId playerIglooId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Igloo", 1);
                InstanceId opponentRainzId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Rainz", 1);
                InstanceId opponentIglooId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Igloo", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                       player => {},
                       opponent => {},
                       player =>
                       {
                           player.CardPlay(playerIglooId, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentIglooId, ItemPosition.Start);
                       },
                       player =>
                       {
                           player.CardAttack(playerIglooId, opponentIglooId);
                       },
                       opponent =>
                       {
                           opponent.CardAttack(opponentIglooId, pvpTestContext.GetCurrentPlayer().InstanceId);
                       },
                       player =>
                       {
                           player.CardAttack(playerIglooId, pvpTestContext.GetOpponentPlayer().InstanceId);
                           player.CardPlay(playerRainzId, ItemPosition.Start);
                       },
                       opponent => {},
                       player => {},
                       opponent => {}
                };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(pvpTestContext.GetCurrentPlayer().InitialDefense, pvpTestContext.GetCurrentPlayer().Defense);
                    Assert.AreEqual(5, ((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerIglooId)).CurrentDefense);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            }, 500);
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Vindrom()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 5,
                    new DeckCardData("Vindrom", 1),
                    new DeckCardData("Slab", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 5,
                    new DeckCardData("Vindrom", 1),
                    new DeckCardData("Slab", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerVindromId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Vindrom", 1);
                InstanceId playerSlabId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Slab", 1);
                InstanceId opponentVindromId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Vindrom", 1);
                InstanceId opponentSlabId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Slab", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                       player => {},
                       opponent => {},
                       player =>
                       {
                           player.CardPlay(playerSlabId, ItemPosition.Start);
                           player.CardPlay(playerVindromId, ItemPosition.Start);
                           player.CardAbilityUsed(playerVindromId, Enumerators.AbilityType.STUN, new List<ParametrizedAbilityInstanceId>());
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentSlabId, ItemPosition.Start);
                           opponent.CardPlay(opponentVindromId, ItemPosition.Start);
                           opponent.CardAbilityUsed(opponentVindromId, Enumerators.AbilityType.STUN, new List<ParametrizedAbilityInstanceId>());
                       },
                       player => 
                       {
                           player.CardAttack(playerVindromId, opponentSlabId);
                       },
                       opponent => 
                       {
                           Assert.IsTrue(((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentSlabId)).IsStun);
                       }
                };

                Action validateEndState = () =>
                {};

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            }, 500);
        }
    }
}
