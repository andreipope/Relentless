using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

public class MultiplayerTests
{
    private TestHelper _testHelper = new TestHelper (0);

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

    [UnityTest]
    [Timeout (500000)]
    public IEnumerator Test_A1_MatchmakingCancel ()
    {
        _testHelper.SetTestName ("PvP - Matchmaking Cancel");
        yield return _testHelper.MainMenuTransition ("Button_Play");

        yield return _testHelper.AssertIfWentDirectlyToTutorial (
            _testHelper.GoBackToMainAndPressPlay ());

        yield return _testHelper.AssertCurrentPageName ("PlaySelectionPage");
        yield return _testHelper.MainMenuTransition ("Button_PvPMode");
        yield return _testHelper.AssertCurrentPageName ("PvPSelectionPage");
        yield return _testHelper.MainMenuTransition ("Button_CasualType");
        yield return _testHelper.AssertCurrentPageName ("HordeSelectionPage");

        int selectedHordeIndex = 0;

        yield return _testHelper.SelectAHordeByIndex (selectedHordeIndex);
        _testHelper.RecordExpectedOverlordName (selectedHordeIndex);
        _testHelper.SetPvPTags (new[] {
            "pvpTest",
            "NoOpponentCancel"
        });

        yield return _testHelper.LetsThink ();

        yield return _testHelper.MainMenuTransition ("Button_Battle");

        yield return _testHelper.LetsThink ();
        yield return _testHelper.LetsThink ();
        yield return _testHelper.LetsThink ();

        yield return _testHelper.ClickGenericButton ("Button_Cancel");

        yield return _testHelper.LetsThink ();

        _testHelper.TestEndHandler ();
    }

    [UnityTest]
    [Timeout (500000)]
    public IEnumerator Test_A2_MatchmakingTimeout ()
    {
        _testHelper.SetTestName ("PvP - Matchmaking Cancel");
        yield return _testHelper.MainMenuTransition ("Button_Play");
        yield return _testHelper.AssertIfWentDirectlyToTutorial (
            _testHelper.GoBackToMainAndPressPlay ());

        yield return _testHelper.AssertCurrentPageName ("PlaySelectionPage");
        yield return _testHelper.MainMenuTransition ("Button_PvPMode");
        yield return _testHelper.AssertCurrentPageName ("PvPSelectionPage");
        yield return _testHelper.MainMenuTransition ("Button_CasualType");
        yield return _testHelper.AssertCurrentPageName ("HordeSelectionPage");

        int selectedHordeIndex = 0;

        yield return _testHelper.SelectAHordeByIndex (selectedHordeIndex);
        _testHelper.RecordExpectedOverlordName (selectedHordeIndex);
        _testHelper.SetPvPTags (new[] {
            "pvpTest",
            "NoOpponentTimeout"
        });

        yield return _testHelper.LetsThink ();

        yield return _testHelper.MainMenuTransition ("Button_Battle");
        yield return _testHelper.AssertPvPStartedOrMatchmakingFailed (
                _testHelper.PlayAMatch (),
                _testHelper.ClickGenericButton ("Button_Cancel"));

        yield return _testHelper.LetsThink ();

        _testHelper.TestEndHandler ();
    }

    [UnityTest]
    [Timeout (500000)]
    public IEnumerator Test_A3_MatchmakeAndQuit ()
    {
        _testHelper.SetTestName ("PvP - Matchmaking And Quit");
        yield return _testHelper.MainMenuTransition ("Button_Play");
        yield return _testHelper.AssertIfWentDirectlyToTutorial (
            _testHelper.GoBackToMainAndPressPlay ());

        yield return _testHelper.AssertCurrentPageName ("PlaySelectionPage");
        yield return _testHelper.MainMenuTransition ("Button_PvPMode");
        yield return _testHelper.AssertCurrentPageName ("PvPSelectionPage");
        yield return _testHelper.MainMenuTransition ("Button_CasualType");
        yield return _testHelper.AssertCurrentPageName ("HordeSelectionPage");

        int selectedHordeIndex = 0;

        yield return _testHelper.SelectAHordeByIndex (selectedHordeIndex);
        _testHelper.RecordExpectedOverlordName (selectedHordeIndex);
        _testHelper.SetPvPTags (new[] {
            "pvpTest"
        });

        yield return _testHelper.LetsThink ();

        yield return _testHelper.MainMenuTransition ("Button_Battle");
        yield return _testHelper.AssertCurrentPageName ("GameplayPage");
        yield return _testHelper.WaitUntilPlayerOrderIsDecided ();
        _testHelper.AssertOverlordName ();

        yield return _testHelper.ClickGenericButton ("Button_Settings");
        yield return _testHelper.ClickGenericButton ("Button_QuitToMainMenu");
        yield return _testHelper.RespondToYesNoOverlay (true);

        yield return _testHelper.LetsThink ();

        _testHelper.TestEndHandler ();
    }

