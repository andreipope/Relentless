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
                               new []{playerPyromaz1Id, playerPyromaz2Id, playerPyromaz3Id}
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
    }
}
