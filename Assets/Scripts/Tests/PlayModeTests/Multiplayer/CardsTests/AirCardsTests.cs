using System;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using UnityEngine.TestTools;

namespace Loom.ZombieBattleground.Test.MultiplayerTests
{
    public class AirCardsTests : BaseIntegrationTest
    {
        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator MindFlayer()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 2, new DeckCardData[]
                {
                    new DeckCardData("Mind Flayer", 1),
                    new DeckCardData("Pyromaz", 20),
                });
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 2, new DeckCardData[]
                {
                    new DeckCardData("Mind Flayer", 1),
                    new DeckCardData("Wood", 20),
                });

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerPyromazId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Pyromaz", 1);
                InstanceId playerMindFlayerId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Mind Flayer", 1);

                InstanceId opponentWoodId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Wood", 1);
                InstanceId opponentWood2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Wood", 2);
                InstanceId opponentWood3Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Wood", 3);
                InstanceId opponentWood4Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Wood", 4);
                InstanceId opponentMindFlayerId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Mind Flayer", 1);
                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                   {
                       player => {},
                       opponent => {},
                       player =>
                       {
                           player.CardPlay(playerPyromazId, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentWoodId, ItemPosition.Start);
                       },
                       player =>
                       {
                           player.CardPlay(playerMindFlayerId, ItemPosition.Start);
                           player.CardAbilityUsed(playerMindFlayerId, Enumerators.AbilityType.TAKE_CONTROL_ENEMY_UNIT, new List<ParametrizedAbilityInstanceId>()
                           {  
                               new ParametrizedAbilityInstanceId(opponentWoodId)
                           });
                           player.LetsThink(2);
                           player.AssertInQueue(() => {
                                Assert.IsTrue(TestHelper.GetCurrentPlayer().CardsOnBoard.FindAll(card => card.Card.Prototype.MouldId == 251).Count > 0);
                           });
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentWood2Id, ItemPosition.Start);
                           opponent.CardPlay(opponentWood3Id, ItemPosition.Start);
                           opponent.CardPlay(opponentWood4Id, ItemPosition.Start);
                           opponent.LetsThink(2);
                           opponent.CardAttack(opponentWood2Id, playerMindFlayerId);
                           opponent.CardAttack(opponentWood3Id, playerMindFlayerId);
                           opponent.CardAttack(opponentWood4Id, opponentWoodId);
                           opponent.LetsThink(2);
                           opponent.CardPlay(opponentMindFlayerId, ItemPosition.Start);
                           opponent.CardAbilityUsed(opponentMindFlayerId, Enumerators.AbilityType.TAKE_CONTROL_ENEMY_UNIT, new List<ParametrizedAbilityInstanceId>()
                           {  
                               new ParametrizedAbilityInstanceId(playerPyromazId)
                           });
                           opponent.LetsThink(2);
                       },
                   };

                Action validateEndState = () =>
                {
                    Assert.IsTrue(TestHelper.GetOpponentPlayer().CardsOnBoard.FindAll(card => card.Card.Prototype.MouldId == 10).Count > 0);
                };

            await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            }, 300);
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Banshee()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 3,
                    new DeckCardData("Banshee", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 3,
                    new DeckCardData("Banshee", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Banshee", 1);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Banshee", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                       player => player.CardPlay(playerCardId, ItemPosition.Start),
                       opponent => opponent.CardPlay(opponentCardId, ItemPosition.Start),
                       player => {}
                };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(true, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCardId)).HasFeral);
                    Assert.AreEqual(true, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCardId)).HasFeral);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            }, 300);
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Zonic()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 3,
                    new DeckCardData("Zonic", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 3,
                    new DeckCardData("Zonic", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zonic", 1);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zonic", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                       player => player.CardPlay(playerCardId, ItemPosition.Start),
                       opponent => opponent.CardPlay(opponentCardId, ItemPosition.Start),
                       player => player.CardAttack(playerCardId, opponentCardId),
                       opponent => {},
                       player => {}
                };

                Action validateEndState = () =>
                {
                    BoardUnitModel playerUnit = ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCardId));
                    BoardUnitModel opponentUnit = ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCardId));
                    Assert.AreEqual(playerUnit.Card.Prototype.Defense, playerUnit.CurrentDefense);
                    Assert.AreEqual(opponentUnit.Card.Prototype.Defense, opponentUnit.CurrentDefense);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            }, 300);
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator ZeuZ()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0,
                    new DeckCardData("ZeuZ", 1),
                    new DeckCardData("Igloo", 20)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    new DeckCardData("ZeuZ", 1),
                    new DeckCardData("Igloo", 20)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerZeuzId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "ZeuZ", 1);
                InstanceId playerIglooId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Igloo", 1);
                InstanceId opponentZeuzId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "ZeuZ", 1);
                InstanceId opponentIglooId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Igloo", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                       player => 
                       {
                           player.CardPlay(playerIglooId, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentIglooId, ItemPosition.Start);
                           opponent.CardPlay(opponentZeuzId, ItemPosition.Start);
                       },
                       player => 
                       {
                           player.CardPlay(playerZeuzId, ItemPosition.Start);
                       },
                       opponent => {},
                       player => {}
                };

                Action validateEndState = () =>
                {
                    BoardUnitModel playerZeuzUnit = ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerZeuzId));
                    BoardUnitModel playerIglooUnit = ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerIglooId));
                    BoardUnitModel opponentZeuzUnit = ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentZeuzId));
                    BoardUnitModel opponentIglooUnit = ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentIglooId));
                    Assert.AreEqual(playerZeuzUnit.Card.Prototype.Defense, playerZeuzUnit.CurrentDefense);
                    Assert.AreEqual(playerIglooUnit.Card.Prototype.Defense-3, playerIglooUnit.CurrentDefense);
                    Assert.AreEqual(opponentZeuzUnit.Card.Prototype.Defense-3, opponentZeuzUnit.CurrentDefense);
                    Assert.AreEqual(opponentIglooUnit.Card.Prototype.Defense-3, opponentIglooUnit.CurrentDefense);
                    Assert.AreEqual(TestHelper.GetCurrentPlayer().InitialDefense-3, TestHelper.GetCurrentPlayer().Defense);
                    Assert.AreEqual(TestHelper.GetOpponentPlayer().InitialDefense-3, TestHelper.GetOpponentPlayer().Defense);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            }, 400);
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator MonZoon()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0,
                    new DeckCardData("MonZoon", 3),
                    new DeckCardData("Igloo", 20)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    new DeckCardData("MonZoon", 3),
                    new DeckCardData("Igloo", 20)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerMonzoon1Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "MonZoon", 1);
                InstanceId playerMonzoon2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "MonZoon", 2);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                       player =>
                       {
                           player.CardPlay(playerMonzoon1Id, ItemPosition.Start);
                       },
                       opponent => {},
                       player => {}
                };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(7, TestHelper.BattlegroundController.PlayerHandCards.FindAll(x => x.Model.Card.InstanceId == playerMonzoon2Id)[0].Model.Card.InstanceCard.Cost);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            }, 400);
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Zyclone()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0,
                    new DeckCardData("Zyclone", 1),
                    new DeckCardData("Whizpar", 1),
                    new DeckCardData("Banshee", 1),
                    new DeckCardData("Enrager", 20)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    new DeckCardData("Zyclone", 1),
                    new DeckCardData("Whizpar", 1),
                    new DeckCardData("Banshee", 1),
                    new DeckCardData("Enrager", 20)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerZycloneId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zyclone", 1);
                InstanceId playerWhizparId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Whizpar", 1);
                InstanceId playerBansheeId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Banshee", 1);
                InstanceId playerEnragerId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Enrager", 1);
                InstanceId opponentZycloneId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zyclone", 1);
                InstanceId opponentWhizparId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Whizpar", 1);
                InstanceId opponentBansheeId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Banshee", 1);
                InstanceId opponentEnragerId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Enrager", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                       player => {},
                       opponent => {},
                       player =>
                       {
                           player.CardPlay(playerBansheeId, ItemPosition.Start);
                           player.CardPlay(playerWhizparId, ItemPosition.Start);
                           player.CardPlay(playerEnragerId, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentBansheeId, ItemPosition.Start);
                           opponent.CardPlay(opponentWhizparId, ItemPosition.Start);
                           opponent.CardPlay(opponentEnragerId, ItemPosition.Start);
                       },
                       player =>
                       {
                           player.CardPlay(playerZycloneId, ItemPosition.Start);
                       },
                       opponent => {},
                       player =>
                       {
                           player.CardPlay(playerBansheeId, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentBansheeId, ItemPosition.Start);
                       },
                       player => {},
                       opponent => {}
                };

                Action validateEndState = () =>
                {
                    Assert.IsNull(((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerWhizparId)));
                    Assert.IsNull(((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentWhizparId)));
                    Assert.NotNull(((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerEnragerId)));
                    Assert.NotNull(((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentEnragerId)));
                    Assert.NotNull(((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerBansheeId)));
                    Assert.NotNull(((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentBansheeId)));
                    Assert.NotNull(((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerZycloneId)));
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            }, 400);
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Zephyr()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0,
                    new DeckCardData("Zephyr", 1),
                    new DeckCardData("Banshee", 20)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    new DeckCardData("Zephyr", 1),
                    new DeckCardData("Banshee", 20)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerZephyrId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zephyr", 1);
                InstanceId playerBanshee1Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Banshee", 1);
                InstanceId playerBanshee2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Banshee", 2);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                       player =>
                       {
                           player.CardPlay(playerBanshee1Id, ItemPosition.Start);
                           player.CardPlay(playerBanshee2Id, ItemPosition.Start);
                       },
                       opponent => {},
                       player => {}
                };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(3, TestHelper.BattlegroundController.PlayerHandCards.FindAll(x => x.Model.Card.InstanceId == playerZephyrId)[0].Model.Card.InstanceCard.Cost);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            }, 400);
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Whizpar()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0,
                    new DeckCardData("Whizpar", 1),
                    new DeckCardData("Igloo", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    new DeckCardData("Whizpar", 1),
                    new DeckCardData("Igloo", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerWhizparId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Whizpar", 1);
                InstanceId playerIglooId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Igloo", 1);
                InstanceId opponentWhizparId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Whizpar", 1);
                InstanceId opponentIglooId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Igloo", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                       player => {},
                       opponent => {},
                       player => {
                           player.CardPlay(playerWhizparId, ItemPosition.Start);
                           player.CardAbilityUsed(playerWhizparId, Enumerators.AbilityType.MODIFICATOR_STATS, new List<ParametrizedAbilityInstanceId>());
                           player.CardPlay(playerIglooId, ItemPosition.Start);
                       },
                       opponent => {
                           opponent.CardPlay(opponentWhizparId, ItemPosition.Start);
                           opponent.CardAbilityUsed(opponentWhizparId, Enumerators.AbilityType.MODIFICATOR_STATS, new List<ParametrizedAbilityInstanceId>());
                           opponent.CardPlay(opponentIglooId, ItemPosition.Start);
                       },
                       player => {
                           player.CardAttack(playerWhizparId, opponentIglooId);
                       },
                       opponent => {
                           opponent.CardAttack(opponentWhizparId, playerIglooId);
                       },
                       player => {},
                       opponent => {}
                };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(3, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerIglooId)).CurrentDefense);
                    Assert.AreEqual(3, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentIglooId)).CurrentDefense);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            }, 300);
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Buffer()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0,
                    new DeckCardData("Buffer", 1),
                    new DeckCardData("Banshee", 20)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    new DeckCardData("Buffer", 1),
                    new DeckCardData("Banshee", 20)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerBufferId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Buffer", 1);
                InstanceId playerBansheeId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Banshee", 1);
                InstanceId opponentBufferId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Buffer", 1);
                InstanceId opponentBansheeId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Banshee", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                       player => {},
                       opponent => {},
                       player =>
                       {
                           player.CardPlay(playerBansheeId, ItemPosition.Start);
                           player.CardPlay(playerBufferId, ItemPosition.Start);
                           player.CardAbilityUsed(playerBufferId, Enumerators.AbilityType.MODIFICATOR_STATS, new List<ParametrizedAbilityInstanceId>());
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentBansheeId, ItemPosition.Start);
                           opponent.CardPlay(opponentBufferId, ItemPosition.Start);
                           opponent.CardAbilityUsed(opponentBufferId, Enumerators.AbilityType.MODIFICATOR_STATS, new List<ParametrizedAbilityInstanceId>());
                       },
                       player => {
                           player.CardAttack(playerBufferId, opponentBufferId);
                       },
                       opponent => {},
                       player => {}
                };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(3, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerBansheeId)).CurrentDamage);
                    Assert.AreEqual(3, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentBansheeId)).CurrentDamage);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            }, 400);
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Bouncer()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 3, new DeckCardData("Bouncer", 10));
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 3, new DeckCardData("Bouncer", 10));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Bouncer", 1);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Bouncer", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                    player => player.CardPlay(playerCardId, ItemPosition.Start),
                    opponent => opponent.CardPlay(opponentCardId, ItemPosition.Start),
                    player => {}
                };

                Action validateEndState = () =>
                {
                    Assert.IsTrue(((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCardId)).IsHeavyUnit);
                    Assert.IsTrue(((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCardId)).IsHeavyUnit);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            }, 300);
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Pushhh()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 3,
                    new DeckCardData("Pushhh", 1),
                    new DeckCardData("Bouncer", 20)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 3,
                    new DeckCardData("Pushhh", 1),
                    new DeckCardData("Bouncer", 20)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerPushhhId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Pushhh", 1);
                InstanceId playerBouncerId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Bouncer", 1);
                InstanceId opponentPushhhId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Pushhh", 1);
                InstanceId opponentBouncerId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Bouncer", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                       player => player.CardPlay(playerBouncerId, ItemPosition.Start),
                       opponent => opponent.CardPlay(opponentBouncerId, ItemPosition.Start),
                       player => player.CardPlay(playerPushhhId, ItemPosition.Start, opponentBouncerId),
                       opponent => opponent.CardPlay(opponentPushhhId, ItemPosition.Start, playerBouncerId),
                       player => {},
                       opponent => {
                           Assert.AreEqual(1, pvpTestContext.GetCurrentPlayer().CardsOnBoard.Count);
                           Assert.AreEqual(1, pvpTestContext.GetOpponentPlayer().CardsOnBoard.Count);
                       },
                };

                Action validateEndState = () => {};

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            }, 300);
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Whiffer()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 3,
                    new DeckCardData("Whiffer", 1),
                    new DeckCardData("Bouncer", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 3,
                    new DeckCardData("Whiffer", 1),
                    new DeckCardData("Bouncer", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerWhifferId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Whiffer", 1);
                InstanceId playerBouncerId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Bouncer", 1);
                InstanceId opponentWhifferId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Whiffer", 1);
                InstanceId opponentBouncerId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Bouncer", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                       player => player.CardPlay(playerBouncerId, ItemPosition.Start),
                       opponent => opponent.CardPlay(opponentBouncerId, ItemPosition.Start),
                       player => player.CardPlay(playerWhifferId, ItemPosition.Start, playerBouncerId),
                       opponent =>
                       {
                           opponent.CardPlay(opponentWhifferId, ItemPosition.Start, opponentBouncerId);
                           opponent.LetsThink(4);
                           opponent.AssertInQueue(() => {
                                Assert.AreEqual(1, pvpTestContext.GetCurrentPlayer().CardsOnBoard.Count);
                                Assert.AreEqual(1, pvpTestContext.GetOpponentPlayer().CardsOnBoard.Count);
                           });
                       },
                       player => player.CardPlay(playerBouncerId, ItemPosition.Start),
                       opponent => opponent.CardPlay(opponentBouncerId, ItemPosition.Start),
                };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(2, pvpTestContext.GetCurrentPlayer().CardsOnBoard.Count);
                    Assert.AreEqual(2, pvpTestContext.GetOpponentPlayer().CardsOnBoard.Count);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            }, 300);
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Ztormonk()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 3,
                    new DeckCardData("Ztormonk", 2),
                    new DeckCardData("Zludge", 20)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 3,
                    new DeckCardData("Ztormonk", 2),
                    new DeckCardData("Zludge", 20)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerZtormonkId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Ztormonk", 1);
                InstanceId playerZludge1Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zludge", 1);
                InstanceId playerZludge2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zludge", 2);
                InstanceId opponentZtormonkId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Ztormonk", 1);
                InstanceId opponentZludge1Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zludge", 1);
                InstanceId opponentZludge2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zludge", 2);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                    player => player.CardPlay(playerZludge1Id, ItemPosition.Start),
                    opponent => opponent.CardPlay(opponentZludge1Id, ItemPosition.Start),
                    player => player.CardPlay(playerZludge2Id, ItemPosition.End),
                    opponent => opponent.CardPlay(opponentZludge2Id, ItemPosition.Start),
                    player => player.CardPlay(playerZtormonkId, ItemPosition.End),
                    opponent => opponent.CardPlay(opponentZtormonkId, new ItemPosition(1)),
                    player => {},
                    opponent => {}
                };

                Action validateEndState = () =>
                {
                    Assert.IsTrue(((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerZludge2Id)).HasBuffShield);
                    Assert.IsTrue(((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentZludge2Id)).HasBuffShield);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            }, 300);
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Draft()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 3,
                    new DeckCardData("Draft", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 3,
                    new DeckCardData("Draft", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Draft", 1);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Draft", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                       player => player.CardPlay(playerCardId, ItemPosition.Start),
                       opponent => opponent.CardPlay(opponentCardId, ItemPosition.Start),
                };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(1, pvpTestContext.GetCurrentPlayer().CardsOnBoard.Count);
                    Assert.AreEqual(1, pvpTestContext.GetOpponentPlayer().CardsOnBoard.Count);
                    Assert.AreEqual(5, pvpTestContext.GetCurrentPlayer().CardsInHand.Count);
                    Assert.AreEqual(5, pvpTestContext.GetOpponentPlayer().CardsInHand.Count);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            }, 300);
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Gaz()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 3, new DeckCardData("Gaz", 10));
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 3, new DeckCardData("Gaz", 10));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Gaz", 1);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Gaz", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                    player => player.CardPlay(playerCardId, ItemPosition.Start),
                    opponent => opponent.CardPlay(opponentCardId, ItemPosition.Start),
                };

                Action validateEndState = () =>
                {
                    Assert.IsTrue(pvpTestContext.GetCurrentPlayer().CardsInHand.FindAll(x => x.Card.Prototype.MouldId == 156).Count > 0);
                    Assert.IsTrue(pvpTestContext.GetOpponentPlayer().CardsInHand.FindAll(x => x.Card.Prototype.MouldId == 156).Count > 0);
                    Assert.IsTrue(((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCardId)).HasBuffShield);
                    Assert.IsTrue(((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCardId)).HasBuffShield);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            }, 300);
        }

        [UnityTest]
         [Timeout(int.MaxValue)]
         public IEnumerator Wheezy()
         {
             return AsyncTest(async () =>
             {
                  Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 3,
                     new DeckCardData("Wheezy", 1),
                     new DeckCardData("Whizpar", 10)
                  );
                  Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 3,
                     new DeckCardData("Wheezy", 1),
                     new DeckCardData("Whizpar", 10)
                  );

                  PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                  {
                     Player1HasFirstTurn = true
                  };

                  InstanceId playerWheezyId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Wheezy", 1);
                  InstanceId playerWhizparId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Whizpar", 1);
                  InstanceId opponentWheezyId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Wheezy", 1);
                  InstanceId opponentWhizparId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Whizpar", 1);

                  IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                  {
                        player =>
                        {
                            player.CardPlay(playerWheezyId, ItemPosition.Start, skipEntryAbilities: true);
                            player.CardAbilityUsed(playerWheezyId, Common.Enumerators.AbilityType.LOWER_COST_OF_CARD_IN_HAND, new List<ParametrizedAbilityInstanceId>()
                            {
                                new ParametrizedAbilityInstanceId(pvpTestContext.GetOpponentPlayer().InstanceId,
                                    new ParametrizedAbilityParameters()
                                         {
                                             CardName = playerWhizparId.Id.ToString()
                                         }
                                     )
                            });
                        },
                        opponent =>
                        {
                            opponent.CardPlay(opponentWheezyId, ItemPosition.Start, skipEntryAbilities: true);
                            opponent.CardAbilityUsed(opponentWheezyId, Common.Enumerators.AbilityType.LOWER_COST_OF_CARD_IN_HAND, new List<ParametrizedAbilityInstanceId>()
                            {
                                new ParametrizedAbilityInstanceId(pvpTestContext.GetOpponentPlayer().InstanceId,
                                    new ParametrizedAbilityParameters()
                                         {
                                             CardName = opponentWhizparId.Id.ToString()
                                         }
                                     )
                            });
                            opponent.LetsThink(2);
                        }
                  };

                  Action validateEndState = () =>
                  {
                     Assert.IsTrue(pvpTestContext.GetCurrentPlayer().CardsInHand.FindAll(card => card.Card.InstanceCard.Cost < card.Card.Prototype.Cost).Count > 0);
                     Assert.IsTrue(pvpTestContext.GetOpponentPlayer().CardsInHand.FindAll(card => card.Card.InstanceCard.Cost < card.Card.Prototype.Cost).Count > 0);
                  };

                  await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
             }, 300);
         }

          [UnityTest]
         [Timeout(int.MaxValue)]
         public IEnumerator Soothsayer()
         {
             return AsyncTest(async () =>
             {
                  Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 3,
                     new DeckCardData("Soothsayer", 1),
                     new DeckCardData("Slab", 15));
                  Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 3,
                     new DeckCardData("Soothsayer", 1),
                     new DeckCardData("Slab", 15));

                  PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                  {
                     Player1HasFirstTurn = true
                  };

                  InstanceId playerSoothsayerId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Soothsayer", 1);
                  InstanceId opponentSoothsayerId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Soothsayer", 1);

                  IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                  {
                     player => player.CardPlay(playerSoothsayerId, ItemPosition.Start),
                     opponent => opponent.CardPlay(opponentSoothsayerId, ItemPosition.Start)
                  };

                  Action validateEndState = () =>
                  {
                     Assert.AreEqual(5, pvpTestContext.GetCurrentPlayer().CardsInHand.Count);
                     Assert.AreEqual(5, pvpTestContext.GetOpponentPlayer().CardsInHand.Count);
                  };

                  await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
             }, 300);
         }

          [UnityTest]
         [Timeout(int.MaxValue)]
         public IEnumerator FumeZ()
         {
             return AsyncTest(async () =>
             {
                 Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 3,
                     new DeckCardData("FumeZ", 10));
                 Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 3,
                     new DeckCardData("FumeZ", 10));

                  PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                 {
                     Player1HasFirstTurn = true
                 };

                  InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "FumeZ", 1);
                 InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "FumeZ", 1);

                  IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                 {
                     player => player.CardPlay(playerCardId, ItemPosition.Start),
                     opponent =>
                     {
                         opponent.CardPlay(opponentCardId, ItemPosition.Start);
                     },
                 };

                  Action validateEndState = () =>
                 {
                     Assert.IsTrue(pvpTestContext.GetCurrentPlayer().CardsInHand.FindAll(card => card.Card.Prototype.MouldId == 155).Count > 0);
                     Assert.IsTrue(pvpTestContext.GetOpponentPlayer().CardsInHand.FindAll(card => card.Card.Prototype.MouldId == 155).Count > 0);
                 };

                  await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
             }, 300);
         }

         [UnityTest]
         [Timeout(int.MaxValue)]
         public IEnumerator Zhocker()
         {
             return AsyncTest(async () =>
             {
                 Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 3,
                     new DeckCardData("Zhocker", 10));
                 Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 3,
                     new DeckCardData("Zhocker", 10));

                  PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                 {
                     Player1HasFirstTurn = true
                 };

                 InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zhocker", 1);
                 InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zhocker", 1);

                 int delayedDamage = 2;

                 IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                 {
                     player => {},
                     opponent => {},
                     player =>
                     {
                         player.CardPlay(playerCardId, ItemPosition.Start);
                         player.CardAbilityUsed(playerCardId, Enumerators.AbilityType.DELAYED_GAIN_ATTACK, new List<ParametrizedAbilityInstanceId>());
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

                 await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
             }, 300);
         }

          [UnityTest]
         [Timeout(int.MaxValue)]
         public IEnumerator FlowZ()
         {
             return AsyncTest(async () =>
             {
                 Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 3,
                     new DeckCardData("FlowZ", 1),
                     new DeckCardData("Slab", 15));
                 Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 3,
                     new DeckCardData("FlowZ", 1),
                     new DeckCardData("Slab", 15));

                  PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                 {
                     Player1HasFirstTurn = true
                 };

                  InstanceId playerFlowZId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "FlowZ", 1);
                  InstanceId playerSlabId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Slab", 1);

                  InstanceId opponentFlowZnId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "FlowZ", 1);
                  InstanceId opponentSlabId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Slab", 1);

                  IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                  {
                     player => {},
                     opponent => {},
                     player =>
                     {
                         player.CardPlay(playerSlabId, ItemPosition.Start);
                         player.CardPlay(playerFlowZId, ItemPosition.Start);
                         player.CardAbilityUsed(playerFlowZId, Enumerators.AbilityType.DRAW_CARD, new List<ParametrizedAbilityInstanceId>());
                     },
                     opponent =>
                     {
                         opponent.CardPlay(opponentSlabId, ItemPosition.Start);
                         opponent.CardPlay(opponentFlowZnId, ItemPosition.Start);
                         opponent.CardAbilityUsed(opponentFlowZnId, Enumerators.AbilityType.DRAW_CARD, new List<ParametrizedAbilityInstanceId>());
                     },
                     player =>
                     {
                         player.CardAttack(playerSlabId, opponentFlowZnId);
                     },
                     opponent =>
                     {
                         opponent.CardAttack(opponentSlabId, playerFlowZId);
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
         public IEnumerator Ztormcaller()
         {
             return AsyncTest(async () =>
             {
                 Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 3,
                     new DeckCardData("Ztormcaller", 1),
                     new DeckCardData("Slab", 15));
                 Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 3,
                     new DeckCardData("Ztormcaller", 1),
                     new DeckCardData("Slab", 15));

                 PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                 {
                     Player1HasFirstTurn = true
                 };

                 InstanceId playerZtormcallerId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Ztormcaller", 1);
                 InstanceId playerSlabId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Slab", 1);
                 InstanceId playerSlab2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Slab", 2);
                 InstanceId playerSlab3Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Slab", 3);

                 InstanceId opponentZtormcallerId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Ztormcaller", 1);
                 InstanceId opponentSlabId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Slab", 1);
                 InstanceId opponentSlab2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Slab", 2);
                 InstanceId opponentSlab3Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Slab", 3);

                 int difference = 1;

                 IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                 {
                     player => {},
                     opponent => {},
                     player =>
                     {
                         player.CardPlay(playerSlabId, ItemPosition.Start);
                         player.CardPlay(playerSlab2Id, ItemPosition.Start);
                         player.CardPlay(playerSlab3Id, ItemPosition.Start);
                     },
                     opponent =>
                     {
                         opponent.CardPlay(opponentSlabId, ItemPosition.Start);
                         opponent.CardPlay(opponentSlab2Id, ItemPosition.Start);
                         opponent.CardPlay(opponentSlab3Id, ItemPosition.Start);
                     },
                     player =>
                     {
                         player.CardPlay(playerZtormcallerId, ItemPosition.Start, opponentSlab2Id);
                         player.CardAbilityUsed(playerZtormcallerId, Enumerators.AbilityType.SWING, new List<ParametrizedAbilityInstanceId>());
                     },
                     opponent =>
                     {
                         opponent.CardPlay(opponentZtormcallerId, ItemPosition.Start, playerSlab2Id);
                         opponent.CardAbilityUsed(opponentZtormcallerId, Enumerators.AbilityType.SWING, new List<ParametrizedAbilityInstanceId>());
                     },
                     player =>
                     {
                         player.CardAttack(playerZtormcallerId, opponentSlab2Id);
                     },
                     opponent =>
                     {
                         opponent.CardAttack(opponentZtormcallerId, playerSlab2Id);
                         opponent.LetsThink(2);
                     },
                 };

                 Action validateEndState = () =>
                 {
                     BoardUnitModel playerSlab = (BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerSlabId);
                     BoardUnitModel playerSlab3 = (BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerSlab3Id);
                     BoardUnitModel opponentSlab = (BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentSlabId);
                     BoardUnitModel opponentSlab3 = (BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentSlab3Id);

                     Assert.Null((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerSlab2Id));
                     Assert.Null((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentSlab2Id));
                     Assert.AreEqual(playerSlab.Card.Prototype.Defense - difference, playerSlab.CurrentDefense);
                     Assert.AreEqual(playerSlab3.Card.Prototype.Defense - difference, playerSlab3.CurrentDefense);
                     Assert.AreEqual(opponentSlab.Card.Prototype.Defense - difference, opponentSlab.CurrentDefense);
                     Assert.AreEqual(opponentSlab3.Card.Prototype.Defense - difference, opponentSlab3.CurrentDefense);
                 };

                 await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
             }, 300);
         }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Dragger()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 3,
                    new DeckCardData("Dragger", 1),
                    new DeckCardData("Zhocker", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 3,
                    new DeckCardData("Dragger", 1),
                    new DeckCardData("Zhocker", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerDraggerId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Dragger", 1);
                InstanceId opponentDraggerId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Dragger", 1);

                InstanceId opponentZhockerId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zhocker", 1);

                BoardUnitModel opponentZnowman = null;
                BoardUnitModel playerUnitFromDeck = null;

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                   {
                       player => {},
                       opponent =>
                       {
                           opponent.CardPlay(opponentDraggerId, ItemPosition.Start, null, true);
                           opponent.CardAbilityUsed(opponentDraggerId, Enumerators.AbilityType.SUMMON_UNIT_FROM_HAND, new List<ParametrizedAbilityInstanceId>(){
                               new ParametrizedAbilityInstanceId(opponentZhockerId),
                           });
                           opponent.LetsThink(2);
                       },
                       player =>
                       {
                           player.CardPlay(playerDraggerId, ItemPosition.Start);
                           player.LetsThink(2);
                           opponentZnowman = (BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentZhockerId);

                       },
                       opponent =>
                       {
                           playerUnitFromDeck = pvpTestContext.GetCurrentPlayer().CardsOnBoard.FirstOrDefault(card => card.InstanceId != playerDraggerId);
                       },
                       player =>
                       {
                           player.LetsThink(2);
                           player.AssertInQueue(() => {
                               Assert.NotNull(playerUnitFromDeck);
                               Assert.IsTrue(playerUnitFromDeck.UnitCanBeUsable());
                           });
                       },
                       opponent =>
                       {
                           opponent.LetsThink(2);
                           opponent.AssertInQueue(() => {
                               Assert.NotNull(opponentZnowman);
                               Assert.IsTrue(opponentZnowman.UnitCanBeUsable());
                           });
                       },
                   };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(2, pvpTestContext.GetCurrentPlayer().CardsOnBoard.Count);
                    Assert.AreEqual(2, pvpTestContext.GetOpponentPlayer().CardsOnBoard.Count);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Zquall()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 3,
                    new DeckCardData("Zquall", 1),
                    new DeckCardData("Zhocker", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 3,
                    new DeckCardData("Zquall", 1),
                    new DeckCardData("Zhocker", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerZquallId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zquall", 1);
                InstanceId opponentZquallId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zquall", 1);

                InstanceId opponentZhockerId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zhocker", 1);
                InstanceId opponentZhocker2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zhocker", 2);

                BoardUnitModel opponentZnowman = null;
                BoardUnitModel opponentZnowman2 = null;

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                   {
                       player => {},
                       opponent =>
                       {
                           opponent.CardPlay(opponentZquallId, ItemPosition.Start, null, true);
                           opponent.CardAbilityUsed(opponentZquallId, Enumerators.AbilityType.SUMMON_UNIT_FROM_HAND, new List<ParametrizedAbilityInstanceId>(){
                               new ParametrizedAbilityInstanceId(opponentZhockerId),
                               new ParametrizedAbilityInstanceId(opponentZhocker2Id),
                           });
                           opponent.LetsThink(2);
                       },
                       player =>
                       {
                           player.CardPlay(playerZquallId, ItemPosition.Start);
                           player.LetsThink(2);
                       },
                       opponent =>
                       {
                           opponentZnowman = (BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentZhockerId);
                           opponentZnowman2 = (BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentZhocker2Id);
                           opponent.LetsThink(2);
                           opponent.AssertInQueue(() => {
                               Assert.NotNull(opponentZnowman);
                               Assert.IsTrue(opponentZnowman.UnitCanBeUsable());
                               Assert.NotNull(opponentZnowman2);
                               Assert.IsTrue(opponentZnowman2.UnitCanBeUsable());
                           });
                       },
                       player =>
                       {
                           player.LetsThink(2);
                           player.AssertInQueue(() => {
                               foreach (BoardUnitModel unit in pvpTestContext.GetCurrentPlayer().CardsOnBoard)
                               {
                                   Assert.NotNull(unit);
                                   Assert.IsTrue(unit.UnitCanBeUsable());
                               }
                           });
                       },
                   };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(3, pvpTestContext.GetCurrentPlayer().CardsOnBoard.Count);
                    Assert.AreEqual(3, pvpTestContext.GetOpponentPlayer().CardsOnBoard.Count);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }
    }
}
