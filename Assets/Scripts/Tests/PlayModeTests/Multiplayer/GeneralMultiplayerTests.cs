using System;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using log4net;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.Protobuf;
using UnityEngine;
using UnityEngine.TestTools;
using Deck = Loom.ZombieBattleground.Data.Deck;
using InstanceId = Loom.ZombieBattleground.Data.InstanceId;

namespace Loom.ZombieBattleground.Test.MultiplayerTests
{
    public class GeneralMultiplayerTests : BaseIntegrationTest
    {
        private static readonly ILog Log = Logging.GetLog(nameof(GeneralMultiplayerTests));

        [UnityTest]
        [Timeout(int.MaxValue)]
        [Ignore("seems broken")]
        public IEnumerator Slab()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = new Deck(
                    0,
                    0,
                    "test deck",
                    new List<DeckCardData>
                    {
                        new DeckCardData("Slab", 30)
                    },
                    Enumerators.OverlordSkill.NONE,
                    Enumerators.OverlordSkill.NONE
                );

                Deck opponentDeck = new Deck(
                    0,
                    0,
                    "test deck2",
                    new List<DeckCardData>
                    {
                        new DeckCardData("Slab", 30)
                    },
                    Enumerators.OverlordSkill.NONE,
                    Enumerators.OverlordSkill.NONE
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck) {
                    Player1HasFirstTurn = true
                };

                InstanceId playerSlabId = pvpTestContext.GetCardInstanceIdByIndex(playerDeck, 0);
                InstanceId opponentSlabId = pvpTestContext.GetCardInstanceIdByIndex(opponentDeck, 0);
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
                       player => player.CardPlay(playerSlabId, ItemPosition.Start),
                       opponent => opponent.CardPlay(opponentSlabId, ItemPosition.Start),
                       player => player.CardAttack(playerSlabId, pvpTestContext.GetOpponentPlayer().InstanceId),
                       opponent => opponent.CardAttack(opponentSlabId, pvpTestContext.GetCurrentPlayer().InstanceId),
                       player => player.CardAttack(playerSlabId, pvpTestContext.GetOpponentPlayer().InstanceId),
                       opponent => opponent.CardAttack(opponentSlabId, pvpTestContext.GetCurrentPlayer().InstanceId),
                       player => player.CardAttack(playerSlabId, pvpTestContext.GetOpponentPlayer().InstanceId),
                       opponent => {},
                       player => player.CardAttack(playerSlabId, pvpTestContext.GetOpponentPlayer().InstanceId),
                       opponent => {},
                       player => player.CardAttack(playerSlabId, pvpTestContext.GetOpponentPlayer().InstanceId),
                       opponent => {},
                       player => player.CardAttack(playerSlabId, pvpTestContext.GetOpponentPlayer().InstanceId),
                       opponent => {},
                       player => player.CardAttack(playerSlabId, pvpTestContext.GetOpponentPlayer().InstanceId),
                       opponent => {},
                       player => player.CardAttack(playerSlabId, pvpTestContext.GetOpponentPlayer().InstanceId),
                       opponent => {},
                       player => player.CardAttack(playerSlabId, pvpTestContext.GetOpponentPlayer().InstanceId),
                       opponent => {},
                       player => player.CardAttack(playerSlabId, pvpTestContext.GetOpponentPlayer().InstanceId),
                   };

