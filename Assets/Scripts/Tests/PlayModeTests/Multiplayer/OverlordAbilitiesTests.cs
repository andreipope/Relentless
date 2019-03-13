using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Loom.ZombieBattleground.Test.MultiplayerTests
{
    public class OverlordAbilitiesTests : BaseIntegrationTest
    {
        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator FireballOverlordAbility()
        {
            return AsyncTest(async () =>
            {
                Deck opponentDeck = new Deck(
                    0,
                    1,
                    "test deck",
                    new List<DeckCardData>
                    {
                        new DeckCardData("Slab", 30)
                    },
                    Enumerators.OverlordSkill.FIREBALL,
                    Enumerators.OverlordSkill.MASS_RABIES
                );

                Deck playerDeck = new Deck(
                    0,
                    1,
                    "test deck2",
                    new List<DeckCardData>
                    {
                        new DeckCardData("Slab", 30)
                    },
                    Enumerators.OverlordSkill.FIREBALL,
                    Enumerators.OverlordSkill.MASS_RABIES
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck) {
                    Player1HasFirstTurn = true
                };

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
                       opponent =>
                       {
                           opponent.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(pvpTestContext.GetCurrentPlayer().InstanceId)
                           });
                       },
                       player =>
                       {
                           Assert.AreEqual(18, pvpTestContext.GetCurrentPlayer().Defense);
                           player.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(pvpTestContext.GetOpponentPlayer().InstanceId)
                           });
                       },
                       opponent => Assert.AreEqual(18, pvpTestContext.GetOpponentPlayer().Defense),
                       player => {},
                   };

                Action validateEndState = () =>
                {

                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Reanimate()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 5,
                    Enumerators.OverlordSkill.REANIMATE,
                    Enumerators.OverlordSkill.NONE,
                    new DeckCardData("Puffer", 15));

                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 5,
                    Enumerators.OverlordSkill.REANIMATE,
                    Enumerators.OverlordSkill.NONE,
                    new DeckCardData("Puffer", 15));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerPufferId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Puffer", 1);
                InstanceId playerPuffer2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Puffer", 2);
                InstanceId playerPuffer3Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Puffer", 3);

                InstanceId opponentPufferId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Puffer", 1);
                InstanceId opponentPuffer2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Puffer", 2);
                InstanceId opponentPuffer3Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Puffer", 3);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                   {
                       player =>
                       {
                           player.CardPlay(playerPufferId, ItemPosition.Start);
                           player.CardPlay(playerPuffer2Id, ItemPosition.Start);
                           player.CardPlay(playerPuffer3Id, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentPufferId, ItemPosition.Start);
                           opponent.CardPlay(opponentPuffer2Id, ItemPosition.Start);
                           opponent.CardPlay(opponentPuffer3Id, ItemPosition.Start);
                       },
                       player => {},
                       opponent =>
                       {
                           opponent.CardAttack(opponentPufferId, playerPufferId);
                           opponent.CardAttack(opponentPuffer2Id, playerPuffer2Id);
                           opponent.CardAttack(opponentPuffer3Id, playerPuffer3Id);
                       },
                       player => {},
                       opponent => {},
                       player => {},
                       opponent => {},
                       player =>
                       {
                           player.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(playerPuffer3Id,
                                    new ParametrizedAbilityParameters()
                                    {
                                        CardName = playerPuffer3Id.Id.ToString()
                                    }),
                                new ParametrizedAbilityInstanceId(playerPufferId,
                                    new ParametrizedAbilityParameters()
                                    {
                                        CardName = playerPufferId.Id.ToString()
                                    })
                           });
                       },
                       opponent =>
                       {
                           opponent.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(opponentPuffer2Id,
                                    new ParametrizedAbilityParameters()
                                    {
                                        CardName = opponentPuffer2Id.Id.ToString()
                                    }),
                                new ParametrizedAbilityInstanceId(opponentPuffer3Id,
                                    new ParametrizedAbilityParameters()
                                    {
                                        CardName = opponentPuffer3Id.Id.ToString()
                                    })
                           });
                       },
                       player => {},
                       opponent => {},
                       };

                Action validateEndState = () =>
                {
                    Assert.NotNull(TestHelper.BattlegroundController.PlayerBoardCards.Select(card => card.Model.InstanceId == playerPuffer3Id));
                    Assert.NotNull(TestHelper.BattlegroundController.PlayerBoardCards.Select(card => card.Model.InstanceId == playerPufferId));
                    Assert.NotNull(TestHelper.BattlegroundController.OpponentBoardCards.Select(card => card.Model.InstanceId == opponentPuffer2Id));
                    Assert.NotNull(TestHelper.BattlegroundController.OpponentBoardCards.Select(card => card.Model.InstanceId == opponentPuffer3Id));
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator HealingTouch()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 5,
                    Enumerators.OverlordSkill.HEALING_TOUCH,
                    Enumerators.OverlordSkill.ENHANCE,
                    new DeckCardData("Slab", 15));

                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 5,
                    Enumerators.OverlordSkill.HEALING_TOUCH,
                    Enumerators.OverlordSkill.ENHANCE,
                    new DeckCardData("Slab", 15));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Slab", 1);

                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Slab", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                   {
                       player =>
                       {
                           player.CardPlay(playerCardId, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentCardId, ItemPosition.Start);
                       },
                       player =>
                       {
                           player.CardAttack(playerCardId, opponentCardId);
                       },
                       opponent => {},
                       player => {},
                       opponent => {},
                       player =>
                       {
                           player.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(playerCardId)
                           });
                       },
                       opponent =>
                       {
                           opponent.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(opponentCardId)
                           });
                       },
                       player => {},
                       opponent => {},
                   };

                Action validateEndState = () =>
                {
                    BoardUnitModel playerSlabUnit = (BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCardId);
                    BoardUnitModel opponentSlabUnit = (BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCardId);

                    Assert.AreEqual(playerSlabUnit.Card.Prototype.Defense, playerSlabUnit.CurrentHp);
                    Assert.AreEqual(opponentSlabUnit.Card.Prototype.Defense, opponentSlabUnit.CurrentHp);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Mend()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 5,
                    Enumerators.OverlordSkill.MEND,
                    Enumerators.OverlordSkill.ENHANCE,
                    new DeckCardData("Slab", 15));

                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 5,
                    Enumerators.OverlordSkill.MEND,
                    Enumerators.OverlordSkill.ENHANCE,
                    new DeckCardData("Slab", 15));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };


                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Slab", 1);

                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Slab", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                   {
                       player =>
                       {
                           player.CardPlay(playerCardId, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentCardId, ItemPosition.Start);
                       },
                       player =>
                       {
                           player.CardAttack(playerCardId, pvpTestContext.GetCurrentPlayer().InstanceId);
                       },
                       opponent =>
                       {
                           opponent.CardAttack(opponentCardId, pvpTestContext.GetOpponentPlayer().InstanceId);
                       },
                       player => {},
                       opponent => {},
                       player => {},
                       opponent => {},
                       player => {},
                       opponent => {},
                       player =>
                       {
                           player.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(pvpTestContext.GetCurrentPlayer().InstanceId)
                           });
                       },
                       opponent =>
                       {
                           opponent.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(pvpTestContext.GetOpponentPlayer().InstanceId)
                           });
                       },
                       player => {},
                       opponent => {},
                   };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(pvpTestContext.GetCurrentPlayer().InitialHp, pvpTestContext.GetCurrentPlayer().Defense);
                    Assert.AreEqual(pvpTestContext.GetOpponentPlayer().InitialHp, pvpTestContext.GetOpponentPlayer().Defense);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Ressurect()
        {
            return AsyncTest(async () =>
            {
            Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 5,
                Enumerators.OverlordSkill.RESSURECT,
                Enumerators.OverlordSkill.NONE,
                new DeckCardData("Puffer", 15));

            Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 5,
                Enumerators.OverlordSkill.RESSURECT,
                Enumerators.OverlordSkill.NONE,
                new DeckCardData("Puffer", 15));

            PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
            {
                Player1HasFirstTurn = true
            };

            InstanceId playerPufferId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Puffer", 1);
            InstanceId playerPuffer2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Puffer", 2);
            InstanceId playerPuffer3Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Puffer", 3);

            InstanceId opponentPufferId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Puffer", 1);
            InstanceId opponentPuffer2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Puffer", 2);
            InstanceId opponentPuffer3Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Puffer", 3);

            IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
               {
                       player =>
                       {
                           player.CardPlay(playerPufferId, ItemPosition.Start);
                           player.CardPlay(playerPuffer2Id, ItemPosition.Start);
                           player.CardPlay(playerPuffer3Id, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentPufferId, ItemPosition.Start);
                           opponent.CardPlay(opponentPuffer2Id, ItemPosition.Start);
                           opponent.CardPlay(opponentPuffer3Id, ItemPosition.Start);
                       },
                       player => {},
                       opponent =>
                       {
                           opponent.CardAttack(opponentPufferId, playerPufferId);
                           opponent.CardAttack(opponentPuffer2Id, playerPuffer2Id);
                           opponent.CardAttack(opponentPuffer3Id, playerPuffer3Id);
                       },
                       player => {},
                       opponent => {},
                       player => {},
                       opponent => {},
                       player =>
                       {
                           player.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(playerPuffer3Id,
                                    new ParametrizedAbilityParameters()
                                    {
                                        CardName = playerPuffer3Id.Id.ToString()
                                    })
                           });
                       },
                       opponent =>
                       {
                           opponent.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(opponentPuffer2Id,
                                    new ParametrizedAbilityParameters()
                                    {
                                        CardName = opponentPuffer2Id.Id.ToString()
                                    })
                           });
                       },
                       player => {},
                       opponent => {},
                   };

                Action validateEndState = () =>
                {
                    Assert.NotNull(TestHelper.BattlegroundController.PlayerBoardCards.Select(card => card.Model.InstanceId == playerPuffer3Id));
                    Assert.NotNull(TestHelper.BattlegroundController.OpponentBoardCards.Select(card => card.Model.InstanceId == opponentPuffer2Id));
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Enhance()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 5,
                    Enumerators.OverlordSkill.ENHANCE,
                    Enumerators.OverlordSkill.HEALING_TOUCH,
                    new DeckCardData("Keeper", 15));

                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 5,
                    Enumerators.OverlordSkill.ENHANCE,
                    Enumerators.OverlordSkill.HEALING_TOUCH,
                    new DeckCardData("Keeper", 15));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };


                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Keeper", 1);

                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Keeper", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                   {
                       player =>
                       {
                           player.CardPlay(playerCardId, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentCardId, ItemPosition.Start);
                       },
                       player =>
                       {
                           player.CardAttack(playerCardId, opponentCardId);
                       },
                       opponent => {},
                       player =>
                       {
                           player.CardAttack(playerCardId, pvpTestContext.GetOpponentPlayer().InstanceId);
                       },
                       opponent =>
                       {
                           opponent.CardAttack(opponentCardId, pvpTestContext.GetCurrentPlayer().InstanceId);
                       },
                       player => {},
                       opponent => {},
                       player => {},
                       opponent => {},
                       player =>
                       {
                           player.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(playerCardId),
                                new ParametrizedAbilityInstanceId(pvpTestContext.GetCurrentPlayer().InstanceId)
                           });
                       },
                       opponent =>
                       {
                           opponent.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(opponentCardId),
                                new ParametrizedAbilityInstanceId(pvpTestContext.GetOpponentPlayer().InstanceId)
                           });
                       },
                       player => {},
                       opponent => {},
                   };

                Action validateEndState = () =>
                {
                    BoardUnitModel playerUnit = (BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCardId);
                    BoardUnitModel opponentUnit = (BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCardId);

                    Assert.AreEqual(pvpTestContext.GetCurrentPlayer().InitialHp, pvpTestContext.GetCurrentPlayer().Defense);
                    Assert.AreEqual(pvpTestContext.GetOpponentPlayer().InitialHp, pvpTestContext.GetOpponentPlayer().Defense);
                    Assert.AreEqual(playerUnit.Card.Prototype.Defense, playerUnit.CurrentHp);
                    Assert.AreEqual(opponentUnit.Card.Prototype.Defense, opponentUnit.CurrentHp);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator PoisonDart()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 4,
                    Enumerators.OverlordSkill.POISON_DART,
                    Enumerators.OverlordSkill.TOXIC_POWER,
                    new DeckCardData("Hazmaz", 15));

                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 4,
                    Enumerators.OverlordSkill.POISON_DART,
                    Enumerators.OverlordSkill.TOXIC_POWER,
                    new DeckCardData("Hazmaz", 15));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Hazmaz", 1);

                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Hazmaz", 1);

                int difference = 1;

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                   {
                       player =>
                       {
                           player.CardPlay(playerCardId, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentCardId, ItemPosition.Start);
                       },
                       player => {},
                       opponent => {},
                       player => {},
                       opponent => {},
                       player =>
                       {
                           player.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(opponentCardId)
                           });
                       },
                       opponent =>
                       {
                           opponent.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(playerCardId)
                           });
                       },
                       player => {},
                       opponent => {},
                   };

                Action validateEndState = () =>
                {
                    BoardUnitModel playerUnit = (BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCardId);
                    BoardUnitModel opponentUnit = (BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCardId);

                    Assert.AreEqual(playerUnit.Card.Prototype.Defense - difference, playerUnit.CurrentHp);
                    Assert.AreEqual(opponentUnit.Card.Prototype.Defense - difference, opponentUnit.CurrentHp);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator ToxicPower()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 4,
                    Enumerators.OverlordSkill.TOXIC_POWER,
                    Enumerators.OverlordSkill.POISON_DART,
                    new DeckCardData("Hazmaz", 15));

                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 4,
                    Enumerators.OverlordSkill.TOXIC_POWER,
                    Enumerators.OverlordSkill.POISON_DART,
                    new DeckCardData("Hazmaz", 15));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Hazmaz", 1);

                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Hazmaz", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                   {
                       player =>
                       {
                           player.CardPlay(playerCardId, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentCardId, ItemPosition.Start);
                       },
                       player => {},
                       opponent => {},
                       player => {},
                       opponent => {},
                       player => {},
                       opponent => {},
                       player =>
                       {
                           player.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(playerCardId)
                           });
                           player.CardAttack(playerCardId, pvpTestContext.GetOpponentPlayer().InstanceId);
                       },
                       opponent =>
                       {
                           opponent.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(opponentCardId)
                           });
                           opponent.CardAttack(opponentCardId, pvpTestContext.GetCurrentPlayer().InstanceId);
                       },
                       player => {},
                       opponent => {},
                   };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(pvpTestContext.GetCurrentPlayer().InitialHp - 2, pvpTestContext.GetCurrentPlayer().Defense);
                    Assert.AreEqual(pvpTestContext.GetOpponentPlayer().InitialHp - 2, pvpTestContext.GetOpponentPlayer().Defense);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Push()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 3,
                    Enumerators.OverlordSkill.PUSH,
                    Enumerators.OverlordSkill.DRAW,
                    new DeckCardData("Bouncer", 1),
                    new DeckCardData("Whizpar", 15));

                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 3,
                    Enumerators.OverlordSkill.PUSH,
                    Enumerators.OverlordSkill.DRAW,
                    new DeckCardData("Bouncer", 1),
                    new DeckCardData("Whizpar", 15));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerBouncerId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Bouncer", 1);
                InstanceId playerWhizparId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Whizpar", 1);

                InstanceId opponentBouncerId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Bouncer", 1);
                InstanceId opponentWhizparId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Whizpar", 1);

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
                           player.CardPlay(playerBouncerId, ItemPosition.Start);
                           player.CardPlay(playerWhizparId, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentBouncerId, ItemPosition.Start);
                           opponent.CardPlay(opponentWhizparId, ItemPosition.Start);
                       },
                       player => {},
                       opponent => {},
                       player => {},
                       opponent => {},
                       player =>
                       {
                           player.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(playerBouncerId)
                           });
                       },
                       opponent =>
                       {
                           opponent.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(opponentBouncerId)
                           });
                       },
                       player => {},
                       opponent => {},
                   };

                Action validateEndState = () =>
                {
                    Assert.IsNotNull(pvpTestContext.GetCurrentPlayer().CardsInHand.Select(card => card.Card.Prototype.MouldId == 32));
                    Assert.IsNotNull(pvpTestContext.GetOpponentPlayer().CardsInHand.Select(card => card.Card.Prototype.MouldId == 32));
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Draw()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 3,
                    Enumerators.OverlordSkill.DRAW,
                    Enumerators.OverlordSkill.NONE,
                    new DeckCardData("Whizpar", 15));

                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 3,
                    Enumerators.OverlordSkill.DRAW,
                    Enumerators.OverlordSkill.NONE,
                    new DeckCardData("Whizpar", 15));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                int countPlayerCards = 0;
                int countOpponentCards = 0;

                InstanceId playerWhizparId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Whizpar", 1);
                InstanceId playerWhizpar2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Whizpar", 2);

                InstanceId opponentWhizparId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Whizpar", 1);
                InstanceId opponentWhizpar2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Whizpar", 2);

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
                           player.CardPlay(playerWhizparId, ItemPosition.Start);
                           player.CardPlay(playerWhizpar2Id, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentWhizparId, ItemPosition.Start);
                           opponent.CardPlay(opponentWhizpar2Id, ItemPosition.Start);
                       },
                       player =>
                       {
                           countPlayerCards = pvpTestContext.GetCurrentPlayer().CardsInHand.Count;
                           player.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>());
                       },
                       opponent =>
                       {
                           countOpponentCards = pvpTestContext.GetOpponentPlayer().CardsInHand.Count;
                           opponent.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>());
                       },
                       player =>
                       {
                           Assert.AreEqual(countPlayerCards + 2, pvpTestContext.GetCurrentPlayer().CardsInHand.Count);
                       },
                       opponent =>
                       {
                           Assert.AreEqual(countOpponentCards + 2, pvpTestContext.GetOpponentPlayer().CardsInHand.Count);
                       },
                   };

                Action validateEndState = () => {};

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Retreat()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 3,
                    Enumerators.OverlordSkill.RETREAT,
                    Enumerators.OverlordSkill.NONE,
                    new DeckCardData("Bouncer", 1),
                    new DeckCardData("Whizpar", 15));

            Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 3,
                    Enumerators.OverlordSkill.RETREAT,
                    Enumerators.OverlordSkill.NONE,
                    new DeckCardData("Bouncer", 1),
                    new DeckCardData("Whizpar", 15));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerBouncerId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Bouncer", 1);
                InstanceId playerWhizparId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Whizpar", 1);
                InstanceId playerWhizpar2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Whizpar", 2);
                InstanceId playerWhizpar3Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Whizpar", 3);

                InstanceId opponentBouncerId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Bouncer", 1);
                InstanceId opponentWhizparId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Whizpar", 1);
                InstanceId opponentWhizpar2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Whizpar", 2);
                InstanceId opponentWhizpar3Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Whizpar", 3);
                InstanceId opponentWhizpar4Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Whizpar", 4);
                InstanceId opponentWhizpar5Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Whizpar", 5);

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
                       player =>
                       {
                           player.CardPlay(playerBouncerId, ItemPosition.Start);
                           player.CardPlay(playerWhizparId, ItemPosition.Start);
                           player.CardPlay(playerWhizpar2Id, ItemPosition.Start);
                           player.CardPlay(playerWhizpar3Id, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentBouncerId, ItemPosition.Start);
                           opponent.CardPlay(opponentWhizparId, ItemPosition.Start);
                           opponent.CardPlay(opponentWhizpar2Id, ItemPosition.Start);
                           opponent.CardPlay(opponentWhizpar3Id, ItemPosition.Start);
                           opponent.CardPlay(opponentWhizpar4Id, ItemPosition.Start);
                           opponent.CardPlay(opponentWhizpar5Id, ItemPosition.Start);
                       },
                       player => {},
                       opponent =>
                       {
                           opponent.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>());
                       },
                       player => {},
                       opponent => {},
                   };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(0, TestHelper.BattlegroundController.PlayerBoardCards.Count);
                    Assert.AreEqual(0, TestHelper.BattlegroundController.OpponentBoardCards.Count);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Harden()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0,
                    Enumerators.OverlordSkill.HARDEN,
                    Enumerators.OverlordSkill.NONE,
                    new DeckCardData("Slab", 15));

                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    Enumerators.OverlordSkill.HARDEN,
                    Enumerators.OverlordSkill.NONE,
                    new DeckCardData("Slab", 15));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                int value = 2;

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
                       player =>
                       {
                           player.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(pvpTestContext.GetCurrentPlayer().InstanceId)
                           });
                       },
                       opponent =>
                       {
                           opponent.OverlordSkillUsed(new SkillId(0),  new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(pvpTestContext.GetOpponentPlayer().InstanceId)
                           });
                       },
                   };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(pvpTestContext.GetCurrentPlayer().InitialHp + value, pvpTestContext.GetCurrentPlayer().Defense);
                    Assert.AreEqual(pvpTestContext.GetOpponentPlayer().InitialHp + value, pvpTestContext.GetOpponentPlayer().Defense);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Stoneskin()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0,
                    Enumerators.OverlordSkill.STONE_SKIN,
                    Enumerators.OverlordSkill.NONE,
                    new DeckCardData("Slab", 15));

                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    Enumerators.OverlordSkill.STONE_SKIN,
                    Enumerators.OverlordSkill.NONE,
                    new DeckCardData("Slab", 15));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerSlabId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Slab", 1);
                InstanceId playerSlab2Id  = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Slab", 2);

                InstanceId opponentSlabId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Slab", 1);
                InstanceId opponentSlab2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Slab", 2);

                int value = 1;

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
                           player.CardPlay(playerSlabId, ItemPosition.Start);
                           player.CardPlay(playerSlab2Id, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentSlabId, ItemPosition.Start);
                           opponent.CardPlay(opponentSlab2Id, ItemPosition.Start);
                       },
                       player =>
                       {
                           player.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(playerSlabId)
                           });
                       },
                       opponent =>
                       {
                           opponent.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(opponentSlabId)
                           });
                       },
                       player => {},
                       opponent => {},
                   };

                Action validateEndState = () =>
                {
                    BoardUnitModel playerUnit = (BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerSlabId);
                    BoardUnitModel opponentUnit = (BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentSlabId);

                    Assert.AreEqual(playerUnit.Card.Prototype.Defense + value, playerUnit.CurrentHp);
                    Assert.AreEqual(opponentUnit.Card.Prototype.Defense + value, opponentUnit.CurrentHp);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Fortify()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0,
                    Enumerators.OverlordSkill.FORTIFY,
                    Enumerators.OverlordSkill.NONE,
                    new DeckCardData("Slab", 15));

                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    Enumerators.OverlordSkill.FORTIFY,
                    Enumerators.OverlordSkill.NONE,
                    new DeckCardData("Slab", 15));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerSlabId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Slab", 1);
                InstanceId playerSlab2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Slab", 2);

                InstanceId opponentSlabId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Slab", 1);
                InstanceId opponentSlab2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Slab", 2);

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
                           player.CardPlay(playerSlabId, ItemPosition.Start);
                           player.CardPlay(playerSlab2Id, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentSlabId, ItemPosition.Start);
                           opponent.CardPlay(opponentSlab2Id, ItemPosition.Start);
                       },
                       player =>
                       {
                           player.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(playerSlabId)
                           });
                       },
                       opponent =>
                       {
                           opponent.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(opponentSlabId)
                           });
                       },
                       player => {},
                       opponent => {},
                   };

                Action validateEndState = () =>
                {
                    Assert.IsTrue(((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerSlabId)).IsHeavyUnit);
                    Assert.IsTrue(((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentSlabId)).IsHeavyUnit);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Phalanx()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0,
                    Enumerators.OverlordSkill.PHALANX,
                    Enumerators.OverlordSkill.NONE,
                    new DeckCardData("Slab", 15));

                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    Enumerators.OverlordSkill.PHALANX,
                    Enumerators.OverlordSkill.NONE,
                    new DeckCardData("Slab", 15));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerSlabId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Slab", 1);
                InstanceId playerSlab2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Slab", 2);
                InstanceId playerSlab3Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Slab", 3);

                InstanceId opponentSlabId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Slab", 1);
                InstanceId opponentSlab2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Slab", 2);
                InstanceId opponentSlab3Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Slab", 3);

                int value = 1;

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
                           player.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(playerSlabId),
                                new ParametrizedAbilityInstanceId(playerSlab2Id),
                                new ParametrizedAbilityInstanceId(playerSlab3Id),
                           });
                       },
                       opponent =>
                       {
                           opponent.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(opponentSlabId),
                                new ParametrizedAbilityInstanceId(opponentSlab2Id),
                                new ParametrizedAbilityInstanceId(opponentSlab3Id),
                           });
                       },
                       player => {},
                       opponent => {},
                   };

                Action validateEndState = () =>
                {
                    BoardUnitModel playerSlab = (BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerSlabId);
                    BoardUnitModel playerSlab2 = (BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerSlab2Id);
                    BoardUnitModel playerSlab3 = (BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerSlab3Id);

                    BoardUnitModel opponentSlab = (BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentSlabId);
                    BoardUnitModel opponentSlab2 = (BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentSlab2Id);
                    BoardUnitModel opponentSlab3 = (BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentSlab3Id);

                    Assert.AreEqual(playerSlab.Card.Prototype.Defense + value, playerSlab.CurrentHp);
                    Assert.AreEqual(playerSlab2.Card.Prototype.Defense + value, playerSlab2.CurrentHp);
                    Assert.AreEqual(playerSlab3.Card.Prototype.Defense + value, playerSlab3.CurrentHp);

                    Assert.AreEqual(opponentSlab.Card.Prototype.Defense + value, opponentSlab.CurrentHp);
                    Assert.AreEqual(opponentSlab2.Card.Prototype.Defense + value, opponentSlab2.CurrentHp);
                    Assert.AreEqual(opponentSlab3.Card.Prototype.Defense + value, opponentSlab3.CurrentHp);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Freeze()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 2,
                    Enumerators.OverlordSkill.FREEZE,
                    Enumerators.OverlordSkill.NONE,
                    new DeckCardData("Znowy", 15));

                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 2,
                    Enumerators.OverlordSkill.FREEZE,
                    Enumerators.OverlordSkill.NONE,
                    new DeckCardData("Znowy", 15));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerZnowyId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Znowy", 1);
                InstanceId playerZnowy2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Znowy", 2);

                InstanceId opponentZnowyId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Znowy", 1);
                InstanceId opponentZnowy2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Znowy", 2);

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
                           player.CardPlay(playerZnowyId, ItemPosition.Start);
                           player.CardPlay(playerZnowy2Id, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentZnowyId, ItemPosition.Start);
                           opponent.CardPlay(opponentZnowy2Id, ItemPosition.Start);
                       },
                       player =>
                       {
                           player.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(opponentZnowyId)
                           });
                       },
                       opponent =>
                       {
                           opponent.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(playerZnowyId)
                           });
                       },
                   };

                Action validateEndState = () =>
                {
                    Assert.IsTrue(((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentZnowyId)).UnitStatus == Enumerators.UnitStatusType.FROZEN);
                    Assert.IsTrue(((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerZnowyId)).UnitStatus == Enumerators.UnitStatusType.FROZEN);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator IceBolt()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 2,
                    Enumerators.OverlordSkill.ICE_BOLT,
                    Enumerators.OverlordSkill.NONE,
                    new DeckCardData("Slab", 15));

                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 2,
                    Enumerators.OverlordSkill.ICE_BOLT,
                    Enumerators.OverlordSkill.NONE,
                    new DeckCardData("Slab", 15));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerSlabId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Slab", 1);
                InstanceId playerSlab2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Slab", 2);

                InstanceId opponentSlabId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Slab", 1);
                InstanceId opponentSlab2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Slab", 2);

                int difference = 2;

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
                           player.CardPlay(playerSlabId, ItemPosition.Start);
                           player.CardPlay(playerSlab2Id, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentSlabId, ItemPosition.Start);
                           opponent.CardPlay(opponentSlab2Id, ItemPosition.Start);
                       },
                       player =>
                       {
                           player.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(opponentSlabId)
                           });
                       },
                       opponent =>
                       {
                           opponent.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(playerSlabId)
                           });
                       },
                   };

                Action validateEndState = () =>
                {
                    BoardUnitModel playerUnit1 = (BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerSlabId);
                    BoardUnitModel opponentUnit1 = (BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerSlabId);

                    Assert.IsTrue(playerUnit1.IsStun);
                    Assert.AreEqual(playerUnit1.Card.Prototype.Defense - difference, playerUnit1.CurrentHp);

                    Assert.IsTrue(opponentUnit1.IsStun);
                    Assert.AreEqual(opponentUnit1.Card.Prototype.Defense - difference, opponentUnit1.CurrentHp);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator IceWall()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 2,
                    Enumerators.OverlordSkill.ICE_WALL,
                    Enumerators.OverlordSkill.NONE,
                    new DeckCardData("Znowy", 15));

                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 2,
                    Enumerators.OverlordSkill.ICE_WALL,
                    Enumerators.OverlordSkill.NONE,
                    new DeckCardData("Znowy", 15));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerZnowyId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Znowy", 1);
                InstanceId playerZnowy2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Znowy", 2);

                InstanceId opponentZnowyId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Znowy", 1);
                InstanceId opponentZnowy2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Znowy", 2);

                int buffedHealth = 2;

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
                           player.CardPlay(playerZnowyId, ItemPosition.Start);
                           player.CardPlay(playerZnowy2Id, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentZnowyId, ItemPosition.Start);
                           opponent.CardPlay(opponentZnowy2Id, ItemPosition.Start);
                       },
                       player =>
                       {
                           player.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(playerZnowyId)
                           });
                       },
                       opponent =>
                       {
                           opponent.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(pvpTestContext.GetOpponentPlayer().InstanceId)
                           });
                       },
                       player => {},
                       opponent => {},
                   };

                Action validateEndState = () =>
                {
                    BoardUnitModel playerUnit = (BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerZnowyId);

                    Assert.AreEqual(playerUnit.Card.Prototype.Defense + buffedHealth, playerUnit.CurrentHp);
                    Assert.AreEqual(pvpTestContext.GetOpponentPlayer().InitialHp + buffedHealth, pvpTestContext.GetOpponentPlayer().Defense);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Shatter()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 2,
                    Enumerators.OverlordSkill.FREEZE,
                    Enumerators.OverlordSkill.SHATTER,
                    new DeckCardData("Slab", 15));

                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 2,
                    Enumerators.OverlordSkill.FREEZE,
                    Enumerators.OverlordSkill.SHATTER,
                    new DeckCardData("Slab", 15));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerSlabId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Slab", 1);
                InstanceId playerSlab2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Slab", 2);

                InstanceId opponentSlabId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Slab", 1);
                InstanceId opponentSlab2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Slab", 2);

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
                           player.CardPlay(playerSlabId, ItemPosition.Start);
                           player.CardPlay(playerSlab2Id, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentSlabId, ItemPosition.Start);
                           opponent.CardPlay(opponentSlab2Id, ItemPosition.Start);
                       },
                       player =>
                       {
                           player.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(opponentSlabId)
                           });
                           player.OverlordSkillUsed(new SkillId(1), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(opponentSlabId)
                           });
                       },
                       opponent =>
                       {
                           opponent.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(playerSlabId)
                           });
                           opponent.OverlordSkillUsed(new SkillId(1), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(playerSlabId)
                           });
                       },
                       player => {},
                       opponent => {},
                       player => {},
                       opponent => {},
                   };

                Action validateEndState = () =>
                {
                    Assert.Null((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerSlabId));
                    Assert.Null((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentSlabId));
                    Assert.AreEqual(1, TestHelper.BattlegroundController.PlayerBoardCards.Count);
                    Assert.AreEqual(1, TestHelper.BattlegroundController.OpponentBoardCards.Count);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator MeteorShower()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 1,
                    Enumerators.OverlordSkill.METEOR_SHOWER,
                    Enumerators.OverlordSkill.NONE,
                    new DeckCardData("Slab", 15));

                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 1,
                    Enumerators.OverlordSkill.METEOR_SHOWER,
                    Enumerators.OverlordSkill.NONE,
                    new DeckCardData("Slab", 15));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                int count = 3;
                int difference = 2;

                InstanceId[] playerCardsId = new InstanceId[count];
                InstanceId[] opponentCardsId = new InstanceId[count];

                for (int i = 0; i < count; i++)
                {
                    playerCardsId[i] = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Slab", i + 1);
                }

                for (int i = 0; i < count; i++)
                {
                    opponentCardsId[i] = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Slab", i + 1);
                }

                List<ParametrizedAbilityInstanceId> targets = new List<ParametrizedAbilityInstanceId>();
                targets.AddRange(playerCardsId.Select(id => new ParametrizedAbilityInstanceId(id, null)));
                targets.AddRange(opponentCardsId.Select(id => new ParametrizedAbilityInstanceId(id, null)));

                BoardUnitModel[] units = new BoardUnitModel[count * 2];

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
                           for (int i = 0; i < playerCardsId.Length; i++)
                           {
                               player.CardPlay(playerCardsId[i], ItemPosition.Start);
                           }
                       },
                       opponent =>
                       {
                           for (int i = 0; i < opponentCardsId.Length; i++)
                           {
                               opponent.CardPlay(opponentCardsId[i], ItemPosition.Start);
                           }
                       },
                       player =>
                       {
                           player.OverlordSkillUsed(new SkillId(0), targets);
                       },
                       player => {},
                       opponent => {},
                       opponent =>
                       {
                           
                           for (int i = 0; i < count; i++)
                           {
                               units[i] = (BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCardsId[i]);
                               units[count + i] = (BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCardsId[i]);

                               Assert.AreEqual(units[i].Card.Prototype.Defense - difference, units[i].CurrentHp);
                               Assert.AreEqual(units[count + i].Card.Prototype.Defense - difference, units[i].CurrentHp);
                           }
                           opponent.OverlordSkillUsed(new SkillId(0), targets);
                       },
                       player => {},
                       opponent => {},
                       player => {},
                       opponent => {},
                   };

                Action validateEndState = () =>
                {
                    for (int i = 0; i < units.Length; i++)
                    {
                        Assert.AreEqual(0, units[i].CurrentHp);
                    }
                    Assert.AreEqual(0, TestHelper.BattlegroundController.PlayerBoardCards.Count);
                    Assert.AreEqual(0, TestHelper.BattlegroundController.OpponentBoardCards.Count);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Rabies()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 1,
                    Enumerators.OverlordSkill.RABIES,
                    Enumerators.OverlordSkill.NONE,
                    new DeckCardData("Pyromaz", 15));

                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 1,
                    Enumerators.OverlordSkill.RABIES,
                    Enumerators.OverlordSkill.NONE,
                    new DeckCardData("Pyromaz", 15));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerPyromazId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Pyromaz", 1);
                InstanceId playerPyromaz2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Pyromaz", 2);

                InstanceId opponentPyromazId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Pyromaz", 1);
                InstanceId opponentPyromaz2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Pyromaz", 2);

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
                           player.CardPlay(playerPyromazId, ItemPosition.Start);
                           player.CardPlay(playerPyromaz2Id, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentPyromazId, ItemPosition.Start);
                           opponent.CardPlay(opponentPyromaz2Id, ItemPosition.Start);
                       },
                       player =>
                       {
                           player.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(playerPyromazId)
                           });
                       },
                       opponent =>
                       {
                           opponent.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(opponentPyromazId)
                           });
                       },
                       player => {},
                       opponent => {},
                   };

                Action validateEndState = () =>
                {
                    Assert.IsTrue(((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerPyromazId)).HasFeral);
                    Assert.IsTrue(((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentPyromazId)).HasFeral);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator FireBolt()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 1,
                    Enumerators.OverlordSkill.FIRE_BOLT,
                    Enumerators.OverlordSkill.NONE,
                    new DeckCardData("Slab", 15));

                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 1,
                    Enumerators.OverlordSkill.FIRE_BOLT,
                    Enumerators.OverlordSkill.NONE,
                    new DeckCardData("Slab", 15));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                int difference = 1;

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Slab", 1);

                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Slab", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                   {
                       player =>
                       {
                           player.CardPlay(playerCardId, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentCardId, ItemPosition.Start);
                       },
                       player => {},
                       opponent => {},
                       player => {},
                       opponent => {},
                       player =>
                       {
                           player.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(opponentCardId)
                           });
                       },
                       opponent =>
                       {
                           opponent.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(playerCardId)
                           });
                       },
                       player => {},
                       opponent => {},
                   };

                Action validateEndState = () =>
                {
                    BoardUnitModel playerUnit = (BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCardId);
                    BoardUnitModel opponentUnit = (BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCardId);
                    Assert.AreEqual(playerUnit.Card.Prototype.Defense - difference, playerUnit.CurrentHp);
                    Assert.AreEqual(opponentUnit.Card.Prototype.Defense - difference, opponentUnit.CurrentHp);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Fortress()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0,
                    Enumerators.OverlordSkill.FORTRESS,
                    Enumerators.OverlordSkill.NONE,
                    new DeckCardData("Slab", 15));

                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    Enumerators.OverlordSkill.FORTRESS,
                    Enumerators.OverlordSkill.NONE,
                    new DeckCardData("Slab", 15));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerSlabId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Slab", 1);
                InstanceId playerSlab2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Slab", 2);

                InstanceId opponentSlabId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Slab", 1);
                InstanceId opponentSlab2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Slab", 2);
                InstanceId opponentSlab3Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Slab", 3);

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
                           player.CardPlay(playerSlabId, ItemPosition.Start);
                           player.CardPlay(playerSlab2Id, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentSlabId, ItemPosition.Start);
                           opponent.CardPlay(opponentSlab2Id, ItemPosition.Start);
                           opponent.CardPlay(opponentSlab3Id, ItemPosition.Start);
                       },
                       player =>
                       {
                           player.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(playerSlabId),
                                new ParametrizedAbilityInstanceId(playerSlab2Id)
                           });

                       },
                       opponent =>
                       {
                           opponent.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(opponentSlabId),
                                new ParametrizedAbilityInstanceId(opponentSlab3Id)
                           });
                       },
                       player => {},
                       opponent => {},
                   };

                Action validateEndState = () =>
                {
                    Assert.IsTrue(((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerSlabId)).IsHeavyUnit);
                    Assert.IsTrue(((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerSlab2Id)).IsHeavyUnit);
                    Assert.IsTrue(((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentSlabId)).IsHeavyUnit);
                    Assert.IsTrue(!((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentSlab2Id)).IsHeavyUnit);
                    Assert.IsTrue(((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentSlab3Id)).IsHeavyUnit);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Blizzard()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 2,
                    Enumerators.OverlordSkill.BLIZZARD,
                    Enumerators.OverlordSkill.NONE,
                    new DeckCardData("Slab", 15));

                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 2,
                    Enumerators.OverlordSkill.BLIZZARD,
                    Enumerators.OverlordSkill.NONE,
                    new DeckCardData("Slab", 15));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerSlabId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Slab", 1);
                InstanceId playerSlab2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Slab", 2);
                InstanceId playerSlab3Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Slab", 3);

                InstanceId opponentSlabId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Slab", 1);
                InstanceId opponentSlab2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Slab", 2);
                InstanceId opponentSlab3Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Slab", 3);

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
                           player.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(opponentSlabId),
                                new ParametrizedAbilityInstanceId(opponentSlab3Id)
                           });
                       },
                       opponent =>
                       {
                           opponent.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(playerSlabId),
                                new ParametrizedAbilityInstanceId(playerSlab3Id)
                           });
                       },
                   };

                Action validateEndState = () =>
                {
                    Assert.IsTrue(((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerSlabId)).IsStun);
                    Assert.IsTrue(!((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerSlab2Id)).IsStun);
                    Assert.IsTrue(((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerSlab3Id)).IsStun);
                    Assert.IsTrue(((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentSlabId)).IsStun);
                    Assert.IsTrue(!((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentSlab2Id)).IsStun);
                    Assert.IsTrue(((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentSlab3Id)).IsStun);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator WindShield()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 3,
                    Enumerators.OverlordSkill.WIND_SHIELD,
                    Enumerators.OverlordSkill.NONE,
                    new DeckCardData("Whizpar", 15));

                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 3,
                    Enumerators.OverlordSkill.WIND_SHIELD,
                    Enumerators.OverlordSkill.NONE,
                    new DeckCardData("Whizpar", 15));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerWhizparId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Whizpar", 1);
                InstanceId playerWhizpar2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Whizpar", 2);

                InstanceId opponentWhizparId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Whizpar", 1);
                InstanceId opponentWhizpar2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Whizpar", 2);

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
                           player.CardPlay(playerWhizparId, ItemPosition.Start);
                           player.CardPlay(playerWhizpar2Id, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentWhizparId, ItemPosition.Start);
                           opponent.CardPlay(opponentWhizpar2Id, ItemPosition.Start);
                       },
                       player =>
                       {
                           player.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(playerWhizparId)
                           });
                       },
                       opponent =>
                       {
                           opponent.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(opponentWhizpar2Id)
                           });
                       },
                       player => {},
                       opponent => {},
                   };

                Action validateEndState = () =>
                {
                    Assert.IsTrue(((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerWhizparId)).HasBuffShield);
                    Assert.IsTrue(!((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerWhizpar2Id)).HasBuffShield);
                    Assert.IsTrue(((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentWhizpar2Id)).HasBuffShield);
                    Assert.IsTrue(!((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentWhizparId)).HasBuffShield);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Levitate()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 3,
                    Enumerators.OverlordSkill.LEVITATE,
                    Enumerators.OverlordSkill.NONE,
                    new DeckCardData("Whizpar", 15));

                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 3,
                    Enumerators.OverlordSkill.LEVITATE,
                    Enumerators.OverlordSkill.NONE,
                    new DeckCardData("Whizpar", 15));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerWhizparId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Whizpar", 1);
                InstanceId playerWhizpar2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Whizpar", 2);

                InstanceId opponentWhizparId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Whizpar", 1);
                InstanceId opponentWhizpar2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Whizpar", 2);

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
                       player =>
                       {
                           player.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(playerWhizparId,
                                    new ParametrizedAbilityParameters()
                                        {
                                            CardName = playerWhizparId.Id.ToString()
                                        }
                                    )
                           });
                       },
                       opponent =>
                       {
                           opponent.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(opponentWhizpar2Id,
                                    new ParametrizedAbilityParameters()
                                        {
                                            CardName = opponentWhizpar2Id.Id.ToString()
                                        }
                                    )
                           });
                       },
                       player => {},
                       opponent => {},
                   };

                Action validateEndState = () =>
                {
                    BoardUnitModel playerCardInHand = TestHelper.BattlegroundController.GetBoardUnitModelByInstanceId(playerWhizparId);
                    BoardUnitModel opponentCardInHand = TestHelper.BattlegroundController.GetBoardUnitModelByInstanceId(opponentWhizpar2Id);

                    Assert.AreEqual(playerCardInHand.Card.Prototype.Cost - 1, playerCardInHand.Card.InstanceCard.Cost);
                    Assert.AreEqual(opponentCardInHand.Card.Prototype.Cost - 1, playerCardInHand.Card.InstanceCard.Cost);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Breakout()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 4,
                    Enumerators.OverlordSkill.BREAKOUT,
                    Enumerators.OverlordSkill.NONE,
                    new DeckCardData("Slab", 15));

                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 4,
                    Enumerators.OverlordSkill.BREAKOUT,
                    Enumerators.OverlordSkill.NONE,
                    new DeckCardData("Slab", 15));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerSlabId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Slab", 1);
                InstanceId playerSlab2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Slab", 2);
                InstanceId playerSlab3Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Slab", 3);

                InstanceId opponentSlabId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Slab", 1);
                InstanceId opponentSlab2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Slab", 2);
                InstanceId opponentSlab3Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Slab", 3);

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
                           player.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(opponentSlabId,
                                    new ParametrizedAbilityParameters()
                                    {
                                        Attack = 2
                                    }),
                                new ParametrizedAbilityInstanceId(opponentSlab3Id,
                                    new ParametrizedAbilityParameters()
                                    {
                                        Attack = 2
                                    }),
                                new ParametrizedAbilityInstanceId(pvpTestContext.GetOpponentPlayer().InstanceId,
                                    new ParametrizedAbilityParameters()
                                    {
                                        Attack = 1
                                    })
                           });
                       },
                       opponent =>
                       {
                           opponent.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(playerSlabId,
                                    new ParametrizedAbilityParameters()
                                    {
                                        Attack = 1
                                    }),
                                new ParametrizedAbilityInstanceId(playerSlab3Id,
                                    new ParametrizedAbilityParameters()
                                    {
                                        Attack = 1
                                    }),
                                new ParametrizedAbilityInstanceId(pvpTestContext.GetCurrentPlayer().InstanceId,
                                    new ParametrizedAbilityParameters()
                                    {
                                        Attack = 3
                                    }),
                           });
                       },
                   };

                Action validateEndState = () =>
                {
                    BoardUnitModel playerUnit1 = (BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerSlabId);
                    BoardUnitModel playerUnit3 = (BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerSlab3Id);

                    BoardUnitModel opponentUnit1 = (BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentSlabId);
                    BoardUnitModel opponentUnit3 = (BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentSlab3Id);

                    Assert.AreEqual(playerUnit1.Card.Prototype.Defense - 1, playerUnit1.CurrentHp);
                    Assert.AreEqual(playerUnit3.Card.Prototype.Defense - 1, playerUnit3.CurrentHp);
                    Assert.AreEqual(pvpTestContext.GetCurrentPlayer().InitialHp - 3, pvpTestContext.GetCurrentPlayer().Defense);

                    Assert.AreEqual(opponentUnit1.Card.Prototype.Defense - 2, opponentUnit1.CurrentHp);
                    Assert.AreEqual(opponentUnit3.Card.Prototype.Defense - 2, opponentUnit3.CurrentHp);
                    Assert.AreEqual(pvpTestContext.GetOpponentPlayer().InitialHp - 1, pvpTestContext.GetOpponentPlayer().Defense);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator MassRabies()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 1,
                    Enumerators.OverlordSkill.MASS_RABIES,
                    Enumerators.OverlordSkill.NONE,
                    new DeckCardData("Pyromaz", 15));

                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 1,
                    Enumerators.OverlordSkill.MASS_RABIES,
                    Enumerators.OverlordSkill.NONE,
                    new DeckCardData("Pyromaz", 15));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerPyromazId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Pyromaz", 1);
                InstanceId playerPyromaz2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Pyromaz", 2);
                InstanceId playerPyromaz3Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Pyromaz", 3);

                InstanceId opponentPyromazId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Pyromaz", 1);
                InstanceId opponentPyromaz2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Pyromaz", 2);
                InstanceId opponentPyromaz3Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Pyromaz", 3);

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
                           player.CardPlay(playerPyromazId, ItemPosition.Start);
                           player.CardPlay(playerPyromaz2Id, ItemPosition.Start);
                           player.CardPlay(playerPyromaz3Id, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentPyromazId, ItemPosition.Start);
                           opponent.CardPlay(opponentPyromaz2Id, ItemPosition.Start);
                           opponent.CardPlay(opponentPyromaz3Id, ItemPosition.Start);
                       },
                       player =>
                       {
                           player.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(playerPyromazId),
                                new ParametrizedAbilityInstanceId(playerPyromaz2Id)
                           });

                       },
                       opponent =>
                       {
                           opponent.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(opponentPyromazId),
                                new ParametrizedAbilityInstanceId(opponentPyromaz3Id)
                           });
                       },
                       player => {},
                       opponent => {},
                   };

                Action validateEndState = () =>
                {
                    Assert.IsTrue(((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerPyromazId)).HasFeral);
                    Assert.IsTrue(((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerPyromaz2Id)).HasFeral);
                    Assert.IsTrue(!((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerPyromaz3Id)).HasFeral);
                    Assert.IsTrue(((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentPyromazId)).HasFeral);
                    Assert.IsTrue(!((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentPyromaz2Id)).HasFeral);
                    Assert.IsTrue(((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentPyromaz3Id)).HasFeral);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Infect()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 4,
                    Enumerators.OverlordSkill.INFECT,
                    Enumerators.OverlordSkill.NONE,
                    new DeckCardData("Hazmaz", 15));

                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 4,
                    Enumerators.OverlordSkill.INFECT,
                    Enumerators.OverlordSkill.NONE,
                    new DeckCardData("Hazmaz", 15));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerHazmazId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Hazmaz", 1);
                InstanceId playerHazmaz2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Hazmaz", 2);
                InstanceId playerHazmaz3Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Hazmaz", 3);

                InstanceId opponentHazmazId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Hazmaz", 1);
                InstanceId opponentHazmaz2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Hazmaz", 2);
                InstanceId opponentHazmaz3Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Hazmaz", 3);

                int playerDifference = 0;
                int opponentDiference = 0;

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
                           player.CardPlay(playerHazmazId, ItemPosition.Start);
                           player.CardPlay(playerHazmaz2Id, ItemPosition.Start);
                           player.CardPlay(playerHazmaz3Id, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentHazmazId, ItemPosition.Start);
                           opponent.CardPlay(opponentHazmaz2Id, ItemPosition.Start);
                           opponent.CardPlay(opponentHazmaz3Id, ItemPosition.Start);
                       },
                       player =>
                       {
                           playerDifference = ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerHazmazId)).CurrentDamage;
                           player.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(playerHazmazId),
                                new ParametrizedAbilityInstanceId(opponentHazmaz3Id),
                           });
                       },
                       opponent =>
                       {
                           opponentDiference = ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentHazmaz2Id)).CurrentDamage;
                           opponent.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(opponentHazmaz2Id),
                                new ParametrizedAbilityInstanceId(playerHazmaz3Id)
                           });
                       },
                       player => {},
                       opponent => {},
                   };

                Action validateEndState = () =>
                {
                    BoardUnitModel playerUnit3 = (BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerHazmaz3Id);
                    BoardUnitModel opponentUnit3 = (BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentHazmaz3Id);

                    Assert.AreEqual(playerUnit3.Card.Prototype.Defense - opponentDiference, playerUnit3.CurrentHp);
                    Assert.AreEqual(opponentUnit3.Card.Prototype.Defense - playerDifference, opponentUnit3.CurrentHp);
                    Assert.Null((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerHazmazId));
                    Assert.Null((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentHazmaz2Id));
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Epidemic()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 4,
                    Enumerators.OverlordSkill.EPIDEMIC,
                    Enumerators.OverlordSkill.NONE,
                    new DeckCardData("Hazmaz", 15));

                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 4,
                    Enumerators.OverlordSkill.EPIDEMIC,
                    Enumerators.OverlordSkill.NONE,
                    new DeckCardData("Hazmaz", 15));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerHazmazId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Hazmaz", 1);
                InstanceId playerHazmaz2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Hazmaz", 2);
                InstanceId playerHazmaz3Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Hazmaz", 3);
                InstanceId playerHazmaz4Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Hazmaz", 4);
                InstanceId playerHazmaz5Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Hazmaz", 5);

                InstanceId opponentHazmazId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Hazmaz", 1);
                InstanceId opponentHazmaz2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Hazmaz", 2);
                InstanceId opponentHazmaz3Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Hazmaz", 3);
                InstanceId opponentHazmaz4Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Hazmaz", 4);
                InstanceId opponentHazmaz5Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Hazmaz", 5);

                int playerDifference1 = 0;
                int playerDifference2 = 0;

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
                           player.CardPlay(playerHazmazId, ItemPosition.Start);
                           player.CardPlay(playerHazmaz2Id, ItemPosition.Start);
                           player.CardPlay(playerHazmaz3Id, ItemPosition.Start);
                           player.CardPlay(playerHazmaz4Id, ItemPosition.Start);
                           player.CardPlay(playerHazmaz5Id, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentHazmazId, ItemPosition.Start);
                           opponent.CardPlay(opponentHazmaz2Id, ItemPosition.Start);
                           opponent.CardPlay(opponentHazmaz3Id, ItemPosition.Start);
                           opponent.CardPlay(opponentHazmaz4Id, ItemPosition.Start);
                           opponent.CardPlay(opponentHazmaz5Id, ItemPosition.Start);
                       },
                       player =>
                       {
                           playerDifference1 = ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerHazmazId)).CurrentDamage;
                           playerDifference2 = ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerHazmaz4Id)).CurrentDamage;
                           player.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(playerHazmazId,
                                    new ParametrizedAbilityParameters()
                                    {
                                        CardName = opponentHazmaz3Id.Id.ToString()
                                    }),
                                new ParametrizedAbilityInstanceId(playerHazmaz4Id,
                                    new ParametrizedAbilityParameters()
                                    {
                                        CardName = opponentHazmazId.Id.ToString()
                                    })
                           });
                       },
                       opponent =>
                       {
                           opponent.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(opponentHazmaz2Id,
                                    new ParametrizedAbilityParameters()
                                    {
                                        CardName = playerHazmaz3Id.Id.ToString()
                                    }),
                                new ParametrizedAbilityInstanceId(opponentHazmaz4Id,
                                    new ParametrizedAbilityParameters()
                                    {
                                        CardName = playerHazmaz3Id.Id.ToString()
                                    }),
                           });
                       },
                       player => {},
                       opponent => {},
                   };

                Action validateEndState = () =>
                {
                    BoardUnitModel opponentUnit1 = (BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentHazmazId);
                    BoardUnitModel opponentUnit3 = (BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentHazmaz3Id);

                    Assert.AreEqual(opponentUnit3.Card.Prototype.Defense - playerDifference1, opponentUnit3.CurrentHp);
                    Assert.AreEqual(opponentUnit1.Card.Prototype.Defense - playerDifference2, opponentUnit1.CurrentHp);
                    Assert.Null((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerHazmazId));
                    Assert.Null((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerHazmaz3Id));
                    Assert.Null((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerHazmaz4Id));
                    Assert.Null((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentHazmaz2Id));
                    Assert.Null((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentHazmaz4Id));
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }
    }
}
