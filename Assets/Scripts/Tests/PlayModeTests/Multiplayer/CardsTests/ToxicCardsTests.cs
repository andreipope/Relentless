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
                    new DeckCardData("RelentleZZ", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    new DeckCardData("RelentleZZ", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "RelentleZZ", 1);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "RelentleZZ", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                       player => {},
                       opponent => {},
                       player => {},
                       opponent => {},
                       player => player.CardPlay(playerCardId, ItemPosition.Start),
                       opponent => opponent.CardPlay(opponentCardId, ItemPosition.Start),
                };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(2, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCardId)).CurrentDefense);
                    Assert.AreEqual(2, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCardId)).CurrentDefense);
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
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0,
                    new DeckCardData("Zpitter", 1),
                    new DeckCardData("Burrrnn", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    new DeckCardData("Zpitter", 1),
                    new DeckCardData("Burrrnn", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerBurnId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Burrrnn", 1);
                InstanceId playerBurn2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Burrrnn", 2);
                InstanceId playerBurn3Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Burrrnn", 3);
                InstanceId playerZpitterId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zpitter", 1);

                InstanceId opponentBurnId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Burrrnn", 1);
                InstanceId opponentBurn2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Burrrnn", 2);
                InstanceId opponentBurn3Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Burrrnn", 3);
                InstanceId opponentZpitterId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zpitter", 1);
                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                   {
                       player => {},
                       opponent => {},
                       player => {},
                       opponent => {},
                       player =>
                       {
                           player.CardPlay(playerBurnId, ItemPosition.Start);
                           player.CardPlay(playerBurn2Id, ItemPosition.Start);
                           player.CardPlay(playerBurn3Id, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentBurnId, ItemPosition.Start);
                           opponent.CardPlay(opponentBurn2Id, ItemPosition.Start);
                           opponent.CardPlay(opponentBurn3Id, ItemPosition.Start);
                       },
                       player => {
                           player.CardPlay(playerZpitterId, ItemPosition.Start);
                       },  
                       opponent => {
                           opponent.CardPlay(opponentZpitterId, ItemPosition.Start);
                       },
                       player => {}
                   };

                Action validateEndState = () =>
                {
                    Assert.IsTrue(TestHelper.GetCurrentPlayer().CardsOnBoard.FindAll(card => card.CurrentDefense == card.Card.Prototype.Defense - 1).Count > 0);
                    Assert.IsTrue(TestHelper.GetOpponentPlayer().CardsOnBoard.FindAll(card => card.CurrentDefense == card.Card.Prototype.Defense - 1).Count > 0);
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
                    new DeckCardData("Ectoplazm", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 4,
                    new DeckCardData("Ectoplazm", 10)
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
                    new DeckCardData("Ghoul", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    new DeckCardData("Ghoul", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Ghoul", 1);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Ghoul", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                   {
                       player => player.CardPlay(playerCardId, ItemPosition.Start),
                       opponent => opponent.CardPlay(opponentCardId, ItemPosition.Start),
                       player =>
                       {
                           player.CardAttack(playerCardId, pvpTestContext.GetOpponentPlayer().InstanceId);
                       },
                       opponent =>
                       {
                           opponent.CardAttack(opponentCardId, pvpTestContext.GetCurrentPlayer().InstanceId);
                       }
                   };

                int value = 8;

                Action validateEndState = () =>
                {
                    Assert.AreEqual(pvpTestContext.GetCurrentPlayer().InitialDefense - value, pvpTestContext.GetCurrentPlayer().Defense);
                    Assert.AreEqual(pvpTestContext.GetOpponentPlayer().InitialDefense - value, pvpTestContext.GetOpponentPlayer().Defense);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, true,
                    true, true);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Wazte()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 4,
                    new DeckCardData("Wazte", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 4,
                    new DeckCardData("Wazte", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Wazte", 1);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Wazte", 1);
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
                    Assert.AreEqual(true, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCardId)).IsHeavyUnit);
                    Assert.AreEqual(true, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCardId)).IsHeavyUnit);
                    Assert.AreEqual(2, pvpTestContext.GetOpponentPlayer().GooVials);
                    Assert.AreEqual(2, pvpTestContext.GetCurrentPlayer().GooVials);
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
                    new DeckCardData("Azzazzin", 2),
                    new DeckCardData("Cerberuz", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    new DeckCardData("Azzazzin", 2),
                    new DeckCardData("Cerberuz", 10)
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
                    new DeckCardData("Hazzard", 2),
                    new DeckCardData("Mountain", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 4,
                    new DeckCardData("Hazzard", 2),
                    new DeckCardData("Mountain", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerMountainId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Mountain", 1);
                InstanceId playerHazzardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Hazzard", 1);
                InstanceId opponentMountainId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Mountain", 1);
                InstanceId opponentHazzardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Hazzard", 1);
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
                           player.CardPlay(playerHazzardId, ItemPosition.Start);
                           player.LetsThink(2);
                           player.AssertInQueue(() => {
                               Assert.AreEqual(3, pvpTestContext.GetCurrentPlayer().GooVials);
                           });
                           player.CardPlay(playerMountainId, ItemPosition.Start);
                           player.CardAbilityUsed(playerHazzardId, Enumerators.AbilityType.DESTROY_TARGET_UNIT_AFTER_ATTACK, new List<ParametrizedAbilityInstanceId>());

                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentHazzardId, ItemPosition.Start);
                           opponent.LetsThink(2);
                           opponent.AssertInQueue(() => {
                               Assert.AreEqual(3, pvpTestContext.GetOpponentPlayer().GooVials);
                           });
                           opponent.CardPlay(opponentMountainId, ItemPosition.Start);
                           opponent.CardAbilityUsed(opponentHazzardId, Enumerators.AbilityType.DESTROY_TARGET_UNIT_AFTER_ATTACK, new List<ParametrizedAbilityInstanceId>());
                       },
                       player => player.CardAttack(playerHazzardId, opponentMountainId),
                       opponent => opponent.CardAttack(opponentHazzardId, playerMountainId),
                       player => {},
                       opponent => {},
                   };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(1, TestHelper.GameplayManager.CurrentPlayer.CardsOnBoard.Count);
                    Assert.AreEqual(1, TestHelper.GameplayManager.OpponentPlayer.CardsOnBoard.Count);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Zlimey()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0,
                    new DeckCardData("Zlimey", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    new DeckCardData("Zlimey", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zlimey", 1);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zlimey", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                   {
                       player => player.CardPlay(playerCardId, ItemPosition.Start),
                       opponent => opponent.CardPlay(opponentCardId, ItemPosition.Start),
                   };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(pvpTestContext.GetCurrentPlayer().InitialDefense - 2, pvpTestContext.GetCurrentPlayer().Defense);
                    Assert.AreEqual(pvpTestContext.GetOpponentPlayer().InitialDefense - 2, pvpTestContext.GetOpponentPlayer().Defense);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Kabomb_Just_Local()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0,
                    new DeckCardData("Kabomb", 1),
                    new DeckCardData("Slab", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    new DeckCardData("Kabomb", 1),
                    new DeckCardData("Slab", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerKabombId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Kabomb", 1);
                InstanceId playerSlabId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Slab", 1);
                InstanceId opponentKabombId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Kabomb", 1);
                InstanceId opponentSlabId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Slab", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                       player =>
                       {
                           player.CardPlay(playerKabombId, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentSlabId, ItemPosition.Start);
                       },
                       player =>
                       {
                           player.CardAttack(playerKabombId, opponentSlabId);
                       },
                       opponent => {},
                       player => {}
                };

                Action validateEndState = () =>
                {
                    Assert.IsTrue(TestHelper.GetOpponentPlayer().Defense == TestHelper.GetOpponentPlayer().InitialDefense - 5
                    || (TestHelper.GetOpponentPlayer().CardsOnBoard.FindAll(card => card.Card.Prototype.MouldId == 101)).ToList().Count == 0);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            }, 600);
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        [Category("PlayQuickSubset2")]
        public IEnumerator Hazmat()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0,
                    new DeckCardData("Hazmat", 1),
                    new DeckCardData("Zlab", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    new DeckCardData("Hazmat", 1),
                    new DeckCardData("Zlab", 10)
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
                           opponent.CardPlay(opponentCardId, ItemPosition.Start);
                           opponent.CardAbilityUsed(opponentCardId, Enumerators.AbilityType.GIVE_BUFFS_TO_UNIT, new List<ParametrizedAbilityInstanceId>(){
                               new ParametrizedAbilityInstanceId(opponentZlab2Id),
                           });
                       }
                };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(true, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerZlabId))
                        .BuffsOnUnit.Contains(Enumerators.BuffType.DESTROY));
                    Assert.AreEqual(true, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentZlab2Id))
                        .BuffsOnUnit.Contains(Enumerators.BuffType.DESTROY));
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Zeptic()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0,
                    new DeckCardData("Zeptic", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    new DeckCardData("Zeptic", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zeptic", 1);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zeptic", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                   {
                       player => {},
                       opponent => {},
                       player => {},
                       opponent => {},
                       player => player.CardPlay(playerCardId, ItemPosition.Start),
                       opponent => opponent.CardPlay(opponentCardId, ItemPosition.Start)
                   };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(pvpTestContext.GetCurrentPlayer().InitialDefense - 2, pvpTestContext.GetCurrentPlayer().Defense);
                    Assert.AreEqual(pvpTestContext.GetOpponentPlayer().InitialDefense - 2, pvpTestContext.GetOpponentPlayer().Defense);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, true,
                    true, true);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Germ()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 4,
                    new DeckCardData("Germ", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 4,
                    new DeckCardData("Germ", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Germ", 1);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Germ", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                   {
                       player => {},
                       opponent => {},
                       player => {},
                       opponent => {},
                       player => player.CardPlay(playerCardId, ItemPosition.Start),
                       opponent => opponent.CardPlay(opponentCardId, ItemPosition.Start)
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
        public IEnumerator Zcavenger()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0,
                    new DeckCardData("Zcavenger", 20),
                    new DeckCardData("Boomstick", 1)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    new DeckCardData("Zcavenger", 20),
                    new DeckCardData("Boomstick", 1)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zcavenger", 1);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zcavenger", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                   {
                       player => {},
                       opponent => {},
                       player => {},
                       opponent => {},
                       player => player.CardPlay(playerCardId, ItemPosition.Start),
                       opponent => opponent.CardPlay(opponentCardId, ItemPosition.Start)
                   };

                Action validateEndState = () =>
                {
                    Assert.IsTrue(TestHelper.GetCurrentPlayer().CardsInHand.FindAll(card => card.Card.Prototype.Faction == Enumerators.Faction.ITEM).Count > 0);
                    Assert.IsTrue(TestHelper.GetOpponentPlayer().CardsInHand.FindAll(card => card.Card.Prototype.Faction == Enumerators.Faction.ITEM).Count > 0);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Scab()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0,
                    new DeckCardData("Scab", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    new DeckCardData("Scab", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Scab", 1);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Scab", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                       player => {},
                       opponent => {},
                       player => {},
                       opponent => {},
                       player => player.CardPlay(playerCardId, ItemPosition.Start),
                       opponent => opponent.CardPlay(opponentCardId, ItemPosition.Start),
                       player => {},
                       opponent => {}
                };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(true, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCardId)).HasFeral);
                    Assert.AreEqual(true, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCardId)).HasFeral);
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
                    new DeckCardData("Zkewer", 1),
                    new DeckCardData("Pyrite", 5)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    new DeckCardData("Zkewer", 1),
                    new DeckCardData("Pyrite", 5)
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

                    foreach(BoardUnitModel card in TestHelper.GameplayManager.CurrentPlayer.PlayerCardsController.CardsInHand)
                    {
                        Assert.AreEqual(card.Prototype.Cost + 2, card.InstanceCard.Cost);
                    }

                    foreach (BoardUnitModel card in TestHelper.GameplayManager.OpponentPlayer.PlayerCardsController.CardsInHand)
                    {
                        Assert.AreEqual(card.Prototype.Cost + 2, card.InstanceCard.Cost);
                    }
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Bane()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0,
                    new DeckCardData("Bane", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    new DeckCardData("Bane", 10)
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
                    new DeckCardData("Polluter", 1),
                    new DeckCardData("Spiker", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    new DeckCardData("Polluter", 1),
                    new DeckCardData("Spiker", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerPolluterId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Polluter", 1);
                InstanceId playerSpikerbId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Spiker", 1);
                InstanceId opponentPolluterId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Polluter", 1);
                InstanceId opponentSpikerId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Spiker", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                       player =>
                       {
                           player.CardPlay(playerPolluterId, ItemPosition.Start);
                           player.CardAbilityUsed(playerPolluterId, Enumerators.AbilityType.ADD_GOO_VIAL, new List<ParametrizedAbilityInstanceId>());
                           player.CardPlay(playerSpikerbId, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentPolluterId, ItemPosition.Start);
                           opponent.CardAbilityUsed(opponentPolluterId, Enumerators.AbilityType.ADD_GOO_VIAL, new List<ParametrizedAbilityInstanceId>());
                           opponent.CardPlay(opponentSpikerId, ItemPosition.Start);
                       },
                       player =>
                       {
                           player.CardAttack(playerPolluterId, opponentSpikerId);
                           player.LetsThink(10);
                           player.AssertInQueue(() => {
                                Assert.AreEqual(3, TestHelper.GetCurrentPlayer().GooVials);
                           });
                       },
                       opponent =>
                       {
                           opponent.CardAttack(opponentPolluterId, playerSpikerbId);
                           opponent.LetsThink(10);
                           opponent.AssertInQueue(() => {
                                Assert.AreEqual(3, TestHelper.GetOpponentPlayer().GooVials);
                           });
                       },
                       player => {},
                       opponent => {},
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
                    new DeckCardData("Ztink", 2),
                    new DeckCardData("Trunk", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 4,
                    new DeckCardData("Ztink", 2),
                    new DeckCardData("Trunk", 10)
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
                    new DeckCardData("Goozilla", 1),
                    new DeckCardData("Hot", 20)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    new DeckCardData("Goozilla", 1),
                    new DeckCardData("Hot", 20)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Goozilla", 1);
                InstanceId playerHotId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Hot", 1);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Goozilla", 1);
                InstanceId opponentHotId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Hot", 1);

                int value = 6;

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                    player =>
                    {
                        player.CardPlay(playerHotId, ItemPosition.Start);
                        player.CardPlay(playerCardId, ItemPosition.Start, playerHotId);
                    },
                    opponent =>
                    {
                        opponent.CardPlay(opponentHotId, ItemPosition.Start);
                        opponent.CardPlay(opponentCardId, ItemPosition.Start);
                        opponent.CardAbilityUsed(opponentCardId, Enumerators.AbilityType.CHANGE_STAT, new List<ParametrizedAbilityInstanceId>()
                        {
                            new ParametrizedAbilityInstanceId(opponentHotId),
                        });
                    },
                };

                Action validateEndState = () =>
                {
                    BoardUnitModel playerUnit = (BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerHotId);
                    BoardUnitModel opponentUnit = (BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentHotId);
                    Assert.AreEqual(value, playerUnit.BuffedDamage);
                    Assert.AreEqual(-value, playerUnit.BuffedDefense);
                    Assert.AreEqual(value, opponentUnit.BuffedDamage);
                    Assert.AreEqual(-value, opponentUnit.BuffedDefense);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            }, 400);
        }


        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Chernobill()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0,
                    new DeckCardData("Cherno-bill", 1),
                    new DeckCardData("MonZoon", 2),
                    new DeckCardData("Slab", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    new DeckCardData("Cherno-bill", 1),
                    new DeckCardData("MonZoon", 2),
                    new DeckCardData("Slab", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerChernobillId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Cherno-bill", 1);
                InstanceId playerMonZoonId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "MonZoon", 1);
                InstanceId playerMonZoon2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "MonZoon", 2);
                InstanceId playerSlabId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Slab", 1);
                InstanceId playerSlab2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Slab", 2);
                InstanceId opponentChernobillId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Cherno-bill", 1);
                InstanceId opponentMonZoonId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "MonZoon", 1);
                InstanceId opponentMonZoon2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "MonZoon", 2);
                InstanceId opponentSlabId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Slab", 1);
                InstanceId opponentSlab2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Slab", 2);
                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                   {
                       player => {},
                       opponent =>
                       {
                           opponent.CardPlay(opponentChernobillId, ItemPosition.Start);
                           opponent.CardAbilityUsed(opponentChernobillId, Enumerators.AbilityType.MASSIVE_DAMAGE, new List<ParametrizedAbilityInstanceId>());
                       },
                       player =>
                       {
                           player.CardPlay(playerMonZoonId, ItemPosition.Start);
                           player.CardPlay(playerMonZoon2Id, ItemPosition.Start);
                           player.CardPlay(playerSlabId, ItemPosition.Start);
                           

                       },
                       opponent =>
                       {
                            opponent.CardPlay(opponentSlabId, ItemPosition.Start);
                       },
                       player =>
                       {
                           player.CardAttack(playerMonZoonId, opponentChernobillId);
                           player.CardAttack(playerMonZoon2Id, opponentChernobillId);
                           player.LetsThink(10);
                           player.AssertInQueue(() => {
                               Assert.AreEqual(1, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerSlabId)).CurrentDefense);
                               Assert.AreEqual(1, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentSlabId)).CurrentDefense);
                           });
                       },
                       opponent => {
                           opponent.CardPlay(opponentMonZoonId, ItemPosition.Start);
                           opponent.CardPlay(opponentMonZoon2Id, ItemPosition.Start);
                           opponent.CardPlay(opponentSlab2Id, ItemPosition.Start);
                       },
                       player =>
                       {
                           player.CardPlay(playerSlab2Id, ItemPosition.Start);
                           player.CardPlay(playerChernobillId, ItemPosition.Start);
                           player.CardAbilityUsed(playerChernobillId, Enumerators.AbilityType.MASSIVE_DAMAGE, new List<ParametrizedAbilityInstanceId>());
                           player.LetsThink(10);
                       },
                       opponent =>
                       {
                           opponent.CardAttack(opponentMonZoonId, playerChernobillId);
                           opponent.CardAttack(opponentMonZoon2Id, playerChernobillId);
                           opponent.LetsThink(14);
                           opponent.AssertInQueue(() => {
                               Assert.AreEqual(1, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerSlab2Id)).CurrentDefense);
                               Assert.AreEqual(1, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentSlab2Id)).CurrentDefense);
                           });
                       },
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
        public IEnumerator Zlopper()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0,
                    new DeckCardData("Zlopper", 2),
                    new DeckCardData("Boomer", 2),
                    new DeckCardData("MonZoon", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    new DeckCardData("Zlopper", 2),
                    new DeckCardData("Boomer", 2),
                    new DeckCardData("MonZoon", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerZlopperId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zlopper", 1);
                InstanceId playerBoomerId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Boomer", 1);
                InstanceId playerBoomer2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Boomer", 2);
                InstanceId playerMonZoonId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "MonZoon", 1);
                InstanceId opponentZlopperId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zlopper", 1);
                InstanceId opponentBoomerId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Boomer", 1);
                InstanceId opponentBoomer2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Boomer", 2);
                InstanceId opponentMonZoonId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "MonZoon", 1);
                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                   {
                       player => {},
                       opponent => {},
                       player =>
                       {
                           player.CardPlay(playerBoomerId, ItemPosition.Start);
                           player.CardPlay(playerBoomer2Id, ItemPosition.Start);
                           player.CardPlay(playerMonZoonId, ItemPosition.Start);
                           player.CardPlay(playerZlopperId, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentBoomerId, ItemPosition.Start);
                           opponent.CardPlay(opponentBoomer2Id, ItemPosition.Start);
                           opponent.CardPlay(opponentMonZoonId, ItemPosition.Start);
                           opponent.CardPlay(opponentZlopperId, ItemPosition.Start);
                       },
                       player => {},
                       opponent => {},
                   };

                Action validateEndState = () =>
                {
                    BoardUnitModel playerBoomerUnit = (BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerBoomerId);
                    BoardUnitModel playerBoomer2Unit = (BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerBoomer2Id);
                    BoardUnitModel playerMonZoonUnit = (BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerMonZoonId);
                    BoardUnitModel opponentBoomerUnit = (BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentBoomerId);
                    BoardUnitModel opponentBoomer2Unit = (BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentBoomer2Id);
                    BoardUnitModel opponentMonZoonUnit = (BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentMonZoonId);
                    Assert.AreEqual(playerBoomerUnit.Card.Prototype.Damage + 1, playerBoomerUnit.CurrentDamage);
                    Assert.AreEqual(playerBoomer2Unit.Card.Prototype.Damage + 1, playerBoomer2Unit.CurrentDamage);
                    Assert.AreEqual(playerMonZoonUnit.Card.Prototype.Damage, playerMonZoonUnit.CurrentDamage);
                    Assert.AreEqual(opponentBoomerUnit.Card.Prototype.Damage + 1, opponentBoomerUnit.CurrentDamage);
                    Assert.AreEqual(opponentBoomer2Unit.Card.Prototype.Damage + 1, opponentBoomer2Unit.CurrentDamage);
                    Assert.AreEqual(playerMonZoonUnit.Card.Prototype.Damage, playerMonZoonUnit.CurrentDamage);
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
                    new DeckCardData("Zeeter", 1),
                    new DeckCardData("Boomer", 2),
                    new DeckCardData("MonZoon", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    new DeckCardData("Zeeter", 1),
                    new DeckCardData("Boomer", 2),
                    new DeckCardData("MonZoon", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerZeeterId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zeeter", 1);
                InstanceId playerBoomerId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Boomer", 1);
                InstanceId playerMonZoonId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "MonZoon", 1);
                InstanceId opponentZeeterId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zeeter", 1);
                InstanceId opponentBoomerId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Boomer", 1);
                InstanceId opponentMonZoonId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "MonZoon", 1);
                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                   {
                       player => {},
                       opponent => {},
                       player =>
                       {
                           player.CardPlay(playerBoomerId, ItemPosition.Start);
                           player.CardPlay(playerMonZoonId, ItemPosition.Start);
                           player.LetsThink(2);
                           player.CardPlay(playerZeeterId, ItemPosition.Start, playerBoomerId);
                           player.LetsThink(5);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentBoomerId, ItemPosition.Start);
                           opponent.CardPlay(opponentMonZoonId, ItemPosition.Start);
                           opponent.LetsThink(2);
                           opponent.CardPlay(opponentZeeterId, ItemPosition.Start, opponentBoomerId);
                           opponent.LetsThink(5);
                       },
                   };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(7, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerZeeterId)).CurrentDefense);
                    Assert.AreEqual(7, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerZeeterId)).CurrentDamage);
                    Assert.AreEqual(7, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentZeeterId)).CurrentDefense);
                    Assert.AreEqual(7, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentZeeterId)).CurrentDamage);
                    Assert.AreEqual(2, TestHelper.GetCurrentPlayer().CardsOnBoard.Count);
                    Assert.AreEqual(2, TestHelper.GetOpponentPlayer().CardsOnBoard.Count);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }


        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Kabomb()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0,
                    new DeckCardData("Kabomb", 1),
                    new DeckCardData("BurZt", 1),
                    new DeckCardData("Cerberus", 1),
                    new DeckCardData("MonZoon", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    new DeckCardData("Kabomb", 1),
                    new DeckCardData("BurZt", 1),
                    new DeckCardData("Cerberus", 1),
                    new DeckCardData("MonZoon", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerKabombId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Kabomb", 1);
                InstanceId playerBurZtId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "BurZt", 1);
                InstanceId playerCerberusId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Cerberus", 1);
                InstanceId playerMonZoonId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "MonZoon", 1);
                InstanceId opponentKabombId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Kabomb", 1);
                InstanceId opponentBurZtId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "BurZt", 1);
                InstanceId opponentCerberusId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Cerberus", 1);
                InstanceId opponentMonZoonId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "MonZoon", 1);
                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                   {
                       player => {},
                       opponent => {},
                       player =>
                       {
                           player.CardPlay(playerCerberusId, ItemPosition.Start);
                           player.CardPlay(playerMonZoonId, ItemPosition.Start);
                           player.CardPlay(playerBurZtId, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentCerberusId, ItemPosition.Start);
                           opponent.CardPlay(opponentMonZoonId, ItemPosition.Start);
                           opponent.CardPlay(opponentBurZtId, ItemPosition.Start);
                       },
                       player => {
                           player.CardPlay(playerKabombId, ItemPosition.Start);
                           player.CardAbilityUsed(playerKabombId, Enumerators.AbilityType.TAKE_DAMAGE_RANDOM_ENEMY, new List<ParametrizedAbilityInstanceId>());
                       },
                       opponent => {},
                       player => {
                           player.CardAttack(playerKabombId, opponentBurZtId);
                       },
                       opponent => {
                           opponent.CardPlay(opponentKabombId, ItemPosition.Start);
                           opponent.CardAbilityUsed(opponentKabombId, Enumerators.AbilityType.TAKE_DAMAGE_RANDOM_ENEMY, new List<ParametrizedAbilityInstanceId>());
                       },
                       player => {},
                       opponent => {
                           opponent.CardAttack(opponentKabombId, playerBurZtId);
                       },
                       player => {}
                   };

                Action validateEndState = () =>
                {
                    Assert.IsTrue(TestHelper.GetCurrentPlayer().CardsOnBoard.FindAll(card => card.CurrentDefense == card.Card.Prototype.Defense - 5).Count > 0 ||
                        TestHelper.GetCurrentPlayer().InitialDefense - 5 == TestHelper.GetCurrentPlayer().Defense);
                    Assert.IsTrue(TestHelper.GetOpponentPlayer().CardsOnBoard.FindAll(card => card.CurrentDefense == card.Card.Prototype.Defense - 5).Count > 0 ||
                        TestHelper.GetOpponentPlayer().InitialDefense - 5 == TestHelper.GetCurrentPlayer().Defense);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Zteroid()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0,
                    new DeckCardData("Zteroid", 1),
                    new DeckCardData("Cerberus", 1),
                    new DeckCardData("MonZoon", 1),
                    new DeckCardData("Rager", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    new DeckCardData("Zteroid", 1),
                    new DeckCardData("Cerberus", 1),
                    new DeckCardData("MonZoon", 1),
                    new DeckCardData("Rager", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerZteroidId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zteroid", 1);
                InstanceId playerCerberusId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Cerberus", 1);
                InstanceId playerMonZoonId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "MonZoon", 1);
                InstanceId playerRagerId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Rager", 1);
                  InstanceId opponentZteroidId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zteroid", 1);
                InstanceId opponentCerberusId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Cerberus", 1);
                InstanceId opponentMonZoonId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "MonZoon", 1);
                InstanceId opponentRagerId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Rager", 1);
                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                   {
                       player => {},
                       opponent => {},
                       player =>
                       {
                           player.CardPlay(playerCerberusId, ItemPosition.Start);
                           player.CardPlay(playerMonZoonId, ItemPosition.Start);
                           player.CardPlay(playerZteroidId, ItemPosition.Start);
                           player.CardPlay(playerRagerId, ItemPosition.Start);
                           player.CardAbilityUsed(playerZteroidId, Enumerators.AbilityType.CHANGE_STAT_UNTILL_END_OF_TURN, new List<ParametrizedAbilityInstanceId>());
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentCerberusId, ItemPosition.Start);
                           opponent.CardPlay(opponentMonZoonId, ItemPosition.Start);
                           opponent.CardPlay(opponentZteroidId, ItemPosition.Start);
                           opponent.CardPlay(opponentRagerId, ItemPosition.Start);
                           opponent.CardAbilityUsed(opponentZteroidId, Enumerators.AbilityType.CHANGE_STAT_UNTILL_END_OF_TURN, new List<ParametrizedAbilityInstanceId>());
                       },
                       player =>
                       {
                           player.CardAttack(playerRagerId, opponentZteroidId);
                           player.LetsThink(16);
                           player.AssertInQueue(() => {
                                Assert.AreEqual(9, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCerberusId)).CurrentDamage);
                                Assert.AreEqual(8, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentMonZoonId)).CurrentDamage);
                           });
                       },
                       opponent =>
                       {
                           opponent.CardAttack(opponentRagerId, playerZteroidId);
                           opponent.LetsThink(16);
                           opponent.AssertInQueue(() => {
                                Assert.AreEqual(9, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCerberusId)).CurrentDamage);
                                Assert.AreEqual(8, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerMonZoonId)).CurrentDamage);
                           });
                       },
                       player => {},
                       opponent => {},
                };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(7, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCerberusId)).CurrentDamage);
                    Assert.AreEqual(6, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerMonZoonId)).CurrentDamage);
                    Assert.AreEqual(7, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCerberusId)).CurrentDamage);
                    Assert.AreEqual(6, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentMonZoonId)).CurrentDamage);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }


        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Boomer()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0,
                    new DeckCardData("Boomer", 1),
                    new DeckCardData("MonZoon", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    new DeckCardData("Boomer", 1),
                    new DeckCardData("MonZoon", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerBoomerId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Boomer", 1);
                InstanceId playerMonZoonId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "MonZoon", 1);
                InstanceId playerMonZoon2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "MonZoon", 2);
                InstanceId playerMonZoon3Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "MonZoon", 3);
                InstanceId opponentBoomerId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Boomer", 1);
                InstanceId opponentMonZoonId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "MonZoon", 1);
                InstanceId opponentMonZoon2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "MonZoon", 2);
                InstanceId opponentMonZoon3Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "MonZoon", 3);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                    player => {},
                    opponent => {},
                    player =>
                    {
                        player.CardPlay(playerMonZoonId, ItemPosition.Start);
                        player.CardPlay(playerMonZoon2Id, ItemPosition.Start);
                        player.CardPlay(playerBoomerId, ItemPosition.Start);
                        player.CardPlay(playerMonZoon3Id, ItemPosition.Start);
                        player.CardAbilityUsed(playerBoomerId, Enumerators.AbilityType.ADJACENT_UNITS_GET_STAT, new List<ParametrizedAbilityInstanceId>());

                    },
                    opponent =>
                    {
                        opponent.CardPlay(opponentMonZoonId, ItemPosition.Start);
                        opponent.CardPlay(opponentMonZoon2Id, ItemPosition.Start);
                        opponent.CardPlay(opponentBoomerId, ItemPosition.Start);
                        opponent.CardPlay(opponentMonZoon3Id, ItemPosition.Start);
                        opponent.CardAbilityUsed(opponentBoomerId, Enumerators.AbilityType.ADJACENT_UNITS_GET_STAT, new List<ParametrizedAbilityInstanceId>());
                    },
                    player => player.CardAttack(playerMonZoonId, opponentBoomerId),
                    opponent => opponent.CardAttack(opponentMonZoonId, playerBoomerId),
                    player => {},
                    opponent => {},
                };

                Action validateEndState = () =>
                {
                    BoardUnitModel playerMonZoon2Unit = (BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerMonZoon2Id);
                    BoardUnitModel playerMonZoon3Unit = (BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerMonZoon3Id);
                    BoardUnitModel opponentMonZoon2Unit = (BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentMonZoon2Id);
                    BoardUnitModel opponentMonZoon3Unit = (BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentMonZoon3Id);

                    Assert.AreEqual(playerMonZoon2Unit.Card.Prototype.Damage + 2, playerMonZoon2Unit.CurrentDamage);
                    Assert.AreEqual(playerMonZoon3Unit.Card.Prototype.Damage + 2, playerMonZoon3Unit.CurrentDamage);
                    Assert.AreEqual(opponentMonZoon2Unit.Card.Prototype.Damage + 2, opponentMonZoon2Unit.CurrentDamage);
                    Assert.AreEqual(opponentMonZoon3Unit.Card.Prototype.Damage + 2, opponentMonZoon3Unit.CurrentDamage);
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
                    new DeckCardData("Zludge", 1),
                    new DeckCardData("Slab", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    new DeckCardData("Zludge", 1),
                    new DeckCardData("Slab", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerZludgeId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zludge", 1);
                InstanceId playerSlabId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Slab", 1);
                InstanceId opponentZludgeId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zludge", 1);
                InstanceId opponentSlabId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Slab", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                    player => {},
                    opponent => {},
                    player =>
                    {
                        player.CardPlay(playerSlabId, ItemPosition.Start);
                        player.CardAbilityUsed(playerZludgeId, Enumerators.AbilityType.RAGE, new List<ParametrizedAbilityInstanceId>());
                        player.CardPlay(playerZludgeId, ItemPosition.Start);
                    },
                    opponent =>
                    {
                        opponent.CardPlay(opponentSlabId, ItemPosition.Start);
                        opponent.CardAbilityUsed(opponentZludgeId, Enumerators.AbilityType.RAGE, new List<ParametrizedAbilityInstanceId>());
                        opponent.CardPlay(opponentZludgeId, ItemPosition.Start);
                    },
                    player => player.CardAttack(playerSlabId, opponentZludgeId),
                    opponent => opponent.CardAttack(opponentSlabId, playerZludgeId),
                    player => {},
                    opponent => {},
                };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(2, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerZludgeId)).CurrentDefense);
                    Assert.AreEqual(6, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerZludgeId)).CurrentDamage);
                    Assert.AreEqual(2, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentZludgeId)).CurrentDefense);
                    Assert.AreEqual(6, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentZludgeId)).CurrentDamage);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, enableBackendGameLogicMatch: true);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Gloop()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 4,
                    new DeckCardData("Gloop", 1),
                    new DeckCardData("Trunk", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 4,
                    new DeckCardData("Gloop", 1),
                    new DeckCardData("Trunk", 10)
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
