using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Loom.ZombieBattleground.Test
{
    public class OverlordAbilitiesTests : BaseIntegrationTest
    {
        [UnityTest]
        [Timeout(150 * 1000 * TestHelper.TestTimeScale)]
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
                           opponent.OverlordSkillUsed(new SkillId(0), pvpTestContext.GetCurrentPlayer().InstanceId);
                       },
                       player =>
                       {
                           Assert.AreEqual(18, pvpTestContext.GetCurrentPlayer().Defense);
                           player.OverlordSkillUsed(new SkillId(0), pvpTestContext.GetOpponentPlayer().InstanceId);
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
        [Timeout(150 * 1000 * TestHelper.TestTimeScale)]
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
                           Time.timeScale = 0.5f;
                       },
                       opponent =>
                       {
                           int numCardsWithFeral =
                               new[]
                                   {
                                       (BoardUnitModel) TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerPyromaz1Id),
                                       (BoardUnitModel) TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerPyromaz2Id),
                                       (BoardUnitModel) TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerPyromaz3Id)
                                   }.Select(model => model.HasFeral)
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
        [Timeout(150 * 1000 * TestHelper.TestTimeScale)]
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
        [Timeout(150 * 1000 * TestHelper.TestTimeScale)]
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
                           player.OverlordSkillUsed(new SkillId(0), playerCardId);
                       },
                       opponent =>
                       {
                           opponent.OverlordSkillUsed(new SkillId(0), opponentCardId);
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
        [Timeout(150 * 1000 * TestHelper.TestTimeScale)]
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
        [Timeout(150 * 1000 * TestHelper.TestTimeScale)]
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
        [Timeout(150 * 1000 * TestHelper.TestTimeScale)]
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
                       opponent =>
                       {
                       },
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
    }
}
