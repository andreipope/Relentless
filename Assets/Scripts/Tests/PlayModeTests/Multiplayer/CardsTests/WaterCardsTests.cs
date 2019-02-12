using System;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using UnityEngine.TestTools;

namespace Loom.ZombieBattleground.Test.MultiplayerTests
{
    public class WaterCardsTests : BaseIntegrationTest
    {
        [UnityTest]
        [Timeout(150 * 1000 * TestHelper.TestTimeScale)]
        public IEnumerator Znowman()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0,
                    new DeckCardData("Slab", 5),
                    new DeckCardData("Znowman", 1)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    new DeckCardData("Slab", 5),
                    new DeckCardData("Znowman", 1)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerZnowmanId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Znowman", 1);
                InstanceId playerSlabId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Slab", 1);
                InstanceId playerSlab2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Slab", 2);
                InstanceId opponentZnowmanId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Znowman", 1);
                InstanceId opponentSlabId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Slab", 1);
                InstanceId opponentSlab2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Slab", 2);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                       player => {},
                       opponent => {},
                       player => {},
                       opponent => {},
                       player => {
                           player.CardPlay(playerZnowmanId, ItemPosition.Start);
                           player.CardPlay(playerSlabId, ItemPosition.Start);
                           player.CardPlay(playerSlab2Id, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentZnowmanId, ItemPosition.Start);
                           opponent.CardPlay(opponentSlabId, ItemPosition.Start);
                           opponent.CardPlay(opponentSlab2Id, ItemPosition.Start);
                       },
                       player => player.CardAttack(playerSlabId, opponentZnowmanId),
                       opponent => opponent.CardAttack(opponentSlabId, playerZnowmanId),

                };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(3, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerSlabId)).CurrentHp);
                    Assert.AreEqual(1, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerZnowmanId)).CurrentHp);
                    Assert.AreEqual(true, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerZnowmanId)).HasHeavy);
                    Assert.AreEqual(true, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerSlabId)).IsStun);
                    Assert.AreEqual(true, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerSlab2Id)).IsStun);

                    Assert.AreEqual(3, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentSlabId)).CurrentHp);
                    Assert.AreEqual(1, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentZnowmanId)).CurrentHp);
                    Assert.AreEqual(true, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentZnowmanId)).HasHeavy);
                    Assert.AreEqual(true, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentSlabId)).IsStun);
                    Assert.AreEqual(true, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentSlab2Id)).IsStun);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }
    }
}