    [UnityTest]
    [Timeout (500000)]
    public IEnumerator Test_A4_MatchmakeWaitForOurTurnAndQuit ()
    {
        _testHelper.SetTestName ("PvP - Matchmake, Wait for Our Turn and Quit");

        yield return _testHelper.ClickGenericButton ("Button_Play");

        yield return _testHelper.AssertIfWentDirectlyToTutorial (
            _testHelper.GoBackToMainAndPressPlay ());

        yield return _testHelper.AssertCurrentPageName ("PlaySelectionPage");
        yield return _testHelper.ClickGenericButton ("Button_PvPMode");
        yield return _testHelper.AssertCurrentPageName ("PvPSelectionPage");
        yield return _testHelper.ClickGenericButton ("Button_CasualType");
        yield return _testHelper.AssertCurrentPageName ("HordeSelectionPage");

        int selectedHordeIndex = 0;

        yield return _testHelper.SelectAHordeByIndex (selectedHordeIndex);
        _testHelper.RecordExpectedOverlordName (selectedHordeIndex);
        _testHelper.SetPvPTags (new[] {
            "pvpTest"
        });

        yield return _testHelper.LetsThink ();

        yield return _testHelper.ClickGenericButton ("Button_Battle");
        yield return _testHelper.AssertCurrentPageName ("GameplayPage");
        yield return _testHelper.WaitUntilPlayerOrderIsDecided ();
        yield return _testHelper.AssertMulliganPopupCameUp (
            _testHelper.ClickGenericButton ("Button_Keep"),
            null);
        yield return _testHelper.WaitUntilOurFirstTurn ();
        yield return _testHelper.ClickGenericButton ("Button_Settings");
        yield return _testHelper.ClickGenericButton ("Button_QuitToMainMenu");
        yield return _testHelper.RespondToYesNoOverlay (true);

        _testHelper.TestEndHandler ();
    }

    [UnityTest]
    [Timeout (500000)]
    public IEnumerator Test_A5_MatchmakeMakeOneMoveAndQuit ()
    {
        _testHelper.SetTestName ("PvP - Matchmake, Make One Move And Quit");

        yield return _testHelper.MainMenuTransition ("Button_Play");
        yield return _testHelper.AssertIfWentDirectlyToTutorial (
            _testHelper.GoBackToMainAndPressPlay ());

        yield return _testHelper.AssertCurrentPageName ("PlaySelectionPage");
        yield return _testHelper.MainMenuTransition ("Button_PvPMode");
        yield return _testHelper.AssertCurrentPageName ("PvPSelectionPage");
        yield return _testHelper.MainMenuTransition ("Button_CasualType");
        yield return _testHelper.AssertCurrentPageName ("HordeSelectionPage");

        int selectedHordeIndex = 0;

        yield return _testHelper.SelectAHordeByIndex (selectedHordeIndex);
        _testHelper.RecordExpectedOverlordName (selectedHordeIndex);
        _testHelper.SetPvPTags (new[] {
            "pvpTest"
        });

        yield return _testHelper.LetsThink ();

        yield return _testHelper.MainMenuTransition ("Button_Battle");
        yield return _testHelper.PlayAMatch (1);
        yield return _testHelper.ClickGenericButton ("Button_Settings");
        yield return _testHelper.ClickGenericButton ("Button_QuitToMainMenu");
        yield return _testHelper.RespondToYesNoOverlay (true);

        _testHelper.TestEndHandler ();
    }

    [UnityTest]
    [Timeout (900000)]
    public IEnumerator Test_A6_MatchmakeAndPlay ()
    {
        _testHelper.SetTestName ("PvP - Matchmaking And Play");
        yield return _testHelper.MainMenuTransition ("Button_Play");
        yield return _testHelper.AssertIfWentDirectlyToTutorial (
            _testHelper.GoBackToMainAndPressPlay ());

        yield return _testHelper.AssertCurrentPageName ("PlaySelectionPage");
        yield return _testHelper.MainMenuTransition ("Button_PvPMode");
        yield return _testHelper.AssertCurrentPageName ("PvPSelectionPage");
        yield return _testHelper.MainMenuTransition ("Button_CasualType");
        yield return _testHelper.AssertCurrentPageName ("HordeSelectionPage");

        int selectedHordeIndex = 0;

        yield return _testHelper.SelectAHordeByIndex (selectedHordeIndex);
        _testHelper.RecordExpectedOverlordName (selectedHordeIndex);
        _testHelper.SetPvPTags (new[] {
            "pvpTest"
        });

        yield return _testHelper.LetsThink ();

        yield return _testHelper.MainMenuTransition ("Button_Battle");
        yield return _testHelper.PlayAMatch ();

        _testHelper.TestEndHandler ();
    }

