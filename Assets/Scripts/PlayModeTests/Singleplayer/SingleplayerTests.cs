using NUnit.Framework;
using System.Collections;
using UnityEngine.TestTools;

public class SingleplayerTests
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

    private IEnumerator SoloGameplay (bool assertOverlordName = false)
    {
        if (_testHelper.IsTestFinished)
            yield break;

        _testHelper.InitalizePlayer ();

        yield return _testHelper.WaitUntilPlayerOrderIsDecided ();

        if (assertOverlordName)
        {
            _testHelper.AssertOverlordName ();
        }

        yield return _testHelper.AssertMulliganPopupCameUp (
            _testHelper.DecideWhichCardsToPick (),
            null);

        yield return _testHelper.WaitUntilOurFirstTurn ();

        yield return _testHelper.MakeMoves ();

        yield return null;
    }

    [UnityTest]
    [Timeout (900000)]
    public IEnumerator Test_G1_PlayWithDefaultHorde ()
    {
        _testHelper.SetTestName ("Solo - Gameplay with Default");

        #region Solo Gameplay

        yield return _testHelper.ClickGenericButton ("Button_Play");

        yield return _testHelper.AssertIfWentDirectlyToTutorial (
            _testHelper.GoBackToMainAndPressPlay ());

        yield return _testHelper.AssertCurrentPageName ("PlaySelectionPage");

        yield return _testHelper.ClickGenericButton ("Button_SoloMode");

        yield return _testHelper.AssertCurrentPageName ("HordeSelectionPage");

        int selectedHordeIndex = 0;

        yield return _testHelper.SelectAHordeByIndex (selectedHordeIndex);

        _testHelper.RecordExpectedOverlordName (selectedHordeIndex);

        yield return _testHelper.ClickGenericButton ("Button_Battle");

        yield return _testHelper.AssertCurrentPageName ("GameplayPage");

        yield return SoloGameplay (true);

        yield return _testHelper.ClickGenericButton ("Button_Continue");

        yield return _testHelper.AssertCurrentPageName ("HordeSelectionPage");

        #endregion

        _testHelper.TestEndHandler ();
    }

    [UnityTest]
    [Timeout (900000)]
    public IEnumerator Test_G2_PlayWithRazuHorde ()
    {
        _testHelper.SetTestName ("Solo - Gameplay with Razu");

        #region Solo Gameplay

        yield return _testHelper.ClickGenericButton ("Button_Play");

        yield return _testHelper.AssertIfWentDirectlyToTutorial (
            _testHelper.GoBackToMainAndPressPlay ());

        yield return _testHelper.AssertCurrentPageName ("PlaySelectionPage");

        yield return _testHelper.ClickGenericButton ("Button_SoloMode");

        yield return _testHelper.AssertCurrentPageName ("HordeSelectionPage");

        yield return _testHelper.SelectAHordeByName ("Razu");

        _testHelper.RecordExpectedOverlordName (_testHelper.SelectedHordeIndex);

        yield return _testHelper.ClickGenericButton ("Button_Battle");

        yield return _testHelper.AssertCurrentPageName ("GameplayPage");

        yield return SoloGameplay (true);

        yield return _testHelper.ClickGenericButton ("Button_Continue");

        yield return _testHelper.AssertCurrentPageName ("HordeSelectionPage");

        #endregion

        _testHelper.TestEndHandler ();
    }

    [UnityTest]
    [Timeout (900000)]
    public IEnumerator Test_G3_PlayWithKalileHorde ()
    {
        _testHelper.SetTestName ("Solo - Gameplay with Kalile");

        #region Solo Gameplay

        if (!_testHelper.IsTestFinished)
            yield return _testHelper.MainMenuTransition ("Button_Play");

        if (!_testHelper.IsTestFinished)
            yield return _testHelper.AssertIfWentDirectlyToTutorial (
                _testHelper.GoBackToMainAndPressPlay ());

        if (!_testHelper.IsTestFinished)
            yield return _testHelper.AssertCurrentPageName ("PlaySelectionPage");

        if (!_testHelper.IsTestFinished)
            yield return _testHelper.MainMenuTransition ("Button_SoloMode");

        if (!_testHelper.IsTestFinished)
            yield return _testHelper.AssertCurrentPageName ("HordeSelectionPage");

        if (!_testHelper.IsTestFinished)
            yield return _testHelper.SelectAHordeByName ("Kalile");

        if (!_testHelper.IsTestFinished)
            if (!_testHelper.IsTestFinished)
                _testHelper.RecordExpectedOverlordName (_testHelper.SelectedHordeIndex);

        if (!_testHelper.IsTestFinished)
            yield return _testHelper.MainMenuTransition ("Button_Battle");

        if (!_testHelper.IsTestFinished)
            yield return _testHelper.AssertCurrentPageName ("GameplayPage");

        if (!_testHelper.IsTestFinished)
            yield return SoloGameplay ();

        if (!_testHelper.IsTestFinished)
            yield return _testHelper.ClickGenericButton ("Button_Continue");

        if (!_testHelper.IsTestFinished)
            yield return _testHelper.AssertCurrentPageName ("HordeSelectionPage");

        #endregion

        _testHelper.TestEndHandler ();
    }

    [UnityTest]
    [Timeout (900000)]
    public IEnumerator Test_G4_PlayWithValashHorde ()
    {
        _testHelper.SetTestName ("Solo - Gameplay with Valash");

        #region Solo Gameplay

        if (!_testHelper.IsTestFinished)
            yield return _testHelper.MainMenuTransition ("Button_Play");

        if (!_testHelper.IsTestFinished)
            yield return _testHelper.AssertIfWentDirectlyToTutorial (
                _testHelper.GoBackToMainAndPressPlay ());

        if (!_testHelper.IsTestFinished)
            yield return _testHelper.AssertCurrentPageName ("PlaySelectionPage");

        if (!_testHelper.IsTestFinished)
            yield return _testHelper.MainMenuTransition ("Button_SoloMode");

        if (!_testHelper.IsTestFinished)
            yield return _testHelper.AssertCurrentPageName ("HordeSelectionPage");

        if (!_testHelper.IsTestFinished)
            yield return _testHelper.SelectAHordeByName ("Valash");

        if (!_testHelper.IsTestFinished)
            _testHelper.RecordExpectedOverlordName (_testHelper.SelectedHordeIndex);

        if (!_testHelper.IsTestFinished)
            yield return _testHelper.MainMenuTransition ("Button_Battle");

        if (!_testHelper.IsTestFinished)
            yield return _testHelper.AssertCurrentPageName ("GameplayPage");

        if (!_testHelper.IsTestFinished)
            yield return SoloGameplay ();

        if (!_testHelper.IsTestFinished)
            yield return _testHelper.ClickGenericButton ("Button_Continue");

        if (!_testHelper.IsTestFinished)
            yield return _testHelper.AssertCurrentPageName ("HordeSelectionPage");

        #endregion

        _testHelper.TestEndHandler ();
    }

    [UnityTest]
    [Timeout (3600000)]
    public IEnumerator Test_G5_PlayWithAllHordes ()
    {
        _testHelper.SetTestName ("Solo - Gameplay with All Hordes");

        if (!_testHelper.IsTestFinished)
            yield return _testHelper.MainMenuTransition ("Button_Play");

        if (!_testHelper.IsTestFinished)
            yield return _testHelper.AssertIfWentDirectlyToTutorial (
                _testHelper.GoBackToMainAndPressPlay ());

        if (!_testHelper.IsTestFinished)
            yield return _testHelper.AssertCurrentPageName ("PlaySelectionPage");

        if (!_testHelper.IsTestFinished)
            yield return _testHelper.MainMenuTransition ("Button_SoloMode");

        if (!_testHelper.IsTestFinished)
            yield return _testHelper.AssertCurrentPageName ("HordeSelectionPage");

        float allowedTestTime = 3000f;
        int hordeIndex = -1;
        while (_testHelper.GetTestTime () <= allowedTestTime)
        {
            if (_testHelper.IsTestFinished)
                break;
                
            hordeIndex = (hordeIndex + 1) % (_testHelper.GetNumberOfHordes () - 1);

            int selectedHordeIndex = hordeIndex;

            if (!_testHelper.IsTestFinished)
                yield return _testHelper.SelectAHordeByIndex (selectedHordeIndex);

            if (!_testHelper.IsTestFinished)
                _testHelper.RecordExpectedOverlordName (selectedHordeIndex);

            if (!_testHelper.IsTestFinished)
                yield return _testHelper.MainMenuTransition ("Button_Battle");

            if (!_testHelper.IsTestFinished)
                yield return _testHelper.AssertCurrentPageName ("GameplayPage");

            if (!_testHelper.IsTestFinished)
                yield return SoloGameplay ();

            if (!_testHelper.IsTestFinished)
                yield return _testHelper.ClickGenericButton ("Button_Continue");

            if (!_testHelper.IsTestFinished)
                yield return _testHelper.AssertCurrentPageName ("HordeSelectionPage");

            yield return _testHelper.LetsThink ();
            yield return _testHelper.LetsThink ();
            yield return _testHelper.LetsThink ();
        }

        _testHelper.TestEndHandler ();
    }

    [UnityTest]
    [Timeout (900000)]
    public IEnumerator Test_G6_StartPlayingQuitAndCheckButtons ()
    {
        _testHelper.SetTestName ("Solo - Start Playing, Quit and Check if Buttons Work");

        if (!_testHelper.IsTestFinished)
            yield return _testHelper.MainMenuTransition ("Button_Play");

        if (!_testHelper.IsTestFinished)
            yield return _testHelper.AssertIfWentDirectlyToTutorial (
                _testHelper.GoBackToMainAndPressPlay ());

        if (!_testHelper.IsTestFinished)
            yield return _testHelper.AssertCurrentPageName ("PlaySelectionPage");

        if (!_testHelper.IsTestFinished)
            yield return _testHelper.MainMenuTransition ("Button_SoloMode");

        if (!_testHelper.IsTestFinished)
            yield return _testHelper.AssertCurrentPageName ("HordeSelectionPage");

        int selectedHordeIndex = 0;

        if (!_testHelper.IsTestFinished)
            yield return _testHelper.SelectAHordeByIndex (selectedHordeIndex);

        if (!_testHelper.IsTestFinished)
            _testHelper.RecordExpectedOverlordName (selectedHordeIndex);

        if (!_testHelper.IsTestFinished)
            yield return _testHelper.MainMenuTransition ("Button_Battle");

        if (!_testHelper.IsTestFinished)
            yield return _testHelper.AssertCurrentPageName ("GameplayPage");

        if (!_testHelper.IsTestFinished)
            yield return _testHelper.WaitUntilPlayerOrderIsDecided ();

        if (!_testHelper.IsTestFinished)
            _testHelper.AssertOverlordName ();

        if (!_testHelper.IsTestFinished)
            yield return _testHelper.ClickGenericButton ("Button_Settings");

        if (!_testHelper.IsTestFinished)
            yield return _testHelper.ClickGenericButton ("Button_QuitToMainMenu");

        if (!_testHelper.IsTestFinished)
            yield return _testHelper.RespondToYesNoOverlay (true);

        if (!_testHelper.IsTestFinished)
            yield return _testHelper.AssertCurrentPageName ("MainMenuPage");

        yield return _testHelper.LetsThink ();
        yield return _testHelper.LetsThink ();

        if (!_testHelper.IsTestFinished)
            yield return _testHelper.ButtonListClickCheck (new string[] {
                "Button_Army",
                "Button_Credits",
                "Button_OpenPacks",
                "Button_Play",
                "Button_Settings",
                "Button_Shop"
            });

        yield return _testHelper.LetsThink ();

        _testHelper.TestEndHandler ();
    }

    [UnityTest]
    public IEnumerator TestN_Cleanup ()
    {
        // Nothing, just to ascertain cleanup

        yield return null;
    }
}