using System;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using UnityEngine.TestTools;
using System.Linq;

namespace Loom.ZombieBattleground.Test.MultiplayerTests
{
    public class ItemsCardsTests : BaseIntegrationTest
    {
[UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Shovel()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0,
                    new DeckCardData("Shovel", 2),
                    new DeckCardData("Tiny", 6)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    new DeckCardData("Shovel", 2),
                    new DeckCardData("Tiny", 6)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerShovelId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Shovel", 1);
                InstanceId playerShovel1Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Shovel", 2);
                InstanceId playerTinyId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Tiny", 1);
                InstanceId opponentShovelId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Shovel", 1);
                InstanceId opponentShovel1Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Shovel", 2);
                InstanceId opponentTinyId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Tiny", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                       player => player.CardPlay(playerTinyId, ItemPosition.Start),
                       opponent => opponent.CardPlay(opponentTinyId, ItemPosition.Start),
                       player =>
                       {
                            TestHelper.AbilitiesController.HasPredefinedChoosableAbility = true;
                            TestHelper.AbilitiesController.PredefinedChoosableAbilityId = 0;
                            player.CardPlay(playerShovelId, ItemPosition.Start, pvpTestContext.GetOpponentPlayer().InstanceId);
                       },
                       opponent =>
                       {
                            opponent.CardAttack(opponentTinyId, playerTinyId);
                            opponent.CardPlay(opponentShovelId, ItemPosition.Start, null, true);
                            opponent.CardAbilityUsed(opponentShovelId, Enumerators.AbilityType.DAMAGE_TARGET, new List<ParametrizedAbilityInstanceId>()
                            {
                                new ParametrizedAbilityInstanceId(pvpTestContext.GetCurrentPlayer().InstanceId)
                            });
                       },
                       player =>
                       {
                            TestHelper.AbilitiesController.HasPredefinedChoosableAbility = true;
                            TestHelper.AbilitiesController.PredefinedChoosableAbilityId = 1;
                            player.CardPlay(playerShovel1Id, ItemPosition.Start, playerTinyId);
                       },
                       opponent =>
                       {
                            opponent.CardPlay(opponentShovel1Id, ItemPosition.Start, null, true);
                            opponent.CardAbilityUsed(opponentShovel1Id, Enumerators.AbilityType.HEAL, new List<ParametrizedAbilityInstanceId>()
                            {
                                new ParametrizedAbilityInstanceId(opponentTinyId)
                            });
                       }
                };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(17, pvpTestContext.GetOpponentPlayer().Defense);
                    Assert.AreEqual(17, pvpTestContext.GetCurrentPlayer().Defense);
                    Assert.AreEqual(4, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerTinyId)).CurrentDefense);
                    Assert.AreEqual(4, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentTinyId)).CurrentDefense);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }
    
        [UnityTest]
        [Timeout(int.MaxValue)]
        [Category("PlayQuickSubset2")]
        public IEnumerator Boomstick()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0,
                    new DeckCardData("Boomstick", 1),
                    new DeckCardData("Zlab", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    new DeckCardData("Boomstick", 1),
                    new DeckCardData("Zlab", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                int value = 2;

                InstanceId playerZlabId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zlab", 1);
                InstanceId playerZlab2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zlab", 2);
                InstanceId playerZlab3Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zlab", 3);
                InstanceId playerZlab4Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zlab", 4);
                InstanceId playerBoomstickId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Boomstick", 1);

                InstanceId opponentZlabId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zlab", 1);
                InstanceId opponentZlab2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zlab", 2);
                InstanceId opponentZlab3Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zlab", 3);
                InstanceId opponentZlab4Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zlab", 4);
                InstanceId opponentBoomstickId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Boomstick", 1);
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
                           player.CardPlay(playerZlab4Id, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentZlabId, ItemPosition.Start);
                           opponent.CardPlay(opponentZlab2Id, ItemPosition.Start);
                           opponent.CardPlay(opponentZlab3Id, ItemPosition.Start);
                           opponent.CardPlay(opponentZlab4Id, ItemPosition.Start);
                       },
                       player =>
                       {
                           player.CardPlay(playerBoomstickId, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentBoomstickId, ItemPosition.Start, skipEntryAbilities:true);
                           opponent.CardAbilityUsed(opponentBoomstickId, Enumerators.AbilityType.MASSIVE_DAMAGE, new List<ParametrizedAbilityInstanceId>(){});
                       },
                       player => {},
                       opponent => {}
                   };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(pvpTestContext.GetCurrentPlayer().CardsOnBoard.Count,
                        pvpTestContext.GetCurrentPlayer().CardsOnBoard.FindAll(card => card.CurrentDefense == card.Card.Prototype.Defense - value).Count);
                    Assert.AreEqual(pvpTestContext.GetOpponentPlayer().CardsOnBoard.Count,
                        pvpTestContext.GetOpponentPlayer().CardsOnBoard.FindAll(card => card.CurrentDefense == card.Card.Prototype.Defense - value).Count);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }


        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Stapler()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0,
                    new DeckCardData("Stapler", 1),
                    new DeckCardData("Earthshaker", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    new DeckCardData("Stapler", 1),
                    new DeckCardData("Earthshaker", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerEarthshakerId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Earthshaker", 1);
                InstanceId playerStaplerId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Stapler", 1);

                InstanceId opponentEarthshakerId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Earthshaker", 1);
                InstanceId opponentStaplerId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Stapler", 1);
                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                   {
                       player => {},
                       opponent => {},
                       player => player.CardPlay(playerEarthshakerId, ItemPosition.Start),
                       opponent => opponent.CardPlay(opponentEarthshakerId, ItemPosition.Start),
                       player =>
                       {
                           player.CardAttack(playerEarthshakerId, opponentEarthshakerId);
                           player.CardPlay(playerStaplerId, ItemPosition.Start, playerEarthshakerId);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentStaplerId, ItemPosition.Start, null, true);
                           opponent.CardAbilityUsed(opponentStaplerId, Enumerators.AbilityType.HEAL, new List<ParametrizedAbilityInstanceId>(){
                               new ParametrizedAbilityInstanceId(opponentEarthshakerId)
                           });
                       }
                   };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(5, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerEarthshakerId)).CurrentDefense);
                    Assert.AreEqual(5, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentEarthshakerId)).CurrentDefense);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }



        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Chainsaw()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0,
                    new DeckCardData("Chainsaw", 1),
                    new DeckCardData("Earthshaker", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    new DeckCardData("Chainsaw", 1),
                    new DeckCardData("Earthshaker", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerEarthshakerId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Earthshaker", 1);
                InstanceId playerChainsawId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Chainsaw", 1);

                InstanceId opponentEarthshakerId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Earthshaker", 1);
                InstanceId opponentChainsawId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Chainsaw", 1);
                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                   {
                       player => {},
                       opponent => {},
                       player => {},
                       opponent => {},
                       player => player.CardPlay(playerEarthshakerId, ItemPosition.Start),
                       opponent => opponent.CardPlay(opponentEarthshakerId, ItemPosition.Start),
                       player =>
                       {
                           player.CardPlay(playerChainsawId, ItemPosition.Start, playerEarthshakerId);
                           player.CardAttack(playerEarthshakerId, TestHelper.GetOpponentPlayer().InstanceId);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentChainsawId, ItemPosition.Start, null, true);
                           opponent.CardAbilityUsed(opponentChainsawId, Enumerators.AbilityType.UNIT_WEAPON, new List<ParametrizedAbilityInstanceId>(){
                               new ParametrizedAbilityInstanceId(opponentEarthshakerId)
                           });
                           opponent.CardAttack(opponentEarthshakerId, TestHelper.GetCurrentPlayer().InstanceId);
                       },
                   };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(1, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerEarthshakerId)).CurrentDefense);
                    Assert.AreEqual(1, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentEarthshakerId)).CurrentDefense);
                    Assert.AreEqual(TestHelper.GetCurrentPlayer().InitialDefense - 8, TestHelper.GetCurrentPlayer().Defense);
                    Assert.AreEqual(TestHelper.GetOpponentPlayer().InitialDefense - 8, TestHelper.GetOpponentPlayer().Defense);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Torch()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0,
                    new DeckCardData("Torch", 1),
                    new DeckCardData("Zlab", 1),
                    new DeckCardData("Pyromaz", 1),
                    new DeckCardData("Brook", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    new DeckCardData("Torch", 1),
                    new DeckCardData("Zlab", 1),
                    new DeckCardData("Pyromaz", 1),
                    new DeckCardData("Brook", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerZlabId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zlab", 1);
                InstanceId playerPyromazId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Pyromaz", 1);
                InstanceId playerBrookId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Brook", 1);
                InstanceId playerTorchId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Torch", 1);

                InstanceId opponentZlabId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zlab", 1);
                InstanceId opponentPyromazId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Pyromaz", 1);
                InstanceId opponentBrookId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Brook", 1);
                InstanceId opponentTorchId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Torch", 1);
                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                   {
                       player => {},
                       opponent => {},
                       player =>
                       {
                           player.CardPlay(playerZlabId, ItemPosition.Start);
                           player.CardPlay(playerPyromazId, ItemPosition.Start);
                           player.CardPlay(playerBrookId, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentZlabId, ItemPosition.Start);
                           opponent.CardPlay(opponentPyromazId, ItemPosition.Start);
                           opponent.CardPlay(opponentBrookId, ItemPosition.Start);
                       },
                       player =>
                       {
                           player.CardPlay(playerTorchId, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentTorchId, ItemPosition.Start, null, true);
                           opponent.CardAbilityUsed(opponentTorchId, Enumerators.AbilityType.DESTROY_UNIT_BY_COST, new List<ParametrizedAbilityInstanceId>(){
                               new ParametrizedAbilityInstanceId(opponentPyromazId)
                           });
                       },
                       player => {},
                       opponent => {},
                   };

                Action validateEndState = () =>
                {
                    Assert.IsTrue(pvpTestContext.GetCurrentPlayer().CardsOnBoard.FindAll(card => card.Card.Prototype.Cost < 4).Count == 1);
                    Assert.IsTrue(pvpTestContext.GetOpponentPlayer().CardsOnBoard.FindAll(card => card.Card.Prototype.Cost < 4).Count == 1);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Extinguisher()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0,
                    new DeckCardData("Extinguisher", 1),
                    new DeckCardData("Zlab", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    new DeckCardData("Extinguisher", 1),
                    new DeckCardData("Zlab", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerZlabId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zlab", 1);
                InstanceId playerZlab2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zlab", 2);
                InstanceId playerExtinguisherId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Extinguisher", 1);

                InstanceId opponentZlabId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zlab", 1);
                InstanceId opponentZlab2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zlab", 2);
                InstanceId opponentExtinguisherId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Extinguisher", 1);
                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                   {
                       player =>
                       {
                           player.CardPlay(playerZlabId, ItemPosition.Start);
                           player.CardPlay(playerZlab2Id, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentZlabId, ItemPosition.Start);
                           opponent.CardPlay(opponentZlab2Id, ItemPosition.Start);
                       },
                       player =>
                       {
                           player.CardPlay(playerExtinguisherId, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentExtinguisherId, ItemPosition.Start, null, true);
                           opponent.CardAbilityUsed(opponentExtinguisherId, Enumerators.AbilityType.FREEZE_UNITS, new List<ParametrizedAbilityInstanceId>(){});
                       }
                   };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(pvpTestContext.GetCurrentPlayer().CardsOnBoard.Count,
                                    pvpTestContext.GetCurrentPlayer().CardsOnBoard.FindAll(card => card.IsStun == true).Count);
                    Assert.AreEqual(pvpTestContext.GetOpponentPlayer().CardsOnBoard.Count,
                                    pvpTestContext.GetOpponentPlayer().CardsOnBoard.FindAll(card => card.IsStun == true).Count);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Bulldozer()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0,
                    new DeckCardData("Bulldozer", 1),
                    new DeckCardData("Zlab", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    new DeckCardData("Bulldozer", 1),
                    new DeckCardData("Zlab", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerZlabId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zlab", 1);
                InstanceId playerZlab2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zlab", 2);
                InstanceId playerZlab3Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zlab", 3);
                InstanceId playerZlab4Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zlab", 4);
                InstanceId playerBulldozerId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Bulldozer", 1);

                InstanceId opponentZlabId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zlab", 1);
                InstanceId opponentZlab2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zlab", 2);
                InstanceId opponentZlab3Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zlab", 3);
                InstanceId opponentZlab4Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zlab", 4);
                InstanceId opponentBulldozerId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Bulldozer", 1);
                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                   {
                       player => {},
                       opponent => {},
                       player =>
                       {
                           player.CardPlay(playerZlabId, ItemPosition.Start);
                           player.CardPlay(playerZlab2Id, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentZlabId, ItemPosition.Start);
                           opponent.CardPlay(opponentZlab2Id, ItemPosition.Start);
                           opponent.LetsThink(2);
                           opponent.CardPlay(opponentBulldozerId, ItemPosition.Start, null, true);
                           opponent.CardAbilityUsed(opponentBulldozerId, Enumerators.AbilityType.DESTROY_UNITS, new List<ParametrizedAbilityInstanceId>(){});
                           opponent.LetsThink(4);
                           opponent.CardPlay(opponentZlab3Id, ItemPosition.Start);
                           opponent.CardPlay(opponentZlab4Id, ItemPosition.Start);
                       },
                       player =>
                       {
                           player.CardPlay(playerZlab3Id, ItemPosition.Start);
                           player.CardPlay(playerZlab4Id, ItemPosition.Start);
                           player.LetsThink(2);
                           player.CardPlay(playerBulldozerId, ItemPosition.Start);
                           player.LetsThink(6);
                       },
                       opponent =>{},
                   };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(0, pvpTestContext.GetCurrentPlayer().CardsOnBoard.Count);
                    Assert.AreEqual(0, pvpTestContext.GetOpponentPlayer().CardsOnBoard.Count);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Leash()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0,
                    new DeckCardData("Leash", 1),
                    new DeckCardData("Zlab", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    new DeckCardData("Leash", 1),
                    new DeckCardData("Zlab", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerZlabId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zlab", 1);
                InstanceId playerZlab2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zlab", 2);
                InstanceId playerLeashId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Leash", 1);

                InstanceId opponentZlabId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zlab", 1);
                InstanceId opponentZlab2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zlab", 2);
                InstanceId opponentLeashId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Leash", 1);
                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                   {
                       player => {},
                       opponent => {},
                       player =>
                       {
                           player.CardPlay(playerZlabId, ItemPosition.Start);
                           player.CardPlay(playerZlab2Id, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentZlabId, ItemPosition.Start);
                           opponent.CardPlay(opponentZlab2Id, ItemPosition.Start);
                       },
                       player =>
                       {
                           player.CardPlay(playerLeashId, ItemPosition.Start, opponentZlab2Id);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentLeashId, ItemPosition.Start, null, true);
                           opponent.CardAbilityUsed(opponentLeashId, Enumerators.AbilityType.TAKE_CONTROL_ENEMY_UNIT, new List<ParametrizedAbilityInstanceId>(){
                               new ParametrizedAbilityInstanceId(playerZlab2Id)
                           });
                       },
                       player => {},
                       opponent => {},
                   };

                Action validateEndState = () =>
                {
                    BoardUnitModel playerZlabUnit = (BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerZlabId);
                    BoardUnitModel playerZlab2Unit = (BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerZlab2Id);
                    BoardUnitModel opponentZlabUnit = (BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentZlabId);
                    BoardUnitModel opponentZlab2Unit = (BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentZlab2Id);

                    Assert.IsTrue(pvpTestContext.GetCurrentPlayer().CardsOnBoard.Contains(opponentZlab2Unit));

                    Assert.IsTrue(pvpTestContext.GetOpponentPlayer().CardsOnBoard.Contains(playerZlab2Unit));
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }


        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Whistle()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0,
                   new DeckCardData("Whistle", 10)
               );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    new DeckCardData("Whistle", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Whistle", 1);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Whistle", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                    player =>
                    {
                        player.CardPlay(playerCardId, ItemPosition.Start);
                    },
                    opponent =>
                    {
                        opponent.CardPlay(opponentCardId, ItemPosition.Start, null, true);
                        opponent.CardAbilityUsed(opponentCardId, Enumerators.AbilityType.DRAW_CARD, new List<ParametrizedAbilityInstanceId>());
                    }
                };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(5, pvpTestContext.GetCurrentPlayer().CardsInHand.Count);
                    Assert.AreEqual(5, pvpTestContext.GetOpponentPlayer().CardsInHand.Count);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }


        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator GooBeaker()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 1, new DeckCardData("Goo Beaker", 6));
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 1, new DeckCardData("Goo Beaker", 6));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Goo Beaker", 1);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Goo Beaker", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                    player =>
                    {
                        player.LetsThink(2);
                        player.CardPlay(playerCardId, ItemPosition.Start);
                        player.LetsThink(2);
                        player.AssertInQueue(() => {
                            Assert.AreEqual(2, pvpTestContext.GetCurrentPlayer().GooVials);
                        });
                    },
                    opponent =>
                    {
                        opponent.LetsThink(2);
                        opponent.CardPlay(opponentCardId, ItemPosition.Start, null, true);
                        opponent.CardAbilityUsed(opponentCardId, Enumerators.AbilityType.ADD_GOO_VIAL, new List<ParametrizedAbilityInstanceId>());
                        opponent.LetsThink(2);
                        opponent.AssertInQueue(() => {
                            Assert.AreEqual(2, pvpTestContext.GetOpponentPlayer().GooVials);
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
        public IEnumerator GooBottles()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 1, new DeckCardData("Goo Bottles", 6));
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 1, new DeckCardData("Goo Bottles", 6));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Goo Bottles", 1);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Goo Bottles", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                    player =>
                    {
                        player.LetsThink(2);
                        player.CardPlay(playerCardId, ItemPosition.Start);
                        player.LetsThink(2);
                        player.AssertInQueue(() => {
                            Assert.AreEqual(3, pvpTestContext.GetCurrentPlayer().GooVials);
                        });
                    },
                    opponent =>
                    {
                        opponent.LetsThink(2);
                        opponent.CardPlay(opponentCardId, ItemPosition.Start, null, true);
                        opponent.CardAbilityUsed(opponentCardId, Enumerators.AbilityType.ADD_GOO_VIAL, new List<ParametrizedAbilityInstanceId>());
                        opponent.LetsThink(2);
                        opponent.AssertInQueue(() => {
                            Assert.AreEqual(3, pvpTestContext.GetOpponentPlayer().GooVials);
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
        public IEnumerator SuperSerum()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0,
                    new DeckCardData("Super Serum", 1),
                    new DeckCardData("Earthshaker", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    new DeckCardData("Super Serum", 1),
                    new DeckCardData("Earthshaker", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerEarthshakerId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Earthshaker", 1);
                InstanceId playerSuperSerumId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Super Serum", 1);

                InstanceId opponentEarthshakerId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Earthshaker", 1);
                InstanceId opponentSuperSerumId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Super Serum", 1);
                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                   {
                       player => {},
                       opponent => {},
                       player => player.CardPlay(playerEarthshakerId, ItemPosition.Start),
                       opponent => opponent.CardPlay(opponentEarthshakerId, ItemPosition.Start),
                       player =>
                       {
                           player.CardPlay(playerSuperSerumId, ItemPosition.Start, playerEarthshakerId);
                           player.LetsThink(6);
                           player.AssertInQueue(() => {
                                Assert.AreEqual(8, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerEarthshakerId)).CurrentDefense);
                                Assert.AreEqual(7, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerEarthshakerId)).CurrentDamage);
                           });
                           player.CardAttack(playerEarthshakerId, TestHelper.GetOpponentPlayer().InstanceId);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentSuperSerumId, ItemPosition.Start, null, true);
                           opponent.CardAbilityUsed(opponentSuperSerumId, Enumerators.AbilityType.UNIT_WEAPON, new List<ParametrizedAbilityInstanceId>(){
                               new ParametrizedAbilityInstanceId(opponentEarthshakerId)
                           });
                           opponent.LetsThink(6);
                           opponent.AssertInQueue(() => {
                                Assert.AreEqual(8, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentEarthshakerId)).CurrentDefense);
                                Assert.AreEqual(7, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentEarthshakerId)).CurrentDamage);
                           });
                           opponent.CardAttack(opponentEarthshakerId, TestHelper.GetCurrentPlayer().InstanceId);
                       },
                   };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(5, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerEarthshakerId)).CurrentDefense);
                    Assert.AreEqual(7, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerEarthshakerId)).CurrentDamage);
                    Assert.AreEqual(5, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentEarthshakerId)).CurrentDefense);
                    Assert.AreEqual(7, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentEarthshakerId)).CurrentDamage);
                    Assert.AreEqual(TestHelper.GetCurrentPlayer().InitialDefense - 7, TestHelper.GetCurrentPlayer().Defense);
                    Assert.AreEqual(TestHelper.GetOpponentPlayer().InitialDefense - 7, TestHelper.GetOpponentPlayer().Defense);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }


        [UnityTest]
        [Timeout(int.MaxValue)]
        [Category("PlayQuickSubset2")]
        public IEnumerator Harpoon()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 1, new DeckCardData("Harpoon", 6));
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 1, new DeckCardData("Harpoon", 6));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Harpoon", 1);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Harpoon", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                    player =>
                    {
                        player.CardPlay(playerCardId, ItemPosition.Start, pvpTestContext.GetOpponentPlayer().InstanceId);
                    },
                    opponent =>
                    {
                        opponent.CardPlay(opponentCardId, ItemPosition.Start, null, true);
                        opponent.CardAbilityUsed(opponentCardId, Enumerators.AbilityType.DAMAGE_TARGET, new List<ParametrizedAbilityInstanceId>(){
                               new ParametrizedAbilityInstanceId(pvpTestContext.GetCurrentPlayer().InstanceId)
                        });
                    },
                };

                int value = 6;

                Action validateEndState = () =>
                {
                    Assert.AreEqual(pvpTestContext.GetCurrentPlayer().InitialDefense - value, pvpTestContext.GetCurrentPlayer().Defense);
                    Assert.AreEqual(pvpTestContext.GetOpponentPlayer().InitialDefense - value, pvpTestContext.GetOpponentPlayer().Defense);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        [Category("PlayQuickSubset2")]
        public IEnumerator Bazooka()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 1,
                    new DeckCardData("Bazooka", 1),
                    new DeckCardData("Hot", 20));
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 1,
                    new DeckCardData("Bazooka", 1),
                    new DeckCardData("Hot", 20));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Bazooka", 1);
                InstanceId playerHotId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Hot", 1);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Bazooka", 1);
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
                        opponent.CardPlay(opponentCardId, ItemPosition.Start, null, true);
                        opponent.CardAbilityUsed(opponentCardId, Enumerators.AbilityType.DAMAGE_TARGET, new List<ParametrizedAbilityInstanceId>(){
                               new ParametrizedAbilityInstanceId(playerHotId)
                        });
                    },
                    player =>
                    {
                        player.CardPlay(playerCardId, ItemPosition.Start, opponentHotId);
                    },
                };

                int value = 10;

                Action validateEndState = () =>
                {
                    BoardUnitModel playerHotUnit = (BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerHotId);
                    BoardUnitModel opponentHotUnit = (BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentHotId);

                    Assert.AreEqual(playerHotUnit.MaxCurrentDefense - value, playerHotUnit.CurrentDefense);
                    Assert.AreEqual(opponentHotUnit.MaxCurrentDefense - value, opponentHotUnit.CurrentDefense);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }


        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator FreshMeat()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0,
                    new DeckCardData("Fresh Meat", 1),
                    new DeckCardData("Earthshaker", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    new DeckCardData("Fresh Meat", 1),
                    new DeckCardData("Earthshaker", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerEarthshakerId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Earthshaker", 1);
                InstanceId playerEarthshaker2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Earthshaker", 2);
                InstanceId playerEarthshaker3Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Earthshaker", 3);
                InstanceId playerFreshMeatId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Fresh Meat", 1);

                InstanceId opponentEarthshakerId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Earthshaker", 1);
                InstanceId opponentEarthshaker2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Earthshaker", 2);
                InstanceId opponentEarthshaker3Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Earthshaker", 3);
                InstanceId opponentFreshMeatId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Fresh Meat", 1);
                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                   {
                       player => {},
                       opponent => {},
                       player => {},
                       opponent => {},
                       player =>
                       {
                           player.CardPlay(playerEarthshakerId, ItemPosition.Start);
                           player.CardPlay(playerEarthshaker2Id, ItemPosition.Start);
                           player.CardPlay(playerEarthshaker3Id, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentEarthshakerId, ItemPosition.Start);
                           opponent.CardPlay(opponentEarthshaker2Id, ItemPosition.Start);
                           opponent.CardPlay(opponentEarthshaker3Id, ItemPosition.Start);
                       },
                       player =>
                       {
                           player.CardPlay(playerFreshMeatId, ItemPosition.Start);
                           player.LetsThink(4);
                           player.AssertInQueue(() => {
                               Assert.AreEqual(pvpTestContext.GetOpponentPlayer().CardsOnBoard.Count,
                                        pvpTestContext.GetOpponentPlayer().CardsOnBoard.FindAll(card => card.CurrentDamage == card.Card.Prototype.Damage - 3).Count);
                           });
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentFreshMeatId, ItemPosition.Start, null, true);
                           opponent.CardAbilityUsed(opponentFreshMeatId, Enumerators.AbilityType.CHANGE_STAT_UNTILL_END_OF_TURN, new List<ParametrizedAbilityInstanceId>(){});
                           opponent.LetsThink(4);
                           opponent.AssertInQueue(() => {
                                Assert.AreEqual(pvpTestContext.GetCurrentPlayer().CardsOnBoard.Count,
                                        pvpTestContext.GetCurrentPlayer().CardsOnBoard.FindAll(card => card.CurrentDamage == card.Card.Prototype.Damage - 3).Count);
                           });
                       },
                       player => {},
                       opponent => {},
                   };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(pvpTestContext.GetCurrentPlayer().CardsOnBoard.Count,
                                    pvpTestContext.GetCurrentPlayer().CardsOnBoard.FindAll(card => card.CurrentDamage == card.Card.Prototype.Damage).Count);
                    Assert.AreEqual(pvpTestContext.GetOpponentPlayer().CardsOnBoard.Count,
                                    pvpTestContext.GetOpponentPlayer().CardsOnBoard.FindAll(card => card.CurrentDamage == card.Card.Prototype.Damage).Count);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Lawnmower()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0,
                    new DeckCardData("Lawnmower", 1),
                    new DeckCardData("Zlab", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    new DeckCardData("Lawnmower", 1),
                    new DeckCardData("Zlab", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerZlabId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zlab", 1);
                InstanceId playerZlab2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zlab", 2);
                InstanceId playerLawnmowerId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Lawnmower", 1);

                InstanceId opponentZlabId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zlab", 1);
                InstanceId opponentZlab2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zlab", 2);
                InstanceId opponentLawnmowerId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Lawnmower", 1);

                int value = 2;

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
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentZlabId, ItemPosition.Start);
                           opponent.CardPlay(opponentZlab2Id, ItemPosition.Start);
                       },
                       player => 
                       {
                           player.CardPlay(playerLawnmowerId, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentLawnmowerId, ItemPosition.Start, null, true);
                           opponent.CardAbilityUsed(opponentLawnmowerId, Enumerators.AbilityType.DAMAGE_AND_DISTRACT_TARGET, new List<ParametrizedAbilityInstanceId>(){});
                       }
                   };

                Action validateEndState = () =>
                {
                    BoardUnitModel playerZlabUnit = (BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerZlabId);
                    BoardUnitModel playerZlab2Unit = (BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerZlab2Id);
                    BoardUnitModel opponentZlabUnit = (BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentZlabId);
                    BoardUnitModel opponentZlab2Unit = (BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentZlab2Id);

                    Assert.IsTrue(playerZlabUnit.WasDistracted);
                    Assert.AreEqual(playerZlabUnit.MaxCurrentDefense - value, playerZlabUnit.CurrentDefense);

                    Assert.IsTrue(playerZlab2Unit.WasDistracted);
                    Assert.AreEqual(playerZlab2Unit.MaxCurrentDefense - value, playerZlab2Unit.CurrentDefense);

                    Assert.IsTrue(opponentZlabUnit.WasDistracted);
                    Assert.AreEqual(opponentZlabUnit.MaxCurrentDefense - value, opponentZlabUnit.CurrentDefense);

                    Assert.IsTrue(opponentZlab2Unit.WasDistracted);
                    Assert.AreEqual(opponentZlab2Unit.MaxCurrentDefense - value, opponentZlab2Unit.CurrentDefense);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Cart()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0,
                    new DeckCardData("Cart", 1),
                    new DeckCardData("Zlab", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    new DeckCardData("Cart", 1),
                    new DeckCardData("Zlab", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerZlabId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zlab", 1);
                InstanceId playerZlab2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zlab", 2);
                InstanceId playerCartId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Cart", 1);

                InstanceId opponentZlabId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zlab", 1);
                InstanceId opponentZlab2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zlab", 2);
                InstanceId opponentCartId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Cart", 1);
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
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentZlabId, ItemPosition.Start);
                           opponent.CardPlay(opponentZlab2Id, ItemPosition.Start);
                       },
                       player =>
                       {
                           player.CardPlay(playerCartId, ItemPosition.Start, playerZlabId);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentCartId, ItemPosition.Start, null, true);
                           opponent.CardAbilityUsed(opponentCartId, Enumerators.AbilityType.GIVE_BUFFS_TO_UNIT, new List<ParametrizedAbilityInstanceId>()
                           {
                               new ParametrizedAbilityInstanceId(opponentZlab2Id)
                           });
                       }
                   };

                Action validateEndState = () =>
                {
                    Assert.IsTrue(((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerZlabId)).HasSwing);
                    Assert.IsTrue(((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentZlab2Id)).HasSwing);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        [Category("PlayQuickSubset2")]
        public IEnumerator Molotov()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0,
                    new DeckCardData("Molotov", 1),
                    new DeckCardData("Zlab", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    new DeckCardData("Molotov", 1),
                    new DeckCardData("Zlab", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerZlabId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zlab", 1);
                InstanceId playerZlab2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zlab", 2);
                InstanceId playerZlab3Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zlab", 3);
                InstanceId playerZlab4Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zlab", 4);
                InstanceId playerZlab5Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zlab", 5);
                InstanceId playerMolotovId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Molotov", 1);

                InstanceId opponentZlabId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zlab", 1);
                InstanceId opponentZlab2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zlab", 2);
                InstanceId opponentZlab3Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zlab", 3);
                InstanceId opponentZlab4Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zlab", 4);
                InstanceId opponentZlab5Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zlab", 5);
                InstanceId opponentMolotovId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Molotov", 1);
                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                   {
                       player => {},
                       opponent => {},
                       player => {},
                       opponent => {},
                       player =>
                       {
                           player.CardPlay(playerZlabId, ItemPosition.Start);
                           player.LetsThink();
                           player.CardPlay(playerZlab2Id, ItemPosition.Start);
                           player.LetsThink();
                           player.CardPlay(playerZlab3Id, ItemPosition.Start);
                           player.LetsThink();
                           player.CardPlay(playerZlab4Id, ItemPosition.Start);
                           player.LetsThink();
                           player.CardPlay(playerZlab5Id, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentZlabId, ItemPosition.Start);
                           opponent.LetsThink();
                           opponent.CardPlay(opponentZlab2Id, ItemPosition.Start);
                           opponent.LetsThink();
                           opponent.CardPlay(opponentZlab3Id, ItemPosition.Start);
                           opponent.LetsThink();
                           opponent.CardPlay(opponentZlab4Id, ItemPosition.Start);
                           opponent.LetsThink();
                           opponent.CardPlay(opponentZlab5Id, ItemPosition.Start);
                           opponent.LetsThink(2);
                       },
                       player =>
                       {
                           player.CardPlay(playerMolotovId, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentMolotovId, ItemPosition.Start);
                       }
                   };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(8, pvpTestContext.GetCurrentPlayer().CardsOnBoard.Count + pvpTestContext.GetOpponentPlayer().CardsOnBoard.Count);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }


        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator NailBomb()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0,
                    new DeckCardData("Nail Bomb", 1),
                    new DeckCardData("Zlab", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    new DeckCardData("Nail Bomb", 1),
                    new DeckCardData("Zlab", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerZlabId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zlab", 1);
                InstanceId playerZlab2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zlab", 2);
                InstanceId playerZlab3Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zlab", 3);
                InstanceId playerZlab4Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zlab", 4);
                InstanceId playerZlab5Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zlab", 5);
                InstanceId playerNailBombId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Nail Bomb", 1);

                InstanceId opponentZlabId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zlab", 1);
                InstanceId opponentZlab2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zlab", 2);
                InstanceId opponentZlab3Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zlab", 3);
                InstanceId opponentZlab4Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zlab", 4);
                InstanceId opponentZlab5Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zlab", 5);
                InstanceId opponentNailBombId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Nail Bomb", 1);
                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                   {
                       player => {},
                       opponent => {},
                       player => {},
                       opponent => {},
                       player =>
                       {
                           player.CardPlay(playerZlabId, ItemPosition.Start);
                           player.LetsThink();
                           player.CardPlay(playerZlab2Id, ItemPosition.Start);
                           player.LetsThink();
                           player.CardPlay(playerZlab3Id, ItemPosition.Start);
                           player.LetsThink();
                           player.CardPlay(playerZlab4Id, ItemPosition.Start);
                           player.LetsThink();
                           player.CardPlay(playerZlab5Id, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentZlabId, ItemPosition.Start);
                           opponent.LetsThink();
                           opponent.CardPlay(opponentZlab2Id, ItemPosition.Start);
                           opponent.LetsThink();
                           opponent.CardPlay(opponentZlab3Id, ItemPosition.Start);
                           opponent.LetsThink();
                           opponent.CardPlay(opponentZlab4Id, ItemPosition.Start);
                           opponent.LetsThink();
                           opponent.CardPlay(opponentZlab5Id, ItemPosition.Start);
                           opponent.LetsThink(2);
                       },
                       player =>
                       {
                           player.CardPlay(playerNailBombId, ItemPosition.Start, opponentZlab3Id);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentNailBombId, ItemPosition.Start, null, true);
                           opponent.CardAbilityUsed(opponentNailBombId, Enumerators.AbilityType.DAMAGE_TARGET_ADJUSTMENTS, new List<ParametrizedAbilityInstanceId>(){
                               new ParametrizedAbilityInstanceId(playerZlab3Id)
                           });
                       }
                   };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(4, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerZlabId)).CurrentDefense);
                    Assert.AreEqual(4, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerZlab5Id)).CurrentDefense);
                    Assert.AreEqual(2, pvpTestContext.GetCurrentPlayer().CardsOnBoard.Count);

                    Assert.AreEqual(4, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentZlabId)).CurrentDefense);
                    Assert.AreEqual(4, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentZlab5Id)).CurrentDefense);
                    Assert.AreEqual(2, pvpTestContext.GetOpponentPlayer().CardsOnBoard.Count);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Bat()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0,
                    new DeckCardData("Bat", 1),
                    new DeckCardData("Znowman", 1),
                    new DeckCardData("Zlab", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    new DeckCardData("Bat", 1),
                    new DeckCardData("Znowman", 1),
                    new DeckCardData("Zlab", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerZnowmanId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Znowman", 1);
                InstanceId playerZlabId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zlab", 1);
                InstanceId playerBatId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Bat", 1);

                InstanceId opponentZnowmanId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Znowman", 1);
                InstanceId opponentZlabId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zlab", 1);
                InstanceId opponentBatId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Bat", 1);
                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                   {
                       player => {},
                       opponent => {},
                       player =>
                       {
                           player.CardPlay(playerZnowmanId, ItemPosition.Start);
                           player.CardPlay(playerZlabId, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentZnowmanId, ItemPosition.Start);
                           opponent.CardPlay(opponentZlabId, ItemPosition.Start);
                       },
                       player =>
                       {
                           player.CardPlay(playerBatId, ItemPosition.Start, opponentZnowmanId);
                           player.LetsThink(2);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentBatId, ItemPosition.Start, null, true);
                           opponent.CardAbilityUsed(opponentBatId, Enumerators.AbilityType.DAMAGE_AND_DISTRACT_TARGET, new List<ParametrizedAbilityInstanceId>(){
                               new ParametrizedAbilityInstanceId(playerZnowmanId)
                           });
                           opponent.LetsThink(2);
                       },
                       player => player.CardAttack(playerZlabId, opponentZnowmanId),
                       opponent =>
                       {
                           opponent.CardAttack(opponentZlabId, playerZnowmanId);
                           opponent.LetsThink(2);
                       },
                   };

                Action validateEndState = () =>
                {
                    Assert.IsFalse(((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerZlabId)).IsStun);
                    Assert.IsFalse(((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentZlabId)).IsStun);
                    Assert.AreEqual(1, pvpTestContext.GetCurrentPlayer().CardsOnBoard.Count);
                    Assert.AreEqual(1, pvpTestContext.GetOpponentPlayer().CardsOnBoard.Count);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }


        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator SupplyDrop()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0,
                    new DeckCardData("Supply Drop", 1),
                    new DeckCardData("Zlab", 10),
                    new DeckCardData("Znowman", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    new DeckCardData("Supply Drop", 1),
                    new DeckCardData("Zlab", 10),
                    new DeckCardData("Znowman", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Supply Drop", 1);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Supply Drop", 1);

                InstanceId playerZnowmanId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Znowman", 1);
                InstanceId opponentZnowmanId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Znowman", 1);

                BoardUnitModel playerZnowman = null;
                BoardUnitModel opponentZnowman = null;

                BoardUnitModel playerUnitFromDeck = null;
                BoardUnitModel opponentUnitFromDeck = null;

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                   {
                       player => {},
                       opponent =>
                       {
                           opponent.CardPlay(opponentCardId, ItemPosition.Start, null, true);
                           opponent.CardAbilityUsed(opponentCardId, Enumerators.AbilityType.PUT_RANDOM_UNIT_FROM_DECK_ON_BOARD, new List<ParametrizedAbilityInstanceId>(){
                               new ParametrizedAbilityInstanceId(playerZnowmanId),
                               new ParametrizedAbilityInstanceId(opponentZnowmanId)
                           });
                           opponent.LetsThink(2);
                       },
                       player =>
                       {
                           player.CardPlay(playerCardId, ItemPosition.Start);
                           player.LetsThink(2);
                       },
                       opponent =>
                       {
                           playerUnitFromDeck = pvpTestContext.GetCurrentPlayer().CardsOnBoard.FirstOrDefault(card => card.InstanceId != playerZnowmanId);
                           opponentUnitFromDeck = pvpTestContext.GetOpponentPlayer().CardsOnBoard.FirstOrDefault(card => card.InstanceId != opponentZnowmanId);
                       },
                       player =>
                       {
                           playerZnowman = (BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerZnowmanId);
                           opponentZnowman = (BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentZnowmanId);

                           player.LetsThink(2);
                           player.AssertInQueue(() => {
                               Assert.NotNull(playerZnowman);
                               Assert.IsTrue(playerZnowman.UnitCanBeUsable());
                               Assert.NotNull(playerUnitFromDeck);
                               Assert.IsTrue(playerUnitFromDeck.UnitCanBeUsable());
                           });
                       },
                       opponent =>
                       {
                           opponent.LetsThink(2);
                           opponent.AssertInQueue(() => {
                               Assert.NotNull(opponentZnowman);
                               Assert.IsTrue(opponentZnowman.UnitCanBeUsable());
                               Assert.NotNull(opponentUnitFromDeck);
                               Assert.IsTrue(opponentUnitFromDeck.UnitCanBeUsable());
                           });
                       },
                   };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(2, pvpTestContext.GetCurrentPlayer().CardsOnBoard.Count);
                    Assert.AreEqual(2, pvpTestContext.GetOpponentPlayer().CardsOnBoard.Count);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }


        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator JunkSpear()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0,
                    new DeckCardData("Junk Spear", 1),
                    new DeckCardData("Whistle", 20)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    new DeckCardData("Junk Spear", 1),
                    new DeckCardData("Whistle", 3),
                    new DeckCardData("Trunk", 20)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerWhistleId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Whistle", 1);
                InstanceId playerWhistle2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Whistle", 2);
                InstanceId playerWhistle3Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Whistle", 3);
                InstanceId playerJunkSpearId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Junk Spear", 1);
                InstanceId opponentWhistleId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Whistle", 1);
                InstanceId opponentWhistle2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Whistle", 2);
                InstanceId opponentWhistle3Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Whistle", 3);
                InstanceId opponentJunkSpearId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Junk Spear", 1);
                InstanceId opponentTrunkId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Trunk", 1);
                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                   {
                       player => {},
                       opponent => {},
                       player =>
                       {
                           player.CardPlay(playerWhistleId, ItemPosition.Start);
                           player.LetsThink(1);
                           player.CardPlay(playerWhistle2Id, ItemPosition.Start);
                           player.LetsThink(1);
                           player.CardPlay(playerWhistle3Id, ItemPosition.Start);
                           player.LetsThink(1);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentWhistleId, ItemPosition.Start);
                           opponent.LetsThink(1);
                           opponent.CardPlay(opponentWhistle2Id, ItemPosition.Start);
                           opponent.LetsThink(1);
                           opponent.CardPlay(opponentWhistle3Id, ItemPosition.Start);
                           opponent.LetsThink(1);
                           opponent.CardPlay(opponentTrunkId, ItemPosition.Start);
                           opponent.LetsThink(1);
                       },
                       player =>
                       {
                           player.CardPlay(playerJunkSpearId, ItemPosition.Start, opponentTrunkId);
                           player.LetsThink(2);
                       },
                       opponent =>
                       {
                       },
                       player =>
                       {
                       }
                   };

                Action validateEndState = () =>
                {
                    BoardUnitModel trunkOpponentModel  = ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentTrunkId));
                    Assert.AreEqual(trunkOpponentModel.MaxCurrentDefense-3, trunkOpponentModel.CurrentDefense);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }


        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator ZedKit()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0,
                    new DeckCardData("Zed Kit", 1),
                    new DeckCardData("Boomer", 3),
                    new DeckCardData("IgneouZ", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    new DeckCardData("Zed Kit", 1),
                    new DeckCardData("Boomer", 3),
                    new DeckCardData("IgneouZ", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerBoomerId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Boomer", 1);
                InstanceId playerBoomer2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Boomer", 2);
                InstanceId playerBoomer3Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Boomer", 3);
                InstanceId playerIgneouZId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "IgneouZ", 1);
                InstanceId playerIgneouZ2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "IgneouZ", 2);
                InstanceId playerIgneouZ3Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "IgneouZ", 3);
                InstanceId playerZedKitId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zed Kit", 1);

                InstanceId opponentBoomerId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Boomer", 1);
                InstanceId opponentBoomer2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Boomer", 2);
                InstanceId opponentBoomer3Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Boomer", 3);
                InstanceId opponentIgneouZId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "IgneouZ", 1);
                InstanceId opponentIgneouZ2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "IgneouZ", 2);
                InstanceId opponentIgneouZ3Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "IgneouZ", 3);
                InstanceId opponentZedKitId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zed Kit", 1);
                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                   {
                       player => {},
                       opponent => {},
                       player => {},
                       opponent => {},
                       player => {},
                       opponent => {},
                       player =>
                       {
                           player.CardPlay(playerBoomerId, ItemPosition.Start);
                           player.CardPlay(playerBoomer2Id, ItemPosition.Start);
                           player.CardPlay(playerBoomer3Id, ItemPosition.Start);
                           player.CardPlay(playerIgneouZId, ItemPosition.Start);
                           player.CardPlay(playerIgneouZ2Id, ItemPosition.Start);
                           player.CardPlay(playerIgneouZ3Id, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentBoomerId, ItemPosition.Start);
                           opponent.CardPlay(opponentBoomer2Id, ItemPosition.Start);
                           opponent.CardPlay(opponentBoomer3Id, ItemPosition.Start);
                           opponent.CardPlay(opponentIgneouZId, ItemPosition.Start);
                           opponent.CardPlay(opponentIgneouZ2Id, ItemPosition.Start);
                           opponent.CardPlay(opponentIgneouZ3Id, ItemPosition.Start);
                       },
                       player =>
                       {
                           player.CardAttack(playerBoomerId, opponentIgneouZId);
                           player.CardAttack(playerBoomer2Id, opponentIgneouZ2Id);
                           player.CardAttack(playerBoomer3Id, opponentIgneouZ3Id);
                           player.LetsThink(1);
                           player.CardAttack(playerIgneouZId, opponentBoomerId);
                           player.CardAttack(playerIgneouZ2Id, opponentBoomer2Id);
                           player.CardAttack(playerIgneouZ3Id, opponentBoomer3Id);
                           player.LetsThink(4);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentZedKitId, ItemPosition.Start, null, true);
                           opponent.CardAbilityUsed(opponentZedKitId, Enumerators.AbilityType.HEAL, new List<ParametrizedAbilityInstanceId>(){
                               new ParametrizedAbilityInstanceId(opponentBoomerId),
                               new ParametrizedAbilityInstanceId(opponentBoomer2Id),
                           });
                           opponent.LetsThink(2);
                       },
                       player =>
                       {
                           player.CardPlay(playerZedKitId, ItemPosition.Start);
                           player.LetsThink(2);
                       },
                       opponent =>{},
                   };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(2, pvpTestContext.GetCurrentPlayer().CardsOnBoard.FindAll(card => card.CurrentDefense == 5).Count);
                    Assert.AreEqual(2, pvpTestContext.GetOpponentPlayer().CardsOnBoard.FindAll(card => card.CurrentDefense == 5).Count);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }
    }
}
