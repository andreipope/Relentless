using NUnit.Framework;
using System.Collections;
using UnityEngine;
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
        {
            yield break;
        }

        _testHelper.InitalizePlayer ();

        if (!_testHelper.IsTestFinished)
            yield return _testHelper.WaitUntilPlayerOrderIsDecided ();

        if (assertOverlordName)
        {
            _testHelper.AssertOverlordName ();
        }

        if (!_testHelper.IsTestFinished)
            yield return _testHelper.AssertMulliganPopupCameUp (
                _testHelper.DecideWhichCardsToPick (),
                null);

        if (!_testHelper.IsTestFinished)
            yield return _testHelper.WaitUntilOurFirstTurn ();

        if (!_testHelper.IsTestFinished)
            yield return _testHelper.MakeMoves ();

        yield return null;
    }

    [UnityTest]
    [Timeout (900000)]
    public IEnumerator Test_G1_PlayWithDefaultHorde ()
    {
        _testHelper.SetTestName ("Solo - Gameplay with Default");

        #region Solo Gameplay

        yield return _testHelper.MainMenuTransition ("Button_Play");

        yield return _testHelper.AssertIfWentDirectlyToTutorial (
            _testHelper.GoBackToMainAndPressPlay ());

        yield return _testHelper.AssertCurrentPageName ("PlaySelectionPage");

        yield return _testHelper.MainMenuTransition ("Button_SoloMode2");

        yield return _testHelper.AssertCurrentPageName ("HordeSelectionPage");

        int selectedHordeIndex = 0;

        yield return _testHelper.SelectAHordeByIndex (selectedHordeIndex);

        _testHelper.RecordExpectedOverlordName (selectedHordeIndex);

        yield return _testHelper.MainMenuTransition ("Button_Battle");

        yield return _testHelper.AssertCurrentPageName ("GameplayPage");

        yield return SoloGameplay (true);

        yield return _testHelper.ClickGenericButton ("Button_Continue");

        yield return _testHelper.AssertCurrentPageName ("HordeSelectionPage");

        Debug.LogWarning ("0");

        #endregion

        _testHelper.TestEndHandler ();

        yield return null;
    }

    [UnityTest]
    [Timeout (900000)]
    public IEnumerator Test_G2_PlayWithRazuHorde ()
    {
        _testHelper.SetTestName ("Solo - Gameplay with Razu");

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
            yield return _testHelper.SelectAHordeByName ("Razu");

        if (!_testHelper.IsTestFinished)
            _testHelper.RecordExpectedOverlordName (_testHelper.SelectedHordeIndex);

        if (!_testHelper.IsTestFinished)
            yield return _testHelper.MainMenuTransition ("Button_Battle");

        if (!_testHelper.IsTestFinished)
            yield return _testHelper.AssertCurrentPageName ("GameplayPage");

        if (!_testHelper.IsTestFinished)
            yield return SoloGameplay (true);

        if (!_testHelper.IsTestFinished)
            yield return _testHelper.ClickGenericButton ("Button_Continue");

        if (!_testHelper.IsTestFinished)
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
    }

    [UnityTest]
    public IEnumerator TestN_Cleanup ()
    {
        // Nothing, just to ascertain cleanup

        yield return null;
    }
}

