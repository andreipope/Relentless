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
    public class AirCardsTests : BaseIntegrationTest
    {
        [UnityTest]
        [Timeout(150 * 1000 * TestHelper.TestTimeScale)]
        public IEnumerator MindFlayer()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 2, new DeckCardData[]
                {
                    new DeckCardData("Mind Flayer", 2),
                    new DeckCardData("Pyromaz", 2),
                });
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 2, new DeckCardData[]
                {
                    new DeckCardData("Mind Flayer", 2),
                    new DeckCardData("Pyromaz", 2),
                });

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerWhizparId1 = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Pyromaz", 1);
                InstanceId playerWhizparId2 = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Pyromaz", 2);
                InstanceId playerMindFlayerId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Mind Flayer", 1);

                InstanceId opponentWhizparId1 = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Pyromaz", 1);
                InstanceId opponentWhizparId2 = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Pyromaz", 2);
                InstanceId opponentMindFlayerId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Mind Flayer", 1);
                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                   {
                       player => {},
                       opponent => {},
                       player =>
                       {
                           player.CardPlay(playerWhizparId1, ItemPosition.Start);
                           player.CardPlay(playerWhizparId2, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentWhizparId1, ItemPosition.Start);
                       },
                       player =>
                       {
                           player.CardAttack(playerWhizparId1, opponentWhizparId1);
                           player.CardPlay(playerMindFlayerId, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentWhizparId2, ItemPosition.Start);
                       },
                       player => {},
                       opponent =>
                       {
                           opponent.CardAttack(opponentWhizparId2, playerWhizparId2);
                           opponent.CardPlay(opponentMindFlayerId, ItemPosition.Start);
                       },
                       player => {},
                       opponent => {},
                   };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(0, TestHelper.BattlegroundController.PlayerBoardCards.Count);
                    Assert.AreEqual(2, TestHelper.BattlegroundController.OpponentBoardCards.Count);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }
    }
}
