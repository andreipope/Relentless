using System;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using log4net;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.Protobuf;
using UnityEngine;
using UnityEngine.TestTools;
using Deck = Loom.ZombieBattleground.Data.Deck;
using InstanceId = Loom.ZombieBattleground.Data.InstanceId;

namespace Loom.ZombieBattleground.Test.MultiplayerTests
{
    public class GeneralMultiplayerTests : BaseIntegrationTest
    {
        private static readonly ILog Log = Logging.GetLog(nameof(GeneralMultiplayerTests));

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator CorrectCardDraw()
        {
            return AsyncTest(async () =>
            {
                Deck opponentDeck = PvPTestUtility.GetDeckWithCards(
                    "deck 1", 1,
                    new TestCardData("Zlab", 30)
                );

                Deck playerDeck = PvPTestUtility.GetDeckWithCards(
                    "deck 2", 1,
                    new TestCardData("Zlab", 30)
                );

                PvpTestContext pvpTestContext = new PvpTestContext(playerDeck, opponentDeck);

                IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
                {
                    player =>
                    {
                        Assert.AreEqual(4, pvpTestContext.GetCurrentPlayer().CardsInHand.Count);
                        Assert.AreEqual(3, pvpTestContext.GetOpponentPlayer().CardsInHand.Count);
                    },
                    opponent =>
                    {
                        Assert.AreEqual(4, pvpTestContext.GetCurrentPlayer().CardsInHand.Count);
                        Assert.AreEqual(5, pvpTestContext.GetOpponentPlayer().CardsInHand.Count);
                    },
                    player =>
                    {
                        Assert.AreEqual(5, pvpTestContext.GetCurrentPlayer().CardsInHand.Count);
                        Assert.AreEqual(5, pvpTestContext.GetOpponentPlayer().CardsInHand.Count);
                    },
                };

                await PvPTestUtility.GenericPvPTest(pvpTestContext, turns, null);
            });
        }

        #region specific situations tests

        #endregion
    }
}
