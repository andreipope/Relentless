using System;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using UnityEngine.TestTools;

namespace Loom.ZombieBattleground.Test.MultiplayerTests
{
    public class ItemsCardsTests : BaseIntegrationTest
    {
        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Boomstick()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0,
                    new DeckCardData("Boomstick", 1),
                    new DeckCardData("Slab", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    new DeckCardData("Boomstick", 1),
                    new DeckCardData("Slab", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerSlabId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Slab", 1);
                InstanceId playerSlab2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Slab", 2);
                InstanceId playerSlab3Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Slab", 3);
                InstanceId playerSlab4Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Slab", 4);
                InstanceId playerBoomstickId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Boomstick", 1);

                InstanceId opponentSlabId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Slab", 1);
                InstanceId opponentSlab2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Slab", 2);
                InstanceId opponentSlab3Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Slab", 3);
                InstanceId opponentSlab4Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Slab", 4);
                InstanceId opponentBoomstickId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Boomstick", 1);
                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                   {
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
                       player => player.CardPlay(playerBoomstickId, ItemPosition.Start),
                       opponent => opponent.CardPlay(opponentBoomstickId, ItemPosition.Start),
                   };

                Action validateEndState = () =>
                {
                    Assert.IsTrue(pvpTestContext.GetCurrentPlayer().BoardCards.FindAll(card => card.Model.CurrentHp == card.Model.InitialHp - 2).Count == 0);
                    Assert.IsTrue(pvpTestContext.GetOpponentPlayer().BoardCards.FindAll(card => card.Model.CurrentHp == card.Model.InitialHp - 2).Count == 0);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }
               

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Stapler()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0,
                    new DeckCardData("Stapler", 1),
                    new DeckCardData("Earthshaker", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    new DeckCardData("Stapler", 1),
                    new DeckCardData("Earthshaker", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerEarthshakerId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Earthshaker", 1);
                InstanceId playerStaplerId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Stapler", 1);

                InstanceId opponentEarthshakerId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Earthshaker", 1);
                InstanceId opponentStaplerId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Stapler", 1);
                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                   {
                       player => {},
                       opponent => {},
                       player => {},
                       opponent => {},
                       player => player.CardPlay(playerEarthshakerId, ItemPosition.Start),
                       opponent => opponent.CardPlay(opponentEarthshakerId, ItemPosition.Start),
                       player =>
                       {
                           player.CardAttack(playerEarthshakerId, opponentEarthshakerId);
                           player.CardPlay(playerStaplerId, ItemPosition.Start, playerEarthshakerId);
                       },
                       opponent => opponent.CardPlay(opponentStaplerId, ItemPosition.Start, opponentEarthshakerId),
                   };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(5, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerEarthshakerId)).CurrentHp);
                    Assert.AreEqual(5, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentEarthshakerId)).CurrentHp);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }



        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Chainsaw()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0,
                    new DeckCardData("Chainsaw", 1),
                    new DeckCardData("Earthshaker", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    new DeckCardData("Chainsaw", 1),
                    new DeckCardData("Earthshaker", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerEarthshakerId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Earthshaker", 1);
                InstanceId playerChainsawId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Chainsaw", 1);

                InstanceId opponentEarthshakerId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Earthshaker", 1);
                InstanceId opponentChainsawId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Chainsaw", 1);
                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                   {
                       player => {},
                       opponent => {},
                       player => {},
                       opponent => {},
                       player => player.CardPlay(playerEarthshakerId, ItemPosition.Start),
                       opponent => opponent.CardPlay(opponentEarthshakerId, ItemPosition.Start),
                       player =>
                       {
                           player.CardPlay(playerChainsawId, ItemPosition.Start, playerEarthshakerId);
                           player.CardAttack(playerEarthshakerId, TestHelper.GetOpponentPlayer().InstanceId);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentChainsawId, ItemPosition.Start, opponentEarthshakerId);
                           opponent.CardAttack(opponentEarthshakerId, TestHelper.GetCurrentPlayer().InstanceId);
                       },
                   };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(1, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerEarthshakerId)).CurrentHp);
                    Assert.AreEqual(1, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentEarthshakerId)).CurrentHp);
                    Assert.AreEqual(TestHelper.GetCurrentPlayer().InitialHp - 8, TestHelper.GetCurrentPlayer().Defense);
                    Assert.AreEqual(TestHelper.GetOpponentPlayer().InitialHp - 8, TestHelper.GetOpponentPlayer().Defense);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Torch()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0,
                    new DeckCardData("Torch", 1),
                    new DeckCardData("Slab", 1),
                    new DeckCardData("Pyromaz", 1),
                    new DeckCardData("Brook", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    new DeckCardData("Torch", 1),
                    new DeckCardData("Slab", 1),
                    new DeckCardData("Pyromaz", 1),
                    new DeckCardData("Brook", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerSlabId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Slab", 1);
                InstanceId playerPyromazId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Pyromaz", 2);
                InstanceId playerBrookId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Brook", 3);
                InstanceId playerTorchId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Torch", 1);

                InstanceId opponentSlabId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Slab", 1);
                InstanceId opponentPyromazId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Pyromaz", 2);
                InstanceId opponentBrookId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Brook", 3);
                InstanceId opponentTorchId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Torch", 1);
                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                   {
                       player => {},
                       opponent => {},
                       player => {},
                       opponent => {},
                       player =>
                       {
                           player.CardPlay(playerSlabId, ItemPosition.Start);
                           player.CardPlay(playerPyromazId, ItemPosition.Start);
                           player.CardPlay(playerBrookId, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentSlabId, ItemPosition.Start);
                           opponent.CardPlay(opponentPyromazId, ItemPosition.Start);
                           opponent.CardPlay(opponentBrookId, ItemPosition.Start);
                       },
                       player => player.CardPlay(playerTorchId, ItemPosition.Start),
                       opponent => opponent.CardPlay(opponentTorchId, ItemPosition.Start),
                       player => {},
                       opponent => {},
                   };

                Action validateEndState = () =>
                {
                    Assert.IsTrue(pvpTestContext.GetCurrentPlayer().BoardCards.FindAll(card => card.Model.Card.LibraryCard.Cost < 4).Count == 1);
                    Assert.IsTrue(pvpTestContext.GetOpponentPlayer().BoardCards.FindAll(card => card.Model.Card.LibraryCard.Cost < 4).Count == 1);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Extinguisher()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0,
                    new DeckCardData("Extinguisher", 1),
                    new DeckCardData("Slab", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    new DeckCardData("Extinguisher", 1),
                    new DeckCardData("Slab", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerSlabId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Slab", 1);
                InstanceId playerSlab2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Slab", 2);
                InstanceId playerExtinguisherId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Extinguisher", 1);

                InstanceId opponentSlabId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Slab", 1);
                InstanceId opponentSlab2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Slab", 2);
                InstanceId opponentExtinguisherId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Extinguisher", 1);
                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                   {
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
                           opponent.CardPlay(opponentSlab2Id, ItemPosition.Start);
                       },
                       player => player.CardPlay(playerExtinguisherId, ItemPosition.Start),
                       opponent => opponent.CardPlay(opponentExtinguisherId, ItemPosition.Start),
                   };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(pvpTestContext.GetCurrentPlayer().BoardCards.Count,
                                    pvpTestContext.GetCurrentPlayer().BoardCards.FindAll(card => card.Model.IsStun == true).Count);
                    Assert.AreEqual(pvpTestContext.GetOpponentPlayer().BoardCards.Count,
                                    pvpTestContext.GetOpponentPlayer().BoardCards.FindAll(card => card.Model.IsStun == true).Count);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Bulldozer()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0,
                    new DeckCardData("Bulldozer", 1),
                    new DeckCardData("Slab", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    new DeckCardData("Bulldozer", 1),
                    new DeckCardData("Slab", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerSlabId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Slab", 1);
                InstanceId playerSlab2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Slab", 2);
                InstanceId playerSlab3Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Slab", 3);
                InstanceId playerSlab4Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Slab", 4);
                InstanceId playerBulldozerId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Bulldozer", 1);

                InstanceId opponentSlabId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Slab", 1);
                InstanceId opponentSlab2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Slab", 2);
                InstanceId opponentSlab3Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Slab", 3);
                InstanceId opponentSlab4Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Slab", 4);
                InstanceId opponentBulldozerId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Bulldozer", 1);
                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                   {
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
                       player => player.CardPlay(playerBulldozerId, ItemPosition.Start),
                       opponent => opponent.CardPlay(opponentBulldozerId, ItemPosition.Start),
                       player => {},
                       opponent => {},
                   };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(0, pvpTestContext.GetCurrentPlayer().BoardCards.Count);
                    Assert.AreEqual(0, pvpTestContext.GetOpponentPlayer().BoardCards.Count);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Leash()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0,
                    new DeckCardData("Leash", 1),
                    new DeckCardData("Slab", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    new DeckCardData("Leash", 1),
                    new DeckCardData("Slab", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerSlabId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Slab", 1);
                InstanceId playerSlab2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Slab", 2);
                InstanceId playerLeashId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Leash", 1);

                InstanceId opponentSlabId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Slab", 1);
                InstanceId opponentSlab2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Slab", 2);
                InstanceId opponentLeashId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Leash", 1);
                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                   {
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
                           opponent.CardPlay(opponentSlab2Id, ItemPosition.Start);
                       },
                       player => player.CardPlay(playerLeashId, ItemPosition.Start),
                       opponent => opponent.CardPlay(opponentLeashId, ItemPosition.Start),
                       player => {},
                       opponent => {},
                   };

                Action validateEndState = () =>
                {
                    BoardUnitModel playerSlabUnit = (BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerSlabId);
                    BoardUnitModel playerSlab2Unit = (BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerSlab2Id);
                    BoardUnitModel opponentSlabUnit = (BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentSlabId);
                    BoardUnitModel opponentSlab2Unit = (BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentSlab2Id);

                    ;

                    Assert.IsTrue(pvpTestContext.GetCurrentPlayer().BoardCards.Contains(TestHelper.BattlegroundController.GetBoardUnitViewByModel(opponentSlabUnit))
                        || pvpTestContext.GetCurrentPlayer().BoardCards.Contains(TestHelper.BattlegroundController.GetBoardUnitViewByModel(opponentSlab2Unit)));
                    Assert.IsTrue(pvpTestContext.GetOpponentPlayer().BoardCards.Contains(TestHelper.BattlegroundController.GetBoardUnitViewByModel(opponentSlabUnit))
                        || pvpTestContext.GetOpponentPlayer().BoardCards.Contains(TestHelper.BattlegroundController.GetBoardUnitViewByModel(opponentSlab2Unit)));
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }


        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Whistle()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0,
                   new DeckCardData("Whistle", 10)
               );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    new DeckCardData("Whistle", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Whistle", 1);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Whistle", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                    player =>
                    {
                        player.CardPlay(playerCardId, ItemPosition.Start);
                        //player.CardAbilityUsed(playerCardId, Enumerators.AbilityType.DRAW_CARD, new List<ParametrizedAbilityInstanceId>());
                    },
                    opponent => {},
                    player => {},
                    opponent => opponent.CardPlay(opponentCardId, ItemPosition.Start),
                   /* opponent =>
                    {
                        opponent.CardPlay(opponentCardId, ItemPosition.Start);
                        //opponent.CardAbilityUsed(opponentCardId, Enumerators.AbilityType.DRAW_CARD, new List<ParametrizedAbilityInstanceId>());
                    }*/
                };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(5, pvpTestContext.GetCurrentPlayer().CardsInHand.Count);
                    //Assert.AreEqual(5, pvpTestContext.GetOpponentPlayer().CardsInHand.Count);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }


        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator GooBeaker()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 1, new DeckCardData("Goo Beaker", 6));
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 1, new DeckCardData("Goo Beaker", 6));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Goo Beaker", 1);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Goo Beaker", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                    player =>
                    {
                        player.CardPlay(playerCardId, ItemPosition.Start);
                        player.CardAbilityUsed(playerCardId, Enumerators.AbilityType.ADD_GOO_VIAL, new List<ParametrizedAbilityInstanceId>());
                    },
                    opponent =>
                    {
                        Assert.AreEqual(3, pvpTestContext.GetCurrentPlayer().GooVials);
                        opponent.CardPlay(opponentCardId, ItemPosition.Start);
                        opponent.CardAbilityUsed(opponentCardId, Enumerators.AbilityType.ADD_GOO_VIAL, new List<ParametrizedAbilityInstanceId>());
                    },
                    player => Assert.AreEqual(3, pvpTestContext.GetOpponentPlayer().GooVials),
                    opponent => {}
                };

                Action validateEndState = () =>
                {
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }


        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator GooBottles()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 1, new DeckCardData("Goo Bottles", 6));
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 1, new DeckCardData("Goo Bottles", 6));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Goo Bottles", 1);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Goo Bottles", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                    player =>
                    {
                        player.CardPlay(playerCardId, ItemPosition.Start);
                        player.CardAbilityUsed(playerCardId, Enumerators.AbilityType.ADD_GOO_VIAL, new List<ParametrizedAbilityInstanceId>());
                    },
                    opponent =>
                    {
                        Assert.AreEqual(4, pvpTestContext.GetCurrentPlayer().GooVials);
                        opponent.CardPlay(opponentCardId, ItemPosition.Start);
                        opponent.CardAbilityUsed(opponentCardId, Enumerators.AbilityType.ADD_GOO_VIAL, new List<ParametrizedAbilityInstanceId>());
                    },
                    player => Assert.AreEqual(4, pvpTestContext.GetOpponentPlayer().GooVials),
                    opponent => {}
                };

                Action validateEndState = () =>
                {
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }


        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator SuperSerum()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0,
                    new DeckCardData("Super Serum", 1),
                    new DeckCardData("Earthshaker", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    new DeckCardData("Super Serum", 1),
                    new DeckCardData("Earthshaker", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerEarthshakerId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Earthshaker", 1);
                InstanceId playerSuperSerumId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Super Serum", 1);

                InstanceId opponentEarthshakerId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Earthshaker", 1);
                InstanceId opponentSuperSerumId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Super Serum", 1);
                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                   {
                       player => {},
                       opponent => {},
                       player => {},
                       opponent => {},
                       player => player.CardPlay(playerEarthshakerId, ItemPosition.Start),
                       opponent => opponent.CardPlay(opponentEarthshakerId, ItemPosition.Start),
                       player =>
                       {
                           player.CardPlay(playerSuperSerumId, ItemPosition.Start, playerEarthshakerId);
                           player.CardAttack(playerEarthshakerId, TestHelper.GetOpponentPlayer().InstanceId);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentSuperSerumId, ItemPosition.Start, opponentEarthshakerId);
                           opponent.CardAttack(opponentEarthshakerId, TestHelper.GetCurrentPlayer().InstanceId);
                       },
                   };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(5, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerEarthshakerId)).CurrentHp);
                    Assert.AreEqual(5, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerEarthshakerId)).CurrentDamage);
                    Assert.AreEqual(7, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentEarthshakerId)).CurrentHp);
                    Assert.AreEqual(7, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentEarthshakerId)).CurrentDamage);
                    Assert.AreEqual(TestHelper.GetCurrentPlayer().InitialHp - 7, TestHelper.GetCurrentPlayer().Defense);
                    Assert.AreEqual(TestHelper.GetOpponentPlayer().InitialHp - 7, TestHelper.GetOpponentPlayer().Defense);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }


        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Harpoon()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 1, new DeckCardData("Harpoon", 6));
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 1, new DeckCardData("Harpoon", 6));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Harpoon", 1);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Harpoon", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                    player =>
                    {
                        player.CardPlay(playerCardId, ItemPosition.Start, pvpTestContext.GetOpponentPlayer().InstanceId);
                    },
                    opponent =>
                    {
                        opponent.CardPlay(opponentCardId, ItemPosition.Start, pvpTestContext.GetCurrentPlayer().InstanceId);
                    },
                };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(pvpTestContext.GetCurrentPlayer().Defense - 3, pvpTestContext.GetCurrentPlayer().Defense);
                    Assert.AreEqual(pvpTestContext.GetOpponentPlayer().Defense - 3, pvpTestContext.GetOpponentPlayer().Defense);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Bazooka()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 1, new DeckCardData("Bazooka", 6));
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 1, new DeckCardData("Bazooka", 6));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Bazooka", 1);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Bazooka", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                    player =>
                    {
                        player.CardPlay(playerCardId, ItemPosition.Start, pvpTestContext.GetOpponentPlayer().InstanceId);
                    },
                    opponent =>
                    {
                        opponent.CardPlay(opponentCardId, ItemPosition.Start, pvpTestContext.GetCurrentPlayer().InstanceId);
                    },
                };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(pvpTestContext.GetCurrentPlayer().InitialHp - 5, pvpTestContext.GetCurrentPlayer().Defense);
                    Assert.AreEqual(pvpTestContext.GetOpponentPlayer().InitialHp - 5, pvpTestContext.GetOpponentPlayer().Defense);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator FreshMeat()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0,
                    new DeckCardData("Fresh Meat", 1),
                    new DeckCardData("Earthshaker", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    new DeckCardData("Fresh Meat", 1),
                    new DeckCardData("Earthshaker", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerEarthshakerId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Earthshaker", 1);
                InstanceId playerEarthshaker2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Earthshaker", 2);
                InstanceId playerEarthshaker3Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Earthshaker", 3);
                InstanceId playerFreshMeatId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Fresh Meat", 1);

                InstanceId opponentEarthshakerId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Earthshaker", 1);
                InstanceId opponentEarthshaker2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Earthshaker", 2);
                InstanceId opponentEarthshaker3Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Earthshaker", 3);
                InstanceId opponentFreshMeatId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Fresh Meat", 1);
                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                   {
                       player => {},
                       opponent => {},
                       player => {},
                       opponent => {},
                       player =>
                       {
                           player.CardPlay(playerEarthshakerId, ItemPosition.Start);
                           player.CardPlay(playerEarthshaker2Id, ItemPosition.Start);
                           player.CardPlay(playerEarthshaker3Id, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(playerEarthshakerId, ItemPosition.Start);
                           opponent.CardPlay(playerEarthshaker2Id, ItemPosition.Start);
                           opponent.CardPlay(playerEarthshaker3Id, ItemPosition.Start);
                       },
                       player =>
                       {
                           player.CardPlay(playerFreshMeatId, ItemPosition.Start);
                           Assert.AreEqual(pvpTestContext.GetCurrentPlayer().BoardCards.Count,
                                        pvpTestContext.GetCurrentPlayer().BoardCards.FindAll(card => card.Model.CurrentDamage == card.Model.Card.LibraryCard.Damage - 3));
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentFreshMeatId, ItemPosition.Start);
                           Assert.AreEqual(pvpTestContext.GetOpponentPlayer().BoardCards.Count,
                                        pvpTestContext.GetOpponentPlayer().BoardCards.FindAll(card => card.Model.CurrentDamage == card.Model.Card.LibraryCard.Damage - 3));
                       }
                   };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(pvpTestContext.GetCurrentPlayer().BoardCards.Count,
                                    pvpTestContext.GetCurrentPlayer().BoardCards.FindAll(card => card.Model.CurrentDamage == card.Model.Card.LibraryCard.Damage).Count);
                    Assert.AreEqual(pvpTestContext.GetOpponentPlayer().BoardCards.Count,
                                    pvpTestContext.GetOpponentPlayer().BoardCards.FindAll(card => card.Model.CurrentDamage == card.Model.Card.LibraryCard.Damage).Count);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Lawnmower()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0,
                    new DeckCardData("Lawnmower", 1),
                    new DeckCardData("Shale", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    new DeckCardData("Lawnmower", 1),
                    new DeckCardData("Shale", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerShaleId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Shale", 1);
                InstanceId playerShale2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Shale", 2);
                InstanceId playerLawnmowerId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Lawnmower", 1);

                InstanceId opponentShaleId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Shale", 1);
                InstanceId opponentShale2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Shale", 2);
                InstanceId opponentLawnmowerId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Lawnmower", 1);
                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                   {
                       player => {},
                       opponent => {},
                       player => {},
                       opponent => {},
                       player =>
                       {
                           player.CardPlay(playerShaleId, ItemPosition.Start);
                           player.CardPlay(playerShale2Id, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentShaleId, ItemPosition.Start);
                           opponent.CardPlay(opponentShale2Id, ItemPosition.Start);
                       },
                       player => player.CardPlay(playerLawnmowerId, ItemPosition.Start),
                       opponent => opponent.CardPlay(opponentLawnmowerId, ItemPosition.Start),
                   };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(0, pvpTestContext.GetCurrentPlayer().BoardCards.Count);
                    Assert.AreEqual(0, pvpTestContext.GetOpponentPlayer().BoardCards.Count);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator ShoppingCart()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0,
                    new DeckCardData("Shopping Cart", 1),
                    new DeckCardData("Slab", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    new DeckCardData("Shopping Cart", 1),
                    new DeckCardData("Slab", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerSlabId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Slab", 1);
                InstanceId playerSlab2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Slab", 2);
                InstanceId playerShoppingCartId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Shopping Cart", 1);

                InstanceId opponentSlabId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Slab", 1);
                InstanceId opponentSlab2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Slab", 2);
                InstanceId opponentShoppingCartId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Shopping Cart", 1);
                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                   {
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
                           opponent.CardPlay(opponentSlab2Id, ItemPosition.Start);
                       },
                       player => player.CardPlay(playerShoppingCartId, ItemPosition.Start),
                       opponent => opponent.CardPlay(opponentShoppingCartId, ItemPosition.Start),
                   };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(pvpTestContext.GetCurrentPlayer().BoardCards.Count,
                                    pvpTestContext.GetCurrentPlayer().BoardCards.FindAll(card => card.Model.HasSwing == true).Count);
                    Assert.AreEqual(pvpTestContext.GetOpponentPlayer().BoardCards.Count,
                                    pvpTestContext.GetOpponentPlayer().BoardCards.FindAll(card => card.Model.HasSwing == true).Count);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }
    }
}
