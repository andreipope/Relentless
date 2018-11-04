using NUnit.Framework;
using System.Collections;
using Loom.ZombieBattleground;
using Loom.ZombieBattleground.Common;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

public class TutorialTests
{
    private const string _testerKey = "f32ef9a2cfcb";

    private Scene _testScene;
    private GameObject _testerGameObject;
    private UnityEngine.EventSystems.VirtualInputModule _virtualInputModule;
    private RectTransform _fakeCursorTransform;

    private string _testName;
    private float _testStartTime;

    #region Setup & TearDown

    private IEnumerator TestSetup (string sceneToLoadFirst, string testName = "")
    {
        _testName = testName;
        _testStartTime = Time.unscaledTime;

        _testScene = SceneManager.GetActiveScene ();
        _testerGameObject = _testScene.GetRootGameObjects ()[0];
        _testerGameObject.AddComponent<TestScriptProtector> ();

        yield return SceneManager.LoadSceneAsync (sceneToLoadFirst, LoadSceneMode.Single);

        yield return null;
    }

    private IEnumerator TestTearDown ()
    {
        _testScene = SceneManager.CreateScene ("testScene");
        _testerGameObject.GetComponent<TestScriptProtector> ().enabled = false;
        SceneManager.MoveGameObjectToScene (_testerGameObject, _testScene);
        Scene currentScene = SceneManager.GetActiveScene ();

        SceneManager.SetActiveScene (_testScene);
        yield return SceneManager.UnloadSceneAsync (currentScene);

        Debug.LogFormat (
            "\"{0}\" test successfully finished in {1} seconds.",
            _testName,
            Time.unscaledTime - _testStartTime
        );
    }

    #endregion

    private IEnumerator AddVirtualInputModule ()
    {
        GameObject testSetup = GameObject.Instantiate (Resources.Load<GameObject> ("Prefabs/TestSetup"));
        _fakeCursorTransform = testSetup.transform.Find ("Canvas/FakeCursor").GetComponent<RectTransform> ();
        Camera uiCamera = testSetup.transform.Find ("UI Camera").GetComponent<Camera> ();

        UnityEngine.EventSystems.StandaloneInputModule inputModule = GameObject.FindObjectOfType<UnityEngine.EventSystems.StandaloneInputModule> ();
        _virtualInputModule = inputModule.gameObject.AddComponent<UnityEngine.EventSystems.VirtualInputModule> ();
        inputModule.enabled = false;
        _virtualInputModule.SetLinks (_fakeCursorTransform, uiCamera);

        yield return null;
    }

    private float _positionalTolerance = 5f;

    private IEnumerator MoveCursorToObject (string objectName, float duration)
    {
        GameObject targetObject = GameObject.Find (objectName);

        Vector2 from = _fakeCursorTransform.position;
        Vector2 to = targetObject.transform.position;

        Vector2 cursorPosition = from;
        float interpolation = 0f;
        while (Vector2.Distance (cursorPosition, to) >= _positionalTolerance)
        {
            cursorPosition = Vector2.Lerp (from, to, interpolation / duration);
            _fakeCursorTransform.position = cursorPosition;

            interpolation = Mathf.Min(interpolation + Time.unscaledTime, duration);

            yield return null;
        }
    }

    private IEnumerator FakeClick ()
    {
        _virtualInputModule.Press ();

        yield return null;

        _virtualInputModule.Release ();

        yield return null;
    }

    private IEnumerator HandleLogin ()
    {
        GameObject pressAnyText = null;
        yield return new WaitUntil (() => { pressAnyText = GameObject.Find ("PressAnyText"); return pressAnyText != null; });

        pressAnyText.SetActive (false);
        GameClient.Get<IUIManager> ().DrawPopup<LoginPopup> ();

        InputField testerKeyField = null;
        yield return new WaitUntil (() => { testerKeyField = GameObject.Find ("InputField_Beta")?.GetComponent<InputField> (); return testerKeyField != null; });

        testerKeyField.text = _testerKey;
        GameObject.Find ("Button_Beta").GetComponent<ButtonShiftingContent> ().onClick.Invoke ();

        yield return null;
    }

