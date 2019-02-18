using System;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using UnityEngine.TestTools;

namespace Loom.ZombieBattleground.Test.MultiplayerTests
{
    public class EarthCardsTests : BaseIntegrationTest
    {
        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Rockky()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0, new DeckCardData("Rockky", 1));
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0, new DeckCardData("Mind Flayer", 1));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Rockky", 1);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Mind Flayer", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                       player => {},
                       opponent => {},
                       player => {},
                       opponent => {},
                       player => player.CardPlay(playerCardId, ItemPosition.Start),
                       opponent => opponent.CardPlay(opponentCardId, ItemPosition.Start),
                       player => player.CardAttack(playerCardId, opponentCardId)
                };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(3, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCardId)).CurrentHp);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator DuZt()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0, new DeckCardData("DuZt", 10));
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0, new DeckCardData("DuZt", 10));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "DuZt", 1);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "DuZt", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                    player => {},
                    opponent => {},
                    player => {},
                    opponent => {},
                    player => player.CardPlay(playerCardId, ItemPosition.Start),
                    opponent => opponent.CardPlay(opponentCardId, ItemPosition.Start),
                    player => player.CardAttack(playerCardId, opponentCardId),
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
        public IEnumerator Blocker()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0, new DeckCardData("Blocker", 10));
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0, new DeckCardData("Blocker", 10));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Blocker", 1);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Blocker", 1);

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
                    Assert.IsTrue(((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCardId)).IsHeavyUnit);
                    Assert.IsTrue(((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCardId)).IsHeavyUnit);
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
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0, new DeckCardData("Pit", 2));
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0, new DeckCardData("Pit", 2));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Pit", 1);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Pit", 1);

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
                        opponent.CardAbilityUsed(opponentCardId, Enumerators.AbilityType.ADD_CARD_BY_NAME_TO_HAND, new List<ParametrizedAbilityInstanceId>());
                    },
                    player => { }
                };

                Action validateEndState = () =>
                {
                    Assert.IsTrue(pvpTestContext.GetCurrentPlayer().CardsInHand.FindAll(x => x.LibraryCard.MouldId == 155).Count > 0);
                    Assert.IsTrue(pvpTestContext.GetOpponentPlayer().CardsInHand.FindAll(x => x.LibraryCard.MouldId == 155).Count > 0);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Bolderr()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 1, new DeckCardData("Bolderr", 2));
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 1, new DeckCardData("Bolderr", 2));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Bolderr", 1);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Bolderr", 1);
                InstanceId opponentCard1Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Bolderr", 2);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                    player => {},
                    opponent => {},
                    player => {},
                    opponent => opponent.CardPlay(opponentCardId, ItemPosition.Start),
                    player => player.CardPlay(playerCardId, ItemPosition.Start, opponentCardId),
                    opponent =>
                    {
                        opponent.CardPlay(opponentCard1Id, ItemPosition.Start);
                        opponent.CardAbilityUsed(opponentCard1Id, Enumerators.AbilityType.DAMAGE_TARGET, new List<ParametrizedAbilityInstanceId>()
                        {
                            new ParametrizedAbilityInstanceId(playerCardId)
                        });
                    },
                    player => {},
                    opponent => {}
                };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(1, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCardId)).CurrentHp);
                    Assert.AreEqual(1, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCardId)).CurrentHp);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Pebble()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 1, new DeckCardData("Pebble", 10));
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 1, new DeckCardData("Pebble", 10));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Pebble", 1);
                InstanceId playerCard1Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Pebble", 2);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Pebble", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                    player => {},
                    opponent => {},
                    player => {},
                    opponent => {},
                    player => player.CardPlay(playerCardId, ItemPosition.Start),
                    opponent => {},
                    player => {},
                    opponent =>
                    {
                        opponent.CardPlay(opponentCardId, ItemPosition.Start);
                        opponent.CardAbilityUsed(opponentCardId, Enumerators.AbilityType.TAKE_DAMAGE_RANDOM_ENEMY, new List<ParametrizedAbilityInstanceId>()
                        {
                            new ParametrizedAbilityInstanceId(playerCardId)
                        });
                    },
                    player => player.CardPlay(playerCard1Id, ItemPosition.Start)
                };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(1, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCardId)).CurrentHp);
                    Assert.AreEqual(1, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCardId)).CurrentHp);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Protector()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 1, new DeckCardData("Protector", 1), new DeckCardData("Pyromaz", 1));
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 1, new DeckCardData("Protector", 1), new DeckCardData("Pyromaz", 1));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Protector", 1);
                InstanceId playerCard1Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Pyromaz", 1);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Protector", 1);
                InstanceId opponentCard1Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Pyromaz", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                    player => {},
                    opponent => {},
                    player => {},
                    opponent => {},
                    player =>
                    {
                        player.CardPlay(playerCard1Id, ItemPosition.Start);
                        player.CardPlay(playerCardId, ItemPosition.Start);
                    },
                    opponent =>
                    {
                        opponent.CardPlay(opponentCard1Id, ItemPosition.Start);
                        opponent.CardPlay(opponentCardId, ItemPosition.Start);
                        opponent.CardAbilityUsed(opponentCardId, Enumerators.AbilityType.TAKE_UNIT_TYPE_TO_ALLY_UNIT, new List<ParametrizedAbilityInstanceId>()
                        {
                            new ParametrizedAbilityInstanceId(opponentCard1Id)
                        });
                    },
                    player => player.CardAttack(playerCardId, opponentCardId),
                    opponent => opponent.CardAttack(opponentCardId, playerCardId),
                    player => {},
                    opponent => {},
                };

                Action validateEndState = () =>
                {
                    Assert.IsNull(TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCardId));
                    Assert.IsNull(TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCardId));

                    Assert.IsTrue(((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCard1Id)).IsHeavyUnit);
                    Assert.IsTrue(((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCard1Id)).IsHeavyUnit);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Rubble()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0, new DeckCardData("Rubble", 10));
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0, new DeckCardData("Rubble", 10));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Rubble", 1);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Rubble", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                    player => {},
                    opponent => {},
                    player => {},
                    opponent => {},
                    player => player.CardPlay(playerCardId, ItemPosition.Start),
                    opponent => opponent.CardPlay(opponentCardId, ItemPosition.Start),
                    player => player.CardAttack(playerCardId, opponentCardId),
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
        public IEnumerator Tiny()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0, new DeckCardData("Tiny", 10));
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0, new DeckCardData("Tiny", 10));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Tiny", 1);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Tiny", 1);

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
                    Assert.IsTrue(((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCardId)).IsHeavyUnit);
                    Assert.IsTrue(((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCardId)).IsHeavyUnit);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Slab()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0, new DeckCardData("Slab", 10));
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0, new DeckCardData("Slab", 10));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Slab", 1);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Slab", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                    player => {},
                    opponent => {},
                    player => {},
                    opponent => {},
                    player => player.CardPlay(playerCardId, ItemPosition.Start),
                    opponent => opponent.CardPlay(opponentCardId, ItemPosition.Start),
                    player => player.CardAttack(playerCardId, opponentCardId),
                    opponent => opponent.CardAttack(opponentCardId, playerCardId),
                    player => {},
                    opponent => {},
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

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Shale", 1);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Shale", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                    player => {},
                    opponent => {},
                    player => {},
                    opponent => {},
                    player =>
                    {
                        player.CardPlay(playerCardId, ItemPosition.Start);
                        player.CardAttack(playerCardId, pvpTestContext.GetOpponentPlayer().InstanceId);
                    },
                    opponent => { },
                    player => { },
                    opponent =>
                    {
                        opponent.CardPlay(opponentCardId, ItemPosition.Start);
                        opponent.CardAttack(opponentCardId, pvpTestContext.GetCurrentPlayer().InstanceId);
                    }
                };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(17, pvpTestContext.GetCurrentPlayer().Defense);
                    Assert.AreEqual(17, pvpTestContext.GetOpponentPlayer().Defense);
                    Assert.IsTrue(((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCardId)).HasFeral);
                    Assert.IsTrue(((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCardId)).HasFeral);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
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

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Hardy", 1);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Hardy", 1);

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
                    Assert.IsTrue(((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCardId)).IsHeavyUnit);
                    Assert.IsTrue(((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCardId)).IsHeavyUnit);
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
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0, new DeckCardData("Crumbz", 10));
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0, new DeckCardData("Crumbz", 10));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Crumbz", 1);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Crumbz", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                    player => player.CardPlay(playerCardId, ItemPosition.Start),
                    opponent =>
                    {
                        opponent.CardPlay(opponentCardId, ItemPosition.Start);
                        opponent.CardAbilityUsed(opponentCardId, Enumerators.AbilityType.DRAW_CARD, new List<ParametrizedAbilityInstanceId>());
                    },
                    player => player.CardAttack(playerCardId, opponentCardId),
                    opponent => opponent.CardAttack(opponentCardId, playerCardId),
                    player => player.CardAttack(playerCardId, opponentCardId),
                    opponent => {},
                    player => {},
                    opponent => {},
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
        public IEnumerator Golem()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0, new DeckCardData("Golem", 10));
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0, new DeckCardData("Golem", 10));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Golem", 1);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Golem", 1);

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
                    Assert.IsTrue(((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCardId)).IsHeavyUnit);
                    Assert.IsTrue(((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCardId)).IsHeavyUnit);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Walley()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0,
                                                                   new DeckCardData("Walley", 1),
                                                                   new DeckCardData("Pyromaz", 4));
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                                                                    new DeckCardData("Walley", 1),
                                                                    new DeckCardData("Pyromaz", 4));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Walley", 1);
                InstanceId playerCard1Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Pyromaz", 1);
                InstanceId playerCard2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Pyromaz", 2);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Walley", 1);
                InstanceId opponentCard1Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Pyromaz", 1);
                InstanceId opponentCard2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Pyromaz", 2);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                    player => {},
                    opponent => {},
                    player => {},
                    opponent => {},
                    player => player.CardPlay(playerCard1Id, ItemPosition.Start),
                    opponent => opponent.CardPlay(opponentCard1Id, ItemPosition.Start),
                    player => player.CardPlay(playerCard2Id, ItemPosition.End),
                    opponent => opponent.CardPlay(opponentCard2Id, ItemPosition.Start),
                    player => player.CardPlay(playerCardId, ItemPosition.End),
                    opponent => opponent.CardPlay(opponentCardId, new ItemPosition(1)),
                    player => {}
                };

                Action validateEndState = () =>
                {
                    Assert.IsTrue(((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCard2Id)).IsHeavyUnit);
                    Assert.IsTrue(((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCard2Id)).IsHeavyUnit);
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

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Spiker", 1);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Spiker", 1);

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
                    Assert.IsTrue(((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCardId)).IsHeavyUnit);
                    Assert.IsTrue(((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCardId)).IsHeavyUnit);
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

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Crater", 1);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Crater", 1);

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
                        opponent.CardAbilityUsed(opponentCardId, Enumerators.AbilityType.ADD_CARD_BY_NAME_TO_HAND, new List<ParametrizedAbilityInstanceId>());
                    },
                    player => { }
                };

                Action validateEndState = () =>
                {
                    Assert.IsTrue(pvpTestContext.GetCurrentPlayer().CardsInHand.FindAll(x => x.LibraryCard.MouldId == 156).Count > 0);
                    Assert.IsTrue(pvpTestContext.GetOpponentPlayer().CardsInHand.FindAll(x => x.LibraryCard.MouldId == 156).Count > 0);
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

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Groundy", 1);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Groundy", 1);

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
                           opponent.CardAbilityUsed(opponentCardId, Enumerators.AbilityType.TAKE_UNIT_TYPE_TO_ALLY_UNIT, new List<ParametrizedAbilityInstanceId>());
                       },
                       player => {},
                       opponent => {}
                };

                Action validateEndState = () =>
                {
                    Assert.IsTrue(((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCardId)).IsHeavyUnit);
                    Assert.IsTrue(((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCardId)).IsHeavyUnit);
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
                                                                   new DeckCardData("Groundy", 1));
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 1,
                                                                   new DeckCardData("Earthshaker", 3),
                                                                   new DeckCardData("Groundy", 1));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Earthshaker", 1);
                InstanceId playerCard1Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Groundy", 1);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Earthshaker", 1);
                InstanceId opponentCard1Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Groundy", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                       player => {},
                       opponent => {},
                       player => player.CardPlay(playerCard1Id, ItemPosition.Start),
                       opponent => opponent.CardPlay(opponentCard1Id, ItemPosition.Start),
                       player => player.CardPlay(playerCardId, ItemPosition.Start, opponentCard1Id),
                       opponent =>
                       {
                           opponent.CardPlay(opponentCardId, ItemPosition.Start);
                           opponent.CardAbilityUsed(opponentCardId, Enumerators.AbilityType.DESTROY_UNIT_BY_TYPE, new List<ParametrizedAbilityInstanceId>()
                           {
                               new ParametrizedAbilityInstanceId(playerCard1Id)
                           });
                       },
                       player => {},
                       opponent => {}
                };

                Action validateEndState = () =>
                {
                    Assert.IsNull(TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCard1Id));
                    Assert.IsNull(TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCard1Id));
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, true);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator IgneouZ()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 1, new DeckCardData("IgneouZ", 1));
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 1, new DeckCardData("IgneouZ", 1));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "IgneouZ", 1);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "IgneouZ", 1);

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
                       player => player.CardPlay(playerCardId, ItemPosition.Start),
                       opponent => opponent.CardPlay(opponentCardId, ItemPosition.Start)
                };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(7, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCardId)).CurrentHp);
                    Assert.AreEqual(7, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCardId)).CurrentHp);
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
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 1, new DeckCardData("Pyrite", 1));
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 1, new DeckCardData("Pyrite", 1));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Pyrite", 1);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Pyrite", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                       player => {},
                       opponent => {},
                       player => player.CardPlay(playerCardId, ItemPosition.Start),
                       opponent =>
                       {
                           opponent.CardPlay(opponentCardId, ItemPosition.Start);
                           opponent.CardAbilityUsed(opponentCardId, Enumerators.AbilityType.DELAYED_LOSE_HEAVY_GAIN_ATTACK, new List<ParametrizedAbilityInstanceId>());
                       },
                       player => {},
                       opponent => {}
                };

                Action validateEndState = () =>
                {
                    Assert.IsFalse(((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCardId)).IsHeavyUnit);
                    Assert.IsFalse(((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCardId)).IsHeavyUnit);

                    Assert.AreEqual(7, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCardId)).CurrentDamage);
                    Assert.AreEqual(7, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCardId)).CurrentDamage);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Zolid()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0, new DeckCardData("Zolid", 10));
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0, new DeckCardData("Zolid", 10));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zolid", 1);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zolid", 1);

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
                    Assert.IsTrue(((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCardId)).IsHeavyUnit);
                    Assert.IsTrue(((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCardId)).IsHeavyUnit);
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
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 1,
                                                                  new DeckCardData("Defender", 1),
                                                                  new DeckCardData("Rockky", 3));
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 1,
                                                                    new DeckCardData("Defender", 1),
                                                                    new DeckCardData("Rockky", 3));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Defender", 1);
                InstanceId playerCard1Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Rockky", 1);
                InstanceId playerCard2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Rockky", 2);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Defender", 1);
                InstanceId opponentCard1Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Rockky", 1);
                InstanceId opponentCard2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Rockky", 2);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                       player => {},
                       opponent => {},
                       player => {},
                       opponent => {},
                       player =>
                       {
                           player.CardPlay(playerCard1Id, ItemPosition.Start);
                           player.CardPlay(playerCard2Id, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentCard1Id, ItemPosition.Start);
                           opponent.CardPlay(opponentCard2Id, ItemPosition.Start);
                       },
                       player => player.CardPlay(playerCardId, ItemPosition.Start),
                       opponent => opponent.CardPlay(opponentCardId, ItemPosition.Start)
                };

                Action validateEndState = () =>
                {
                    Assert.IsTrue(((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCard1Id)).IsHeavyUnit);
                    Assert.IsTrue(((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCard2Id)).IsHeavyUnit);
                    Assert.IsTrue(((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCard1Id)).IsHeavyUnit);
                    Assert.IsTrue(((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCard2Id)).IsHeavyUnit);
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
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 1, new DeckCardData("Mountain", 9));
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 1, new DeckCardData("Mountain", 9));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Mountain", 1);
                InstanceId playerCard1Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Mountain", 2);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Mountain", 1);
                InstanceId opponentCard1Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Mountain", 2);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                       player => {},
                       opponent => {},
                       player => player.CardPlay(playerCardId, ItemPosition.Start),
                       opponent => opponent.CardPlay(opponentCardId, ItemPosition.Start),
                       player => player.CardPlay(playerCard1Id, ItemPosition.Start),
                       opponent => opponent.CardPlay(opponentCard1Id, ItemPosition.Start),
                       player => player.CardAttack(playerCardId, opponentCardId),
                       opponent => opponent.CardAttack(opponentCard1Id, playerCard1Id),
                       player => {}
                };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(2, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCardId)).CurrentHp);
                    Assert.AreEqual(2, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCard1Id)).CurrentHp);
                    Assert.AreEqual(2, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCardId)).CurrentHp);
                    Assert.IsNull(TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCard1Id));
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
                                                                                  new DeckCardData("Rockky", 3));
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 1,
                                                                    new DeckCardData("Gaea", 1),
                                                                    new DeckCardData("Rockky", 3));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Gaea", 1);
                InstanceId playerCard1Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Rockky", 1);
                InstanceId playerCard2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Rockky", 2);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Gaea", 1);
                InstanceId opponentCard1Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Rockky", 1);
                InstanceId opponentCard2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Rockky", 2);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                       player => {},
                       opponent => {},
                       player => {},
                       opponent => {},
                       player =>
                       {
                           player.CardPlay(playerCard1Id, ItemPosition.Start);
                           player.CardPlay(playerCard2Id, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentCard1Id, ItemPosition.Start);
                           opponent.CardPlay(opponentCard2Id, ItemPosition.Start);
                       },
                       player => player.CardPlay(playerCardId, ItemPosition.Start),
                       opponent => opponent.CardPlay(opponentCardId, ItemPosition.Start),
                       player => {}
                };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(4, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCard1Id)).CurrentDamage);
                    Assert.AreEqual(4, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCard2Id)).CurrentDamage);
                    Assert.AreEqual(4, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCard1Id)).CurrentDamage);
                    Assert.AreEqual(4, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCard2Id)).CurrentDamage);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }
    }
}
