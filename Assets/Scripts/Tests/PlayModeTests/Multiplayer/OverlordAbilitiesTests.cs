using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Loom.ZombieBattleground.Test.MultiplayerTests
{
    public class OverlordAbilitiesTests : BaseIntegrationTest
    {
        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Fireball()
        {
            return AsyncTest(async () =>
            {
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards(
                    "deck 1", 1,
                    Enumerators.Skill.FIREBALL,
                    Enumerators.Skill.MASS_RABIES,
                    new TestCardData("Zlab", 30)
                );

                Deck playerDeck = PvPTestUtility.GetDeckWithCards(
                    "deck 2", 1,
                    Enumerators.Skill.FIREBALL,
                    Enumerators.Skill.MASS_RABIES,
                    new TestCardData("Zlab", 30)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

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
                           opponent.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(pvpTestContext.GetCurrentPlayer().InstanceId)
                           });
                       },
                       player =>
                       {
                           Assert.AreEqual(pvpTestContext.GetCurrentPlayer().InitialDefense - 2, pvpTestContext.GetCurrentPlayer().Defense);
                           player.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(pvpTestContext.GetOpponentPlayer().InstanceId)
                           });
                       },
                       opponent => Assert.AreEqual(pvpTestContext.GetOpponentPlayer().InitialDefense - 2, pvpTestContext.GetOpponentPlayer().Defense),
                       player => {},
                   };

                Action validateEndState = () =>
                {

                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Reanimate()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 5,
                    Enumerators.Skill.REANIMATE,
                    Enumerators.Skill.NONE,
                    new TestCardData("Puffer", 15));

                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 5,
                    Enumerators.Skill.REANIMATE,
                    Enumerators.Skill.NONE,
                    new TestCardData("Puffer", 15));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerPufferId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Puffer", 1);
                InstanceId playerPuffer2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Puffer", 2);
                InstanceId playerPuffer3Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Puffer", 3);

                InstanceId opponentPufferId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Puffer", 1);
                InstanceId opponentPuffer2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Puffer", 2);
                InstanceId opponentPuffer3Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Puffer", 3);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                   {
                       player =>
                       {
                           player.CardPlay(playerPufferId, ItemPosition.Start);
                           player.CardPlay(playerPuffer2Id, ItemPosition.Start);
                           player.CardPlay(playerPuffer3Id, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentPufferId, ItemPosition.Start);
                           opponent.CardPlay(opponentPuffer2Id, ItemPosition.Start);
                           opponent.CardPlay(opponentPuffer3Id, ItemPosition.Start);
                       },
                       player => {},
                       opponent =>
                       {
                           opponent.CardAttack(opponentPufferId, playerPufferId);
                           opponent.CardAttack(opponentPuffer2Id, playerPuffer2Id);
                           opponent.CardAttack(opponentPuffer3Id, playerPuffer3Id);
                       },
                       player => {},
                       opponent => {},
                       player => {},
                       opponent => {},
                       player =>
                       {
                           player.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(playerPuffer3Id,
                                    new ParametrizedAbilityParameters()
                                    {
                                        CardName = playerPuffer3Id.Id.ToString()
                                    }),
                                new ParametrizedAbilityInstanceId(playerPufferId,
                                    new ParametrizedAbilityParameters()
                                    {
                                        CardName = playerPufferId.Id.ToString()
                                    })
                           });
                       },
                       opponent =>
                       {
                           opponent.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(opponentPuffer2Id,
                                    new ParametrizedAbilityParameters()
                                    {
                                        CardName = opponentPuffer2Id.Id.ToString()
                                    }),
                                new ParametrizedAbilityInstanceId(opponentPuffer3Id,
                                    new ParametrizedAbilityParameters()
                                    {
                                        CardName = opponentPuffer3Id.Id.ToString()
                                    })
                           });
                       },
                       player => {},
                       opponent => {},
                       };

                Action validateEndState = () =>
                {
                    Assert.NotNull(TestHelper.GameplayManager.CurrentPlayer.CardsOnBoard.Select(card => card.InstanceId == playerPuffer3Id));
                    Assert.NotNull(TestHelper.GameplayManager.CurrentPlayer.CardsOnBoard.Select(card => card.InstanceId == playerPufferId));
                    Assert.NotNull(TestHelper.GameplayManager.OpponentPlayer.CardsOnBoard.Select(card => card.InstanceId == opponentPuffer2Id));
                    Assert.NotNull(TestHelper.GameplayManager.OpponentPlayer.CardsOnBoard.Select(card => card.InstanceId == opponentPuffer3Id));
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator HealingTouch()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 5,
                    Enumerators.Skill.HEALING_TOUCH,
                    Enumerators.Skill.ENHANCE,
                    new TestCardData("Zlab", 15));

                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 5,
                    Enumerators.Skill.HEALING_TOUCH,
                    Enumerators.Skill.ENHANCE,
                    new TestCardData("Zlab", 15));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zlab", 1);

                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zlab", 1);

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
                           player.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(playerCardId)
                           });
                       },
                       opponent =>
                       {
                           opponent.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(opponentCardId)
                           });
                       },
                       player => {},
                       opponent => {},
                   };

                Action validateEndState = () =>
                {
                    CardModel playerSlabUnit = (CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCardId);
                    CardModel opponentSlabUnit = (CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCardId);

                    Assert.AreEqual(playerSlabUnit.Card.Prototype.Defense-1, playerSlabUnit.CurrentDefense);
                    Assert.AreEqual(opponentSlabUnit.Card.Prototype.Defense-1, opponentSlabUnit.CurrentDefense);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Mend()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 5,
                    Enumerators.Skill.MEND,
                    Enumerators.Skill.ENHANCE,
                    new TestCardData("Zlab", 15));

                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 5,
                    Enumerators.Skill.MEND,
                    Enumerators.Skill.ENHANCE,
                    new TestCardData("Zlab", 15));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);


                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zlab", 1);

                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zlab", 1);

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
                       player => {},
                       opponent => {},
                       player =>
                       {
                           player.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(pvpTestContext.GetCurrentPlayer().InstanceId)
                           });
                       },
                       opponent =>
                       {
                           opponent.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(pvpTestContext.GetOpponentPlayer().InstanceId)
                           });
                       },
                       player => {},
                       opponent => {},
                   };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(pvpTestContext.GetCurrentPlayer().InitialDefense-1, pvpTestContext.GetCurrentPlayer().Defense);
                    Assert.AreEqual(pvpTestContext.GetOpponentPlayer().InitialDefense-1, pvpTestContext.GetOpponentPlayer().Defense);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Ressurect()
        {
            return AsyncTest(async () =>
            {
            Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 5,
                Enumerators.Skill.RESSURECT,
                Enumerators.Skill.NONE,
                new TestCardData("Bloomer", 15));

            Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 5,
                Enumerators.Skill.RESSURECT,
                Enumerators.Skill.NONE,
                new TestCardData("Bloomer", 15));

            PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

            InstanceId playerPufferId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Bloomer", 1);
            InstanceId playerPuffer2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Bloomer", 2);
            InstanceId playerPuffer3Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Bloomer", 3);

            InstanceId opponentPufferId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Bloomer", 1);
            InstanceId opponentPuffer2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Bloomer", 2);
            InstanceId opponentPuffer3Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Bloomer", 3);

            IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
               {
                       player =>
                       {
                           player.CardPlay(playerPufferId, ItemPosition.Start);
                           player.CardPlay(playerPuffer2Id, ItemPosition.Start);
                           player.CardPlay(playerPuffer3Id, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentPufferId, ItemPosition.Start);
                           opponent.CardPlay(opponentPuffer2Id, ItemPosition.Start);
                           opponent.CardPlay(opponentPuffer3Id, ItemPosition.Start);
                       },
                       player => {},
                       opponent =>
                       {
                           opponent.CardAttack(opponentPufferId, playerPufferId);
                           opponent.CardAttack(opponentPuffer2Id, playerPuffer2Id);
                           opponent.CardAttack(opponentPuffer3Id, playerPuffer3Id);
                       },
                       player => {},
                       opponent => {},
                       player => {},
                       opponent => {},
                       player =>
                       {
                           player.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(playerPuffer3Id,
                                    new ParametrizedAbilityParameters()
                                    {
                                        CardName = playerPuffer3Id.Id.ToString()
                                    })
                           });
                       },
                       opponent =>
                       {
                           opponent.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(opponentPuffer2Id,
                                    new ParametrizedAbilityParameters()
                                    {
                                        CardName = opponentPuffer2Id.Id.ToString()
                                    })
                           });
                       },
                       player => {},
                       opponent => {},
                   };

                Action validateEndState = () =>
                {
                    Assert.NotNull(TestHelper.GameplayManager.CurrentPlayer.CardsOnBoard.Select(card => card.InstanceId == playerPuffer3Id));
                    Assert.NotNull(TestHelper.GameplayManager.OpponentPlayer.CardsOnBoard.Select(card => card.InstanceId == opponentPuffer2Id));
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Enhance()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 5,
                    Enumerators.Skill.ENHANCE,
                    Enumerators.Skill.HEALING_TOUCH,
                    new TestCardData("Keeper", 15));

                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 5,
                    Enumerators.Skill.ENHANCE,
                    Enumerators.Skill.HEALING_TOUCH,
                    new TestCardData("Keeper", 15));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);


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
                       opponent => {},
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
                           player.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(playerCardId),
                                new ParametrizedAbilityInstanceId(pvpTestContext.GetCurrentPlayer().InstanceId)
                           });
                       },
                       opponent =>
                       {
                           opponent.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(opponentCardId),
                                new ParametrizedAbilityInstanceId(pvpTestContext.GetOpponentPlayer().InstanceId)
                           });
                       },
                       player => {},
                       opponent => {},
                   };

                Action validateEndState = () =>
                {
                    CardModel playerUnit = (CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCardId);
                    CardModel opponentUnit = (CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCardId);

                    Assert.AreEqual(pvpTestContext.GetCurrentPlayer().InitialDefense-1, pvpTestContext.GetCurrentPlayer().Defense);
                    Assert.AreEqual(pvpTestContext.GetOpponentPlayer().InitialDefense-1, pvpTestContext.GetOpponentPlayer().Defense);
                    Assert.AreEqual(playerUnit.Card.Prototype.Defense-1, playerUnit.CurrentDefense);
                    Assert.AreEqual(opponentUnit.Card.Prototype.Defense-1, opponentUnit.CurrentDefense);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator PoisonDart()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 4,
                    Enumerators.Skill.POISON_DART,
                    Enumerators.Skill.TOXIC_POWER,
                    new TestCardData("Hazmat", 15));

                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 4,
                    Enumerators.Skill.POISON_DART,
                    Enumerators.Skill.TOXIC_POWER,
                    new TestCardData("Hazmat", 15));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Hazmat", 1);

                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Hazmat", 1);

                int difference = 1;

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
                       opponent => {},
                       player => {},
                       opponent => {},
                       player =>
                       {
                           player.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(opponentCardId)
                           });
                       },
                       opponent =>
                       {
                           opponent.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(playerCardId)
                           });
                       },
                       player => {},
                       opponent => {},
                   };

                Action validateEndState = () =>
                {
                    CardModel playerUnit = (CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCardId);
                    CardModel opponentUnit = (CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCardId);

                    Assert.AreEqual(playerUnit.Card.Prototype.Defense - difference, playerUnit.CurrentDefense);
                    Assert.AreEqual(opponentUnit.Card.Prototype.Defense - difference, opponentUnit.CurrentDefense);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator ToxicPower()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 4,
                    Enumerators.Skill.TOXIC_POWER,
                    Enumerators.Skill.POISON_DART,
                    new TestCardData("Hazmat", 15));

                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 4,
                    Enumerators.Skill.TOXIC_POWER,
                    Enumerators.Skill.POISON_DART,
                    new TestCardData("Hazmat", 15));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Hazmat", 1);

                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Hazmat", 1);

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
                       opponent => {},
                       player => {},
                       opponent => {},
                       player => {},
                       opponent => {},
                       player =>
                       {
                           player.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(playerCardId)
                           });
                           player.CardAttack(playerCardId, pvpTestContext.GetOpponentPlayer().InstanceId);
                       },
                       opponent =>
                       {
                           opponent.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(opponentCardId)
                           });
                           opponent.CardAttack(opponentCardId, pvpTestContext.GetCurrentPlayer().InstanceId);
                       },
                       player => {},
                       opponent => {},
                   };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(pvpTestContext.GetCurrentPlayer().InitialDefense - 4, pvpTestContext.GetCurrentPlayer().Defense);
                    Assert.AreEqual(pvpTestContext.GetOpponentPlayer().InitialDefense - 4, pvpTestContext.GetOpponentPlayer().Defense);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Push()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 3,
                    Enumerators.Skill.PUSH,
                    Enumerators.Skill.DRAW,
                    new TestCardData("Bouncer", 1),
                    new TestCardData("Whizper", 15));

                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 3,
                    Enumerators.Skill.PUSH,
                    Enumerators.Skill.DRAW,
                    new TestCardData("Bouncer", 1),
                    new TestCardData("Whizper", 15));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerBouncerId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Bouncer", 1);
                InstanceId playerWhizperId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Whizper", 1);

                InstanceId opponentBouncerId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Bouncer", 1);
                InstanceId opponentWhizperId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Whizper", 1);

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
                           player.CardPlay(playerBouncerId, ItemPosition.Start);
                           player.CardPlay(playerWhizperId, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentBouncerId, ItemPosition.Start);
                           opponent.CardPlay(opponentWhizperId, ItemPosition.Start);
                       },
                       player => {},
                       opponent => {},
                       player => {},
                       opponent => {},
                       player =>
                       {
                           player.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(playerBouncerId)
                           });
                       },
                       opponent =>
                       {
                           opponent.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(opponentBouncerId)
                           });
                       },
                       player => {},
                       opponent => {},
                   };

                Action validateEndState = () =>
                {
                    Assert.IsNotNull(pvpTestContext.GetCurrentPlayer().CardsInHand.Select(card => card.Card.Prototype.CardKey.MouldId.Id == 32));
                    Assert.IsNotNull(pvpTestContext.GetOpponentPlayer().CardsInHand.Select(card => card.Card.Prototype.CardKey.MouldId.Id == 32));
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Draw()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 3,
                    Enumerators.Skill.DRAW,
                    Enumerators.Skill.NONE,
                    new TestCardData("Whizper", 15));

                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 3,
                    Enumerators.Skill.DRAW,
                    Enumerators.Skill.NONE,
                    new TestCardData("Whizper", 15));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                int countPlayerCards = 0;
                int countOpponentCards = 0;

                InstanceId playerWhizperId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Whizper", 1);
                InstanceId playerWhizper2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Whizper", 2);

                InstanceId opponentWhizperId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Whizper", 1);
                InstanceId opponentWhizper2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Whizper", 2);

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
                           player.CardPlay(playerWhizperId, ItemPosition.Start);
                           player.CardPlay(playerWhizper2Id, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentWhizperId, ItemPosition.Start);
                           opponent.CardPlay(opponentWhizper2Id, ItemPosition.Start);
                       },
                       player =>
                       {
                           countPlayerCards = pvpTestContext.GetCurrentPlayer().CardsInHand.Count;
                           player.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>());
                       },
                       opponent =>
                       {
                           countOpponentCards = pvpTestContext.GetOpponentPlayer().CardsInHand.Count;
                           opponent.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>());
                       },
                       player =>
                       {
                           Assert.AreEqual(countPlayerCards + 2, pvpTestContext.GetCurrentPlayer().CardsInHand.Count);
                       },
                       opponent =>
                       {
                           Assert.AreEqual(countOpponentCards + 2, pvpTestContext.GetOpponentPlayer().CardsInHand.Count);
                       },
                   };

                Action validateEndState = () => {};

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Retreat()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 3,
                    Enumerators.Skill.RETREAT,
                    Enumerators.Skill.NONE,
                    new TestCardData("Bouncer", 1),
                    new TestCardData("Whizper", 15));

            Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 3,
                    Enumerators.Skill.RETREAT,
                    Enumerators.Skill.NONE,
                    new TestCardData("Bouncer", 1),
                    new TestCardData("Whizper", 15));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerBouncerId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Bouncer", 1);
                InstanceId playerWhizperId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Whizper", 1);
                InstanceId playerWhizper2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Whizper", 2);
                InstanceId playerWhizper3Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Whizper", 3);

                InstanceId opponentBouncerId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Bouncer", 1);
                InstanceId opponentWhizperId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Whizper", 1);
                InstanceId opponentWhizper2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Whizper", 2);
                InstanceId opponentWhizper3Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Whizper", 3);
                InstanceId opponentWhizper4Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Whizper", 4);
                InstanceId opponentWhizper5Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Whizper", 5);

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
                           player.CardPlay(playerBouncerId, ItemPosition.Start);
                           player.CardPlay(playerWhizperId, ItemPosition.Start);
                           player.CardPlay(playerWhizper2Id, ItemPosition.Start);
                           player.CardPlay(playerWhizper3Id, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentBouncerId, ItemPosition.Start);
                           opponent.CardPlay(opponentWhizperId, ItemPosition.Start);
                           opponent.CardPlay(opponentWhizper2Id, ItemPosition.Start);
                           opponent.CardPlay(opponentWhizper3Id, ItemPosition.Start);
                           opponent.CardPlay(opponentWhizper4Id, ItemPosition.Start);
                           opponent.CardPlay(opponentWhizper5Id, ItemPosition.Start);
                       },
                       player => {},
                       opponent =>
                       {
                           opponent.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>());
                       },
                       player => {},
                       opponent => {},
                   };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(0, TestHelper.GameplayManager.CurrentPlayer.CardsOnBoard.Count);
                    Assert.AreEqual(0, TestHelper.GameplayManager.OpponentPlayer.CardsOnBoard.Count);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Harden()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0,
                    Enumerators.Skill.HARDEN,
                    Enumerators.Skill.NONE,
                    new TestCardData("Zlab", 15));

                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    Enumerators.Skill.HARDEN,
                    Enumerators.Skill.NONE,
                    new TestCardData("Zlab", 15));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                int value = 2;

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
                           player.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(pvpTestContext.GetCurrentPlayer().InstanceId)
                           });
                       },
                       opponent =>
                       {
                           opponent.OverlordSkillUsed(new SkillId(0),  new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(pvpTestContext.GetOpponentPlayer().InstanceId)
                           });
                       },
                   };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(pvpTestContext.GetCurrentPlayer().InitialDefense + value, pvpTestContext.GetCurrentPlayer().Defense);
                    Assert.AreEqual(pvpTestContext.GetOpponentPlayer().InitialDefense + value, pvpTestContext.GetOpponentPlayer().Defense);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Stoneskin()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0,
                    Enumerators.Skill.STONE_SKIN,
                    Enumerators.Skill.NONE,
                    new TestCardData("Zlab", 15));

                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    Enumerators.Skill.STONE_SKIN,
                    Enumerators.Skill.NONE,
                    new TestCardData("Zlab", 15));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerSlabId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zlab", 1);
                InstanceId playerSlab2Id  = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zlab", 2);

                InstanceId opponentSlabId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zlab", 1);
                InstanceId opponentSlab2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zlab", 2);

                int value = 1;

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
                           player.CardPlay(playerSlabId, ItemPosition.Start);
                           player.CardPlay(playerSlab2Id, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentSlabId, ItemPosition.Start);
                           opponent.CardPlay(opponentSlab2Id, ItemPosition.Start);
                       },
                       player =>
                       {
                           player.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(playerSlabId)
                           });
                       },
                       opponent =>
                       {
                           opponent.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(opponentSlabId)
                           });
                       },
                       player => {},
                       opponent => {},
                   };

                Action validateEndState = () =>
                {
                    CardModel playerUnit = (CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerSlabId);
                    CardModel opponentUnit = (CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentSlabId);

                    Assert.AreEqual(playerUnit.Card.Prototype.Defense + value, playerUnit.CurrentDefense);
                    Assert.AreEqual(opponentUnit.Card.Prototype.Defense + value, opponentUnit.CurrentDefense);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Fortify()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0,
                    Enumerators.Skill.FORTIFY,
                    Enumerators.Skill.NONE,
                    new TestCardData("Zlab", 15));

                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    Enumerators.Skill.FORTIFY,
                    Enumerators.Skill.NONE,
                    new TestCardData("Zlab", 15));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerSlabId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zlab", 1);
                InstanceId playerSlab2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zlab", 2);

                InstanceId opponentSlabId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zlab", 1);
                InstanceId opponentSlab2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zlab", 2);

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
                           player.CardPlay(playerSlabId, ItemPosition.Start);
                           player.CardPlay(playerSlab2Id, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentSlabId, ItemPosition.Start);
                           opponent.CardPlay(opponentSlab2Id, ItemPosition.Start);
                       },
                       player =>
                       {
                           player.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(playerSlabId)
                           });
                       },
                       opponent =>
                       {
                           opponent.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(opponentSlabId)
                           });
                       },
                       player => {},
                       opponent => {},
                   };

                Action validateEndState = () =>
                {
                    Assert.IsTrue(((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerSlabId)).IsHeavyUnit);
                    Assert.IsTrue(((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentSlabId)).IsHeavyUnit);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Phalanx()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0,
                    Enumerators.Skill.PHALANX,
                    Enumerators.Skill.NONE,
                    new TestCardData("Zlab", 15));

                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    Enumerators.Skill.PHALANX,
                    Enumerators.Skill.NONE,
                    new TestCardData("Zlab", 15));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerSlabId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zlab", 1);
                InstanceId playerSlab2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zlab", 2);
                InstanceId playerSlab3Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zlab", 3);

                InstanceId opponentSlabId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zlab", 1);
                InstanceId opponentSlab2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zlab", 2);
                InstanceId opponentSlab3Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zlab", 3);

                int value = 1;

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
                           player.CardPlay(playerSlabId, ItemPosition.Start);
                           player.CardPlay(playerSlab2Id, ItemPosition.Start);
                           player.CardPlay(playerSlab3Id, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentSlabId, ItemPosition.Start);
                           opponent.CardPlay(opponentSlab2Id, ItemPosition.Start);
                           opponent.CardPlay(opponentSlab3Id, ItemPosition.Start);
                       },
                       player =>
                       {
                           player.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(playerSlabId),
                                new ParametrizedAbilityInstanceId(playerSlab2Id),
                                new ParametrizedAbilityInstanceId(playerSlab3Id),
                           });
                       },
                       opponent =>
                       {
                           opponent.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(opponentSlabId),
                                new ParametrizedAbilityInstanceId(opponentSlab2Id),
                                new ParametrizedAbilityInstanceId(opponentSlab3Id),
                           });
                       },
                       player => {},
                       opponent => {},
                   };

                Action validateEndState = () =>
                {
                    CardModel playerSlab = (CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerSlabId);
                    CardModel playerSlab2 = (CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerSlab2Id);
                    CardModel playerSlab3 = (CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerSlab3Id);

                    CardModel opponentSlab = (CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentSlabId);
                    CardModel opponentSlab2 = (CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentSlab2Id);
                    CardModel opponentSlab3 = (CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentSlab3Id);

                    Assert.AreEqual(playerSlab.Card.Prototype.Defense + value, playerSlab.CurrentDefense);
                    Assert.AreEqual(playerSlab2.Card.Prototype.Defense + value, playerSlab2.CurrentDefense);
                    Assert.AreEqual(playerSlab3.Card.Prototype.Defense + value, playerSlab3.CurrentDefense);

                    Assert.AreEqual(opponentSlab.Card.Prototype.Defense + value, opponentSlab.CurrentDefense);
                    Assert.AreEqual(opponentSlab2.Card.Prototype.Defense + value, opponentSlab2.CurrentDefense);
                    Assert.AreEqual(opponentSlab3.Card.Prototype.Defense + value, opponentSlab3.CurrentDefense);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        [Category("PlayQuickSubset")]
        public IEnumerator Freeze()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 2,
                    Enumerators.Skill.FREEZE,
                    Enumerators.Skill.NONE,
                    new TestCardData("Trunk", 15));

                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 2,
                    Enumerators.Skill.FREEZE,
                    Enumerators.Skill.NONE,
                    new TestCardData("Trunk", 15));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerTrunkId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Trunk", 1);
                InstanceId playerTrunk2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Trunk", 2);

                InstanceId opponentTrunkId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Trunk", 1);
                InstanceId opponentTrunk2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Trunk", 2);

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
                           player.CardPlay(playerTrunkId, ItemPosition.Start);
                           player.CardPlay(playerTrunk2Id, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentTrunkId, ItemPosition.Start);
                           opponent.CardPlay(opponentTrunk2Id, ItemPosition.Start);
                       },
                       player =>
                       {
                           player.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(opponentTrunkId)
                           });
                       },
                       opponent =>
                       {
                           opponent.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(playerTrunkId)
                           });
                       },
                   };

                Action validateEndState = () =>
                {
                    Assert.IsTrue(((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentTrunkId)).UnitSpecialStatus == Enumerators.UnitSpecialStatus.FROZEN);
                    Assert.IsTrue(((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerTrunkId)).UnitSpecialStatus == Enumerators.UnitSpecialStatus.FROZEN);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator IceBolt()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 2,
                    Enumerators.Skill.ICE_BOLT,
                    Enumerators.Skill.NONE,
                    new TestCardData("Zlab", 15));

                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 2,
                    Enumerators.Skill.ICE_BOLT,
                    Enumerators.Skill.NONE,
                    new TestCardData("Zlab", 15));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerSlabId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zlab", 1);
                InstanceId playerSlab2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zlab", 2);

                InstanceId opponentSlabId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zlab", 1);
                InstanceId opponentSlab2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zlab", 2);

                int difference = 2;

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
                           player.CardPlay(playerSlabId, ItemPosition.Start);
                           player.CardPlay(playerSlab2Id, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentSlabId, ItemPosition.Start);
                           opponent.CardPlay(opponentSlab2Id, ItemPosition.Start);
                       },
                       player =>
                       {
                           player.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(opponentSlabId)
                           });
                       },
                       opponent =>
                       {
                           opponent.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(playerSlabId)
                           });
                       },
                   };

                Action validateEndState = () =>
                {
                    CardModel playerUnit1 = (CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerSlabId);
                    CardModel opponentUnit1 = (CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerSlabId);

                    Assert.IsTrue(playerUnit1.IsStun);
                    Assert.AreEqual(playerUnit1.Card.Prototype.Defense - difference, playerUnit1.CurrentDefense);

                    Assert.IsTrue(opponentUnit1.IsStun);
                    Assert.AreEqual(opponentUnit1.Card.Prototype.Defense - difference, opponentUnit1.CurrentDefense);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator IceWall()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 2,
                    Enumerators.Skill.ICE_WALL,
                    Enumerators.Skill.NONE,
                    new TestCardData("Frozen", 15));

                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 2,
                    Enumerators.Skill.ICE_WALL,
                    Enumerators.Skill.NONE,
                    new TestCardData("Frozen", 15));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerTrunkId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Frozen", 1);
                InstanceId playerTrunk2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Frozen", 2);

                InstanceId opponentTrunkId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Frozen", 1);
                InstanceId opponentTrunk2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Frozen", 2);

                int buffedDefense = 2;

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
                           player.CardPlay(playerTrunkId, ItemPosition.Start);
                           player.CardPlay(playerTrunk2Id, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentTrunkId, ItemPosition.Start);
                           opponent.CardPlay(opponentTrunk2Id, ItemPosition.Start);
                       },
                       player =>
                       {
                           player.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(playerTrunkId)
                           });
                       },
                       opponent =>
                       {
                           opponent.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(pvpTestContext.GetOpponentPlayer().InstanceId)
                           });
                       },
                       player => {},
                       opponent => {},
                   };

                Action validateEndState = () =>
                {
                    CardModel playerUnit = (CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerTrunkId);

                    Assert.AreEqual(playerUnit.Card.Prototype.Defense + buffedDefense, playerUnit.CurrentDefense);
                    Assert.AreEqual(pvpTestContext.GetOpponentPlayer().InitialDefense + buffedDefense, pvpTestContext.GetOpponentPlayer().Defense);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Shatter()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 2,
                    Enumerators.Skill.FREEZE,
                    Enumerators.Skill.SHATTER,
                    new TestCardData("Zlab", 2),
                    new TestCardData("Maelztrom", 20));

                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 2,
                    Enumerators.Skill.FREEZE,
                    Enumerators.Skill.SHATTER,
                    new TestCardData("Zlab", 2),
                    new TestCardData("Maelztrom", 20));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerSlabId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zlab", 1);
                InstanceId playerSlab2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zlab", 2);
                InstanceId playerMaelztromId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Maelztrom", 1);

                InstanceId opponentSlabId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zlab", 1);
                InstanceId opponentSlab2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zlab", 2);

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
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentSlabId, ItemPosition.Start);
                           opponent.CardPlay(opponentSlab2Id, ItemPosition.Start);
                       },
                       player =>
                       {
                           player.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(opponentSlabId)
                           });
                           player.LetsThink(5);
                           player.OverlordSkillUsed(new SkillId(1), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(opponentSlabId)
                           });
                           player.CardPlay(playerMaelztromId, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(playerMaelztromId)
                           });
                           opponent.OverlordSkillUsed(new SkillId(1), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(playerMaelztromId)
                           });
                       },
                       player => {},
                       opponent => {},
                       player => {},
                       opponent => {},
                   };

                Action validateEndState = () =>
                {
                    Assert.Null((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerMaelztromId));
                    Assert.Null((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentSlabId));
                    Assert.AreEqual(0, TestHelper.GameplayManager.CurrentPlayer.CardsOnBoard.Count);
                    Assert.AreEqual(0, TestHelper.GameplayManager.OpponentPlayer.CardsOnBoard.Count);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator MeteorShower()
        {
            return AsyncTest(async () =>
            {
  				Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 1,
                    Enumerators.Skill.METEOR_SHOWER,
                    Enumerators.Skill.NONE,
                    new TestCardData("Bark", 15));

                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 1,
                    Enumerators.Skill.METEOR_SHOWER,
                    Enumerators.Skill.NONE,
                    new TestCardData("Bark", 15));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                int count = 3;

                InstanceId[] playerCardsId = new InstanceId[count];
                InstanceId[] opponentCardsId = new InstanceId[count];

                for (int i = 0; i < count; i++)
                {
                    playerCardsId[i] = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Bark", i + 1);
                }

                for (int i = 0; i < count; i++)
                {
                    opponentCardsId[i] = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Bark", i + 1);
                }

                List<ParametrizedAbilityInstanceId> targets = new List<ParametrizedAbilityInstanceId>();
                targets.AddRange(playerCardsId.Select(id => new ParametrizedAbilityInstanceId(id, null)));
                targets.AddRange(opponentCardsId.Select(id => new ParametrizedAbilityInstanceId(id, null)));

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
                           for (int i = 0; i < playerCardsId.Length; i++)
                           {
                               player.CardPlay(playerCardsId[i], ItemPosition.Start);
                           }
                       },
                       opponent =>
                       {
                           for (int i = 0; i < opponentCardsId.Length; i++)
                           {
                               opponent.CardPlay(opponentCardsId[i], ItemPosition.Start);
                           }
                       },
                       player =>
                       {
                           player.OverlordSkillUsed(new SkillId(0), targets);
                       },
                       opponent => {},
                       player => {},
                       opponent =>
                       {
                           opponent.OverlordSkillUsed(new SkillId(0), targets);
                       },
                       player => {},
                       opponent => {},
                       player => {},
                       opponent => {},
                   };

                Action validateEndState = () =>
                {
                    Assert.AreEqual(0, pvpTestContext.GetCurrentPlayer().CardsOnBoard.Count);
                    Assert.AreEqual(0, pvpTestContext.GetOpponentPlayer().CardsOnBoard.Count);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Rabies()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 1,
                    Enumerators.Skill.RABIES,
                    Enumerators.Skill.NONE,
                    new TestCardData("Pyromaz", 15));

                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 1,
                    Enumerators.Skill.RABIES,
                    Enumerators.Skill.NONE,
                    new TestCardData("Pyromaz", 15));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerPyromazId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Pyromaz", 1);
                InstanceId playerPyromaz2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Pyromaz", 2);

                InstanceId opponentPyromazId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Pyromaz", 1);
                InstanceId opponentPyromaz2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Pyromaz", 2);

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
                           player.CardPlay(playerPyromazId, ItemPosition.Start);
                           player.CardPlay(playerPyromaz2Id, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentPyromazId, ItemPosition.Start);
                           opponent.CardPlay(opponentPyromaz2Id, ItemPosition.Start);
                       },
                       player =>
                       {
                           player.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(playerPyromazId)
                           });
                       },
                       opponent =>
                       {
                           opponent.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(opponentPyromazId)
                           });
                       },
                       player => {},
                       opponent => {},
                   };

                Action validateEndState = () =>
                {
                    Assert.IsTrue(((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerPyromazId)).HasFeral);
                    Assert.IsTrue(((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentPyromazId)).HasFeral);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator FireBolt()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 1,
                    Enumerators.Skill.FIRE_BOLT,
                    Enumerators.Skill.NONE,
                    new TestCardData("Zlab", 15));

                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 1,
                    Enumerators.Skill.FIRE_BOLT,
                    Enumerators.Skill.NONE,
                    new TestCardData("Zlab", 15));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                int difference = 1;

                InstanceId playerCardId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zlab", 1);

                InstanceId opponentCardId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zlab", 1);

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
                       opponent => {},
                       player => {},
                       opponent => {},
                       player =>
                       {
                           player.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(opponentCardId)
                           });
                       },
                       opponent =>
                       {
                           opponent.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(playerCardId)
                           });
                       },
                       player => {},
                       opponent => {},
                   };

                Action validateEndState = () =>
                {
                    CardModel playerUnit = (CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerCardId);
                    CardModel opponentUnit = (CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentCardId);
                    Assert.AreEqual(playerUnit.Card.Prototype.Defense - difference, playerUnit.CurrentDefense);
                    Assert.AreEqual(opponentUnit.Card.Prototype.Defense - difference, opponentUnit.CurrentDefense);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Fortress()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 0,
                    Enumerators.Skill.FORTRESS,
                    Enumerators.Skill.NONE,
                    new TestCardData("Zlab", 15));

                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 0,
                    Enumerators.Skill.FORTRESS,
                    Enumerators.Skill.NONE,
                    new TestCardData("Zlab", 15));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerSlabId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zlab", 1);
                InstanceId playerSlab2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zlab", 2);

                InstanceId opponentSlabId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zlab", 1);
                InstanceId opponentSlab2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zlab", 2);
                InstanceId opponentSlab3Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zlab", 3);

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
                           player.CardPlay(playerSlabId, ItemPosition.Start);
                           player.CardPlay(playerSlab2Id, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentSlabId, ItemPosition.Start);
                           opponent.CardPlay(opponentSlab2Id, ItemPosition.Start);
                           opponent.CardPlay(opponentSlab3Id, ItemPosition.Start);
                       },
                       player =>
                       {
                           player.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(playerSlabId),
                                new ParametrizedAbilityInstanceId(playerSlab2Id)
                           });

                       },
                       opponent =>
                       {
                           opponent.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(opponentSlabId),
                                new ParametrizedAbilityInstanceId(opponentSlab3Id)
                           });
                       },
                       player => {},
                       opponent => {},
                   };

                Action validateEndState = () =>
                {
                    Assert.IsTrue(((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerSlabId)).IsHeavyUnit);
                    Assert.IsTrue(((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerSlab2Id)).IsHeavyUnit);
                    Assert.IsTrue(((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentSlabId)).IsHeavyUnit);
                    Assert.IsTrue(!((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentSlab2Id)).IsHeavyUnit);
                    Assert.IsTrue(((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentSlab3Id)).IsHeavyUnit);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Blizzard()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 2,
                    Enumerators.Skill.BLIZZARD,
                    Enumerators.Skill.NONE,
                    new TestCardData("Zlab", 15));

                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 2,
                    Enumerators.Skill.BLIZZARD,
                    Enumerators.Skill.NONE,
                    new TestCardData("Zlab", 15));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerSlabId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zlab", 1);
                InstanceId playerSlab2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zlab", 2);
                InstanceId playerSlab3Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zlab", 3);

                InstanceId opponentSlabId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zlab", 1);
                InstanceId opponentSlab2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zlab", 2);
                InstanceId opponentSlab3Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zlab", 3);

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
                           player.CardPlay(playerSlabId, ItemPosition.Start);
                           player.CardPlay(playerSlab2Id, ItemPosition.Start);
                           player.CardPlay(playerSlab3Id, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentSlabId, ItemPosition.Start);
                           opponent.CardPlay(opponentSlab2Id, ItemPosition.Start);
                           opponent.CardPlay(opponentSlab3Id, ItemPosition.Start);
                       },
                       player =>
                       {
                           player.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(opponentSlabId),
                                new ParametrizedAbilityInstanceId(opponentSlab3Id)
                           });
                       },
                       opponent =>
                       {
                           opponent.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(playerSlabId),
                                new ParametrizedAbilityInstanceId(playerSlab3Id)
                           });
                       },
                   };

                Action validateEndState = () =>
                {
                    Assert.IsTrue(((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerSlabId)).IsStun);
                    Assert.IsTrue(!((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerSlab2Id)).IsStun);
                    Assert.IsTrue(((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerSlab3Id)).IsStun);
                    Assert.IsTrue(((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentSlabId)).IsStun);
                    Assert.IsTrue(!((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentSlab2Id)).IsStun);
                    Assert.IsTrue(((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentSlab3Id)).IsStun);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator WindShield()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 3,
                    Enumerators.Skill.WIND_SHIELD,
                    Enumerators.Skill.NONE,
                    new TestCardData("Whizper", 15));

                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 3,
                    Enumerators.Skill.WIND_SHIELD,
                    Enumerators.Skill.NONE,
                    new TestCardData("Whizper", 15));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerWhizperId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Whizper", 1);
                InstanceId playerWhizper2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Whizper", 2);

                InstanceId opponentWhizperId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Whizper", 1);
                InstanceId opponentWhizper2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Whizper", 2);

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
                           player.CardPlay(playerWhizperId, ItemPosition.Start);
                           player.CardPlay(playerWhizper2Id, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentWhizperId, ItemPosition.Start);
                           opponent.CardPlay(opponentWhizper2Id, ItemPosition.Start);
                       },
                       player =>
                       {
                           player.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(playerWhizperId)
                           });
                       },
                       opponent =>
                       {
                           opponent.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(opponentWhizper2Id)
                           });
                       },
                       player => {},
                       opponent => {},
                   };

                Action validateEndState = () =>
                {
                    Assert.IsTrue(((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerWhizperId)).HasBuffShield);
                    Assert.IsTrue(!((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerWhizper2Id)).HasBuffShield);
                    Assert.IsTrue(((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentWhizper2Id)).HasBuffShield);
                    Assert.IsTrue(!((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentWhizperId)).HasBuffShield);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Levitate()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 3,
                    Enumerators.Skill.LEVITATE,
                    Enumerators.Skill.NONE,
                    new TestCardData("Gale", 6));

                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 3,
                    Enumerators.Skill.LEVITATE,
                    Enumerators.Skill.NONE,
                    new TestCardData("Gale", 6));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerGaleId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Gale", 1);
                InstanceId playerGale2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Gale", 2);

                InstanceId opponentGaleId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Gale", 1);
                InstanceId opponentGale2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Gale", 2);

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
                           player.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(playerGaleId,
                                    new ParametrizedAbilityParameters()
                                        {
                                            CardName = playerGaleId.Id.ToString()
                                        }
                                    )
                           });
                       },
                       opponent =>
                       {
                           opponent.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(opponentGale2Id,
                                    new ParametrizedAbilityParameters()
                                        {
                                            CardName = opponentGale2Id.Id.ToString()
                                        }
                                    )
                           });
                       },
                       player => {},
                       opponent => {},
                   };

                Action validateEndState = () =>
                {
                    CardModel playerCardInHand = TestHelper.BattlegroundController.GetCardModelByInstanceId(playerGaleId);
                    CardModel opponentCardInHand = TestHelper.BattlegroundController.GetCardModelByInstanceId(opponentGale2Id);

                    Assert.AreEqual(playerCardInHand.Card.Prototype.Cost - 1, playerCardInHand.CurrentCost);
                    Assert.AreEqual(opponentCardInHand.Card.Prototype.Cost - 1, playerCardInHand.CurrentCost);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Breakout()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 4,
                    Enumerators.Skill.BREAKOUT,
                    Enumerators.Skill.NONE,
                    new TestCardData("Zlab", 15));

                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 4,
                    Enumerators.Skill.BREAKOUT,
                    Enumerators.Skill.NONE,
                    new TestCardData("Zlab", 15));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerSlabId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zlab", 1);
                InstanceId playerSlab2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zlab", 2);
                InstanceId playerSlab3Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Zlab", 3);

                InstanceId opponentSlabId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zlab", 1);
                InstanceId opponentSlab2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zlab", 2);
                InstanceId opponentSlab3Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Zlab", 3);

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
                           player.CardPlay(playerSlabId, ItemPosition.Start);
                           player.CardPlay(playerSlab2Id, ItemPosition.Start);
                           player.CardPlay(playerSlab3Id, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentSlabId, ItemPosition.Start);
                           opponent.CardPlay(opponentSlab2Id, ItemPosition.Start);
                           opponent.CardPlay(opponentSlab3Id, ItemPosition.Start);
                       },
                       player =>
                       {
                           player.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(opponentSlabId,
                                    new ParametrizedAbilityParameters()
                                    {
                                        Attack = 2
                                    }),
                                new ParametrizedAbilityInstanceId(opponentSlab3Id,
                                    new ParametrizedAbilityParameters()
                                    {
                                        Attack = 2
                                    }),
                                new ParametrizedAbilityInstanceId(pvpTestContext.GetOpponentPlayer().InstanceId,
                                    new ParametrizedAbilityParameters()
                                    {
                                        Attack = 1
                                    })
                           });
                       },
                       opponent =>
                       {
                           opponent.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(playerSlabId,
                                    new ParametrizedAbilityParameters()
                                    {
                                        Attack = 1
                                    }),
                                new ParametrizedAbilityInstanceId(playerSlab3Id,
                                    new ParametrizedAbilityParameters()
                                    {
                                        Attack = 1
                                    }),
                                new ParametrizedAbilityInstanceId(pvpTestContext.GetCurrentPlayer().InstanceId,
                                    new ParametrizedAbilityParameters()
                                    {
                                        Attack = 3
                                    }),
                           });
                       },
                   };

                Action validateEndState = () =>
                {
                    CardModel playerUnit1 = (CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerSlabId);
                    CardModel playerUnit3 = (CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerSlab3Id);

                    CardModel opponentUnit1 = (CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentSlabId);
                    CardModel opponentUnit3 = (CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentSlab3Id);

                    Assert.AreEqual(playerUnit1.Card.Prototype.Defense - 1, playerUnit1.CurrentDefense);
                    Assert.AreEqual(playerUnit3.Card.Prototype.Defense - 1, playerUnit3.CurrentDefense);
                    Assert.AreEqual(pvpTestContext.GetCurrentPlayer().InitialDefense - 3, pvpTestContext.GetCurrentPlayer().Defense);

                    Assert.AreEqual(opponentUnit1.Card.Prototype.Defense - 2, opponentUnit1.CurrentDefense);
                    Assert.AreEqual(opponentUnit3.Card.Prototype.Defense - 2, opponentUnit3.CurrentDefense);
                    Assert.AreEqual(pvpTestContext.GetOpponentPlayer().InitialDefense - 1, pvpTestContext.GetOpponentPlayer().Defense);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator MassRabies()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 1,
                    Enumerators.Skill.MASS_RABIES,
                    Enumerators.Skill.NONE,
                    new TestCardData("Pyromaz", 15));

                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 1,
                    Enumerators.Skill.MASS_RABIES,
                    Enumerators.Skill.NONE,
                    new TestCardData("Pyromaz", 15));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerPyromazId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Pyromaz", 1);
                InstanceId playerPyromaz2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Pyromaz", 2);
                InstanceId playerPyromaz3Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Pyromaz", 3);

                InstanceId opponentPyromazId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Pyromaz", 1);
                InstanceId opponentPyromaz2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Pyromaz", 2);
                InstanceId opponentPyromaz3Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Pyromaz", 3);

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
                           player.CardPlay(playerPyromazId, ItemPosition.Start);
                           player.CardPlay(playerPyromaz2Id, ItemPosition.Start);
                           player.CardPlay(playerPyromaz3Id, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentPyromazId, ItemPosition.Start);
                           opponent.CardPlay(opponentPyromaz2Id, ItemPosition.Start);
                           opponent.CardPlay(opponentPyromaz3Id, ItemPosition.Start);
                       },
                       player =>
                       {
                           player.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(playerPyromazId),
                                new ParametrizedAbilityInstanceId(playerPyromaz2Id)
                           });

                       },
                       opponent =>
                       {
                           opponent.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(opponentPyromazId),
                                new ParametrizedAbilityInstanceId(opponentPyromaz3Id)
                           });
                       },
                       player => {},
                       opponent => {},
                   };

                Action validateEndState = () =>
                {
                    Assert.IsTrue(((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerPyromazId)).HasFeral);
                    Assert.IsTrue(((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerPyromaz2Id)).HasFeral);
                    Assert.IsTrue(!((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerPyromaz3Id)).HasFeral);
                    Assert.IsTrue(((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentPyromazId)).HasFeral);
                    Assert.IsTrue(!((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentPyromaz2Id)).HasFeral);
                    Assert.IsTrue(((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentPyromaz3Id)).HasFeral);
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Infect()
        {
            return AsyncTest(async () =>
            {
   				Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 4,
                    Enumerators.Skill.INFECT,
                    Enumerators.Skill.NONE,
                    new TestCardData("Wazte", 15));

                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 4,
                    Enumerators.Skill.INFECT,
                    Enumerators.Skill.NONE,
                    new TestCardData("Wazte", 15));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerWazteId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Wazte", 1);
                InstanceId playerWazte2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Wazte", 2);
                InstanceId playerWazte3Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Wazte", 3);

                InstanceId opponentWazteId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Wazte", 1);
                InstanceId opponentWazte2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Wazte", 2);
                InstanceId opponentWazte3Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Wazte", 3);

                int playerDifference = 0;
                int opponentDiference = 0;

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
                           player.CardPlay(playerWazteId, ItemPosition.Start);
                           player.CardPlay(playerWazte2Id, ItemPosition.Start);
                           player.CardPlay(playerWazte3Id, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentWazteId, ItemPosition.Start);
                           opponent.CardPlay(opponentWazte2Id, ItemPosition.Start);
                           opponent.CardPlay(opponentWazte3Id, ItemPosition.Start);
                       },
                       player =>
                       {
                           playerDifference = ((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerWazteId)).CurrentDamage;
                           player.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(playerWazteId),
                                new ParametrizedAbilityInstanceId(opponentWazte3Id),
                           });
                       },
                       opponent =>
                       {
                           opponentDiference = ((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentWazte2Id)).CurrentDamage;
                           opponent.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(opponentWazte2Id),
                                new ParametrizedAbilityInstanceId(playerWazte3Id)
                           });
                       },
                       player => {},
                       opponent => {},
                   };

                Action validateEndState = () =>
                {
                    Assert.Null((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerWazte3Id));
                    Assert.Null((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentWazte3Id));
                    Assert.Null((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerWazteId));
                    Assert.Null((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentWazte2Id));
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator Epidemic()
        {
            return AsyncTest(async () =>
            {
                Deck playerDeck = PvPTestUtility.GetDeckWithCards("deck 1", 4,
                    Enumerators.Skill.EPIDEMIC,
                    Enumerators.Skill.NONE,
                    new TestCardData("Wazte", 15));

                Deck opponentDeck = PvPTestUtility.GetDeckWithCards("deck 2", 4,
                    Enumerators.Skill.EPIDEMIC,
                    Enumerators.Skill.NONE,
                    new TestCardData("Wazte", 15));

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                InstanceId playerWazteId = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Wazte", 1);
                InstanceId playerWazte2Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Wazte", 2);
                InstanceId playerWazte3Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Wazte", 3);
                InstanceId playerWazte4Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Wazte", 4);
                InstanceId playerWazte5Id = pvpTestContext.GetCardInstanceIdByName(playerDeck, "Wazte", 5);

                InstanceId opponentWazteId = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Wazte", 1);
                InstanceId opponentWazte2Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Wazte", 2);
                InstanceId opponentWazte3Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Wazte", 3);
                InstanceId opponentWazte4Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Wazte", 4);
                InstanceId opponentWazte5Id = pvpTestContext.GetCardInstanceIdByName(opponentDeck, "Wazte", 5);

                int playerDifference1 = 0;
                int playerDifference2 = 0;

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
                           player.CardPlay(playerWazteId, ItemPosition.Start);
                           player.CardPlay(playerWazte2Id, ItemPosition.Start);
                           player.CardPlay(playerWazte3Id, ItemPosition.Start);
                           player.CardPlay(playerWazte4Id, ItemPosition.Start);
                           player.CardPlay(playerWazte5Id, ItemPosition.Start);
                       },
                       opponent =>
                       {
                           opponent.CardPlay(opponentWazteId, ItemPosition.Start);
                           opponent.CardPlay(opponentWazte2Id, ItemPosition.Start);
                           opponent.CardPlay(opponentWazte3Id, ItemPosition.Start);
                           opponent.CardPlay(opponentWazte4Id, ItemPosition.Start);
                           opponent.CardPlay(opponentWazte5Id, ItemPosition.Start);
                       },
                       player =>
                       {
                           playerDifference1 = ((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerWazteId)).CurrentDamage;
                           playerDifference2 = ((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerWazte4Id)).CurrentDamage;
                           player.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(playerWazteId,
                                    new ParametrizedAbilityParameters()
                                    {
                                        CardName = opponentWazte3Id.Id.ToString()
                                    }),
                                new ParametrizedAbilityInstanceId(playerWazte4Id,
                                    new ParametrizedAbilityParameters()
                                    {
                                        CardName = opponentWazteId.Id.ToString()
                                    })
                           });
                       },
                       opponent =>
                       {
                           opponent.OverlordSkillUsed(new SkillId(0), new List<ParametrizedAbilityInstanceId>()
                           {
                                new ParametrizedAbilityInstanceId(opponentWazte2Id,
                                    new ParametrizedAbilityParameters()
                                    {
                                        CardName = playerWazte3Id.Id.ToString()
                                    }),
                                new ParametrizedAbilityInstanceId(opponentWazte4Id,
                                    new ParametrizedAbilityParameters()
                                    {
                                        CardName = playerWazte2Id.Id.ToString()
                                    }),
                           });
                       },
                       player => {},
                       opponent => {},
                   };

                Action validateEndState = () =>
                {
                    Assert.Null((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerWazteId));
                    Assert.Null((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerWazte3Id));
                    Assert.Null((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerWazte4Id));
                    Assert.Null((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(playerWazte2Id));
                    Assert.Null((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentWazte2Id));
                    Assert.Null((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentWazte4Id));
                    Assert.Null((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentWazteId));
                    Assert.Null((CardModel)TestHelper.BattlegroundController.GetBoardObjectByInstanceId(opponentWazte3Id));
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, validateEndState, false);
            });
        }
    }
}