                await PvPTestUtility.GenericPvPTest(
                    pvpTestContext,
                    turns,
                    () =>
                    {
                        // FIXME: references to the players are nulled immediately after the game ends,
                        // so we can't assert the state at that moment?
                        //Assert.AreEqual(0, pvpTestContext.GetOpponentPlayer().Defense);
                    }
                );
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Bane()
        {
            return AsyncTest(async () =>
            {
                Deck opponentDeck = new Deck(
                    0,
                    0,
                    "test deck",
                    new List<DeckCardData>
                    {
                        new DeckCardData("Slab", 30)
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
                        new DeckCardData("Bane", 30)
                    },
                    Enumerators.OverlordSkill.NONE,
                    Enumerators.OverlordSkill.NONE
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck) {
                    Player1HasFirstTurn = true
                };

                InstanceId playerBaneId = pvpTestContext.GetCardInstanceIdByIndex(playerDeck, 0);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                   {
                       player => {},
                       opponent => {},
                       player => {},
                       opponent => {},
                       player => {},
                       opponent => {},

                       player => player.CardPlay(playerBaneId, ItemPosition.Start),
                       opponent => {}
                   };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(19, pvpTestContext.GetCurrentPlayer().Defense);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }


        [UnityTest]
        [Timeout(int.MaxValue)]
        //[Ignore("seems broken")]
        public IEnumerator Zeptic_Lose()
        {
            return AsyncTest(async () =>
            {
                Deck opponentDeck = new Deck(
                    0,
                    0,
                    "test deck",
                    new List<DeckCardData>
                    {
                        new DeckCardData("Slab", 30)
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
                        new DeckCardData("Zeptic", 30)
                    },
                    Enumerators.OverlordSkill.NONE,
                    Enumerators.OverlordSkill.NONE
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck) {
                    Player1HasFirstTurn = true
                };

                InstanceId playerZepticId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zeptic", 1);
                InstanceId playerZepticId2 = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zeptic", 2);
                InstanceId opponentSlabId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Slab", 1);

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

                       player => {},
                       opponent => opponent.CardPlay(opponentSlabId, ItemPosition.Start),

                       player => player.CardPlay(playerZepticId, ItemPosition.Start),
                       opponent => opponent.CardAttack(opponentSlabId, pvpTestContext.GetCurrentPlayer().InstanceId),

                       player => {},
                       opponent => opponent.CardAttack(opponentSlabId, pvpTestContext.GetCurrentPlayer().InstanceId),

                       player => {},
                       opponent => opponent.CardAttack(opponentSlabId, pvpTestContext.GetCurrentPlayer().InstanceId),

                       player => {},
                       opponent => opponent.CardAttack(opponentSlabId, pvpTestContext.GetCurrentPlayer().InstanceId),

                       player => {},
                       opponent => opponent.CardAttack(opponentSlabId, pvpTestContext.GetCurrentPlayer().InstanceId),

                       player => {},
                       opponent => opponent.CardAttack(opponentSlabId, pvpTestContext.GetCurrentPlayer().InstanceId),

                       player => {},
                       opponent => opponent.CardAttack(opponentSlabId, pvpTestContext.GetCurrentPlayer().InstanceId),

                       player => {},
                       opponent => opponent.CardAttack(opponentSlabId, pvpTestContext.GetCurrentPlayer().InstanceId),

                       player => player.CardPlay(playerZepticId2, ItemPosition.Start),
                       opponent => {}
                   };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(0, pvpTestContext.GetCurrentPlayer().Defense);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }


        


        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator CorrectCardDraw()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = new Deck(
                    0,
                    0,
                    "test deck",
                    new List<DeckCardData>
                    {
                        new DeckCardData("Slab", 30)
                    },
                    Enumerators.OverlordSkill.NONE,
                    Enumerators.OverlordSkill.NONE
                );

                Deck opponentDeck = new Deck(
                    0,
                    0,
                    "test deck2",
                    new List<DeckCardData>
                    {
                        new DeckCardData("Slab", 30)
                    },
                    Enumerators.OverlordSkill.NONE,
                    Enumerators.OverlordSkill.NONE
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck) {
                    Player1HasFirstTurn = true
                };

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                    player =>
                    {
                        Assert.AreEqual(4, pvpTestContext.GetCurrentPlayer().CardsInHand.Count);
                        Assert.AreEqual(3, pvpTestContext.GetOpponentPlayer().CardsInHand.Count);
                    },
                    opponent =>
                    {
                        Assert.AreEqual(4, pvpTestContext.GetCurrentPlayer().CardsInHand.Count);
                        Assert.AreEqual(5, pvpTestContext.GetOpponentPlayer().CardsInHand.Count);
                    },
                    player =>
                    {
                        Assert.AreEqual(5, pvpTestContext.GetCurrentPlayer().CardsInHand.Count);
                        Assert.AreEqual(5, pvpTestContext.GetOpponentPlayer().CardsInHand.Count);
                    },
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, null);
            });
        }

        //Toxic Faction Tests
        [UnityTest]
        [Timeout(int.MaxValue)]
        [Ignore("seems broken")]
        public IEnumerator Zpitter_v4()
        {
            return AsyncTest(async () =>
            {
                Deck opponentDeck = new Deck(
                    0,
                    0,
                    "test deck",
                    new List<DeckCardData>
                    {
                        new DeckCardData("Zpitter", 2),
                        new DeckCardData("Burn", 2)
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
                        new DeckCardData("Zpitter", 2),
                        new DeckCardData("Burn", 2)
                    },
                    Enumerators.OverlordSkill.NONE,
                    Enumerators.OverlordSkill.NONE
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerBurnId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Burn", 1);
                InstanceId opponentBurnId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Burn", 1);
                InstanceId playerZpitterId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zpitter", 1);
                InstanceId opponentZpitterId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zpitter", 1);
                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                   {
                       player => {},
                       opponent => {},
                       player => {},
                       opponent => {},
                       player => player.CardPlay(playerZpitterId, ItemPosition.Start),
                       opponent => opponent.CardPlay(opponentZpitterId, ItemPosition.Start),
                   };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(pvpTestContext.GetCurrentPlayer().InitialDefense - 1, pvpTestContext.GetCurrentPlayer().BuffedDefense);
                    Assert.AreEqual(pvpTestContext.GetOpponentPlayer().InitialDefense - 1, pvpTestContext.GetOpponentPlayer().BuffedDefense);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        [Ignore("seems broken")]
        public IEnumerator Ghoul_v4()
        {
            return AsyncTest(async () =>
            {
                Deck opponentDeck = new Deck(
                    0,
                    0,
                    "test deck",
                    new List<DeckCardData>
                    {
                        new DeckCardData("Ghoul", 2),
                        new DeckCardData("Burn", 10)
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
                        new DeckCardData("Ghoul", 2),
                        new DeckCardData("Burn", 10)
                    },
                    Enumerators.OverlordSkill.NONE,
                    Enumerators.OverlordSkill.NONE
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerBurnId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Burn", 1);
                InstanceId opponentBurnId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Burn", 1);
                InstanceId playerGhoulId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Ghoul", 1);
                InstanceId opponentGhoulId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Ghoul", 1);
                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                   {
                       player => {},
                       opponent => {},
                       player => {},
                       opponent => {},
                       player =>
                       {
                           player.CardPlay(playerGhoulId, ItemPosition.Start);
                           player.CardPlay(playerBurnId, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentGhoulId, ItemPosition.Start);
                           opponent.CardPlay(opponentBurnId, ItemPosition.Start);
                       },
                       player => player.CardAttack(playerGhoulId, opponentBurnId),
                       opponent => opponent.CardAttack(opponentGhoulId, playerBurnId),
                   };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(pvpTestContext.GetCurrentPlayer().InitialDefense - 3, pvpTestContext.GetCurrentPlayer().MaxCurrentDefense);
                    Assert.AreEqual(pvpTestContext.GetOpponentPlayer().InitialDefense - 3, pvpTestContext.GetOpponentPlayer().MaxCurrentDefense);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }
 
        [UnityTest]
        [Timeout(int.MaxValue)]
        [Ignore("seems broken")]
        public IEnumerator Zlopper_v4()
        {
            return AsyncTest(async () =>
            {
                Deck opponentDeck = new Deck(
                    0,
                    0,
                    "test deck",
                    new List<DeckCardData>
                    {
                        new DeckCardData("Zlopper", 2),
                        new DeckCardData("Cerberuz", 10)
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
                        new DeckCardData("Zlopper", 2),
                        new DeckCardData("Cerberuz", 10)
                    },
                    Enumerators.OverlordSkill.NONE,
                    Enumerators.OverlordSkill.NONE
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerCerberuzId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Cerberuz", 1);
                InstanceId opponentCerberuzId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Cerberuz", 1);
                InstanceId playerZlopperId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zlopper", 1);
                InstanceId opponentZlopperId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zlopper", 1);
                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                   {
                       player => {},
                       opponent => {},
                       player => {},
                       opponent => {},
                       player =>
                       {
                           player.CardPlay(playerCerberuzId, ItemPosition.Start);
                           player.CardPlay(playerZlopperId, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentCerberuzId, ItemPosition.Start);
                           opponent.CardPlay(opponentZlopperId, ItemPosition.Start);
                       },
                       player => player.CardAttack(playerZlopperId, opponentCerberuzId),
                       opponent => opponent.CardAttack(opponentZlopperId, playerCerberuzId),
                   };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(1, TestHelper.GameplayManager.CurrentPlayer.CardsOnBoard.Count);
                    Assert.AreEqual(1, TestHelper.GameplayManager.OpponentPlayer.CardsOnBoard.Count);
                    Assert.NotNull(TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerZlopperId));
                    Assert.NotNull(TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentZlopperId));
                    Assert.AreEqual(5, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerZlopperId)).CurrentDefense);
                    Assert.AreEqual(5, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentZlopperId)).CurrentDefense);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }

        //Item Faction Tests
        [UnityTest]
        [Timeout(int.MaxValue)]
        [Ignore("seems broken")]
        public IEnumerator Boomstick_v4()
        {
            return AsyncTest(async () =>
            {
                Deck opponentDeck = new Deck(
                    0,
                    0,
                    "test deck",
                    new List<DeckCardData>
                    {
                        new DeckCardData("Boomstick", 2),
                        new DeckCardData("Cerberuz", 2),
                        new DeckCardData("Burn", 4)
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
                        new DeckCardData("Boomstick", 2),
                        new DeckCardData("Cerberuz", 2),
                        new DeckCardData("Burn", 4)
                    },
                    Enumerators.OverlordSkill.NONE,
                    Enumerators.OverlordSkill.NONE
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerCerberuzId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Cerberuz", 1);
                InstanceId opponentCerberuzId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Cerberuz", 1);
                InstanceId playerBoomstickId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Boomstick", 1);
                InstanceId opponentBoomstickId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Boomstick", 1);
                InstanceId playerBurnId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Burn", 1);
                InstanceId opponentBurnId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Burn", 1);
                
                Log.Info("playerCerberuzId = " + playerCerberuzId);
                Log.Info("opponentCerberuzId = " + opponentCerberuzId);
                Log.Info("playerBurnId = " + playerBurnId);
                Log.Info("opponentBurnId = " + opponentBurnId);
                Log.Info("playerBoomstickId = " + playerBoomstickId);
                Log.Info("opponentBoomstickId = " + opponentBoomstickId);
                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                   {
                       player => {},
                       opponent => {},
                       player => {},
                       opponent => {},
                       player =>
                       {
                           player.CardPlay(playerCerberuzId, ItemPosition.Start);
                           player.CardPlay(playerBurnId, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentCerberuzId, ItemPosition.Start);
                           opponent.CardPlay(opponentBurnId, ItemPosition.Start);
                       },
                       player => player.CardPlay(playerBoomstickId, ItemPosition.Start),
                       opponent => opponent.CardPlay(opponentBoomstickId, ItemPosition.Start),
                   };
                Action validateEndState = () =>
                {
                    Assert.AreEqual(1, TestHelper.GameplayManager.CurrentPlayer.CardsOnBoard.Count);
                    Assert.AreEqual(1, TestHelper.GameplayManager.OpponentPlayer.CardsOnBoard.Count);
                    Assert.NotNull(TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCerberuzId));
                    Assert.NotNull(TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCerberuzId));
                    Assert.AreEqual(4, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCerberuzId)).CurrentDefense);
                    Assert.AreEqual(4, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCerberuzId)).CurrentDefense);
                    Assert.AreEqual(pvpTestContext.GetCurrentPlayer().InitialDefense, pvpTestContext.GetCurrentPlayer().MaxCurrentDefense);
                    Assert.AreEqual(pvpTestContext.GetOpponentPlayer().InitialDefense, pvpTestContext.GetOpponentPlayer().MaxCurrentDefense);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }

        #region specific situations tests

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Task306_GameCrashedQuaziCameOnBoard()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 1,
                                                                  new DeckCardData("Pyromaz", 2),
                                                                  new DeckCardData("Quazi", 5));
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 1,
                                                                    new DeckCardData("Pyromaz", 2),
                                                                    new DeckCardData("Quazi", 5));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerPyromazId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Pyromaz", 1);
                InstanceId playerPyromaz1Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Pyromaz", 2);
                InstanceId playerQuaziId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Quazi", 1);
                InstanceId playerQuazi1Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Quazi", 2);

                InstanceId opponentPyromazId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Pyromaz", 1);
                InstanceId opponentPyromaz1Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Pyromaz", 2);
                InstanceId opponentQuaziId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Quazi", 1);
                InstanceId opponentQuazi1Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Quazi", 2);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                       player => player.CardPlay(playerPyromazId, ItemPosition.Start),
                       opponent => opponent.CardPlay(opponentPyromazId, ItemPosition.Start),
                       player => player.CardPlay(playerPyromaz1Id, ItemPosition.Start),
                       opponent => opponent.CardPlay(opponentPyromaz1Id, ItemPosition.Start),
                       player =>
                       {
                            player.CardPlay(playerQuaziId, ItemPosition.Start);
                            player.CardPlay(playerQuazi1Id, ItemPosition.Start);
                       },
                       opponent => {},
                       player => {},
                       opponent =>
                       {
                            opponent.CardPlay(opponentQuaziId, ItemPosition.Start);
                            opponent.CardPlay(opponentQuazi1Id, ItemPosition.Start);
                       },
                       player => player.CardAttack(playerPyromaz1Id, opponentPyromazId),
                       opponent => opponent.CardAttack(opponentQuaziId, playerQuaziId),
                       player => {}
                };

                Action validateEndState = () =>
                {
                    Assert.NotNull(TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentQuazi1Id));
                    Assert.NotNull(TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerQuazi1Id));
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Task328_WaitTurnActionsStuck()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 1, new DeckCardData("DuZt", 2));
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 1, new DeckCardData("DuZt", 2));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerDuZtId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "DuZt", 1);
                InstanceId playerDuZt1Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "DuZt", 2);

                InstanceId opponentDuZtId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "DuZt", 1);
                InstanceId opponentDuZt1Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "DuZt", 2);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                       player => player.CardPlay(playerDuZtId, ItemPosition.Start),
                       opponent => opponent.CardPlay(opponentDuZtId, ItemPosition.Start),
                       player => player.CardPlay(playerDuZt1Id, ItemPosition.Start),
                       opponent => opponent.CardPlay(opponentDuZt1Id, ItemPosition.Start),
                       player => player.CardAttack(playerDuZtId, opponentDuZt1Id),
                       opponent => opponent.CardAttack(opponentDuZtId, playerDuZt1Id),
                       player => {}
                };

                Action validateEndState = () =>
                {
                    Assert.Null(TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerDuZtId));
                    Assert.Null(TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerDuZt1Id));
                    Assert.Null(TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentDuZtId));
                    Assert.Null(TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentDuZt1Id));
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Task308_ZonicAttackedPlayerInsteadCard()
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
                    Assert.AreEqual(playerUnit.Card.Prototype.Defense, playerUnit.CurrentDefense);
                    Assert.AreEqual(opponentUnit.Card.Prototype.Defense, opponentUnit.CurrentDefense);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        [Ignore("seems broken")]
        public IEnumerator Task327_GameCrashedWhenCardDidntPlayedOnSecondClient()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0,
                                                                  new DeckCardData("Firecaller", 2),
                                                                  new DeckCardData("Zlinger", 6)

                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                                                                    new DeckCardData("Firecaller", 2),
                                                                    new DeckCardData("Zlinger", 6)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerFirecallerId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Firecaller", 1);
                InstanceId playerZlingerId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zlinger", 1);

                InstanceId opponentFirecallerId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Firecaller", 1);
                InstanceId opponentFirecaller1Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Firecaller", 2);
                InstanceId opponentZlingerId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zlinger", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                       player => {},
                       opponent => opponent.CardPlay(opponentFirecallerId, ItemPosition.Start),
                       player =>
                       {
                            player.CardPlay(playerFirecallerId, ItemPosition.Start);
                            player.CardPlay(playerZlingerId, ItemPosition.Start);

                            player.RankBuff(playerZlingerId, new List<InstanceId>()
                            {
                                playerFirecallerId
                            });

                            player.CardAttack(playerFirecallerId, opponentFirecallerId);
                       },
                       opponent =>
                       {
                            opponent.CardPlay(opponentFirecaller1Id, ItemPosition.Start);
                            opponent.CardPlay(opponentZlingerId, ItemPosition.Start);
                            opponent.RankBuff(opponentZlingerId, new List<InstanceId>()
                            {
                                opponentFirecaller1Id
                            });

                            opponent.CardAttack(opponentFirecaller1Id, playerZlingerId);
                       },
                       player => { },
                       opponent => { }
                };

                Action validateEndState = () =>
                {
                    Assert.Null(TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerFirecallerId));
                    Assert.Null(TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentFirecaller1Id));
                    Assert.Null(TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentFirecallerId));
                    Assert.Null(TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerZlingerId));
                    Assert.AreEqual(1, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentZlingerId)).CurrentDefense);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }

        #endregion
    }
}
