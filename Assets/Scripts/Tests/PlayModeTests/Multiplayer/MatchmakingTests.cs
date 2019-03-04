using System.Collections;
using System.Threading.Tasks;
using Loom.ZombieBattleground.Common;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Loom.ZombieBattleground.Test
{
    [Ignore("seem broken, verify")]
    public class MatchmakingTests : BaseIntegrationTest
    {
        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator MatchmakingCancel()
        {
            return AsyncTest(async () =>
            {
                await PvPTestUtility.StartOnlineMatch(createOpponent: false);

                await TestHelper.LetsThink(5, true);

                await TestHelper.ClickGenericButton("Button_Cancel");

                await TestHelper.AssertPvPStartedOrMatchmakingFailed(
                    () =>
                    {
                        Assert.Fail("It shouldn't have been matched.");
                        return Task.CompletedTask;
                    },
                    () => TestHelper.ClickGenericButton ("Button_Cancel"));
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator MatchmakingTimeout()
        {
            return AsyncTest(async () =>
            {
                await PvPTestUtility.StartOnlineMatch(createOpponent: false);

                await TestHelper.AssertPvPStartedOrMatchmakingFailed(
                    () =>
                    {
                        Assert.Fail("It shouldn't have been matched.");
                        return Task.CompletedTask;
                    },
                    () => TestHelper.ClickGenericButton("Button_Cancel"));
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator MatchmakeAndQuit()
        {
            return AsyncTest(async () =>
            {
                await PvPTestUtility.StartOnlineMatch();
                await TestHelper.AssertPvPStartedOrMatchmakingFailed(
                    null,
                    () =>
                    {
                        Assert.Fail("Didn't match, so couldn't check.");
                        return Task.CompletedTask;
                    });

                await TestHelper.MatchmakeOpponentDebugClient();

                await TestHelper.AssertCurrentPageName(Enumerators.AppState.GAMEPLAY);
                await TestHelper.WaitUntilPlayerOrderIsDecided();
                TestHelper.AssertOverlordName();

                await TestHelper.ClickGenericButton("Button_Settings");
                await TestHelper.ClickGenericButton("Button_QuitToMainMenu");
                await TestHelper.RespondToYesNoOverlay(true);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator MatchmakeWaitForOurTurnAndQuit()
        {
            return AsyncTest(async () =>
            {
                await PvPTestUtility.StartOnlineMatch();

                await TestHelper.MatchmakeOpponentDebugClient();

                await TestHelper.AssertCurrentPageName(Enumerators.AppState.GAMEPLAY);
                await TestHelper.WaitUntilPlayerOrderIsDecided();
                await TestHelper.AssertMulliganPopupCameUp(
                    () => TestHelper.ClickGenericButton("Button_Keep"),
                    null);
                await TestHelper.WaitUntilOurFirstTurn();
                await TestHelper.ClickGenericButton("Button_Settings");
                await TestHelper.ClickGenericButton("Button_QuitToMainMenu");
                await TestHelper.RespondToYesNoOverlay(true);
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator MatchmakingCancelAndMatchmake()
        {
            return AsyncTest(async () =>
            {
                await TestHelper.MainMenuTransition("Button_Play");

                await TestHelper.AssertCurrentPageName(Enumerators.AppState.PlaySelection);
                await TestHelper.MainMenuTransition("Button_PvPMode");
                await TestHelper.AssertCurrentPageName(Enumerators.AppState.PvPSelection);
                await TestHelper.MainMenuTransition("Button_CasualType");
                await TestHelper.AssertCurrentPageName(Enumerators.AppState.HordeSelection);

                int selectedHordeIndex = 0;

                await TestHelper.SelectAHordeByIndex(selectedHordeIndex);
                TestHelper.RecordExpectedOverlordName(selectedHordeIndex);

                // Matchmaking Cancel

                TestHelper.SetPvPTags(new[]
                {
                    "pvpTestNoOpponentCancel"
                });

                await TestHelper.LetsThink();
                await TestHelper.MainMenuTransition("Button_Battle");
                await TestHelper.AssertPvPStartedOrMatchmakingFailed(
                    () =>
                    {
                        Assert.Fail("It shouldn't have been matched.");
                        return Task.CompletedTask;
                    },
                    () => TestHelper.ClickGenericButton ("Button_Cancel"));

                await TestHelper.LetsThink();
                await TestHelper.LetsThink();
                await TestHelper.LetsThink();

                await TestHelper.ClickGenericButton("Button_Cancel");

                await TestHelper.LetsThink();
                await TestHelper.LetsThink();

                // Matchmake and Quit

                TestHelper.SetPvPTags(new[]
                {
                    "pvpTest"
                });

                await TestHelper.LetsThink();

                await TestHelper.MainMenuTransition("Button_Battle");
                await TestHelper.AssertPvPStartedOrMatchmakingFailed(
                    null,
                    () =>
                    {
                        Assert.Fail("Didn't match, so couldn't check.");
                        return Task.CompletedTask;
                    });

                await TestHelper.AssertCurrentPageName(Enumerators.AppState.GAMEPLAY);
                await TestHelper.WaitUntilPlayerOrderIsDecided();
                TestHelper.AssertOverlordName();
                await TestHelper.ClickGenericButton("Button_Settings");

                await TestHelper.LetsThink();

                await TestHelper.ClickGenericButton("Button_QuitToMainMenu");

                await TestHelper.LetsThink();

                await TestHelper.RespondToYesNoOverlay(true);

                await TestHelper.LetsThink();
            });
        }
    }
}
