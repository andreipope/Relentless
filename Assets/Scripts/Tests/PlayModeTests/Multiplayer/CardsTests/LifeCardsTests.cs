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
        public IEnumerator Cactuz()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0, new TestCardData("Cactuz", 6));
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0, new TestCardData("Cactuz", 6));

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
        public IEnumerator Wood()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0,
                    new TestCardData("Wood", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    new TestCardData("Wood", 10)
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
        public IEnumerator Huzk()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0,
                    new TestCardData("Huzk", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    new TestCardData("Huzk", 10)
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
        public IEnumerator Yggdrazil()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0,
                    new TestCardData("Yggdrazil", 1),
                    new TestCardData("Trunk", 20)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    new TestCardData("Yggdrazil", 1),
                    new TestCardData("Trunk", 20)
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
        public IEnumerator Puffer()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0,
                    new TestCardData("Puffer", 1),
                    new TestCardData("Hot", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    new TestCardData("Puffer", 1),
                    new TestCardData("Hot", 10)
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
        public IEnumerator Azuraz()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 5,
                    new TestCardData("Azuraz", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 5,
                    new TestCardData("Azuraz", 10)
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

                    Assert.AreEqual(2, TestHelper.GameplayManager.CurrentPlayer.CardsOnBoard.Select(card => card.Card.Prototype.CardKey.MouldId.Id == id).ToList().Count);
                    Assert.AreEqual(2, TestHelper.GameplayManager.OpponentPlayer.CardsOnBoard.Select(card => card.Card.Prototype.CardKey.MouldId.Id == id).ToList().Count);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            }, 300);
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Bloomer()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0,
                    new TestCardData("Bloomer", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    new TestCardData("Bloomer", 10)
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
        public IEnumerator Zap()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 1, 
                    new TestCardData("Zap", 1),
                    new TestCardData("Trunk", 20)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 1,
                    new TestCardData("Zap", 1),
                    new TestCardData("Trunk", 20)
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
        public IEnumerator Amber()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0, new TestCardData("Amber", 10));
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0, new TestCardData("Amber", 10));

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
        public IEnumerator Prezerver()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 5,
                    new TestCardData("Prezerver", 1),
                    new TestCardData("Zlab", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 5,
                    new TestCardData("Prezerver", 1),
                    new TestCardData("Zlab", 10)
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
                    Assert.AreEqual(value, ((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentZlab2Id)).BuffedDefense);
                    Assert.AreEqual(value, ((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentZlab2Id)).BuffedDefense);
                    Assert.AreEqual(value, ((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerZlab1Id)).BuffedDamage);
                    Assert.AreEqual(value, ((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerZlab1Id)).BuffedDamage);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            }, 400);
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Keeper()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 5,
                    new TestCardData("Keeper", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 5,
                    new TestCardData("Keeper", 10)
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
                    new TestCardData("Wizp", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 5,
                    new TestCardData("Wizp", 10)
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
                    Assert.AreEqual(3, ((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerWiZpId)).CurrentDefense);
                    Assert.AreEqual(3, ((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentWiZpId)).CurrentDefense);
                    Assert.AreEqual(2, ((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerWiZpId)).CurrentDamage);
                    Assert.AreEqual(2, ((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentWiZpId)).CurrentDamage);
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
                    new TestCardData("Shroom", 1),
                    new TestCardData("Trunk", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 5,
                    new TestCardData("Shroom", 1),
                    new TestCardData("Trunk", 10)
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
        public IEnumerator Zucker()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 5,
                    new TestCardData("Zucker", 1),
                    new TestCardData("Bark", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 5,
                    new TestCardData("Zucker", 1),
                    new TestCardData("Bark", 10)
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
                       opponent => {
                           opponent.CardAttack(opponentZuckerId, playerBark2Id);
                       },
                       player => {
                           player.CardAttack(playerBark2Id, opponentZuckerId);
                       },
                       opponent => {}
                };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(1, ((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerBarkId)).BuffedDamage);
                    Assert.AreEqual(0, ((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentBarkId)).BuffedDamage);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            }, 300);
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator EverlaZting()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 5,
                    new TestCardData("Everlazting", 1),
                    new TestCardData("Zeeder", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 5,
                    new TestCardData("Everlazting", 1),
                    new TestCardData("Zeeder", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerEverlaztingId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Everlazting", 1);
                InstanceId playerZeederId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zeeder", 1);
                InstanceId opponentEverlaztingId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Everlazting", 1);
                InstanceId opponentZeederId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zeeder", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                       player => {},
                       opponent =>
                       {
                           opponent.CardPlay(opponentEverlaztingId, ItemPosition.Start);
                           opponent.CardAbilityUsed(opponentEverlaztingId, Enumerators.AbilityType.SHUFFLE_THIS_CARD_TO_DECK, new List<ParametrizedAbilityInstanceId>());
                           opponent.CardPlay(opponentZeederId, ItemPosition.Start, opponentEverlaztingId);
                       },
                       player =>
                       {
                           player.CardPlay(playerEverlaztingId, ItemPosition.Start);
                           player.CardAbilityUsed(playerEverlaztingId, Enumerators.AbilityType.SHUFFLE_THIS_CARD_TO_DECK, new List<ParametrizedAbilityInstanceId>());
                           player.CardPlay(playerZeederId, ItemPosition.Start, playerEverlaztingId);
                       },
                       opponent =>
                       {
                           opponent.CardAttack(opponentEverlaztingId, playerEverlaztingId);
                       },
                       player => {
                       },
                       opponent => {},
                       player => {}
                };

                Action validateEndState = () =>
                {
                    bool playerHasEverlazting = false;
                    bool opponentHasEverlazting = false;

                    string cardToFind = "Everlazting";

                    Assert.NotNull(TestHelper.GameplayManager.CurrentPlayer.CardsOnBoard.Select(card => card.Name == cardToFind));
                    Assert.NotNull(TestHelper.GameplayManager.OpponentPlayer.CardsOnBoard.Select(card => card.Name == cardToFind));

                    foreach (CardModel card in TestHelper.GameplayManager.CurrentPlayer.CardsInDeck)
                    {
                        if (card.Prototype.Name == cardToFind)
                        {
                            playerHasEverlazting = true;
                            break;
                        }
                    }

                    foreach (CardModel card in TestHelper.GameplayManager.CurrentPlayer.CardsInHand)
                    {
                        if (card.Card.Prototype.Name == cardToFind)
                        {
                            playerHasEverlazting = true;
                            break;
                        }
                    }

                    foreach (CardModel card in TestHelper.GameplayManager.OpponentPlayer.CardsInDeck)
                    {
                        if (card.Prototype.Name == cardToFind)
                        {
                            opponentHasEverlazting = true;
                            break;
                        }
                    }

                    foreach (CardModel card in TestHelper.GameplayManager.OpponentPlayer.CardsInHand)
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
                    new TestCardData("Planter", 1),
                    new TestCardData("Trunk", 20)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 5,
                    new TestCardData("Planter", 1),
                    new TestCardData("Trunk", 20)
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
                    new TestCardData("Zeeder", 1),
                    new TestCardData("Huzk", 20)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    new TestCardData("Zeeder", 1),
                    new TestCardData("Huzk", 20)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zeeder", 1);
                InstanceId playerHuzkId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Huzk", 1);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zeeder", 1);
                InstanceId opponentHuzkId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Huzk", 1);

                string cardToFind = "Huzk";

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                    player => {},
                    opponent => {},
                    player =>
                    {
                        player.CardPlay(playerHuzkId, ItemPosition.Start);
                        player.CardAbilityUsed(playerHuzkId, Enumerators.AbilityType.REANIMATE_UNIT, new List<ParametrizedAbilityInstanceId>());
                    },
                    opponent =>
                    {
                        opponent.CardPlay(opponentHuzkId, ItemPosition.Start);
                        opponent.CardAbilityUsed(opponentHuzkId, Enumerators.AbilityType.REANIMATE_UNIT, new List<ParametrizedAbilityInstanceId>());
                    },
                    player => 
                    {
                        player.CardAttack(playerHuzkId, opponentHuzkId);
                    },
                    opponent => {},
                    player => 
                    {
                        player.CardPlay(playerCardId, ItemPosition.Start, pvpTestContext.GetCurrentPlayer().CardsOnBoard.FindAll(card => card.Name == cardToFind).ToList()[0].Card.InstanceId);
                    },
                    opponent => 
                    {
                        opponent.CardPlay(opponentCardId, ItemPosition.Start, pvpTestContext.GetOpponentPlayer().CardsOnBoard.FindAll(card => card.Name == cardToFind).ToList()[0].Card.InstanceId);
                    },
                    player => {},
                    opponent => 
                    {
                        opponent.CardAttack(pvpTestContext.GetOpponentPlayer().CardsOnBoard.FindAll(card => card.Name == cardToFind).ToList()[0].Card.InstanceId, pvpTestContext.GetCurrentPlayer().CardsOnBoard.FindAll(card => card.Name == cardToFind).ToList()[0].Card.InstanceId);
                    },
                    player => {},
                    opponent => {}
                };

                Action validateEndState = () =>
                {
                    Assert.NotNull(TestHelper.GameplayManager.CurrentPlayer.CardsOnBoard.Select(card => card.Name == cardToFind));
                    Assert.NotNull(TestHelper.GameplayManager.OpponentPlayer.CardsOnBoard.Select(card => card.Name == cardToFind));
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            }, 400);
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator ZVirusWithLeash()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 5,
                    new TestCardData("Z-Virus", 1),
                    new TestCardData("Leash", 1),
                    new TestCardData("Vindrom", 1),
                    new TestCardData("Trunk", 20)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 5,
                    new TestCardData("Z-Virus", 1),
                    new TestCardData("Trunk", 20)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerZVirusId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Z-Virus", 1);
                InstanceId playerLeashId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Leash", 1);
                InstanceId playerVindromId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Vindrom", 1);
                InstanceId playerTrunkId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Trunk", 1);
                InstanceId opponentZVirusId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Z-Virus", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                       player => {},
                       opponent => {},
                       player =>
                       {
                           player.CardAbilityUsed(playerZVirusId, Enumerators.AbilityType.GAIN_STATS_OF_ADJACENT_UNITS, new List<ParametrizedAbilityInstanceId>());
                           player.CardPlay(playerZVirusId, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardAbilityUsed(opponentZVirusId, Enumerators.AbilityType.GAIN_STATS_OF_ADJACENT_UNITS, new List<ParametrizedAbilityInstanceId>());
                           opponent.CardPlay(opponentZVirusId, ItemPosition.Start);
                       },
                       player => 
                       {
                           player.CardPlay(playerLeashId, ItemPosition.Start, opponentZVirusId);
                       },
                       opponent => 
                       {},
                       player => {
                           player.CardPlay(playerTrunkId, ItemPosition.Start);
                       },
                       opponent => 
                       {},
                       player => 
                       {
                           player.CardAbilityUsed(playerVindromId, Enumerators.AbilityType.CHANGE_STAT, new List<ParametrizedAbilityInstanceId>());
                           player.CardPlay(playerVindromId, ItemPosition.Start);
                       },
                       opponent => {},
                       player => {}
                };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(10, ((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentZVirusId)).CurrentDamage);
                    Assert.AreEqual(14, ((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentZVirusId)).CurrentDefense);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            }, 300);
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator ZVirus()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 5,
                    new TestCardData("Z-Virus", 1),
                    new TestCardData("Geyzer", 1),
                    new TestCardData("Trunk", 20)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 5,
                    new TestCardData("Z-Virus", 1),
                    new TestCardData("Geyzer", 1),
                    new TestCardData("Trunk", 20)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerZVirusId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Z-Virus", 1);
                InstanceId playerTrunkId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Trunk", 1);
                InstanceId playerTrunk2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Trunk", 2);
                InstanceId opponentZVirusId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Z-Virus", 1);
                InstanceId opponentGeyzerId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Geyzer", 1);
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
                       player => 
                       {
                           player.CardPlay(playerZVirusId, ItemPosition.Start);
                           player.CardAbilityUsed(playerZVirusId, Enumerators.AbilityType.GAIN_STATS_OF_ADJACENT_UNITS, new List<ParametrizedAbilityInstanceId>());
                       },
                       opponent => 
                       {
                           opponent.CardPlay(opponentZVirusId, ItemPosition.Start);
                           opponent.CardAbilityUsed(opponentZVirusId, Enumerators.AbilityType.GAIN_STATS_OF_ADJACENT_UNITS, new List<ParametrizedAbilityInstanceId>());
                           opponent.CardPlay(opponentGeyzerId, ItemPosition.Start);
                       },
                       player => {
                       },
                       opponent => 
                       {
                           opponent.CardAttack(opponentGeyzerId, playerZVirusId);
                           opponent.CardAttack(opponentZVirusId, playerZVirusId);
                       },
                       player => 
                       {
                           player.CardPlay(playerTrunk2Id, ItemPosition.Start);
                       },
                       opponent => {},
                       player => {}
                };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(10, ((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentZVirusId)).CurrentDamage);
                    Assert.AreEqual(2, ((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentZVirusId)).CurrentDefense);
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
                    new TestCardData("Shaman", 1),
                    new TestCardData("Trunk", 20)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 5,
                    new TestCardData("Shaman", 1),
                    new TestCardData("Trunk", 20)
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
                    Assert.AreEqual(8, ((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerTrunk1Id)).CurrentDefense);
                    Assert.AreEqual(8, ((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerTrunk2Id)).CurrentDefense);
                    Assert.AreEqual(8, ((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentTrunk1Id)).CurrentDefense);
                    Assert.AreEqual(8, ((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentTrunk2Id)).CurrentDefense);
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
                    new TestCardData("Zplit", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 5,
                    new TestCardData("Zplit", 10)
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
                    new TestCardData("Blight", 1),
                    new TestCardData("Hot", 2),
                    new TestCardData("Yggdrazil", 1),
                    new TestCardData("Zeeder", 1),
                    new TestCardData("Zlab", 20)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 5,
                    new TestCardData("Blight", 1),
                    new TestCardData("Hot", 2),
                    new TestCardData("Yggdrazil", 1),
                    new TestCardData("Zeeder", 1),
                    new TestCardData("Zlab", 20)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerBlightId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Blight", 1);
                InstanceId playerHotId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Hot", 1);
                InstanceId playerHot2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Hot", 2);
                InstanceId playerYggdrazil2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Yggdrazil", 1);
                InstanceId playerZeederId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zeeder", 1);
                InstanceId opponentBlightId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Blight", 1);
                InstanceId opponentHotId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Hot", 1);
                InstanceId opponentHot2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Hot", 2);
                InstanceId opponentYggdrazil2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Yggdrazil", 1);
                InstanceId opponentZeederId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zeeder", 1);

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
                           player.CardPlay(playerZeederId, ItemPosition.Start, playerBlightId);
                       },
                       opponent => {
                           opponent.CardPlay(opponentZeederId, ItemPosition.Start, opponentBlightId);
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
                    Assert.NotNull((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerHot2Id));
                    Assert.NotNull((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerYggdrazil2Id));
                    Assert.NotNull((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentHot2Id));
                    Assert.NotNull((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentYggdrazil2Id));
                    Assert.AreEqual(5, pvpTestContext.GetCurrentPlayer().CardsOnBoard.Count);
                    Assert.AreEqual(5, pvpTestContext.GetOpponentPlayer().CardsOnBoard.Count);
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
                    new TestCardData("Rainz", 1),
                    new TestCardData("Zeeder", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 5,
                    new TestCardData("Rainz", 1),
                    new TestCardData("Zeeder", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerRainzId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Rainz", 1);
                InstanceId playerZeederId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zeeder", 1);
                InstanceId opponentRainzId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Rainz", 1);
                InstanceId opponentZeederId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zeeder", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                       player => {},
                       opponent => {},
                       player =>
                       {
                           player.CardPlay(playerRainzId, ItemPosition.Start);
                           player.CardAbilityUsed(playerRainzId, Enumerators.AbilityType.HEAL, new List<ParametrizedAbilityInstanceId>());
                           player.CardPlay(playerZeederId, ItemPosition.Start, playerRainzId);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentRainzId, ItemPosition.Start);
                           opponent.CardAbilityUsed(opponentRainzId, Enumerators.AbilityType.HEAL, new List<ParametrizedAbilityInstanceId>());
                           opponent.CardPlay(opponentZeederId, ItemPosition.Start, opponentRainzId);
                       },
                       player =>
                       {
                           player.CardAttack(playerRainzId, pvpTestContext.GetOpponentPlayer().InstanceId);
                           player.CardAttack(playerZeederId, pvpTestContext.GetOpponentPlayer().InstanceId);
                       },
                       opponent => 
                       {
                           opponent.CardAttack(opponentRainzId, pvpTestContext.GetCurrentPlayer().InstanceId);
                           opponent.CardAttack(opponentZeederId, pvpTestContext.GetCurrentPlayer().InstanceId);
                       },
                       player => 
                       {
                           player.CardAttack(playerRainzId, opponentRainzId);
                       },
                       opponent => {},
                       player => 
                       {
                           player.CardAttack(pvpTestContext.GetCurrentPlayer().CardsOnBoard.FindAll(card => card.Name == "Rainz").ToList()[0].Card.InstanceId, pvpTestContext.GetOpponentPlayer().CardsOnBoard.FindAll(card => card.Name == "Rainz").ToList()[0].Card.InstanceId);
                       },
                       opponent => {
                       },
                       player => {}

                };

                Action validateEndState = () =>
                {
                    int damageDealtLeft = 0;
                    Assert.AreEqual(pvpTestContext.GetCurrentPlayer().InitialDefense - damageDealtLeft, pvpTestContext.GetCurrentPlayer().Defense);
                    Assert.AreEqual(pvpTestContext.GetOpponentPlayer().InitialDefense - damageDealtLeft, pvpTestContext.GetOpponentPlayer().Defense);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            }, 500);
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        [Category("PlayQuickSubset")]
        public IEnumerator Vindrom()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 5,
                                    new TestCardData("Vindrom", 1),
                                    new TestCardData("Bark", 10)
                                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 5,
                    new TestCardData("Vindrom", 1),
                    new TestCardData("Bark", 10)
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
                    Assert.AreEqual(value, ((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerBarkId)).BuffedDefense);
                    Assert.AreEqual(value, ((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerBark2Id)).BuffedDefense);
                    Assert.AreEqual(value, ((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentBarkId)).BuffedDefense);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            }, 500);
        }
    }
}