public class TutorialTests
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
        if (TestContext.CurrentContext.Test.Name == "TestN_Cleanup")
        {
            yield return _testHelper.TearDown_Cleanup ();
        }
        else
        {
            yield return _testHelper.TearDown_GoBackToMainScreen ();
        }

        yield return _testHelper.ReportTestTime ();
    }

    #endregion

    private IEnumerator SkipTutorial ()
    {
        yield return _testHelper.ClickGenericButton ("Button_Skip");

        yield return _testHelper.RespondToYesNoOverlay (true);

        yield return _testHelper.ClickGenericButton ("Button_Skip");

        yield return _testHelper.RespondToYesNoOverlay (true);

        yield return null;
    }

    private IEnumerator PlayTutorial_Part1 ()
    {
        for (int i = 0; i < 3; i++)
        {
            yield return _testHelper.ClickGenericButton ("Button_Next");
        }

        yield return _testHelper.ClickGenericButton ("Button_Play");

        for (int i = 0; i < 4; i++)
        {
            yield return _testHelper.ClickGenericButton ("Button_Next");
        }

        yield return _testHelper.WaitUntilWeHaveACardAtHand ();

        yield return _testHelper.PlayCardFromHandToBoard (new[] { 0 });

        yield return _testHelper.ClickGenericButton ("Button_Next");

        yield return _testHelper.EndTurn ();

        yield return _testHelper.WaitUntilCardIsAddedToBoard ("OpponentBoard");
        yield return _testHelper.WaitUntilAIBrainStops ();

        yield return _testHelper.ClickGenericButton ("Button_Next");

        yield return _testHelper.WaitUntilOurTurnStarts ();
        yield return _testHelper.WaitUntilInputIsUnblocked ();

        yield return _testHelper.LetsThink ();
        yield return _testHelper.LetsThink ();
        yield return _testHelper.LetsThink ();

        yield return _testHelper.PlayCardFromBoardToOpponent (new[] { 0 }, new[] { 0 });

        for (int i = 0; i < 2; i++)
        {
            yield return _testHelper.ClickGenericButton ("Button_Next");
        }

        yield return _testHelper.EndTurn ();

        yield return _testHelper.WaitUntilOurTurnStarts ();
        yield return _testHelper.WaitUntilInputIsUnblocked ();

        yield return _testHelper.ClickGenericButton ("Button_Next");

        yield return _testHelper.LetsThink ();
        yield return _testHelper.LetsThink ();
        yield return _testHelper.LetsThink ();

        yield return _testHelper.PlayCardFromBoardToOpponent (new[] { 0 }, null, true);

        yield return _testHelper.ClickGenericButton ("Button_Next");

        yield return _testHelper.LetsThink ();
        yield return _testHelper.LetsThink ();

        yield return _testHelper.EndTurn ();

        yield return _testHelper.WaitUntilOurTurnStarts ();
        yield return _testHelper.WaitUntilInputIsUnblocked ();

        yield return _testHelper.ClickGenericButton ("Button_Next");

        yield return _testHelper.LetsThink ();
        yield return _testHelper.LetsThink ();
        yield return _testHelper.LetsThink ();

        yield return _testHelper.PlayCardFromBoardToOpponent (new[] { 0 }, new[] { 0 });

        for (int i = 0; i < 3; i++)
        {
            yield return _testHelper.ClickGenericButton ("Button_Next");
        }

        yield return _testHelper.WaitUntilAIBrainStops ();
        yield return _testHelper.WaitUntilInputIsUnblocked ();

        yield return _testHelper.LetsThink ();
        yield return _testHelper.LetsThink ();
        yield return _testHelper.LetsThink ();

        yield return _testHelper.PlayCardFromHandToBoard (new[] { 1 });

        yield return _testHelper.LetsThink ();
        yield return _testHelper.LetsThink ();
        yield return _testHelper.LetsThink ();

        yield return _testHelper.PlayCardFromBoardToOpponent (new[] { 0 }, null, true);

        yield return _testHelper.LetsThink ();
        yield return _testHelper.LetsThink ();

        for (int i = 0; i < 3; i++)
        {
            yield return _testHelper.ClickGenericButton ("Button_Next");
        }

        yield return _testHelper.UseSkillToOpponentPlayer ();

        for (int i = 0; i < 4; i++)
        {
            yield return _testHelper.ClickGenericButton ("Button_Next");
        }

        yield return null;
    }

    private IEnumerator PlayTutorial_Part2 ()
    {
        yield return _testHelper.ClickGenericButton ("Button_Next");

        yield return _testHelper.WaitUntilOurTurnStarts ();
        yield return _testHelper.WaitUntilInputIsUnblocked ();

        for (int i = 0; i < 11; i++)
        {
            yield return _testHelper.ClickGenericButton ("Button_Next");
        }

        yield return _testHelper.PlayCardFromHandToBoard (new[] { 1 });

        yield return _testHelper.ClickGenericButton ("Button_Next");

        yield return _testHelper.LetsThink ();
        yield return _testHelper.LetsThink ();

        yield return _testHelper.PlayCardFromHandToBoard (new[] { 0 });

        for (int i = 0; i < 12; i++)
        {
            yield return _testHelper.ClickGenericButton ("Button_Next");
        }

        yield return _testHelper.LetsThink ();

        yield return _testHelper.PlayNonSleepingCardsFromBoardToOpponentPlayer ();

        for (int i = 0; i < 5; i++)
        {
            yield return _testHelper.ClickGenericButton ("Button_Next");
        }

        yield return null;
    }

    [UnityTest]
    [Timeout (500000)]
    public IEnumerator Test_T1_TutorialNonSkip ()
    {
        _testHelper.SetTestName ("Solo - Tutorial Non-Skip");

        yield return _testHelper.MainMenuTransition ("Button_Play");

        yield return _testHelper.AssertIfWentDirectlyToTutorial (
            _testHelper.GoBackToMainAndPressPlay ());

        yield return _testHelper.AssertCurrentPageName ("PlaySelectionPage");

        #region Tutorial Non-Skip

        yield return _testHelper.MainMenuTransition ("Button_Tutorial");

        yield return _testHelper.AssertCurrentPageName ("GameplayPage");

        yield return PlayTutorial_Part1 ();

        yield return _testHelper.ClickGenericButton ("Button_Continue");

        yield return PlayTutorial_Part2 ();

        yield return _testHelper.ClickGenericButton ("Button_Continue");

        yield return _testHelper.AssertCurrentPageName ("HordeSelectionPage");

        #endregion
    }

    [UnityTest]
    [Timeout (500000)]
    public IEnumerator Test_T2_TutorialSkip ()
    {
        _testHelper.SetTestName ("Solo - Tutorial Skip");

        yield return _testHelper.MainMenuTransition ("Button_Play");

        yield return _testHelper.AssertIfWentDirectlyToTutorial (
            _testHelper.GoBackToMainAndPressPlay ());

        yield return _testHelper.AssertCurrentPageName ("PlaySelectionPage");

        #region Tutorial Skip

        yield return _testHelper.MainMenuTransition ("Button_Tutorial");

        yield return _testHelper.AssertCurrentPageName ("GameplayPage");

        yield return SkipTutorial ();

        #endregion
    }

    [UnityTest]
    public IEnumerator TestN_Cleanup ()
    {
        // Nothing, just to ascertain cleanup

        yield return null;
    }
}

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