using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.TestTools;

namespace Loom.ZombieBattleground.Test.MultiplayerTests
{
    public class EarthCardsTests : BaseIntegrationTest
    {
        [UnityTest]
        [Timeout(int.MaxValue)]
        [Category("PlayQuickSubset2")]
        public IEnumerator Rocky()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0,
                    new DeckCardData("Rocky", 1),
                    new DeckCardData("Zlab", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    new DeckCardData("Rocky", 1),
                    new DeckCardData("Zlab", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerRockyId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Rocky", 1);
                InstanceId playerZlab1Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zlab", 1);
                InstanceId playerZlab2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zlab", 2);
                InstanceId opponentRockyId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Rocky", 1);
                InstanceId opponentZlab1Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zlab", 1);
                InstanceId opponentZlab2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zlab", 2);

                int value = 2;

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                    player => {},
                    opponent =>
                    {
                        opponent.CardPlay(opponentZlab1Id, ItemPosition.Start);
                        opponent.CardPlay(opponentZlab2Id, ItemPosition.Start);
                    },
                    player =>
                    {
                        player.CardPlay(playerZlab1Id, ItemPosition.Start);
                        player.CardPlay(playerZlab2Id, ItemPosition.Start);
                    },
                    opponent =>
                    {
                        opponent.CardPlay(opponentRockyId, ItemPosition.Start, opponentZlab2Id);
                        opponent.CardAttack(opponentZlab2Id, playerZlab2Id);
                    },
                    player =>
                    {
                        player.CardPlay(playerRockyId, ItemPosition.Start, playerZlab1Id);
                        player.CardAttack(playerZlab1Id, opponentZlab1Id);
                    },
                };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(value, ((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentZlab2Id)).BuffedDefense);
                    Assert.AreEqual(value, ((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerZlab1Id)).BuffedDefense);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            }, 400);
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        [Category("PlayQuickSubset2")]
        public IEnumerator Duzt()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0, new DeckCardData("Duzt", 10));
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0, new DeckCardData("Duzt", 10));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Duzt", 1);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Duzt", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                    player => {},
                    opponent => {},
                    player => player.CardPlay(playerCardId, ItemPosition.Start),
                    opponent => opponent.CardPlay(opponentCardId, ItemPosition.Start),
                };

                Action validateEndState = () =>
                {
                    Assert.IsTrue(((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCardId)).HasHeavy);
                    Assert.IsTrue(((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCardId)).HasHeavy);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Blocker()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0, new DeckCardData("Blocker", 10));
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0, new DeckCardData("Blocker", 10));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Blocker", 1);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Blocker", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                    player => {},
                    opponent => {},
                    player => player.CardPlay(playerCardId, ItemPosition.Start),
                    opponent => opponent.CardPlay(opponentCardId, ItemPosition.Start)
                };

                Action validateEndState = () =>
                {
                    Assert.IsTrue(((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCardId)).IsHeavyUnit);
                    Assert.IsTrue(((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCardId)).IsHeavyUnit);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Pit()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 1,
                    new DeckCardData("Pit", 1),
                    new DeckCardData("Hot", 20)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 1,
                    new DeckCardData("Pit", 1),
                    new DeckCardData("Hot", 20)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Pit", 1);
                InstanceId playerHotId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Hot", 1);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Pit", 1);
                InstanceId opponentHotId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Hot", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                    player =>
                    {
                        player.LetsThink(2);
                        player.CardPlay(playerHotId, ItemPosition.Start);
                        player.CardPlay(playerCardId, ItemPosition.Start);
                        player.CardAbilityUsed(opponentCardId, Enumerators.AbilityType.ADD_GOO_VIAL, new List<ParametrizedAbilityInstanceId>());
                    },
                    opponent =>
                    {
                        opponent.CardPlay(opponentHotId, ItemPosition.Start);
                        opponent.CardPlay(opponentCardId, ItemPosition.Start);
                        opponent.CardAbilityUsed(opponentCardId, Enumerators.AbilityType.ADD_GOO_VIAL, new List<ParametrizedAbilityInstanceId>());
                    },
                    player =>
                    {
                        player.CardAttack(playerHotId, opponentCardId);
                        player.LetsThink(10);
                        player.AssertInQueue(() => {
                            Assert.AreEqual(2, pvpTestContext.GetOpponentPlayer().GooVials);
                        });
                    },
                    opponent =>
                    {
                        opponent.CardAttack(opponentHotId, playerCardId);
                        opponent.LetsThink(10);
                        opponent.AssertInQueue(() => {
                            Assert.AreEqual(3, pvpTestContext.GetCurrentPlayer().GooVials);
                        });
                    },
                };

                Action validateEndState = () => { };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Boulder()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 1, new DeckCardData("Boulder", 1),
                    new DeckCardData("Zlab", 20));
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 1, new DeckCardData("Boulder", 1),
                    new DeckCardData("Zlab", 20));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerBoulderId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Boulder", 1);
                InstanceId playerZlabId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zlab", 1);
                InstanceId opponentBoulderId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Boulder", 1);
                InstanceId opponenZlabId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zlab", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                    player => {},
                    opponent => {},
                    player =>
                    {
                        player.CardPlay(playerZlabId, ItemPosition.Start);
                        player.CardPlay(playerBoulderId, ItemPosition.Start, playerZlabId);
                    },
                    opponent =>
                    {
                        opponent.CardPlay(opponenZlabId, ItemPosition.Start);
                        opponent.CardPlay(opponentBoulderId, ItemPosition.Start, opponenZlabId);
                    },
                    player => {},
                };

                Action validateEndState = () =>
                {
                    Assert.IsTrue(((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerZlabId)).HasHeavy);
                    Assert.IsTrue(((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponenZlabId)).HasHeavy);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Pebble()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 1, new DeckCardData("Pebble", 20));
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 1, new DeckCardData("Pebble", 20));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerPebbleId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Pebble", 1);
                InstanceId opponentPebbleId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Pebble", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                    player => {},
                    opponent => {},
                    player => 
                    {
                        player.CardPlay(playerPebbleId, ItemPosition.Start);
                        player.CardAbilityUsed(opponentPebbleId, Enumerators.AbilityType.ATTACK_OVERLORD, new List<ParametrizedAbilityInstanceId>());
                    },
                    opponent =>
                    {
                        opponent.CardPlay(opponentPebbleId, ItemPosition.Start);
                        opponent.CardAbilityUsed(opponentPebbleId, Enumerators.AbilityType.ATTACK_OVERLORD, new List<ParametrizedAbilityInstanceId>());
                    },
                    player => 
                    {
                        player.CardAttack(playerPebbleId, opponentPebbleId);
                    },
                    opponent => {},
                    player => {}
                };

                Action validateEndState = () =>
                {
                    int damageDealt = 2;
                    Assert.AreEqual(pvpTestContext.GetCurrentPlayer().MaxCurrentDefense-damageDealt, pvpTestContext.GetCurrentPlayer().Defense);
                    Assert.AreEqual(pvpTestContext.GetOpponentPlayer().MaxCurrentDefense-damageDealt, pvpTestContext.GetOpponentPlayer().Defense);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Protector()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 5,
                    new DeckCardData("Protector", 1),
                    new DeckCardData("Hot", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 5,
                    new DeckCardData("Protector", 1),
                    new DeckCardData("Hot", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerProtectorId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Protector", 1);
                InstanceId playerHotId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Hot", 1);
                InstanceId playerHot2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Hot", 2);
                InstanceId opponentProtectorId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Protector", 1);
                InstanceId opponentHotId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Hot", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                       player => {},
                       opponent =>
                       {
                           opponent.CardPlay(opponentHotId, ItemPosition.Start);
                           opponent.CardPlay(opponentProtectorId, ItemPosition.Start, opponentHotId);
                       },
                       player =>
                       {
                           player.CardPlay(playerHotId, ItemPosition.Start);
                           player.CardPlay(playerHot2Id, ItemPosition.Start);
                           player.CardPlay(playerProtectorId, ItemPosition.Start, playerHot2Id);
                       },
                       opponent => {},
                       player => {}
                };

                int value = 4;

                Action validateEndState = () =>
                {
                    Assert.AreEqual(value, ((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerHot2Id)).BuffedDefense);
                    Assert.AreEqual(value, ((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentHotId)).BuffedDefense);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            }, 300);
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Rubble()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0, new DeckCardData("Rubble", 10));
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0, new DeckCardData("Rubble", 10));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Rubble", 1);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Rubble", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                    player => {},
                    opponent => {},
                    player => {
                        player.CardPlay(playerCardId, ItemPosition.Start);
                        player.CardAbilityUsed(playerCardId, Enumerators.AbilityType.MODIFICATOR_STATS, new List<ParametrizedAbilityInstanceId>());
                    },
                    opponent => {
                        opponent.LetsThink(10);
                        opponent.AssertInQueue(() => {
                            Assert.AreEqual(5, ((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCardId)).CurrentDamage);
                        });
                    },
                    player => {
                        player.LetsThink(10);
                        player.AssertInQueue(() => {
                            Assert.AreEqual(3, ((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCardId)).CurrentDamage);
                        });
                    },
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
        public IEnumerator Tiny()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0, new DeckCardData("Tiny", 10));
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0, new DeckCardData("Tiny", 10));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Tiny", 1);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Tiny", 1);
                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                   {
                       player => {},
                       opponent => {},
                       player => {
                           player.CardPlay(playerCardId, ItemPosition.Start);
                           player.CardAbilityUsed(playerCardId, Enumerators.AbilityType.CHANGE_STAT, new List<ParametrizedAbilityInstanceId>());
                       },
                       opponent => {
                           opponent.CardPlay(opponentCardId, ItemPosition.Start);
                           opponent.CardAbilityUsed(opponentCardId, Enumerators.AbilityType.CHANGE_STAT, new List<ParametrizedAbilityInstanceId>());
                       },
                   };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(6, ((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCardId)).CurrentDefense);
                    Assert.AreEqual(6, ((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCardId)).CurrentDefense);
                    Assert.IsTrue(((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCardId)).IsHeavyUnit);
                    Assert.IsTrue(((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCardId)).IsHeavyUnit);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Zlab()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0, new DeckCardData("Zlab", 10));
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0, new DeckCardData("Zlab", 10));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zlab", 1);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zlab", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                    player => {},
                    opponent => {},
                    player => player.CardPlay(playerCardId, ItemPosition.Start),
                    opponent => opponent.CardPlay(opponentCardId, ItemPosition.Start),
                    player => player.CardAttack(playerCardId, opponentCardId),
                    opponent => opponent.CardAttack(opponentCardId, playerCardId),
                    player => {},
                    opponent => {}
                };

                Action validateEndState = () =>
                {
                    Assert.IsNull(TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCardId));
                    Assert.IsNull(TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCardId));
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Shale()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0, new DeckCardData("Shale", 10));
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0, new DeckCardData("Shale", 10));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Shale", 1);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Shale", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                    player => {},
                    opponent => {},
                    player =>
                    {
                        player.CardPlay(playerCardId, ItemPosition.Start);
                        player.CardAbilityUsed(opponentCardId, Enumerators.AbilityType.CHANGE_STAT, new List<ParametrizedAbilityInstanceId>());
                    },
                    opponent => {},
                    player =>
                    {
                        player.CardAttack(playerCardId, pvpTestContext.GetOpponentPlayer().InstanceId);
                    },
                    opponent =>
                    {
                        opponent.CardPlay(opponentCardId, ItemPosition.Start);
                        opponent.CardAbilityUsed(opponentCardId, Enumerators.AbilityType.CHANGE_STAT, new List<ParametrizedAbilityInstanceId>());
                    },
                    player => {},
                    opponent =>
                    {
                        opponent.CardAttack(opponentCardId, pvpTestContext.GetCurrentPlayer().InstanceId);
                    }
                };

                int value = 2;

                Action validateEndState = () =>
                {
                    Assert.AreEqual(value, ((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCardId)).BuffedDefense);
                    Assert.AreEqual(value, ((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCardId)).BuffedDefense);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Hardy()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0, new DeckCardData("Hardy", 10));
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0, new DeckCardData("Hardy", 10));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Hardy", 1);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Hardy", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                    player => {},
                    opponent => {},
                    player => 
                    {
                        player.CardPlay(playerCardId, ItemPosition.Start);
                        player.CardAbilityUsed(playerCardId, Enumerators.AbilityType.CHANGE_STAT, new List<ParametrizedAbilityInstanceId>());
                    },
                    opponent => 
                    {
                        opponent.CardPlay(opponentCardId, ItemPosition.Start);
                        opponent.CardAbilityUsed(opponentCardId, Enumerators.AbilityType.CHANGE_STAT, new List<ParametrizedAbilityInstanceId>());
                    },
                    player => {},
                    opponent => {}
                };

                Action validateEndState = () =>
                {
                    CardModel playerHardyModel = ((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCardId));
                    Assert.AreEqual(5, playerHardyModel.CurrentDefense);
                    CardModel opponentHardyModel = ((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCardId));
                    Assert.AreEqual(5, opponentHardyModel.CurrentDefense);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Crumbz()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0, new DeckCardData("Crumbz", 7));
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0, new DeckCardData("Crumbz", 7));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Crumbz", 1);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Crumbz", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                    player => {},
                    opponent => {},
                    player => player.CardPlay(playerCardId, ItemPosition.Start),
                    opponent => opponent.CardPlay(opponentCardId, ItemPosition.Start),
                    player => player.CardAttack(playerCardId, opponentCardId),
                    opponent => opponent.CardAttack(opponentCardId, playerCardId),
                    player => player.CardAttack(playerCardId, opponentCardId),
                    opponent => {}
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
        public IEnumerator Golem()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0,
                    new DeckCardData("Golem", 1),
                    new DeckCardData("Zlab", 20));
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    new DeckCardData("Golem", 1),
                    new DeckCardData("Zlab", 20));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Golem", 1);
                InstanceId playerZlabId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zlab", 1);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Golem", 1);
                InstanceId opponentZlabId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zlab", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                 {
                     player => {},
                     opponent => {},
                     player =>
                     {
                         player.CardPlay(playerCardId, ItemPosition.Start);
                         player.CardPlay(playerZlabId, ItemPosition.Start);
                     },
                     opponent =>
                     {
                         opponent.CardPlay(opponentCardId, ItemPosition.Start);
                         opponent.CardPlay(opponentZlabId, ItemPosition.Start);
                     },
                     player =>
                     {
                         player.CardAttack(playerZlabId, opponentCardId);
                     },
                     opponent =>
                     {
                         opponent.CardAttack(opponentZlabId, playerCardId);
                         opponent.LetsThink(2);
                     },
                 };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(6, pvpTestContext.GetCurrentPlayer().CardsInHand.Count);
                    Assert.AreEqual(5, pvpTestContext.GetOpponentPlayer().CardsInHand.Count);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Walley()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 1,
                    new DeckCardData("Walley", 1),
                    new DeckCardData("Zlab", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 1,
                    new DeckCardData("Walley", 1),
                    new DeckCardData("Zlab", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerWalleyId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Walley", 1);
                InstanceId playerZlab1Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zlab", 1);
                InstanceId playerZlab2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zlab", 2);
                InstanceId opponentWalleyId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Walley", 1);
                InstanceId opponentZlab1Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zlab", 1);
                InstanceId opponentZlab2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zlab", 2);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                    player => {},
                    opponent => {},
                    player => {
                        player.CardPlay(playerZlab1Id, ItemPosition.Start);
                        player.CardPlay(playerZlab2Id, ItemPosition.Start);
                        player.CardPlay(playerWalleyId, ItemPosition.Start);
                    },
                    opponent => {
                        opponent.CardPlay(opponentZlab1Id, ItemPosition.Start);
                        opponent.CardPlay(opponentZlab2Id, ItemPosition.Start);
                        opponent.CardPlay(opponentWalleyId, ItemPosition.Start);
                    },
                    player => {}
                };
                Action validateEndState = () =>
                {
                    CardModel playerZlab1Unit = ((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerZlab1Id));
                    CardModel playerZlab2Unit = ((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerZlab2Id));
                    CardModel opponentZlab1Unit = ((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentZlab1Id));
                    CardModel opponentZlab2Unit = ((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentZlab2Id));

                    Assert.AreEqual(playerZlab1Unit.Card.Prototype.Defense, playerZlab1Unit.CurrentDefense);
                    Assert.AreEqual(playerZlab1Unit.Card.Prototype.Damage, playerZlab1Unit.CurrentDamage);
                    Assert.AreEqual(playerZlab2Unit.Card.Prototype.Defense + 2, playerZlab2Unit.CurrentDefense);
                    Assert.AreEqual(playerZlab2Unit.Card.Prototype.Damage + 1, playerZlab2Unit.CurrentDamage);
                    Assert.IsTrue(!playerZlab1Unit.IsHeavyUnit);
                    Assert.IsTrue(playerZlab2Unit.IsHeavyUnit);

                    Assert.AreEqual(opponentZlab1Unit.Card.Prototype.Defense, opponentZlab1Unit.CurrentDefense);
                    Assert.AreEqual(opponentZlab1Unit.Card.Prototype.Damage, opponentZlab1Unit.CurrentDamage);
                    Assert.AreEqual(opponentZlab2Unit.Card.Prototype.Defense + 2, opponentZlab2Unit.CurrentDefense);
                    Assert.AreEqual(opponentZlab2Unit.Card.Prototype.Damage + 1, opponentZlab2Unit.CurrentDamage);
                    Assert.IsTrue(!opponentZlab1Unit.IsHeavyUnit);
                    Assert.IsTrue(opponentZlab2Unit.IsHeavyUnit);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Spiker()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0, new DeckCardData("Spiker", 10));
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0, new DeckCardData("Spiker", 10));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Spiker", 1);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Spiker", 1);

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
                    Assert.IsTrue(((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCardId)).IsHeavyUnit);
                    Assert.IsTrue(((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCardId)).IsHeavyUnit);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Crater()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0, new DeckCardData("Crater", 10));
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0, new DeckCardData("Crater", 10));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Crater", 1);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Crater", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                    player =>
                    {
                        player.LetsThink(2);
                        player.CardPlay(playerCardId, ItemPosition.Start);
                        player.LetsThink(2);
                        player.AssertInQueue(() => {
                            Assert.AreEqual(2, pvpTestContext.GetCurrentPlayer().GooVials);
                        });
                    },
                    opponent =>
                    {
                        opponent.LetsThink(2);
                        opponent.CardPlay(opponentCardId, ItemPosition.Start);
                        opponent.LetsThink(2);
                        opponent.AssertInQueue(() => {
                            Assert.AreEqual(2, pvpTestContext.GetOpponentPlayer().GooVials);
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
        public IEnumerator Groundy()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0, new DeckCardData("Groundy", 10));
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0, new DeckCardData("Groundy", 10));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Groundy", 1);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Groundy", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                       player => {},
                       opponent => {},
                       player => player.CardPlay(playerCardId, ItemPosition.Start),
                       opponent =>
                       {
                           opponent.CardPlay(opponentCardId, ItemPosition.Start);
                           opponent.CardAbilityUsed(opponentCardId, Enumerators.AbilityType.TAKE_UNIT_TYPE_TO_ALLY_UNIT, new List<ParametrizedAbilityInstanceId>());
                           opponent.LetsThink(2);
                       },
                };

                Action validateEndState = () =>
                {
                    Assert.IsTrue(((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCardId)).IsHeavyUnit);
                    Assert.IsTrue(((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCardId)).IsHeavyUnit);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Earthshaker()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 1,
                                                                   new DeckCardData("Earthshaker", 3),
                                                                   new DeckCardData("Zolid", 20));
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 1,
                                                                   new DeckCardData("Earthshaker", 3),
                                                                   new DeckCardData("Zolid", 20));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerEarthshakerId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Earthshaker", 1);
                InstanceId playerZolidId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zolid", 1);
                InstanceId opponentEarthshakerId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Earthshaker", 1);
                InstanceId opponentZolidId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zolid", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                       player => {},
                       opponent => {},
                       player => player.CardPlay(playerZolidId, ItemPosition.Start),
                       opponent => opponent.CardPlay(opponentZolidId, ItemPosition.Start),
                       player => player.CardPlay(playerEarthshakerId, ItemPosition.Start, opponentZolidId),
                       opponent =>
                       {
                           opponent.CardPlay(opponentEarthshakerId, ItemPosition.Start, playerZolidId);
                           opponent.LetsThink(2);
                       },
                };

                Action validateEndState = () =>
                {
                    Assert.IsNull(TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerZolidId));
                    Assert.IsNull(TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentZolidId));
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator IgneouZ()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 1,
                    new DeckCardData("IgneouZ", 1),
                    new DeckCardData("Hot", 20)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 1,
                    new DeckCardData("IgneouZ", 1),
                    new DeckCardData("Hot", 20)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "IgneouZ", 1);
                InstanceId playerHotId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Hot", 1);
                InstanceId playerHot2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Hot", 2);
                InstanceId playerHot3Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Hot", 3);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "IgneouZ", 1);
                InstanceId opponentHotId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Hot", 1);
                InstanceId opponentHot2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Hot", 2);
                InstanceId opponentHot3Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Hot", 3);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                       player => {},
                       opponent => {},
                       player =>
                       {
                           player.CardPlay(playerHotId, ItemPosition.Start);
                           player.CardPlay(playerHot2Id, ItemPosition.Start);
                           player.CardPlay(playerHot3Id, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentHotId, ItemPosition.Start);
                           opponent.CardPlay(opponentHot2Id, ItemPosition.Start);
                           opponent.CardPlay(opponentHot3Id, ItemPosition.Start);
                       },
                       player =>
                       {
                           player.CardAttack(playerHotId, pvpTestContext.GetOpponentPlayer().InstanceId);
                           player.CardAttack(playerHot2Id, pvpTestContext.GetOpponentPlayer().InstanceId);
                           player.CardAttack(playerHot3Id, pvpTestContext.GetOpponentPlayer().InstanceId);
                       },
                       opponent =>
                       {
                           opponent.CardAttack(opponentHotId, pvpTestContext.GetCurrentPlayer().InstanceId);
                           opponent.CardAttack(opponentHot2Id, pvpTestContext.GetCurrentPlayer().InstanceId);
                           opponent.CardAttack(opponentHot3Id, pvpTestContext.GetCurrentPlayer().InstanceId);
                       },
                       player => {},
                       opponent => {},
                       player => player.CardPlay(playerCardId, ItemPosition.Start),
                       opponent => opponent.CardPlay(opponentCardId, ItemPosition.Start)
                };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(7, ((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCardId)).CurrentDefense);
                    Assert.AreEqual(7, ((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCardId)).CurrentDefense);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Pyrite()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 1, new DeckCardData("Pyrite", 10));
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 1, new DeckCardData("Pyrite", 10));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Pyrite", 1);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Pyrite", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                       player => {},
                       opponent => {},
                       player => {
                           player.CardPlay(playerCardId, ItemPosition.Start);
                           player.CardAbilityUsed(playerCardId, Enumerators.AbilityType.RAGE, new List<ParametrizedAbilityInstanceId>());
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentCardId, ItemPosition.Start);
                           opponent.CardAbilityUsed(opponentCardId, Enumerators.AbilityType.RAGE, new List<ParametrizedAbilityInstanceId>());
                       },
                       player => player.CardAttack(playerCardId, opponentCardId),
                       opponent => {}
                };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(10, ((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCardId)).CurrentDamage);
                    Assert.AreEqual(10, ((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCardId)).CurrentDamage);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        [Category("PlayQuickSubset2")]
        public IEnumerator Zolid()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0, new DeckCardData("Zolid", 10));
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0, new DeckCardData("Zolid", 10));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zolid", 1);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zolid", 1);

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
                    Assert.IsTrue(((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCardId)).IsHeavyUnit);
                    Assert.IsTrue(((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCardId)).IsHeavyUnit);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Defender()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0,
                    new DeckCardData("Defender", 1),
                    new DeckCardData("Breezee", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    new DeckCardData("Defender", 1),
                    new DeckCardData("Breezee", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerDefenderId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Defender", 1);
                InstanceId playerBreezeeId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Breezee", 1);
                InstanceId opponentDefenderId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Defender", 1);
                InstanceId opponentBreezeeId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Breezee", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                    player => {},
                    opponent => {},
                    player =>
                    {
                        player.CardPlay(playerDefenderId, ItemPosition.Start);
                        player.CardAbilityUsed(playerDefenderId, Enumerators.AbilityType.RAGE, new List<ParametrizedAbilityInstanceId>());
                        player.CardPlay(playerBreezeeId, ItemPosition.Start);
                    },
                    opponent =>
                    {
                        opponent.CardPlay(opponentDefenderId, ItemPosition.Start);
                        opponent.CardAbilityUsed(opponentDefenderId, Enumerators.AbilityType.RAGE, new List<ParametrizedAbilityInstanceId>());
                        opponent.CardPlay(opponentBreezeeId, ItemPosition.Start);
                    },
                    player => player.CardAttack(playerBreezeeId, opponentDefenderId),
                    opponent => opponent.CardAttack(opponentBreezeeId, playerDefenderId),
                    player => {},
                    opponent => {},
                };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(7, ((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerDefenderId)).CurrentDamage);
                    Assert.IsTrue(((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerDefenderId)).IsHeavyUnit);
                    Assert.AreEqual(7, ((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentDefenderId)).CurrentDamage);
                    Assert.IsTrue(((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentDefenderId)).IsHeavyUnit);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Mountain()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 1,
                    new DeckCardData("Mountain", 1),
                    new DeckCardData("Hot", 20)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 1,
                    new DeckCardData("Mountain", 1),
                    new DeckCardData("Hot", 20)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerMountainId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Mountain", 1);
                InstanceId playerHotId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Hot", 1);
                InstanceId opponentMountainId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Mountain", 1);
                InstanceId opponentHotId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Hot", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                       player => {},
                       opponent => {},
                       player =>
                       {
                           player.CardPlay(playerMountainId, ItemPosition.Start);
                           player.CardAbilityUsed(playerMountainId, Enumerators.AbilityType.BLOCK_TAKE_DAMAGE, new List<ParametrizedAbilityInstanceId>());
                           player.CardPlay(playerHotId, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentHotId, ItemPosition.Start);
                           opponent.CardPlay(opponentMountainId, ItemPosition.Start);
                           opponent.CardAbilityUsed(opponentMountainId, Enumerators.AbilityType.BLOCK_TAKE_DAMAGE, new List<ParametrizedAbilityInstanceId>());
                       },
                       player =>
                       {
                           player.CardAttack(playerHotId, opponentMountainId);
                       },
                       opponent =>
                       {
                           opponent.CardAttack(opponentHotId, playerMountainId);
                       },
                };

                int value = 2;

                Action validateEndState = () =>
                {
                    Assert.AreEqual(((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerMountainId)).Prototype.Defense - value,
                        ((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerMountainId)).CurrentDefense);
                    Assert.AreEqual(((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentMountainId)).Prototype.Defense - value,
                        ((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentMountainId)).CurrentDefense);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Gaea()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 1,
                                                                                  new DeckCardData("Gaea", 1),
                                                                                  new DeckCardData("Trunk", 20));
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 1,
                                                                    new DeckCardData("Gaea", 1),
                                                                    new DeckCardData("Trunk", 20));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerGaeaId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Gaea", 1);
                InstanceId playerTrunk1Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Trunk", 1);
                InstanceId playerTrunk2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Trunk", 2);
                InstanceId opponentGaeaId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Gaea", 1);
                InstanceId opponentTrunk1Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Trunk", 1);
                InstanceId opponentTrunk2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Trunk", 2);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                       player => {},
                       opponent => {},
                       player =>
                       {
                           player.CardPlay(playerTrunk1Id, ItemPosition.Start);
                           player.CardPlay(playerTrunk2Id, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentTrunk1Id, ItemPosition.Start);
                           opponent.CardPlay(opponentTrunk2Id, ItemPosition.Start);
                       },
                       player => player.CardPlay(playerGaeaId, ItemPosition.Start),
                       opponent => opponent.CardPlay(opponentGaeaId, ItemPosition.Start),
                       player => {},
                       opponent => {}
                };

                Action validateEndState = () =>
                {
                    Assert.IsTrue(((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerTrunk1Id)).HasHeavy);
                    Assert.IsTrue(((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerTrunk2Id)).HasHeavy);
                    Assert.IsTrue(((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentTrunk1Id)).HasHeavy);
                    Assert.IsTrue(((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentTrunk2Id)).HasHeavy);
                    Assert.IsFalse(((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerGaeaId)).HasHeavy);
                    Assert.IsFalse(((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentGaeaId)).HasHeavy);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        [Category("PlayQuickSubset2")]
        public IEnumerator Zpike()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 1,
                    new DeckCardData("Zpike", 1),
                    new DeckCardData("Yggdrazil", 10));
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 1,
                                                                    new DeckCardData("Zpike", 1),
                                                                    new DeckCardData("Yggdrazil", 10));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerZpikeId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zpike", 1);
                InstanceId playerYggdrazilId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Yggdrazil", 1);
                InstanceId opponentZpikeId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zpike", 1);
                InstanceId opponentYggdrazilId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Yggdrazil", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                       player => {},
                       opponent => {},
                       player =>
                       {
                           player.CardPlay(playerYggdrazilId, ItemPosition.Start);
                           player.CardPlay(playerZpikeId, ItemPosition.Start);
                           player.CardAbilityUsed(playerZpikeId, Enumerators.AbilityType.DEAL_DAMAGE_TO_TARGET_THAT_ATTACK_THIS, new List<ParametrizedAbilityInstanceId>());
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentYggdrazilId, ItemPosition.Start);
                           opponent.CardPlay(opponentZpikeId, ItemPosition.Start);
                           opponent.CardAbilityUsed(opponentZpikeId, Enumerators.AbilityType.DEAL_DAMAGE_TO_TARGET_THAT_ATTACK_THIS, new List<ParametrizedAbilityInstanceId>());
                       },
                       player =>
                       {
                           player.CardAttack(playerYggdrazilId, opponentZpikeId);
                       },
                       opponent =>
                       {
                           opponent.CardAttack(opponentYggdrazilId, playerZpikeId);
                           opponent.LetsThink(2);
                       }
                };

                Action validateEndState = () =>
                {
                    CardModel playerUnit = (CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerYggdrazilId);
                    CardModel opponentUnit = (CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentYggdrazilId);
                    Assert.IsNull(playerUnit);
                    Assert.IsNull(opponentUnit);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }
    }
}