    private IEnumerator ClickGenericButton (string buttonName)
    {
        GameObject menuButtonGameObject = null;

        yield return new WaitUntil (() => {
            menuButtonGameObject = GameObject.Find (buttonName);

            if (menuButtonGameObject == null || !menuButtonGameObject.activeInHierarchy)
            {
                return false;
            }
            else if (menuButtonGameObject.GetComponent<ButtonShiftingContent> () != null)
            {
                menuButtonGameObject.GetComponent<ButtonShiftingContent> ().onClick.Invoke ();

                return true;
            }
            else if (menuButtonGameObject.GetComponent<Button> () != null)
            {
                menuButtonGameObject.GetComponent<Button> ().onClick.Invoke ();

                return true;
            }

            return false;
        });

        yield return null;
    }

    private IEnumerator MainMenuTransition (string transitionPath, float delay = 0.5f)
    {
        foreach (string buttonName in transitionPath.Split ('/'))
        {
            yield return ClickGenericButton (buttonName);

            yield return new WaitForSeconds (delay);
        }
    }

    private IEnumerator RespondToOverlay (bool isResponseYes)
    {
        string buttonName = isResponseYes ? "Button_Yes" : "Button_No";

        ButtonShiftingContent overlayButton = null;
        yield return new WaitUntil (() => { overlayButton = GameObject.Find (buttonName)?.GetComponent<ButtonShiftingContent> (); return overlayButton != null; });

        overlayButton.onClick.Invoke ();

        yield return null;
    }

    private IEnumerator SkipTutorial ()
    {
        ButtonShiftingContent skipTutorialButton = null;
        yield return new WaitUntil (() => { skipTutorialButton = GameObject.Find ("Button_Skip")?.GetComponent<ButtonShiftingContent> (); return skipTutorialButton != null; });

        skipTutorialButton.onClick.Invoke ();

        yield return null;

        yield return RespondToOverlay (true);

        skipTutorialButton = null;
        yield return new WaitUntil (() => { skipTutorialButton = GameObject.Find ("Button_Skip")?.GetComponent<ButtonShiftingContent> (); return skipTutorialButton != null; });

        skipTutorialButton.onClick.Invoke ();

        yield return RespondToOverlay (true);

        yield return null;
    }

    private string lastCheckedPageName;

    private IEnumerator AssertCurrentPageName (string expectedPageName)
    {
        GameObject canvas1GameObject = null;
        yield return new WaitUntil (() => {
            canvas1GameObject = GameObject.Find ("Canvas1");

            if (canvas1GameObject != null && canvas1GameObject.transform.childCount >= 2)
            {
                if (canvas1GameObject.transform.GetChild (1).name.Split ('(')[0] == lastCheckedPageName)
                {
                    return false;
                }

                return true;
            }

            return false;
        });
        string actualPageName = canvas1GameObject.transform.GetChild (1).name.Split ('(')[0];

        Assert.AreEqual (expectedPageName, actualPageName);

        lastCheckedPageName = actualPageName;
    }

    private IEnumerator WaitUntilPageUnloads ()
    {
        GameObject canvas1GameObject;
        yield return new WaitUntil (() => {
            canvas1GameObject = GameObject.Find ("Canvas1");

            if (canvas1GameObject != null && canvas1GameObject.transform.childCount <= 1)
            {
                return true;
            }

            return false;
        });
    }

    private IGameplayManager _gameplayManager;
    private BattlegroundController _battlegroundController;
    private SkillsController _skillsController;

    private void SetGameplayManagers ()
    {
        _gameplayManager = GameClient.Get<IGameplayManager> ();
        _battlegroundController = _gameplayManager.GetController<BattlegroundController> ();
        _skillsController = _gameplayManager.GetController<SkillsController> ();
    }

