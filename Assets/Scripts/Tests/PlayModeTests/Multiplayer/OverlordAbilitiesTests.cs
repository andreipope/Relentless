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
        public IEnumerator MassRabiesOverlordAbility()
        {
            return AsyncTest(async () =>
            {
                Deck opponentDeck = new Deck(
                    0,
                    1,
                    "test deck",
                    new List<DeckCardData>
                    {
                        new DeckCardData("Pyromaz", 30)
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
                        new DeckCardData("Pyromaz", 30)
                    },
                    Enumerators.OverlordSkill.FIREBALL,
                    Enumerators.OverlordSkill.MASS_RABIES
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck) {
                    Player1HasFirstTurn = true
                };


                InstanceId playerPyromaz1Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Pyromaz", 1);
                InstanceId playerPyromaz2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Pyromaz", 2);
                InstanceId playerPyromaz3Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Pyromaz", 3);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                   {
                       player =>
                       {
                           player.CardPlay(playerPyromaz1Id, ItemPosition.End);
                           player.CardPlay(playerPyromaz2Id, ItemPosition.End);
                           player.CardPlay(playerPyromaz3Id, ItemPosition.End);
                       },
                       opponent => {},
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
                           player.OverlordSkillUsed(new SkillId(1), null);
                       },
                       opponent =>
                       {
                           int numCardsWithFeral =
                               new[]
                                   {
                                       playerPyromaz1Id, playerPyromaz2Id, playerPyromaz3Id
                                   }
                                   .Select(id => (BoardUnitModel) TestHelper.BattlegroundController.GetBoardObjectByInstanceId(id))
                                   .Select(model => model.HasFeral)
                                   .Count();

                           Assert.AreEqual(2, numCardsWithFeral);
                       },
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

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Puffer", 1);
                InstanceId playerCardId2 = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Puffer", 2);
                InstanceId playerCardId3 = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Puffer", 3);

                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Puffer", 1);
                InstanceId opponentCardId2 = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Puffer", 2);
                InstanceId opponentCardId3 = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Puffer", 3);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                   {
                       player =>
                       {
                           player.CardPlay(playerCardId, ItemPosition.Start);
                           player.CardPlay(playerCardId2, ItemPosition.Start);
                           player.CardPlay(playerCardId3, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentCardId, ItemPosition.Start);
                           opponent.CardPlay(opponentCardId2, ItemPosition.Start);
                           opponent.CardPlay(opponentCardId3, ItemPosition.Start);
                       },
                       player => {},
                       opponent =>
                       {
                           opponent.CardAttack(opponentCardId, playerCardId);
                           opponent.CardAttack(opponentCardId2, playerCardId2);
                           opponent.CardAttack(opponentCardId3, playerCardId3);
                       },
                       player => {},
                       opponent => {},
                       player => {},
                       opponent => {},
                       player =>
                       {
                           player.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(playerCardId3,
                                    new ParametrizedAbilityParameters()
                                    {
                                        CardName = playerCardId3.Id.ToString()
                                    }),
                                new ParametrizedAbilityInstanceId(playerCardId,
                                    new ParametrizedAbilityParameters()
                                    {
                                        CardName = playerCardId.Id.ToString()
                                    })
                           });
                       },
                       opponent =>
                       {
                           opponent.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(opponentCardId2,
                                    new ParametrizedAbilityParameters()
                                    {
                                        CardName = opponentCardId2.Id.ToString()
                                    }),
                                new ParametrizedAbilityInstanceId(opponentCardId3,
                                    new ParametrizedAbilityParameters()
                                    {
                                        CardName = opponentCardId3.Id.ToString()
                                    })
                           });
                       },
                       player => {},
                       opponent => {},
                       };

                Action validateEndState = () =>
                {
                    Assert.NotNull(TestHelper.BattlegroundController.PlayerBoardCards.Select(card => card.Model.InstanceId == playerCardId3));
                    Assert.NotNull(TestHelper.BattlegroundController.PlayerBoardCards.Select(card => card.Model.InstanceId == playerCardId));
                    Assert.NotNull(TestHelper.BattlegroundController.OpponentBoardCards.Select(card => card.Model.InstanceId == opponentCardId2));
                    Assert.NotNull(TestHelper.BattlegroundController.OpponentBoardCards.Select(card => card.Model.InstanceId == opponentCardId3));
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
                    Assert.AreEqual(4, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCardId)).CurrentHp);
                    Assert.AreEqual(4, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCardId)).CurrentHp);
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
                           player.OverlordSkillUsed(new SkillId(0), null);
                       },
                       opponent =>
                       {
                           opponent.OverlordSkillUsed(new SkillId(0), null);
                       },
                       player => {},
                       opponent => {},
                   };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(20, pvpTestContext.GetCurrentPlayer().Defense);
                    Assert.AreEqual(20, pvpTestContext.GetOpponentPlayer().Defense);
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

            InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Puffer", 1);
            InstanceId playerCardId2 = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Puffer", 2);
            InstanceId playerCardId3 = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Puffer", 3);

            InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Puffer", 1);
            InstanceId opponentCardId2 = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Puffer", 2);
            InstanceId opponentCardId3 = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Puffer", 3);

            IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
               {
                       player =>
                       {
                           player.CardPlay(playerCardId, ItemPosition.Start);
                           player.CardPlay(playerCardId2, ItemPosition.Start);
                           player.CardPlay(playerCardId3, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentCardId, ItemPosition.Start);
                           opponent.CardPlay(opponentCardId2, ItemPosition.Start);
                           opponent.CardPlay(opponentCardId3, ItemPosition.Start);
                       },
                       player => {},
                       opponent =>
                       {
                           opponent.CardAttack(opponentCardId, playerCardId);
                           opponent.CardAttack(opponentCardId2, playerCardId2);
                           opponent.CardAttack(opponentCardId3, playerCardId3);
                       },
                       player => {},
                       opponent => {},
                       player => {},
                       opponent => {},
                       player =>
                       {
                           player.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(playerCardId3,
                                    new ParametrizedAbilityParameters()
                                    {
                                        CardName = playerCardId3.Id.ToString()
                                    })
                           });
                       },
                       opponent =>
                       {
                           opponent.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(opponentCardId2,
                                    new ParametrizedAbilityParameters()
                                    {
                                        CardName = opponentCardId2.Id.ToString()
                                    })
                           });
                       },
                       player => {},
                       opponent => {},
                   };

                Action validateEndState = () =>
                {
                    Assert.NotNull(TestHelper.BattlegroundController.PlayerBoardCards.Select(card => card.Model.InstanceId == playerCardId3));
                    Assert.NotNull(TestHelper.BattlegroundController.OpponentBoardCards.Select(card => card.Model.InstanceId == opponentCardId2));
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
                           player.OverlordSkillUsed(new SkillId(0), null);
                       },
                       opponent =>
                       {
                           opponent.OverlordSkillUsed(new SkillId(0), null);
                       },
                       player => {},
                       opponent => {},
                   };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(20, pvpTestContext.GetCurrentPlayer().Defense);
                    Assert.AreEqual(20, pvpTestContext.GetOpponentPlayer().Defense);
                    Assert.AreEqual(3, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCardId)).CurrentHp);
                    Assert.AreEqual(3, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCardId)).CurrentHp);
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
                    Assert.AreEqual(1, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCardId)).CurrentHp);
                    Assert.AreEqual(1, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCardId)).CurrentHp);
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

                InstanceId playerCardId1 = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Bouncer", 1);
                InstanceId playerCardId2 = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Whizpar", 1);

                InstanceId opponentCardId1 = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Bouncer", 1);
                InstanceId opponentCardId2 = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Whizpar", 1);

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
                           player.CardPlay(playerCardId1, ItemPosition.Start);
                           player.CardPlay(playerCardId2, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentCardId1, ItemPosition.Start);
                           opponent.CardPlay(opponentCardId2, ItemPosition.Start);
                       },
                       player => {},
                       opponent => {},
                       player => {},
                       opponent => {},
                       player =>
                       {
                           player.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(playerCardId1)
                           });
                       },
                       opponent =>
                       {
                           opponent.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(opponentCardId1)
                           });
                       },
                       player => {},
                       opponent => {},
                   };

                Action validateEndState = () =>
                {
                    Assert.IsNotNull(pvpTestContext.GetCurrentPlayer().CardsInHand.Select(card => card.LibraryCard.MouldId == 32));
                    Assert.IsNotNull(pvpTestContext.GetOpponentPlayer().CardsInHand.Select(card => card.LibraryCard.MouldId == 32));
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

                InstanceId playerCardId1 = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Whizpar", 1);
                InstanceId playerCardId2 = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Whizpar", 2);

                InstanceId opponentCardId1 = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Whizpar", 1);
                InstanceId opponentCardId2 = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Whizpar", 2);

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
                           player.CardPlay(playerCardId1, ItemPosition.Start);
                           player.CardPlay(playerCardId2, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentCardId1, ItemPosition.Start);
                           opponent.CardPlay(opponentCardId2, ItemPosition.Start);
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

                InstanceId playerCardId1 = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Bouncer", 1);
                InstanceId playerCardId2 = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Whizpar", 1);
                InstanceId playerCardId3 = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Whizpar", 2);
                InstanceId playerCardId4 = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Whizpar", 3);

                InstanceId opponentCardId1 = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Bouncer", 1);
                InstanceId opponentCardId2 = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Whizpar", 1);
                InstanceId opponentCardId3 = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Whizpar", 2);
                InstanceId opponentCardId4 = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Whizpar", 3);
                InstanceId opponentCardId5 = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Whizpar", 4);
                InstanceId opponentCardId6 = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Whizpar", 5);

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
                           player.CardPlay(playerCardId1, ItemPosition.Start);
                           player.CardPlay(playerCardId2, ItemPosition.Start);
                           player.CardPlay(playerCardId3, ItemPosition.Start);
                           player.CardPlay(playerCardId4, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentCardId1, ItemPosition.Start);
                           opponent.CardPlay(opponentCardId2, ItemPosition.Start);
                           opponent.CardPlay(opponentCardId3, ItemPosition.Start);
                           opponent.CardPlay(opponentCardId4, ItemPosition.Start);
                           opponent.CardPlay(opponentCardId5, ItemPosition.Start);
                           opponent.CardPlay(opponentCardId6, ItemPosition.Start);
                       },
                       player => {},
                       opponent =>
                       {
                           opponent.OverlordSkillUsed(new SkillId(0), null);
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
                           player.OverlordSkillUsed(new SkillId(0), null);
                       },
                       opponent =>
                       {
                           opponent.OverlordSkillUsed(new SkillId(0), null);
                       },
                   };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(pvpTestContext.GetCurrentPlayer().InitialHp + 2, pvpTestContext.GetCurrentPlayer().Defense);
                    Assert.AreEqual(pvpTestContext.GetOpponentPlayer().InitialHp + 2, pvpTestContext.GetOpponentPlayer().Defense);
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

                InstanceId playerCardId1 = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Slab", 1);
                InstanceId playerCardId2  = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Slab", 2);

                InstanceId opponentCardId1 = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Slab", 1);
                InstanceId opponentCardId2 = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Slab", 2);

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
                           player.CardPlay(playerCardId1, ItemPosition.Start);
                           player.CardPlay(playerCardId2, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentCardId1, ItemPosition.Start);
                           opponent.CardPlay(opponentCardId2, ItemPosition.Start);
                       },
                       player =>
                       {
                           player.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(playerCardId1)
                           });
                       },
                       opponent =>
                       {
                           opponent.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(opponentCardId1)
                           });
                       },
                       player => {},
                       opponent => {},
                   };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(5, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCardId1)).CurrentHp);
                    Assert.AreEqual(5, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCardId1)).CurrentHp);
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

                InstanceId playerCardId1 = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Slab", 1);
                InstanceId playerCardId2 = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Slab", 2);

                InstanceId opponentCardId1 = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Slab", 1);
                InstanceId opponentCardId2 = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Slab", 2);

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
                           player.CardPlay(playerCardId1, ItemPosition.Start);
                           player.CardPlay(playerCardId2, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentCardId1, ItemPosition.Start);
                           opponent.CardPlay(opponentCardId2, ItemPosition.Start);
                       },
                       player =>
                       {
                           player.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(playerCardId1)
                           });
                       },
                       opponent =>
                       {
                           opponent.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(opponentCardId1)
                           });
                       },
                       player => {},
                       opponent => {},
                   };

                Action validateEndState = () =>
                {
                    Assert.IsTrue(((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCardId1)).IsHeavyUnit);
                    Assert.IsTrue(((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCardId1)).IsHeavyUnit);
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

                InstanceId playerCardId1 = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Slab", 1);
                InstanceId playerCardId2 = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Slab", 2);
                InstanceId playerCardId3 = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Slab", 3);

                InstanceId opponentCardId1 = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Slab", 1);
                InstanceId opponentCardId2 = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Slab", 2);
                InstanceId opponentCardId3 = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Slab", 3);

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
                           player.CardPlay(playerCardId1, ItemPosition.Start);
                           player.CardPlay(playerCardId2, ItemPosition.Start);
                           player.CardPlay(playerCardId3, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentCardId1, ItemPosition.Start);
                           opponent.CardPlay(opponentCardId2, ItemPosition.Start);
                           opponent.CardPlay(opponentCardId3, ItemPosition.Start);
                       },
                       player =>
                       {
                           player.OverlordSkillUsed(new SkillId(0), null);
                       },
                       opponent =>
                       {
                           opponent.OverlordSkillUsed(new SkillId(0), null);
                       },
                       player => {},
                       opponent => {},
                   };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(5, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCardId1)).CurrentHp);
                    Assert.AreEqual(5, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCardId2)).CurrentHp);
                    Assert.AreEqual(5, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCardId3)).CurrentHp);
                    Assert.AreEqual(5, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCardId1)).CurrentHp);
                    Assert.AreEqual(5, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCardId2)).CurrentHp);
                    Assert.AreEqual(5, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCardId3)).CurrentHp);
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

                InstanceId playerCardId1 = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Znowy", 1);
                InstanceId playerCardId2 = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Znowy", 2);

                InstanceId opponentCardId1 = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Znowy", 1);
                InstanceId opponentCardId2 = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Znowy", 2);

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
                           player.CardPlay(playerCardId1, ItemPosition.Start);
                           player.CardPlay(playerCardId2, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentCardId1, ItemPosition.Start);
                           opponent.CardPlay(opponentCardId2, ItemPosition.Start);
                       },
                       player =>
                       {
                           player.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(opponentCardId1)
                           });
                       },
                       opponent =>
                       {
                           opponent.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(playerCardId1)
                           });
                       },
                   };

                Action validateEndState = () =>
                {
                    Assert.IsTrue(((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCardId1)).UnitStatus == Enumerators.UnitStatusType.FROZEN);
                    Assert.IsTrue(((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCardId1)).UnitStatus == Enumerators.UnitStatusType.FROZEN);
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

                InstanceId playerCardId1 = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Slab", 1);
                InstanceId playerCardId2 = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Slab", 2);

                InstanceId opponentCardId1 = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Slab", 1);
                InstanceId opponentCardId2 = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Slab", 2);

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
                           player.CardPlay(playerCardId1, ItemPosition.Start);
                           player.CardPlay(playerCardId2, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentCardId1, ItemPosition.Start);
                           opponent.CardPlay(opponentCardId2, ItemPosition.Start);
                       },
                       player =>
                       {
                           player.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(opponentCardId1)
                           });
                       },
                       opponent =>
                       {
                           opponent.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(playerCardId1)
                           });
                       },
                   };

                Action validateEndState = () =>
                {
                    BoardUnitModel playerUnit1 = (BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCardId1);
                    BoardUnitModel opponentUnit1 = (BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCardId1);

                    Assert.IsTrue(playerUnit1.IsStun);
                    Assert.AreEqual(playerUnit1.InitialHp - difference, playerUnit1.CurrentHp);

                    Assert.IsTrue(opponentUnit1.IsStun);
                    Assert.AreEqual(opponentUnit1.InitialHp - difference, opponentUnit1.CurrentHp);
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

                InstanceId playerCardId1 = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Znowy", 1);
                InstanceId playerCardId2 = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Znowy", 2);

                InstanceId opponentCardId1 = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Znowy", 1);
                InstanceId opponentCardId2 = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Znowy", 2);

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
                           player.CardPlay(playerCardId1, ItemPosition.Start);
                           player.CardPlay(playerCardId2, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentCardId1, ItemPosition.Start);
                           opponent.CardPlay(opponentCardId2, ItemPosition.Start);
                       },
                       player =>
                       {
                           player.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(playerCardId1)
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
                    BoardUnitModel playerUnit = (BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCardId1);

                    Assert.AreEqual(playerUnit.InitialHp + buffedHealth, playerUnit.CurrentHp);
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

                InstanceId playerCardId1 = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Slab", 1);
                InstanceId playerCardId2 = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Slab", 2);

                InstanceId opponentCardId1 = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Slab", 1);
                InstanceId opponentCardId2 = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Slab", 2);

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
                           player.CardPlay(playerCardId1, ItemPosition.Start);
                           player.CardPlay(playerCardId2, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentCardId1, ItemPosition.Start);
                           opponent.CardPlay(opponentCardId2, ItemPosition.Start);
                       },
                       player =>
                       {
                           player.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(opponentCardId1)
                           });
                           player.OverlordSkillUsed(new SkillId(1), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(opponentCardId1)
                           });
                       },
                       opponent =>
                       {
                           opponent.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(playerCardId1)
                           });
                           opponent.OverlordSkillUsed(new SkillId(1), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(playerCardId1)
                           });
                       },
                       player => {},
                       opponent => {},
                       player => {},
                       opponent => {},
                   };

                Action validateEndState = () =>
                {
                    Assert.Null((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCardId1));
                    Assert.Null((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCardId1));
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
                           player.OverlordSkillUsed(new SkillId(0), null);
                       },
                       opponent =>
                       {
                           
                           for (int i = 0; i < count; i++)
                           {
                               units[i] = (BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCardsId[i]);
                               units[count + i] = (BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCardsId[i]);

                               Assert.AreEqual(units[i].InitialHp - difference, units[i].CurrentHp);
                               Assert.AreEqual(units[count + i].InitialHp - difference, units[i].CurrentHp);
                           }
                           opponent.OverlordSkillUsed(new SkillId(0), null);
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

                InstanceId playerCardId1 = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Pyromaz", 1);
                InstanceId playerCardId2 = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Pyromaz", 2);

                InstanceId opponentCardId1 = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Pyromaz", 1);
                InstanceId opponentCardId2 = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Pyromaz", 2);

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
                           player.CardPlay(playerCardId1, ItemPosition.Start);
                           player.CardPlay(playerCardId2, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentCardId1, ItemPosition.Start);
                           opponent.CardPlay(opponentCardId2, ItemPosition.Start);
                       },
                       player =>
                       {
                           player.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(playerCardId1)
                           });
                       },
                       opponent =>
                       {
                           opponent.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(opponentCardId1)
                           });
                       },
                       player => {},
                       opponent => {},
                   };

                Action validateEndState = () =>
                {
                    Assert.IsTrue(((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCardId1)).HasFeral);
                    Assert.IsTrue(((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCardId1)).HasFeral);
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
                    Assert.AreEqual(playerUnit.InitialHp - difference, playerUnit.CurrentHp);
                    Assert.AreEqual(opponentUnit.InitialHp - difference, opponentUnit.CurrentHp);
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

                InstanceId playerCardId1 = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Slab", 1);
                InstanceId playerCardId2 = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Slab", 2);

                InstanceId opponentCardId1 = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Slab", 1);
                InstanceId opponentCardId2 = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Slab", 2);
                InstanceId opponentCardId3 = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Slab", 3);

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
                           player.CardPlay(playerCardId1, ItemPosition.Start);
                           player.CardPlay(playerCardId2, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentCardId1, ItemPosition.Start);
                           opponent.CardPlay(opponentCardId2, ItemPosition.Start);
                           opponent.CardPlay(opponentCardId3, ItemPosition.Start);
                       },
                       player =>
                       {
                           player.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(playerCardId1),
                                new ParametrizedAbilityInstanceId(playerCardId2)
                           });

                       },
                       opponent =>
                       {
                           opponent.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(opponentCardId1),
                                new ParametrizedAbilityInstanceId(opponentCardId3)
                           });
                       },
                       player => {},
                       opponent => {},
                   };

                Action validateEndState = () =>
                {
                    Assert.IsTrue(((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCardId1)).IsHeavyUnit);
                    Assert.IsTrue(((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCardId2)).IsHeavyUnit);
                    Assert.IsTrue(((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCardId1)).IsHeavyUnit);
                    Assert.IsTrue(!((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCardId2)).IsHeavyUnit);
                    Assert.IsTrue(((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCardId3)).IsHeavyUnit);
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

                InstanceId playerCardId1 = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Slab", 1);
                InstanceId playerCardId2 = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Slab", 2);
                InstanceId playerCardId3 = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Slab", 3);

                InstanceId opponentCardId1 = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Slab", 1);
                InstanceId opponentCardId2 = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Slab", 2);
                InstanceId opponentCardId3 = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Slab", 3);

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
                           player.CardPlay(playerCardId1, ItemPosition.Start);
                           player.CardPlay(playerCardId2, ItemPosition.Start);
                           player.CardPlay(playerCardId3, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentCardId1, ItemPosition.Start);
                           opponent.CardPlay(opponentCardId2, ItemPosition.Start);
                           opponent.CardPlay(opponentCardId3, ItemPosition.Start);
                       },
                       player =>
                       {
                           player.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(opponentCardId1),
                                new ParametrizedAbilityInstanceId(opponentCardId3)
                           });
                       },
                       opponent =>
                       {
                           opponent.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(playerCardId1),
                                new ParametrizedAbilityInstanceId(playerCardId3)
                           });
                       },
                   };

                Action validateEndState = () =>
                {
                    Assert.IsTrue(((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCardId1)).IsStun);
                    Assert.IsTrue(!((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCardId2)).IsStun);
                    Assert.IsTrue(((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCardId3)).IsStun);
                    Assert.IsTrue(((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCardId1)).IsStun);
                    Assert.IsTrue(!((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCardId2)).IsStun);
                    Assert.IsTrue(((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCardId3)).IsStun);
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

                InstanceId playerCardId1 = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Whizpar", 1);
                InstanceId playerCardId2 = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Whizpar", 2);

                InstanceId opponentCardId1 = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Whizpar", 1);
                InstanceId opponentCardId2 = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Whizpar", 2);

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
                           player.CardPlay(playerCardId1, ItemPosition.Start);
                           player.CardPlay(playerCardId2, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentCardId1, ItemPosition.Start);
                           opponent.CardPlay(opponentCardId2, ItemPosition.Start);
                       },
                       player =>
                       {
                           player.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(playerCardId1)
                           });
                       },
                       opponent =>
                       {
                           opponent.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(opponentCardId2)
                           });
                       },
                       player => {},
                       opponent => {},
                   };

                Action validateEndState = () =>
                {
                    Assert.IsTrue(((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCardId1)).HasBuffShield);
                    Assert.IsTrue(!((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCardId2)).HasBuffShield);
                    Assert.IsTrue(((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCardId2)).HasBuffShield);
                    Assert.IsTrue(!((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCardId1)).HasBuffShield);
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

                InstanceId playerCardId1 = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Whizpar", 1);
                InstanceId playerCardId2 = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Whizpar", 2);

                InstanceId opponentCardId1 = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Whizpar", 1);
                InstanceId opponentCardId2 = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Whizpar", 2);

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
                                new ParametrizedAbilityInstanceId(playerCardId1,
                                    new ParametrizedAbilityParameters()
                                        {
                                            CardName = playerCardId1.Id.ToString()
                                        }
                                    )
                           });
                       },
                       opponent =>
                       {
                           opponent.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(opponentCardId2,
                                    new ParametrizedAbilityParameters()
                                        {
                                            CardName = opponentCardId2.Id.ToString()
                                        }
                                    )
                           });
                       },
                       player => {},
                       opponent => {},
                   };

                Action validateEndState = () =>
                {
                    WorkingCard playerCardInHand = TestHelper.BattlegroundController.GetWorkingCardByInstanceId(playerCardId1);
                    WorkingCard opponentCardInHand = TestHelper.BattlegroundController.GetWorkingCardByInstanceId(opponentCardId2);

                    Assert.AreEqual(playerCardInHand.LibraryCard.Cost - 1, playerCardInHand.InstanceCard.Cost);
                    Assert.AreEqual(opponentCardInHand.LibraryCard.Cost - 1, playerCardInHand.InstanceCard.Cost);
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

                InstanceId playerCardId1 = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Slab", 1);
                InstanceId playerCardId2 = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Slab", 2);
                InstanceId playerCardId3 = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Slab", 3);

                InstanceId opponentCardId1 = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Slab", 1);
                InstanceId opponentCardId2 = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Slab", 2);
                InstanceId opponentCardId3 = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Slab", 3);

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
                           player.CardPlay(playerCardId1, ItemPosition.Start);
                           player.CardPlay(playerCardId2, ItemPosition.Start);
                           player.CardPlay(playerCardId3, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentCardId1, ItemPosition.Start);
                           opponent.CardPlay(opponentCardId2, ItemPosition.Start);
                           opponent.CardPlay(opponentCardId3, ItemPosition.Start);
                       },
                       player =>
                       {
                           player.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(opponentCardId1,
                                    new ParametrizedAbilityParameters()
                                    {
                                        Attack = 2
                                    }),
                                new ParametrizedAbilityInstanceId(opponentCardId3,
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
                                new ParametrizedAbilityInstanceId(playerCardId1,
                                    new ParametrizedAbilityParameters()
                                    {
                                        Attack = 1
                                    }),
                                new ParametrizedAbilityInstanceId(playerCardId3,
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
                    BoardUnitModel playerUnit1 = (BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCardId1);
                    BoardUnitModel playerUnit3 = (BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCardId3);

                    BoardUnitModel opponentUnit1 = (BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCardId1);
                    BoardUnitModel opponentUnit3 = (BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCardId3);

                    Assert.AreEqual(playerUnit1.InitialHp - 1, playerUnit1.CurrentHp);
                    Assert.AreEqual(playerUnit3.InitialHp - 1, playerUnit3.CurrentHp);
                    Assert.AreEqual(pvpTestContext.GetCurrentPlayer().InitialHp - 3, pvpTestContext.GetCurrentPlayer().Defense);

                    Assert.AreEqual(opponentUnit1.InitialHp - 2, opponentUnit1.CurrentHp);
                    Assert.AreEqual(opponentUnit3.InitialHp - 2, opponentUnit3.CurrentHp);
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

                InstanceId playerCardId1 = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Pyromaz", 1);
                InstanceId playerCardId2 = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Pyromaz", 2);
                InstanceId playerCardId3 = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Pyromaz", 3);

                InstanceId opponentCardId1 = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Pyromaz", 1);
                InstanceId opponentCardId2 = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Pyromaz", 2);
                InstanceId opponentCardId3 = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Pyromaz", 3);

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
                           player.CardPlay(playerCardId1, ItemPosition.Start);
                           player.CardPlay(playerCardId2, ItemPosition.Start);
                           player.CardPlay(playerCardId3, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentCardId1, ItemPosition.Start);
                           opponent.CardPlay(opponentCardId2, ItemPosition.Start);
                           opponent.CardPlay(opponentCardId3, ItemPosition.Start);
                       },
                       player =>
                       {
                           player.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(playerCardId1),
                                new ParametrizedAbilityInstanceId(playerCardId2)
                           });

                       },
                       opponent =>
                       {
                           opponent.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(opponentCardId1),
                                new ParametrizedAbilityInstanceId(opponentCardId3)
                           });
                       },
                       player => {},
                       opponent => {},
                   };

                Action validateEndState = () =>
                {
                    Assert.IsTrue(((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCardId1)).HasFeral);
                    Assert.IsTrue(((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCardId2)).HasFeral);
                    Assert.IsTrue(!((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCardId3)).HasFeral);
                    Assert.IsTrue(((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCardId1)).HasFeral);
                    Assert.IsTrue(!((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCardId2)).HasFeral);
                    Assert.IsTrue(((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCardId3)).HasFeral);
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

                InstanceId playerCardId1 = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Hazmaz", 1);
                InstanceId playerCardId2 = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Hazmaz", 2);
                InstanceId playerCardId3 = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Hazmaz", 3);

                InstanceId opponentCardId1 = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Hazmaz", 1);
                InstanceId opponentCardId2 = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Hazmaz", 2);
                InstanceId opponentCardId3 = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Hazmaz", 3);

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
                           player.CardPlay(playerCardId1, ItemPosition.Start);
                           player.CardPlay(playerCardId2, ItemPosition.Start);
                           player.CardPlay(playerCardId3, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentCardId1, ItemPosition.Start);
                           opponent.CardPlay(opponentCardId2, ItemPosition.Start);
                           opponent.CardPlay(opponentCardId3, ItemPosition.Start);
                       },
                       player =>
                       {
                           playerDifference = ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCardId1)).CurrentDamage;
                           player.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(playerCardId1),
                                new ParametrizedAbilityInstanceId(opponentCardId3),
                           });
                       },
                       opponent =>
                       {
                           opponentDiference = ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCardId2)).CurrentDamage;
                           opponent.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(opponentCardId2),
                                new ParametrizedAbilityInstanceId(playerCardId3)
                           });
                       },
                   };

                Action validateEndState = () =>
                {
                    BoardUnitModel playerUnit3 = (BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCardId3);
                    BoardUnitModel opponentUnit3 = (BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCardId3);

                    Assert.AreEqual(playerUnit3.InitialHp - opponentDiference, playerUnit3.CurrentHp);
                    Assert.AreEqual(opponentUnit3.InitialHp - playerDifference, opponentUnit3.CurrentHp);
                    Assert.Null((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCardId1));
                    Assert.Null((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCardId2));
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

                InstanceId playerCardId1 = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Hazmaz", 1);
                InstanceId playerCardId2 = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Hazmaz", 2);
                InstanceId playerCardId3 = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Hazmaz", 3);
                InstanceId playerCardId4 = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Hazmaz", 4);
                InstanceId playerCardId5 = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Hazmaz", 5);

                InstanceId opponentCardId1 = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Hazmaz", 1);
                InstanceId opponentCardId2 = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Hazmaz", 2);
                InstanceId opponentCardId3 = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Hazmaz", 3);
                InstanceId opponentCardId4 = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Hazmaz", 4);
                InstanceId opponentCardId5 = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Hazmaz", 5);

                int playerDifference1 = 0;
                int playerDifference2 = 0;
                int opponentDiference1 = 0;
                int opponentDiference2 = 0;

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
                           player.CardPlay(playerCardId1, ItemPosition.Start);
                           player.CardPlay(playerCardId2, ItemPosition.Start);
                           player.CardPlay(playerCardId3, ItemPosition.Start);
                           player.CardPlay(playerCardId4, ItemPosition.Start);
                           player.CardPlay(playerCardId5, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentCardId1, ItemPosition.Start);
                           opponent.CardPlay(opponentCardId2, ItemPosition.Start);
                           opponent.CardPlay(opponentCardId3, ItemPosition.Start);
                           opponent.CardPlay(opponentCardId4, ItemPosition.Start);
                           opponent.CardPlay(opponentCardId5, ItemPosition.Start);
                       },
                       player =>
                       {
                           playerDifference1 = ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCardId1)).CurrentDamage;
                           playerDifference2 = ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCardId4)).CurrentDamage;
                           player.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(playerCardId1),
                                new ParametrizedAbilityInstanceId(opponentCardId3),
                                new ParametrizedAbilityInstanceId(playerCardId4),
                                new ParametrizedAbilityInstanceId(opponentCardId1),
                           });
                       },
                       opponent =>
                       {
                           opponentDiference1 = ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCardId2)).CurrentDamage;
                           opponentDiference2 = ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCardId4)).CurrentDamage;
                           opponent.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(opponentCardId2),
                                new ParametrizedAbilityInstanceId(playerCardId3),
                                new ParametrizedAbilityInstanceId(opponentCardId4),
                                new ParametrizedAbilityInstanceId(playerCardId2),
                           });
                       },
                   };

                Action validateEndState = () =>
                {
                    BoardUnitModel playerUnit2 = (BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCardId2);
                    BoardUnitModel playerUnit3 = (BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCardId3);
                    BoardUnitModel opponentUnit1 = (BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCardId1);
                    BoardUnitModel opponentUnit3 = (BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCardId3);

                    Assert.AreEqual(playerUnit3.InitialHp - opponentDiference1, playerUnit3.CurrentHp);
                    Assert.AreEqual(playerUnit2.InitialHp - opponentDiference2, playerUnit2.CurrentHp);
                    Assert.AreEqual(opponentUnit3.InitialHp - playerDifference1, opponentUnit3.CurrentHp);
                    Assert.AreEqual(opponentUnit1.InitialHp - playerDifference2, opponentUnit1.CurrentHp);
                    Assert.Null((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCardId1));
                    Assert.Null((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCardId4));
                    Assert.Null((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCardId2));
                    Assert.Null((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCardId4));
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }
    }
}
