using System;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
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
        [UnityTest]
        [Timeout(150 * 1000 * TestHelper.TestTimeScale)]
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
        [Timeout(150 * 1000 * TestHelper.TestTimeScale)]
        public IEnumerator MindFlayer()
        {
            return AsyncTest(async () =>
            {
                Deck opponentDeck = new Deck(
                    0,
                    0,
                    "test deck",
                    new List<DeckCardData>
                    {
                        new DeckCardData("Mind Flayer", 2),
                        new DeckCardData("Whizpar", 2)
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
                        new DeckCardData("Mind Flayer", 2),
                        new DeckCardData("Whizpar", 2)
                    },
                    Enumerators.OverlordSkill.NONE,
                    Enumerators.OverlordSkill.NONE
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerWhizparId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Whizpar", 1);
                InstanceId opponentWhizparId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Whizpar", 1);
                InstanceId playerMindFlayerId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Mind Flayer", 1);
                InstanceId opponentMindFlayerId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Mind Flayer", 1);
                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                   {
                       player => {},
                       opponent => {},
                       player => player.CardPlay(playerWhizparId, ItemPosition.Start),
                       opponent =>
                       {
                           opponent.CardPlay(opponentWhizparId, ItemPosition.Start);
                       },
                       player =>
                       {
                           player.CardAttack(playerWhizparId, opponentWhizparId);
                           player.CardPlay(playerMindFlayerId, ItemPosition.Start);
                       },
                   };

                Action validateEndState = () =>
                {
                    List<AbilityBase> abilities = TestHelper.AbilitiesController.GetAbilitiesConnectedToUnit((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerMindFlayerId));
                    Assert.AreEqual(1, abilities.Count);
                    Assert.AreEqual(0, (abilities[0] as TakeControlEnemyUnitAbility).MovedUnits.Count);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }

        [UnityTest]
        [Timeout(150 * 1000 * TestHelper.TestTimeScale)]
        public IEnumerator Maelstrom()
        {
            return AsyncTest(async () =>
            {
                Deck opponentDeck = new Deck(
                    0,
                    0,
                    "test deck",
                    new List<DeckCardData>
                    {
                        new DeckCardData("Maelstrom", 2),
                        new DeckCardData("Whizpar", 2)
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
                        new DeckCardData("Maelstrom", 2),
                        new DeckCardData("Whizpar", 2)
                    },
                    Enumerators.OverlordSkill.NONE,
                    Enumerators.OverlordSkill.NONE
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerWhizparId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Whizpar", 1);
                InstanceId opponentWhizparId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Whizpar", 1);
                InstanceId playerMaelstromId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Maelstrom", 1);
                InstanceId opponentMaelstromrId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Maelstrom", 1);
                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                   {
                       player => {},
                       opponent => {},
                       player => player.CardPlay(playerWhizparId, ItemPosition.Start),
                       opponent =>
                       {
                           opponent.CardPlay(opponentWhizparId, ItemPosition.Start);
                       },
                       player =>
                       {
                           player.CardAttack(playerWhizparId, opponentWhizparId);
                           player.CardPlay(playerMaelstromId, ItemPosition.Start);
                       },
                   };

                Action validateEndState = () =>
                {
                    List<AbilityBase> abilities = TestHelper.AbilitiesController.GetAbilitiesConnectedToUnit((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerMaelstromId));
                    Assert.AreEqual(1, abilities.Count);
                    Assert.AreEqual(0, (abilities[0] as ReturnUnitsOnBoardToOwnersHandsAbility).Units.Count);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }

        [UnityTest]
        [Timeout(150 * 1000 * TestHelper.TestTimeScale)]
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
        [Timeout(150 * 1000 * TestHelper.TestTimeScale)]
        public IEnumerator Ectoplasm()
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
                        new DeckCardData("Ectoplasm", 30)
                    },
                    Enumerators.OverlordSkill.NONE,
                    Enumerators.OverlordSkill.NONE
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck) {
                    Player1HasFirstTurn = true
                };

                InstanceId playerEctoplasmId = pvpTestContext.GetCardInstanceIdByIndex(playerDeck, 0);
                InstanceId opponentSlabId = pvpTestContext.GetCardInstanceIdByIndex(opponentDeck, 0);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                   {
                       player => {},
                       opponent => {},

                       player => player.CardPlay(playerEctoplasmId, ItemPosition.Start),
                       opponent => {},
                   };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(3, pvpTestContext.GetCurrentPlayer().GooVials);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }


        [UnityTest]
        [Timeout(150 * 1000 * TestHelper.TestTimeScale)]
        public IEnumerator Poizom()
        {
            return AsyncTest(async () =>
            {
                Deck opponentDeck = new Deck(
                    0,
                    0,
                    "test deck",
                    new List<DeckCardData>
                    {
                        new DeckCardData("Wood", 30)
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
                        new DeckCardData("Poizom", 30)
                    },
                    Enumerators.OverlordSkill.NONE,
                    Enumerators.OverlordSkill.NONE
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck) {
                    Player1HasFirstTurn = true
                };

                InstanceId playerPoizomId = pvpTestContext.GetCardInstanceIdByIndex(playerDeck, 0);
                InstanceId opponentWoodId = pvpTestContext.GetCardInstanceIdByIndex(opponentDeck, 0);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                   {
                       player => {},
                       opponent => {},
                       player => {},
                       opponent => {},

                       player => player.CardPlay(playerPoizomId, ItemPosition.Start),
                       opponent => opponent.CardPlay(opponentWoodId, ItemPosition.Start),

                       player => player.CardAttack(playerPoizomId, opponentWoodId),
                       opponent => {}
                   };

                Action validateEndState = () =>
                    {
                        Assert.AreEqual(1,
                            ((BoardUnitModel) TestHelper.BattlegroundController.GetBoardObjectByInstanceId(
                                opponentWoodId)).CurrentHp);
                    };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }


        [UnityTest]
        [Timeout(150 * 1000 * TestHelper.TestTimeScale)]
        public IEnumerator Zlimey()
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
                        new DeckCardData("Zlimey", 30)
                    },
                    Enumerators.OverlordSkill.NONE,
                    Enumerators.OverlordSkill.NONE
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck) {
                    Player1HasFirstTurn = true
                };

                InstanceId playerZlimeyId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zlimey", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                   {
                       player => player.CardPlay(playerZlimeyId, ItemPosition.Start),
                       opponent => {}
                   };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(18, pvpTestContext.GetCurrentPlayer().Defense);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }

        [UnityTest]
        [Timeout(150 * 1000 * TestHelper.TestTimeScale)]
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
        [Timeout(150 * 1000 * TestHelper.TestTimeScale)]
        public IEnumerator Zeptic()
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

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                   {
                       player => player.CardPlay(playerZepticId, ItemPosition.Start),
                       opponent => {}
                   };

                Action validateEndState = () =>
                {
                    Assert.NotNull(TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerZepticId));
                    Assert.AreEqual(1, ((BoardUnitModel) TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerZepticId)).CurrentHp);
                    Assert.AreEqual(18, pvpTestContext.GetCurrentPlayer().Defense);
                    Assert.AreEqual(3, pvpTestContext.GetCurrentPlayer().CurrentGoo);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }


        [UnityTest]
        [Timeout(150 * 1000 * TestHelper.TestTimeScale)]
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
        [Timeout(150 * 1000 * TestHelper.TestTimeScale)]
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
                    Assert.AreEqual(pvpTestContext.GetCurrentPlayer().InitialHp - 1, pvpTestContext.GetCurrentPlayer().BuffedHp);
                    Assert.AreEqual(pvpTestContext.GetOpponentPlayer().InitialHp - 1, pvpTestContext.GetOpponentPlayer().BuffedHp);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }

        [UnityTest]
        [Timeout(150 * 1000 * TestHelper.TestTimeScale)]
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
                    Assert.AreEqual(pvpTestContext.GetCurrentPlayer().InitialHp - 3, pvpTestContext.GetCurrentPlayer().MaxCurrentHp);
                    Assert.AreEqual(pvpTestContext.GetOpponentPlayer().InitialHp - 3, pvpTestContext.GetOpponentPlayer().MaxCurrentHp);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }


        [UnityTest]
        [Timeout(150 * 1000 * TestHelper.TestTimeScale)]
        public IEnumerator Wazte_v4()
        {
            return AsyncTest(async () =>
            {
                Deck opponentDeck = new Deck(
                    0,
                    0,
                    "test deck",
                    new List<DeckCardData>
                    {
                        new DeckCardData("Wazte", 2),
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
                        new DeckCardData("Wazte", 2),
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
                InstanceId playerWazteId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Wazte", 1);
                InstanceId opponentWazteId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Wazte", 1);
                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                   {
                       player => {},
                       opponent => {},
                       player => player.CardPlay(playerWazteId, ItemPosition.Start),
                       opponent => opponent.CardPlay(opponentWazteId, ItemPosition.Start),
                   };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(3, pvpTestContext.GetCurrentPlayer().GooVials);
                    Assert.AreEqual(2, pvpTestContext.GetOpponentPlayer().GooVials);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }

        [UnityTest]
        [Timeout(150 * 1000 * TestHelper.TestTimeScale)]
        public IEnumerator Azzazzin_v4()
        {
            return AsyncTest(async () =>
            {
                Deck opponentDeck = new Deck(
                    0,
                    0,
                    "test deck",
                    new List<DeckCardData>
                    {
                        new DeckCardData("Azzazzin", 2),
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
                        new DeckCardData("Azzazzin", 2),
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
                InstanceId playerAzzazzinId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Azzazzin", 1);
                InstanceId opponentAzzazzinId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Azzazzin", 1);
                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                   {
                       player => {},
                       opponent => {},
                       player => {},
                       opponent => {},
                       player => player.CardPlay(playerCerberuzId, ItemPosition.Start),
                       opponent => opponent.CardPlay(opponentCerberuzId, ItemPosition.Start),
                       player =>
                       {
                           player.CardPlay(playerAzzazzinId, ItemPosition.Start);
                           player.CardAttack(playerAzzazzinId, opponentCerberuzId);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentAzzazzinId, ItemPosition.Start);
                           opponent.CardAttack(opponentAzzazzinId, playerCerberuzId);
                       },
                   };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(0, TestHelper.BattlegroundController.PlayerBoardCards.Count);
                    Assert.AreEqual(0, TestHelper.BattlegroundController.OpponentBoardCards.Count);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }

        [UnityTest]
        [Timeout(150 * 1000 * TestHelper.TestTimeScale)]
        public IEnumerator Hazzard_v4()
        {
            return AsyncTest(async () =>
            {
                Deck opponentDeck = new Deck(
                    0,
                    0,
                    "test deck",
                    new List<DeckCardData>
                    {
                        new DeckCardData("Hazzard", 2),
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
                        new DeckCardData("Hazzard", 2),
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
                InstanceId playerHazzardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Hazzard", 1);
                InstanceId opponentHazzardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Hazzard", 1);
                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                   {
                       player => {},
                       opponent => {},
                       player => player.CardPlay(playerHazzardId, ItemPosition.Start),
                       opponent => opponent.CardPlay(opponentHazzardId, ItemPosition.Start),
                   };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(2, pvpTestContext.GetCurrentPlayer().GooVials);
                    Assert.AreEqual(1, pvpTestContext.GetOpponentPlayer().GooVials);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }

        [UnityTest]
        [Timeout(150 * 1000 * TestHelper.TestTimeScale)]
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
                    Assert.AreEqual(1, TestHelper.BattlegroundController.PlayerBoardCards.Count);
                    Assert.AreEqual(1, TestHelper.BattlegroundController.OpponentBoardCards.Count);
                    Assert.NotNull(TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerZlopperId));
                    Assert.NotNull(TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentZlopperId));
                    Assert.AreEqual(5, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerZlopperId)).CurrentHp);
                    Assert.AreEqual(5, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentZlopperId)).CurrentHp);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }

        //Item Faction Tests
        [UnityTest]
        [Timeout(150 * 1000 * TestHelper.TestTimeScale)]
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
                
                Debug.Log("playerCerberuzId = " + playerCerberuzId);
                Debug.Log("opponentCerberuzId = " + opponentCerberuzId);
                Debug.Log("playerBurnId = " + playerBurnId);
                Debug.Log("opponentBurnId = " + opponentBurnId);
                Debug.Log("playerBoomstickId = " + playerBoomstickId);
                Debug.Log("opponentBoomstickId = " + opponentBoomstickId);
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
                    Assert.AreEqual(1, TestHelper.BattlegroundController.PlayerBoardCards.Count);
                    Assert.AreEqual(1, TestHelper.BattlegroundController.OpponentBoardCards.Count);
                    Assert.NotNull(TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCerberuzId));
                    Assert.NotNull(TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCerberuzId));
                    Assert.AreEqual(4, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCerberuzId)).CurrentHp);
                    Assert.AreEqual(4, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCerberuzId)).CurrentHp);
                    Assert.AreEqual(pvpTestContext.GetCurrentPlayer().InitialHp, pvpTestContext.GetCurrentPlayer().MaxCurrentHp);
                    Assert.AreEqual(pvpTestContext.GetOpponentPlayer().InitialHp, pvpTestContext.GetOpponentPlayer().MaxCurrentHp);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }
    }
}
