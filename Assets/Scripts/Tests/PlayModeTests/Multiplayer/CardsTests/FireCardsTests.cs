using System;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using UnityEngine;
using UnityEngine.TestTools;
using System.Linq;

namespace Loom.ZombieBattleground.Test.MultiplayerTests
{
    public class FireCardsTests : BaseIntegrationTest
    {
        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Zlinger()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 1,
                    new DeckCardData("Zlinger", 1),
                    new DeckCardData("Hot", 20));
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 1,
                    new DeckCardData("Zlinger", 1),
                    new DeckCardData("Hot", 20));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zlinger", 1);
                InstanceId playerHotId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Hot", 1);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zlinger", 1);
                InstanceId opponentHotId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Hot", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                    player =>
                    {
                        player.CardPlay(playerHotId, ItemPosition.Start);
                    },
                    opponent =>
                    {
                        opponent.CardPlay(opponentHotId, ItemPosition.Start);
                        opponent.CardPlay(opponentCardId, ItemPosition.Start, playerHotId);
                    },
                    player =>
                    {
                        player.CardPlay(playerCardId, ItemPosition.Start, opponentHotId);
                    },
                };

                int value = 1;

                Action validateEndState = () =>
                {
                    CardModel playerHotUnit = (CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerHotId);
                    CardModel opponentHotUnit = (CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentHotId);

                    Assert.AreEqual(playerHotUnit.MaxCurrentDefense - value, playerHotUnit.CurrentDefense);
                    Assert.AreEqual(opponentHotUnit.MaxCurrentDefense - value, opponentHotUnit.CurrentDefense);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Quazi()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 1,
                    new DeckCardData("Quazi", 1),
                    new DeckCardData("Pyromaz", 3));
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 1,
                    new DeckCardData("Quazi", 1),
                    new DeckCardData("Pyromaz", 3));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerQuaziId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Quazi", 1);
                InstanceId playerPyromaz1Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Pyromaz", 1);
                InstanceId playerPyromaz2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Pyromaz", 2);
                InstanceId playerPyromaz3Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Pyromaz", 3);
                InstanceId opponentQuaziId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Quazi", 1);
                InstanceId opponentPyromaz1Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Pyromaz", 1);
                InstanceId opponentPyromaz2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Pyromaz", 2);
                InstanceId opponentPyromaz3Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Pyromaz", 3);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                    player =>
                    {
                        player.CardPlay(playerPyromaz1Id, ItemPosition.Start);
                        player.CardPlay(playerPyromaz2Id, ItemPosition.Start);
                        player.CardPlay(playerPyromaz3Id, ItemPosition.Start);
                    },
                    opponent =>
                    {
                        opponent.CardPlay(opponentPyromaz1Id, ItemPosition.Start);
                        opponent.CardPlay(opponentPyromaz2Id, ItemPosition.Start);
                        opponent.CardPlay(opponentPyromaz3Id, ItemPosition.Start);
                    },
                    player =>
                    {
                        player.CardAttack(playerPyromaz1Id, pvpTestContext.GetOpponentPlayer().InstanceId);
                        player.CardAttack(playerPyromaz2Id, pvpTestContext.GetOpponentPlayer().InstanceId);
                        player.CardAttack(playerPyromaz3Id, pvpTestContext.GetOpponentPlayer().InstanceId);
                    },
                    opponent =>
                    {
                        opponent.CardAttack(opponentPyromaz1Id, pvpTestContext.GetCurrentPlayer().InstanceId);
                        opponent.CardAttack(opponentPyromaz2Id, pvpTestContext.GetCurrentPlayer().InstanceId);
                        opponent.CardAttack(opponentPyromaz3Id, pvpTestContext.GetCurrentPlayer().InstanceId);
                    },
                    player =>
                    {
                        player.CardAttack(playerPyromaz1Id, pvpTestContext.GetOpponentPlayer().InstanceId);
                        player.CardAttack(playerPyromaz2Id, pvpTestContext.GetOpponentPlayer().InstanceId);
                        player.CardAttack(playerPyromaz3Id, pvpTestContext.GetOpponentPlayer().InstanceId);
                    },
                    opponent =>
                    {
                        opponent.CardAttack(opponentPyromaz1Id, pvpTestContext.GetCurrentPlayer().InstanceId);
                        opponent.CardAttack(opponentPyromaz2Id, pvpTestContext.GetCurrentPlayer().InstanceId);
                        opponent.CardAttack(opponentPyromaz3Id, pvpTestContext.GetCurrentPlayer().InstanceId);
                    },
                    player =>
                    {
                        player.CardAttack(playerPyromaz1Id, pvpTestContext.GetOpponentPlayer().InstanceId);
                        player.CardAttack(playerPyromaz2Id, pvpTestContext.GetOpponentPlayer().InstanceId);
                        player.CardAttack(playerPyromaz3Id, pvpTestContext.GetOpponentPlayer().InstanceId);
                    },
                    opponent =>
                    {
                        opponent.CardAttack(opponentPyromaz1Id, pvpTestContext.GetCurrentPlayer().InstanceId);
                        opponent.CardAttack(opponentPyromaz2Id, pvpTestContext.GetCurrentPlayer().InstanceId);
                        opponent.CardAttack(opponentPyromaz3Id, pvpTestContext.GetCurrentPlayer().InstanceId);
                    },
                    player => player.CardPlay(playerQuaziId, ItemPosition.Start),
                    opponent => opponent.CardPlay(opponentQuaziId, ItemPosition.Start)
                };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(7, pvpTestContext.GetCurrentPlayer().GooVials);
                    Assert.AreEqual(6, pvpTestContext.GetOpponentPlayer().GooVials);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            }, 400);
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Ember()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 1, new DeckCardData("Ember", 10));
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 1, new DeckCardData("Ember", 10));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

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
                        player.CardAttack(playerCardId, pvpTestContext.GetOpponentPlayer().InstanceId);
                    },
                    opponent =>
                    {
                        opponent.CardPlay(opponentCardId, ItemPosition.Start);
                        opponent.CardAttack(opponentCardId, pvpTestContext.GetCurrentPlayer().InstanceId);
                    },
                    player => {},
                    opponent => {}
                };

                int value = 3;

                Action validateEndState = () =>
                {
                    Assert.AreEqual(pvpTestContext.GetCurrentPlayer().InitialDefense - value, pvpTestContext.GetCurrentPlayer().Defense);
                    Assert.AreEqual(pvpTestContext.GetOpponentPlayer().InitialDefense - value, pvpTestContext.GetOpponentPlayer().Defense);

                    Assert.IsTrue(((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCardId)).HasFeral);
                    Assert.IsTrue(((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCardId)).HasFeral);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            }, 400);
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Firewall()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 1,
                    new DeckCardData("Firewall", 1),
                    new DeckCardData("Zlab", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 1,
                    new DeckCardData("Firewall", 1),
                    new DeckCardData("Zlab", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerFirewallId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Firewall", 1);
                InstanceId playerZlab1Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zlab", 1);
                InstanceId playerZlab2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zlab", 2);
                InstanceId opponentFirewallId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Firewall", 1);
                InstanceId opponentZlab1Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zlab", 1);
                InstanceId opponentZlab2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zlab", 2);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                    player => {},
                    opponent => {},
                    player => {
                        player.CardPlay(playerZlab1Id, ItemPosition.Start);
                        player.CardPlay(playerZlab2Id, ItemPosition.Start);
                        player.CardPlay(playerFirewallId, ItemPosition.Start);
                    },
                    opponent => {
                        opponent.CardPlay(opponentZlab1Id, ItemPosition.Start);
                        opponent.CardPlay(opponentZlab2Id, ItemPosition.Start);
                        opponent.CardPlay(opponentFirewallId, ItemPosition.Start);
                    },
                    player => {}
                };
                Action validateEndState = () =>
                {
                    CardModel playerZlab1Unit = ((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerZlab1Id));
                    CardModel playerZlab2Unit = ((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerZlab2Id));
                    CardModel opponentZlab1Unit = ((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentZlab1Id));
                    CardModel opponentZlab2Unit = ((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentZlab2Id));

                    Assert.AreEqual(playerZlab1Unit.Card.Prototype.Damage, playerZlab1Unit.CurrentDamage);
                    Assert.AreEqual(playerZlab2Unit.Card.Prototype.Damage + 2, playerZlab2Unit.CurrentDamage);

                    Assert.AreEqual(opponentZlab1Unit.Card.Prototype.Damage, opponentZlab1Unit.CurrentDamage);
                    Assert.AreEqual(opponentZlab2Unit.Card.Prototype.Damage + 2, opponentZlab2Unit.CurrentDamage);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            }, 400);
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Burzt()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 1, new DeckCardData("Burzt", 10));
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 1, new DeckCardData("Burzt", 10));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Burzt", 1);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Burzt", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                    player => {},
                    opponent => {},
                    player => {},
                    opponent => {
                        opponent.CardPlay(opponentCardId, ItemPosition.Start);
                        opponent.CardAbilityUsed(opponentCardId, Enumerators.AbilityType.TAKE_DAMAGE_RANDOM_ENEMY, new List<ParametrizedAbilityInstanceId>());
                    },
                    player => {
                        player.CardPlay(playerCardId, ItemPosition.Start);
                        player.CardAbilityUsed(playerCardId, Enumerators.AbilityType.TAKE_DAMAGE_RANDOM_ENEMY, new List<ParametrizedAbilityInstanceId>());
                    },
                    opponent => {},
                };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(1, ((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCardId)).CurrentDefense);
                    Assert.AreEqual(1, ((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCardId)).CurrentDefense);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            }, 400);
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        [Category("PlayQuickSubset2")]
        public IEnumerator BlaZter()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 1, 
                    new DeckCardData("Blazter", 1),
                    new DeckCardData("Zlab", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 1, 
                    new DeckCardData("Blazter", 1),
                    new DeckCardData("Zlab", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerBlaZterId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Blazter", 1);
                InstanceId playerZlab1Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zlab", 1);
                InstanceId playerZlab2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zlab", 2);
                InstanceId opponentBlaZterId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Blazter", 1);
                InstanceId opponentZlab1Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zlab", 1);
                InstanceId opponentZlab2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zlab", 2);

                int value = 2;

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
                        opponent.CardPlay(opponentBlaZterId, ItemPosition.Start, opponentZlab2Id);
                        opponent.CardAttack(opponentZlab2Id, playerZlab2Id);
                    },
                    player =>
                    {
                        player.CardPlay(playerBlaZterId, ItemPosition.Start, playerZlab1Id);
                        player.CardAttack(playerZlab1Id, opponentZlab1Id);
                    },
                };

                Action validateEndState = () =>
                {
                    Assert.IsNull(TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentZlab1Id));
                    Assert.IsNull(TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerZlab2Id));
                    Assert.AreEqual(value, ((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentZlab2Id)).BuffedDamage);
                    Assert.AreEqual(value, ((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerZlab1Id)).BuffedDamage);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            }, 400);
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Firecaller()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 1, 
                    new DeckCardData("Firecaller", 1),
                    new DeckCardData("Trunk", 20)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 1, 
                    new DeckCardData("Firecaller", 1),
                    new DeckCardData("Trunk", 20)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerFirecallerId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Firecaller", 1);
                InstanceId playerTrunkId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Trunk", 1);
                InstanceId opponentFirecallerId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Firecaller", 1);
                InstanceId opponentTrunkId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Trunk", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                    player => {},
                    opponent => {},
                    player => {
                        player.CardPlay(playerTrunkId, ItemPosition.Start);
                        player.CardPlay(playerFirecallerId, ItemPosition.Start);
                        player.CardAbilityUsed(playerFirecallerId, Enumerators.AbilityType.DRAW_CARD, new List<ParametrizedAbilityInstanceId>());
                    },
                    opponent =>
                    {
                        opponent.CardPlay(opponentTrunkId, ItemPosition.Start);
                        opponent.CardPlay(opponentFirecallerId, ItemPosition.Start);
                        opponent.CardAbilityUsed(playerFirecallerId, Enumerators.AbilityType.DRAW_CARD, new List<ParametrizedAbilityInstanceId>());
                    },
                    player => {
                        player.CardAttack(playerFirecallerId, opponentTrunkId);
                    },
                    opponent => {
                        opponent.CardAttack(opponentFirecallerId, playerTrunkId);
                    },
                    player => {},
                    opponent => {}
                };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(7, pvpTestContext.GetCurrentPlayer().CardsInHand.Count);
                    Assert.AreEqual(7, pvpTestContext.GetOpponentPlayer().CardsInHand.Count);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            }, 400);
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Rabiez()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 1,
                    new DeckCardData("Rabiez", 1),
                    new DeckCardData("Igloo", 20)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 1,
                    new DeckCardData("Rabiez", 1),
                    new DeckCardData("Igloo", 20)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerRabieZId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Rabiez", 1);
                InstanceId playerIglooId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Igloo", 1);
                InstanceId opponentRabieZId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Rabiez", 1);
                InstanceId opponentIglooId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Igloo", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                       player =>
                       {
                           player.CardPlay(playerIglooId, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentIglooId, ItemPosition.Start);
                           opponent.CardPlay(opponentRabieZId, ItemPosition.Start);
                       },
                       player =>
                       {
                           player.CardPlay(playerRabieZId, ItemPosition.Start);
                       },
                       opponent => {},
                       player => {}
                };

                Action validateEndState = () =>
                {
                    CardModel playerRabiezUnit = ((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerRabieZId));
                    CardModel playerIglooUnit = ((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerIglooId));
                    CardModel opponentRabiezUnit = ((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentRabieZId));
                    CardModel opponentIglooUnit = ((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentIglooId));
                    Assert.AreEqual(playerRabiezUnit.Card.Prototype.Defense, playerRabiezUnit.CurrentDefense);
                    Assert.AreEqual(playerIglooUnit.Card.Prototype.Defense - 2, playerIglooUnit.CurrentDefense);
                    Assert.AreEqual(opponentRabiezUnit.Card.Prototype.Defense - 1, opponentRabiezUnit.CurrentDefense);
                    Assert.AreEqual(opponentIglooUnit.Card.Prototype.Defense - 2, opponentIglooUnit.CurrentDefense);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            }, 400);
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        [Category("PlayQuickSubset2")]
        public IEnumerator Flare()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 1, new DeckCardData("Flare", 10));
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 1, new DeckCardData("Flare", 10));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Flare", 1);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Flare", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                    player => { },
                    opponent => { },
                    player => player.CardPlay(playerCardId, ItemPosition.Start),
                    opponent => opponent.CardPlay(opponentCardId, ItemPosition.Start),
                };

                Action validateEndState = () =>
                {
                    Assert.IsTrue(((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCardId)).HasFeral);
                    Assert.IsTrue(((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCardId)).HasFeral);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            }, 400);
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
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
                       new DeckCardData("Zlab", 2)
                    },
                    Enumerators.Skill.NONE,
                    Enumerators.Skill.NONE
                );

                Deck playerDeck = new Deck(
                    0,
                    0,
                    "test deck2",
                    new List<DeckCardData>
                    {
                       new DeckCardData("Cynderman", 2),
                       new DeckCardData("Zlab", 2)
                    },
                    Enumerators.Skill.NONE,
                    Enumerators.Skill.NONE
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerZlabId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zlab", 1);
                InstanceId opponentZlabId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zlab", 1);
                InstanceId playerCyndermanId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Cynderman", 1);
                InstanceId opponentCyndermanId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Cynderman", 1);
                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                   {
                      player => {},
                      opponent => {},
                      player => {},
                      opponent => {},
                      player => player.CardPlay(playerZlabId, ItemPosition.Start),
                      opponent =>
                      {
                          opponent.CardPlay(opponentZlabId, ItemPosition.Start);
                          opponent.CardPlay(opponentCyndermanId, ItemPosition.Start, playerZlabId);
                      },
                      player =>
                      {
                          player.CardPlay(playerCyndermanId, ItemPosition.Start, opponentCyndermanId);
                      },
                      opponent => {},
                      player => {}
                   };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(1, ((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerZlabId)).CurrentDefense);
                    Assert.IsNull((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCyndermanId));
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            }, 400);
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Werezomb()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 1,
                    new DeckCardData("Werezomb", 1),
                    new DeckCardData("Pyromaz", 20)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 1,
                    new DeckCardData("Werezomb", 1),
                    new DeckCardData("Pyromaz", 20)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Werezomb", 1);
                InstanceId playerPyromazId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Pyromaz", 1);
                InstanceId playerPyromaz2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Pyromaz", 2);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Werezomb", 1);
                InstanceId opponentPyromazId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Pyromaz", 1);
                InstanceId opponentPyromaz2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Pyromaz", 2);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                    player => {},
                    opponent => {},
                    player => {},
                    opponent => {},
                    player =>
                    {
                        player.CardPlay(playerPyromazId, ItemPosition.Start);
                        player.CardPlay(playerPyromaz2Id, ItemPosition.Start);
                        player.CardPlay(playerCardId, ItemPosition.Start, playerPyromazId);
                    },
                    opponent =>
                    {
                        opponent.CardPlay(opponentPyromazId, ItemPosition.Start);
                        opponent.CardPlay(opponentPyromaz2Id, ItemPosition.Start);
                        opponent.CardPlay(opponentCardId, ItemPosition.Start, opponentPyromaz2Id);
                    },
                    player => {},
                    opponent => {},
                };

                Action validateEndState = () =>
                {
                    Assert.IsTrue(((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerPyromazId)).HasFeral);
                    Assert.IsTrue(((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentPyromaz2Id)).HasFeral);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            }, 400);
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Modo()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 1, 
                    new DeckCardData("Modo", 1),
                    new DeckCardData("Trunk", 20)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 1,
                    new DeckCardData("Modo", 1),
                    new DeckCardData("Trunk", 20)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerModoId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Modo", 1);
                InstanceId playerTrunkId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Trunk", 1);
                InstanceId opponentModoId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Modo", 1);
                InstanceId opponentTrunkId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Trunk", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                    player => {},
                    opponent => {},
                    player => {
                        player.CardPlay(playerTrunkId, ItemPosition.Start);
                        player.CardPlay(playerModoId, ItemPosition.Start);
                        player.CardAbilityUsed(playerModoId, Enumerators.AbilityType.GET_GOO_THIS_TURN, new List<ParametrizedAbilityInstanceId>());
                    },
                    opponent => {
                        opponent.CardPlay(opponentTrunkId, ItemPosition.Start);
                        opponent.CardPlay(opponentModoId, ItemPosition.Start);
                        opponent.CardAbilityUsed(opponentModoId, Enumerators.AbilityType.GET_GOO_THIS_TURN, new List<ParametrizedAbilityInstanceId>());
                    },
                    player =>
                    {
                        player.LetsThink(2);
                        player.CardAttack(playerModoId, opponentTrunkId);
                        player.LetsThink(5);
                        player.AssertInQueue(() => {
                            Assert.AreEqual(4, pvpTestContext.GetCurrentPlayer().CurrentGoo);
                        });
                    },
                    opponent =>
                    {
                        opponent.LetsThink(2);
                        opponent.CardAttack(opponentModoId, playerTrunkId);
                        opponent.LetsThink(5);
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
        public IEnumerator Rager()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 1,
                    new DeckCardData("Rager", 1),
                    new DeckCardData("Zlab", 20)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 1,
                    new DeckCardData("Rager", 1),
                    new DeckCardData("Zlab", 20)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Rager", 1);
                InstanceId playerZlabId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zlab", 1);
                InstanceId playerZlab2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zlab", 2);
                InstanceId playerZlab3Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zlab", 3);
                InstanceId playerZlab4Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zlab", 4);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Rager", 1);
                InstanceId opponentZlabId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zlab", 1);
                InstanceId opponentZlab2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zlab", 2);
                InstanceId opponentZlab3Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zlab", 3);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                    player => {},
                    opponent => {},
                    player =>
                    {
                        player.CardPlay(playerZlabId, ItemPosition.Start);
                        player.CardPlay(playerZlab2Id, ItemPosition.Start);
                        player.CardPlay(playerZlab3Id, ItemPosition.Start);
                        player.CardPlay(playerZlab4Id, ItemPosition.Start);
                        player.CardPlay(playerCardId, ItemPosition.Start);
                        player.CardAbilityUsed(playerCardId, Enumerators.AbilityType.TAKE_DAMAGE_RANDOM_ENEMY, new List<ParametrizedAbilityInstanceId>());
                    },
                    opponent =>
                    {
                        opponent.CardPlay(opponentZlabId, ItemPosition.Start);
                        opponent.CardPlay(opponentZlab2Id, ItemPosition.Start);
                        opponent.CardPlay(opponentZlab3Id, ItemPosition.Start);
                        opponent.CardPlay(opponentCardId, ItemPosition.Start);
                        opponent.CardAbilityUsed(opponentCardId, Enumerators.AbilityType.TAKE_DAMAGE_RANDOM_ENEMY, new List<ParametrizedAbilityInstanceId>());
                    },
                    player =>
                    { 
                        player.CardAttack(playerCardId, opponentZlabId);
                    },
                    opponent =>
                    {
                        opponent.CardAttack(opponentCardId, playerZlabId);
                    },
                    player => {},
                    opponent => {}
                };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(3, pvpTestContext.GetCurrentPlayer().CardsOnBoard.Count);
                    Assert.AreEqual(2, pvpTestContext.GetOpponentPlayer().CardsOnBoard.Count);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            }, 400);
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator FireMaw()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 1, new DeckCardData("Fire-Maw", 10));
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 1, new DeckCardData("Fire-Maw", 10));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Fire-Maw", 1);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Fire-Maw", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                    player => {},
                    opponent => {},
                    player => {},
                    opponent => {},
                    player => {
                        player.CardPlay(playerCardId, ItemPosition.Start);
                        player.CardAbilityUsed(playerCardId, Enumerators.AbilityType.ATTACK_NUMBER_OF_TIMES_PER_TURN, new List<ParametrizedAbilityInstanceId>());
                    },
                    opponent =>
                    {
                        opponent.CardPlay(opponentCardId, ItemPosition.Start);
                        opponent.CardAbilityUsed(opponentCardId, Enumerators.AbilityType.ATTACK_NUMBER_OF_TIMES_PER_TURN, new List<ParametrizedAbilityInstanceId>());
                    },
                    player =>
                    {
                        player.CardAttack(playerCardId, pvpTestContext.GetOpponentPlayer().InstanceId);
                        player.CardAttack(playerCardId, pvpTestContext.GetOpponentPlayer().InstanceId);
                    },
                    opponent =>
                    {
                        opponent.CardAttack(opponentCardId, pvpTestContext.GetCurrentPlayer().InstanceId);
                        opponent.CardAttack(opponentCardId, pvpTestContext.GetCurrentPlayer().InstanceId);
                    },
                    player => {},
                    opponent => {}
                };

                int value = 8;

                Action validateEndState = () =>
                {
                    Assert.AreEqual(pvpTestContext.GetCurrentPlayer().InitialDefense - value, pvpTestContext.GetCurrentPlayer().Defense);
                    Assert.AreEqual(pvpTestContext.GetOpponentPlayer().InitialDefense - value, pvpTestContext.GetOpponentPlayer().Defense);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            }, 400);
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Alpha()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 1,
                    new DeckCardData("Alpha", 1),
                    new DeckCardData("Hot", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 1,
                    new DeckCardData("Alpha", 1),
                    new DeckCardData("Hot", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Alpha", 1);
                InstanceId playerHotId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Hot", 1);
                InstanceId playerHot2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Hot", 2);
                InstanceId playerHot3Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Hot", 3);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Alpha", 1);
                InstanceId opponentHotId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Hot", 1);
                InstanceId opponentHot2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Hot", 2);

                int value = 2;

                CardModel playerHotUnit = null;
                CardModel playerHot2Unit = null;
                CardModel playerHot3Unit = null;
                CardModel playerAlphaUnit = null;

                CardModel opponentHotUnit = null;
                CardModel opponentHot2Unit = null;
                CardModel opponentAlphaUnit = null;

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                    player => {},
                    opponent => {},
                    player => {},
                    opponent => {},
                    player =>
                    {
                        player.CardPlay(playerHotId, ItemPosition.Start);
                        player.CardPlay(playerHot2Id, ItemPosition.Start);
                        player.CardPlay(playerHot3Id, ItemPosition.Start);
                        player.CardPlay(playerCardId, ItemPosition.Start);
                        player.CardAbilityUsed(playerCardId, Enumerators.AbilityType.TAKE_UNIT_TYPE_TO_ALLY_UNIT, new List<ParametrizedAbilityInstanceId>());
                        player.LetsThink(2, true);

                        player.AssertInQueue(() =>
                        {
                            playerHotUnit = (CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerHotId);
                            playerHot2Unit = (CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerHot2Id);
                            playerHot3Unit = (CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerHot3Id);
                            playerAlphaUnit = (CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCardId);
                        });
                    },
                    opponent =>
                    {
                        opponent.CardPlay(opponentHotId, ItemPosition.Start);
                        opponent.CardPlay(opponentHot2Id, ItemPosition.Start);
                        opponent.CardPlay(opponentCardId, ItemPosition.Start);
                        opponent.CardAbilityUsed(opponentCardId, Enumerators.AbilityType.TAKE_UNIT_TYPE_TO_ALLY_UNIT, new List<ParametrizedAbilityInstanceId>());
                        opponent.LetsThink(2, true);

                        opponent.AssertInQueue(() =>
                        {
                            opponentHotUnit = (CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentHotId);
                            opponentHot2Unit = (CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentHot2Id);
                            opponentAlphaUnit = (CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCardId);
                        });
                    },
                    player => {},
                    opponent => {}
                };

                Action validateEndState = () =>
                {
                    Assert.IsTrue(playerHotUnit.HasFeral);
                    Assert.IsTrue(playerHot2Unit.HasFeral);
                    Assert.IsTrue(playerHot3Unit.HasFeral);
                    Assert.IsTrue(opponentHotUnit.HasFeral);
                    Assert.IsTrue(opponentHot2Unit.HasFeral);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            }, 400);
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Volcan()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 1,
                    new DeckCardData("Volcan", 1),
                    new DeckCardData("Zlab", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 1,
                    new DeckCardData("Volcan", 1),
                    new DeckCardData("Zlab", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Volcan", 1);
                InstanceId playerZlabId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zlab", 1);
                InstanceId playerZlab2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zlab", 2);
                InstanceId playerZlab3Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zlab", 3);
                InstanceId playerZlab4Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zlab", 4);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Volcan", 1);
                InstanceId opponentZlabId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zlab", 1);
                InstanceId opponentZlab2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zlab", 2);
                InstanceId opponentZlab3Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zlab", 3);
                InstanceId opponentZlab4Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zlab", 4);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                    player => {},
                    opponent => {},
                    player => {},
                    opponent =>
                    {
                        opponent.CardPlay(opponentZlabId, ItemPosition.Start);
                        opponent.CardPlay(opponentZlab2Id, ItemPosition.Start);
                        opponent.CardPlay(opponentZlab3Id, ItemPosition.Start);
                        opponent.CardPlay(opponentZlab4Id, ItemPosition.Start);
                    },
                    player =>
                    {
                        player.CardPlay(playerZlabId, ItemPosition.Start);
                        player.CardPlay(playerZlab2Id, ItemPosition.Start);
                        player.CardPlay(playerZlab3Id, ItemPosition.Start);
                        player.CardPlay(playerZlab4Id, ItemPosition.Start);
                        player.CardPlay(playerCardId, ItemPosition.Start);
                        player.CardAbilityUsed(playerCardId, Enumerators.AbilityType.TAKE_DAMAGE_RANDOM_ENEMY, new List<ParametrizedAbilityInstanceId>());
                    },
                    opponent =>
                    {
                        opponent.CardPlay(opponentCardId, ItemPosition.Start);
                        opponent.CardAbilityUsed(opponentCardId, Enumerators.AbilityType.TAKE_DAMAGE_RANDOM_ENEMY, new List<ParametrizedAbilityInstanceId>());
                    },
                    player => {}
                };

                Action validateEndState = () =>
                {
                    int value = 7;

                    Assert.IsTrue(pvpTestContext.GetCurrentPlayer().CardsOnBoard.Count == 4 ||
                        pvpTestContext.GetCurrentPlayer().Defense == pvpTestContext.GetCurrentPlayer().MaxCurrentDefense - value);

                    Assert.IsTrue(pvpTestContext.GetOpponentPlayer().CardsOnBoard.Count == 4 ||
                        pvpTestContext.GetOpponentPlayer().Defense == pvpTestContext.GetOpponentPlayer().MaxCurrentDefense - value);

                    Assert.IsFalse(((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCardId))
                     .CanAttackByDefault);

                    Assert.IsFalse(((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCardId)).CanAttackByDefault);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            }, 400);
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Zhampion()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 1,
                    new DeckCardData("Zhampion", 1),
                    new DeckCardData("Burn", 10));
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 1,
                    new DeckCardData("Zhampion", 1),
                    new DeckCardData("Burn", 10));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zhampion", 1);
                InstanceId playerBurnId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Burn", 1);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zhampion", 1);
                InstanceId opponentBurnId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Burn", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                    player => {},
                    opponent => {},
                    player => {},
                    opponent => {},
                    player =>
                    {
                        player.CardPlay(playerBurnId, ItemPosition.Start);
                        player.CardPlay(playerCardId, ItemPosition.Start, playerBurnId);

                    },
                    opponent =>
                    {
                        opponent.CardPlay(opponentBurnId, ItemPosition.Start);
                        opponent.CardPlay(opponentCardId, ItemPosition.Start, opponentBurnId);
                    },
                    player => {},
                    opponent => {}
                };

                int buffedAttack = 5;

                Action validateEndState = () =>
                {
                    Assert.AreEqual(buffedAttack, ((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerBurnId)).BuffedDamage);
                    Assert.AreEqual(buffedAttack, ((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentBurnId)).BuffedDamage);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            }, 400);
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Enrager()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 1,
                    new DeckCardData("Enrager", 1),
                    new DeckCardData("Hot", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 1,
                    new DeckCardData("Enrager", 1),
                    new DeckCardData("Hot", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Enrager", 1);
                InstanceId playerHotId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Hot", 1);
                InstanceId playerHot2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Hot", 2);
                InstanceId playerHot3Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Hot", 3);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Enrager", 1);
                InstanceId opponentHotId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Hot", 1);
                InstanceId opponentHot2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Hot", 2);

                int value = 2;

                CardModel playerHotUnit = null;
                CardModel playerHot2Unit = null;
                CardModel playerHot3Unit = null;
                CardModel playerEnragerUnit = null;

                CardModel opponentHotUnit = null;
                CardModel opponentHot2Unit = null;
                CardModel opponentEnragerUnit = null;

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                    player => {},
                    opponent => {},
                    player => {},
                    opponent => {},
                    player =>
                    {
                        player.CardPlay(playerHotId, ItemPosition.Start);
                        player.CardPlay(playerHot2Id, ItemPosition.Start);
                        player.CardPlay(playerHot3Id, ItemPosition.Start);
                        player.CardPlay(playerCardId, ItemPosition.Start);
                        player.LetsThink(2, true);

                        player.AssertInQueue(() =>
                        {
                            playerHotUnit = (CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerHotId);
                            playerHot2Unit = (CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerHot2Id);
                            playerHot3Unit = (CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerHot3Id);
                            playerEnragerUnit = (CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCardId);

                            Assert.AreEqual(playerHotUnit.Prototype.Damage + value, playerHotUnit.CurrentDamage);
                            Assert.AreEqual(playerHot2Unit.Prototype.Damage + value, playerHot2Unit.CurrentDamage);
                            Assert.AreEqual(playerHot3Unit.Prototype.Damage + value, playerHot3Unit.CurrentDamage);
                            Assert.AreEqual(playerEnragerUnit.Prototype.Damage + value, playerEnragerUnit.CurrentDamage);
                        });
                    },
                    opponent =>
                    {
                        Assert.AreEqual(playerHotUnit.Prototype.Damage, playerHotUnit.CurrentDamage);
                        Assert.AreEqual(playerHot2Unit.Prototype.Damage, playerHot2Unit.CurrentDamage);
                        Assert.AreEqual(playerHot3Unit.Prototype.Damage, playerHot3Unit.CurrentDamage);
                        Assert.AreEqual(playerEnragerUnit.Prototype.Damage, playerEnragerUnit.CurrentDamage);

                        opponent.CardPlay(opponentHotId, ItemPosition.Start);
                        opponent.CardPlay(opponentHot2Id, ItemPosition.Start);
                        opponent.CardPlay(opponentCardId, ItemPosition.Start);
                        opponent.LetsThink(2, true);

                        opponent.AssertInQueue(() =>
                        {
                            opponentHotUnit = (CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentHotId);
                            opponentHot2Unit = (CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentHot2Id);
                            opponentEnragerUnit = (CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCardId);

                            Assert.AreEqual(opponentHotUnit.Prototype.Damage + value, opponentHotUnit.CurrentDamage);
                            Assert.AreEqual(opponentHot2Unit.Prototype.Damage + value, opponentHot2Unit.CurrentDamage);
                            Assert.AreEqual(opponentEnragerUnit.Prototype.Damage + value, opponentEnragerUnit.CurrentDamage);
                        });
                    },
                    player =>
                    {
                        Assert.AreEqual(opponentHotUnit.Prototype.Damage, opponentHotUnit.CurrentDamage);
                        Assert.AreEqual(opponentHot2Unit.Prototype.Damage, opponentHot2Unit.CurrentDamage);
                        Assert.AreEqual(opponentEnragerUnit.Prototype.Damage, opponentEnragerUnit.CurrentDamage);
                    },
                    opponent => {}
                };

                Action validateEndState = () => {};

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            }, 400);
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        [Category("PlayQuickSubset2")]
        public IEnumerator Gargantua()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 1, new DeckCardData("Gargantua", 6));
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 1, new DeckCardData("Gargantua", 6));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Gargantua", 1);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Gargantua", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                    player =>
                    {
                        player.CardPlay(playerCardId, ItemPosition.Start, pvpTestContext.GetOpponentPlayer().InstanceId);
                    },
                    opponent =>
                    {
                        opponent.CardPlay(opponentCardId, ItemPosition.Start, pvpTestContext.GetCurrentPlayer().InstanceId);
                    },
                };

                int value = 8;

                Action validateEndState = () =>
                {
                    Assert.AreEqual(pvpTestContext.GetCurrentPlayer().InitialDefense - value, pvpTestContext.GetCurrentPlayer().Defense);
                    Assert.AreEqual(pvpTestContext.GetOpponentPlayer().InitialDefense - value, pvpTestContext.GetOpponentPlayer().Defense);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        [Category("PlayQuickSubset2")]
        public IEnumerator Cerberuz()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 1,
                    new DeckCardData("Cerberuz", 1),
                    new DeckCardData("Zlab", 20)

                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 1,
                    new DeckCardData("Cerberuz", 1),
                    new DeckCardData("Zlab", 20)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerMountainId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Cerberuz", 1);
                InstanceId playerZlabId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zlab", 1);
                InstanceId playerZlab2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zlab", 2);
                InstanceId playerZlab3Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zlab", 3);
                InstanceId opponentMountainId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Cerberuz", 1);
                InstanceId opponentZlabId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zlab", 1);
                InstanceId opponentZlab2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zlab", 2);
                InstanceId opponentZlab3Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zlab", 3);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                       player => {},
                       opponent => {},
                       player => {},
                       opponent => {},
                       player =>
                       {
                           player.CardPlay(playerZlabId, ItemPosition.Start);
                           player.CardPlay(playerZlab2Id, ItemPosition.Start);
                           player.CardPlay(playerZlab3Id, ItemPosition.Start);
                           player.CardPlay(playerMountainId, ItemPosition.Start);
                           player.CardAbilityUsed(playerMountainId, Enumerators.AbilityType.SWING, new List<ParametrizedAbilityInstanceId>());
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentZlabId, ItemPosition.Start);
                           opponent.CardPlay(opponentZlab2Id, ItemPosition.Start);
                           opponent.CardPlay(opponentZlab3Id, ItemPosition.Start);
                           opponent.CardPlay(opponentMountainId, ItemPosition.Start);
                           opponent.CardAbilityUsed(opponentMountainId, Enumerators.AbilityType.SWING, new List<ParametrizedAbilityInstanceId>());
                       },
                       player =>
                       {
                           player.CardAttack(playerMountainId, opponentZlab2Id);
                       },
                       opponent =>
                       {
                           opponent.CardAttack(opponentMountainId, playerZlab2Id);
                           opponent.LetsThink(6);
                       }
                };

                Action validateEndState = () =>
                {
                    Assert.IsNull(TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerZlabId));
                    Assert.IsNull(TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerZlab2Id));
                    Assert.IsNull(TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerZlab3Id));
                    Assert.IsNull(TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentZlabId));
                    Assert.IsNull(TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentZlab2Id));
                    Assert.IsNull(TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentZlab3Id));
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }
    }
}
