using System;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using Loom.ZombieBattleground.Data;
using UnityEngine.TestTools;
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
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 3,
                    new DeckCardData("Banshee", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 3,
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
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 3,
                    new DeckCardData("Zonic", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 3,
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
        public IEnumerator Bouncer()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 3, new DeckCardData("Bouncer", 10));
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 3, new DeckCardData("Bouncer", 10));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Bouncer", 1);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Bouncer", 1);

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
        public IEnumerator Pushhh()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 3,
                    new DeckCardData("Pushhh", 4),
                    new DeckCardData("Bouncer", 4)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 3,
                    new DeckCardData("Pushhh", 4),
                    new DeckCardData("Bouncer", 4)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerPushhhId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Pushhh", 1);
                InstanceId playerBouncerId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Bouncer", 1);
                InstanceId opponentPushhhId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Pushhh", 1);
                InstanceId opponentBouncerId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Bouncer", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                       player => {},
                       opponent => {},
                       player => player.CardPlay(playerBouncerId, ItemPosition.Start),
                       opponent => opponent.CardPlay(opponentBouncerId, ItemPosition.Start),
                       player => player.CardPlay(playerPushhhId, ItemPosition.Start, opponentBouncerId),
                       opponent => opponent.CardPlay(opponentPushhhId, ItemPosition.Start, playerBouncerId)
                };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(1, pvpTestContext.GetCurrentPlayer().BoardCards.Count);
                    Assert.AreEqual(1, pvpTestContext.GetOpponentPlayer().BoardCards.Count);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Whiffer()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 3,
                    new DeckCardData("Whiffer", 4),
                    new DeckCardData("Bouncer", 4)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 3,
                    new DeckCardData("Whiffer", 4),
                    new DeckCardData("Bouncer", 4)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerWhifferId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Whiffer", 1);
                InstanceId playerBouncerId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Bouncer", 1);
                InstanceId opponentWhifferId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Whiffer", 1);
                InstanceId opponentBouncerId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Bouncer", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                       player => {},
                       opponent => {},
                       player => player.CardPlay(playerBouncerId, ItemPosition.Start),
                       opponent => opponent.CardPlay(opponentBouncerId, ItemPosition.Start),
                       player => player.CardPlay(playerWhifferId, ItemPosition.Start, playerBouncerId),
                       opponent => opponent.CardPlay(opponentWhifferId, ItemPosition.Start, opponentBouncerId)
                };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(1, pvpTestContext.GetCurrentPlayer().BoardCards.Count);
                    Assert.AreEqual(1, pvpTestContext.GetOpponentPlayer().BoardCards.Count);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Ztormonk()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 3,
                    new DeckCardData("Ztormonk", 2),
                    new DeckCardData("Zludge", 4)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 3,
                    new DeckCardData("Ztormonk", 2),
                    new DeckCardData("Zludge", 4)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerZtormonkId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Ztormonk", 1);
                InstanceId playerZkyId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zludge", 1);
                InstanceId playerZky1Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zludge", 2);
                InstanceId opponentZtormonkId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Ztormonk", 1);
                InstanceId opponentZkyId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zludge", 1);
                InstanceId opponentZky1Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zludge", 2);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                    player => {},
                    opponent => {},
                    player => {},
                    opponent => {},
                    player => player.CardPlay(playerZkyId, ItemPosition.Start),
                    opponent => opponent.CardPlay(opponentZkyId, ItemPosition.Start),
                    player => player.CardPlay(playerZky1Id, ItemPosition.End),
                    opponent => opponent.CardPlay(opponentZky1Id, ItemPosition.Start),
                    player => player.CardPlay(playerZtormonkId, ItemPosition.End),
                    opponent => opponent.CardPlay(opponentZtormonkId, new ItemPosition(1)),
                    player => {}
                };

                Action validateEndState = () =>
                {
                    Assert.IsTrue(((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerZky1Id)).HasBuffShield);
                    Assert.IsTrue(((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentZky1Id)).HasBuffShield);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Draft()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 3,
                    new DeckCardData("Draft", 7)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 3,
                    new DeckCardData("Draft", 7)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Draft", 1);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Draft", 1);

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
                    Assert.AreEqual(1, pvpTestContext.GetCurrentPlayer().BoardCards.Count);
                    Assert.AreEqual(1, pvpTestContext.GetOpponentPlayer().BoardCards.Count);
                    Assert.AreEqual(6, pvpTestContext.GetCurrentPlayer().CardsInHand.Count);
                    Assert.AreEqual(6, pvpTestContext.GetOpponentPlayer().CardsInHand.Count);
                    Assert.AreEqual(pvpTestContext.GetCurrentPlayer().InitialHp - 1, pvpTestContext.GetCurrentPlayer().Defense);
                    Assert.AreEqual(pvpTestContext.GetOpponentPlayer().InitialHp - 1, pvpTestContext.GetOpponentPlayer().Defense);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Gaz()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 3, new DeckCardData("Gaz", 10));
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 3, new DeckCardData("Gaz", 10));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Gaz", 1);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Gaz", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                    player => {},
                    opponent => {},
                    player => player.CardPlay(playerCardId, ItemPosition.Start),
                    opponent => opponent.CardPlay(opponentCardId, ItemPosition.Start),
                    player => {}
                };

                Action validateEndState = () =>
                {
                    Assert.IsTrue(pvpTestContext.GetCurrentPlayer().CardsInHand.FindAll(x => x.LibraryCard.MouldId == 156).Count > 0);
                    Assert.IsTrue(pvpTestContext.GetOpponentPlayer().CardsInHand.FindAll(x => x.LibraryCard.MouldId == 156).Count > 0);
                    Assert.IsTrue(((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCardId)).HasBuffShield);
                    Assert.IsTrue(((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCardId)).HasBuffShield);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }
    }
}
