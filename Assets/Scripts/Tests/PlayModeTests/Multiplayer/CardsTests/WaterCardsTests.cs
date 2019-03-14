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
    public class WaterCardsTests : BaseIntegrationTest
    {
		[UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Znowman()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0,
                    new DeckCardData("Slab", 5),
                    new DeckCardData("Znowman", 1)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    new DeckCardData("Slab", 5),
                    new DeckCardData("Znowman", 1)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerZnowmanId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Znowman", 1);
                InstanceId playerSlabId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Slab", 1);
                InstanceId playerSlab2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Slab", 2);
                InstanceId opponentZnowmanId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Znowman", 1);
                InstanceId opponentSlabId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Slab", 1);
                InstanceId opponentSlab2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Slab", 2);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                       player => {},
                       opponent => {},
                       player => {},
                       opponent => {},
                       player => {
                           player.CardPlay(playerZnowmanId, ItemPosition.Start);
                           player.CardAbilityUsed(playerZnowmanId, Enumerators.AbilityType.ENEMY_THAT_ATTACKS_BECOME_FROZEN, new List<ParametrizedAbilityInstanceId>());
                           player.CardPlay(playerSlabId, ItemPosition.Start);
                           player.CardPlay(playerSlab2Id, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentZnowmanId, ItemPosition.Start);
                           opponent.CardAbilityUsed(opponentZnowmanId, Enumerators.AbilityType.ENEMY_THAT_ATTACKS_BECOME_FROZEN, new List<ParametrizedAbilityInstanceId>());
                           opponent.CardPlay(opponentSlabId, ItemPosition.Start);
                           opponent.CardPlay(opponentSlab2Id, ItemPosition.Start);
                       },
                       player =>
                       {
                           player.CardAttack(playerSlabId, opponentZnowmanId);
                       },
                       opponent =>
                       {
                           opponent.CardAttack(opponentSlabId, playerZnowmanId);
                       },
                       player =>
                       {
                           Assert.AreEqual(true, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerSlabId)).IsStun);
                           Assert.AreEqual(false, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerSlab2Id)).IsStun);
                       },
                       opponent =>
                       {
                           Assert.AreEqual(true, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentSlabId)).IsStun);
                           Assert.AreEqual(false, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentSlab2Id)).IsStun);
                       },
                };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(3, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerSlabId)).CurrentDefense);
                    Assert.AreEqual(1, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerZnowmanId)).CurrentDefense);

                    Assert.AreEqual(3, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentSlabId)).CurrentDefense);
                    Assert.AreEqual(1, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentZnowmanId)).CurrentDefense);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }

    
        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Jetter()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 2,
                    new DeckCardData("Jetter", 2));
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 2,
                    new DeckCardData("Jetter", 2));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerJetter1 = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Jetter", 1);
                InstanceId playerJetter2 = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Jetter", 2);

                InstanceId opponentJetter1 = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Jetter", 1);
                InstanceId opponentJetter2 = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Jetter", 2);

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
                       player => player.CardPlay(playerJetter1, ItemPosition.Start),
                       opponent =>
                       {
                           opponent.CardPlay(opponentJetter1, ItemPosition.Start, playerJetter1);
                       },
                       player => player.CardPlay(playerJetter2, ItemPosition.Start, opponentJetter1),
                       opponent =>
                       {
                           opponent.CardPlay(opponentJetter2, ItemPosition.Start, playerJetter2);
                       },
                };

                BoardUnitModel playerJetter1Model = null;
                BoardUnitModel playerJetter2Model = null;
                BoardUnitModel opponentJetter1Model = null;
                BoardUnitModel opponentJetter2Model = null;

                Action validateEndState = () =>
                {
                    playerJetter1Model = (BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerJetter1);
                    playerJetter2Model = (BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerJetter2);
                    opponentJetter1Model = (BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentJetter1);
                    opponentJetter2Model = (BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentJetter2);

                    Assert.NotNull(playerJetter1Model);
                    Assert.NotNull(playerJetter2Model);
                    Assert.NotNull(opponentJetter1Model);
                    Assert.NotNull(opponentJetter2Model);

                    Assert.AreEqual(playerJetter1Model.MaxCurrentDefense - 1, playerJetter1Model.CurrentDefense);
                    Assert.AreEqual(playerJetter2Model.MaxCurrentDefense - 1, playerJetter2Model.CurrentDefense);
                    Assert.AreEqual(opponentJetter1Model.MaxCurrentDefense - 1, opponentJetter1Model.CurrentDefense);
                    Assert.AreEqual(opponentJetter2Model.MaxCurrentDefense, opponentJetter2Model.CurrentDefense);
                };


                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Maelstrom()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 2,
                    new DeckCardData("Maelstrom", 1),
                    new DeckCardData("Whizpar", 2));
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 2,
                    new DeckCardData("Maelstrom", 1),
                    new DeckCardData("Whizpar", 2));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerWhizparId1 = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Whizpar", 1);
                InstanceId playerWhizparId2 = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Whizpar", 2);
                InstanceId playerMaelstromId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Maelstrom", 1);

                InstanceId opponentWhizparId1 = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Whizpar", 1);
                InstanceId opponentWhizparId2 = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Whizpar", 2);
                InstanceId opponentMaelstromrId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Maelstrom", 1);

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
                       player =>
                       {
                           player.CardPlay(playerWhizparId1, ItemPosition.Start);
                           player.CardPlay(playerWhizparId2, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentWhizparId1, ItemPosition.Start);
                           opponent.CardPlay(opponentWhizparId2, ItemPosition.Start);
                       },
                       player =>
                       {
                           player.CardAttack(playerWhizparId1, opponentWhizparId1);
                           player.CardPlay(playerMaelstromId, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentWhizparId2, ItemPosition.Start);
                           opponent.CardPlay(opponentMaelstromrId, ItemPosition.Start);
                       },
                       player => {},
                       opponent => {},
                   };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(0, pvpTestContext.GetCurrentPlayer().CardsOnBoard.Count);
                    Assert.AreEqual(1, pvpTestContext.GetOpponentPlayer().CardsOnBoard.Count);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Izze()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 2,
                    new DeckCardData("Izze", 2),
                    new DeckCardData("Burrrnn", 1),
                    new DeckCardData("Slab", 1)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 2,
                    new DeckCardData("Izze", 2),
                    new DeckCardData("Burrrnn", 1),
                    new DeckCardData("Slab", 1)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerIzze1 = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Izze", 1);
                InstanceId playerIzze2 = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Izze", 2);
                InstanceId playerBurrrnn = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Burrrnn", 1);
                InstanceId playerSlab = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Slab", 1);

                InstanceId opponentIzze1 = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Izze", 1);
                InstanceId opponentIzze2 = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Izze", 2);
                InstanceId opponentBurrrnn = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Burrrnn", 1);
                InstanceId opponentSlab = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Slab", 1);


                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                       player => {},
                       opponent => {},
                       player =>
                       {
                           player.CardPlay(playerBurrrnn, ItemPosition.Start);
                           player.CardPlay(playerSlab, ItemPosition.Start);
                           player.CardPlay(playerIzze1, ItemPosition.Start);
                           player.CardPlay(playerIzze2, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentSlab, ItemPosition.Start);
                           opponent.CardPlay(opponentBurrrnn, ItemPosition.Start);
                           opponent.CardPlay(opponentIzze1, ItemPosition.Start);
                           opponent.CardPlay(opponentIzze2, ItemPosition.Start);
                       },
                       player =>
                       {
                           player.CardAttack(playerIzze1, opponentSlab);
                           player.CardAttack(playerIzze2, opponentBurrrnn);
                       },
                       opponent =>
                       {
                           opponent.CardAttack(opponentIzze1, playerBurrrnn);
                           opponent.CardAttack(opponentIzze2, playerSlab);
                       },
                       player => {},
                       opponent => {},
                       player => {},
                       opponent => {},
                };

                BoardUnitModel playerBurrrnnModel = null;
                BoardUnitModel playerSlabModel = null;
                BoardUnitModel opponentSlabModel = null;
                BoardUnitModel opponentBurrrnnModel = null;

                Action validateEndState = () =>
                {
                    playerBurrrnnModel = (BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerBurrrnn);
                    playerSlabModel = (BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerSlab);
                    opponentSlabModel = (BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentSlab);
                    opponentBurrrnnModel = (BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentBurrrnn);

                    Assert.Null(playerBurrrnnModel);
                    Assert.NotNull(playerSlabModel);
                    Assert.NotNull(opponentSlabModel);
                    Assert.Null(opponentBurrrnnModel);

                    Assert.AreEqual(playerSlabModel.MaxCurrentDefense - 1, playerSlabModel.CurrentDefense);
                    Assert.AreEqual(opponentSlabModel.MaxCurrentDefense - 1, opponentSlabModel.CurrentDefense);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Ztalagmite()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 2,
                    new DeckCardData("Ztalagmite", 15));
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 2,
                    new DeckCardData("Ztalagmite", 15));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerZtalagmiteId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Ztalagmite", 1);

                InstanceId opponentZtalagmiteId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Ztalagmite", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                   {
                       player => {},
                       opponent => {},
                       player =>
                       {
                           player.CardPlay(playerZtalagmiteId, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentZtalagmiteId, ItemPosition.Start);
                       },
                       player => {},
                       opponent => {},
                   };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(pvpTestContext.GetCurrentPlayer().MaxCurrentDefense - 3, pvpTestContext.GetCurrentPlayer().Defense);
                    Assert.AreEqual(pvpTestContext.GetOpponentPlayer().MaxCurrentDefense - 3, pvpTestContext.GetOpponentPlayer().Defense);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Ozmoziz()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 2,
                    new DeckCardData("Ozmoziz", 10));
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 2,
                    new DeckCardData("Ozmoziz", 10));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Ozmoziz", 1);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Ozmoziz", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                    player => {},
                    opponent => {},
                    player => {},
                    opponent => {},
                    player => player.CardPlay(playerCardId, ItemPosition.Start),
                    opponent =>
                    {
                        opponent.CardPlay(opponentCardId, ItemPosition.Start);
                        opponent.CardAbilityUsed(opponentCardId, Enumerators.AbilityType.ADD_CARD_BY_NAME_TO_HAND, new List<ParametrizedAbilityInstanceId>());
                    },
                    player => { }
                };

                Action validateEndState = () =>
                {
                    Assert.IsTrue(pvpTestContext.GetCurrentPlayer().CardsInHand.FindAll(card => card.Card.Prototype.MouldId == 155).Count > 0);
                    Assert.IsTrue(pvpTestContext.GetOpponentPlayer().CardsInHand.FindAll(card => card.Card.Prototype.MouldId == 155).Count > 0);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator HoZer()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 2,
                    new DeckCardData("Znowy", 1),
                    new DeckCardData("HoZer", 2));
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 2,
                    new DeckCardData("Znowy", 1),
                    new DeckCardData("HoZer", 2));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerHoZer1 = pvpTestContext.GetCardInstanceIdByName(playerDeck, "HoZer", 1);
                InstanceId playerHoZer2 = pvpTestContext.GetCardInstanceIdByName(playerDeck, "HoZer", 2);
                InstanceId playerZnowy = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Znowy", 1);

                InstanceId opponentHoZer1 = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "HoZer", 1);
                InstanceId opponentHoZer2 = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "HoZer", 2);
                InstanceId opponentZnowy = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Znowy", 1);


                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                       player => {},
                       opponent => {},
                       player =>
                       {
                           player.CardPlay(playerHoZer1, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentHoZer1, ItemPosition.Start);
                       },
                       player =>
                       {
                           player.CardPlay(playerZnowy, ItemPosition.Start);
                           player.CardPlay(playerHoZer2, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentZnowy, ItemPosition.Start);
                           opponent.CardPlay(opponentHoZer2, ItemPosition.Start);
                       },
                };

                BoardUnitModel playerHoZer1Model = null;
                BoardUnitModel playerHoZer2Model = null;
                BoardUnitModel opponentHoZer1Model = null;
                BoardUnitModel opponentHoZer2Model = null;

                Action validateEndState = () =>
                {
                    playerHoZer1Model = (BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerHoZer1);
                    playerHoZer2Model = (BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerHoZer2);
                    opponentHoZer1Model = (BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentHoZer1);
                    opponentHoZer2Model = (BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentHoZer2);

                    Assert.AreEqual(playerHoZer1Model.Card.Prototype.Damage, playerHoZer1Model.CurrentDamage);
                    Assert.AreEqual(playerHoZer2Model.Card.Prototype.Damage + 1, playerHoZer2Model.CurrentDamage);
                    Assert.AreEqual(opponentHoZer1Model.Card.Prototype.Damage, opponentHoZer1Model.CurrentDamage);
                    Assert.AreEqual(opponentHoZer2Model.Card.Prototype.Damage + 1, opponentHoZer2Model.CurrentDamage);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator FroZen()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 2,
                    new DeckCardData("FroZen", 1),
                    new DeckCardData("Slab", 15));
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 2,
                    new DeckCardData("FroZen", 1),
                    new DeckCardData("Slab", 15));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerFroZenId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "FroZen", 1);
                InstanceId playerSlabId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Slab", 1);

                InstanceId opponentFroZenId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "FroZen", 1);
                InstanceId opponentSlabId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Slab", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                    player => {},
                    opponent => {},
                    player => {},
                    opponent => {},
                    player =>
                    {
                        player.CardPlay(playerSlabId, ItemPosition.Start);
                        player.CardPlay(playerFroZenId, ItemPosition.Start);
                    },
                    opponent =>
                    {
                        opponent.CardPlay(opponentSlabId, ItemPosition.Start);
                        opponent.CardPlay(opponentFroZenId, ItemPosition.Start);
                        opponent.CardAbilityUsed(opponentFroZenId, Enumerators.AbilityType.DRAW_CARD, new List<ParametrizedAbilityInstanceId>());
                    },
                    player =>
                    {
                        player.CardAttack(playerSlabId, opponentFroZenId);
                    },
                    opponent =>
                    {
                        opponent.CardAttack(opponentSlabId, playerFroZenId);
                    },
                    player => {},
                    opponent => {},
                };

                Action validateEndState = () =>
                {
                    Assert.IsTrue(pvpTestContext.GetCurrentPlayer().CardsInHand.Count == 8);
                    Assert.IsTrue(pvpTestContext.GetOpponentPlayer().CardsInHand.Count == 8);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Slider()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 2,
                    new DeckCardData("Slider", 1),
                    new DeckCardData("Slab", 15));
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 2,
                    new DeckCardData("Slider", 1),
                    new DeckCardData("Slab", 15));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerSliderId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Slider", 1);
                InstanceId playerSlabId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Slab", 1);

                InstanceId opponentSliderId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Slider", 1);
                InstanceId opponentSlabId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Slab", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                    player => {},
                    opponent => {},
                    player => {},
                    opponent => {},
                    player =>
                    {
                        player.CardPlay(playerSlabId, ItemPosition.Start);
                    },
                    opponent =>
                    {
                        opponent.CardPlay(opponentSlabId, ItemPosition.Start);
                    },
                    player =>
                    {
                        player.CardPlay(playerSliderId, ItemPosition.Start, opponentSlabId);
                    },
                    opponent =>
                    {
                        opponent.CardPlay(opponentSliderId, ItemPosition.Start, playerSlabId);
                    },
                };

                Action validateEndState = () =>
                {
                    Assert.IsTrue(((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerSlabId))
                        .GameMechanicDescriptionsOnUnit.Contains(Enumerators.GameMechanicDescription.Distract));
                    Assert.IsTrue(((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentSlabId))
                       .GameMechanicDescriptionsOnUnit.Contains(Enumerators.GameMechanicDescription.Distract));
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Geyzer()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 2,
                    new DeckCardData("Geyzer", 10));
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 2,
                    new DeckCardData("Geyzer", 10));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Geyzer", 1);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Geyzer", 1);

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
                        opponent.CardAbilityUsed(opponentCardId, Enumerators.AbilityType.ADD_CARD_BY_NAME_TO_HAND, new List<ParametrizedAbilityInstanceId>());
                    },
                    player => { }
                };

                Action validateEndState = () =>
                {
                    Assert.IsNotNull(pvpTestContext.GetCurrentPlayer().CardsInHand.Select(card => card.Card.Prototype.MouldId == 156));
                    Assert.IsNotNull(pvpTestContext.GetOpponentPlayer().CardsInHand.Select(card => card.Card.Prototype.MouldId == 156));
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Froztbite()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 2,
                    new DeckCardData("Froztbite", 10));
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 2,
                    new DeckCardData("Froztbite", 10));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Froztbite", 1);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Froztbite", 1);

                int delayedDamage = 6;

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
                        opponent.CardAbilityUsed(opponentCardId, Enumerators.AbilityType.DELAYED_GAIN_ATTACK, new List<ParametrizedAbilityInstanceId>());
                    },
                    player => {},
                    opponent => {},
                };

                Action validateEndState = () =>
                {
                    BoardUnitModel playerUnit = (BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCardId);
                    BoardUnitModel opponentUnit = (BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCardId);
                    Assert.AreEqual(playerUnit.Card.Prototype.Damage + delayedDamage, playerUnit.CurrentDamage);
                    Assert.AreEqual(opponentUnit.Card.Prototype.Damage + delayedDamage, opponentUnit.CurrentDamage);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Freezzee()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 2,
                    new DeckCardData("Freezzee", 2),
                    new DeckCardData("Slab", 20));
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 2,
                    new DeckCardData("Freezzee", 2),
                    new DeckCardData("Slab", 20));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerFreezzeeId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Freezzee", 1);
                InstanceId playerFreezzee2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Freezzee", 2);
                InstanceId playerSlabId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Slab", 1);
                InstanceId playerSlab2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Slab", 2);
                InstanceId playerSlab3Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Slab", 3);
                InstanceId opponentFreezzeeId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Freezzee", 1);
                InstanceId opponentFreezzee2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Freezzee", 2);
                InstanceId opponentSlabId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Slab", 1);
                InstanceId opponentSlab2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Slab", 2);
                InstanceId opponentSlab3Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Slab", 3);

                BoardUnitModel playerUnitSlab = null;
                BoardUnitModel playerUnitSlab2 = null;
                BoardUnitModel playerUnitSlab3 = null;

                BoardUnitModel opponentUnitSlab = null;
                BoardUnitModel opponentUnitSlab2 = null;
                BoardUnitModel opponentUnitSlab3 = null;

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                    player => {},
                    opponent => {},
                    player => {},
                    opponent => {},
                    player => {},
                    opponent => {},
                    player =>
                    {
                        player.CardPlay(playerSlabId, ItemPosition.End);
                        player.CardPlay(playerSlab2Id, ItemPosition.End);
                        player.CardPlay(playerSlab3Id, ItemPosition.End);                       
                    },
                    opponent =>
                    {
                        opponent.CardPlay(opponentSlabId, ItemPosition.End);
                        opponent.CardPlay(opponentSlab2Id, ItemPosition.End);
                        opponent.CardPlay(opponentSlab3Id, ItemPosition.End);
                    },
                    player =>
                    {
                        playerUnitSlab = ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerSlabId));
                        playerUnitSlab2 = ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerSlab2Id));
                        playerUnitSlab3 = ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerSlab3Id));
                        player.CardPlay(playerFreezzeeId, ItemPosition.End, opponentSlab2Id);
                        player.CardPlay(playerFreezzee2Id, ItemPosition.End, opponentSlab2Id);
                    },
                    opponent =>
                    {
                        opponentUnitSlab = ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentSlabId));
                        opponentUnitSlab2 = ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentSlab2Id));
                        opponentUnitSlab3 = ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentSlab3Id));
                        opponent.CardPlay(opponentFreezzeeId, ItemPosition.End, playerSlab2Id);
                        opponent.CardPlay(opponentFreezzee2Id, ItemPosition.End, playerSlab2Id);
                    },
                    player =>
                    {
                        Assert.AreEqual(playerUnitSlab.Card.Prototype.Defense - 2, playerUnitSlab.CurrentDefense);
                        Assert.AreEqual(playerUnitSlab2.Card.Prototype.Defense - 2, playerUnitSlab2.CurrentDefense);
                        Assert.AreEqual(playerUnitSlab3.Card.Prototype.Defense - 2, playerUnitSlab3.CurrentDefense);
                    },
                    opponent =>
                    {
                        Assert.AreEqual(opponentUnitSlab.Card.Prototype.Defense - 2, opponentUnitSlab.CurrentDefense);
                        Assert.AreEqual(opponentUnitSlab2.Card.Prototype.Defense - 2, opponentUnitSlab2.CurrentDefense);
                        Assert.AreEqual(opponentUnitSlab3.Card.Prototype.Defense - 2, opponentUnitSlab3.CurrentDefense);
                    },
                };

                Action validateEndState = () => {};

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Zhatterer()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 2,
                    new DeckCardData("Slab", 1),
                    new DeckCardData("Freezzee", 1),
                    new DeckCardData("Zhatterer", 10));
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 2,
                    new DeckCardData("Slab", 1),
                    new DeckCardData("Freezzee", 1),
                    new DeckCardData("Zhatterer", 10));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerSlabId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Slab", 1);
                InstanceId playerZhattererId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zhatterer", 1);
                InstanceId playerFreezzeeId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Freezzee", 1);
                InstanceId opponentSlabId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Slab", 1);
                InstanceId opponentZhattererId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zhatterer", 1);
                InstanceId opponentFreezzeeId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Freezzee", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                    player => {},
                    opponent => {},
                    player => {},
                    opponent => {},
                    player =>
                    {
                        player.CardPlay(playerSlabId, ItemPosition.Start);
                    },
                    opponent =>
                    {
                        opponent.CardPlay(opponentSlabId, ItemPosition.Start);
                        opponent.CardPlay(opponentFreezzeeId, ItemPosition.Start, playerSlabId);
                        opponent.CardPlay(opponentZhattererId, ItemPosition.Start, playerSlabId);
                    },
                    player =>
                    {
                        player.CardPlay(playerFreezzeeId, ItemPosition.Start, opponentSlabId);
                        player.CardPlay(playerZhattererId, ItemPosition.Start, opponentSlabId);
                    },
                    player => {},
                    opponent => {},
                };

                Action validateEndState = () =>
                {
                    Assert.Null((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerSlabId));
                    Assert.Null((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentSlabId));
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Izicle()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 2,
                    new DeckCardData("Izicle", 1),
                    new DeckCardData("Znowy", 20));

                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 2,
                    new DeckCardData("Izicle", 1),
                    new DeckCardData("Znowy", 20));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerIzicleId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Izicle", 1);
                InstanceId playerZnowyId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Znowy", 1);
                InstanceId opponentIzicleId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Izicle", 1);
                InstanceId opponentZnowyId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Znowy", 1);

                int difference = 2;

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                    player => {},
                    opponent => {},
                    player => {},
                    opponent =>
                    {
                        opponent.CardPlay(opponentZnowyId, ItemPosition.Start);
                        opponent.CardPlay(opponentIzicleId, ItemPosition.Start);
                        opponent.CardAbilityUsed(opponentIzicleId, Enumerators.AbilityType.DAMAGE_TARGET, new List<ParametrizedAbilityInstanceId>()
                        {
                            new ParametrizedAbilityInstanceId(pvpTestContext.GetCurrentPlayer().InstanceId)
                        });
                    },
                    player =>
                    {
                        player.CardPlay(playerZnowyId, ItemPosition.Start);
                        player.CardPlay(playerIzicleId, ItemPosition.Start, pvpTestContext.GetOpponentPlayer().InstanceId);
                    },
                    player => {},
                    opponent => {},
                };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(pvpTestContext.GetCurrentPlayer().InitialDefense - difference, pvpTestContext.GetCurrentPlayer().Defense);
                    Assert.AreEqual(pvpTestContext.GetOpponentPlayer().InitialDefense - difference, pvpTestContext.GetOpponentPlayer().Defense);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Zpring()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 2,
                    new DeckCardData("Zpring", 1),
                    new DeckCardData("Zwoop", 20));

                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 2,
                    new DeckCardData("Zpring", 1),
                    new DeckCardData("Zwoop", 20));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerZpringId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zpring", 1);
                InstanceId playerZwoopId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zwoop", 1);
                InstanceId opponentZpringId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zpring", 1);
                InstanceId opponentZwoopId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zwoop", 1);

                int value = 1;

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                    player => {},
                    opponent => {},
                    player => {},
                    opponent =>
                    {
                        opponent.CardPlay(opponentZwoopId, ItemPosition.Start);
                        opponent.CardPlay(opponentZpringId, ItemPosition.Start);
                    },
                    player =>
                    {
                        player.CardPlay(playerZpringId, ItemPosition.Start);
                        player.CardPlay(playerZwoopId, ItemPosition.Start);
                    },
                    opponent => {},
                    player => {},
                    opponent =>
                    {
                        Assert.AreEqual(value, pvpTestContext.GetOpponentPlayer().ExtraGoo);
                        Assert.AreEqual(value, pvpTestContext.GetCurrentPlayer().ExtraGoo);
                        opponent.CardAttack(opponentZwoopId, playerZpringId);
                    },
                    player =>
                    {
                        player.CardAttack(playerZwoopId, opponentZpringId);
                    },
                    opponent => {},
                };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(0, pvpTestContext.GetCurrentPlayer().ExtraGoo);
                    Assert.AreEqual(0, pvpTestContext.GetOpponentPlayer().ExtraGoo);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Blizzard()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 2,
                    new DeckCardData("Blizzard", 1),
                    new DeckCardData("Slab", 20));

                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 2,
                    new DeckCardData("Blizzard", 1),
                    new DeckCardData("Slab", 20));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerBlizzardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Blizzard", 1);
                InstanceId playerSlabId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Slab", 1);
                InstanceId playerSlab2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Slab", 2);
                InstanceId opponentBlizzardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Blizzard", 1);
                InstanceId opponentSlabId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Slab", 1);
                InstanceId opponentSlab2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Slab", 2);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                    player => {},
                    opponent => {},
                    player => {},
                    opponent => {},
                    player => {},
                    opponent =>
                    {
                        opponent.CardPlay(opponentSlabId, ItemPosition.Start);
                        opponent.CardPlay(opponentSlab2Id, ItemPosition.Start);
                    },
                    player =>
                    {
                        player.CardPlay(playerSlabId, ItemPosition.Start);
                        player.CardPlay(playerSlab2Id, ItemPosition.Start);
                    },
                    opponent =>
                    {
                        opponent.CardPlay(opponentBlizzardId, ItemPosition.Start);
                    },
                    player =>
                    {
                        player.CardPlay(playerBlizzardId, ItemPosition.Start);
                    },
                    opponent =>
                    {
                        Assert.IsTrue(((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerSlabId)).IsStun);
                        Assert.IsTrue(((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerSlab2Id)).IsStun);
                        Assert.IsTrue(((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentSlabId)).IsStun);
                        Assert.IsTrue(((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentSlab2Id)).IsStun);
                    },
                };

                Action validateEndState = () => {};

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Zplash()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 2,
                    new DeckCardData("Zplash", 1),
                    new DeckCardData("Brook", 4),
                    new DeckCardData("Slab", 12));

                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 2,
                    new DeckCardData("Zplash", 1),
                    new DeckCardData("Brook", 2),
                    new DeckCardData("Slab", 14));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerZplashId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zplash", 1);
                InstanceId playerSlabId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Slab", 1);
                InstanceId playerSlab2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Slab", 2);

                InstanceId opponentZplashId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zplash", 1);
                InstanceId opponentSlabId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Slab", 1);

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
                    player => {},
                    opponent => {},
                    player =>
                    {
                        player.CardPlay(playerSlabId, ItemPosition.Start);
                        player.CardPlay(playerSlab2Id, ItemPosition.Start);
                    },
                    opponent => {},
                    player =>
                    {
                        player.CardPlay(playerZplashId, ItemPosition.Start);
                    },
                    opponent =>
                    {
                        opponent.CardPlay(opponentZplashId, ItemPosition.Start);
                        opponent.CardAbilityUsed(opponentZplashId, Enumerators.AbilityType.TAKE_DAMAGE_RANDOM_ENEMY, new List<ParametrizedAbilityInstanceId>());
                    },
                    player => {},
                    opponent => {},
                };

                Action validateEndState = () =>
                {
                    Assert.IsNull(TestHelper.BattlegroundController.GetBoardUnitModelByInstanceId(playerSlabId));
                    Assert.AreEqual(pvpTestContext.GetOpponentPlayer().InitialDefense - 4, pvpTestContext.GetOpponentPlayer().Defense);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator ZubZero()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 2,
                    new DeckCardData("Zub-Zero", 1),
                    new DeckCardData("Slab", 15));

                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 2,
                    new DeckCardData("Zub-Zero", 1),
                    new DeckCardData("Slab", 15));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerSlabId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Slab", 1);
                InstanceId playerSlab2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Slab", 2);
                InstanceId playerZubZeroId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zub-Zero", 1);

                InstanceId opponentSlabId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Slab", 1);
                InstanceId opponentZubZeroId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zub-Zero", 1);

                int difference = 3;

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                   {
                       player => {},
                       opponent => {},
                       player => {},
                       opponent => {},
                       player => {},
                       opponent => {},
                       player =>
                       {
                           player.CardPlay(playerSlabId, ItemPosition.Start);
                           player.CardPlay(playerSlab2Id, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentSlabId, ItemPosition.Start);
                       },
                       player =>
                       {
                           player.CardPlay(playerZubZeroId, ItemPosition.Start, opponentSlabId);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentZubZeroId, ItemPosition.Start);
                           opponent.CardAbilityUsed(opponentZubZeroId, Enumerators.AbilityType.DAMAGE_TARGET_FREEZE_IT_IF_SURVIVES, new List<ParametrizedAbilityInstanceId>()
                           {
                               new ParametrizedAbilityInstanceId(playerSlabId)
                           });
                       },
                   };

                Action validateEndState = () =>
                {
                    BoardUnitModel playerUnit1 = (BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerSlabId);
                    BoardUnitModel opponentUnit1 = (BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentSlabId);

                    Assert.IsTrue(playerUnit1.IsStun);
                    Assert.AreEqual(playerUnit1.Card.Prototype.Defense - difference, playerUnit1.CurrentDefense);

                    Assert.IsTrue(opponentUnit1.IsStun);
                    Assert.AreEqual(opponentUnit1.Card.Prototype.Defense - difference, opponentUnit1.CurrentDefense);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Tzunamy()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 2,
                    new DeckCardData("Tzunamy", 1),
                    new DeckCardData("Slab", 15));

                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 2,
                    new DeckCardData("Tzunamy", 1),
                    new DeckCardData("Slab", 15));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerSlabId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Slab", 1);
                InstanceId playerSlab2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Slab", 2);
                InstanceId playerSlab3Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Slab", 3);
                InstanceId playerSlab4Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Slab", 4);
                InstanceId playerTzunamyId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Tzunamy", 1);

                InstanceId opponentSlabId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Slab", 1);
                InstanceId opponentSlab2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Slab", 2);
                InstanceId opponentSlab3Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Slab", 3);
                InstanceId opponentSlab4Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Slab", 4);
                InstanceId opponentTzunamyId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Tzunamy", 1);

                int difference = 3;

                BoardUnitModel playerSlab = null;
                BoardUnitModel playerSlab2 = null;
                BoardUnitModel playerSlab3 = null;
                BoardUnitModel playerSlab4 = null;
                BoardUnitModel opponentSlab = null;
                BoardUnitModel opponentSlab2 = null;
                BoardUnitModel opponentSlab3 = null;
                BoardUnitModel opponentSlab4 = null;

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                   {
                       player => {},
                       opponent => {},
                       player => {},
                       opponent => {},
                       player => {},
                       opponent => {},
                       player =>
                       {
                           player.CardPlay(playerSlabId, ItemPosition.Start);
                           player.CardPlay(playerSlab2Id, ItemPosition.Start);
                           player.CardPlay(playerSlab3Id, ItemPosition.Start);
                           player.CardPlay(playerSlab4Id, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentSlabId, ItemPosition.Start);
                           opponent.CardPlay(opponentSlab2Id, ItemPosition.Start);
                           opponent.CardPlay(opponentSlab3Id, ItemPosition.Start);
                           opponent.CardPlay(opponentSlab4Id, ItemPosition.Start);
                       },
                       player =>
                       {
                           playerSlab = (BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerSlabId);
                           playerSlab2 = (BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerSlab2Id);
                           playerSlab3 = (BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerSlab3Id);
                           playerSlab4 = (BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerSlab4Id);
                           opponentSlab = (BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentSlabId);
                           opponentSlab2 = (BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentSlab2Id);
                           opponentSlab3 = (BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentSlab3Id);
                           opponentSlab4 = (BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentSlab4Id);

                           player.CardPlay(playerTzunamyId, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentTzunamyId, ItemPosition.Start);
                           //opponent.CardAbilityUsed(opponentTzunamyId, Enumerators.AbilityType.DAMAGE_ENEMY_UNITS_AND_FREEZE_THEM, new List<ParametrizedAbilityInstanceId>());
                       },
                       player =>
                       {
                           Assert.IsTrue(opponentSlab.IsStun);
                           Assert.IsTrue(opponentSlab2.IsStun);
                           Assert.IsTrue(opponentSlab3.IsStun);
                           Assert.IsTrue(opponentSlab4.IsStun);
                       },
                       opponent =>
                       {
                           Assert.IsTrue(playerSlab.IsStun);
                           Assert.IsTrue(playerSlab2.IsStun);
                           Assert.IsTrue(playerSlab3.IsStun);
                           Assert.IsTrue(playerSlab4.IsStun);
                       },
                   };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(playerSlab.Card.Prototype.Defense - difference, playerSlab.CurrentDefense);
                    Assert.AreEqual(playerSlab2.Card.Prototype.Defense - difference, playerSlab2.CurrentDefense);
                    Assert.AreEqual(playerSlab3.Card.Prototype.Defense - difference, playerSlab3.CurrentDefense);
                    Assert.AreEqual(playerSlab4.Card.Prototype.Defense - difference, playerSlab4.CurrentDefense);
                    
                    Assert.AreEqual(opponentSlab.Card.Prototype.Defense - difference, opponentSlab.CurrentDefense);
                    Assert.AreEqual(opponentSlab2.Card.Prototype.Defense - difference, opponentSlab2.CurrentDefense);
                    Assert.AreEqual(opponentSlab3.Card.Prototype.Defense - difference, opponentSlab3.CurrentDefense);
                    Assert.AreEqual(opponentSlab4.Card.Prototype.Defense - difference, opponentSlab4.CurrentDefense);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Vortex()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 2,
                    new DeckCardData("Vortex", 1),
                    new DeckCardData("Znowy", 15));

                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 2,
                    new DeckCardData("Vortex", 1),
                    new DeckCardData("Znowy", 15));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerZnowyId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Znowy", 1);
                InstanceId playerZnowy2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Znowy", 2);
                InstanceId playerZnowy3Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Znowy", 3);
                InstanceId playerZnowy4Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Znowy", 4);
                InstanceId playerVortexId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Vortex", 1);

                InstanceId opponentZnowyId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Znowy", 1);
                InstanceId opponentZnowy2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Znowy", 2);
                InstanceId opponentZnowy3Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Znowy", 3);
                InstanceId opponentZnowy4Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Znowy", 4);
                InstanceId opponentVortexId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Vortex", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                   {
                       player => {},
                       opponent => {},
                       player => {},
                       opponent => {},
                       player => {},
                       opponent => {},
                       player =>
                       {
                           player.CardPlay(playerZnowyId, ItemPosition.Start);
                           player.CardPlay(playerZnowy2Id, ItemPosition.Start);
                           player.CardPlay(playerZnowy3Id, ItemPosition.Start);
                           player.CardPlay(playerZnowy4Id, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentZnowy4Id, ItemPosition.Start);
                           opponent.CardPlay(opponentZnowy3Id, ItemPosition.Start);
                           opponent.CardPlay(opponentZnowy2Id, ItemPosition.Start);
                           opponent.CardPlay(opponentZnowyId, ItemPosition.Start);
                       },
                       player =>
                       {
                           player.CardPlay(playerVortexId, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentVortexId, ItemPosition.Start);
                           opponent.CardAbilityUsed(opponentVortexId, Enumerators.AbilityType.REPLACE_UNITS_WITH_TYPE_ON_STRONGER_ONES, new List<ParametrizedAbilityInstanceId>()
                           {
                               new ParametrizedAbilityInstanceId(opponentZnowyId,
                                   new ParametrizedAbilityParameters()
                                   {
                                       CardName = "FroZen"
                                   }),
                               new ParametrizedAbilityInstanceId(opponentZnowy2Id,
                                   new ParametrizedAbilityParameters()
                                   {
                                       CardName = "HoZer"
                                   }),
                               new ParametrizedAbilityInstanceId(opponentZnowy3Id,
                                   new ParametrizedAbilityParameters()
                                   {
                                       CardName = "Znowman"
                                   }),
                               new ParametrizedAbilityInstanceId(opponentZnowy4Id,
                                   new ParametrizedAbilityParameters()
                                   {
                                       CardName = "Brook"
                                   }),
                           });
                       },
                       player => {},
                       opponent => {},
                   };

                Action validateEndState = () =>
                {
                    Assert.IsNotNull(TestHelper.GameplayManager.OpponentPlayer.CardsOnBoard.Select(unit => unit.Card.Prototype.Name == "FroZen"));
                    Assert.IsNotNull(TestHelper.GameplayManager.OpponentPlayer.CardsOnBoard.Select(unit => unit.Card.Prototype.Name == "HoZer"));
                    Assert.IsNotNull(TestHelper.GameplayManager.OpponentPlayer.CardsOnBoard.Select(unit => unit.Card.Prototype.Name == "Znowman"));
                    Assert.IsNotNull(TestHelper.GameplayManager.OpponentPlayer.CardsOnBoard.Select(unit => unit.Card.Prototype.Name == "Brook"));
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false, true, true);
            });
        }
    }
}
