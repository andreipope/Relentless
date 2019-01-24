using System.Collections;
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

                await TestHelper.AssertCurrentPageName("PlaySelectionPage");
                await TestHelper.ClickGenericButton("Button_SoloMode");
                await TestHelper.AssertCurrentPageName("HordeSelectionPage");
                await TestHelper.ClickGenericButton("Image_BaackgroundGeneral");
                await TestHelper.AssertCurrentPageName("OverlordSelectionPage");
                await TestHelper.PickOverlord("Razu", true);

                await TestHelper.LetsThink();

                await TestHelper.ClickGenericButton("Canvas_BackLayer/Button_Continue");
                await TestHelper.AssertCurrentPageName("HordeEditingPage");
                await TestHelper.ClickGenericButton("Button_Back");
                await TestHelper.RespondToYesNoOverlay(false);
                await TestHelper.AssertCurrentPageName("HordeSelectionPage");
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

                await TestHelper.AssertCurrentPageName("PlaySelectionPage");
                await TestHelper.ClickGenericButton("Button_SoloMode");
                await TestHelper.AssertCurrentPageName("HordeSelectionPage");

                await TestHelper.SelectAHordeByName("Draft", false);
                if (TestHelper.SelectedHordeIndex != -1)
                {
                    await TestHelper.RemoveAHorde(TestHelper.SelectedHordeIndex);
                }

                await TestHelper.ClickGenericButton("Image_BaackgroundGeneral");
                await TestHelper.AssertCurrentPageName("OverlordSelectionPage");
                await TestHelper.PickOverlord("Razu", true);
                await TestHelper.LetsThink();
                await TestHelper.ClickGenericButton("Canvas_BackLayer/Button_Continue");
                await TestHelper.AssertCurrentPageName("HordeEditingPage");
                await TestHelper.SetDeckTitle("Draft");
                await TestHelper.ClickGenericButton("Button_Back");
                await TestHelper.RespondToYesNoOverlay(true);
                await TestHelper.AssertCurrentPageName("HordeSelectionPage");
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

                await TestHelper.AssertCurrentPageName("PlaySelectionPage");
                await TestHelper.ClickGenericButton("Button_SoloMode");
                await TestHelper.AssertCurrentPageName("HordeSelectionPage");
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

                await TestHelper.AssertCurrentPageName("PlaySelectionPage");
                await TestHelper.ClickGenericButton("Button_SoloMode");
                await TestHelper.AssertCurrentPageName("HordeSelectionPage");
                await TestHelper.AddRazuHorde();
                await TestHelper.AssertCurrentPageName("HordeSelectionPage");
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

                await TestHelper.AssertCurrentPageName("PlaySelectionPage");
                await TestHelper.ClickGenericButton("Button_SoloMode");
                await TestHelper.AssertCurrentPageName("HordeSelectionPage");
                await TestHelper.AddKalileHorde();
                await TestHelper.AssertCurrentPageName("HordeSelectionPage");
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

                await TestHelper.AssertCurrentPageName("PlaySelectionPage");
                await TestHelper.ClickGenericButton("Button_SoloMode");
                await TestHelper.AssertCurrentPageName("HordeSelectionPage");
                await TestHelper.AddValashHorde();
                await TestHelper.AssertCurrentPageName("HordeSelectionPage");
            });
        }
    }
}
