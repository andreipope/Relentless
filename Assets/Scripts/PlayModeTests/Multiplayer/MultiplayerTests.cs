using NUnit.Framework;
using System.Collections;
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
    public IEnumerator Test1_SingleplayerTrial()
    {
        _testHelper.SetTestName ("Tutorial - Singleplayer Trial");

        yield return _testHelper.MainMenuTransition ("Button_Play");

        yield return _testHelper.AssertCurrentPageName ("PlaySelectionPage");

        yield return _testHelper.MainMenuTransition ("Button_SoloMode");

        yield return _testHelper.AssertCurrentPageName ("HordeSelectionPage");

        yield return _testHelper.MainMenuTransition ("Button_Battle");

        yield return _testHelper.AssertCurrentPageName ("GameplayPage");

        _testHelper.InitalizePlayer ();

        yield return _testHelper.WaitUntilPlayerOrderIsDecided ();

        yield return _testHelper.DecideWhichCardsToPick ();

        yield return _testHelper.WaitUntilOurFirstTurn ();

        yield return _testHelper.MakeMoves ();

        yield return _testHelper.ClickGenericButton ("Button_Continue");

        yield return _testHelper.AssertCurrentPageName ("HordeSelectionPage");
    }

    [UnityTest]
    [Timeout (500000)]
    public IEnumerator Test2_MatchmakingCancel ()
    {
        _testHelper.SetTestName ("Tutorial - Matchmaking Cancel");

        yield return _testHelper.MainMenuTransition ("Button_Play");

        yield return _testHelper.AssertCurrentPageName ("PlaySelectionPage");

        yield return _testHelper.MainMenuTransition ("Button_PvPMode");

        yield return _testHelper.AssertCurrentPageName ("PvPSelectionPage");

        yield return _testHelper.MainMenuTransition ("Button_CasualType");

        yield return _testHelper.AssertCurrentPageName ("HordeSelectionPage");

        _testHelper.SetPvPTags (new[] {
            "pvpTestNoOpponent"
        });

        yield return _testHelper.MainMenuTransition ("Button_Battle");

        yield return _testHelper.ClickGenericButton ("Button_Cancel");

        yield return _testHelper.LetsThink ();
    }

    [UnityTest]
    [Timeout (500000)]
    public IEnumerator Test3_MatchmakeAndQuit ()
    {
        _testHelper.SetTestName ("Tutorial - Matchmaking And Cancel");

        yield return _testHelper.MainMenuTransition ("Button_Play");

        yield return _testHelper.AssertCurrentPageName ("PlaySelectionPage");

        yield return _testHelper.MainMenuTransition ("Button_PvPMode");

        yield return _testHelper.AssertCurrentPageName ("PvPSelectionPage");

        yield return _testHelper.MainMenuTransition ("Button_CasualType");

        yield return _testHelper.AssertCurrentPageName ("HordeSelectionPage");

        _testHelper.SetPvPTags (new[] {
            "pvpTest"
        });

        yield return _testHelper.MainMenuTransition ("Button_Battle");

        yield return _testHelper.AssertCurrentPageName ("GameplayPage");

        yield return _testHelper.WaitUntilPlayerOrderIsDecided ();

        yield return _testHelper.ClickGenericButton ("Button_Settings");

        yield return _testHelper.ClickGenericButton ("Button_QuitToMainMenu");

        yield return _testHelper.RespondToYesNoOverlay (true);

        yield return _testHelper.GoBackToMainPage ();

        yield return _testHelper.LetsThink ();
    }

    [UnityTest]
    [Timeout (500000)]
    public IEnumerator Test4_MatchmakeAndPlay ()
    {
        _testHelper.SetTestName ("Tutorial - Matchmaking And Play");

        yield return _testHelper.MainMenuTransition ("Button_Play");

        yield return _testHelper.AssertCurrentPageName ("PlaySelectionPage");

        yield return _testHelper.MainMenuTransition ("Button_PvPMode");

        yield return _testHelper.AssertCurrentPageName ("PvPSelectionPage");

        yield return _testHelper.MainMenuTransition ("Button_CasualType");

        yield return _testHelper.AssertCurrentPageName ("HordeSelectionPage");

        _testHelper.SetPvPTags (new[] {
            "pvpTest"
        });

        yield return _testHelper.MainMenuTransition ("Button_Battle");

        yield return _testHelper.AssertCurrentPageName ("GameplayPage");

        _testHelper.InitalizePlayer ();

        yield return _testHelper.WaitUntilPlayerOrderIsDecided ();

        yield return _testHelper.DecideWhichCardsToPick ();

        yield return _testHelper.WaitUntilOurFirstTurn ();

        yield return _testHelper.MakeMoves ();

        yield return _testHelper.ClickGenericButton ("Button_Continue");

        yield return _testHelper.AssertCurrentPageName ("HordeSelectionPage");
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
    [Timeout (500000)]
    public IEnumerator Test1_MatchmakeAndPlay ()
    {
        _testHelper.SetTestName ("Tutorial - Matchmaking And Play");

        yield return _testHelper.MainMenuTransition ("Button_Play");

        yield return _testHelper.AssertCurrentPageName ("PlaySelectionPage");

        yield return _testHelper.MainMenuTransition ("Button_PvPMode");

        yield return _testHelper.AssertCurrentPageName ("PvPSelectionPage");

        yield return _testHelper.MainMenuTransition ("Button_CasualType");

        yield return _testHelper.AssertCurrentPageName ("HordeSelectionPage");

        _testHelper.SetPvPTags (new[] {
            "pvpTest"
        });

        while (true)
        {
            yield return _testHelper.ClickGenericButton ("Button_Battle");

            yield return _testHelper.AssertPvPStartedOrMatchmakingFailed (
                PlayAMatch (),
                PressOK ());

            yield return _testHelper.LetsThink ();
        }
    }

    private IEnumerator PlayAMatch ()
    {
        yield return _testHelper.AssertCurrentPageName ("GameplayPage");

        _testHelper.InitalizePlayer ();

        yield return _testHelper.WaitUntilPlayerOrderIsDecided ();

        yield return _testHelper.DecideWhichCardsToPick ();

        yield return _testHelper.WaitUntilOurFirstTurn ();

        yield return _testHelper.MakeMoves ();

        yield return _testHelper.ClickGenericButton ("Button_Continue");

        yield return _testHelper.AssertCurrentPageName ("HordeSelectionPage");
    }

    private IEnumerator PressOK ()
    {
        yield return _testHelper.ClickGenericButton ("Button_OK");
    }
}