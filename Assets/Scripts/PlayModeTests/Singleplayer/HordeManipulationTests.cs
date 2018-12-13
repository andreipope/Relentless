using NUnit.Framework;
using System.Collections;
using UnityEngine.TestTools;

public class HordeManipulationTests
{
    private TestHelper _testHelper = new TestHelper ();

    #region Setup & TearDown

    [UnitySetUp]
    public IEnumerator PerTestSetup ()
    {
        yield return _testHelper.SetUp ();
    }

    [UnityTearDown]
    public IEnumerator PerTestTearDown ()
    {
        yield return _testHelper.TearDown ();

        yield return _testHelper.ReportTestTime ();
    }

    #endregion

    [UnityTest]
    [Timeout (500000)]
    public IEnumerator Test_H1_CreateAHordeAndCancel ()
    {
        _testHelper.SetTestName ("Solo - Create a Horde and cancel");

        yield return _testHelper.MainMenuTransition ("Button_Play");

        yield return _testHelper.AssertIfWentDirectlyToTutorial (
            _testHelper.GoBackToMainAndPressPlay ());

        yield return _testHelper.AssertCurrentPageName ("PlaySelectionPage");

        yield return _testHelper.MainMenuTransition ("Button_SoloMode");

        yield return _testHelper.AssertCurrentPageName ("HordeSelectionPage");

        yield return _testHelper.ClickGenericButton ("Image_BaackgroundGeneral");

        yield return _testHelper.AssertCurrentPageName ("OverlordSelectionPage");

        yield return _testHelper.PickOverlord ("Razu", true);

        yield return _testHelper.LetsThink ();

        yield return _testHelper.ClickGenericButton ("Canvas_BackLayer/Button_Continue");

        yield return _testHelper.AssertCurrentPageName ("HordeEditingPage");

        yield return _testHelper.MainMenuTransition ("Button_Back");
        yield return _testHelper.RespondToYesNoOverlay (false);

        yield return _testHelper.AssertCurrentPageName ("HordeSelectionPage");

        yield return null;
    }

    [UnityTest]
    [Timeout (500000)]
    public IEnumerator Test_H2_RemoveAllHordesExceptFirst ()
    {
        _testHelper.SetTestName ("Solo - Remove all Hordes except first");

        yield return _testHelper.MainMenuTransition ("Button_Play");

        yield return _testHelper.AssertIfWentDirectlyToTutorial (
            _testHelper.GoBackToMainAndPressPlay ());

        yield return _testHelper.AssertCurrentPageName ("PlaySelectionPage");

        yield return _testHelper.MainMenuTransition ("Button_SoloMode");

        yield return _testHelper.AssertCurrentPageName ("HordeSelectionPage");

        yield return _testHelper.RemoveAllHordesExceptDefault ();

        yield return _testHelper.LetsThink ();

        yield return null;
    }

    [UnityTest]
    [Timeout (500000)]
    public IEnumerator Test_H3_CreateARazuHordeAndSave ()
    {
        _testHelper.SetTestName ("Solo - Create a Horde and save");

        yield return _testHelper.MainMenuTransition ("Button_Play");

        yield return _testHelper.AssertIfWentDirectlyToTutorial (
            _testHelper.GoBackToMainAndPressPlay ());

        yield return _testHelper.AssertCurrentPageName ("PlaySelectionPage");

        yield return _testHelper.MainMenuTransition ("Button_SoloMode");

        yield return _testHelper.AssertCurrentPageName ("HordeSelectionPage");

        yield return _testHelper.AddRazuHorde ();

        yield return _testHelper.AssertCurrentPageName ("HordeSelectionPage");

        yield return null;
    }

    [UnityTest]
    [Timeout (500000)]
    public IEnumerator Test_H4_CreateKalileHorde ()
    {
        _testHelper.SetTestName ("Solo - Create a Horde and save");

        yield return _testHelper.MainMenuTransition ("Button_Play");

        yield return _testHelper.AssertIfWentDirectlyToTutorial (
            _testHelper.GoBackToMainAndPressPlay ());

        yield return _testHelper.AssertCurrentPageName ("PlaySelectionPage");

        yield return _testHelper.MainMenuTransition ("Button_SoloMode");

        yield return _testHelper.AssertCurrentPageName ("HordeSelectionPage");

        yield return _testHelper.AddKalileHorde ();

        yield return _testHelper.AssertCurrentPageName ("HordeSelectionPage");

        yield return null;
    }

    [UnityTest]
    [Timeout (500000)]
    public IEnumerator Test_H5_CreateValashHorde ()
    {
        _testHelper.SetTestName ("Solo - Create a Horde and save");

        yield return _testHelper.MainMenuTransition ("Button_Play");

        yield return _testHelper.AssertIfWentDirectlyToTutorial (
            _testHelper.GoBackToMainAndPressPlay ());

        yield return _testHelper.AssertCurrentPageName ("PlaySelectionPage");

        yield return _testHelper.MainMenuTransition ("Button_SoloMode");

        yield return _testHelper.AssertCurrentPageName ("HordeSelectionPage");

        yield return _testHelper.AddValashHorde ();

        yield return _testHelper.AssertCurrentPageName ("HordeSelectionPage");

        yield return null;
    }

    [UnityTest]
    public IEnumerator TestN_Cleanup ()
    {
        // Nothing, just to ascertain cleanup

        yield return null;
    }
}