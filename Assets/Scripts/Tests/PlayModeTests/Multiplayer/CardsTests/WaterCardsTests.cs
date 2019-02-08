using System;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using UnityEngine.TestTools;

namespace Loom.ZombieBattleground.Test.MultiplayerCardsTests
{
    public class WaterCardsTests : BaseCardsTest
    {
        [UnityTest]
        [Timeout(150 * 1000 * TestHelper.TestTimeScale)]
        public IEnumerator Jetter()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestHelper.GetDeckWithCards("deck 1", new List<string>() { "Jetter" });
                Deck opponentDeck = PvPTestHelper.GetDeckWithCards("deck 2", new List<string>() { "Jetter" });


                InstanceId playerJetter1 = new InstanceId(0);
                InstanceId playerJetter2 = new InstanceId(1);
                InstanceId opponentJetter1 = new InstanceId(2);
                InstanceId opponentJetter2 = new InstanceId(3);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                       opponent => {},
                       player => {},
                       opponent => {},
                       player => {},
                       opponent => {},
                       player => {},
                       opponent => {},
                       player => {},
                       opponent => {},
                       player => player.CardPlay(playerJetter1, ItemPosition.Start),
                       opponent =>
                       {
                           opponent.CardPlay(opponentJetter1, ItemPosition.Start);
                           opponent.CardAbilityUsed(
                               opponentJetter1,
                               Enumerators.AbilityType.DAMAGE_TARGET,
                               new List<ParametrizedAbilityInstanceId>
                               {
                                   new ParametrizedAbilityInstanceId(playerJetter1, Enumerators.AffectObjectType.Character)
                               }
                           );
                       },
                       player => player.CardPlay(playerJetter2, ItemPosition.Start, opponentJetter1),
                       opponent =>
                       {
                           opponent.CardPlay(opponentJetter2, ItemPosition.Start);
                           opponent.CardAbilityUsed(
                               opponentJetter2,
                               Enumerators.AbilityType.DAMAGE_TARGET,
                               new List<ParametrizedAbilityInstanceId>
                               {
                                   new ParametrizedAbilityInstanceId(playerJetter2, Enumerators.AffectObjectType.Character)
                               }
                           );
                       },
                };
                  
                await DoPvPMatch(turns, playerDeck, opponentDeck);

                BoardUnitModel playerJetter1Model = null;
                BoardUnitModel playerJetter2Model = null;
                BoardUnitModel opponentJetter1Model = null;
                BoardUnitModel opponentJetter2Model = null;


                playerJetter1Model = (BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectById(playerJetter1);
                playerJetter2Model = (BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectById(playerJetter2);
                opponentJetter1Model = (BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectById(opponentJetter1);
                opponentJetter2Model = (BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectById(opponentJetter2);

                Assert.NotNull(playerJetter1Model);
                Assert.NotNull(playerJetter2Model);
                Assert.NotNull(opponentJetter1Model);
                Assert.NotNull(opponentJetter2Model);

                Assert.AreEqual(playerJetter1Model.CurrentHp - 1, playerJetter1Model.CurrentHp);
                Assert.AreEqual(playerJetter2Model.CurrentHp - 1, playerJetter2Model.CurrentHp);
                Assert.AreEqual(opponentJetter1Model.CurrentHp - 1, opponentJetter1Model.CurrentHp);
                Assert.AreEqual(opponentJetter2Model.CurrentHp, opponentJetter2Model.CurrentHp);
            });
        }
    }
}
