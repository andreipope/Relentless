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
    public class EarthCardsTests : BaseIntegrationTest
    {
        [UnityTest]
        [Timeout(150 * 1000 * TestHelper.TestTimeScale)]
        public IEnumerator Protector()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0,
                    new DeckCardData("Protector", 1),
                    new DeckCardData("Slab", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    new DeckCardData("Protector", 1),
                    new DeckCardData("Slab", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerProtectorId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Protector", 1);
                InstanceId playerSlab1Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Slab", 1);
                InstanceId playerSlab2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Slab", 2);
                InstanceId playerSlab3Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Slab", 3);
                InstanceId playerSlab4Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Slab", 4);
                InstanceId playerSlab5Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Slab", 5);
                InstanceId opponentProtectorId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Protector", 1);
                InstanceId opponentSlab1Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Slab", 1);
                InstanceId opponentSlab2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Slab", 2);
                InstanceId opponentSlab3Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Slab", 3);
                InstanceId opponentSlab4Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Slab", 4);
                InstanceId opponentSlab5Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Slab", 5);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                       player => {},
                       opponent => {},
                       player => 
                       {
                           player.CardPlay(playerProtectorId, ItemPosition.Start);
                       },
                       opponent => 
                       {
                           opponent.CardPlay(opponentSlab1Id, ItemPosition.Start);
                       },
                       player =>
                       {
                           player.CardPlay(playerSlab1Id, ItemPosition.Start);
                           player.CardPlay(playerSlab2Id, ItemPosition.Start);
                           player.CardPlay(playerSlab3Id, ItemPosition.Start);
                           player.CardPlay(playerSlab4Id, ItemPosition.Start);
                           player.CardPlay(playerSlab5Id, ItemPosition.Start);
                           player.CardAttack(playerProtectorId, opponentSlab1Id);
                       },
                       opponent => {},
                       player => {}
                };

                Action validateEndState = () =>
                {
                    Assert.IsTrue(((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerSlab1Id)).HasHeavy);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }
    }
}