    private IEnumerator PlayCardFromHandToBoard (int[] cardIndices)
    {
        int playerBoardCardCount = _battlegroundController.PlayerBoardCards.Count;

        foreach (int index in cardIndices)
        {
            BoardCard cardToPlay = _battlegroundController.PlayerHandCards[index];

            HandBoardCard handBoardCard = cardToPlay.HandBoardCard;
            handBoardCard.Enabled = true;
            handBoardCard.OnSelected ();

            cardToPlay.Transform.position = Vector3.zero;

            handBoardCard.MouseUp (cardToPlay.GameObject);

            yield return new WaitUntil (() => _battlegroundController.PlayerBoardCards.Count > playerBoardCardCount);
            playerBoardCardCount = _battlegroundController.PlayerBoardCards.Count;

            BoardUnitModel newPlayedCardModel = _battlegroundController.PlayerBoardCards[playerBoardCardCount - 1].Model;
            if (newPlayedCardModel.HasFeral)
            {
                yield return new WaitUntil (() => newPlayedCardModel.IsPlayable);
            }

            yield return null;
        }
    }

    private IEnumerator PlayCardFromBoardToOpponentBoard (int[] attackingCardIndices, int[] attackedCardIndices)
    {
        Assert.AreEqual (attackingCardIndices.Length, attackedCardIndices.Length) ;

        for (int i = 0; i < attackedCardIndices.Length; i++)
        {
            int attackingCardIndex = attackingCardIndices[i];
            int attackedCardIndex = attackedCardIndices[i];

            BoardUnitView cardToPlayView = _battlegroundController.PlayerBoardCards[attackingCardIndex];
            BoardUnitView cardPlayedAgainstView = _battlegroundController.OpponentBoardCards[attackedCardIndex];

            cardToPlayView.SetSelectedUnit (true);

            BoardUnitModel cardToPlayModel = cardToPlayView.Model;
            BoardUnitModel cardPlayedAgainstModel = cardPlayedAgainstView.Model;

            cardToPlayModel.DoCombat (cardPlayedAgainstModel);

            yield return null;
        }
    }

    private IEnumerator PlayCardFromBoardToOpponentPlayer (int[] attackingCardIndices)
    {
        for (int i = 0; i < attackingCardIndices.Length; i++)
        {
            int attackingCardIndex = attackingCardIndices[i];

            BoardUnitView cardToPlayView = _battlegroundController.PlayerBoardCards[attackingCardIndex];
            BoardUnitModel cardToPlayModel = cardToPlayView.Model;

            Player opponentPlayer = _gameplayManager.OpponentPlayer;

            cardToPlayModel.DoCombat (opponentPlayer);

            yield return null;
        }
    }

    private IEnumerator PlayNonSleepingCardsFromBoardToOpponentPlayer ()
    {
        for (int i = 0; i < _battlegroundController.PlayerBoardCards.Count; i++)
        {
            BoardUnitView cardToPlayView = _battlegroundController.PlayerBoardCards[i];
            BoardUnitModel cardToPlayModel = cardToPlayView.Model;

            Player opponentPlayer = _gameplayManager.OpponentPlayer;

            if (cardToPlayModel.IsPlayable)
            {
                cardToPlayModel.DoCombat (opponentPlayer);

                yield return null;
            }
        }
    }

