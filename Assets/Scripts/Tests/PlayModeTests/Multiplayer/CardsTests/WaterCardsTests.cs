using System;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using UnityEngine.TestTools;
using System.Linq;
using UnityEngine;

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
                    new DeckCardData("Zlab", 5),
                    new DeckCardData("Znowman", 1)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    new DeckCardData("Zlab", 5),
                    new DeckCardData("Znowman", 1)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerZnowmanId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Znowman", 1);
                InstanceId playerZlabId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zlab", 1);
                InstanceId playerZlab2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zlab", 2);

                InstanceId opponentZnowmanId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Znowman", 1);
                InstanceId opponentZlabId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zlab", 1);
                InstanceId opponentZlab2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zlab", 2);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                       player => {},
                       opponent => {},
                       player => {},
                       opponent => {},
                       player => {
                           player.CardPlay(playerZnowmanId, ItemPosition.Start);
                           player.CardAbilityUsed(playerZnowmanId, Enumerators.AbilityType.ENEMY_THAT_ATTACKS_BECOME_FROZEN, new List<ParametrizedAbilityInstanceId>());
                           player.CardAbilityUsed(playerZnowmanId, Enumerators.AbilityType.STUN, new List<ParametrizedAbilityInstanceId>());
                           player.CardPlay(playerZlabId, ItemPosition.Start);
                           player.CardPlay(playerZlab2Id, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentZnowmanId, ItemPosition.Start);
                           opponent.CardAbilityUsed(opponentZnowmanId, Enumerators.AbilityType.ENEMY_THAT_ATTACKS_BECOME_FROZEN, new List<ParametrizedAbilityInstanceId>());
                           opponent.CardAbilityUsed(opponentZnowmanId, Enumerators.AbilityType.STUN, new List<ParametrizedAbilityInstanceId>());
                           opponent.CardPlay(opponentZlabId, ItemPosition.Start);
                           opponent.CardPlay(opponentZlab2Id, ItemPosition.Start);
                       },
                       player =>
                       {
                           player.CardAttack(playerZlabId, opponentZnowmanId);
                       },
                       opponent =>
                       {
                           opponent.CardAttack(opponentZlab2Id, playerZnowmanId);
                           opponent.CardAttack(opponentZnowmanId, playerZlab2Id);
                       },
                       player =>
                       {
                           player.CardAttack(playerZnowmanId, opponentZlabId);
                           
                       },
                       opponent =>
                       {
                           Assert.AreEqual(true, ((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerZlabId)).IsStun);
                           Assert.AreEqual(true, ((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerZlab2Id)).IsStun);
                       },
                       player =>
                       {
                           Assert.AreEqual(true, ((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentZlabId)).IsStun);
                           Assert.AreEqual(true, ((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentZlab2Id)).IsStun);
                       },
                };

                Action validateEndState = () => { };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }

    
        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Jetter()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 2,
                    new DeckCardData("Jetter", 1),
                    new DeckCardData("Hot", 20)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 2,
                    new DeckCardData("Jetter", 1),
                    new DeckCardData("Hot", 20)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerJetter = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Jetter", 1);
                InstanceId playerHotId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Hot", 1);
                InstanceId playerHot2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Hot", 2);

                InstanceId opponentJetter = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Jetter", 1);
                InstanceId opponentHotId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Hot", 1);
                InstanceId opponentHot2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Hot", 2);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                       player => {},
                       opponent => {},
                       player =>
                       {
                           player.CardPlay(playerHotId, ItemPosition.Start);
                           player.CardPlay(playerHot2Id, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentHotId, ItemPosition.Start);
                           opponent.CardPlay(opponentHot2Id, ItemPosition.Start);
                       },
                       player =>
                       {
                           player.CardAttack(playerHotId, pvpTestContext.GetOpponentPlayer().InstanceId);
                           player.CardAttack(playerHot2Id, pvpTestContext.GetOpponentPlayer().InstanceId);
                       },
                       opponent =>
                       {
                           opponent.CardAttack(opponentHotId, pvpTestContext.GetCurrentPlayer().InstanceId);
                           opponent.CardAttack(opponentHot2Id, pvpTestContext.GetCurrentPlayer().InstanceId);
                       },
                       player =>
                       {
                           player.CardPlay(playerJetter, ItemPosition.Start, opponentHotId);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentJetter, ItemPosition.Start, playerHotId);
                       }
                };

                Action validateEndState = () =>
                {
                    int value = 4;

                    CardModel playerHotUnit = (CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerHotId);
                    CardModel opponentHotUnit = (CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentHotId);

                    Assert.AreEqual(playerHotUnit.MaxCurrentDefense - value, playerHotUnit.CurrentDefense);
                    Assert.AreEqual(opponentHotUnit.MaxCurrentDefense - value, opponentHotUnit.CurrentDefense);
                };


                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Maelztrom()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 2,
                    new DeckCardData("Maelztrom", 1),
                    new DeckCardData("Whizper", 20));
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 2,
                    new DeckCardData("Maelztrom", 1),
                    new DeckCardData("Whizper", 20));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerWhizperId1 = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Whizper", 1);
                InstanceId playerWhizperId2 = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Whizper", 2);
                InstanceId playerMaelstromId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Maelztrom", 1);

                InstanceId opponentWhizperId1 = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Whizper", 1);
                InstanceId opponentWhizperId2 = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Whizper", 2);
                InstanceId opponentMaelstromrId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Maelztrom", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                   {
                       player => {},
                       opponent => {},
                       player =>
                       {
                           player.CardPlay(playerWhizperId1, ItemPosition.Start);
                           player.CardPlay(playerWhizperId2, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentWhizperId1, ItemPosition.Start);
                           opponent.CardPlay(opponentWhizperId2, ItemPosition.Start);
                       },
                       player =>
                       {
                           player.CardPlay(playerMaelstromId, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentWhizperId1, ItemPosition.Start);
                           opponent.CardPlay(opponentWhizperId2, ItemPosition.Start);
                       },
                       player => {},
                       opponent => {
                           opponent.CardPlay(opponentMaelstromrId, ItemPosition.Start);
                       },
                       player => {},
                       opponent => {}
                   };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(0, pvpTestContext.GetCurrentPlayer().CardsOnBoard.Count);
                    Assert.AreEqual(1, pvpTestContext.GetOpponentPlayer().CardsOnBoard.Count);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Ice()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 2,
                    new DeckCardData("Ice", 2),
                    new DeckCardData("Zlab", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 2,
                    new DeckCardData("Ice", 2),
                    new DeckCardData("Zlab", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerIce1 = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Ice", 1);
                InstanceId playerZlab = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zlab", 1);

                InstanceId opponentIce1 = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Ice", 1);
                InstanceId opponentZlab = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zlab", 1);


                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                       player => {},
                       opponent => {},
                       player =>
                       {
                           player.CardPlay(playerZlab, ItemPosition.Start);
                           player.CardPlay(playerIce1, ItemPosition.Start);
                           player.CardAbilityUsed(playerIce1, Enumerators.AbilityType.STUN, new List<ParametrizedAbilityInstanceId>());

                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentZlab, ItemPosition.Start);
                           opponent.CardPlay(opponentIce1, ItemPosition.Start);
                           opponent.CardAbilityUsed(opponentIce1, Enumerators.AbilityType.STUN, new List<ParametrizedAbilityInstanceId>());
                       },
                       player =>
                       {
                           player.CardAttack(playerIce1, opponentZlab);
                       },
                       opponent =>
                       {
                           opponent.CardAttack(opponentIce1, playerZlab);
                       },
                };

                Action validateEndState = () =>
                {
                    Assert.IsTrue(((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerZlab)).IsStun);
                    Assert.IsTrue(((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentZlab)).IsStun);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Ztalagmite()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 2,
                    new DeckCardData("Ztalagmite", 5));
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 2,
                    new DeckCardData("Ztalagmite", 5));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerZtalagmite1 = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Ztalagmite", 1);
                InstanceId playerZtalagmite2 = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Ztalagmite", 2);
                InstanceId opponentZtalagmite1 = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Ztalagmite", 1);
                InstanceId opponentZtalagmite2 = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Ztalagmite", 2);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                       player => player.CardPlay(playerZtalagmite1, ItemPosition.Start),
                       opponent => opponent.CardPlay(opponentZtalagmite1, ItemPosition.Start),
                       player => player.CardPlay(playerZtalagmite2, ItemPosition.Start),
                       opponent => opponent.CardPlay(opponentZtalagmite2, ItemPosition.Start)
                };

                Action validateEndState = () =>
                {
                    CardModel playerZtalagmite1Model = (CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerZtalagmite1);
                    CardModel playerZtalagmite2Model = (CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerZtalagmite2);
                    CardModel opponentZtalagmite1Model = (CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentZtalagmite1);
                    CardModel opponentZtalagmite2Model = (CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentZtalagmite2);

                    Assert.AreEqual(playerZtalagmite1Model.Card.Prototype.Defense, playerZtalagmite1Model.CurrentDefense);
                    Assert.AreEqual(playerZtalagmite2Model.Card.Prototype.Defense + 2, playerZtalagmite2Model.CurrentDefense);
                    Assert.AreEqual(opponentZtalagmite1Model.Card.Prototype.Defense + 2, opponentZtalagmite1Model.CurrentDefense);
                    Assert.AreEqual(opponentZtalagmite2Model.Card.Prototype.Defense + 4, opponentZtalagmite2Model.CurrentDefense);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Ozmoziz()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 2,
                    new DeckCardData("Ozmoziz", 10));
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 2,
                    new DeckCardData("Ozmoziz", 10));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerOzmozizId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Ozmoziz", 1);
                InstanceId playerOzmoziz1Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Ozmoziz", 2);
                InstanceId opponentOzmozizId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Ozmoziz", 1);
                InstanceId opponentOzmoziz1Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Ozmoziz", 2);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                    player => player.CardPlay(playerOzmozizId, ItemPosition.Start),
                    opponent => opponent.CardPlay(opponentOzmozizId, ItemPosition.Start),
                    player =>player.CardAttack(playerOzmozizId, pvpTestContext.GetOpponentPlayer().InstanceId),
                    opponent =>
                    {
                        opponent.CardPlay(opponentOzmoziz1Id, ItemPosition.Start);
                        opponent.CardAttack(opponentOzmozizId, pvpTestContext.GetCurrentPlayer().InstanceId);
                    },
                    player => {},
                    opponent => opponent.CardAttack(opponentOzmoziz1Id, pvpTestContext.GetCurrentPlayer().InstanceId),
                    player => player.CardPlay(playerOzmoziz1Id, ItemPosition.Start)
                };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(pvpTestContext.GetCurrentPlayer().GooVials, 5);
                    Assert.AreEqual(pvpTestContext.GetOpponentPlayer().GooVials, 5);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Hozer()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 2,
                    new DeckCardData("Hozer", 5));
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 2,
                    new DeckCardData("Hozer", 5));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerHoZer1 = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Hozer", 1);
                InstanceId playerHoZer2 = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Hozer", 2);
                InstanceId opponentHoZer1 = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Hozer", 1);
                InstanceId opponentHoZer2 = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Hozer", 2);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                       player => player.CardPlay(playerHoZer1, ItemPosition.Start),
                       opponent => opponent.CardPlay(opponentHoZer1, ItemPosition.Start),
                       player => player.CardPlay(playerHoZer2, ItemPosition.Start),
                       opponent => opponent.CardPlay(opponentHoZer2, ItemPosition.Start)
                };

                Action validateEndState = () =>
                {
                    CardModel playerHoZer1Model = (CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerHoZer1);
                    CardModel playerHoZer2Model = (CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerHoZer2);
                    CardModel opponentHoZer1Model = (CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentHoZer1);
                    CardModel opponentHoZer2Model = (CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentHoZer2);

                    Assert.AreEqual(playerHoZer1Model.Card.Prototype.Damage, playerHoZer1Model.CurrentDamage);
                    Assert.AreEqual(playerHoZer2Model.Card.Prototype.Damage + 1, playerHoZer2Model.CurrentDamage);
                    Assert.AreEqual(opponentHoZer1Model.Card.Prototype.Damage + 1, opponentHoZer1Model.CurrentDamage);
                    Assert.AreEqual(opponentHoZer2Model.Card.Prototype.Damage + 2, opponentHoZer2Model.CurrentDamage);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Zlider()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 2,
                    new DeckCardData("Zlider", 1),
                    new DeckCardData("Zlab", 15));
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 2,
                    new DeckCardData("Zlider", 1),
                    new DeckCardData("Zlab", 15));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerZliderId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zlider", 1);
                InstanceId playerZlabId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zlab", 1);

                InstanceId opponentZliderId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zlider", 1);
                InstanceId opponentZlabId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zlab", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                    player => {},
                    opponent => {},
                    player => {},
                    opponent => {},
                    player =>
                    {
                        player.CardPlay(playerZlabId, ItemPosition.Start);
                    },
                    opponent =>
                    {
                        opponent.CardPlay(opponentZlabId, ItemPosition.Start);
                    },
                    player =>
                    {
                        player.CardPlay(playerZliderId, ItemPosition.Start, opponentZlabId);
                    },
                    opponent =>
                    {
                        opponent.CardPlay(opponentZliderId, ItemPosition.Start, playerZlabId);
                    },
                };

                Action validateEndState = () =>
                {
                    Assert.IsTrue(((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerZlabId))
                        .GameMechanicDescriptionsOnUnit.Contains(Enumerators.GameMechanicDescription.Distract));
                    Assert.IsTrue(((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentZlabId))
                       .GameMechanicDescriptionsOnUnit.Contains(Enumerators.GameMechanicDescription.Distract));
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Geyzer()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 2,
                    new DeckCardData("Geyzer", 5));
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 2,
                    new DeckCardData("Geyzer", 5));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerGeyzerId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Geyzer", 1);
                InstanceId playerGeyzer1Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Geyzer", 2);
                InstanceId opponentGeyzerId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Geyzer", 1);
                InstanceId opponentGeyzer1Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Geyzer", 2);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                    player => player.CardPlay(playerGeyzerId, ItemPosition.Start),
                    opponent =>
                    {
                        opponent.LetsThink(1f);
                        opponent.CardPlay(opponentGeyzerId, ItemPosition.Start);
                        opponent.LetsThink(1f);
                        opponent.CardPlay(opponentGeyzer1Id, ItemPosition.Start);
                    },
                    player => player.CardPlay(playerGeyzer1Id, ItemPosition.Start)
                };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(3, pvpTestContext.GetCurrentPlayer().GooVials);
                    Assert.AreEqual(3, pvpTestContext.GetOpponentPlayer().GooVials);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        [Category("PlayQuickSubset2")]
        public IEnumerator Freezee()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 2,
                    new DeckCardData("Freezee", 2),
                    new DeckCardData("Zlab", 20));
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 2,
                    new DeckCardData("Freezee", 2),
                    new DeckCardData("Zlab", 20));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerFreezeeId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Freezee", 1);
                InstanceId playerFreezee2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Freezee", 2);
                InstanceId playerZlabId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zlab", 1);
                InstanceId playerZlab2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zlab", 2);
                InstanceId playerZlab3Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zlab", 3);
                InstanceId opponentFreezeeId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Freezee", 1);
                InstanceId opponentFreezee2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Freezee", 2);
                InstanceId opponentZlabId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zlab", 1);
                InstanceId opponentZlab2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zlab", 2);
                InstanceId opponentZlab3Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zlab", 3);

                CardModel playerUnitZlab = null;
                CardModel playerUnitZlab2 = null;
                CardModel playerUnitZlab3 = null;

                CardModel opponentUnitZlab = null;
                CardModel opponentUnitZlab2 = null;
                CardModel opponentUnitZlab3 = null;

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
                        player.CardPlay(playerZlabId, ItemPosition.End);
                        player.CardPlay(playerZlab2Id, ItemPosition.End);
                        player.CardPlay(playerZlab3Id, ItemPosition.End);                       
                    },
                    opponent =>
                    {
                        opponent.CardPlay(opponentZlabId, ItemPosition.End);
                        opponent.CardPlay(opponentZlab2Id, ItemPosition.End);
                        opponent.CardPlay(opponentZlab3Id, ItemPosition.End);
                    },
                    player =>
                    {
                        playerUnitZlab = ((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerZlabId));
                        playerUnitZlab2 = ((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerZlab2Id));
                        playerUnitZlab3 = ((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerZlab3Id));
                        player.CardPlay(playerFreezeeId, ItemPosition.End, opponentZlab2Id);
                        player.CardPlay(playerFreezee2Id, ItemPosition.End, opponentZlab2Id);
                    },
                    opponent =>
                    {
                        opponentUnitZlab = ((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentZlabId));
                        opponentUnitZlab2 = ((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentZlab2Id));
                        opponentUnitZlab3 = ((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentZlab3Id));
                        opponent.CardPlay(opponentFreezeeId, ItemPosition.End, playerZlab2Id);
                        opponent.CardPlay(opponentFreezee2Id, ItemPosition.End, playerZlab2Id);
                    },
                    player =>
                    {
                        Assert.IsTrue(playerUnitZlab.IsStun);
                        Assert.IsTrue(playerUnitZlab2.IsStun);
                        Assert.IsTrue(playerUnitZlab3.IsStun);
                        Assert.IsTrue(opponentUnitZlab.IsStun);
                        Assert.IsTrue(opponentUnitZlab2.IsStun);
                        Assert.IsTrue(opponentUnitZlab3.IsStun);
                    },
                };

                Action validateEndState = () => {};

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        [Category("PlayQuickSubset2")]
        public IEnumerator Zhatterer()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 2,
                    new DeckCardData("Zlab", 1),
                    new DeckCardData("Freezee", 1),
                    new DeckCardData("Zhatterer", 10));
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 2,
                    new DeckCardData("Zlab", 1),
                    new DeckCardData("Freezee", 1),
                    new DeckCardData("Zhatterer", 10));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerZlabId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zlab", 1);
                InstanceId playerZhattererId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zhatterer", 1);
                InstanceId playerFreezeeId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Freezee", 1);
                InstanceId opponentZlabId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zlab", 1);
                InstanceId opponentZhattererId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zhatterer", 1);
                InstanceId opponentFreezeeId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Freezee", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                    player => {},
                    opponent => {},
                    player => {},
                    opponent => {},
                    player =>
                    {
                        player.CardPlay(playerZlabId, ItemPosition.Start);
                    },
                    opponent =>
                    {
                        opponent.CardPlay(opponentZlabId, ItemPosition.Start);
                        opponent.CardPlay(opponentFreezeeId, ItemPosition.Start, playerZlabId);
                        opponent.CardPlay(opponentZhattererId, ItemPosition.Start, playerZlabId);
                    },
                    player =>
                    {
                        player.CardPlay(playerFreezeeId, ItemPosition.Start, opponentZlabId);
                        player.CardPlay(playerZhattererId, ItemPosition.Start, opponentZlabId);
                    },
                    player => {},
                    opponent => {},
                };

                Action validateEndState = () =>
                {
                    Assert.Null((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerZlabId));
                    Assert.Null((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentZlabId));
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Icicle()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 2,
                    new DeckCardData("Icicle", 1),
                    new DeckCardData("Cerberuz", 5));

                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 2,
                    new DeckCardData("Icicle", 1),
                    new DeckCardData("Cerberuz", 5));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerIcicleId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Icicle", 1);
                InstanceId playerCerberuzId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Cerberuz", 1);
                InstanceId opponentIcicleId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Icicle", 1);
                InstanceId opponentCerberuzId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Cerberuz", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                    player =>  player.CardPlay(playerCerberuzId, ItemPosition.Start),
                    opponent => opponent.CardPlay(opponentCerberuzId, ItemPosition.Start),
                    player => player.CardPlay(playerIcicleId, ItemPosition.Start, opponentCerberuzId),
                    opponent =>opponent.CardPlay(opponentIcicleId, ItemPosition.Start, playerCerberuzId),
                    player => {},
                    opponent => {}
                };

                Action validateEndState = () =>
                {
                    Assert.IsNull(TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCerberuzId));
                    Assert.IsNull(TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCerberuzId));
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Zpring()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 2,
                    new DeckCardData("Zpring", 1),
                    new DeckCardData("Zwoop", 20));

                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 2,
                    new DeckCardData("Zpring", 1),
                    new DeckCardData("Zwoop", 20));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerZpringId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zpring", 1);
                InstanceId playerZwoopId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zwoop", 1);
                InstanceId opponentZpringId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zpring", 1);
                InstanceId opponentZwoopId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zwoop", 1);

                int value = 1;

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                    player => {},
                    opponent => {},
                    player => {},
                    opponent =>
                    {
                        opponent.CardPlay(opponentZwoopId, ItemPosition.Start);
                        opponent.CardPlay(opponentZpringId, ItemPosition.Start);
                    },
                    player =>
                    {
                        player.CardPlay(playerZpringId, ItemPosition.Start);
                        player.CardPlay(playerZwoopId, ItemPosition.Start);
                    },
                    opponent => {},
                    player => {},
                    opponent =>
                    {
                        Assert.AreEqual(value, pvpTestContext.GetOpponentPlayer().ExtraGoo);
                        Assert.AreEqual(value, pvpTestContext.GetCurrentPlayer().ExtraGoo);
                        opponent.CardAttack(opponentZwoopId, playerZpringId);
                    },
                    player =>
                    {
                        player.CardAttack(playerZwoopId, opponentZpringId);
                    },
                    opponent => {},
                };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(0, pvpTestContext.GetCurrentPlayer().ExtraGoo);
                    Assert.AreEqual(0, pvpTestContext.GetOpponentPlayer().ExtraGoo);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        [Category("PlayQuickSubset2")]
        public IEnumerator Blizzard()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 2,
                    new DeckCardData("Blizzard", 1),
                    new DeckCardData("Zlab", 20));

                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 2,
                    new DeckCardData("Blizzard", 1),
                    new DeckCardData("Zlab", 20));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerBlizzardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Blizzard", 1);
                InstanceId playerZlabId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zlab", 1);
                InstanceId playerZlab2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zlab", 2);
                InstanceId opponentBlizzardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Blizzard", 1);
                InstanceId opponentZlabId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zlab", 1);
                InstanceId opponentZlab2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zlab", 2);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                    player => {},
                    opponent => {},
                    player => {},
                    opponent => {},
                    player => {},
                    opponent =>
                    {
                        opponent.CardPlay(opponentZlabId, ItemPosition.Start);
                        opponent.CardPlay(opponentZlab2Id, ItemPosition.Start);
                    },
                    player =>
                    {
                        player.CardPlay(playerZlabId, ItemPosition.Start);
                        player.CardPlay(playerZlab2Id, ItemPosition.Start);
                    },
                    opponent =>
                    {
                        opponent.CardPlay(opponentBlizzardId, ItemPosition.Start);
                    },
                    player =>
                    {
                        player.CardPlay(playerBlizzardId, ItemPosition.Start);
                    },
                    opponent =>
                    {
                        Assert.IsTrue(((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerZlabId)).IsStun);
                        Assert.IsTrue(((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerZlab2Id)).IsStun);
                        Assert.IsTrue(((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentZlabId)).IsStun);
                        Assert.IsTrue(((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentZlab2Id)).IsStun);
                    },
                };

                Action validateEndState = () => {};

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Zplash()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0,
                    new DeckCardData("Zplash", 1),
                    new DeckCardData("Trunk", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    new DeckCardData("Zplash", 1),
                    new DeckCardData("Trunk", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);
                
                InstanceId playerZplashId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zplash", 1);
                InstanceId playerTrunk1Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Trunk", 1);
                InstanceId playerTrunk2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Trunk", 2);

                InstanceId opponentZplashId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zplash", 1);
                InstanceId opponentTrunk1Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Trunk", 1);
                InstanceId opponentTrunk2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Trunk", 2);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                   {
                       player => {},
                       opponent => {},
                       player =>
                       {
                           player.CardPlay(playerTrunk1Id, ItemPosition.Start);
                           player.CardPlay(playerTrunk2Id, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentTrunk1Id, ItemPosition.Start);
                           opponent.CardPlay(opponentTrunk2Id, ItemPosition.Start);
                       },
                       player =>
                       {
                           player.CardPlay(playerZplashId, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentZplashId, ItemPosition.Start);
                       },
                       player => {},
                       opponent => {}
                   };

                Action validateEndState = () =>
                {
                    int damageValue = 2;
                    CardModel trunk1OpponentModel  = ((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentTrunk1Id));
                    Assert.AreEqual(trunk1OpponentModel.MaxCurrentDefense-damageValue, trunk1OpponentModel.CurrentDefense);
                    CardModel trunk2OpponentModel  = ((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentTrunk2Id));
                    Assert.AreEqual(trunk2OpponentModel.MaxCurrentDefense-damageValue, trunk2OpponentModel.CurrentDefense);

                    CardModel trunk1PlayerModel  = ((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerTrunk1Id));
                    Assert.AreEqual(trunk1PlayerModel.MaxCurrentDefense-damageValue, trunk1PlayerModel.CurrentDefense);
                    CardModel trunk2PlayerModel  = ((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerTrunk2Id));
                    Assert.AreEqual(trunk2PlayerModel.MaxCurrentDefense-damageValue, trunk2PlayerModel.CurrentDefense);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator ZubZero()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 2,
                    new DeckCardData("Zub-Zero", 1),
                    new DeckCardData("Trunk", 15));

                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 2,
                    new DeckCardData("Zub-Zero", 1),
                    new DeckCardData("Trunk", 15));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerZubZeroId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zub-Zero", 1);

                InstanceId opponentTrunkId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Trunk", 1);
                InstanceId opponentZubZeroId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zub-Zero", 1);;

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                   {
                       player => {},
                       opponent => {},
                       player =>
                       {
                           player.CardPlay(playerZubZeroId, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentTrunkId, ItemPosition.Start);
                       },
                       player => {},
                       opponent =>
                       {
                           opponent.CardPlay(opponentZubZeroId, ItemPosition.Start);
                       },
                       player => {},
                       opponent => {}
                   };

                Action validateEndState = () =>
                {
                    CardModel playerUnit1 = (CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerZubZeroId);
                    CardModel opponentUnit1 = (CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentZubZeroId);

                    Assert.IsTrue(playerUnit1.IsHeavyUnit);
                    Assert.IsTrue(playerUnit1.HasBuffShield);

                    Assert.IsFalse(opponentUnit1.IsHeavyUnit);
                    Assert.IsFalse(opponentUnit1.HasBuffHeavy);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        [Category("PlayQuickSubset2")]
        public IEnumerator Tzunamy()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 2,
                    new DeckCardData("Tzunamy", 1),
                    new DeckCardData("Zlab", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 2,
                    new DeckCardData("Tzunamy", 1),
                    new DeckCardData("Zlab", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerZlabId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zlab", 1);
                InstanceId playerZlab2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zlab", 2);
                InstanceId playerZlab3Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zlab", 3);
                InstanceId playerZlab4Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zlab", 4);
                InstanceId playerTzunamyId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Tzunamy", 1);

                InstanceId opponentZlabId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zlab", 1);
                InstanceId opponentZlab2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zlab", 2);
                InstanceId opponentZlab3Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zlab", 3);
                InstanceId opponentZlab4Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zlab", 4);
                InstanceId opponentTzunamyId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Tzunamy", 1);
                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                   {
                       player => {},
                       opponent => {},
                       player => {},
                       opponent => {},
                       player =>
                       {
                           player.CardPlay(playerZlabId, ItemPosition.Start);
                           player.CardPlay(playerZlab2Id, ItemPosition.Start);
                           player.CardPlay(playerZlab3Id, ItemPosition.Start);
                           player.CardPlay(playerZlab4Id, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentZlabId, ItemPosition.Start);
                           opponent.CardPlay(opponentZlab2Id, ItemPosition.Start);
                           opponent.CardPlay(opponentZlab3Id, ItemPosition.Start);
                           opponent.CardPlay(opponentZlab4Id, ItemPosition.Start);
                       },
                       player =>
                       {
                           player.CardPlay(playerTzunamyId, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentTzunamyId, ItemPosition.Start);
                       },
                       player => {},
                       opponent => {}
                   };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(1, pvpTestContext.GetCurrentPlayer().CardsOnBoard.Count);
                    Assert.AreEqual(1, pvpTestContext.GetOpponentPlayer().CardsOnBoard.Count);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Vortex()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 2,
                    new DeckCardData("Vortex", 1),
                    new DeckCardData("Hot", 15));

                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 2,
                    new DeckCardData("Vortex", 1),
                    new DeckCardData("Zlab", 15));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerVortexId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Vortex", 1);
                InstanceId playerHotId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Hot", 1);

                InstanceId opponentZlabId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zlab", 1);
                InstanceId opponentVortexId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Vortex", 1);


                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                   {
                       player => {},
                       opponent => {},
                       player =>
                       {
                           player.CardPlay(playerHotId, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentZlabId, ItemPosition.Start);
                       },
                       player => {},
                       opponent =>
                       {
                           opponent.CardAttack(opponentZlabId, pvpTestContext.GetCurrentPlayer().InstanceId);
                       },
                       player =>
                       {
                           player.CardPlay(playerVortexId, ItemPosition.Start);
                           player.CardAttack(playerHotId, pvpTestContext.GetOpponentPlayer().InstanceId);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentVortexId, ItemPosition.Start);
                       }
                   };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(4, pvpTestContext.GetCurrentPlayer().CardsOnBoard.Count);
                    Assert.AreEqual(4, pvpTestContext.GetOpponentPlayer().CardsOnBoard.Count);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Brook()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 4,
                    new DeckCardData("Brook", 2),
                    new DeckCardData("Trunk", 10)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 4,
                    new DeckCardData("Brook", 2),
                    new DeckCardData("Trunk", 10)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerTrunkId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Trunk", 1);
                InstanceId playerBrookId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Brook", 1);
                InstanceId opponentTrunkId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Trunk", 1);
                InstanceId opponentBrookId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Brook", 1);
                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                   {
                       player => {},
                       opponent => {},
                       player =>
                       {
                           player.CardPlay(playerTrunkId, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentTrunkId, ItemPosition.Start);
                       },
                       player => 
                       {
                           player.CardPlay(playerBrookId, ItemPosition.Start, opponentTrunkId);
                       },
                       opponent => 
                       {
                           opponent.CardPlay(opponentBrookId, ItemPosition.Start, playerTrunkId);
                       },
                   };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(true, ((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerTrunkId)).IsStun);
                    Assert.AreEqual(true, ((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentTrunkId)).IsStun);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Igloo()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 2,
                    new DeckCardData("Igloo", 1),
                    new DeckCardData("Hot", 20)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 2,
                    new DeckCardData("Igloo", 1),
                    new DeckCardData("Hot", 20)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck)
                {
                    Player1HasFirstTurn = true
                };

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Igloo", 1);
                InstanceId playerHotId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Hot", 1);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Igloo", 1);
                InstanceId opponentHotId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Hot", 1);

                int defence = 2;
                int damage = 3;

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                    player =>
                    {
                        player.CardPlay(playerHotId, ItemPosition.Start);
                        player.CardPlay(playerCardId, ItemPosition.Start, playerHotId);
                    },
                    opponent =>
                    {
                        opponent.CardPlay(opponentHotId, ItemPosition.Start);
                        opponent.CardPlay(opponentCardId, ItemPosition.Start, opponentHotId);
                    },
                };

                Action validateEndState = () =>
                {
                    CardModel playerUnit = (CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerHotId);
                    CardModel opponentUnit = (CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentHotId);
                    Assert.AreEqual(damage, playerUnit.BuffedDamage);
                    Assert.AreEqual(defence, playerUnit.BuffedDefense);
                    Assert.AreEqual(damage, opponentUnit.BuffedDamage);
                    Assert.AreEqual(defence, opponentUnit.BuffedDefense);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            }, 400);
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Znowy()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0,
                    new DeckCardData("Znowy", 1),
                    new DeckCardData("Hot", 20)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    new DeckCardData("Znowy", 1),
                    new DeckCardData("Hot", 20)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Znowy", 1);
                InstanceId playerHotId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Hot", 1);
                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Znowy", 1);
                InstanceId opponentHotId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Hot", 1);

                int damage = 1;
                int defence = 2;

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                    player => { },
                    opponent => { },
                    player =>
                    {
                        player.CardPlay(playerHotId, ItemPosition.Start);
                        player.CardPlay(playerCardId, ItemPosition.Start, playerHotId);
                    },
                    opponent =>
                    {
                        opponent.CardPlay(opponentHotId, ItemPosition.Start);
                        opponent.CardPlay(opponentCardId, ItemPosition.Start, opponentHotId);
                    },
                };

                Action validateEndState = () =>
                {
                    CardModel playerUnit = (CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerHotId);
                    CardModel opponentUnit = (CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentHotId);
                    Assert.AreEqual(damage, playerUnit.BuffedDamage);
                    Assert.AreEqual(defence, playerUnit.BuffedDefense);
                    Assert.AreEqual(damage, opponentUnit.BuffedDamage);
                    Assert.AreEqual(defence, opponentUnit.BuffedDefense);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            }, 400);
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Zteam()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 4,
                    new DeckCardData("Zteam", 1),
                    new DeckCardData("Trunk", 2),
                    new DeckCardData("Extinguisher", 20)
                );
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 4,
                    new DeckCardData("Zteam", 1),
                    new DeckCardData("Trunk", 2),
                    new DeckCardData("Extinguisher", 20)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerZteamId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zteam", 1);
                InstanceId playerTrunk1Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Trunk", 1);
                InstanceId playerTrunk2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Trunk", 2);
                InstanceId playerExtinguisherId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Extinguisher", 1);

                InstanceId opponentZteamId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zteam", 1);
                InstanceId opponentTrunk1Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Trunk", 1);
                InstanceId opponentTrunk2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Trunk", 2);
                InstanceId opponentExtinguisherId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Extinguisher", 1);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                   {
                       player => {},
                       opponent => {},
                       player =>
                       {
                           player.CardPlay(playerTrunk1Id, ItemPosition.Start);
                           player.CardPlay(playerTrunk2Id, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentTrunk1Id, ItemPosition.Start);
                           opponent.CardPlay(opponentTrunk2Id, ItemPosition.Start);
                           opponent.CardPlay(opponentExtinguisherId, ItemPosition.Start);
                       },
                       player => 
                       {
                           player.CardPlay(playerExtinguisherId, ItemPosition.Start);
                           player.LetsThink(10);
                           player.CardPlay(playerZteamId, ItemPosition.Start);
                       },
                       opponent => 
                       {
                       },
                       player => {}
                   };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(1, pvpTestContext.GetCurrentPlayer().CardsOnBoard.Count);
                    Assert.AreEqual(0, pvpTestContext.GetOpponentPlayer().CardsOnBoard.Count);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState);
            });
        }
    }
}
