
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;

namespace Loom.ZombieBattleground.Test.MultiplayerCardsTests
{
  public class BaseCardsTest : BaseIntegrationTest
  {
        protected async Task DoPvPMatch(IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns, Deck deck, Deck opponentDeck)
        {
            await GenericPvPTest(turns, () =>
                {
                    TestHelper.DebugCheats.ForceFirstTurnUserId = TestHelper.GetOpponentDebugClient().UserDataModel.UserId;
                    TestHelper.DebugCheats.UseCustomDeck = true;
                    TestHelper.DebugCheats.CustomDeck = deck;
                    TestHelper.DebugCheats.DisableDeckShuffle = true;
                    TestHelper.DebugCheats.IgnoreGooRequirements = true;
                },
                cheats =>
                {
                    cheats.UseCustomDeck = true;
                    cheats.CustomDeck = opponentDeck;
                }
            );
        }

        protected async Task GenericPvPTest(IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns,
                                            Action setupAction,
                                            Action<DebugCheatsConfiguration> modifyOpponentDebugCheats)
        {
            await TestHelper.CreateAndConnectOpponentDebugClient();

            setupAction?.Invoke();

            await StartOnlineMatch(createOpponent: false);

            MatchScenarioPlayer matchScenarioPlayer = new MatchScenarioPlayer(TestHelper, turns);
            await TestHelper.MatchmakeOpponentDebugClient(modifyOpponentDebugCheats);
            await TestHelper.WaitUntilPlayerOrderIsDecided();

            await matchScenarioPlayer.Play();
        }

    }
}