    private IEnumerator UseSkillToOpponentPlayer (bool isPrimary, int targetOpponentCard = -1)
    {
        BoardSkill boardSkill = isPrimary ? _skillsController.PlayerPrimarySkill : _skillsController.PlayerSecondarySkill;

        if (boardSkill.IsSkillReady)
        {
            BoardUnitView targetView;
            BoardObject target;

            boardSkill.StartDoSkill ();

            if (targetOpponentCard != -1 && _battlegroundController.OpponentBoardCards.Count >= 1)
            {
                target = _battlegroundController.OpponentBoardCards[0].Model;
                targetView = _battlegroundController.OpponentBoardCards[0];

                boardSkill.FightTargetingArrow.SelectedCard = targetView;
            }
            else
            {
                target = _gameplayManager.OpponentPlayer;

                boardSkill.FightTargetingArrow.SelectedPlayer = _gameplayManager.OpponentPlayer;
            }

            _skillsController.DoSkillAction (boardSkill, target);
            boardSkill.EndDoSkill ();

            BoardArrowController boardArrowController = _gameplayManager.GetController<BoardArrowController> ();

            yield return new WaitForSeconds (2);
        }

        yield return null;
    }

    private IEnumerator EndTurn ()
    {
        _battlegroundController.StopTurn ();
        GameObject.Find ("_1_btn_endturn").GetComponent<EndTurnButton> ().SetEnabled (false);

        yield return null;
    }

    private IEnumerator WaitUntilCardIsAddedToBoard (string boardName)
    {
        Transform boardTransform = GameObject.Find (boardName).transform;
        int boardChildrenCount = boardTransform.childCount;

        yield return new WaitUntil (() => (boardChildrenCount < boardTransform.childCount) && (boardChildrenCount < _battlegroundController.OpponentBoardCards.Count));
    }

    private IEnumerator WaitUntilInputIsUnblocked ()
    {
        yield return new WaitUntil (() => _gameplayManager.IsLocalPlayerTurn () || GameEnded ());
    }

    private IEnumerator WaitUntilAIBrainStops ()
    {
        yield return new WaitUntil (() => _gameplayManager.GetController<AIController> ().IsBrainWorking == false);
    }

    private IEnumerator WaitUntilOurTurnStarts ()
    {
        yield return new WaitUntil (() => GameObject.Find ("YourTurnPopup(Clone)") != null || GameEnded ());

        yield return new WaitUntil (() => GameObject.Find ("YourTurnPopup(Clone)") == null || GameEnded ());
    }

    private IEnumerator PlayTutorial_Part1 ()
    {
        SetGameplayManagers ();

        for (int i = 0; i < 3; i++)
        {
            yield return ClickGenericButton ("Button_Next");
        }

        yield return ClickGenericButton ("Button_Play");

        for (int i = 0; i < 4; i++)
        {
            yield return ClickGenericButton ("Button_Next");
        }

        yield return PlayCardFromHandToBoard (new[] { 0 });

        yield return ClickGenericButton ("Button_Next");

        /* yield return MoveCursorToObject ("_1_btn_endturn", 3f);

        yield return FakeClick (); */

        yield return EndTurn ();

        yield return WaitUntilCardIsAddedToBoard ("OpponentBoard");
        yield return WaitUntilAIBrainStops ();

        yield return ClickGenericButton ("Button_Next");

        yield return WaitUntilInputIsUnblocked ();

        yield return new WaitForSeconds (4);

        yield return PlayCardFromBoardToOpponentBoard (new[] { 0 }, new[] { 0 });

        for (int i = 0; i < 2; i++)
        {
            yield return ClickGenericButton ("Button_Next");
        }

        yield return EndTurn ();

        yield return WaitUntilOurTurnStarts ();
        yield return WaitUntilInputIsUnblocked ();

        yield return ClickGenericButton ("Button_Next");

        yield return PlayCardFromBoardToOpponentPlayer (new[] { 0 });

        yield return ClickGenericButton ("Button_Next");

        yield return EndTurn ();

        yield return WaitUntilOurTurnStarts ();
        yield return WaitUntilInputIsUnblocked ();

        yield return ClickGenericButton ("Button_Next");

        yield return PlayCardFromBoardToOpponentBoard (new[] { 0 }, new[] { 0 });

        for (int i = 0; i < 3; i++)
        {
            yield return ClickGenericButton ("Button_Next");
        }

        // yield return WaitUntilOurTurnStarts ();

        yield return WaitUntilAIBrainStops ();
        yield return WaitUntilInputIsUnblocked ();

        yield return new WaitForSeconds (4); // we should wait for any card that's damaged to disappear before we add anything, because otherwise it gets complicated to understand if anything has been added (one card is being added, while another one is being removed in some cases)

        yield return PlayCardFromHandToBoard (new[] { 1 });

        // yield return new WaitForSeconds (4);

        yield return PlayCardFromBoardToOpponentPlayer (new[] { 0 });

        for (int i = 0; i < 3; i++)
        {
            yield return ClickGenericButton ("Button_Next");
        }

        yield return UseSkillToOpponentPlayer (true);

        for (int i = 0; i < 4; i++)
        {
            yield return ClickGenericButton ("Button_Next");
        }

        yield return ClickGenericButton ("Button_Continue");

        yield return null;
    }

