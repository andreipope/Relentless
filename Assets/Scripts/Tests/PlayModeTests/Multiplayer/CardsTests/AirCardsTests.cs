using System;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using Loom.ZombieBattleground.Data;
using UnityEngine.TestTools;
using System.Linq;
using Loom.ZombieBattleground.Common;

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
                    new DeckCardData("Mind Flayer", 2),
                    new DeckCardData("Pyromaz", 2),
                });
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 2, new DeckCardData[]
                {
                    new DeckCardData("Mind Flayer", 2),
                    new DeckCardData("Pyromaz", 2),
                });

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerWhizparId1 = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Pyromaz", 1);
                InstanceId playerWhizparId2 = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Pyromaz", 2);
                InstanceId playerMindFlayerId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Mind Flayer", 1);

                InstanceId opponentWhizparId1 = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Pyromaz", 1);
                InstanceId opponentWhizparId2 = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Pyromaz", 2);
                InstanceId opponentMindFlayerId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Mind Flayer", 1);
                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                   {
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
                       },
                       player =>
                       {
                           player.CardAttack(playerWhizparId1, opponentWhizparId1);
                           player.CardPlay(playerMindFlayerId, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentWhizparId2, ItemPosition.Start);
                       },
                       player => {},
                       opponent =>
                       {
                           opponent.CardAttack(opponentWhizparId2, playerWhizparId2);
                           opponent.CardPlay(opponentMindFlayerId, ItemPosition.Start);
                       },
                       player => {},
                       opponent => {},
                   };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(0, TestHelper.BattlegroundController.PlayerBoardCards.Count);
                    Assert.AreEqual(2, TestHelper.BattlegroundController.OpponentBoardCards.Count);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Banshee()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0,
                    new DeckCardData("Banshee", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
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
                       player => {},
                       opponent => {},
                       player => {},
                       opponent => {},
                       player => player.CardPlay(playerCardId, ItemPosition.Start),
                       opponent => opponent.CardPlay(opponentCardId, ItemPosition.Start),
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
        public IEnumerator Zonic()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0,
                    new DeckCardData("Zonic", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
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
                       player => {},
                       opponent => {},
                       player => {},
                       opponent => {},
                       player => player.CardPlay(playerCardId, ItemPosition.Start),
                       opponent => opponent.CardPlay(opponentCardId, ItemPosition.Start),
                       player => player.CardAttack(playerCardId, opponentCardId),
                };

                Action validateEndState = () =>
                {
                    BoardUnitModel playerUnit = ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCardId));
                    BoardUnitModel opponentUnit = ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCardId));
                    Assert.AreEqual(playerUnit.InitialHp, playerUnit.CurrentHp);
                    Assert.AreEqual(opponentUnit.InitialHp, opponentUnit.CurrentHp);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
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
                       player => {},
                       opponent => {},
                       player => {},
                       opponent => {},
                       player => player.CardPlay(playerWheezyId, ItemPosition.Start),
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
                       }
                };

                Action validateEndState = () =>
                {
                    Assert.NotNull(pvpTestContext.GetCurrentPlayer().CardsInHand.Select(card => card.LibraryCard.Cost < card.InstanceCard.Cost));
                    WorkingCard opponentCardInHand = TestHelper.BattlegroundController.GetWorkingCardByInstanceId(opponentWhizparId);
                    Assert.AreEqual(opponentCardInHand.LibraryCard.Cost - 1, opponentCardInHand.InstanceCard.Cost);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
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
                    player => {},
                    opponent => {},
                    player => {},
                    opponent => {},
                    player =>
                    {
                        player.CardPlay(playerSoothsayerId, ItemPosition.Start);
                    },
                    opponent =>
                    {
                        opponent.CardPlay(opponentSoothsayerId, ItemPosition.Start);
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
                    player => {},
                    opponent => {},
                    player => {},
                    opponent => {},
                    player => player.CardPlay(playerCardId, ItemPosition.Start),
                    opponent =>
                    {
                        opponent.CardPlay(opponentCardId, ItemPosition.Start);
                    },
                    player => { }
                };

                Action validateEndState = () =>
                {
                    Assert.NotNull(pvpTestContext.GetCurrentPlayer().CardsInHand.Select(card => card.LibraryCard.MouldId == 155));
                    Assert.NotNull(pvpTestContext.GetOpponentPlayer().CardsInHand.Select(card => card.LibraryCard.MouldId == 155));
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
                    Assert.AreEqual(playerUnit.InitialDamage + delayedDamage, playerUnit.CurrentDamage);
                    Assert.AreEqual(opponentUnit.InitialDamage + delayedDamage, opponentUnit.CurrentDamage);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
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
                    player => {},
                    opponent => {},
                    player =>
                    {
                        player.CardPlay(playerSlabId, ItemPosition.Start);
                        player.CardPlay(playerFlowZId, ItemPosition.Start);
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
                };

                Action validateEndState = () =>
                {
                    Assert.IsTrue(pvpTestContext.GetCurrentPlayer().CardsInHand.Count == 7);
                    Assert.IsTrue(pvpTestContext.GetOpponentPlayer().CardsInHand.Count == 7);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
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
                    Assert.AreEqual(playerSlab.InitialHp - difference, playerSlab.CurrentHp);
                    Assert.AreEqual(playerSlab3.InitialHp - difference, playerSlab3.CurrentHp);
                    Assert.AreEqual(opponentSlab.InitialHp - difference, opponentSlab.CurrentHp);
                    Assert.AreEqual(opponentSlab3.InitialHp - difference, opponentSlab3.CurrentHp);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }
    }
}
