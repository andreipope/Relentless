using Loom.ZombieBattleground;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

public class MultiplayerTests
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

    [UnityTest]
    [Timeout (500000)]
    public IEnumerator Test1_MatchmakingTrial()
    {
        _testHelper.SetTestName ("Tutorial - Matchmaking");

        /* yield return _testHelper.MainMenuTransition ("Button_Play");

        yield return _testHelper.AssertCurrentPageName ("PlaySelectionPage");

        yield return _testHelper.MainMenuTransition ("Button_PvPMode");

        yield return _testHelper.AssertCurrentPageName ("PvPSelectionPage");

        yield return _testHelper.MainMenuTransition ("Button_CasualType");

        yield return _testHelper.AssertCurrentPageName ("HordeSelectionPage");

        yield return new WaitForSeconds (5); */

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

        yield return _testHelper.AssertCurrentPageName ("HordeSelectionPage");

        yield return new WaitForSeconds (5);
    }
}
