using System;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.Editor.Tools;
using Loom.ZombieBattleground.Test;
using UnityEngine;
using UnityEngine.TestTools;

namespace Loom.ZombieBattleground.Test
{

}

public class MultiplayerTests
{
    private TestHelper _testHelper = new TestHelper (0);

    #region Setup & TearDown

    [UnitySetUp]
    public IEnumerator PerTestSetup ()
    {
        yield return _testHelper.PerTestSetup ();
    }

    [UnityTearDown]
    public IEnumerator PerTestTearDown ()
    {
        _testHelper.DebugCheatsConfiguration = new DebugCheatsConfiguration ();

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
    [Timeout(150 * 1000 * TestHelper.TestTimeScale)]
    public IEnumerator Test_A0_PlayDefinedGame()
    {
        _testHelper.SetTestName("PvP - Defined Game");
        yield return _testHelper.MainMenuTransition("Button_Play");
        yield return _testHelper.AssertIfWentDirectlyToTutorial (
            _testHelper.GoBackToMainAndPressPlay());

        yield return _testHelper.AssertCurrentPageName("PlaySelectionPage");
        yield return _testHelper.MainMenuTransition("Button_PvPMode");
        yield return _testHelper.AssertCurrentPageName("PvPSelectionPage");
        yield return _testHelper.MainMenuTransition("Button_CasualType");
        yield return _testHelper.AssertCurrentPageName("HordeSelectionPage");

        int selectedHordeIndex = 0;

        yield return _testHelper.SelectAHordeByIndex(selectedHordeIndex);
        _testHelper.RecordExpectedOverlordName(selectedHordeIndex);
        _testHelper.SetPvPTags(new[] {
            "pvpTest",
            "scenario1"
        });
        _testHelper.DebugCheatsConfiguration.Enabled = true;
        _testHelper.DebugCheatsConfiguration.CustomRandomSeed = 0;

        yield return _testHelper.LetsThink();

        yield return _testHelper.MainMenuTransition("Button_Battle");

        yield return TestHelper.TaskAsIEnumerator(_testHelper.CreateAndConnectOpponentDebugClient());

        IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns = new Action<QueueProxyPlayerActionTestProxy>[]
        {
            opponent => {},
            player => {},
            opponent => {},
            player => {},
            opponent => {},
            player => {},
            opponent => {},
            player => player.CardPlay(new InstanceId(38), 0),
            opponent => {},
            player => player.CardAttack(new InstanceId(38), Enumerators.AffectObjectType.Player, _testHelper.GetOpponentPlayer().InstanceId),
            opponent => {},
            player => player.CardAttack(new InstanceId(38), Enumerators.AffectObjectType.Player, _testHelper.GetOpponentPlayer().InstanceId),
            opponent => {},
            player => player.CardAttack(new InstanceId(38), Enumerators.AffectObjectType.Player, _testHelper.GetOpponentPlayer().InstanceId),
            opponent => {},
            player => player.CardAttack(new InstanceId(38), Enumerators.AffectObjectType.Player, _testHelper.GetOpponentPlayer().InstanceId),
            opponent => {},
            player => player.CardAttack(new InstanceId(38), Enumerators.AffectObjectType.Player, _testHelper.GetOpponentPlayer().InstanceId),
            opponent => {},
            player => player.CardAttack(new InstanceId(38), Enumerators.AffectObjectType.Player, _testHelper.GetOpponentPlayer().InstanceId),
            opponent => {},
            player => player.CardAttack(new InstanceId(38), Enumerators.AffectObjectType.Player, _testHelper.GetOpponentPlayer().InstanceId),
            opponent => {},
            player => player.CardAttack(new InstanceId(38), Enumerators.AffectObjectType.Player, _testHelper.GetOpponentPlayer().InstanceId),
            opponent => {},
            player => player.CardAttack(new InstanceId(38), Enumerators.AffectObjectType.Player, _testHelper.GetOpponentPlayer().InstanceId),
            opponent => {},
            player => player.CardAttack(new InstanceId(38), Enumerators.AffectObjectType.Player, _testHelper.GetOpponentPlayer().InstanceId),
        };

        ScenarioPlayer scenarioPlayer = new ScenarioPlayer(_testHelper, turns);
        yield return TestHelper.TaskAsIEnumerator(_testHelper.MatchmakeOpponentDebugClient());

        yield return scenarioPlayer.Play();
    }

    [UnityTest]
    [Timeout (50 * 1000 * TestHelper.TestTimeScale)]
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
    }

    [UnityTest]
    [Timeout (50 * 1000 * TestHelper.TestTimeScale)]
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
    }

    [UnityTest]
    [Timeout (50 * 1000 * TestHelper.TestTimeScale)]
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

        yield return TestHelper.TaskAsIEnumerator(_testHelper.CreateAndConnectOpponentDebugClient());
        yield return TestHelper.TaskAsIEnumerator(_testHelper.MatchmakeOpponentDebugClient());

        yield return _testHelper.AssertCurrentPageName ("GameplayPage");
        yield return _testHelper.WaitUntilPlayerOrderIsDecided ();
        _testHelper.AssertOverlordName ();

        yield return _testHelper.ClickGenericButton ("Button_Settings");
        yield return _testHelper.ClickGenericButton ("Button_QuitToMainMenu");
        yield return _testHelper.RespondToYesNoOverlay (true);

        yield return _testHelper.LetsThink ();
    }

    [UnityTest]
    [Timeout (50 * 1000 * TestHelper.TestTimeScale)]
    public IEnumerator Test_A4_MatchmakeMakeOneMoveAndQuit ()
    {
        _testHelper.SetTestName ("PvP - Matchmake, Make One Move And Play");
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

        yield return TestHelper.TaskAsIEnumerator(_testHelper.CreateAndConnectOpponentDebugClient());
        _testHelper.SetupOpponentDebugClientToEndTurns();
        yield return TestHelper.TaskAsIEnumerator(_testHelper.MatchmakeOpponentDebugClient());

        yield return _testHelper.PlayAMatch (1);
        yield return _testHelper.ClickGenericButton ("Button_Settings");
        yield return _testHelper.ClickGenericButton ("Button_QuitToMainMenu");
        yield return _testHelper.RespondToYesNoOverlay (true);
    }

    [UnityTest]
    [Timeout (300 * 1000 * TestHelper.TestTimeScale)]
    public IEnumerator Test_A4_MatchmakeAndPlay ()
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

        yield return TestHelper.TaskAsIEnumerator(_testHelper.CreateAndConnectOpponentDebugClient());
        _testHelper.SetupOpponentDebugClientToEndTurns();
        yield return TestHelper.TaskAsIEnumerator(_testHelper.MatchmakeOpponentDebugClient());

        yield return _testHelper.PlayAMatch ();
    }

    [UnityTest]
    [Timeout (50 * 1000 * TestHelper.TestTimeScale)]
    public IEnumerator Test_A5_MatchmakingCancelAndMatchmake ()
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
        yield return _testHelper.PerTestSetup ();
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
    [Timeout (180 * 1000 * TestHelper.TestTimeScale)]
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
