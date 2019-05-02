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
    public class ToxicCardsTests : BaseIntegrationTest
    {
        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator RelentleZZ()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0,
                    new TestCardData("RelentleZZ", 1),
                    new TestCardData("Trunk", 20)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    new TestCardData("RelentleZZ", 1),
                    new TestCardData("Trunk", 20)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerRelentleZZId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "RelentleZZ", 1);
                InstanceId playerTrunkId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Trunk", 1);
                InstanceId opponentRelentleZZId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "RelentleZZ", 1);
                InstanceId opponentTrunkId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Trunk", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                       player => {},
                       opponent => {},
                       player => 
                       {
                           player.CardPlay(playerTrunkId, ItemPosition.Start);
                           player.CardPlay(playerRelentleZZId, ItemPosition.Start);
                           player.CardAbilityUsed(playerRelentleZZId, Enumerators.AbilityType.CARD_RETURN, new List<ParametrizedAbilityInstanceId>());
                       },
                       opponent => 
                       {
                           opponent.CardPlay(opponentTrunkId, ItemPosition.Start);
                           opponent.CardPlay(opponentRelentleZZId, ItemPosition.Start);
                           opponent.CardAbilityUsed(opponentRelentleZZId, Enumerators.AbilityType.CARD_RETURN, new List<ParametrizedAbilityInstanceId>());
                       },
                       player => 
                       {
                           player.CardAttack(playerTrunkId, opponentRelentleZZId);
                       },
                       opponent => 
                       {
                           opponent.CardAttack(opponentTrunkId, playerRelentleZZId);
                       },
                       player => {},
                       opponent => {}
                };

                Action validateEndState = () =>
                {
                    int costIncrease = 1;
                    CardModel playerRelentleZZModel = ((CardModel)TestHelper.BattlegroundController.GetCardModelByInstanceId(playerRelentleZZId));
                    CardModel opponentRelentleZZModel = ((CardModel)TestHelper.BattlegroundController.GetCardModelByInstanceId(opponentRelentleZZId));
                    Assert.AreEqual(playerRelentleZZModel.Card.Prototype.Cost+costIncrease, playerRelentleZZModel.CurrentCost);
                    Assert.AreEqual(playerRelentleZZModel.Card.Prototype.Cost+costIncrease, playerRelentleZZModel.CurrentCost);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Zpitter()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0, new TestCardData("Zpitter", 20));
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0, new TestCardData("Zpitter", 20));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zpitter", 1);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zpitter", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                   {
                       player => {},
                       opponent => {},
                       player => {
                           player.CardPlay(playerCardId, ItemPosition.Start);
                           player.CardAbilityUsed(playerCardId, Enumerators.AbilityType.ATTACK_OVERLORD, new List<ParametrizedAbilityInstanceId>());
                       },
                       opponent => {
                           opponent.CardPlay(opponentCardId, ItemPosition.Start);
                           opponent.CardAbilityUsed(opponentCardId, Enumerators.AbilityType.ATTACK_OVERLORD, new List<ParametrizedAbilityInstanceId>());
                       },
                       player => {},
                       opponent => {},
                   };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(TestHelper.GetCurrentPlayer().InitialDefense - 2, TestHelper.GetCurrentPlayer().Defense);
                    Assert.AreEqual(TestHelper.GetOpponentPlayer().InitialDefense - 2, TestHelper.GetOpponentPlayer().Defense);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            }, 300);
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Ectoplazm()
        {
            return AsyncTest(async () =>
            {

                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 4,
                    new TestCardData("Ectoplazm", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 4,
                    new TestCardData("Ectoplazm", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Ectoplazm", 1);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Ectoplazm", 1);
                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                   {
                       player => {},
                       opponent => {},
                       player =>
                       {
                           player.CardPlay(playerCardId, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentCardId, ItemPosition.Start);
                       },
                       player => {}
                   };

                Action validateEndState = () =>
                {
                    
                    Assert.AreEqual(2, pvpTestContext.GetCurrentPlayer().GooVials);
                    Assert.AreEqual(2, pvpTestContext.GetOpponentPlayer().GooVials);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Ghoul()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0,
                    new TestCardData("Ghoul", 1),
                    new TestCardData("Duzt", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    new TestCardData("Ghoul", 1),
                    new TestCardData("Duzt", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Ghoul", 1);
                InstanceId playerDuztId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Duzt", 1);
                InstanceId playerDuzt2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Duzt", 2);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Ghoul", 1);
                InstanceId opponentDuztId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Duzt", 1);
                InstanceId opponentDuzt2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Duzt", 2);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                   {
                       player => {},
                       opponent => {},
                       player =>
                       {
                           player.CardPlay(playerDuztId, ItemPosition.Start);
                           player.CardPlay(playerDuzt2Id, ItemPosition.Start);
                           player.CardPlay(playerCardId, ItemPosition.Start);
                           player.CardAbilityUsed(playerCardId, Enumerators.AbilityType.ATTACK_OVERLORD, new List<ParametrizedAbilityInstanceId>());
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentDuztId, ItemPosition.Start);
                           opponent.CardPlay(opponentDuzt2Id, ItemPosition.Start);
                           opponent.CardPlay(opponentCardId, ItemPosition.Start);
                           opponent.CardAbilityUsed(opponentCardId, Enumerators.AbilityType.ATTACK_OVERLORD, new List<ParametrizedAbilityInstanceId>());
                       },
                       player =>
                       {
                           player.CardAttack(playerCardId, opponentDuztId);
                       },
                       opponent =>
                       {
                           opponent.CardAttack(opponentCardId, playerDuztId);
                       },
                       player =>
                       {
                           player.CardAttack(playerCardId, opponentDuzt2Id);
                       },
                       opponent =>
                       {
                           opponent.CardAttack(opponentCardId, playerDuzt2Id);
                       },
                       player => {}
                   };

                Action validateEndState = () =>
                {
                    int value = 6;

                    Assert.AreEqual(pvpTestContext.GetCurrentPlayer().InitialDefense - value, pvpTestContext.GetCurrentPlayer().Defense);
                    Assert.AreEqual(pvpTestContext.GetOpponentPlayer().InitialDefense - value, pvpTestContext.GetOpponentPlayer().Defense);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Wazte()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 4,
                    new TestCardData("Wazte", 1),
                    new TestCardData("Hot", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 4,
                    new TestCardData("Wazte", 1),
                    new TestCardData("Hot", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Wazte", 1);
                InstanceId playerHotId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Hot", 1);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Wazte", 1);
                InstanceId opponentHotId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Hot", 1);
                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                   {
                       player => {},
                       opponent => {},
                       player =>
                       {
                           player.CardPlay(playerHotId, ItemPosition.Start);
                           player.CardPlay(playerCardId, ItemPosition.Start);
                           player.CardAbilityUsed(playerCardId, Enumerators.AbilityType.LOSE_GOO, new List<ParametrizedAbilityInstanceId>());
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentHotId, ItemPosition.Start);
                           opponent.CardPlay(opponentCardId, ItemPosition.Start);
                           opponent.CardAbilityUsed(opponentCardId, Enumerators.AbilityType.LOSE_GOO, new List<ParametrizedAbilityInstanceId>());
                       },
                       player =>
                       {
                           player.CardAttack(playerHotId, opponentCardId);
                       },
                       opponent =>
                       {
                           opponent.CardAttack(opponentHotId, playerCardId);
                       },
                       player => {},
                   };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(3, pvpTestContext.GetOpponentPlayer().GooVials);
                    Assert.AreEqual(3, pvpTestContext.GetCurrentPlayer().GooVials);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Azzazzin()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0,
                    new TestCardData("Azzazzin", 2),
                    new TestCardData("Cerberuz", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    new TestCardData("Azzazzin", 2),
                    new TestCardData("Cerberuz", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerCerberuzId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Cerberuz", 1);
                InstanceId opponentCerberuzId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Cerberuz", 1);
                InstanceId playerAzzazzinId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Azzazzin", 1);
                InstanceId opponentAzzazzinId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Azzazzin", 1);
                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                   {
                       player => {},
                       opponent => {},
                       player => player.CardPlay(playerCerberuzId, ItemPosition.Start),
                       opponent => opponent.CardPlay(opponentCerberuzId, ItemPosition.Start),
                       player =>
                       {
                           player.CardPlay(playerAzzazzinId, ItemPosition.Start);
                           player.CardAbilityUsed(playerAzzazzinId, Enumerators.AbilityType.DESTROY_TARGET_UNIT_AFTER_ATTACK, new List<ParametrizedAbilityInstanceId>());
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentAzzazzinId, ItemPosition.Start);
                           opponent.CardAbilityUsed(opponentAzzazzinId, Enumerators.AbilityType.DESTROY_TARGET_UNIT_AFTER_ATTACK, new List<ParametrizedAbilityInstanceId>());
                       },
                       player => 
                       {
                           player.CardAttack(playerAzzazzinId, opponentCerberuzId);
                       },
                       opponent => 
                       {
                           opponent.CardAttack(opponentAzzazzinId, playerCerberuzId);
                       },
                       player => {},
                       opponent => {}
                   };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(0, pvpTestContext.GetCurrentPlayer().CardsOnBoard.Count);
                    Assert.AreEqual(0, pvpTestContext.GetOpponentPlayer().CardsOnBoard.Count);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Hazzard()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 4,
                    new TestCardData("Hazzard", 2),
                    new TestCardData("Trunk", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 4,
                    new TestCardData("Hazzard", 2),
                    new TestCardData("Trunk", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerHazzardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Hazzard", 1);
                InstanceId opponentHazzardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Hazzard", 1);
                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                   {
                       player => {},
                       opponent => {},
                       player =>
                       {
                           player.CardPlay(playerHazzardId, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentHazzardId, ItemPosition.Start);
                       },
                       player => {},
                       opponent => {},
                   };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(2, pvpTestContext.GetCurrentPlayer().GooVials);
                    Assert.AreEqual(1, pvpTestContext.GetOpponentPlayer().GooVials);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Zlimey()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0,
                    new TestCardData("Zlimey", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    new TestCardData("Zlimey", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerZlimeyId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zlimey", 1);
                InstanceId playerZlimey2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zlimey", 2);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zlimey", 1);
                InstanceId opponentZlimey2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zlimey", 2);

                int countCardAtPlayer = 0;
                int countCardsAtOpponent = 0;

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                   {
                       player =>
                       {
                           countCardAtPlayer = pvpTestContext.GetCurrentPlayer().CardsInHand.Count;
                           player.CardPlay(playerZlimeyId, ItemPosition.Start);
                           player.CardAbilityUsed(playerZlimeyId, Enumerators.AbilityType.DISCARD_CARD_FROM_HAND, new List<ParametrizedAbilityInstanceId>()
                           {
                               new ParametrizedAbilityInstanceId(playerZlimey2Id)
                           });
                           player.LetsThink(10);
                           player.AssertInQueue(() =>
                           {
                                Assert.AreEqual(countCardAtPlayer - 2, pvpTestContext.GetCurrentPlayer().CardsInHand.Count);
                           });

                       },
                       opponent =>
                       {
                           countCardsAtOpponent = pvpTestContext.GetOpponentPlayer().CardsInHand.Count;
                           opponent.CardPlay(opponentCardId, ItemPosition.Start);
                           opponent.CardAbilityUsed(opponentCardId, Enumerators.AbilityType.DISCARD_CARD_FROM_HAND, new List<ParametrizedAbilityInstanceId>()
                           {
                               new ParametrizedAbilityInstanceId(opponentZlimey2Id)
                           });
                           opponent.LetsThink(10);
                           opponent.AssertInQueue(() =>
                           {
                                Assert.AreEqual(countCardsAtOpponent - 2, pvpTestContext.GetOpponentPlayer().CardsInHand.Count);
                           });
                       }
                   };

                Action validateEndState = () => {};

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        [Category("PlayQuickSubset2")]
        public IEnumerator Hazmat()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0,
                    new TestCardData("Hazmat", 1),
                    new TestCardData("Zlab", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    new TestCardData("Hazmat", 1),
                    new TestCardData("Zlab", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Hazmat", 1);
                InstanceId playerZlabId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zlab", 1);
                InstanceId playerZlab2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zlab", 2);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Hazmat", 1);
                InstanceId opponentZlabId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zlab", 1);
                InstanceId opponentZlab2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zlab", 2);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                       player => {},
                       opponent => {},
                       player =>
                       {
                           player.CardPlay(playerZlabId, ItemPosition.Start);
                           player.CardPlay(playerZlab2Id, ItemPosition.Start);
                           player.LetsThink(1);
                           player.CardPlay(playerCardId, ItemPosition.Start, playerZlabId);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentZlabId, ItemPosition.Start);
                           opponent.CardPlay(opponentZlab2Id, ItemPosition.Start);
                           opponent.LetsThink(1);
                           opponent.CardPlay(opponentCardId, ItemPosition.Start, opponentZlab2Id);
                       },
                       player => {},
                       opponent => {}
                };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(true, ((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerZlabId))
                        .GameMechanicDescriptionsOnUnit.Contains(Enumerators.GameMechanicDescription.Destroy));
                    Assert.AreEqual(true, ((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentZlab2Id))
                        .GameMechanicDescriptionsOnUnit.Contains(Enumerators.GameMechanicDescription.Destroy));
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Zeptic()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0,
                    new TestCardData("Zeptic", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    new TestCardData("Zeptic", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zeptic", 1);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zeptic", 1);

                CardModel playerUnit = null;
                CardModel opponentUnit = null;

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                   {
                       player => {},
                       opponent => {},
                       player => {},
                       opponent => {},
                       player =>
                       {
                           player.CardPlay(playerCardId, ItemPosition.Start);
                           player.LetsThink(2);
                           player.AssertInQueue(() =>
                           {
                                playerUnit = (CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCardId);
                                Assert.AreEqual(TestHelper.GetCurrentPlayer().CurrentGoo + playerUnit.Prototype.Defense, playerUnit.CurrentDefense);
                                Assert.AreEqual(TestHelper.GetCurrentPlayer().CurrentGoo + playerUnit.Prototype.Damage, playerUnit.CurrentDamage);
                           });
                           player.LetsThink(2);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentCardId, ItemPosition.Start);
                           opponent.LetsThink(2);
                           opponent.AssertInQueue(() =>
                           {
                                opponentUnit = (CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCardId);
                                Assert.AreEqual(TestHelper.GetOpponentPlayer().CurrentGoo + opponentUnit.Prototype.Defense, opponentUnit.CurrentDefense);
                                Assert.AreEqual(TestHelper.GetOpponentPlayer().CurrentGoo + opponentUnit.Prototype.Damage, opponentUnit.CurrentDamage);
                           });
                           opponent.LetsThink(2);
                           
                       }
                   };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(TestHelper.GetCurrentPlayer().CurrentGoo + playerUnit.Prototype.Defense, playerUnit.CurrentDefense);
                    Assert.AreEqual(TestHelper.GetCurrentPlayer().CurrentGoo + playerUnit.Prototype.Damage, playerUnit.CurrentDamage);
                    Assert.AreEqual(TestHelper.GetOpponentPlayer().CurrentGoo + opponentUnit.Prototype.Defense, opponentUnit.CurrentDefense);
                    Assert.AreEqual(TestHelper.GetOpponentPlayer().CurrentGoo + opponentUnit.Prototype.Damage, opponentUnit.CurrentDamage);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Germ()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0,
                    new TestCardData("Germ", 1),
                    new TestCardData("Hot", 20)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    new TestCardData("Germ", 1),
                    new TestCardData("Hot", 20)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Germ", 1);
                InstanceId playerHotId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Hot", 1);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Germ", 1);
                InstanceId opponentHotId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Hot", 1);

                int damage = 2;
                int defence = -1;

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                    player => { },
                    opponent => { },
                    player =>
                    {
                        player.CardPlay(playerHotId, ItemPosition.Start);
                        player.CardPlay(playerCardId, ItemPosition.Start, playerHotId);
                    },
                    opponent =>
                    {
                        opponent.CardPlay(opponentHotId, ItemPosition.Start);
                        opponent.CardPlay(opponentCardId, ItemPosition.Start, opponentHotId);
                    },
                };

                Action validateEndState = () =>
                {
                    CardModel playerUnit = (CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerHotId);
                    CardModel opponentUnit = (CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentHotId);
                    Assert.AreEqual(damage, playerUnit.BuffedDamage);
                    Assert.AreEqual(defence, playerUnit.BuffedDefense);
                    Assert.AreEqual(damage, opponentUnit.BuffedDamage);
                    Assert.AreEqual(defence, opponentUnit.BuffedDefense);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            }, 400);
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Zcavenger()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0,
                   new TestCardData("Zcavenger", 20)
               );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    new TestCardData("Zcavenger", 20)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zcavenger", 1);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zcavenger", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                    player => {},
                    opponent => {},
                    player =>
                    {
                        player.CardPlay(playerCardId, ItemPosition.Start);
                    },
                    opponent =>
                    {
                        opponent.CardPlay(opponentCardId, ItemPosition.Start);
                    },
                    player => {},
                    opponent => {}
                };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(8, pvpTestContext.GetCurrentPlayer().CardsInHand.Count);
                    Assert.AreEqual(8, pvpTestContext.GetOpponentPlayer().CardsInHand.Count);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Zkewer()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0,
                    new TestCardData("Zkewer", 1),
                    new TestCardData("Pyrite", 5)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    new TestCardData("Zkewer", 1),
                    new TestCardData("Pyrite", 5)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerZkewerId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zkewer", 1);
                InstanceId playerPyriteId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Pyrite", 1);
                InstanceId opponentZkewerId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zkewer", 1);
                InstanceId opponentPyriteId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Pyrite", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                       player => {},
                       opponent => {},
                       player =>
                       {
                           player.CardPlay(playerZkewerId, ItemPosition.Start);
                           player.CardAbilityUsed(playerZkewerId, Enumerators.AbilityType.DESTROY_TARGET_UNIT_AFTER_ATTACK, new List<ParametrizedAbilityInstanceId>());
                           player.CardAbilityUsed(playerZkewerId, Enumerators.AbilityType.CHANGE_COST, new List<ParametrizedAbilityInstanceId>());
                           player.CardPlay(playerPyriteId, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentZkewerId, ItemPosition.Start);
                           opponent.CardAbilityUsed(opponentZkewerId, Enumerators.AbilityType.DESTROY_TARGET_UNIT_AFTER_ATTACK, new List<ParametrizedAbilityInstanceId>());
                           opponent.CardAbilityUsed(opponentZkewerId, Enumerators.AbilityType.CHANGE_COST, new List<ParametrizedAbilityInstanceId>());
                           opponent.CardPlay(opponentPyriteId, ItemPosition.Start);
                       },
                       player =>
                       {
                           player.LetsThink();
                           player.CardAttack(playerZkewerId, opponentPyriteId);
                           player.LetsThink();
                       },
                       opponent =>
                       {
                           opponent.LetsThink();
                           opponent.CardAttack(opponentZkewerId, playerPyriteId);
                           opponent.LetsThink(3);
                       },
                };

                Action validateEndState = () =>
                {
                    Assert.IsNull(TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerPyriteId));
                    Assert.IsNull(TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentPyriteId));

                    foreach(CardModel card in TestHelper.GameplayManager.CurrentPlayer.PlayerCardsController.CardsInHand)
                    {
                        Assert.AreEqual(card.Prototype.Cost, card.CurrentCost);
                    }

                    foreach (CardModel card in TestHelper.GameplayManager.OpponentPlayer.PlayerCardsController.CardsInHand)
                    {
                        Assert.AreEqual(card.Prototype.Cost, card.CurrentCost);
                    }
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Bane()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0,
                    new TestCardData("Bane", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    new TestCardData("Bane", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Bane", 1);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Bane", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                   {
                       player => {},
                       opponent => {},
                       player => 
                       {
                           player.CardPlay(playerCardId, ItemPosition.Start);
                           player.CardAbilityUsed(playerCardId, Enumerators.AbilityType.ATTACK_OVERLORD, new List<ParametrizedAbilityInstanceId>());
                       },
                       opponent => 
                       {
                           opponent.CardPlay(opponentCardId, ItemPosition.Start);
                           opponent.CardAbilityUsed(opponentCardId, Enumerators.AbilityType.ATTACK_OVERLORD, new List<ParametrizedAbilityInstanceId>());
                       },
                       player => 
                       {
                           player.CardAttack(playerCardId, opponentCardId);
                       },
                       opponent => {},
                       player => {}
                   };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(pvpTestContext.GetCurrentPlayer().InitialDefense - 5, pvpTestContext.GetCurrentPlayer().Defense);
                    Assert.AreEqual(pvpTestContext.GetOpponentPlayer().InitialDefense - 5, pvpTestContext.GetOpponentPlayer().Defense);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Polluter()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0,
                    new TestCardData("Polluter", 1),
                    new TestCardData("Trunk", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    new TestCardData("Polluter", 1),
                    new TestCardData("Trunk", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerPolluterId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Polluter", 1);
                InstanceId playerTrunkId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Trunk", 1);
                InstanceId opponentPolluterId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Polluter", 1);
                InstanceId opponentTrunkId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Trunk", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                       player =>
                       {
                           player.CardPlay(playerPolluterId, ItemPosition.Start);
                           player.CardAbilityUsed(playerPolluterId, Enumerators.AbilityType.GET_GOO_THIS_TURN, new List<ParametrizedAbilityInstanceId>());
                           player.CardPlay(playerTrunkId, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentPolluterId, ItemPosition.Start);
                           opponent.CardAbilityUsed(opponentPolluterId, Enumerators.AbilityType.GET_GOO_THIS_TURN, new List<ParametrizedAbilityInstanceId>());
                           opponent.CardPlay(opponentTrunkId, ItemPosition.Start);
                       },
                       player =>
                       {
                           player.CardAttack(playerPolluterId, opponentTrunkId);
                           player.LetsThink(10);
                           player.AssertInQueue(() => {
                                Assert.AreEqual(3, TestHelper.GetCurrentPlayer().CurrentGoo);
                           });
                           player.LetsThink(10);
                       },
                       opponent =>
                       {
                           opponent.CardAttack(opponentPolluterId, playerTrunkId);
                           opponent.LetsThink(10);
                           opponent.AssertInQueue(() => {
                                Assert.AreEqual(3, TestHelper.GetOpponentPlayer().CurrentGoo);
                           });
                           opponent.LetsThink(10);
                       },
                       player => {},
                       opponent => {}
                };

                Action validateEndState = () =>
                {
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Ztink()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 4,
                    new TestCardData("Ztink", 2),
                    new TestCardData("Trunk", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 4,
                    new TestCardData("Ztink", 2),
                    new TestCardData("Trunk", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerTrunkId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Trunk", 1);
                InstanceId playerZtinkId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Ztink", 1);
                InstanceId opponentTrunkId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Trunk", 1);
                InstanceId opponentZtinkId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Ztink", 1);
                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                   {
                       player => {},
                       opponent => {},
                       player =>
                       {
                           player.CardPlay(playerTrunkId, ItemPosition.Start);
                           player.CardPlay(playerZtinkId, ItemPosition.Start);
                           player.CardAbilityUsed(playerZtinkId, Enumerators.AbilityType.DESTROY_TARGET_UNIT_AFTER_ATTACK, new List<ParametrizedAbilityInstanceId>());

                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentTrunkId, ItemPosition.Start);
                           opponent.CardPlay(opponentZtinkId, ItemPosition.Start);
                           opponent.CardAbilityUsed(opponentZtinkId, Enumerators.AbilityType.DESTROY_TARGET_UNIT_AFTER_ATTACK, new List<ParametrizedAbilityInstanceId>());
                       },
                       player => player.CardAttack(playerZtinkId, opponentTrunkId),
                       opponent => opponent.CardAttack(opponentZtinkId, playerTrunkId),
                       player => {},
                       opponent => {},
                   };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(1, TestHelper.GameplayManager.CurrentPlayer.CardsOnBoard.Count);
                    Assert.AreEqual(1, TestHelper.GameplayManager.OpponentPlayer.CardsOnBoard.Count);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        [Category("PlayQuickSubset2")]
        public IEnumerator Goozilla()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0,
                    new TestCardData("Goozilla", 1),
                    new TestCardData("Hot", 20)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    new TestCardData("Goozilla", 1),
                    new TestCardData("Hot", 20)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Goozilla", 1);
                InstanceId playerHotId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Hot", 1);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Goozilla", 1);
                InstanceId opponentHotId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Hot", 1);

                int value = 6;

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                    player => { },
                    opponent => { },
                    player =>
                    {
                        player.CardPlay(playerHotId, ItemPosition.Start);
                        player.CardPlay(playerCardId, ItemPosition.Start, playerHotId);
                    },
                    opponent =>
                    {
                        opponent.CardPlay(opponentHotId, ItemPosition.Start);
                        opponent.CardPlay(opponentCardId, ItemPosition.Start, opponentHotId);
                    },
                };

                Action validateEndState = () =>
                {
                    CardModel playerUnit = (CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerHotId);
                    CardModel opponentUnit = (CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentHotId);
                    Assert.AreEqual(value, playerUnit.BuffedDamage);
                    Assert.AreEqual(-value, playerUnit.BuffedDefense);
                    Assert.AreEqual(value, opponentUnit.BuffedDamage);
                    Assert.AreEqual(-value, opponentUnit.BuffedDefense);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            }, 400);
        }


        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Chernobill()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0,
                    new TestCardData("Cherno-bill", 1),
                    new TestCardData("Trunk", 20)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    new TestCardData("Trunk", 20)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerChernoBillId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Cherno-bill", 1);
                InstanceId playerTrunk1Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Trunk", 1);
                InstanceId playerTrunk2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Trunk", 2);

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
                       player =>
                       {
                           player.CardPlay(playerChernoBillId, ItemPosition.Start);
                       },
                       opponent => {},
                       player => {}
                   };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(1, pvpTestContext.GetCurrentPlayer().CardsOnBoard.Count);
                    Assert.AreEqual(0, pvpTestContext.GetOpponentPlayer().CardsOnBoard.Count);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Zlopper()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0,
                    new TestCardData("Zlopper", 1),
                    new TestCardData("Trunk", 20)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    new TestCardData("Zlopper", 1),
                    new TestCardData("Trunk", 20)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerZlopperId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zlopper", 1);
                InstanceId playerTrunkId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Trunk", 1);
                InstanceId opponentZlopperId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zlopper", 1);
                InstanceId opponentTrunkId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Trunk", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                   {
                       player => {},
                       opponent => {},
                       player =>
                       {
                           player.CardPlay(playerTrunkId, ItemPosition.Start);
                           player.CardPlay(playerZlopperId, ItemPosition.Start);
                           player.CardAbilityUsed(playerZlopperId, Enumerators.AbilityType.DESTROY_TARGET_UNIT_AFTER_ATTACK, new List<ParametrizedAbilityInstanceId>());
                       
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentTrunkId, ItemPosition.Start);
                           opponent.CardPlay(opponentZlopperId, ItemPosition.Start);
                           opponent.CardAbilityUsed(opponentZlopperId, Enumerators.AbilityType.DESTROY_TARGET_UNIT_AFTER_ATTACK, new List<ParametrizedAbilityInstanceId>());
                       
                       },
                       player =>
                       {
                           player.CardAttack(playerZlopperId, opponentTrunkId);
                       },
                       opponent => 
                       {
                           opponent.CardAttack(opponentZlopperId, playerTrunkId);
                       },
                       player => {},
                       opponent => {}
                   };

                Action validateEndState = () =>
                {
                   Assert.AreEqual(1, pvpTestContext.GetCurrentPlayer().CardsOnBoard.Count);
                   Assert.AreEqual(1, pvpTestContext.GetOpponentPlayer().CardsOnBoard.Count);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Zeeter()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0,
                    new TestCardData("Zeeter", 1),
                    new TestCardData("Hot", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    new TestCardData("Zeeter", 1),
                    new TestCardData("Hot", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerZeeterId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zeeter", 1);
                InstanceId playerHotId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Hot", 1);
                InstanceId playerHot2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Hot", 2);
                InstanceId playerHot3Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Hot", 3);
                InstanceId opponentZeeterId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zeeter", 1);
                InstanceId opponentHotId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Hot", 1);
                InstanceId opponentHot2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Hot", 2);
                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                   {
                       player => {},
                       opponent => {},
                       player =>
                       {
                           player.CardPlay(playerHotId, ItemPosition.Start);
                           player.CardPlay(playerHot2Id, ItemPosition.Start);
                           player.CardPlay(playerHot3Id, ItemPosition.Start);
                           player.CardPlay(playerZeeterId, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentHotId, ItemPosition.Start);
                           opponent.CardPlay(opponentHot2Id, ItemPosition.Start);
                           opponent.CardPlay(opponentZeeterId, ItemPosition.Start);
                       },
                   };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(1, TestHelper.GetCurrentPlayer().CardsOnBoard.Count);
                    Assert.AreEqual(1, TestHelper.GetOpponentPlayer().CardsOnBoard.Count);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Zteroid()
        {
           return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 3, 
                    new TestCardData("Zteroid", 1),
                    new TestCardData("Trunk", 20)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 3, 
                    new TestCardData("Zteroid", 1),
                    new TestCardData("Trunk", 20)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerZteroidId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zteroid", 1);
                InstanceId playerTrunkId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Trunk", 1);
                InstanceId opponentZteroidId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zteroid", 1);
                InstanceId opponentTrunkId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Trunk", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                    player => {},
                    opponent => {},
                    player => player.CardPlay(playerTrunkId, ItemPosition.Start),
                    opponent => opponent.CardPlay(opponentTrunkId, ItemPosition.Start),
                    player => player.CardPlay(playerZteroidId, ItemPosition.Start, playerTrunkId),
                    opponent => opponent.CardPlay(opponentZteroidId, ItemPosition.Start, opponentTrunkId),
                    player => {},
                    opponent => {}
                };

                Action validateEndState = () =>
                {
                    int damageIncreaseTo = 10;
                    int defenseDecreasedTo = 6;
                    CardModel playerTrunkModel = ((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerTrunkId));
                    Assert.AreEqual(damageIncreaseTo, playerTrunkModel.CurrentDamage);
                    Assert.AreEqual(defenseDecreasedTo, playerTrunkModel.CurrentDefense);
                    CardModel opponentTrunkModel = ((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentTrunkId));
                    Assert.AreEqual(damageIncreaseTo, opponentTrunkModel.CurrentDamage);
                    Assert.AreEqual(defenseDecreasedTo, opponentTrunkModel.CurrentDefense);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            }, 500);
        }


        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Boomer()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0,
                    new TestCardData("Boomer", 1),
                    new TestCardData("MonZoon", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    new TestCardData("Boomer", 1),
                    new TestCardData("MonZoon", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerBoomerId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Boomer", 1);
                InstanceId opponentBoomerId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Boomer", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                    player => {},
                    opponent => {},
                    player =>
                    {
                        player.CardPlay(playerBoomerId, ItemPosition.Start);
                        player.AssertInQueue(() => {
                            Assert.AreEqual(3, pvpTestContext.GetCurrentPlayer().GooVials);
                        });
                    },
                    opponent =>
                    {
                        opponent.CardPlay(opponentBoomerId, ItemPosition.Start);
                        opponent.LetsThink(2);
                        opponent.AssertInQueue(() => {
                            Assert.AreEqual(3, pvpTestContext.GetOpponentPlayer().GooVials);
                        });
                    },
                    player => {},
                    opponent => {},
                };

                Action validateEndState = () =>
                {
                    int damagesDealt = 4;
                    Assert.AreEqual(pvpTestContext.GetCurrentPlayer().InitialDefense-damagesDealt, pvpTestContext.GetCurrentPlayer().Defense);
                    Assert.AreEqual(pvpTestContext.GetOpponentPlayer().InitialDefense-damagesDealt, pvpTestContext.GetOpponentPlayer().Defense);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Zludge()
        {
            return AsyncTest(async () =>
            {
            Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0,
                new TestCardData("Zludge", 1),
                new TestCardData("Grower", 10)
            );
            Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                new TestCardData("Zludge", 1),
                new TestCardData("Grower", 10)
            );

            PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

            InstanceId playerZludgeId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zludge", 1);
            InstanceId playerGrowerId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Grower", 1);
            InstanceId playerGrower2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Grower", 2);
            InstanceId playerGrower3Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Grower", 3);
            InstanceId opponentZludgeId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zludge", 1);
            InstanceId opponentGrowerId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Grower", 1);
            InstanceId opponentGrower2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Grower", 2);
            InstanceId opponentGrower3Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Grower", 3);

            int playerZludgeDamage = 0;
            int opponnentZludgeDamage = 0;

            IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
            {
                    player => {},
                    opponent => {},
                    player =>
                    {
                        player.CardPlay(playerGrowerId, ItemPosition.Start);
                        player.CardPlay(playerGrower2Id, ItemPosition.Start);
                        player.CardPlay(playerGrower3Id, ItemPosition.Start);
                        player.CardPlay(playerZludgeId, ItemPosition.Start);
                        player.CardAbilityUsed(playerZludgeId, Enumerators.AbilityType.MASSIVE_DAMAGE, new List<ParametrizedAbilityInstanceId>());
                    },
                    opponent =>
                    {
                        opponent.CardPlay(opponentGrowerId, ItemPosition.Start);
                        opponent.CardPlay(opponentGrower2Id, ItemPosition.Start);
                        opponent.CardPlay(opponentGrower3Id, ItemPosition.Start);
                        opponent.CardPlay(opponentZludgeId, ItemPosition.Start);
                        opponent.CardAbilityUsed(opponentZludgeId, Enumerators.AbilityType.MASSIVE_DAMAGE, new List<ParametrizedAbilityInstanceId>());
                    },
                    player =>
                    {
                        playerZludgeDamage = ((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerZludgeId)).CurrentDamage;
                        opponnentZludgeDamage = ((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentZludgeId)).CurrentDamage;

                        player.CardAttack(playerZludgeId, opponentZludgeId);
                    },
                    opponent => opponent.CardAttack(opponentZludgeId, playerZludgeId),
                    player => {},
                    opponent => {},
            };

                Action validateEndState = () =>
                { 
                    Assert.AreEqual(0, TestHelper.GameplayManager.CurrentPlayer.CardsOnBoard.Count);
                    Assert.AreEqual(0, TestHelper.GameplayManager.OpponentPlayer.CardsOnBoard.Count);
                    Assert.AreEqual(TestHelper.GameplayManager.CurrentPlayer.InitialDefense - (playerZludgeDamage + opponnentZludgeDamage),
                        TestHelper.GameplayManager.CurrentPlayer.Defense);
                    Assert.AreEqual(TestHelper.GameplayManager.OpponentPlayer.InitialDefense - (playerZludgeDamage + opponnentZludgeDamage),
                        TestHelper.GameplayManager.OpponentPlayer.Defense);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Gloop()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 4,
                    new TestCardData("Gloop", 1),
                    new TestCardData("Trunk", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 4,
                    new TestCardData("Gloop", 1),
                    new TestCardData("Trunk", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerTrunkId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Trunk", 1);
                InstanceId playerGloopId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Gloop", 1);
                InstanceId opponentTrunkId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Trunk", 1);
                InstanceId opponentGloopId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Gloop", 1);
                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                   {
                       player => {},
                       opponent => {},
                       player =>
                       {
                           player.CardPlay(playerTrunkId, ItemPosition.Start);
                           player.CardPlay(playerGloopId, ItemPosition.Start);
                           player.CardAbilityUsed(playerGloopId, Enumerators.AbilityType.DESTROY_TARGET_UNIT_AFTER_ATTACK, new List<ParametrizedAbilityInstanceId>());

                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentTrunkId, ItemPosition.Start);
                           opponent.CardPlay(opponentGloopId, ItemPosition.Start);
                           opponent.CardAbilityUsed(opponentGloopId, Enumerators.AbilityType.DESTROY_TARGET_UNIT_AFTER_ATTACK, new List<ParametrizedAbilityInstanceId>());
                       },
                       player => player.CardAttack(playerGloopId, opponentTrunkId),
                       opponent => opponent.CardAttack(opponentGloopId, playerTrunkId),
                       player => {},
                       opponent => {},
                   };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(0, TestHelper.GameplayManager.CurrentPlayer.CardsOnBoard.Count);
                    Assert.AreEqual(0, TestHelper.GameplayManager.OpponentPlayer.CardsOnBoard.Count);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }
    }
}
