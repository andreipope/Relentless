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
        [Timeout(int.MaxValue)]
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
                           player.CardAbilityUsed(playerZnowmanId, Enumerators.AbilityType.ENEMY_THAT_ATTACKS_BECOME_FROZEN, new List<ParametrizedAbilityInstanceId>());
                           player.CardPlay(playerSlabId, ItemPosition.Start);
                           player.CardPlay(playerSlab2Id, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentZnowmanId, ItemPosition.Start);
                           opponent.CardAbilityUsed(opponentZnowmanId, Enumerators.AbilityType.ENEMY_THAT_ATTACKS_BECOME_FROZEN, new List<ParametrizedAbilityInstanceId>());
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
                    Assert.AreEqual(false, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerSlab2Id)).IsStun);

                    Assert.AreEqual(3, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentSlabId)).CurrentHp);
                    Assert.AreEqual(1, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentZnowmanId)).CurrentHp);
                    Assert.AreEqual(true, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentZnowmanId)).HasHeavy);
                    Assert.AreEqual(true, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentSlabId)).IsStun);
                    Assert.AreEqual(false, ((BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentSlab2Id)).IsStun);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }

    
        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Jetter()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 2, new DeckCardData("Jetter", 2));
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 2, new DeckCardData("Jetter", 2));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerJetter1 = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Jetter", 1);
                InstanceId playerJetter2 = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Jetter", 2);

                InstanceId opponentJetter1 = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Jetter", 1);
                InstanceId opponentJetter2 = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Jetter", 2);

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
                       player => player.CardPlay(playerJetter1, ItemPosition.Start),
                       opponent =>
                       {
                           opponent.CardPlay(opponentJetter1, ItemPosition.Start, playerJetter1);
                       },
                       player => player.CardPlay(playerJetter2, ItemPosition.Start, opponentJetter1),
                       opponent =>
                       {
                           opponent.CardPlay(opponentJetter2, ItemPosition.Start, playerJetter2);
                       },
                };

                BoardUnitModel playerJetter1Model = null;
                BoardUnitModel playerJetter2Model = null;
                BoardUnitModel opponentJetter1Model = null;
                BoardUnitModel opponentJetter2Model = null;

                Action validateEndState = () =>
                {
                    playerJetter1Model = (BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerJetter1);
                    playerJetter2Model = (BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerJetter2);
                    opponentJetter1Model = (BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentJetter1);
                    opponentJetter2Model = (BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentJetter2);

                    Assert.NotNull(playerJetter1Model);
                    Assert.NotNull(playerJetter2Model);
                    Assert.NotNull(opponentJetter1Model);
                    Assert.NotNull(opponentJetter2Model);

                    Assert.AreEqual(playerJetter1Model.MaxCurrentHp - 1, playerJetter1Model.CurrentHp);
                    Assert.AreEqual(playerJetter2Model.MaxCurrentHp - 1, playerJetter2Model.CurrentHp);
                    Assert.AreEqual(opponentJetter1Model.MaxCurrentHp - 1, opponentJetter1Model.CurrentHp);
                    Assert.AreEqual(opponentJetter2Model.MaxCurrentHp, opponentJetter2Model.CurrentHp);
                };


                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Maelstrom()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 2, new DeckCardData[]
                {
                    new DeckCardData("Maelstrom", 1),
                    new DeckCardData("Whizpar", 2)
                });
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 2, new DeckCardData[]
                {
                    new DeckCardData("Maelstrom", 1),
                    new DeckCardData("Whizpar", 2)
                });

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerWhizparId1 = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Whizpar", 1);
                InstanceId playerWhizparId2 = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Whizpar", 2);
                InstanceId playerMaelstromId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Maelstrom", 1);

                InstanceId opponentWhizparId1 = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Whizpar", 1);
                InstanceId opponentWhizparId2 = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Whizpar", 2);
                InstanceId opponentMaelstromrId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Maelstrom", 1);

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
                           player.CardPlay(playerWhizparId1, ItemPosition.Start);
                           player.CardPlay(playerWhizparId2, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentWhizparId1, ItemPosition.Start);
                           opponent.CardPlay(opponentWhizparId2, ItemPosition.Start);
                       },
                       player =>
                       {
                           player.CardAttack(playerWhizparId1, opponentWhizparId1);
                           player.CardPlay(playerMaelstromId, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentWhizparId2, ItemPosition.Start);
                           opponent.CardPlay(opponentMaelstromrId, ItemPosition.Start);
                       },
                       player => {},
                       opponent => {},
                   };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(0, TestHelper.BattlegroundController.PlayerBoardCards.Count);
                    Assert.AreEqual(1, TestHelper.BattlegroundController.OpponentBoardCards.Count);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Izze()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 2, new DeckCardData[]
                {
                    new DeckCardData("Izze", 2),
                    new DeckCardData("Burrrnn", 1),
                    new DeckCardData("Slab", 1)
                });
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 2, new DeckCardData[]
                {
                    new DeckCardData("Izze", 2),
                    new DeckCardData("Burrrnn", 1),
                    new DeckCardData("Slab", 1)
                });

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerIzze1 = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Izze", 1);
                InstanceId playerIzze2 = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Izze", 2);
                InstanceId playerBurrrnn = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Burrrnn", 1);
                InstanceId playerSlab = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Slab", 1);

                InstanceId opponentIzze1 = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Izze", 1);
                InstanceId opponentIzze2 = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Izze", 2);
                InstanceId opponentBurrrnn = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Burrrnn", 1);
                InstanceId opponentSlab = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Slab", 1);


                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                       player => {},
                       opponent => {},
                       player =>
                       {
                           player.CardPlay(playerBurrrnn, ItemPosition.Start);
                           player.CardPlay(playerSlab, ItemPosition.Start);
                           player.CardPlay(playerIzze1, ItemPosition.Start);
                           player.CardPlay(playerIzze2, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentSlab, ItemPosition.Start);
                           opponent.CardPlay(opponentBurrrnn, ItemPosition.Start);
                           opponent.CardPlay(opponentIzze1, ItemPosition.Start);
                           opponent.CardPlay(opponentIzze2, ItemPosition.Start);
                       },
                       player =>
                       {
                           player.CardAttack(playerIzze1, opponentSlab);
                           player.CardAttack(playerIzze2, opponentBurrrnn);
                       },
                       opponent =>
                       {
                           opponent.CardAttack(opponentIzze1, playerBurrrnn);
                           opponent.CardAttack(opponentIzze2, playerSlab);
                       },
                };

                BoardUnitModel playerBurrrnnModel = null;
                BoardUnitModel playerSlabModel = null;
                BoardUnitModel opponentSlabModel = null;
                BoardUnitModel opponentBurrrnnModel = null;

                Action validateEndState = () =>
                {
                    playerBurrrnnModel = (BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerBurrrnn);
                    playerSlabModel = (BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerSlab);
                    opponentSlabModel = (BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentSlab);
                    opponentBurrrnnModel = (BoardUnitModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentBurrrnn);

                    Assert.Null(playerBurrrnnModel);
                    Assert.NotNull(playerSlabModel);
                    Assert.NotNull(opponentSlabModel);
                    Assert.Null(opponentBurrrnnModel);

                    Assert.AreEqual(playerSlabModel.MaxCurrentHp - 1, playerSlabModel.CurrentHp);
                    Assert.AreEqual(opponentSlabModel.MaxCurrentHp - 1, opponentSlabModel.CurrentHp);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }
    }
}
