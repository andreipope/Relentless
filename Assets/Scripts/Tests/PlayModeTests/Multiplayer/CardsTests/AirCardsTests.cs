using System;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using UnityEngine.TestTools;
using UnityEngine;

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
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 2,
                    new TestCardData("Mind Flayer", 1),
                    new TestCardData("Pyromaz", 20)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 2,
                    new TestCardData("Mind Flayer", 1),
                    new TestCardData("Zludge", 20)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerPyromazId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Pyromaz", 1);
                InstanceId playerMindFlayerId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Mind Flayer", 1);

                InstanceId opponentZludgeId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zludge", 1);
                InstanceId opponentZludge2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zludge", 2);
                InstanceId opponentZludge3Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zludge", 3);
                InstanceId opponentZludge4Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zludge", 4);
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
                           opponent.CardPlay(opponentZludgeId, ItemPosition.Start);
                       },
                       player =>
                       {
                           player.CardPlay(playerMindFlayerId, ItemPosition.Start);
                           player.CardAbilityUsed(playerMindFlayerId, Enumerators.AbilityType.TAKE_CONTROL_ENEMY_UNIT, new List<ParametrizedAbilityInstanceId>()
                           {
                               new ParametrizedAbilityInstanceId(opponentZludgeId)
                           });
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentZludge2Id, ItemPosition.Start);
                           opponent.CardPlay(opponentZludge3Id, ItemPosition.Start);
                           opponent.CardPlay(opponentZludge4Id, ItemPosition.Start);
                       },
                       player => {
                           player.AssertInQueue(() => {
                                Assert.IsTrue(TestHelper.GetCurrentPlayer().CardsOnBoard.FindAll(card => card.Card.Prototype.MouldId.Id == 85).Count > 0);
                           });
                       },
                       opponent =>
                       {
                           opponent.CardAttack(opponentZludge2Id, playerMindFlayerId);
                           opponent.CardAttack(opponentZludge3Id, playerMindFlayerId);
                           opponent.CardAttack(opponentZludge4Id, opponentZludgeId);
                           opponent.CardPlay(opponentMindFlayerId, ItemPosition.Start);
                           opponent.CardAbilityUsed(opponentMindFlayerId, Enumerators.AbilityType.TAKE_CONTROL_ENEMY_UNIT, new List<ParametrizedAbilityInstanceId>()
                           {
                               new ParametrizedAbilityInstanceId(playerPyromazId)
                           });
                       },
                };

                Action validateEndState = () =>
                {
                    Assert.IsTrue(TestHelper.GetOpponentPlayer().CardsOnBoard.FindAll(card => card.Card.Prototype.MouldId.Id == 10).Count > 0);
                };
            await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            }, 300);
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator BansheeWithGale()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 3,
                    new TestCardData("Banshee", 2),
                    new TestCardData("Trunk", 1),
                    new TestCardData("Gale", 20)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 3,
                    new TestCardData("Banshee", 2),
                    new TestCardData("Trunk", 1),
                    new TestCardData("Gale", 20)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerBansheeId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Banshee", 1);
                InstanceId playerTrunkId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Trunk", 1);
                InstanceId playerGaleId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Gale", 1);
                InstanceId opponentBansheeId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Banshee", 1);
                InstanceId opponentGaleId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Gale", 1);
                InstanceId opponentTrunkId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Trunk", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                    player => {},
                    opponent => {},
                    player => player.CardPlay(playerTrunkId, ItemPosition.Start),
                    opponent => opponent.CardPlay(opponentTrunkId, ItemPosition.Start),
                    player => {
                        player.CardPlay(playerBansheeId, ItemPosition.Start, opponentTrunkId);
                        player.LetsThink(5);
                        player.CardPlay(playerGaleId, ItemPosition.Start, playerBansheeId);
                        player.LetsThink(5);
                        player.CardPlay(playerBansheeId, ItemPosition.Start, opponentTrunkId);
                        player.LetsThink(5);
                        player.AssertInQueue(() => {
                            Assert.AreEqual(0, ((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentTrunkId)).CurrentDamage);
                        });
                    },
                    opponent => {
                        opponent.LetsThink(5);
                        opponent.AssertInQueue(() => {
                            Assert.AreEqual(6, ((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentTrunkId)).CurrentDamage);
                        });
                    }
                };

                Action validateEndState = () =>
                {
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            }, 500);
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Banshee()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 3,
                    new TestCardData("Banshee", 2),
                    new TestCardData("Trunk", 20)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 3,
                    new TestCardData("Banshee", 2),
                    new TestCardData("Trunk", 20)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerBansheeId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Banshee", 1);
                InstanceId playerBanshee2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Banshee", 2);
                InstanceId playerTrunkId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Trunk", 1);
                InstanceId opponentBansheeId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Banshee", 1);
                InstanceId opponentTrunkId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Trunk", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                    player => {},
                    opponent => {},
                    player => player.CardPlay(playerTrunkId, ItemPosition.Start),
                    opponent => opponent.CardPlay(opponentTrunkId, ItemPosition.Start),
                    player => {
                        player.CardPlay(playerBansheeId, ItemPosition.Start, opponentTrunkId);
                        player.LetsThink(5);
                        player.CardPlay(playerBanshee2Id, ItemPosition.Start, opponentTrunkId);
                        player.LetsThink(5);
                        player.AssertInQueue(() => {
                            Assert.AreEqual(0, ((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentTrunkId)).CurrentDamage);
                        });
                    },
                    opponent => {
                        opponent.LetsThink(5);
                        opponent.AssertInQueue(() => {
                            Assert.AreEqual(6, ((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentTrunkId)).CurrentDamage);
                        });
                    }
                };

                Action validateEndState = () =>
                {
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            }, 500);
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Zonic()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 3,
                    new TestCardData("Zonic", 5)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 3,
                    new TestCardData("Zonic", 5)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zonic", 1);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zonic", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                       player => player.CardPlay(playerCardId, ItemPosition.Start),
                       opponent => opponent.CardPlay(opponentCardId, ItemPosition.Start),
                       player => {} 
                };

                Action validateEndState = () =>
                {
                    Assert.IsTrue(((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCardId)).HasBuffShield);
                    Assert.IsTrue(((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCardId)).HasBuffShield);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            }, 300);
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Zeuz()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0,
                    new TestCardData("Zeuz", 1),
                    new TestCardData("Igloo", 20)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    new TestCardData("Zeuz", 1),
                    new TestCardData("Igloo", 20)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerZeuzId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zeuz", 1);
                InstanceId playerIglooId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Igloo", 1);
                InstanceId opponentZeuzId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zeuz", 1);
                InstanceId opponentIglooId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Igloo", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                       player => {},
                       opponent => {},
                       player => 
                       {
                           player.CardPlay(playerIglooId, ItemPosition.Start);
                           player.CardPlay(playerZeuzId, ItemPosition.Start);
                           player.CardAbilityUsed(playerZeuzId, Enumerators.AbilityType.MASSIVE_DAMAGE, new List<ParametrizedAbilityInstanceId>());
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentIglooId, ItemPosition.Start);
                           opponent.CardPlay(opponentZeuzId, ItemPosition.Start);
                           opponent.CardAbilityUsed(opponentZeuzId, Enumerators.AbilityType.MASSIVE_DAMAGE, new List<ParametrizedAbilityInstanceId>());
                       },
                       player => {},
                       opponent => {},
                       player => {},
                };

                int value = 4;

                Action validateEndState = () =>
                {
                    CardModel playerZeuzUnit = ((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerZeuzId));
                    CardModel playerIglooUnit = ((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerIglooId));
                    CardModel opponentZeuzUnit = ((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentZeuzId));
                    CardModel opponentIglooUnit = ((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentIglooId));
                    Assert.AreEqual(playerIglooUnit.Card.Prototype.Defense - value, playerIglooUnit.CurrentDefense);
                    Assert.AreEqual(opponentIglooUnit.Card.Prototype.Defense - value, opponentIglooUnit.CurrentDefense);
                    Assert.AreEqual(playerZeuzUnit.Card.Prototype.Defense - value, playerZeuzUnit.CurrentDefense);
                    Assert.AreEqual(opponentZeuzUnit.Card.Prototype.Defense - value, opponentZeuzUnit.CurrentDefense);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            }, 400);
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator MonZoon()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0,
                    new TestCardData("MonZoon", 1),
                    new TestCardData("Trunk", 20)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    new TestCardData("MonZoon", 1),
                    new TestCardData("Trunk", 20)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerMonzoonId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "MonZoon", 1);
                InstanceId opponentMonzoonId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "MonZoon", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                       player => {},
                       opponent => {},
                       player => {},
                       opponent => {},
                       player => {},
                       opponent => {},
                       player => {},
                       opponent => {}
                };

                Action validateEndState = () =>
                {
                    CardModel playerMonzoonModel = ((CardModel)TestHelper.BattlegroundController.GetCardModelByInstanceId(playerMonzoonId));
                    Assert.AreEqual(5, playerMonzoonModel.CurrentCost);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            }, 400);
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Zyclone()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0,
                    new TestCardData("Zyclone", 1),
                    new TestCardData("Whizper", 1),
                    new TestCardData("Banshee", 1),
                    new TestCardData("Enrager", 20)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    new TestCardData("Zyclone", 1),
                    new TestCardData("Whizper", 1),
                    new TestCardData("Banshee", 1),
                    new TestCardData("Enrager", 20)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerWhizperId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Whizper", 1);
                InstanceId playerBansheeId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Banshee", 1);
                InstanceId playerEnragerId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Enrager", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                       player => {},
                       opponent => {},
                       player =>
                       {
                           player.CardPlay(playerBansheeId, ItemPosition.Start);
                           player.CardPlay(playerWhizperId, ItemPosition.Start);
                           player.LetsThink(10);
                           player.AssertInQueue(() => {
                                Assert.IsTrue(pvpTestContext.GetCurrentPlayer().CardsInHand.FindAll(card => {
                                    Debug.LogWarning(card.CurrentCost + " " + card.Card.Prototype.Cost);
                                    return card.CurrentCost == card.Card.Prototype.Cost-2;
                                }).Count > 0);
                           });
                       },
                       opponent =>
                       {
                       },
                       player =>
                       {
                           
                           player.CardPlay(playerEnragerId, ItemPosition.Start);
                           player.LetsThink(10);
                           player.AssertInQueue(() => {
                                Assert.IsTrue(pvpTestContext.GetCurrentPlayer().CardsInHand.FindAll(card => {
                                    return card.CurrentCost == card.Card.Prototype.Cost-3;
                                }).Count > 0);
                           });
                       },
                       opponent => {},
                       player => {}
                };

                Action validateEndState = () =>
                {
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            }, 400);
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Zephyr()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0,
                    new TestCardData("Zephyr", 1),
                    new TestCardData("Trunk", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    new TestCardData("Zephyr", 1),
                    new TestCardData("Trunk", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerZephyrId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zephyr", 1);
                InstanceId opponentZephyrId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zephyr", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {                 
                       player => {},
                       opponent => {},
                       player =>
                       {
                           player.CardPlay(playerZephyrId, ItemPosition.Start);
                           player.CardAbilityUsed(playerZephyrId, Enumerators.AbilityType.CHANGE_STAT_OF_CARDS_IN_HAND, new List<ParametrizedAbilityInstanceId>());
                       },
                       opponent => {
                           opponent.CardPlay(opponentZephyrId, ItemPosition.Start);
                           opponent.CardAbilityUsed(opponentZephyrId, Enumerators.AbilityType.CHANGE_STAT_OF_CARDS_IN_HAND, new List<ParametrizedAbilityInstanceId>());
                       },
                       player => {},
                       opponent => {}
                };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(pvpTestContext.GetCurrentPlayer().CardsInHand.Count,
                        pvpTestContext.GetCurrentPlayer().CardsInHand.FindAll(card => card.CurrentCost == card.Card.Prototype.Cost - 1).Count);
                    Assert.AreEqual(pvpTestContext.GetOpponentPlayer().CardsInHand.Count,
                        pvpTestContext.GetOpponentPlayer().CardsInHand.FindAll(card => card.CurrentCost == card.Card.Prototype.Cost - 1).Count);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            }, 400);
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Buffer()
        {
            return AsyncTest(async () =>
             {
                  Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 3,
                     new TestCardData("Buffer", 1),
                     new TestCardData("Trunk", 10)
                  );
                  Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 3,
                     new TestCardData("Buffer", 1),
                     new TestCardData("Trunk", 10)
                  );

                  PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                  InstanceId playerBufferId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Buffer", 1);
                  InstanceId opponentBufferId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Buffer", 1);

                  IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                  {
                        player => {},
                        opponent => {},
                        player =>
                        {
                            player.CardPlay(playerBufferId, ItemPosition.Start);
                        },
                        opponent =>
                        {
                            opponent.CardPlay(opponentBufferId, ItemPosition.Start);
                        },
                        player => 
                        {
                            player.CardAttack(playerBufferId, opponentBufferId);
                        },
                        opponent => 
                        {
                            opponent.CardAttack(opponentBufferId, playerBufferId);
                        },
                        player => {},
                  };

                  Action validateEndState = () =>
                  {
                     Assert.IsTrue(pvpTestContext.GetCurrentPlayer().CardsInHand.FindAll(card => card.CurrentDamage == card.Card.Prototype.Damage+2).Count > 0);
                     Assert.IsTrue(pvpTestContext.GetCurrentPlayer().CardsInHand.FindAll(card => card.CurrentDefense == card.Card.Prototype.Defense+1).Count > 0);

                     Assert.IsTrue(pvpTestContext.GetOpponentPlayer().CardsInHand.FindAll(card => card.CurrentDamage == card.Card.Prototype.Damage+2).Count > 0);
                     Assert.IsTrue(pvpTestContext.GetOpponentPlayer().CardsInHand.FindAll(card => card.CurrentDefense == card.Card.Prototype.Defense+1).Count > 0);
                  };

                  await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
             }, 300);
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Bouncer()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 3, 
                    new TestCardData("Bouncer", 1),
                    new TestCardData("Trunk", 20)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 3, 
                    new TestCardData("Bouncer", 1),
                    new TestCardData("Trunk", 20)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerBouncerId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Bouncer", 1);
                InstanceId playerTrunkId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Trunk", 1);
                InstanceId opponentBouncerId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Bouncer", 1);
                InstanceId opponentTrunkId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Trunk", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                    player => {},
                    opponent => {},
                    player => player.CardPlay(playerTrunkId, ItemPosition.Start),
                    opponent => opponent.CardPlay(opponentTrunkId, ItemPosition.Start),
                    player => player.CardPlay(playerBouncerId, ItemPosition.Start, playerTrunkId),
                    opponent => opponent.CardPlay(opponentBouncerId, ItemPosition.Start, opponentTrunkId),
                    player => {},
                    opponent => {}
                };

                Action validateEndState = () =>
                {
                    Assert.IsTrue(((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerTrunkId)).HasBuffShield);
                    Assert.IsTrue(((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentTrunkId)).HasBuffShield);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            }, 300);
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Gale()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 3,
                    new TestCardData("Gale", 1),
                    new TestCardData("Bouncer", 20)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 3,
                    new TestCardData("Gale", 1),
                    new TestCardData("Bouncer", 20)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerGaleId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Gale", 1);
                InstanceId playerBouncerId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Bouncer", 1);
                InstanceId opponentGaleId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Gale", 1);
                InstanceId opponentBouncerId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Bouncer", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                       player => {},
                       opponent => {},
                       player => player.CardPlay(playerBouncerId, ItemPosition.Start),
                       opponent => opponent.CardPlay(opponentBouncerId, ItemPosition.Start),
                       player => player.CardPlay(playerGaleId, ItemPosition.Start, opponentBouncerId),
                       opponent => opponent.CardPlay(opponentGaleId, ItemPosition.Start, playerBouncerId),
                       player => {},
                       opponent => {},
                };

                Action validateEndState = () => {
                    Assert.AreEqual(1, pvpTestContext.GetCurrentPlayer().CardsOnBoard.Count);
                    Assert.AreEqual(1, pvpTestContext.GetOpponentPlayer().CardsOnBoard.Count);
                };

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
                    new TestCardData("Whiffer", 1),
                    new TestCardData("Znowman", 5)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 3,
                    new TestCardData("Whiffer", 1),
                    new TestCardData("Znowman", 5)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerWhifferId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Whiffer", 1);
                InstanceId playerZnowmanId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Znowman", 1);
                InstanceId opponentWhifferId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Whiffer", 1);
                InstanceId opponentZnowmanId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Znowman", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                       player => player.CardPlay(playerZnowmanId, ItemPosition.Start),
                       opponent => opponent.CardPlay(opponentZnowmanId, ItemPosition.Start),
                       player => player.CardPlay(playerWhifferId, ItemPosition.Start),
                       opponent => opponent.CardPlay(opponentWhifferId, ItemPosition.Start),
                       player => {}
                };

                Action validateEndState = () =>
                {
                    Assert.IsTrue(((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerWhifferId)).AgileEnabled);
                    Assert.IsTrue(((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentWhifferId)).AgileEnabled);
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
                    new TestCardData("Ztormonk", 2),
                    new TestCardData("Zludge", 20)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 3,
                    new TestCardData("Ztormonk", 2),
                    new TestCardData("Zludge", 20)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

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
                    Assert.IsTrue(((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerZludge2Id)).HasBuffShield);
                    Assert.IsTrue(((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentZludge2Id)).HasBuffShield);
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
                    new TestCardData("Draft", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 3,
                    new TestCardData("Draft", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

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
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 3, new TestCardData("Gaz", 10));
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 3, new TestCardData("Gaz", 10));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Gaz", 1);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Gaz", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                    player => {},
                    opponent => {},
                    player => player.CardPlay(playerCardId, ItemPosition.Start),
                    opponent => opponent.CardPlay(opponentCardId, ItemPosition.Start),
                    player => {},
                    opponent => {}
                };

                Action validateEndState = () =>
                {
                    Assert.IsTrue(((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCardId)).HasBuffShield);
                    Assert.IsTrue(((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCardId)).HasBuffShield);
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
                     new TestCardData("Wheezy", 1),
                     new TestCardData("Bouncer", 10)
                  );
                  Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 3,
                     new TestCardData("Wheezy", 1),
                     new TestCardData("Bouncer", 10)
                  );

                  PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                  InstanceId playerWheezyId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Wheezy", 1);
                  InstanceId playerWhizperId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Bouncer", 1);
                  InstanceId opponentWheezyId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Wheezy", 1);
                  InstanceId opponentWhizperId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Bouncer", 1);

                  IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                  {
                        player => {},
                        opponent => {},
                        player =>
                        {
                            player.CardPlay(playerWheezyId, ItemPosition.Start, skipEntryAbilities: true);
                            player.CardAbilityUsed(playerWheezyId, Common.Enumerators.AbilityType.LOWER_COST_OF_CARD_IN_HAND, new List<ParametrizedAbilityInstanceId>()
                            {
                                new ParametrizedAbilityInstanceId(pvpTestContext.GetOpponentPlayer().InstanceId,
                                    new ParametrizedAbilityParameters()
                                         {
                                             CardName = playerWhizperId.Id.ToString()
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
                                             CardName = opponentWhizperId.Id.ToString()
                                         }
                                     )
                            });
                        },
                        player => {},
                        opponent => {},
                  };

                  Action validateEndState = () =>
                  {
                     Assert.IsTrue(pvpTestContext.GetCurrentPlayer().CardsInHand.FindAll(card => card.CurrentCost < card.Card.Prototype.Cost).Count > 0);
                     Assert.IsTrue(pvpTestContext.GetOpponentPlayer().CardsInHand.FindAll(card => card.CurrentCost < card.Card.Prototype.Cost).Count > 0);
                  };

                  await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
             }, 300);
         }

          [UnityTest]
         [Timeout(int.MaxValue)]
         public IEnumerator Zoothsayer()
         {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 3,
                    new TestCardData("Zoothsayer", 1),
                    new TestCardData("Breezee", 2),
                    new TestCardData("Hot", 20));
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 3,
                    new TestCardData("Zoothsayer", 1),
                    new TestCardData("Breezee", 2),
                    new TestCardData("Hot", 20));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerZoothsayerId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zoothsayer", 1);
                InstanceId playerBreezeeId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Breezee", 1);
                InstanceId playerBreezee2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Breezee", 2);
                InstanceId playerHotId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Hot", 1);


                InstanceId opponentZoothsayerId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zoothsayer", 1);
                InstanceId opponentBreezeeId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Breezee", 1);
                InstanceId opponentBreezee2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Breezee", 2);
                InstanceId opponentHotId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Hot", 1);


                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                     player => {},
                     opponent => {},
                     player =>
                     {
                         player.CardPlay(playerHotId, ItemPosition.Start);
                         player.CardPlay(playerZoothsayerId, ItemPosition.Start);
                         player.LetsThink(2);

                     },
                     opponent =>
                     {
                         opponent.CardPlay(opponentHotId, ItemPosition.Start);
                         opponent.CardPlay(opponentZoothsayerId, ItemPosition.Start);
                         opponent.CardAbilityUsed(opponentZoothsayerId, Enumerators.AbilityType.DRAW_CARD, new List<ParametrizedAbilityInstanceId>());
                         opponent.LetsThink(2);

                     },
                     player =>
                     {
                         player.CardAttack(playerHotId, opponentZoothsayerId);
                     },
                     opponent =>
                     {
                         opponent.CardAttack(opponentHotId, playerZoothsayerId);
                     }
                };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(6, pvpTestContext.GetCurrentPlayer().CardsInHand.Count);
                    Assert.AreEqual(6, pvpTestContext.GetOpponentPlayer().CardsInHand.Count);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            }, 300);
        }

          [UnityTest]
         [Timeout(int.MaxValue)]
         public IEnumerator Fumez()
         {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 1, new TestCardData("Fumez", 6));
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 1, new TestCardData("Fumez", 6));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Fumez", 1);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Fumez", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                    player =>
                    {
                        player.LetsThink(2);
                        player.CardPlay(playerCardId, ItemPosition.Start);
                        player.LetsThink(2);
                        player.AssertInQueue(() => {
                            Assert.AreEqual(1, pvpTestContext.GetCurrentPlayer().CurrentGoo);
                        });
                    },
                    opponent =>
                    {
                        opponent.LetsThink(2);
                        opponent.CardPlay(opponentCardId, ItemPosition.Start);
                        opponent.LetsThink(2);
                        opponent.AssertInQueue(() => {
                            Assert.AreEqual(1, pvpTestContext.GetOpponentPlayer().CurrentGoo);
                        });
                    },
                };

                Action validateEndState = () =>
                {
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }

         [UnityTest]
         [Timeout(int.MaxValue)]
         public IEnumerator Zhocker()
         {
             return AsyncTest(async () =>
             {
                 Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 2,
                     new TestCardData("Zhocker", 5));
                 Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 2,
                     new TestCardData("Zhocker", 5));

                 PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                 InstanceId playerZhocker1 = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zhocker", 1);
                 InstanceId playerZhocker2 = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zhocker", 2);
                 InstanceId opponentZhocker1 = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zhocker", 1);
                 InstanceId opponentZhocker2 = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zhocker", 2);

                 IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                 {
                       player => player.CardPlay(playerZhocker1, ItemPosition.Start),
                       opponent => opponent.CardPlay(opponentZhocker1, ItemPosition.Start),
                       player => player.CardPlay(playerZhocker2, ItemPosition.Start),
                       opponent => opponent.CardPlay(opponentZhocker2, ItemPosition.Start)
                 };

                 Action validateEndState = () =>
                 {
                     CardModel playerZhocker1Model = (CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerZhocker1);
                     CardModel playerZhocker2Model = (CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerZhocker2);
                     CardModel opponentZhocker1Model = (CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentZhocker1);
                     CardModel opponentZhocker2Model = (CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentZhocker2);

                     Assert.AreEqual(playerZhocker1Model.Card.Prototype.Damage, playerZhocker1Model.CurrentDamage);
                     Assert.AreEqual(playerZhocker2Model.Card.Prototype.Damage + 1, playerZhocker2Model.CurrentDamage);
                     Assert.AreEqual(opponentZhocker1Model.Card.Prototype.Damage, opponentZhocker1Model.CurrentDamage);
                     Assert.AreEqual(opponentZhocker2Model.Card.Prototype.Damage + 1, opponentZhocker2Model.CurrentDamage);
                 };

                 await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
             }, 300);
         }

         [UnityTest]
         [Timeout(int.MaxValue)]
         public IEnumerator Ztormcaller()
         {
             return AsyncTest(async () =>
             {
                 Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 3,
                     new TestCardData("Ztormcaller", 1),
                     new TestCardData("Zlab", 15));
                 Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 3,
                     new TestCardData("Ztormcaller", 1),
                     new TestCardData("Zlab", 15));

                 PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                 InstanceId playerZtormcallerId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Ztormcaller", 1);
                 InstanceId playerZlabId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zlab", 1);
                 InstanceId playerZlab2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zlab", 2);

                 InstanceId opponentZtormcallerId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Ztormcaller", 1);
                 InstanceId opponentZlabId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zlab", 1);
                 InstanceId opponentZlab2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zlab", 2);

                 IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                 {
                     player => {},
                     opponent => {},
                     player =>
                     {
                         player.CardPlay(playerZtormcallerId, ItemPosition.Start);
                         player.CardAbilityUsed(playerZtormcallerId, Enumerators.AbilityType.DRAW_CARD, new List<ParametrizedAbilityInstanceId>());
                         player.CardAbilityUsed(playerZtormcallerId, Enumerators.AbilityType.DRAW_CARD, new List<ParametrizedAbilityInstanceId>());
                         player.CardPlay(playerZlabId, ItemPosition.Start);
                         player.CardPlay(playerZlab2Id, ItemPosition.Start);
                     },
                     opponent =>
                     {
                         opponent.CardPlay(opponentZtormcallerId, ItemPosition.Start);
                         opponent.CardAbilityUsed(opponentZtormcallerId, Enumerators.AbilityType.DRAW_CARD, new List<ParametrizedAbilityInstanceId>());
                         opponent.CardAbilityUsed(opponentZtormcallerId, Enumerators.AbilityType.DRAW_CARD, new List<ParametrizedAbilityInstanceId>());
                         opponent.CardPlay(opponentZlabId, ItemPosition.Start);
                         opponent.CardPlay(opponentZlab2Id, ItemPosition.Start);
                     },
                     player =>
                     {
                         player.CardAttack(playerZlabId, opponentZtormcallerId);
                         player.CardAttack(playerZlab2Id, opponentZtormcallerId);
                     },
                     opponent =>
                     {
                         opponent.CardAttack(opponentZlabId, playerZtormcallerId);
                         opponent.CardAttack(opponentZlab2Id, playerZtormcallerId);
                         opponent.LetsThink(2);
                     },
                 };

                 Action validateEndState = () =>
                 {
                     Assert.AreEqual(8, pvpTestContext.GetCurrentPlayer().CardsInHand.Count);
                     Assert.AreEqual(8, pvpTestContext.GetOpponentPlayer().CardsInHand.Count);
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
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0,
                   new TestCardData("Dragger", 10)
               );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    new TestCardData("Dragger", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Dragger", 1);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Dragger", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                    player =>
                    {
                        player.CardPlay(playerCardId, ItemPosition.Start);
                    },
                    opponent =>
                    {
                        opponent.CardPlay(opponentCardId, ItemPosition.Start);
                    }
                };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(6, pvpTestContext.GetCurrentPlayer().CardsInHand.Count);
                    Assert.AreEqual(6, pvpTestContext.GetOpponentPlayer().CardsInHand.Count);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Zquall()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 3,
                    new TestCardData("Zquall", 1),
                    new TestCardData("Trunk", 20)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 3,
                    new TestCardData("Zquall", 1),
                    new TestCardData("Trunk", 20)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerZquallId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zquall", 1);
                InstanceId playerTrunk1Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Trunk", 1);
                InstanceId playerTrunk2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Trunk", 2);
                InstanceId playerTrunk3Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Trunk", 3);
                InstanceId playerTrunk4Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Trunk", 4);
                InstanceId playerTrunk5Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Trunk", 5);

                InstanceId opponentZquallId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zquall", 1);
                InstanceId opponentTrunk1Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Trunk", 1);
                InstanceId opponentTrunk2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Trunk", 2);
                InstanceId opponentTrunk3Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Trunk", 3);
                InstanceId opponentTrunk4Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Trunk", 4);
                InstanceId opponentTrunk5Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Trunk", 5);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                   {
                       player => {},
                       opponent => {},
                       player =>
                       {
                           player.AssertInQueue(() => {
                               CardModel playerZquallModel = ((CardModel)TestHelper.BattlegroundController.GetCardModelByInstanceId(playerZquallId));
                               Assert.IsFalse(playerZquallModel.CurrentCost == 0);
                           });
                           player.LetsThink(3);
                           player.CardPlay(playerTrunk1Id, ItemPosition.Start);
                           player.CardPlay(playerTrunk2Id, ItemPosition.Start);
                           player.CardPlay(playerTrunk3Id, ItemPosition.Start);
                           player.CardPlay(playerTrunk4Id, ItemPosition.Start);
                           player.LetsThink(10);
                           player.AssertInQueue(() => {
                               CardModel playerZquallModel = ((CardModel)TestHelper.BattlegroundController.GetCardModelByInstanceId(playerZquallId));
                               Assert.IsTrue(playerZquallModel.CurrentCost == 0);
                           });
                       },
                       opponent => {},
                       player =>
                       {},
                   };

                Action validateEndState = () =>
                {
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Zky()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 3,
                    new TestCardData("Zky", 1),
                    new TestCardData("Zlab", 15));
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 3,
                    new TestCardData("Zky", 1),
                    new TestCardData("Zlab", 15));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerZkyId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zky", 1);
                InstanceId playerZlabId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zlab", 1);
                InstanceId playerZlab2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zlab", 2);

                InstanceId opponentZkyId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zky", 1);
                InstanceId opponentZlabId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zlab", 1);
                InstanceId opponentZlab2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zlab", 2);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                     player => {},
                     opponent => {},
                     player =>
                     {
                         player.CardPlay(playerZkyId, ItemPosition.Start);
                     },
                     opponent =>
                     {
                         opponent.CardPlay(opponentZkyId, ItemPosition.Start);
                         opponent.LetsThink(2);
                     },
                };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(6, pvpTestContext.GetCurrentPlayer().CardsInHand.Count);
                    Assert.AreEqual(6, pvpTestContext.GetOpponentPlayer().CardsInHand.Count);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            }, 300);
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Zwoop()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 3,
                    new TestCardData("Zwoop", 2),
                    new TestCardData("Duzt", 20)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 3,
                    new TestCardData("Zwoop", 2),
                    new TestCardData("Duzt", 20)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerZwoopId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zwoop", 1);
                InstanceId playerDuztId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Duzt", 1);
                InstanceId opponentZwoopId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zwoop", 1);
                InstanceId opponentDuztId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Duzt", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                    player => {},
                    opponent => {},
                    player => player.CardPlay(playerDuztId, ItemPosition.Start),
                    opponent => opponent.CardPlay(opponentDuztId, ItemPosition.Start),
                    player => player.CardPlay(playerZwoopId, ItemPosition.End, opponentDuztId),
                    opponent => opponent.CardPlay(opponentZwoopId, ItemPosition.Start, playerDuztId),
                    player => {},
                    opponent => {}
                };

                Action validateEndState = () =>
                {
                    Assert.IsTrue(((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerDuztId)).WasDistracted);
                    Assert.IsTrue(((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentDuztId)).WasDistracted);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            }, 300);
        }
    }
}
