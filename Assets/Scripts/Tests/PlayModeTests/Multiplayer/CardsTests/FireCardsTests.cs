using System;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using UnityEngine.TestTools;

namespace Loom.ZombieBattleground.Test.MultiplayerTests
{
    public class FireCardsTests : BaseIntegrationTest
    {
        [UnityTest]
        [Timeout(150 * 1000 * TestHelper.TestTimeScale)]
        public IEnumerator Pyromaz()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0, new DeckCardData("Pyromaz", 1));
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0, new DeckCardData("Zludge", 1));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Pyromaz", 1);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zludge", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                       player => {},
                       opponent => {},
                       player => {},
                       opponent => {},
                       player => player.CardPlay(playerCardId, ItemPosition.Start),
                       opponent => opponent.CardPlay(opponentCardId, ItemPosition.Start),
                       player => player.CardAttack(playerCardId, opponentCardId)
                };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(2, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCardId)).CurrentHp);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }

        [UnityTest]
        [Timeout(150 * 1000 * TestHelper.TestTimeScale)]
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
                        new DeckCardData("Slab", 2)
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
                        new DeckCardData("Cynderman", 2),
                        new DeckCardData("Slab", 2)
                    },
                    Enumerators.OverlordSkill.NONE,
                    Enumerators.OverlordSkill.NONE
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerSlabId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Slab", 1);
                InstanceId opponentSlabId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Slab", 1);
                InstanceId playerCyndermanId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Cynderman", 1);
                InstanceId opponentCyndermanId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Cynderman", 1);
                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                   {
                       player => {},
                       opponent => {},
                       player => {},
                       opponent => {},
                       player => player.CardPlay(playerSlabId, ItemPosition.Start),
                       opponent =>
                       {
                           opponent.CardPlay(opponentSlabId, ItemPosition.Start);
                           opponent.CardPlay(opponentCyndermanId, ItemPosition.Start, playerSlabId);
                       },
                       player =>
                       {
                           player.CardPlay(playerCyndermanId, ItemPosition.Start, opponentCyndermanId);
                       },
                   };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(2, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerSlabId)).CurrentHp);
                    Assert.AreEqual(2, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCyndermanId)).CurrentHp);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }
    }
}
