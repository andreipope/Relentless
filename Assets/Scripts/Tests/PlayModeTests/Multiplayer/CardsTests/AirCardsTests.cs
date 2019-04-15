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
                    new DeckCardData("Zlab", 20),
                });

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerPyromazId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Pyromaz", 1);
                InstanceId playerMindFlayerId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Mind Flayer", 1);

                InstanceId opponentZlabId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zlab", 1);
                InstanceId opponentZlab2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zlab", 2);
                InstanceId opponentZlab3Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zlab", 3);
                InstanceId opponentZlab4Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zlab", 4);
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
                           opponent.CardPlay(opponentZlabId, ItemPosition.Start);
                       },
                       player =>
                       {
                           player.CardPlay(playerMindFlayerId, ItemPosition.Start);
                           player.CardAbilityUsed(playerMindFlayerId, Enumerators.AbilityType.TAKE_CONTROL_ENEMY_UNIT, new List<ParametrizedAbilityInstanceId>()
                           {  
                               new ParametrizedAbilityInstanceId(opponentZlabId)
                           });
                           player.LetsThink(2);
                           player.AssertInQueue(() => {
                                Assert.IsTrue(TestHelper.GetCurrentPlayer().CardsOnBoard.FindAll(card => card.Card.Prototype.MouldId == 101).Count > 0);
                           });
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentZlab2Id, ItemPosition.Start);
                           opponent.CardPlay(opponentZlab3Id, ItemPosition.Start);
                           opponent.CardPlay(opponentZlab4Id, ItemPosition.Start);
                           opponent.LetsThink(2);
                           opponent.CardAttack(opponentZlab2Id, playerMindFlayerId);
                           opponent.CardAttack(opponentZlab3Id, playerMindFlayerId);
                           opponent.CardAttack(opponentZlab4Id, opponentZlabId);
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

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

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
                    new DeckCardData("Zonic", 5)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 3,
                    new DeckCardData("Zonic", 5)
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
                    Assert.IsTrue(((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCardId)).HasBuffShield);
                    Assert.IsTrue(((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCardId)).HasBuffShield);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            }, 300);
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Zeuz()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0,
                    new DeckCardData("Zeuz", 1),
                    new DeckCardData("Igloo", 20)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    new DeckCardData("Zeuz", 1),
                    new DeckCardData("Igloo", 20)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerZeuzId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zeuz", 1);
                InstanceId playerIglooId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Igloo", 1);
                InstanceId opponentZeuzId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zeuz", 1);
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
                           opponent.CardAbilityUsed(opponentZeuzId, Enumerators.AbilityType.MASSIVE_DAMAGE, new List<ParametrizedAbilityInstanceId>());
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
                    Assert.IsNull(playerIglooUnit);
                    Assert.IsNull(opponentIglooUnit);
                    Assert.AreEqual(playerZeuzUnit.Card.Prototype.Defense - 6, playerZeuzUnit.CurrentDefense);
                    Assert.AreEqual(opponentZeuzUnit.Card.Prototype.Defense - 6, opponentZeuzUnit.CurrentDefense);
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
                    new DeckCardData("MonZoon", 3),
                    new DeckCardData("Igloo", 20)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    new DeckCardData("MonZoon", 3),
                    new DeckCardData("Igloo", 20)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

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
                    new DeckCardData("Whizper", 1),
                    new DeckCardData("Banshee", 1),
                    new DeckCardData("Enrager", 20)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    new DeckCardData("Zyclone", 1),
                    new DeckCardData("Whizper", 1),
                    new DeckCardData("Banshee", 1),
                    new DeckCardData("Enrager", 20)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerZycloneId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zyclone", 1);
                InstanceId playerWhizperId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Whizper", 1);
                InstanceId playerBansheeId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Banshee", 1);
                InstanceId playerEnragerId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Enrager", 1);
                InstanceId opponentZycloneId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zyclone", 1);
                InstanceId opponentWhizperId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Whizper", 1);
                InstanceId opponentBansheeId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Banshee", 1);
                InstanceId opponentEnragerId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Enrager", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                       player => {},
                       opponent => {},
                       player =>
                       {
                           player.CardPlay(playerBansheeId, ItemPosition.Start);
                           player.CardPlay(playerWhizperId, ItemPosition.Start);
                           player.CardPlay(playerEnragerId, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentBansheeId, ItemPosition.Start);
                           opponent.CardPlay(opponentWhizperId, ItemPosition.Start);
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
                    Assert.IsNull(((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerWhizperId)));
                    Assert.IsNull(((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentWhizperId)));
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

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

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

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

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
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 3, 
                    new DeckCardData("Bouncer", 1),
                    new DeckCardData("Trunk", 20)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 3, 
                    new DeckCardData("Bouncer", 1),
                    new DeckCardData("Trunk", 20)
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
                    Assert.IsTrue(((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerTrunkId)).HasBuffShield);
                    Assert.IsTrue(((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentTrunkId)).HasBuffShield);
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
                    new DeckCardData("Gale", 1),
                    new DeckCardData("Bouncer", 20)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 3,
                    new DeckCardData("Gale", 1),
                    new DeckCardData("Bouncer", 20)
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
                    new DeckCardData("Whiffer", 1),
                    new DeckCardData("Bouncer", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 3,
                    new DeckCardData("Whiffer", 1),
                    new DeckCardData("Bouncer", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

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
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 3, new DeckCardData("Gaz", 10));
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 3, new DeckCardData("Gaz", 10));

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
                     new DeckCardData("Bouncer", 10)
                  );
                  Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 3,
                     new DeckCardData("Wheezy", 1),
                     new DeckCardData("Bouncer", 10)
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
                     Assert.IsTrue(pvpTestContext.GetCurrentPlayer().CardsInHand.FindAll(card => card.Card.InstanceCard.Cost < card.Card.Prototype.Cost).Count > 0);
                     Assert.IsTrue(pvpTestContext.GetOpponentPlayer().CardsInHand.FindAll(card => card.Card.InstanceCard.Cost < card.Card.Prototype.Cost).Count > 0);
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
                    new DeckCardData("Zoothsayer", 1),
                    new DeckCardData("Breezee", 2),
                    new DeckCardData("Hot", 20));
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 3,
                    new DeckCardData("Zoothsayer", 1),
                    new DeckCardData("Breezee", 2),
                    new DeckCardData("Hot", 20));

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
                         player.CardPlay(playerBreezeeId, ItemPosition.Start);
                         player.CardPlay(playerBreezee2Id, ItemPosition.Start);
                         player.CardPlay(playerHotId, ItemPosition.Start);
                         player.CardPlay(playerZoothsayerId, ItemPosition.Start);
                         player.LetsThink(2);

                     },
                     opponent =>
                     {
                         opponent.CardPlay(opponentBreezeeId, ItemPosition.Start);
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
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 1, new DeckCardData("Fumez", 6));
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 1, new DeckCardData("Fumez", 6));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Fumez", 1);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Fumez", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                    player =>
                    {
                        player.LetsThink(2);
                        player.CardPlay(playerCardId, ItemPosition.Start);
                        player.CardAbilityUsed(opponentCardId, Enumerators.AbilityType.ADD_GOO_VIAL, new List<ParametrizedAbilityInstanceId>());
                        player.LetsThink(2);
                        player.AssertInQueue(() => {
                            Assert.AreEqual(2, pvpTestContext.GetCurrentPlayer().GooVials);
                        });
                    },
                    opponent =>
                    {
                        opponent.LetsThink(2);
                        opponent.CardPlay(opponentCardId, ItemPosition.Start, null, true);
                        opponent.CardAbilityUsed(opponentCardId, Enumerators.AbilityType.ADD_GOO_VIAL, new List<ParametrizedAbilityInstanceId>());
                        opponent.LetsThink(2);
                        opponent.AssertInQueue(() => {
                            Assert.AreEqual(2, pvpTestContext.GetOpponentPlayer().GooVials);
                        });
                    },
                };

                Action validateEndState = () =>
                {
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
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

                  PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

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
                     new DeckCardData("Zlab", 15));
                 Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 3,
                     new DeckCardData("FlowZ", 1),
                     new DeckCardData("Zlab", 15));

                  PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                  InstanceId playerFlowZId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "FlowZ", 1);
                  InstanceId playerZlabId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zlab", 1);

                  InstanceId opponentFlowZnId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "FlowZ", 1);
                  InstanceId opponentZlabId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zlab", 1);

                  IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                  {
                     player => {},
                     opponent => {},
                     player =>
                     {
                         player.CardPlay(playerZlabId, ItemPosition.Start);
                         player.CardPlay(playerFlowZId, ItemPosition.Start);
                         player.CardAbilityUsed(playerFlowZId, Enumerators.AbilityType.DRAW_CARD, new List<ParametrizedAbilityInstanceId>());
                     },
                     opponent =>
                     {
                         opponent.CardPlay(opponentZlabId, ItemPosition.Start);
                         opponent.CardPlay(opponentFlowZnId, ItemPosition.Start);
                         opponent.CardAbilityUsed(opponentFlowZnId, Enumerators.AbilityType.DRAW_CARD, new List<ParametrizedAbilityInstanceId>());
                     },
                     player =>
                     {
                         player.CardAttack(playerZlabId, opponentFlowZnId);
                     },
                     opponent =>
                     {
                         opponent.CardAttack(opponentZlabId, playerFlowZId);
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
                     new DeckCardData("Zlab", 15));
                 Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 3,
                     new DeckCardData("Ztormcaller", 1),
                     new DeckCardData("Zlab", 15));

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
                         player.CardPlay(playerZlabId, ItemPosition.Start);
                         player.CardPlay(playerZlab2Id, ItemPosition.Start);
                     },
                     opponent =>
                     {
                         opponent.CardPlay(opponentZtormcallerId, ItemPosition.Start);
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

                 await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
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

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

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

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

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

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Zky()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 3,
                    new DeckCardData("Zky", 1),
                    new DeckCardData("Zlab", 15));
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 3,
                    new DeckCardData("Zky", 1),
                    new DeckCardData("Zlab", 15));

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

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            }, 300);
        }
    }
}