    private IEnumerator PlayTutorial_Part2 ()
    {
        yield return ClickGenericButton ("Button_Next");

        yield return WaitUntilOurTurnStarts ();
        yield return WaitUntilInputIsUnblocked ();

        for (int i = 0; i < 11; i++)
        {
            yield return ClickGenericButton ("Button_Next");
        }

        yield return PlayCardFromHandToBoard (new[] { 1 });

        yield return ClickGenericButton ("Button_Next");

        yield return new WaitForSeconds (2);

        yield return PlayCardFromHandToBoard (new[] { 0 });

        for (int i = 0; i < 11; i++)
        {
            yield return ClickGenericButton ("Button_Next");
        }

        yield return PlayNonSleepingCardsFromBoardToOpponentPlayer ();

        for (int i = 0; i < 5; i++)
        {
            yield return ClickGenericButton ("Button_Next");
        }

        yield return ClickGenericButton ("Button_Continue");

        yield return null;
    }

    private IEnumerator WaitUntilPlayerOrderIsDecided ()
    {
        yield return new WaitUntil (() => GameObject.Find ("PlayerOrderPopup(Clone)") != null);

        yield return new WaitUntil (() => GameObject.Find ("PlayerOrderPopup(Clone)") == null);
    }

    private IEnumerator DecideWhichCardsToPick ()
    {
        CardsController cardsController = _gameplayManager.GetController<CardsController> ();

        int highCardCounter = 0;

        Player currentPlayer = _gameplayManager.CurrentPlayer;
        for (int i = currentPlayer.CardsPreparingToHand.Count - 1; i >= 0; i--)
        {
            BoardCard boardCard = currentPlayer.CardsPreparingToHand[i];

            if ((boardCard.LibraryCard.CardKind == Enumerators.CardKind.SPELL) ||
                (highCardCounter >= 1 && boardCard.LibraryCard.Cost >= 4) ||
                boardCard.LibraryCard.Cost >= 8)
            {
                currentPlayer.CardsPreparingToHand[i].CardShouldBeChanged = !boardCard.CardShouldBeChanged;

                yield return new WaitForSeconds (2);
            }
            else if (boardCard.LibraryCard.Cost >= 4)
            {
                highCardCounter++;
            }
        }

        yield return null;
    }

    private IEnumerator IdentifyWhoseTurnItIsAndProceed ()
    {
        if (_gameplayManager.CurrentTurnPlayer.Id == _gameplayManager.CurrentPlayer.Id)
        {
            yield return MakeADumbMove ();

            yield return EndTurn ();
        }
        else
        {
            yield return WaitUntilOurTurnStarts ();
            yield return WaitUntilInputIsUnblocked ();

            yield return MakeADumbMove ();

            yield return EndTurn ();
        }

        yield return null;
    }

