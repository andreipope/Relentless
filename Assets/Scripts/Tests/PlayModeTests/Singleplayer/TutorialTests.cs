using System.Collections;
using NUnit.Framework;
using System.Threading.Tasks;
using Loom.ZombieBattleground.Common;
using UnityEngine.TestTools;

namespace Loom.ZombieBattleground.Test
{
    [Ignore("broken")]
    public class TutorialTests : BaseIntegrationTest
    {
        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator TutorialNonSkip()
        {
            return AsyncTest(async () =>
            {
                await TestHelper.MainMenuTransition("Button_Play");

                await TestHelper.AssertCurrentPageName(Enumerators.AppState.PlaySelection);

                #region Tutorial Non-Skip

                await TestHelper.MainMenuTransition("Button_Tutorial");

                await TestHelper.AssertCurrentPageName(Enumerators.AppState.GAMEPLAY);

                // TODO: implement

                #endregion
            });
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator TutorialSkip()
        {
            return AsyncTest(async () =>
            {
                await TestHelper.MainMenuTransition("Button_Play");

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
    }
}
