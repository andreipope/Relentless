using System;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using UnityEngine.TestTools;
using System.Linq;

namespace Loom.ZombieBattleground.Test.MultiplayerTests
{
    public class LifeCardsTests : BaseIntegrationTest
    {
        [UnityTest]
        [Timeout(150 * 1000 * TestHelper.TestTimeScale)]
        public IEnumerator Cactuz()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0,
                    new DeckCardData("Cactuz", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    new DeckCardData("Cactuz", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Cactuz", 1);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Cactuz", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                       player => {},
                       opponent => {},
                       player => {},
                       opponent => {},
                       player => player.CardPlay(playerCardId, ItemPosition.Start),
                       opponent => opponent.CardPlay(opponentCardId, ItemPosition.Start),
                       player => player.CardAttack(playerCardId, opponentCardId),
                       player => {},
                       opponent => {},
                };

                Action validateEndState = () =>
                {
                    Assert.NotNull(TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCardId));
                    Assert.NotNull(TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCardId));
                    Assert.AreEqual(2, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCardId)).CurrentHp);
                    Assert.AreEqual(2, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCardId)).CurrentHp);
                    Assert.AreEqual(156, ((BoardUnitModel)TestHelper.BattlegroundController.PlayerHandCards.Select(card => card.LibraryCard.MouldId)));
                    Assert.AreEqual(156, ((BoardUnitModel)TestHelper.BattlegroundController.OpponentHandCards.Select(card => card.WorkingCard.LibraryCard.MouldId)));
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }

        [UnityTest]
        [Timeout(150 * 1000 * TestHelper.TestTimeScale)]
        public IEnumerator Huzk()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0,
                    new DeckCardData("Huzk", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    new DeckCardData("Huzk", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Huzk", 1);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Huzk", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                       player => {},
                       opponent => {},
                       player => {},
                       opponent => {},
                       player => player.CardPlay(playerCardId, ItemPosition.Start),
                       opponent => opponent.CardPlay(opponentCardId, ItemPosition.Start),
                       player => player.CardAttack(playerCardId, opponentCardId),
                       player => {},
                       opponent => {},
                };

                Action validateEndState = () =>
                {
                    Assert.NotNull(TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCardId));
                    Assert.NotNull(TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCardId));
                    Assert.AreEqual(2, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCardId)).CurrentHp);
                    Assert.AreEqual(2, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCardId)).CurrentHp);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }

        [UnityTest]
        [Timeout(150 * 1000 * TestHelper.TestTimeScale)]
        public IEnumerator Yggdrazil()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0,
                    new DeckCardData("Yggdrazil", 1),
                    new DeckCardData("Azuraz", 1),
                    new DeckCardData("MonZoon", 2),
                    new DeckCardData("Pyromaz", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    new DeckCardData("Yggdrazil", 1),
                    new DeckCardData("Azuraz", 1),
                    new DeckCardData("MonZoon", 2),
                    new DeckCardData("Pyromaz", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerYggdrazildId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Yggdrazil", 1);
                InstanceId playerAzurazdId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Azuraz", 1);
                InstanceId playerMonZoonId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "MonZoon", 1);
                InstanceId playerMonZoon2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "MonZoon", 2);
                InstanceId playerPyromazId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Pyromaz", 1);
                InstanceId playerPyromaz2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Pyromaz", 2);
                InstanceId opponentYggdrazildId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Yggdrazil", 1);
                InstanceId opponentAzurazdId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Azuraz", 1);
                InstanceId opponentMonZoonId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "MonZoon", 1);
                InstanceId opponentMonZoon2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "MonZoon", 2);
                InstanceId opponentPyromazId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Pyromaz", 1);
                InstanceId opponentPyromaz2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Pyromaz", 2);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                       player => {},
                       opponent => {},
                       player => {},
                       opponent => {},
                       player =>
                       {
                           player.CardPlay(playerAzurazdId, ItemPosition.Start);
                           player.CardPlay(playerMonZoonId, ItemPosition.Start);
                           player.CardPlay(playerMonZoon2Id, ItemPosition.Start);
                           player.CardPlay(playerPyromazId, ItemPosition.Start);
                           player.CardPlay(playerPyromaz2Id, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentAzurazdId, ItemPosition.Start);
                           opponent.CardPlay(opponentMonZoonId, ItemPosition.Start);
                           opponent.CardPlay(opponentMonZoon2Id, ItemPosition.Start);
                           opponent.CardPlay(opponentPyromazId, ItemPosition.Start);
                           opponent.CardPlay(opponentPyromaz2Id, ItemPosition.Start);
                       },
                       player =>
                       {
                           player.CardAttack(playerAzurazdId, opponentAzurazdId);
                       },
                       opponent => {
                           opponent.CardAttack(opponentMonZoonId, playerMonZoonId);
                           opponent.CardAttack(opponentMonZoon2Id, playerMonZoon2Id);
                       },
                       player => {
                           player.CardAttack(playerPyromazId, opponentPyromazId);
                           player.CardAttack(playerPyromaz2Id, opponentPyromaz2Id);
                       },
                       opponent => {},
                       player => player.CardPlay(playerYggdrazildId, ItemPosition.Start),
                       opponent => {},
                       player => {},
                       opponent => opponent.CardPlay(opponentYggdrazildId, ItemPosition.Start),
                       player => {},
                };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(4, TestHelper.BattlegroundController.PlayerBoardCards.Count);
                    Assert.AreEqual(4, TestHelper.BattlegroundController.OpponentBoardCards.Count);

                    Assert.NotNull(TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerYggdrazildId));
                    Assert.NotNull(TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerAzurazdId));
                    Assert.NotNull(TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerMonZoonId));
                    Assert.NotNull(TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerMonZoon2Id));
                    Assert.IsNull(TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerPyromazId));
                    Assert.IsNull(TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerPyromaz2Id));

                    Assert.NotNull(TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentYggdrazildId));
                    Assert.NotNull(TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentAzurazdId));
                    Assert.NotNull(TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentMonZoonId));
                    Assert.NotNull(TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentMonZoon2Id));
                    Assert.IsNull(TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentPyromazId));
                    Assert.IsNull(TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentPyromaz2Id));
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }
    }
}