    private IEnumerator ConsiderDrawingOneCardFromHand ()
    {
        // Go through cards in the hand and pick highest card player can play
        int availableGoo = _gameplayManager.CurrentPlayer.CurrentGoo;

        int maxCost = 0;
        int maxIndex = -1;

        for (int i = _battlegroundController.PlayerHandCards.Count - 1; i >= 0; i--)
        {
            BoardCard boardCard = _battlegroundController.PlayerHandCards[i];

            if (boardCard.WorkingCard.LibraryCard.CardKind == Enumerators.CardKind.SPELL)
            {
                continue;
            }

            int cost = boardCard.ManaCost;

            if (cost <= availableGoo && cost > maxCost)
            {
                maxCost = cost;
                maxIndex = i;
            }
        }

        if (maxIndex != -1)
        {
            yield return PlayCardFromHandToBoard (new[] { maxIndex });
        }

        yield return new WaitForSeconds (2); // might be required for Arrival Animation
    }

    private IEnumerator ConsiderPlayingOneCardFromBoard ()
    {
        if (_battlegroundController.PlayerBoardCards.Count >= 1)
        {
            BoardUnitView attackingCard = null;
            for (int i = _battlegroundController.PlayerBoardCards.Count - 1; i >= 0; i--)
            {
                if (_battlegroundController.PlayerBoardCards[i].Model.Card.Damage == 0)
                    continue;

                if (_battlegroundController.PlayerBoardCards[i].Model.IsPlayable)
                {
                    attackingCard = _battlegroundController.PlayerBoardCards[i];

                    break;
                }
            }

            if (attackingCard != null)
            {
                int attackedIndex = -1;
                for (int i = 0; i < _battlegroundController.OpponentBoardCards.Count; i++)
                {
                    BoardUnitView attackedCard = _battlegroundController.OpponentBoardCards[i];

                    if (attackedCard.Model.Card.Health >= 1)
                    {
                        attackedIndex = i;

                        break;
                    }
                }

                if (attackedIndex != -1)
                {
                    BoardUnitModel attackedCard = _battlegroundController.OpponentBoardCards[0].Model;

                    attackingCard.Model.DoCombat (attackedCard);

                    cardAttackCounter++;

                    yield return new WaitForSeconds (2);
                }
                else
                {
                    Player attackedPlayer = _gameplayManager.OpponentPlayer;

                    attackingCard.Model.DoCombat (attackedPlayer);

                    cardAttackCounter++;

                    yield return new WaitForSeconds (2);
                }
            }
        }

        yield return null;
    }

    private IEnumerator ConsiderUsingPrimarySkill ()
    {
        // Poison Dart: Deal 1 damage to a unit/player

        int indexWithDamage = -1;

        if (_battlegroundController.OpponentBoardCards.Count >= 1)
        {
            for (int i = 0; i < _battlegroundController.OpponentBoardCards.Count; i++)
            {
                if (_battlegroundController.OpponentBoardCards[i].Model.Card.Damage >= 1)
                {
                    indexWithDamage = i;

                    break;
                }
            }
        }

        if (indexWithDamage != -1)
        {
            yield return UseSkillToOpponentPlayer (true, indexWithDamage);
        }
        else
        {
            yield return UseSkillToOpponentPlayer (true);
        }

        yield return null;
    }

    int cardAttackCounter;

    private IEnumerator MakeADumbMove ()
    {
        int cardsInHand;

        yield return ConsiderUsingPrimarySkill ();

        do
        {
            cardsInHand = _battlegroundController.PlayerBoardCards.Count;

            yield return ConsiderDrawingOneCardFromHand ();
        }
        while (_battlegroundController.PlayerBoardCards.Count > cardsInHand);

        cardAttackCounter = 0;
        int oldCardAttackCounter = 0;
        do
        {
            oldCardAttackCounter = cardAttackCounter;

            yield return ConsiderPlayingOneCardFromBoard ();
        }
        while (cardAttackCounter > oldCardAttackCounter);

        yield return null;
    }

