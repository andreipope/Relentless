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

    private IEnumerator SoloGameplay ()
    {
        _testHelper.InitalizePlayer ();

        yield return _testHelper.WaitUntilPlayerOrderIsDecided ();

        yield return _testHelper.DecideWhichCardsToPick ();

        yield return _testHelper.WaitUntilOurFirstTurn ();

        yield return _testHelper.MakeMoves ();

        yield return null;
    }

    [UnityTest]
    [Timeout (500000)]
    public IEnumerator Test1_TutorialNonSkip ()
    {
        _testHelper.SetTestName ("Tutorial - Non-Skip");

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
    public IEnumerator Test2_TutorialSkip ()
    {
        _testHelper.SetTestName ("Tutorial - Skip");

        #region Tutorial Skip

        yield return _testHelper.MainMenuTransition ("Button_Tutorial");

        yield return _testHelper.AssertCurrentPageName ("GameplayPage");

        yield return SkipTutorial ();

        yield return _testHelper.AssertCurrentPageName ("HordeSelectionPage");

        #endregion
    }

    [UnityTest]
    [Timeout (500000)]
    public IEnumerator Test3_SoloGameplay ()
    {
        _testHelper.SetTestName ("Solo Gameplay");

        #region Solo Gameplay

        yield return _testHelper.MainMenuTransition ("Button_Play");

        yield return _testHelper.AssertCurrentPageName ("PlaySelectionPage");

        yield return _testHelper.MainMenuTransition ("Button_SoloMode");

        yield return _testHelper.AssertCurrentPageName ("HordeSelectionPage");

        yield return _testHelper.MainMenuTransition ("Button_Battle");

        yield return _testHelper.AssertCurrentPageName ("GameplayPage");

        yield return SoloGameplay ();

        yield return _testHelper.ClickGenericButton ("Button_Continue");

        yield return _testHelper.AssertCurrentPageName ("HordeSelectionPage");

        #endregion
    }

    [UnityTest]
    [Timeout (500000)]
    public IEnumerator Test4_CreateAHordeAndCancel ()
    {
        _testHelper.SetTestName ("Create a Horde and cancel");

        yield return _testHelper.MainMenuTransition ("Button_Play");

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

        // >> HordeSelectionPage
        // Tap on Image_BackgroundGeneral

        // Tap on Button_LeftArrow or Button_RightArrow
        // Check children of Panel_OverlordContent/Group/
        // Active one has (Selected) at the end
        // Tap on Button_Continue

        // Check out abilities under Abilities/
        // Children are Toggles
        // Tap on Button_Cancel (takes back to overlord selection) or Button_Continue

        // >> HordeEditingPage

        yield return null;
    }

    [UnityTest]
    [Timeout (500000)]
    public IEnumerator Test5_CreateAHordeAndSave ()
    {
        _testHelper.SetTestName ("Create a Horde and save");

        yield return _testHelper.MainMenuTransition ("Button_Play");

        yield return _testHelper.AssertCurrentPageName ("PlaySelectionPage");

        yield return _testHelper.MainMenuTransition ("Button_SoloMode");

        yield return _testHelper.AssertCurrentPageName ("HordeSelectionPage");

        yield return _testHelper.ClickGenericButton ("Image_BaackgroundGeneral");

        yield return _testHelper.AssertCurrentPageName ("OverlordSelectionPage");

        yield return _testHelper.PickOverlord ("Razu", true);

        yield return _testHelper.PickOverlordAbility (1);

        yield return _testHelper.ClickGenericButton ("Canvas_BackLayer/Button_Continue");

        yield return _testHelper.AssertCurrentPageName ("HordeEditingPage");

        _testHelper.SetupArmyCards ();

        yield return _testHelper.SetDeckTitle ("Razu Deck");

        yield return _testHelper.AddCardToHorde ("Pyromaz");
        yield return _testHelper.AddCardToHorde ("Pyromaz");
        yield return _testHelper.AddCardToHorde ("Pyromaz");
        yield return _testHelper.AddCardToHorde ("Pyromaz");
        // yield return _testHelper.AddCardToHorde ("Pyromaz", true);

        yield return _testHelper.AddCardToHorde ("Quazi");
        yield return _testHelper.AddCardToHorde ("Quazi");
        yield return _testHelper.AddCardToHorde ("Quazi");
        yield return _testHelper.AddCardToHorde ("Quazi");

        yield return _testHelper.AddCardToHorde ("Ember");
        yield return _testHelper.AddCardToHorde ("Ember");
        yield return _testHelper.AddCardToHorde ("Ember");
        yield return _testHelper.AddCardToHorde ("Ember");

        yield return _testHelper.AddCardToHorde ("Firewall");
        yield return _testHelper.AddCardToHorde ("Firewall");
        yield return _testHelper.AddCardToHorde ("Firewall");
        yield return _testHelper.AddCardToHorde ("Firewall");

        yield return _testHelper.AddCardToHorde ("BurZt");
        yield return _testHelper.AddCardToHorde ("BurZt");
        yield return _testHelper.AddCardToHorde ("BurZt");
        yield return _testHelper.AddCardToHorde ("BurZt");

        yield return _testHelper.ClickGenericButton ("Army/ArrowRightButton");

        yield return _testHelper.LetsThink ();

        _testHelper.SetupArmyCards ();

        yield return _testHelper.LetsThink ();

        yield return _testHelper.AddCardToHorde ("BlaZter");

        yield return _testHelper.ClickGenericButton ("Horde/ArrowRightButton");

        yield return _testHelper.AddCardToHorde ("BlaZter");
        yield return _testHelper.AddCardToHorde ("BlaZter");
        yield return _testHelper.AddCardToHorde ("BlaZter");

        yield return _testHelper.AddCardToHorde ("Burrrnn");
        yield return _testHelper.AddCardToHorde ("Burrrnn");

        yield return _testHelper.AddCardToHorde ("Cynderman");
        yield return _testHelper.AddCardToHorde ("Cynderman");

        yield return _testHelper.AddCardToHorde ("Werezomb");
        yield return _testHelper.AddCardToHorde ("Werezomb");

        yield return _testHelper.ClickGenericButton ("Button_Save");

        yield return _testHelper.AssertCurrentPageName ("HordeSelectionPage");

        yield return null;
    }

    [UnityTest]
    [Timeout (500000)]
    public IEnumerator Test6_RemoveSecondHorde ()
    {
        _testHelper.SetTestName ("Create a Horde and save");

        yield return _testHelper.MainMenuTransition ("Button_Play");

        yield return _testHelper.AssertCurrentPageName ("PlaySelectionPage");

        yield return _testHelper.MainMenuTransition ("Button_SoloMode");

        yield return _testHelper.AssertCurrentPageName ("HordeSelectionPage");

        yield return _testHelper.RemoveAHorde (1);

        yield return _testHelper.RespondToYesNoOverlay (true);

        yield return _testHelper.LetsThink ();

        yield return null;
    }

    [UnityTest]
    public IEnumerator TestN_Cleanup ()
    {
        // Nothing, just to ascertain cleanup

        yield return null;
    }
}
