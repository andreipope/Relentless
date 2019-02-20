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
                    Enumerators.OverlordSkill.ENHANCE,
                    new DeckCardData("Puffer", 1),
                    new DeckCardData("Azuraz", 15));

                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 5,
                    Enumerators.OverlordSkill.REANIMATE,
                    Enumerators.OverlordSkill.ENHANCE,
                    new DeckCardData("Puffer", 1),
                    new DeckCardData("Azuraz", 15));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };


                InstanceId playerPufferId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Puffer", 1);
                InstanceId playerAzuraz1Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Azuraz", 1);

                InstanceId opponentPufferId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Puffer", 1);
                InstanceId opponentAzuraz1Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Azuraz", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                   {
                       player =>
                       {
                           player.CardPlay(playerAzuraz1Id, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentAzuraz1Id, ItemPosition.Start);
                       },
                       player =>
                       {
                           player.CardAttack(playerAzuraz1Id, opponentAzuraz1Id);
                       },
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
                       player =>
                       {
                           player.CardPlay(playerPufferId, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentPufferId, ItemPosition.Start);
                       },
                       player => {},
                       opponent => {},
                   };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(2, TestHelper.BattlegroundController.PlayerBoardCards.Count);
                    Assert.AreEqual(2, TestHelper.BattlegroundController.OpponentBoardCards.Count);
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
                    Enumerators.OverlordSkill.ENHANCE,
                    new DeckCardData("Puffer", 15));

                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 5,
                    Enumerators.OverlordSkill.RESSURECT,
                    Enumerators.OverlordSkill.ENHANCE,
                    new DeckCardData("Puffer", 15));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Puffer", 1);

                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Puffer", 1);

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
                       opponent =>
                       {
                           opponent.CardAttack(opponentCardId, playerCardId);
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
                    Assert.AreEqual(1, TestHelper.BattlegroundController.PlayerBoardCards.Count);
                    Assert.AreEqual(1, TestHelper.BattlegroundController.OpponentBoardCards.Count);
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
        [Timeout(150 * 1000 * TestHelper.TestTimeScale)]
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
        [Timeout(150 * 1000 * TestHelper.TestTimeScale)]
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
        [Timeout(150 * 1000 * TestHelper.TestTimeScale)]
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
        [Timeout(150 * 1000 * TestHelper.TestTimeScale)]
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

                InstanceId opponentCardId1 = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Whizpar", 2);
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
                           player.OverlordSkillUsed(new SkillId(0), null);
                       },
                       opponent =>
                       {
                           countOpponentCards = pvpTestContext.GetOpponentPlayer().CardsInHand.Count;
                           opponent.OverlordSkillUsed(new SkillId(0), null);
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
        [Timeout(150 * 1000 * TestHelper.TestTimeScale)]
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
        [Timeout(150 * 1000 * TestHelper.TestTimeScale)]
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
        [Timeout(150 * 1000 * TestHelper.TestTimeScale)]
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
        [Timeout(150 * 1000 * TestHelper.TestTimeScale)]
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
        [Timeout(150 * 1000 * TestHelper.TestTimeScale)]
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
        [Timeout(150 * 1000 * TestHelper.TestTimeScale)]
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
        [Timeout(150 * 1000 * TestHelper.TestTimeScale)]
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
        [Timeout(150 * 1000 * TestHelper.TestTimeScale)]
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
        [Timeout(150 * 1000 * TestHelper.TestTimeScale)]
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
        [Timeout(150 * 1000 * TestHelper.TestTimeScale)]
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
        [Timeout(150 * 1000 * TestHelper.TestTimeScale)]
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
        [Timeout(150 * 1000 * TestHelper.TestTimeScale)]
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
    }
}