    private bool GameEnded ()
    {
        if (_gameplayManager == null || _gameplayManager.IsGameEnded)
        {
            return true;
        }
        else if (_gameplayManager.CurrentPlayer == null || _gameplayManager.OpponentPlayer == null)
        {
            return true;
        }

        int playerHP = _gameplayManager.CurrentPlayer.Defense;
        int opponentHP = _gameplayManager.OpponentPlayer.Defense;

        if (playerHP <= 0 || opponentHP <= 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private IEnumerator SoloGameplay ()
    {
        SetGameplayManagers ();

        yield return WaitUntilPlayerOrderIsDecided ();

        yield return DecideWhichCardsToPick ();

        yield return ClickGenericButton ("Button_Keep");

        yield return IdentifyWhoseTurnItIsAndProceed ();

        // if it doesn't end in 100 moves, end the game anyway
        for (int turns = 1; turns <= 100; turns++)
        {
            if (GameEnded ())
                break;

            yield return WaitUntilOurTurnStarts ();

            if (GameEnded ())
                break;

            yield return WaitUntilInputIsUnblocked ();

            if (GameEnded ())
                break;

            yield return MakeADumbMove ();

            if (GameEnded ())
                break;

            yield return EndTurn ();

            if (GameEnded ())
                break;
        }

        yield return ClickGenericButton ("Button_Continue");

        yield return null;
    }

    [UnityTest]
    [Timeout (500000)]
    public IEnumerator Test_TutorialSkip ()
    {
        yield return TestSetup ("APP_INIT", "Tutorial - Skip");

        yield return AddVirtualInputModule ();

        #region Login

        yield return AssertCurrentPageName ("LoadingPage");

        yield return HandleLogin ();

        yield return AssertCurrentPageName ("MainMenuPage");

        #endregion

        #region Tutorial Skip

        yield return MainMenuTransition ("Button_Tutorial");

        yield return AssertCurrentPageName ("GameplayPage");

        yield return SkipTutorial ();

        yield return AssertCurrentPageName ("HordeSelectionPage");

        #endregion

        yield return TestTearDown ();
    }

    [UnityTest]
    [Timeout (500000)]
    public IEnumerator Test_TutorialNonSkip ()
    {
        yield return TestSetup ("APP_INIT", "Tutorial - Non-Skip");

        yield return AddVirtualInputModule ();

        #region Login

        yield return AssertCurrentPageName ("LoadingPage");

        yield return HandleLogin ();

        yield return AssertCurrentPageName ("MainMenuPage");

        # endregion

        #region Tutorial Non-Skip

        yield return MainMenuTransition ("Button_Tutorial");

        yield return AssertCurrentPageName ("GameplayPage");

        yield return PlayTutorial_Part1 ();

        yield return PlayTutorial_Part2 ();

        yield return AssertCurrentPageName ("HordeSelectionPage");

        #endregion

        yield return TestTearDown ();
    }

    [UnityTest]
    [Timeout (500000)]
    public IEnumerator Test_SoloGameplay ()
    {
        yield return TestSetup ("APP_INIT", "Tutorial - Solo Gameplay");

        yield return AddVirtualInputModule ();

        #region Login

        yield return AssertCurrentPageName ("LoadingPage");

        yield return HandleLogin ();

        yield return AssertCurrentPageName ("MainMenuPage");

        #endregion

        #region Solo Gameplay

        yield return MainMenuTransition ("Button_Play");

        yield return AssertCurrentPageName ("PlaySelectionPage");

        yield return MainMenuTransition ("Button_SoloMode");

        yield return AssertCurrentPageName ("HordeSelectionPage");

        yield return MainMenuTransition ("Button_Battle");

        yield return AssertCurrentPageName ("GameplayPage");

        yield return SoloGameplay ();

        yield return AssertCurrentPageName ("HordeSelectionPage");

        #endregion

        yield return new WaitForSeconds (5);

        yield return TestTearDown ();
    }
}
