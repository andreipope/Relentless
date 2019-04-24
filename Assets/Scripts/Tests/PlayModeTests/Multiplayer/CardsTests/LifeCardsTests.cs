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
        [Category("PlayQuickSubset")]
        public IEnumerator Cactuz()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0, new DeckCardData("Cactuz", 6));
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0, new DeckCardData("Cactuz", 6));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Cactuz", 1);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Cactuz", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                    player =>
                    {
                        player.LetsThink(2);
                        player.CardPlay(playerCardId, ItemPosition.Start);
                        player.LetsThink(2);
                        player.AssertInQueue(() => {
                            Assert.AreEqual(2, pvpTestContext.GetCurrentPlayer().CurrentGoo);
                        });
                    },
                    opponent =>
                    {
                        opponent.LetsThink(2);
                        opponent.CardPlay(opponentCardId, ItemPosition.Start);
                        opponent.LetsThink(2);
                        opponent.AssertInQueue(() => {
                            Assert.AreEqual(2, pvpTestContext.GetOpponentPlayer().CurrentGoo);
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
        [Category("PlayQuickSubset")]
        public IEnumerator Wood()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0,
                    new DeckCardData("Wood", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    new DeckCardData("Wood", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Wood", 1);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Wood", 1);

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
        [Category("PlayQuickSubset")]
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

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

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
        [Category("PlayQuickSubset")]
        public IEnumerator Yggdrazil()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0,
                    new DeckCardData("Yggdrazil", 1),
                    new DeckCardData("Trunk", 20)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    new DeckCardData("Yggdrazil", 1),
                    new DeckCardData("Trunk", 20)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerYggdrazildId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Yggdrazil", 1);
                InstanceId playerTrunk1Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Trunk", 1);
                InstanceId playerTrunk2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Trunk", 2);
                InstanceId playerTrunk3Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Trunk", 3);
                InstanceId opponentYggdrazildId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Yggdrazil", 1);
                InstanceId opponentTrunk1Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Trunk", 1);
                InstanceId opponentTrunk2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Trunk", 2);
                InstanceId opponentTrunk3Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Trunk", 3);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                       player => {},
                       opponent => {},
                       player => {},
                       opponent => {},
                       player =>
                       {
                           player.CardPlay(playerTrunk1Id, ItemPosition.Start);
                           player.CardPlay(playerTrunk2Id, ItemPosition.Start);
                           player.CardPlay(playerTrunk3Id, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentTrunk1Id, ItemPosition.Start);
                           opponent.CardPlay(opponentTrunk2Id, ItemPosition.Start);
                           opponent.CardPlay(opponentTrunk3Id, ItemPosition.Start);
                       },
                       player =>
                       {
                           player.LetsThink(10);
                           player.CardAttack(playerTrunk1Id, opponentTrunk1Id);
                           player.CardAttack(playerTrunk2Id, opponentTrunk2Id);
                           player.CardAttack(playerTrunk3Id, opponentTrunk3Id);
                       },
                       opponent => {
                           opponent.LetsThink(10);
                           opponent.CardAttack(opponentTrunk1Id, playerTrunk1Id);
                           opponent.CardAttack(opponentTrunk2Id, playerTrunk2Id);
                           opponent.CardAttack(opponentTrunk3Id, playerTrunk3Id);
                       },
                       player => {},
                       opponent => {},
                       player => {
                           player.CardPlay(playerYggdrazildId, ItemPosition.Start);
                           player.CardAbilityUsed(playerYggdrazildId, Enumerators.AbilityType.PUT_UNITS_FROM_DISCARD_INTO_PLAY, new List<ParametrizedAbilityInstanceId>());
                       },
                       opponent => {
                           opponent.CardPlay(opponentYggdrazildId, ItemPosition.Start);
                           opponent.CardAbilityUsed(opponentYggdrazildId, Enumerators.AbilityType.PUT_UNITS_FROM_DISCARD_INTO_PLAY, new List<ParametrizedAbilityInstanceId>());
                       },
                       player => {
                           player.LetsThink(10);
                           player.CardAttack(playerYggdrazildId, opponentYggdrazildId);
                       },
                       opponent => {},
                };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(3, TestHelper.GameplayManager.CurrentPlayer.CardsOnBoard.Count);
                    Assert.AreEqual(3, TestHelper.GameplayManager.OpponentPlayer.CardsOnBoard.Count);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            }, 400);
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        [Category("PlayQuickSubset")]
        public IEnumerator Puffer()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0,
                    new DeckCardData("Puffer", 1),
                    new DeckCardData("Hot", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    new DeckCardData("Puffer", 1),
                    new DeckCardData("Hot", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Puffer", 1);
                InstanceId playerHotId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Hot", 1);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Puffer", 1);
                InstanceId opponentHotId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Hot", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                       player => {},
                       opponent => {},
                       player => {},
                       opponent => {},
                       player =>
                       {
                           player.CardPlay(playerHotId, ItemPosition.Start);
                           player.CardPlay(playerCardId, ItemPosition.Start);
                           player.CardAbilityUsed(playerCardId, Enumerators.AbilityType.REANIMATE_UNIT, new List<ParametrizedAbilityInstanceId>());
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentHotId, ItemPosition.Start);
                           opponent.CardPlay(opponentCardId, ItemPosition.Start);
                           opponent.CardAbilityUsed(opponentCardId, Enumerators.AbilityType.REANIMATE_UNIT, new List<ParametrizedAbilityInstanceId>());
                       },
                       player =>
                       {
                           player.CardAttack(playerHotId, opponentCardId);
                       },
                       opponent =>
                       {
                           opponent.CardAttack(opponentHotId, playerCardId);
                       },
                };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(2, TestHelper.GameplayManager.CurrentPlayer.CardsOnBoard.Count);
                    Assert.AreEqual(2, TestHelper.GameplayManager.OpponentPlayer.CardsOnBoard.Count);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            }, 300);
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        [Category("PlayQuickSubset")]
        public IEnumerator Azuraz()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 5,
                    new DeckCardData("Azuraz", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 5,
                    new DeckCardData("Azuraz", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerAzurazId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Azuraz", 1);
                InstanceId opponentAzurazId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Azuraz", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                       player => {},
                       opponent => {},
                       player =>
                       {
                           player.CardPlay(playerAzurazId, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentAzurazId, ItemPosition.Start);
                       },
                       player => {},
                       opponent => {}
                };

                Action validateEndState = () =>
                {
                    int id = 19;

                    Assert.AreEqual(2, TestHelper.GameplayManager.CurrentPlayer.CardsOnBoard.Select(card => card.Card.Prototype.MouldId == id).ToList().Count);
                    Assert.AreEqual(2, TestHelper.GameplayManager.OpponentPlayer.CardsOnBoard.Select(card => card.Card.Prototype.MouldId == id).ToList().Count);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            }, 300);
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        [Category("PlayQuickSubset")]
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

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerBloomer1Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Bloomer", 1);
                InstanceId playerBloomer2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Bloomer", 2);
                InstanceId opponentBloomer1Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Bloomer", 1);
                InstanceId opponentBloomer2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Bloomer", 2);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                       player => player.CardPlay(playerBloomer1Id, ItemPosition.Start),
                       opponent => opponent.CardPlay(opponentBloomer1Id, ItemPosition.Start),
                       player => player.CardPlay(playerBloomer2Id, ItemPosition.Start),
                       opponent => opponent.CardPlay(opponentBloomer2Id, ItemPosition.Start)
                };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(5, pvpTestContext.GetCurrentPlayer().CardsInHand.Count);
                    Assert.AreEqual(7, pvpTestContext.GetOpponentPlayer().CardsInHand.Count);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            }, 300);
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        [Category("PlayQuickSubset")]
        public IEnumerator Zap()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 1, 
                    new DeckCardData("Zap", 1),
                    new DeckCardData("Trunk", 20)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 1,
                    new DeckCardData("Zap", 1),
                    new DeckCardData("Trunk", 20)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerZapId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zap", 1);
                InstanceId playerTrunkId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Trunk", 1);
                InstanceId opponentZapId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zap", 1);
                InstanceId opponentTrunkId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Trunk", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                    player => {},
                    opponent => {},
                    player => {
                        player.CardPlay(playerTrunkId, ItemPosition.Start);
                        player.CardPlay(playerZapId, ItemPosition.Start);
                        player.CardAbilityUsed(playerZapId, Enumerators.AbilityType.GET_GOO_THIS_TURN, new List<ParametrizedAbilityInstanceId>());
                    },
                    opponent => {
                        opponent.CardPlay(opponentTrunkId, ItemPosition.Start);
                        opponent.CardPlay(opponentZapId, ItemPosition.Start);
                        opponent.CardAbilityUsed(opponentZapId, Enumerators.AbilityType.GET_GOO_THIS_TURN, new List<ParametrizedAbilityInstanceId>());
                    },
                    player =>
                    {
                        player.LetsThink(2);
                        player.CardAttack(playerZapId, opponentTrunkId);
                        player.LetsThink(15);
                        player.AssertInQueue(() => {
                            Assert.AreEqual(4, pvpTestContext.GetCurrentPlayer().CurrentGoo);
                        });
                    },
                    opponent =>
                    {
                        opponent.LetsThink(2);
                        opponent.CardAttack(opponentZapId, playerTrunkId);
                        opponent.LetsThink(15);
                        opponent.AssertInQueue(() => {
                            Assert.AreEqual(4, pvpTestContext.GetOpponentPlayer().CurrentGoo);
                        });
                    },
                    player => {},
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
        [Category("PlayQuickSubset")]
        public IEnumerator Amber()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0, new DeckCardData("Amber", 10));
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0, new DeckCardData("Amber", 10));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Amber", 1);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Amber", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                    player =>
                    {
                        player.LetsThink(2);
                        player.CardPlay(playerCardId, ItemPosition.Start);
                        player.LetsThink(2);
                        player.AssertInQueue(() => {
                            Assert.AreEqual(2, pvpTestContext.GetCurrentPlayer().GooVials);
                            Assert.AreEqual(1, pvpTestContext.GetOpponentPlayer().GooVials);
                        });
                    },
                    opponent =>
                    {
                        opponent.LetsThink(2);
                        opponent.CardPlay(opponentCardId, ItemPosition.Start);
                        opponent.LetsThink(2);
                        opponent.AssertInQueue(() => {
                            Assert.AreEqual(3, pvpTestContext.GetOpponentPlayer().GooVials);
                            Assert.AreEqual(3, pvpTestContext.GetCurrentPlayer().GooVials);
                        });
                    },
                };

                Action validateEndState = () => {};

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        [Category("PlayQuickSubset")]
        [Category("PlayQuickSubset2")]
        public IEnumerator Prezerver()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 5,
                    new DeckCardData("Prezerver", 1),
                    new DeckCardData("Zlab", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 5,
                    new DeckCardData("Prezerver", 1),
                    new DeckCardData("Zlab", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerPrezerverId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Prezerver", 1);
                InstanceId playerZlab1Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zlab", 1);
                InstanceId playerZlab2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zlab", 2);
                InstanceId opponentPrezerverId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Prezerver", 1);
                InstanceId opponentZlab1Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zlab", 1);
                InstanceId opponentZlab2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zlab", 2);

                int value = 3;

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
                        opponent.CardPlay(opponentPrezerverId, ItemPosition.Start, opponentZlab2Id);
                        opponent.CardAttack(opponentZlab2Id, playerZlab2Id);
                    },
                    player =>
                    {
                        player.CardPlay(playerPrezerverId, ItemPosition.Start, playerZlab1Id);
                        player.CardAttack(playerZlab1Id, opponentZlab1Id);
                    },
                };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(value, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentZlab2Id)).BuffedDefense);
                    Assert.AreEqual(value, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentZlab2Id)).BuffedDefense);
                    Assert.AreEqual(value, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerZlab1Id)).BuffedDamage);
                    Assert.AreEqual(value, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerZlab1Id)).BuffedDamage);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            }, 400);
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        [Category("PlayQuickSubset")]
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

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

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

                int cost = 2;

                Action validateEndState = () =>
                {
                    Assert.NotNull(TestHelper.GameplayManager.CurrentPlayer.CardsOnBoard.Select(card => card.InstanceId != playerKeeperId &&
                        card.CurrentCost <= cost));
                    Assert.NotNull(TestHelper.GameplayManager.OpponentPlayer.CardsOnBoard.Select(card => card.InstanceId != opponentKeeperId &&
                        card.CurrentCost <= cost));
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            }, 500);
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Wizp()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 5,
                    new DeckCardData("Wizp", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 5,
                    new DeckCardData("Wizp", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerWiZpId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Wizp", 1);
                InstanceId playerWiZp1Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Wizp", 2);
                InstanceId opponentWiZpId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Wizp", 1);
                InstanceId opponentWiZp1Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Wizp", 2);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                       player => {},
                       opponent => {},
                       player =>
                       {
                           player.CardPlay(playerWiZpId, ItemPosition.Start);
                           player.CardPlay(playerWiZp1Id, ItemPosition.Start, playerWiZpId);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentWiZpId, ItemPosition.Start);
                           opponent.CardPlay(opponentWiZp1Id, ItemPosition.Start, opponentWiZpId);
                       },
                       player => {}
                };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(3, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerWiZpId)).CurrentDefense);
                    Assert.AreEqual(3, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentWiZpId)).CurrentDefense);
                    Assert.AreEqual(2, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerWiZpId)).CurrentDamage);
                    Assert.AreEqual(2, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentWiZpId)).CurrentDamage);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            }, 500);
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Shroom()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 5,
                    new DeckCardData("Shroom", 1),
                    new DeckCardData("Trunk", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 5,
                    new DeckCardData("Shroom", 1),
                    new DeckCardData("Trunk", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerShroomId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Shroom", 1);
                InstanceId playerTrunkId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Trunk", 1);
                InstanceId opponentShroomId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Shroom", 1);
                InstanceId opponentTrunkId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Trunk", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                       player => {},
                       opponent =>
                       {
                           opponent.CardPlay(opponentShroomId, ItemPosition.Start);
                           opponent.CardAbilityUsed(opponentShroomId, Enumerators.AbilityType.FILL_BOARD_BY_UNITS, new List<ParametrizedAbilityInstanceId>());
                           opponent.CardPlay(opponentTrunkId, ItemPosition.Start);
                       },
                       player =>
                       {
                           player.CardPlay(playerShroomId, ItemPosition.Start);
                           player.CardAbilityUsed(playerShroomId, Enumerators.AbilityType.FILL_BOARD_BY_UNITS, new List<ParametrizedAbilityInstanceId>());
                           player.CardPlay(playerTrunkId, ItemPosition.Start);
                       },
                       opponent =>
                       {},
                       player => 
                       {
                           player.CardAttack(playerShroomId, opponentTrunkId);
                       },
                       opponent => {},
                       player => {}
                };

                Action validateEndState = () =>
                {
                   Assert.AreEqual(6, pvpTestContext.GetCurrentPlayer().CardsOnBoard.Count);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            }, 500);
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        [Category("PlayQuickSubset")]
        [Category("PlayQuickSubset2")]
        public IEnumerator Zucker()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 5,
                    new DeckCardData("Zucker", 1),
                    new DeckCardData("Bark", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 5,
                    new DeckCardData("Zucker", 1),
                    new DeckCardData("Bark", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerZuckerId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zucker", 1);
                InstanceId playerBarkId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Bark", 1);
                InstanceId playerBark2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Bark", 2);
                InstanceId opponentZuckerId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zucker", 1);
                InstanceId opponentBarkId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Bark", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                       player => {},
                       opponent =>
                       {
                           opponent.CardPlay(opponentBarkId, ItemPosition.Start);
                           opponent.CardPlay(opponentZuckerId, ItemPosition.Start);
                           opponent.CardAbilityUsed(opponentZuckerId, Enumerators.AbilityType.CHANGE_STAT, new List<ParametrizedAbilityInstanceId>());
                       },
                       player =>
                       {
                           player.CardPlay(playerBarkId, ItemPosition.Start);
                           player.CardPlay(playerBark2Id, ItemPosition.Start);
                           player.CardPlay(playerZuckerId, ItemPosition.Start);
                           player.CardAbilityUsed(playerZuckerId, Enumerators.AbilityType.CHANGE_STAT, new List<ParametrizedAbilityInstanceId>());
                       },
                       opponent => {},
                };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(1, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerBarkId)).BuffedDamage);
                    Assert.AreEqual(1, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerBark2Id)).BuffedDamage);
                    Assert.AreEqual(1, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentBarkId)).BuffedDamage);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            }, 300);
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        [Category("PlayQuickSubset")]
        public IEnumerator EverlaZting()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 5,
                    new DeckCardData("Everlazting", 1),
                    new DeckCardData("Zlab", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 5,
                    new DeckCardData("Everlazting", 1),
                    new DeckCardData("Zlab", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerEverlaztingId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Everlazting", 1);
                InstanceId playerZlabId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zlab", 1);
                InstanceId opponentEverlaztingId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Everlazting", 1);
                InstanceId opponentZlabId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zlab", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                       player => {},
                       opponent =>
                       {
                           opponent.CardPlay(opponentZlabId, ItemPosition.Start);
                           opponent.CardPlay(opponentEverlaztingId, ItemPosition.Start);
                           opponent.CardAbilityUsed(opponentEverlaztingId, Enumerators.AbilityType.SHUFFLE_THIS_CARD_TO_DECK, new List<ParametrizedAbilityInstanceId>());
                       },
                       player =>
                       {
                           player.CardPlay(playerZlabId, ItemPosition.Start);
                           player.CardPlay(playerEverlaztingId, ItemPosition.Start);
                           player.CardAbilityUsed(playerEverlaztingId, Enumerators.AbilityType.SHUFFLE_THIS_CARD_TO_DECK, new List<ParametrizedAbilityInstanceId>());
                       },
                       opponent =>
                       {
                           opponent.CardAttack(opponentEverlaztingId, playerZlabId);
                       },
                       player => {
                           player.CardAttack(playerEverlaztingId, opponentZlabId);
                       },
                       opponent => {},
                       player => {}
                };

                Action validateEndState = () =>
                {
                    bool playerHasEverlazting = false;
                    bool opponentHasEverlazting = false;

                    string cardToFind = "Everlazting";

                    foreach (BoardUnitModel card in TestHelper.GameplayManager.CurrentPlayer.CardsInDeck)
                    {
                        if (card.Prototype.Name == cardToFind)
                        {
                            playerHasEverlazting = true;
                            break;
                        }
                    }

                    foreach (BoardUnitModel card in TestHelper.GameplayManager.CurrentPlayer.CardsInHand)
                    {
                        if (card.Card.Prototype.Name == cardToFind)
                        {
                            playerHasEverlazting = true;
                            break;
                        }
                    }

                    foreach (BoardUnitModel card in TestHelper.GameplayManager.OpponentPlayer.CardsInDeck)
                    {
                        if (card.Prototype.Name == cardToFind)
                        {
                            opponentHasEverlazting = true;
                            break;
                        }
                    }

                    foreach (BoardUnitModel card in TestHelper.GameplayManager.OpponentPlayer.CardsInHand)
                    {
                        if (card.Card.Prototype.Name == cardToFind)
                        {
                            opponentHasEverlazting = true;
                            break;
                        }
                    }

                    Assert.IsTrue(playerHasEverlazting);
                    Assert.IsTrue(opponentHasEverlazting);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            }, 300);
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Planter()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 5,
                    new DeckCardData("Planter", 1),
                    new DeckCardData("Trunk", 20)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 5,
                    new DeckCardData("Planter", 1),
                    new DeckCardData("Trunk", 20)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerPlanterId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Planter", 1);
                InstanceId opponentPlanterId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Planter", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                       player => {},
                       opponent => {},
                       player =>
                       {
                           player.CardPlay(playerPlanterId, ItemPosition.Start);
                           player.CardAbilityUsed(playerPlanterId, Enumerators.AbilityType.PUT_UNITS_FRON_LIBRARY_INTO_PLAY, new List<ParametrizedAbilityInstanceId>());
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentPlanterId, ItemPosition.Start);
                           opponent.CardAbilityUsed(opponentPlanterId, Enumerators.AbilityType.PUT_UNITS_FRON_LIBRARY_INTO_PLAY, new List<ParametrizedAbilityInstanceId>());
                       },
                       player =>
                       {
                           player.CardAttack(playerPlanterId, opponentPlanterId);
                       },
                       opponent => {},
                       player => {}
                };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(1, pvpTestContext.GetCurrentPlayer().CardsOnBoard.Count);
                    Assert.AreEqual(1, pvpTestContext.GetOpponentPlayer().CardsOnBoard.Count);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            }, 500);
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Zeeder()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0,
                    new DeckCardData("Zeeder", 1),
                    new DeckCardData("Hot", 20)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    new DeckCardData("Zeeder", 1),
                    new DeckCardData("Hot", 20)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zeeder", 1);
                InstanceId playerHotId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Hot", 1);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zeeder", 1);
                InstanceId opponentHotId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Hot", 1);

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
                    BoardUnitModel playerUnit = (BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerHotId);
                    BoardUnitModel opponentUnit = (BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentHotId);
                    Assert.IsTrue(playerUnit.BuffsOnUnit.Contains(Enumerators.BuffType.REANIMATE));
                    Assert.IsTrue(opponentUnit.BuffsOnUnit.Contains(Enumerators.BuffType.REANIMATE));
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            }, 400);
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator ZVirus()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 5,
                    new DeckCardData("Z-Virus", 1),
                    new DeckCardData("Trunk", 20)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 5,
                    new DeckCardData("Z-Virus", 1),
                    new DeckCardData("Trunk", 20)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerZVirusId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Z-Virus", 1);
                InstanceId playerTrunkId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Trunk", 1);
                InstanceId opponentZVirusId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Z-Virus", 1);
                InstanceId opponentTrunkId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Trunk", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                       player => {},
                       opponent => {},
                       player =>
                       {
                           player.CardPlay(playerTrunkId, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentTrunkId, ItemPosition.Start);
                        },
                       player => {
                           player.CardPlay(playerZVirusId, ItemPosition.Start);
                           player.CardAbilityUsed(playerZVirusId, Enumerators.AbilityType.GAIN_STATS_OF_ADJACENT_UNITS, new List<ParametrizedAbilityInstanceId>());
                       },
                       opponent => {
                           opponent.CardPlay(opponentZVirusId, ItemPosition.Start);
                           opponent.CardAbilityUsed(opponentZVirusId, Enumerators.AbilityType.GAIN_STATS_OF_ADJACENT_UNITS, new List<ParametrizedAbilityInstanceId>());
                       },
                       player => {},
                       opponent => {}
                };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(10, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerZVirusId)).CurrentDamage);
                    Assert.AreEqual(12, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerZVirusId)).CurrentDefense);
                    Assert.AreEqual(10, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentZVirusId)).CurrentDamage);
                    Assert.AreEqual(12, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentZVirusId)).CurrentDefense);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            }, 300);
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Shaman()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 5,
                    new DeckCardData("Shaman", 1),
                    new DeckCardData("Trunk", 20)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 5,
                    new DeckCardData("Shaman", 1),
                    new DeckCardData("Trunk", 20)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerShamanId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Shaman", 1);
                InstanceId playerTrunk1Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Trunk", 1);
                InstanceId playerTrunk2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Trunk", 2);
                InstanceId opponentShamanId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Shaman", 1);
                InstanceId opponentTrunk1Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Trunk", 1);
                InstanceId opponentTrunk2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Trunk", 2);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                       player => {},
                       opponent => {},
                       player =>
                       {
                           player.CardPlay(playerShamanId, ItemPosition.Start);
                           player.CardAbilityUsed(playerShamanId, Enumerators.AbilityType.HEAL, new List<ParametrizedAbilityInstanceId>());
                           player.CardPlay(playerTrunk1Id, ItemPosition.Start);
                           player.CardPlay(playerTrunk2Id, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentShamanId, ItemPosition.Start);
                           opponent.CardAbilityUsed(opponentShamanId, Enumerators.AbilityType.HEAL, new List<ParametrizedAbilityInstanceId>());
                           opponent.CardPlay(opponentTrunk1Id, ItemPosition.Start);
                           opponent.CardPlay(opponentTrunk2Id, ItemPosition.Start);
                       },
                       player => {
                           player.CardAttack(playerTrunk1Id, opponentTrunk1Id);
                           player.CardAttack(playerTrunk2Id, opponentTrunk2Id);
                       },
                       opponent => {},
                       player => {},
                       opponent => {},
                       player => {},
                       opponent => {}
                };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(8, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerTrunk1Id)).CurrentDefense);
                    Assert.AreEqual(8, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerTrunk2Id)).CurrentDefense);
                    Assert.AreEqual(8, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentTrunk1Id)).CurrentDefense);
                    Assert.AreEqual(8, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentTrunk2Id)).CurrentDefense);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            }, 300);
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Zplit()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 5,
                    new DeckCardData("Zplit", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 5,
                    new DeckCardData("Zplit", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerZplitterId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zplit", 1);
                InstanceId opponentZplitterId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zplit", 1);

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
                    string cardToFind = "Zplit";

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
                    new DeckCardData("Blight", 1),
                    new DeckCardData("Hot", 4),
                    new DeckCardData("Zlab", 5)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 5,
                    new DeckCardData("Blight", 1),
                    new DeckCardData("Hot", 4),
                    new DeckCardData("Zlab", 5)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerBlightId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Blight", 1);
                InstanceId playerHotId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Hot", 1);
                InstanceId playerHot2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Hot", 2);
                InstanceId playerHot3Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Hot", 3);
                InstanceId opponentBlightId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Blight", 1);
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
                           player.CardPlay(playerBlightId, ItemPosition.Start);
                           player.CardAbilityUsed(playerBlightId, Enumerators.AbilityType.SUMMON_UNIT_FROM_HAND, new List<ParametrizedAbilityInstanceId>());
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentHotId, ItemPosition.Start);
                           opponent.CardPlay(opponentBlightId, ItemPosition.Start);
                           opponent.CardAbilityUsed(opponentBlightId, Enumerators.AbilityType.SUMMON_UNIT_FROM_HAND, new List<ParametrizedAbilityInstanceId>());
                       },
                       player =>
                       {
                           player.CardAttack(playerHotId, opponentBlightId);
                       },
                       opponent =>
                       {
                           opponent.CardAttack(opponentHotId, playerBlightId);
                       },
                       player => {}
                };

                Action validateEndState = () =>
                {
                    Assert.NotNull((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerHot2Id));
                    Assert.NotNull((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerHot3Id));
                    Assert.NotNull((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentHot2Id));
                    Assert.NotNull((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentHot3Id));
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
                    new DeckCardData("Trunk", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 5,
                    new DeckCardData("Rainz", 1),
                    new DeckCardData("Trunk", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerRainzId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Rainz", 1);
                InstanceId playerTrunk1Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Trunk", 1);
                InstanceId opponentRainzId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Rainz", 1);
                InstanceId opponentTrunk1Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Trunk", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                       player => {},
                       opponent => {},
                       player =>
                       {
                           player.CardPlay(playerRainzId, ItemPosition.Start);
                           player.CardAbilityUsed(playerRainzId, Enumerators.AbilityType.HEAL, new List<ParametrizedAbilityInstanceId>());
                           player.CardPlay(playerTrunk1Id, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentRainzId, ItemPosition.Start);
                           opponent.CardAbilityUsed(opponentRainzId, Enumerators.AbilityType.HEAL, new List<ParametrizedAbilityInstanceId>());
                           opponent.CardPlay(opponentTrunk1Id, ItemPosition.Start);
                       },
                       player =>
                       {
                           player.CardAttack(playerRainzId, pvpTestContext.GetOpponentPlayer().InstanceId);
                           player.CardAttack(playerTrunk1Id, pvpTestContext.GetOpponentPlayer().InstanceId);
                       },
                       opponent => 
                       {
                           opponent.CardAttack(opponentRainzId, pvpTestContext.GetCurrentPlayer().InstanceId);
                           opponent.CardAttack(opponentTrunk1Id, pvpTestContext.GetCurrentPlayer().InstanceId);
                       },
                       player => 
                       {
                           player.CardAttack(playerRainzId, opponentRainzId);
                       },
                       opponent => {},
                       player => {}

                };

                Action validateEndState = () =>
                {
                    int damageDealtLeft = 4;
                    Assert.AreEqual(pvpTestContext.GetCurrentPlayer().InitialDefense - damageDealtLeft, pvpTestContext.GetCurrentPlayer().Defense);
                    Assert.AreEqual(pvpTestContext.GetOpponentPlayer().InitialDefense - damageDealtLeft, pvpTestContext.GetOpponentPlayer().Defense);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            }, 500);
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        [Category("PlayQuickSubset2")]
        public IEnumerator Vindrom()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 5,
                                    new DeckCardData("Vindrom", 1),
                                    new DeckCardData("Bark", 10)
                                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 5,
                    new DeckCardData("Vindrom", 1),
                    new DeckCardData("Bark", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerVindromId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Vindrom", 1);
                InstanceId playerBarkId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Bark", 1);
                InstanceId playerBark2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Bark", 2);
                InstanceId opponentVindromId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Vindrom", 1);
                InstanceId opponentBarkId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Bark", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                       player => {},
                       opponent => {},
                       player => {},
                       opponent =>
                       {
                           opponent.CardPlay(opponentBarkId, ItemPosition.Start);
                           opponent.CardPlay(opponentVindromId, ItemPosition.Start);
                           opponent.CardAbilityUsed(opponentVindromId, Enumerators.AbilityType.CHANGE_STAT, new List<ParametrizedAbilityInstanceId>());
                       },
                       player =>
                       {
                           player.CardPlay(playerBarkId, ItemPosition.Start);
                           player.CardPlay(playerBark2Id, ItemPosition.Start);
                           player.CardPlay(playerVindromId, ItemPosition.Start);
                           player.CardAbilityUsed(playerVindromId, Enumerators.AbilityType.CHANGE_STAT, new List<ParametrizedAbilityInstanceId>());
                       },
                       opponent => {},
                       player => {}
                };

                int value = 2;

                Action validateEndState = () =>
                {
                    Assert.AreEqual(value, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerBarkId)).BuffedDefense);
                    Assert.AreEqual(value, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerBark2Id)).BuffedDefense);
                    Assert.AreEqual(value, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentBarkId)).BuffedDefense);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            }, 500);
        }
    }
}
