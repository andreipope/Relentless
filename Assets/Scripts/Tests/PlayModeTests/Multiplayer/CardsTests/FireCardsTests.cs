using System;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using UnityEngine.TestTools;

namespace Loom.ZombieBattleground.Test.MultiplayerTests
{
    public class FireCardsTests : BaseIntegrationTest
    {
        [UnityTest]
        [Timeout(150 * 1000 * TestHelper.TestTimeScale)]
        public IEnumerator Pyromaz()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 1, new DeckCardData("Pyromaz", 1));
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 1, new DeckCardData("Zludge", 1));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Pyromaz", 1);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zludge", 1);

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
                    Assert.AreEqual(2, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCardId)).CurrentHp);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }

        [UnityTest]
        [Timeout(150 * 1000 * TestHelper.TestTimeScale)]
        public IEnumerator Zlinger()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 1, new DeckCardData("Zlinger", 10));
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 1, new DeckCardData("Zlinger", 10));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zlinger", 1);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zlinger", 1);

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
                    Assert.AreEqual(17, pvpTestContext.GetCurrentPlayer().Defense);
                    Assert.AreEqual(17, pvpTestContext.GetOpponentPlayer().Defense);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }

        [UnityTest]
        [Timeout(150 * 1000 * TestHelper.TestTimeScale)]
        public IEnumerator Sparky()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 1, new DeckCardData("Sparky", 10));
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 1, new DeckCardData("Sparky", 10));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Sparky", 1);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Sparky", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                    player => {},
                    opponent => {},
                    player => {},
                    opponent => {},
                    player => { },
                    opponent => opponent.CardPlay(opponentCardId, ItemPosition.Start),
                    player =>
                    {
                        player.CardPlay(playerCardId, ItemPosition.Start);
                        player.CardAttack(playerCardId, pvpTestContext.GetOpponentPlayer().InstanceId);
                    },
                    opponent => { },
                    player => { }
                };

                Action validateEndState = () =>
                {
                    BoardUnitView unitView = null;
                    try
                    {
                        unitView = TestHelper.GetCardOnBoardByInstanceId(playerCardId, Enumerators.MatchPlayer.CurrentPlayer);
                    }
                    catch (Exception e)
                    {
                        // all fine
                    }

                    Assert.IsNull(unitView);

                    Assert.AreEqual(18, pvpTestContext.GetOpponentPlayer().Defense);

                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }

        [UnityTest]
        [Timeout(150 * 1000 * TestHelper.TestTimeScale)]
        public IEnumerator Quazi()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 1, new DeckCardData("Quazi", 2));
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 1, new DeckCardData("Quazi", 2));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Quazi", 1);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Quazi", 1);

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
                    Assert.IsTrue(pvpTestContext.GetCurrentPlayer().CardsInHand.FindAll(x => x.LibraryCard.Name == "Tainted Goo").Count > 0);
                    Assert.IsTrue(pvpTestContext.GetOpponentPlayer().CardsInHand.FindAll(x => x.LibraryCard.Name == "Tainted Goo").Count > 0);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }

        [UnityTest]
        [Timeout(150 * 1000 * TestHelper.TestTimeScale)]
        public IEnumerator Ember()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 1, new DeckCardData("Ember", 10));
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 1, new DeckCardData("Ember", 10));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Ember", 1);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Ember", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                    player => {},
                    opponent => {},
                    player => {},
                    opponent => {},
                    player =>
                    {
                        player.CardPlay(playerCardId, ItemPosition.Start);
                        player.CardAttack(playerCardId, pvpTestContext.GetCurrentPlayer().InstanceId);
                    },
                    opponent =>
                    {
                        opponent.CardPlay(opponentCardId, ItemPosition.Start);
                        opponent.CardAttack(opponentCardId, pvpTestContext.GetOpponentPlayer().InstanceId);
                    }
                };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(19, pvpTestContext.GetCurrentPlayer().Defense);
                    Assert.AreEqual(19, pvpTestContext.GetOpponentPlayer().Defense);
                    Assert.IsTrue(((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCardId)).HasFeral);
                    Assert.IsTrue(((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCardId)).HasFeral);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }

        [UnityTest]
        [Timeout(150 * 1000 * TestHelper.TestTimeScale)]
        public IEnumerator Firewall()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 1, new DeckCardData("Firewall", 10));
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 1, new DeckCardData("Firewall", 10));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Firewall", 1);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Firewall", 1);

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
        [Timeout(150 * 1000 * TestHelper.TestTimeScale)]
        public IEnumerator BurZt()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 1, new DeckCardData("BurZt", 10));
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 1, new DeckCardData("BurZt", 10));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "BurZt", 1);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "BurZt", 1);

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
                        opponent.CardAbilityUsed(opponentCardId, Enumerators.AbilityType.SET_ATTACK_AVAILABILITY, new List<ParametrizedAbilityInstanceId>());
                    }
                };

                Action validateEndState = () =>
                {
                    Assert.IsTrue(!((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCardId))
                                     .AttackTargetsAvailability.Contains(Enumerators.SkillTargetType.OPPONENT));

                    Assert.IsTrue(!((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCardId))
                                     .AttackTargetsAvailability.Contains(Enumerators.SkillTargetType.OPPONENT));
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }

        [UnityTest]
        [Timeout(150 * 1000 * TestHelper.TestTimeScale)]
        public IEnumerator BlaZter()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 1, new DeckCardData("BlaZter", 2));
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 1, new DeckCardData("BlaZter", 2));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "BlaZter", 1);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "BlaZter", 1);
                InstanceId opponentCard1Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "BlaZter", 2);

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
                };

                Action validateEndState = () =>
                {
                    BoardUnitView unitView = null;
                    try
                    {
                        unitView = TestHelper.GetCardOnBoardByInstanceId(playerCardId, Enumerators.MatchPlayer.CurrentPlayer);
                    }
                    catch (Exception e)
                    {
                        // all fine
                    }

                    Assert.IsNull(unitView);

                    try
                    {
                        unitView = TestHelper.GetCardOnBoardByInstanceId(opponentCardId, Enumerators.MatchPlayer.OpponentPlayer);
                    }
                    catch (Exception e)
                    {
                        // all fine
                    }

                    Assert.IsNull(unitView);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }

        [UnityTest]
        [Timeout(150 * 1000 * TestHelper.TestTimeScale)]
        public IEnumerator Firecaller()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 1, new DeckCardData("Firecaller", 6));
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 1, new DeckCardData("Firecaller", 6));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Firecaller", 1);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Firecaller", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                    player => player.CardPlay(playerCardId, ItemPosition.Start),
                    opponent =>
                    {
                        opponent.CardPlay(opponentCardId, ItemPosition.Start);
                        opponent.CardAbilityUsed(opponentCardId, Enumerators.AbilityType.DRAW_CARD, new List<ParametrizedAbilityInstanceId>());
                    }
                };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(5, pvpTestContext.GetCurrentPlayer().CardsInHand.Count);
                    Assert.AreEqual(5, pvpTestContext.GetOpponentPlayer().CardsInHand.Count);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }

        [UnityTest]
        [Timeout(150 * 1000 * TestHelper.TestTimeScale)]
        public IEnumerator Burrrnn()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 1, new DeckCardData("Burrrnn", 10));
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 1, new DeckCardData("Burrrnn", 10));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Burrrnn", 1);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Burrrnn", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                    player => {},
                    opponent => {},
                    player => {},
                    opponent => {},
                    player =>
                    {
                        player.CardPlay(playerCardId, ItemPosition.Start);
                        player.CardAttack(playerCardId, pvpTestContext.GetCurrentPlayer().InstanceId);
                    },
                    opponent => { },
                    player => { },
                    opponent =>
                    {
                        opponent.CardPlay(opponentCardId, ItemPosition.Start);
                        opponent.CardAttack(opponentCardId, pvpTestContext.GetOpponentPlayer().InstanceId);
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
        [Timeout(150 * 1000 * TestHelper.TestTimeScale)]
        public IEnumerator RabieZ()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 1, new DeckCardData("RabieZ", 1), new DeckCardData("Pyromaz", 1));
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 1, new DeckCardData("RabieZ", 1), new DeckCardData("Pyromaz", 1));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "RabieZ", 1);
                InstanceId playerCard1Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Pyromaz", 1);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "RabieZ", 1);
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
                    },
                    player => player.CardAttack(playerCardId, opponentCardId),
                    opponent => { }
                };

                Action validateEndState = () =>
                {
                    BoardUnitView unitView = null;
                    try
                    {
                        unitView = TestHelper.GetCardOnBoardByInstanceId(playerCardId, Enumerators.MatchPlayer.CurrentPlayer);
                    }
                    catch (Exception e)
                    {
                        // all fine
                    }

                    Assert.IsNull(unitView);

                    try
                    {
                        unitView = TestHelper.GetCardOnBoardByInstanceId(opponentCardId, Enumerators.MatchPlayer.OpponentPlayer);
                    }
                    catch (Exception e)
                    {
                        // all fine
                    }

                    Assert.IsNull(unitView);

                    Assert.IsTrue(((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCard1Id)).HasFeral);
                    Assert.IsTrue(((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCard1Id)).HasFeral);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }

        [UnityTest]
        [Timeout(150 * 1000 * TestHelper.TestTimeScale)]
        public IEnumerator Flare()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 1, new DeckCardData("Flare", 1), new DeckCardData("Zludge", 1));
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 1, new DeckCardData("Flare", 1), new DeckCardData("Zludge", 1));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Flare", 1);
                InstanceId playerCard1Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zludge", 1);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Flare", 1);
                InstanceId opponentCard1Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zludge", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                    player => {},
                    opponent => {},
                    player => {},
                    opponent => {},
                    player => player.CardPlay(playerCard1Id, ItemPosition.Start),
                    opponent => opponent.CardPlay(opponentCard1Id, ItemPosition.Start),
                    player =>
                    {
                        player.CardPlay(playerCardId, ItemPosition.Start);
                        player.CardAttack(playerCardId, opponentCard1Id);
                    },
                    opponent =>
                    {
                        opponent.CardPlay(opponentCardId, ItemPosition.Start);
                        opponent.CardAbilityUsed(opponentCardId, Enumerators.AbilityType.BLITZ, new List<ParametrizedAbilityInstanceId>());
                        opponent.CardAttack(opponentCardId, playerCard1Id);
                    },
                    player => {}
                };

                Action validateEndState = () =>
                {
                    BoardUnitView unitView = null;
                    try
                    {
                        unitView = TestHelper.GetCardOnBoardByInstanceId(playerCardId, Enumerators.MatchPlayer.CurrentPlayer);
                    }
                    catch (Exception e)
                    {
                        // all fine
                    }

                    Assert.IsNull(unitView);

                    try
                    {
                        unitView = TestHelper.GetCardOnBoardByInstanceId(opponentCardId, Enumerators.MatchPlayer.OpponentPlayer);
                    }
                    catch (Exception e)
                    {
                        // all fine
                    }

                    Assert.IsNull(unitView);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }

        [UnityTest]
        [Timeout(150 * 1000 * TestHelper.TestTimeScale)]
        public IEnumerator Torchus()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 1, new DeckCardData("Torchus", 10));
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 1, new DeckCardData("Torchus", 10));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Torchus", 1);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Torchus", 1);

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
        [Timeout(150 * 1000 * TestHelper.TestTimeScale)]
        public IEnumerator Cynderman()
        {
            return AsyncTest(async () =>
            {
                Deck opponentDeck = new Deck(
                    0,
                    0,
                    "test deck",
                    new List<DeckCardData>
                    {
                       new DeckCardData("Cynderman", 2),
                       new DeckCardData("Slab", 2)
                    },
                    Enumerators.OverlordSkill.NONE,
                    Enumerators.OverlordSkill.NONE
                );

                Deck playerDeck = new Deck(
                    0,
                    0,
                    "test deck2",
                    new List<DeckCardData>
                    {
                       new DeckCardData("Cynderman", 2),
                       new DeckCardData("Slab", 2)
                    },
                    Enumerators.OverlordSkill.NONE,
                    Enumerators.OverlordSkill.NONE
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerSlabId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Slab", 1);
                InstanceId opponentSlabId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Slab", 1);
                InstanceId playerCyndermanId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Cynderman", 1);
                InstanceId opponentCyndermanId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Cynderman", 1);
                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                   {
                      player => {},
                      opponent => {},
                      player => {},
                      opponent => {},
                      player => player.CardPlay(playerSlabId, ItemPosition.Start),
                      opponent =>
                      {
                          opponent.CardPlay(opponentSlabId, ItemPosition.Start);
                          opponent.CardPlay(opponentCyndermanId, ItemPosition.Start, playerSlabId);
                      },
                      player =>
                      {
                          player.CardPlay(playerCyndermanId, ItemPosition.Start, opponentCyndermanId);
                      },
                   };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(2, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerSlabId)).CurrentHp);
                    Assert.AreEqual(2, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCyndermanId)).CurrentHp);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }

        [UnityTest]
        [Timeout(150 * 1000 * TestHelper.TestTimeScale)]
        public IEnumerator Werezomb()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 1, new DeckCardData("Werezomb", 1), new DeckCardData("Pyromaz", 1));
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 1, new DeckCardData("Werezomb", 1), new DeckCardData("Pyromaz", 1));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Werezomb", 1);
                InstanceId playerCard1Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Pyromaz", 1);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Werezomb", 1);
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
                        opponent.CardAbilityUsed(opponentCardId, Enumerators.AbilityType.BLITZ, new List<ParametrizedAbilityInstanceId>()
                        {
                            new ParametrizedAbilityInstanceId(opponentCard1Id)
                        });
                    }
                };

                Action validateEndState = () =>
                {
                    Assert.IsTrue(((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCardId)).HasFeral);
                    Assert.IsTrue(((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCardId)).HasFeral);
                    Assert.IsTrue(((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCard1Id)).HasBuffRush);
                    Assert.IsTrue(((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCard1Id)).HasBuffRush);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }

        [UnityTest]
        [Timeout(150 * 1000 * TestHelper.TestTimeScale)]
        public IEnumerator Modo()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 1, new DeckCardData("Modo", 10));
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 1, new DeckCardData("Modo", 10));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Modo", 1);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Modo", 1);

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
                    Assert.IsTrue(pvpTestContext.GetCurrentPlayer().CardsInHand.FindAll(x => x.LibraryCard.Name == "Corrupted Goo").Count > 0);
                    Assert.IsTrue(pvpTestContext.GetOpponentPlayer().CardsInHand.FindAll(x => x.LibraryCard.Name == "Corrupted Goo").Count > 0);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }
    }
}
