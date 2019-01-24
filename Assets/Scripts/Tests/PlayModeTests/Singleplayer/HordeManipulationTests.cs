using System.Collections;
using Loom.ZombieBattleground.Common;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Loom.ZombieBattleground.Test
{
    public class HordeManipulationTests : BaseIntegrationTest
    {
        [UnityTest]
        [Timeout(500000)]
        public IEnumerator CreateHordeAndCancel()
        {
            return AsyncTest(async () =>
            {
                await TestHelper.ClickGenericButton("Button_Play");

                await TestHelper.AssertIfWentDirectlyToTutorial(
                    TestHelper.GoBackToMainAndPressPlay);

                await TestHelper.AssertCurrentPageName(Enumerators.AppState.PlaySelection);
                await TestHelper.ClickGenericButton("Button_SoloMode");
                await TestHelper.AssertCurrentPageName(Enumerators.AppState.HordeSelection);
                await TestHelper.ClickGenericButton("Image_BaackgroundGeneral");
                await TestHelper.AssertCurrentPageName(Enumerators.AppState.HERO_SELECTION);
                await TestHelper.PickOverlord("Razu", true);

                await TestHelper.LetsThink();

                await TestHelper.ClickGenericButton("Canvas_BackLayer/Button_Continue");
                await TestHelper.AssertCurrentPageName(Enumerators.AppState.DECK_EDITING);
                await TestHelper.ClickGenericButton("Button_Back");
                await TestHelper.RespondToYesNoOverlay(false);
                await TestHelper.AssertCurrentPageName(Enumerators.AppState.HordeSelection);
            });
        }

        [UnityTest]
        [Timeout(500000)]
        public IEnumerator CreateHordeAndDraft()
        {
            return AsyncTest(async () =>
            {
                await TestHelper.ClickGenericButton("Button_Play");

                await TestHelper.AssertIfWentDirectlyToTutorial(
                    TestHelper.GoBackToMainAndPressPlay);

                await TestHelper.AssertCurrentPageName(Enumerators.AppState.PlaySelection);
                await TestHelper.ClickGenericButton("Button_SoloMode");
                await TestHelper.AssertCurrentPageName(Enumerators.AppState.HordeSelection);

                await TestHelper.SelectAHordeByName("Draft", false);
                if (TestHelper.SelectedHordeIndex != -1)
                {
                    await TestHelper.RemoveAHorde(TestHelper.SelectedHordeIndex);
                }

                await TestHelper.ClickGenericButton("Image_BaackgroundGeneral");
                await TestHelper.AssertCurrentPageName(Enumerators.AppState.HERO_SELECTION);
                await TestHelper.PickOverlord("Razu", true);
                await TestHelper.LetsThink();
                await TestHelper.ClickGenericButton("Canvas_BackLayer/Button_Continue");
                await TestHelper.AssertCurrentPageName(Enumerators.AppState.DECK_EDITING);
                await TestHelper.SetDeckTitle("Draft");
                await TestHelper.ClickGenericButton("Button_Back");
                await TestHelper.RespondToYesNoOverlay(true);
                await TestHelper.AssertCurrentPageName(Enumerators.AppState.HordeSelection);
                await TestHelper.SelectAHordeByName("Draft", true, "Horde draft isn't displayed.");
            });
        }

        [UnityTest]
        [Timeout(500000)]
        public IEnumerator RemoveAllHordesExceptFirst()
        {
            return AsyncTest(async () =>
            {
                await TestHelper.ClickGenericButton("Button_Play");

                await TestHelper.AssertIfWentDirectlyToTutorial(
                    TestHelper.GoBackToMainAndPressPlay);

                await TestHelper.AssertCurrentPageName(Enumerators.AppState.PlaySelection);
                await TestHelper.ClickGenericButton("Button_SoloMode");
                await TestHelper.AssertCurrentPageName(Enumerators.AppState.HordeSelection);
                await TestHelper.RemoveAllHordesExceptDefault();
            });
        }

        [UnityTest]
        [Timeout(500000)]
        public IEnumerator CreateRazuHordeAndSave()
        {
            return AsyncTest(async () =>
            {
                await TestHelper.ClickGenericButton("Button_Play");

                await TestHelper.AssertIfWentDirectlyToTutorial(
                    TestHelper.GoBackToMainAndPressPlay);

                await TestHelper.AssertCurrentPageName(Enumerators.AppState.PlaySelection);
                await TestHelper.ClickGenericButton("Button_SoloMode");
                await TestHelper.AssertCurrentPageName(Enumerators.AppState.HordeSelection);
                await TestHelper.AddRazuHorde();
                await TestHelper.AssertCurrentPageName(Enumerators.AppState.HordeSelection);
            });
        }

        [UnityTest]
        [Timeout(500000)]
        public IEnumerator CreateKalileHorde()
        {
            return AsyncTest(async () =>
            {
                await TestHelper.ClickGenericButton("Button_Play");

                await TestHelper.AssertIfWentDirectlyToTutorial(
                    TestHelper.GoBackToMainAndPressPlay);

                await TestHelper.AssertCurrentPageName(Enumerators.AppState.PlaySelection);
                await TestHelper.ClickGenericButton("Button_SoloMode");
                await TestHelper.AssertCurrentPageName(Enumerators.AppState.HordeSelection);
                await TestHelper.AddKalileHorde();
                await TestHelper.AssertCurrentPageName(Enumerators.AppState.HordeSelection);
            });
        }

        [UnityTest]
        [Timeout(500000)]
        public IEnumerator CreateValashHorde()
        {
            return AsyncTest(async () =>
            {
                await TestHelper.ClickGenericButton("Button_Play");

                await TestHelper.AssertIfWentDirectlyToTutorial(
                    TestHelper.GoBackToMainAndPressPlay);

                await TestHelper.AssertCurrentPageName(Enumerators.AppState.PlaySelection);
                await TestHelper.ClickGenericButton("Button_SoloMode");
                await TestHelper.AssertCurrentPageName(Enumerators.AppState.HordeSelection);
                await TestHelper.AddValashHorde();
                await TestHelper.AssertCurrentPageName(Enumerators.AppState.HordeSelection);
            });
        }
    }
}