    [UnityTest]
    [Timeout (500000)]
    public IEnumerator Test_A7_MatchmakingCancelAndMatchmake ()
    {
        _testHelper.SetTestName ("PvP - Create a Horde and save");
        yield return _testHelper.MainMenuTransition ("Button_Play");
        yield return _testHelper.AssertIfWentDirectlyToTutorial (
            _testHelper.GoBackToMainAndPressPlay ());

        yield return _testHelper.AssertCurrentPageName ("PlaySelectionPage");
        yield return _testHelper.MainMenuTransition ("Button_PvPMode");
        yield return _testHelper.AssertCurrentPageName ("PvPSelectionPage");
        yield return _testHelper.MainMenuTransition ("Button_CasualType");
        yield return _testHelper.AssertCurrentPageName ("HordeSelectionPage");

        int selectedHordeIndex = 0;

        yield return _testHelper.SelectAHordeByIndex (selectedHordeIndex);
        _testHelper.RecordExpectedOverlordName (selectedHordeIndex);

        #region Matchmaking Cancel

        _testHelper.SetPvPTags (new[] {
            "pvpTestNoOpponentCancel"
        });

        yield return _testHelper.LetsThink ();
        yield return _testHelper.MainMenuTransition ("Button_Battle");

        yield return _testHelper.LetsThink ();
        yield return _testHelper.LetsThink ();
        yield return _testHelper.LetsThink ();

        yield return _testHelper.ClickGenericButton ("Button_Cancel");

        #endregion

        yield return _testHelper.LetsThink ();
        yield return _testHelper.LetsThink ();

        #region Matchmake and Quit

        _testHelper.SetPvPTags (new[] {
            "pvpTest"
        });

        yield return _testHelper.LetsThink ();

        yield return _testHelper.MainMenuTransition ("Button_Battle");
        yield return _testHelper.AssertCurrentPageName ("GameplayPage");
        yield return _testHelper.WaitUntilPlayerOrderIsDecided ();
        _testHelper.AssertOverlordName ();
        yield return _testHelper.ClickGenericButton ("Button_Settings");

        yield return _testHelper.LetsThink ();

        yield return _testHelper.ClickGenericButton ("Button_QuitToMainMenu");

        yield return _testHelper.LetsThink ();

        yield return _testHelper.RespondToYesNoOverlay (true);

        yield return _testHelper.LetsThink ();

        #endregion

        _testHelper.TestEndHandler ();
    }

    [UnityTest]
    public IEnumerator TestN_Cleanup ()
    {
        // Nothing, just to ascertain cleanup

        yield return null;
    }
}


public class MultiplayerPassiveTests
{
    private TestHelper _testHelper = new TestHelper (1);

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

    [UnityTest]
    [Timeout (1800000)]
    public IEnumerator Test_A1_MatchmakeAndPlay ()
    {
        _testHelper.SetTestName ("PvP - Passive Matchmake And Play");
        _testHelper.SetPvPTags (new[] {
            "pvpTest"
        });

        yield return _testHelper.LetsThink ();

        yield return _testHelper.MainMenuTransition ("Button_Play");
        yield return _testHelper.AssertIfWentDirectlyToTutorial (
            _testHelper.GoBackToMainAndPressPlay ());

        yield return _testHelper.AssertCurrentPageName ("PlaySelectionPage");
        yield return _testHelper.MainMenuTransition ("Button_PvPMode");
        yield return _testHelper.AssertCurrentPageName ("PvPSelectionPage");
        yield return _testHelper.MainMenuTransition ("Button_CasualType");
        yield return _testHelper.AssertCurrentPageName ("HordeSelectionPage");

        int selectedHordeIndex = 0;

        yield return _testHelper.SelectAHordeByIndex (selectedHordeIndex);
        _testHelper.RecordExpectedOverlordName (selectedHordeIndex);

        while (true)
        {
            yield return _testHelper.ClickGenericButton ("Button_Battle");
            yield return _testHelper.AssertPvPStartedOrMatchmakingFailed (
                _testHelper.PlayAMatch (),
                _testHelper.PressOK ());

            yield return _testHelper.LetsThink ();
        }
    }

    [UnityTest]
    public IEnumerator TestN_Cleanup ()
    {
        // Nothing, just to ascertain cleanup

        yield return null;
    }
}
