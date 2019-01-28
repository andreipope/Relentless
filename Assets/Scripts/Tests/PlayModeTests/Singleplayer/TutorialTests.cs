using System.Collections;
using NUnit.Framework;
using System.Threading.Tasks;
using Loom.ZombieBattleground.Common;
using UnityEngine.TestTools;

namespace Loom.ZombieBattleground.Test
{
    public class TutorialTests : BaseIntegrationTest
    {
        [UnityTest]
        [Timeout(500000)]
        public IEnumerator TutorialNonSkip()
        {
            return AsyncTest(async () =>
            {
                await TestHelper.MainMenuTransition("Button_Play");

                await TestHelper.AssertIfWentDirectlyToTutorial(
                    TestHelper.GoBackToMainAndPressPlay);

                await TestHelper.AssertCurrentPageName(Enumerators.AppState.PlaySelection);

                #region Tutorial Non-Skip

                await TestHelper.MainMenuTransition("Button_Tutorial");

                await TestHelper.AssertCurrentPageName(Enumerators.AppState.GAMEPLAY);

                await PlayTutorial_Part1();

                await TestHelper.ClickGenericButton("Button_Continue");

                await PlayTutorial_Part2();

                await TestHelper.ClickGenericButton("Button_Continue");

                await TestHelper.AssertCurrentPageName(Enumerators.AppState.HordeSelection);

                #endregion
            });
        }

        [UnityTest]
        [Timeout(500000)]
        public IEnumerator TutorialSkip()
        {
            return AsyncTest(async () =>
            {
                await TestHelper.MainMenuTransition("Button_Play");

                await TestHelper.AssertIfWentDirectlyToTutorial(
                    TestHelper.GoBackToMainAndPressPlay);

                await TestHelper.AssertCurrentPageName(Enumerators.AppState.PlaySelection);

                #region Tutorial Skip

                await TestHelper.MainMenuTransition("Button_Tutorial");

                await TestHelper.AssertCurrentPageName(Enumerators.AppState.GAMEPLAY);

                await SkipTutorial(false);

                #endregion
            });
        }

        private async Task SkipTutorial(bool twoSteps = true)
        {
            await TestHelper.ClickGenericButton("Button_Skip");

            await TestHelper.RespondToYesNoOverlay(true);

            if (twoSteps)
            {
                await TestHelper.ClickGenericButton("Button_Skip");

                await TestHelper.RespondToYesNoOverlay(true);
            }

            await new WaitForUpdate();
        }

        private async Task PlayTutorial_Part1()
        {
            if (TestHelper.IsTestFailed)
            {
                return;
            }

            await TestHelper.ClickGenericButton("Button_Next", count: 3);

            await TestHelper.ClickGenericButton("Button_Play");

            await TestHelper.ClickGenericButton("Button_Next", count: 4);

            await TestHelper.WaitUntilWeHaveACardAtHand();

            await TestHelper.PlayCardFromHandToBoard(new[]
            {
                0
            });

            await TestHelper.ClickGenericButton("Button_Next");

            await TestHelper.EndTurn();

            await TestHelper.WaitUntilCardIsAddedToBoard("OpponentBoard");
            await TestHelper.WaitUntilAIBrainStops();

            await TestHelper.ClickGenericButton("Button_Next");

            await TestHelper.WaitUntilOurTurnStarts();
            await TestHelper.WaitUntilInputIsUnblocked();

            await TestHelper.LetsThink();
            await TestHelper.LetsThink();
            await TestHelper.LetsThink();

            await TestHelper.PlayCardFromBoardToOpponent(new[]
                {
                    0
                },
                new[]
                {
                    0
                });

            for (int i = 0; i < 2; i++)
            {
                await TestHelper.ClickGenericButton("Button_Next");
            }

            await TestHelper.EndTurn();

            await TestHelper.WaitUntilOurTurnStarts();
            await TestHelper.WaitUntilInputIsUnblocked();

            await TestHelper.ClickGenericButton("Button_Next");

            await TestHelper.LetsThink();
            await TestHelper.LetsThink();
            await TestHelper.LetsThink();

            await TestHelper.PlayCardFromBoardToOpponent(new[]
                {
                    0
                },
                null,
                true);

            await TestHelper.ClickGenericButton("Button_Next");

            await TestHelper.LetsThink();
            await TestHelper.LetsThink();

            await TestHelper.EndTurn();

            await TestHelper.WaitUntilOurTurnStarts();
            await TestHelper.WaitUntilInputIsUnblocked();

            await TestHelper.ClickGenericButton("Button_Next");

            await TestHelper.LetsThink();
            await TestHelper.LetsThink();
            await TestHelper.LetsThink();

            await TestHelper.PlayCardFromBoardToOpponent(new[]
                {
                    0
                },
                new[]
                {
                    0
                });

            for (int i = 0; i < 3; i++)
            {
                await TestHelper.ClickGenericButton("Button_Next");
            }

            await TestHelper.WaitUntilAIBrainStops();
            await TestHelper.WaitUntilInputIsUnblocked();

            await TestHelper.LetsThink();
            await TestHelper.LetsThink();
            await TestHelper.LetsThink();

            await TestHelper.PlayCardFromHandToBoard(new[]
            {
                1
            });

            await TestHelper.LetsThink();
            await TestHelper.LetsThink();
            await TestHelper.LetsThink();

            await TestHelper.PlayCardFromBoardToOpponent(new[]
                {
                    0
                },
                null,
                true);

            await TestHelper.LetsThink();
            await TestHelper.LetsThink();

            for (int i = 0; i < 3; i++)
            {
                await TestHelper.ClickGenericButton("Button_Next");
            }

            await TestHelper.UseSkillToOpponentPlayer();

            for (int i = 0; i < 4; i++)
            {
                await TestHelper.ClickGenericButton("Button_Next");
            }

            await new WaitForUpdate();
        }

        private async Task PlayTutorial_Part2()
        {
            if (TestHelper.IsTestFailed)
            {
                return;
            }

            await TestHelper.ClickGenericButton("Button_Next");

            await TestHelper.WaitUntilOurTurnStarts();
            await TestHelper.WaitUntilInputIsUnblocked();

            for (int i = 0; i < 11; i++)
            {
                await TestHelper.ClickGenericButton("Button_Next");
            }

            await TestHelper.PlayCardFromHandToBoard(new[]
            {
                1
            });

            await TestHelper.ClickGenericButton("Button_Next");

            await TestHelper.LetsThink();
            await TestHelper.LetsThink();

            await TestHelper.PlayCardFromHandToBoard(new[]
            {
                0
            });

            for (int i = 0; i < 12; i++)
            {
                await TestHelper.ClickGenericButton("Button_Next");
            }

            await TestHelper.LetsThink();

            await TestHelper.PlayNonSleepingCardsFromBoardToOpponentPlayer();

            for (int i = 0; i < 5; i++)
            {
                await TestHelper.ClickGenericButton("Button_Next");
            }

            await new WaitForUpdate();
        }
    }
}
