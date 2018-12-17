using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.TestTools;

namespace Loom.ZombieBattleground.Test
{
    public class HordeManipulationTests : BaseIntegrationTest
    {

        [UnityAsyncTest]
        [Timeout(500000)]
        public async Task TestAsync()
        {
            Debug.Log("async 1");
            await Task.Delay(2000);
            Debug.Log("async 2");
            await Task.Delay(2000);
            Debug.Log("async 3");
        }

        [UnityAsyncTest]
        [Timeout(500000)]
        public IEnumerator TestAsync2()
        {
            Debug.Log("async 1");
            yield return new WaitForSeconds(2);
            Debug.Log("async 2");
            yield return new WaitForSeconds(2);
            Debug.Log("async 3");
        }

        [UnityTest]
        [Timeout(500000)]
        public async Task Test_H1_CreateAHordeAndCancel()
        {
            TestHelper.SetTestName("Solo - Create a Horde and cancel");

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

            await new WaitForUpdate();

            TestHelper.TestEndHandler();
        }

        [UnityTest]
        [Timeout(500000)]
        public async Task Test_H2_CreateAHordeAndDraft()
        {
            TestHelper.SetTestName("Solo - Create a Horde and draft");

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

            await new WaitForUpdate();

            TestHelper.TestEndHandler();
        }

        [UnityTest]
        [Timeout(500000)]
        public async Task Test_H3_RemoveAllHordesExceptFirst()
        {
            TestHelper.SetTestName("Solo - Remove all Hordes except first");

            await TestHelper.ClickGenericButton("Button_Play");

            await TestHelper.AssertIfWentDirectlyToTutorial(
                TestHelper.GoBackToMainAndPressPlay);

            await TestHelper.AssertCurrentPageName("PlaySelectionPage");
            await TestHelper.ClickGenericButton("Button_SoloMode");
            await TestHelper.AssertCurrentPageName("HordeSelectionPage");
            await TestHelper.RemoveAllHordesExceptDefault();

            await new WaitForUpdate();

            TestHelper.TestEndHandler();
        }

        [UnityTest]
        [Timeout(500000)]
        public async Task Test_H4_CreateARazuHordeAndSave()
        {
            TestHelper.SetTestName("Solo - Create a Horde and save");

            await TestHelper.ClickGenericButton("Button_Play");

            await TestHelper.AssertIfWentDirectlyToTutorial(
                TestHelper.GoBackToMainAndPressPlay);

            await TestHelper.AssertCurrentPageName("PlaySelectionPage");
            await TestHelper.ClickGenericButton("Button_SoloMode");
            await TestHelper.AssertCurrentPageName("HordeSelectionPage");
            await TestHelper.AddRazuHorde();
            await TestHelper.AssertCurrentPageName("HordeSelectionPage");

            await new WaitForUpdate();

            TestHelper.TestEndHandler();
        }

        [UnityTest]
        [Timeout(500000)]
        public async Task Test_H4_CreateKalileHorde()
        {
            TestHelper.SetTestName("Solo - Create a Horde and save");

            await TestHelper.ClickGenericButton("Button_Play");

            await TestHelper.AssertIfWentDirectlyToTutorial(
                TestHelper.GoBackToMainAndPressPlay);

            await TestHelper.AssertCurrentPageName("PlaySelectionPage");
            await TestHelper.ClickGenericButton("Button_SoloMode");
            await TestHelper.AssertCurrentPageName("HordeSelectionPage");
            await TestHelper.AddKalileHorde();
            await TestHelper.AssertCurrentPageName("HordeSelectionPage");

            await new WaitForUpdate();

            TestHelper.TestEndHandler();
        }

        [UnityTest]
        [Timeout(500000)]
        public async Task Test_H5_CreateValashHorde()
        {
            TestHelper.SetTestName("Solo - Create a Horde and save");

            await TestHelper.ClickGenericButton("Button_Play");

            await TestHelper.AssertIfWentDirectlyToTutorial(
                TestHelper.GoBackToMainAndPressPlay);

            await TestHelper.AssertCurrentPageName("PlaySelectionPage");
            await TestHelper.ClickGenericButton("Button_SoloMode");
            await TestHelper.AssertCurrentPageName("HordeSelectionPage");
            await TestHelper.AddValashHorde();
            await TestHelper.AssertCurrentPageName("HordeSelectionPage");

            await new WaitForUpdate();

            TestHelper.TestEndHandler();
        }
    }
}
