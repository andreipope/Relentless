using NUnit.Framework;
using System.Collections;
using UnityEngine.TestTools;

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
        yield return _testHelper.TearDown ();

        yield return _testHelper.ReportTestTime ();
    }

    #endregion

    private IEnumerator SkipTutorial (bool twoSteps = true)
    {
        yield return _testHelper.ClickGenericButton ("Button_Skip");

        yield return _testHelper.RespondToYesNoOverlay (true);

        if (twoSteps)
        {
            yield return _testHelper.ClickGenericButton ("Button_Skip");

            yield return _testHelper.RespondToYesNoOverlay (true);
        }

        yield return null;
    }

    private IEnumerator PlayTutorial_Part1 ()
    {
        if (_testHelper.IsGameEnded ())
        {
            yield break;
        }

        yield return _testHelper.ClickGenericButton ("Button_Next", count: 3);

        yield return _testHelper.ClickGenericButton ("Button_Play");

        yield return _testHelper.ClickGenericButton ("Button_Next", count: 4);

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
        if (_testHelper.IsGameEnded ())
        {
            yield break;
        }

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

        yield return _testHelper.LetsThink ();

        _testHelper.TestEndHandler ();
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

        yield return SkipTutorial (false);

        #endregion

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