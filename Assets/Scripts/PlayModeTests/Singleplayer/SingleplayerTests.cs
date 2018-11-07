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

public class SingleplayerTests
{
    private const string _testerKey = "f32ef9a2cfcb";

    private bool _initialized = false;

    private Scene _testScene;
    private GameObject _testerGameObject;
    private UnityEngine.EventSystems.VirtualInputModule _virtualInputModule;
    private RectTransform _fakeCursorTransform;

    private string _testName;
    private float _testStartTime;

    private TestBroker _testBroker;
    private Enumerators.MatchPlayer _player;
    private Enumerators.MatchPlayer _opponent;

    #region Setup & TearDown

    [UnitySetUp]
    public IEnumerator PerTestSetup ()
    {
        if (!_initialized)
        {
            _testScene = SceneManager.GetActiveScene ();
            _testerGameObject = _testScene.GetRootGameObjects ()[0];
            _testerGameObject.AddComponent<TestScriptProtector> ();

            yield return SceneManager.LoadSceneAsync ("APP_INIT", LoadSceneMode.Single);

            // RemoveGoogleAnalyticsModule ();

            yield return AddVirtualInputModule ();

            #region Login

            yield return AssertCurrentPageName ("LoadingPage");

            yield return HandleLogin ();

            yield return AssertCurrentPageName ("MainMenuPage");

            #endregion

            SetGameplayManagers ();

            _initialized = true;
        }

        _testStartTime = Time.unscaledTime;

        yield return null;
    }

    [UnityTearDown]
    public IEnumerator PerTestTearDown ()
    {
        if (TestContext.CurrentContext.Test.Name == "TestN_Cleanup")
        {
            Scene dontDestroyOnLoadScene = _testerGameObject.scene;

            _testScene = SceneManager.CreateScene ("testScene");
            GameObject.Destroy (_testerGameObject.GetComponent<TestScriptProtector> ());
            SceneManager.MoveGameObjectToScene (_testerGameObject, _testScene);
            Scene currentScene = SceneManager.GetActiveScene ();

            foreach (GameObject rootGameObject in currentScene.GetRootGameObjects ())
            {
                GameObject.Destroy (rootGameObject);
            }
            foreach (GameObject rootGameObject in dontDestroyOnLoadScene.GetRootGameObjects ())
            {
                GameObject.Destroy (rootGameObject);
            }

            yield return new WaitForSeconds (1);

            SceneManager.SetActiveScene (_testScene);
            yield return SceneManager.UnloadSceneAsync (currentScene);
        }
        else
        {
            yield return MainMenuTransition ("Button_Back");

            yield return AssertCurrentPageName ("PlaySelectionPage");

            yield return MainMenuTransition ("Button_Back");

            yield return AssertCurrentPageName ("MainMenuPage");
        }

        Debug.LogFormat (
            "\"{0}\" test successfully finished in {1} seconds.",
            _testName,
            Time.unscaledTime - _testStartTime
        );

        yield return new WaitForSeconds (2);
    }

    #endregion

    private void SetTestName (string testName = "")
    {
        _testName = testName;
    }

    // Google Analytics isn't required for testing and in case of multiple tests it starts being overloaded and providing error on number of requests.
    private void RemoveGoogleAnalyticsModule ()
    {
        GameObject googleAnalyticsGameObject = GameObject.Find ("GAv4");

        if (googleAnalyticsGameObject != null)
        {
            GameObject.Destroy (googleAnalyticsGameObject);
        }
    }

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

    private IEnumerator MainMenuTransition (string transitionPath, float delay = 2f)
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

        yield return null;
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
    private IUIManager _uiManager;
    private IDataManager _dataManager;

    private BattlegroundController _battlegroundController;
    private SkillsController _skillsController;
    private CardsController _cardsController;
    private ActionsQueueController _actionsQueueController;
    private AbilitiesController _abilitiesController;
    private BoardArrowController _boardArrowController;

    private Player _currentPlayer, _opponentPlayer;

    private void SetGameplayManagers ()
    {
        _testBroker = new TestBroker ();
        _player = Enumerators.MatchPlayer.CurrentPlayer;
        _opponent = Enumerators.MatchPlayer.OpponentPlayer;

        _gameplayManager = GameClient.Get<IGameplayManager> ();
        _uiManager = GameClient.Get<IUIManager> ();
        _dataManager = GameClient.Get<IDataManager> ();

        _battlegroundController = _gameplayManager.GetController<BattlegroundController> ();
        _skillsController = _gameplayManager.GetController<SkillsController> ();
        _cardsController = _gameplayManager.GetController<CardsController> ();
        _actionsQueueController = _gameplayManager.GetController<ActionsQueueController> ();
        _abilitiesController = _gameplayManager.GetController<AbilitiesController> ();
        _boardArrowController = _gameplayManager.GetController<BoardArrowController> ();

        _currentPlayer = _gameplayManager.CurrentPlayer;
        _opponentPlayer = _gameplayManager.OpponentPlayer;
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
        Assert.AreEqual (attackingCardIndices.Length, attackedCardIndices.Length);

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

            _boardArrowController.DoAutoTargetingArrowFromTo<OpponentBoardArrow> (boardSkill.SelfObject.transform, target);

            _skillsController.DoSkillAction (boardSkill, target);
            boardSkill.EndDoSkill ();

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

    private IEnumerator MakeAnAIMove ()
    {
        yield return LetsThink ();

        yield return PlayCardsFromHand ();

        yield return LetsThink ();
        yield return LetsThink ();
        yield return LetsThink ();

        yield return UseUnitsOnBoard ();

        yield return UsePlayerSkills ();

        if (_testBroker.GetPlayer (_player).SelfHero.HeroElement == Enumerators.SetType.FIRE)
        {
            yield return UseUnitsOnBoard ();

            yield return LetsThink ();
            yield return LetsThink ();
        }
        else
        {
            yield return LetsThink ();
            yield return LetsThink ();
        }

        yield return null;
    }

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

    private IEnumerator SoloGameplay (bool useAIMoves = false)
    {
        if (useAIMoves)
        {
            InitalizePlayer ();
        }

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

            if (useAIMoves)
            {
                yield return TurnStartedHandler ();

                TurnEndedHandler ();
            }
            else
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

    #region Adapted from AIController

    private const int MinTurnForAttack = 0;
    public BoardCard CurrentSpellCard;

    private readonly System.Random _random = new System.Random ();

    private List<BoardUnitModel> _attackedUnitTargets;
    private List<BoardUnitModel> _unitsToIgnoreThisTurn;

    private List<WorkingCard> _normalUnitCardInHand, _normalSpellCardInHand;

    private void InitalizePlayer ()
    {
        _attackedUnitTargets = new List<BoardUnitModel> ();
        _unitsToIgnoreThisTurn = new List<BoardUnitModel> ();

        _normalUnitCardInHand = new List<WorkingCard> ();
        _normalSpellCardInHand = new List<WorkingCard> ();
    }

    private IEnumerator TurnStartedHandler ()
    {
        yield return MakeAnAIMove ();
    }

    private void TurnEndedHandler ()
    {
        _attackedUnitTargets.Clear ();
        _unitsToIgnoreThisTurn.Clear ();
    }

    // AI step 1
    private IEnumerator PlayCardsFromHand ()
    {
        yield return CheckGooCard ();

        List<WorkingCard> cardsInHand = new List<WorkingCard> ();
        cardsInHand.AddRange (_normalUnitCardInHand);

        bool wasAction = false;
        foreach (WorkingCard card in cardsInHand)
        {
            if (_testBroker.GetPlayer(_player).BoardCards.Count >= _testBroker.GetPlayer (_player).MaxCardsInPlay)
            {
                break;
            }

            if (CardCanBePlayable (card) && CheckSpecialCardRules (card))
            {
                PlayCardOnBoard (card);
                wasAction = true;
                yield return LetsThink ();
                yield return LetsThink ();
            }
        }

        foreach (WorkingCard card in _normalSpellCardInHand)
        {
            if (CardCanBePlayable (card) && CheckSpecialCardRules (card))
            {
                PlayCardOnBoard (card);
                wasAction = true;
                yield return LetsThink ();
                yield return LetsThink ();
            }
        }

        if (wasAction)
        {
            yield return LetsThink ();
            yield return LetsThink ();
        }

        yield return CheckGooCard ();
    }

    // AI step 2
    private IEnumerator UseUnitsOnBoard ()
    {
        List<BoardUnitModel> unitsOnBoard = new List<BoardUnitModel> ();
        List<BoardUnitModel> alreadyUsedUnits = new List<BoardUnitModel> ();

        unitsOnBoard.AddRange (GetUnitsOnBoard ());

        if (OpponentHasHeavyUnits ())
        {
            foreach (BoardUnitModel unit in unitsOnBoard)
            {
                while (UnitCanBeUsable (unit))
                {
                    BoardUnitModel attackedUnit = GetTargetOpponentUnit ();
                    if (attackedUnit != null)
                    {
                        unit.DoCombat (attackedUnit);
                        alreadyUsedUnits.Add (unit);

                        yield return LetsThink ();
                        if (!OpponentHasHeavyUnits ())
                        {
                            break;
                        }
                    }
                    else break;
                }
            }
        }

        foreach (BoardUnitModel creature in alreadyUsedUnits)
        {
            unitsOnBoard.Remove (creature);
        }

        int totalValue = GetPlayerAttackingValue ();
        if (totalValue >= _testBroker.GetPlayer(_player).Defense)
        {
            foreach (BoardUnitModel unit in unitsOnBoard)
            {
                while (UnitCanBeUsable (unit))
                {
                    unit.DoCombat (_testBroker.GetPlayer (_opponent));
                    yield return LetsThink ();
                }
            }
        }
        else
        {
            foreach (BoardUnitModel unit in unitsOnBoard)
            {
                while (UnitCanBeUsable (unit))
                {
                    if (GetPlayerAttackingValue () > GetOpponentAttackingValue ())
                    {
                        unit.DoCombat (_testBroker.GetPlayer (_opponent));
                        yield return LetsThink ();
                    }
                    else
                    {
                        BoardUnitModel attackedCreature = GetRandomOpponentUnit ();

                        if (attackedCreature != null)
                        {
                            unit.DoCombat (attackedCreature);
                            yield return LetsThink ();
                        }
                        else
                        {
                            unit.DoCombat (_testBroker.GetPlayer (_opponent));
                            yield return LetsThink ();
                        }
                    }
                }
            }
        }

        yield return null;
    }

    // AI step 3
    private IEnumerator UsePlayerSkills ()
    {
        bool wasAction = false;

        if (_testBroker.GetPlayer (_player).IsStunned)
            yield break;

        if (_testBroker.GetPlayerPrimarySkill(_player).IsSkillReady)
        {
            DoBoardSkill (_testBroker.GetPlayerPrimarySkill (_player));
            wasAction = true;
        }

        if (wasAction)
        {
            yield return LetsThink ();
        }

        wasAction = false;
        if (_testBroker.GetPlayerSecondarySkill (_player).IsSkillReady)
        {
            DoBoardSkill (_testBroker.GetPlayerSecondarySkill (_player));
            wasAction = true;
        }

        if (wasAction)
        {
            yield return LetsThink ();
            yield return LetsThink ();
        }

        yield return null;
    }

    // some thinking - delay between general actions
    private IEnumerator LetsThink ()
    {
        yield return new WaitForSeconds (1.1f);
    }

    private IEnumerator CheckGooCard ()
    {
        int benefit = 0;
        int boardCount = 0;
        int gooAmount = _testBroker.GetPlayer (_player).CurrentGoo;
        List<WorkingCard> overflowGooCards = new List<WorkingCard> ();
        List<WorkingCard> cards = new List<WorkingCard> ();
        cards.AddRange (GetUnitCardsInHand ());
        cards.AddRange (GetSpellCardsInHand ());
        cards = cards.FindAll (x => CardBePlayableForOverflowGoo (x.LibraryCard.Cost, gooAmount));
        AbilityData overflowGooAbility;
        for (int i = 0; i < cards.Count; i++)
        {
            if (cards[i].LibraryCard.Abilities != null)
            {
                AbilityData attackOverlordAbility = cards[i].LibraryCard.Abilities
                    .Find (x => x.AbilityType == Enumerators.AbilityType.ATTACK_OVERLORD);
                if (attackOverlordAbility != null)
                {
                    if (attackOverlordAbility.Value * 2 >= _testBroker.GetPlayer (_player).Defense)
                        break;
                }

                overflowGooAbility = cards[i].LibraryCard.Abilities
                    .Find (x => x.AbilityType == Enumerators.AbilityType.OVERFLOW_GOO);
                if (overflowGooAbility != null)
                {
                    if (_testBroker.GetPlayer (_player).BoardCards.Count + boardCount < _testBroker.GetPlayer (_player).MaxCardsInPlay - 1)
                    {
                        boardCount++;
                        gooAmount -= cards[i].LibraryCard.Cost;
                        benefit += overflowGooAbility.Value - cards[i].LibraryCard.Cost;
                        overflowGooCards.Add (cards[i]);
                        cards = cards.FindAll (x => CardBePlayableForOverflowGoo (x.LibraryCard.Cost, gooAmount));
                    }
                }
            }
        }

        WorkingCard expensiveCard =
            GetUnitCardsInHand ()
                .Find (
                    x => x.LibraryCard.Cost > _testBroker.GetPlayer (_player).CurrentGoo &&
                        x.LibraryCard.Cost <= _testBroker.GetPlayer (_player).CurrentGoo + benefit);
        if (expensiveCard != null)
        {
            bool wasAction = false;
            foreach (WorkingCard card in overflowGooCards)
            {
                if (_testBroker.GetPlayer (_player).BoardCards.Count >= _testBroker.GetPlayer (_player).MaxCardsInPlay)
                    break;
                if (CardCanBePlayable (card))
                {
                    PlayCardOnBoard (card);
                    wasAction = true;
                    yield return LetsThink ();
                    yield return LetsThink ();
                }
            }

            PlayCardOnBoard (expensiveCard);

            yield return LetsThink ();
            yield return LetsThink ();
            if (wasAction)
            {
                yield return LetsThink ();
                yield return LetsThink ();
            }
        }
        else
        {
            _normalUnitCardInHand.Clear ();
            _normalUnitCardInHand.AddRange (GetUnitCardsInHand ());
            _normalUnitCardInHand.RemoveAll (x =>
                 x.LibraryCard.Abilities.Exists (z => z.AbilityType == Enumerators.AbilityType.OVERFLOW_GOO));
            _normalSpellCardInHand.Clear ();
            _normalSpellCardInHand.AddRange (GetSpellCardsInHand ());
            _normalSpellCardInHand.RemoveAll (x =>
                 x.LibraryCard.Abilities.Exists (z => z.AbilityType == Enumerators.AbilityType.OVERFLOW_GOO));
        }

        yield return LetsThink ();
    }

    private bool CardBePlayableForOverflowGoo (int cost, int goo)
    {
        return cost <= goo && _testBroker.GetPlayer (_player).Turn > MinTurnForAttack;
    }

    private bool CardCanBePlayable (WorkingCard card)
    {
        return card.LibraryCard.Cost <= _testBroker.GetPlayer (_player).CurrentGoo &&
            _testBroker.GetPlayer (_player).Turn > MinTurnForAttack;
    }

    private bool UnitCanBeUsable (BoardUnitModel unit)
    {
        return unit.UnitCanBeUsable ();
    }

    private bool CheckSpecialCardRules (WorkingCard card)
    {
        if (card.LibraryCard.Abilities != null)
        {
            foreach (AbilityData ability in card.LibraryCard.Abilities)
            {
                if (ability.AbilityType == Enumerators.AbilityType.ATTACK_OVERLORD)
                {
                    // smart enough HP to use goo carriers
                    if (ability.Value * 2 >= _testBroker.GetPlayer (_player).Defense)
                    {
                        return false;
                    }
                }
            }
        }

        return true;
    }

    private void PlayCardOnBoard (WorkingCard card)
    {
        BoardCard boardCard = null;
        if (_player == Enumerators.MatchPlayer.CurrentPlayer)
        {
            boardCard = _battlegroundController.PlayerHandCards.Find (x => x.WorkingCard.Equals (card));
        }

        bool needTargetForAbility = false;

        if (card.LibraryCard.Abilities != null && card.LibraryCard.Abilities.Count > 0)
        {
            needTargetForAbility =
                card.LibraryCard.Abilities.FindAll (x => x.AbilityTargetTypes.Count > 0).Count > 0;
        }

        BoardObject target = null;

        if (needTargetForAbility)
        {
            target = GetAbilityTarget (card);
        }

        switch (card.LibraryCard.CardKind)
        {
            case Enumerators.CardKind.CREATURE when _testBroker.GetBoardCards (_player).Count < _gameplayManager.OpponentPlayer.MaxCardsInPlay:
                _testBroker.GetPlayer (_player).RemoveCardFromHand (card);
                _testBroker.GetPlayer (_player).AddCardToBoard (card);

                if (_player == Enumerators.MatchPlayer.CurrentPlayer)
                {
                    _cardsController.PlayPlayerCard (_testBroker.GetPlayer (_player), boardCard, boardCard.HandBoardCard, PlayCardOnBoard => 
                    {
                        PlayerMove playerMove = new PlayerMove (Enumerators.PlayerActionType.PlayCardOnBoard, PlayCardOnBoard);
                        _gameplayManager.PlayerMoves.AddPlayerMove (playerMove);
                    });

                    if (target != null)
                    {
                        WorkingCard workingCard = boardCard.WorkingCard;

                        BoardUnitView boardUnitViewElement = new BoardUnitView (new BoardUnitModel (), _testBroker.GetPlayerBoardGameObject (_player).transform);
                        boardUnitViewElement.Model.OwnerPlayer = card.Owner;
                        boardUnitViewElement.SetObjectInfo (workingCard);

                        GameObject boardUnit = boardUnitViewElement.GameObject;
                        boardUnit.tag = _testBroker.GetSRTags (_player);
                        boardUnit.transform.position = Vector3.zero;

                        bool createTargetArrow = false;

                        if (card.LibraryCard.Abilities != null && card.LibraryCard.Abilities.Count > 0)
                        {
                            createTargetArrow =
                                _abilitiesController.IsAbilityCanActivateTargetAtStart (
                                    card.LibraryCard.Abilities[0]);
                        }

                        if (target != null)
                        {
                            Debug.Log ("Target");

                            Action callback = () => {
                                _abilitiesController.CallAbility (card.LibraryCard, null, workingCard, Enumerators.CardKind.CREATURE, boardUnitViewElement.Model, null, false, null, target);
                            };

                            _boardArrowController.DoAutoTargetingArrowFromTo<OpponentBoardArrow> (boardUnit.transform, target, 1f, action: callback);
                        }
                        else
                        {
                            Debug.Log ("No target");

                            _abilitiesController.CallAbility (card.LibraryCard, null, workingCard,
                                Enumerators.CardKind.CREATURE, boardUnitViewElement.Model, null, false, null, target);
                            _abilitiesController.CallAbility (card.LibraryCard, null, workingCard,
                                Enumerators.CardKind.CREATURE, boardUnitViewElement.Model, null, false, null);
                        }
                    }
                }
                else
                {
                    _cardsController.PlayOpponentCard (_testBroker.GetPlayer (_player), card, target, PlayCardCompleteHandler);
                }

                _cardsController.DrawCardInfo (card);
                break;
            case Enumerators.CardKind.SPELL:
                if (target != null && needTargetForAbility || !needTargetForAbility)
                {
                    _testBroker.GetPlayer (_player).RemoveCardFromHand (card);
                    _testBroker.GetPlayer (_player).AddCardToBoard (card);

                    if (_player == Enumerators.MatchPlayer.CurrentPlayer)
                    {
                        _cardsController.PlayPlayerCard (_testBroker.GetPlayer (_player), boardCard, boardCard.HandBoardCard, PlayCardOnBoard =>
                        {
                            PlayerMove playerMove = new PlayerMove (Enumerators.PlayerActionType.PlayCardOnBoard, PlayCardOnBoard);
                            _gameplayManager.PlayerMoves.AddPlayerMove (playerMove);
                        });
                    }
                    else
                    {
                        _cardsController.PlayOpponentCard (_testBroker.GetPlayer (_player), card, target, PlayCardCompleteHandler);
                    }

                    _cardsController.DrawCardInfo (card);
                }

                break;
        }

        _testBroker.GetPlayer (_player).CurrentGoo -= card.LibraryCard.Cost;
    }

    private void PlayCardCompleteHandler (WorkingCard card, BoardObject target)
    {
        WorkingCard workingCard = null;

        if (_testBroker.GetPlayer (_player).CardsOnBoard.Count > 0)
        {
            workingCard = _testBroker.GetPlayer (_player).CardsOnBoard[_testBroker.GetPlayer (_player).CardsOnBoard.Count - 1];
        }

        if (workingCard == null || card == null)
            return;

        switch (card.LibraryCard.CardKind)
        {
            case Enumerators.CardKind.CREATURE:
                {
                    BoardUnitView boardUnitViewElement = new BoardUnitView (new BoardUnitModel (), _testBroker.GetPlayerBoardGameObject(_player).transform);
                    GameObject boardUnit = boardUnitViewElement.GameObject;
                    boardUnit.tag = _testBroker.GetSRTags (_player);
                    boardUnit.transform.position = Vector3.zero;
                    boardUnitViewElement.Model.OwnerPlayer = card.Owner;

                    boardUnitViewElement.SetObjectInfo (workingCard);
                    _testBroker.GetBoardCards (_player).Add (boardUnitViewElement);

                    boardUnit.transform.position +=
                        Vector3.up * 2f; // Start pos before moving cards to the opponents board

                    _testBroker.GetPlayer (_player).BoardCards.Add (boardUnitViewElement);

                    _actionsQueueController.PostGameActionReport (new PastActionsPopup.PastActionParam ()
                    {
                        ActionType = Enumerators.ActionType.PlayCardFromHand,
                        Caller = boardUnitViewElement.Model,
                        TargetEffects = new List<PastActionsPopup.TargetEffectParam> ()
                    });

                    boardUnitViewElement.PlayArrivalAnimation ();

                    _abilitiesController.ResolveAllAbilitiesOnUnit (boardUnitViewElement.Model, false);

                    if (_player == Enumerators.MatchPlayer.CurrentPlayer)
                    {
                        _battlegroundController.UpdatePositionOfBoardUnitsOfPlayer (
                            _testBroker.GetPlayer (_player).BoardCards,
                            () => {
                                bool createTargetArrow = false;

                                if (card.LibraryCard.Abilities != null && card.LibraryCard.Abilities.Count > 0)
                                {
                                    createTargetArrow =
                                        _abilitiesController.IsAbilityCanActivateTargetAtStart (
                                            card.LibraryCard.Abilities[0]);
                                }

                                if (target != null)
                                {
                                    Debug.Log ("Target");

                                    Action callback = () => {
                                        _abilitiesController.CallAbility (card.LibraryCard, null, workingCard, Enumerators.CardKind.CREATURE, boardUnitViewElement.Model, null, false, null, target);
                                    };

                                    _boardArrowController.DoAutoTargetingArrowFromTo<OpponentBoardArrow> (boardUnit.transform, target, 1f, action: callback);
                                }
                                else
                                {
                                    Debug.Log ("Noarget");

                                    _abilitiesController.CallAbility (card.LibraryCard, null, workingCard,
                                        Enumerators.CardKind.CREATURE, boardUnitViewElement.Model, null, false, null);
                                }
                            });
                    }
                    else
                    {
                        _battlegroundController.UpdatePositionOfBoardUnitsOfOpponent (
                        () => {
                            bool createTargetArrow = false;

                            if (card.LibraryCard.Abilities != null && card.LibraryCard.Abilities.Count > 0)
                            {
                                createTargetArrow =
                                    _abilitiesController.IsAbilityCanActivateTargetAtStart (
                                        card.LibraryCard.Abilities[0]);
                            }

                            if (target != null)
                            {
                                Action callback = () => {
                                    _abilitiesController.CallAbility (card.LibraryCard, null, workingCard, Enumerators.CardKind.CREATURE, boardUnitViewElement.Model, null, false, null, target);
                                };

                                _boardArrowController.DoAutoTargetingArrowFromTo<OpponentBoardArrow> (boardUnit.transform, target, 1f, action: callback);
                            }
                            else
                            {
                                _abilitiesController.CallAbility (card.LibraryCard, null, workingCard,
                                    Enumerators.CardKind.CREATURE, boardUnitViewElement.Model, null, false, null);
                            }
                        });
                    }


                    break;
                }
            case Enumerators.CardKind.SPELL:
                {
                    GameObject spellCard = UnityEngine.Object.Instantiate (_cardsController.ItemCardViewPrefab);
                    spellCard.transform.position = _testBroker.GetSpellsPivotGameObject(_player).transform.position;

                    CurrentSpellCard = new SpellBoardCard (spellCard);

                    CurrentSpellCard.Init (workingCard);
                    CurrentSpellCard.SetHighlightingEnabled (false);

                    BoardSpell boardSpell = new BoardSpell (spellCard, workingCard);

                    spellCard.gameObject.SetActive (false);

                    bool createTargetArrow = false;

                    if (card.LibraryCard.Abilities != null && card.LibraryCard.Abilities.Count > 0)
                    {
                        createTargetArrow =
                            _abilitiesController.IsAbilityCanActivateTargetAtStart (card.LibraryCard.Abilities[0]);
                    }

                    if (target != null)
                    {
                        Action callback = () => {
                            _abilitiesController.CallAbility (card.LibraryCard, null, workingCard, Enumerators.CardKind.SPELL, boardSpell, null, false, null, target);
                        };

                        _boardArrowController.DoAutoTargetingArrowFromTo<OpponentBoardArrow> (_testBroker.GetPlayer (_player).AvatarObject.transform, target, 1f, action: callback);
                    }
                    else
                    {
                        _abilitiesController.CallAbility (card.LibraryCard, null, workingCard, Enumerators.CardKind.SPELL, boardSpell, null, false, null);
                    }

                    break;
                }
        }
    }

    private BoardObject GetAbilityTarget (WorkingCard card)
    {
        Card libraryCard = card.LibraryCard;

        BoardObject target = null;

        List<AbilityData> abilitiesWithTarget = new List<AbilityData> ();

        bool needsToSelectTarget = false;
        foreach (AbilityData ability in libraryCard.Abilities)
        {
            foreach (Enumerators.AbilityTargetType item in ability.AbilityTargetTypes)
            {
                switch (item)
                {
                    case Enumerators.AbilityTargetType.OPPONENT_CARD:
                        if (_testBroker.GetPlayer(_opponent).BoardCards.Count > 1 ||
                            ability.AbilityType == Enumerators.AbilityType.CARD_RETURN &&
                            _testBroker.GetPlayer (_opponent).BoardCards.Count > 0)
                        {
                            needsToSelectTarget = true;
                            abilitiesWithTarget.Add (ability);
                        }

                        break;
                    case Enumerators.AbilityTargetType.PLAYER_CARD:
                        if (_testBroker.GetPlayer (_player).BoardCards.Count > 1 ||
                            libraryCard.CardKind == Enumerators.CardKind.SPELL ||
                            ability.AbilityType == Enumerators.AbilityType.CARD_RETURN &&
                            _testBroker.GetPlayer (_player).BoardCards.Count > 0)
                        {
                            needsToSelectTarget = true;
                            abilitiesWithTarget.Add (ability);
                        }

                        break;
                    case Enumerators.AbilityTargetType.PLAYER:
                    case Enumerators.AbilityTargetType.OPPONENT:
                    case Enumerators.AbilityTargetType.ALL:
                        needsToSelectTarget = true;
                        abilitiesWithTarget.Add (ability);
                        break;
                }
            }
        }

        if (!needsToSelectTarget)
            return null;

        foreach (AbilityData ability in abilitiesWithTarget)
        {
            switch (ability.AbilityType)
            {
                case Enumerators.AbilityType.ADD_GOO_VIAL:
                    target = _testBroker.GetPlayer (_player);
                    break;
                case Enumerators.AbilityType.CARD_RETURN:
                    if (!AddRandomTargetUnit (true, ref target, false, true))
                    {
                        AddRandomTargetUnit (false, ref target, true, true);
                    }

                    break;
                case Enumerators.AbilityType.DAMAGE_TARGET:
                    CheckAndAddTargets (ability, ref target);
                    break;
                case Enumerators.AbilityType.DAMAGE_TARGET_ADJUSTMENTS:
                    if (!AddRandomTargetUnit (true, ref target))
                    {
                        target = _testBroker.GetPlayer (_opponent);
                    }

                    break;
                case Enumerators.AbilityType.MASSIVE_DAMAGE:
                    AddRandomTargetUnit (true, ref target);
                    break;
                case Enumerators.AbilityType.MODIFICATOR_STATS:
                    if (ability.Value > 0)
                    {
                        AddRandomTargetUnit (false, ref target);
                    }
                    else
                    {
                        AddRandomTargetUnit (true, ref target);
                    }

                    break;
                case Enumerators.AbilityType.STUN:
                    CheckAndAddTargets (ability, ref target);
                    break;
                case Enumerators.AbilityType.STUN_OR_DAMAGE_ADJUSTMENTS:
                    CheckAndAddTargets (ability, ref target);
                    break;
                case Enumerators.AbilityType.CHANGE_STAT:
                    if (ability.Value > 0)
                    {
                        AddRandomTargetUnit (false, ref target);
                    }
                    else
                    {
                        AddRandomTargetUnit (true, ref target);
                    }

                    break;
                case Enumerators.AbilityType.SUMMON:
                    break;
                case Enumerators.AbilityType.WEAPON:
                    target = _testBroker.GetPlayer (_opponent);
                    break;
                case Enumerators.AbilityType.SPURT:
                    AddRandomTargetUnit (true, ref target);
                    break;
                case Enumerators.AbilityType.SPELL_ATTACK:
                    CheckAndAddTargets (ability, ref target);
                    break;
                case Enumerators.AbilityType.HEAL:
                    List<BoardUnitModel> units = GetUnitsWithLowHp ();

                    if (units.Count > 0)
                    {
                        target = units[_random.Next (0, units.Count)];
                    }
                    else
                    {
                        target = _testBroker.GetPlayer (_player);
                    }

                    break;
                case Enumerators.AbilityType.DOT:
                    CheckAndAddTargets (ability, ref target);
                    break;
                case Enumerators.AbilityType.DESTROY_UNIT_BY_TYPE:
                    GetTargetByType (ability, ref target, false);
                    break;
            }

            return target; // hack to handle only one ability
        }

        return null;
    }

    private void CheckAndAddTargets (AbilityData ability, ref BoardObject target)
    {
        if (ability.AbilityTargetTypes.Contains (Enumerators.AbilityTargetType.OPPONENT_CARD))
        {
            AddRandomTargetUnit (true, ref target);
        }
        else if (ability.AbilityTargetTypes.Contains (Enumerators.AbilityTargetType.OPPONENT))
        {
            target = _testBroker.GetPlayer(_opponent);
        }
    }

    private void GetTargetByType (AbilityData ability, ref BoardObject target, bool checkPlayerAlso)
    {
        if (ability.AbilityTargetTypes.Contains (Enumerators.AbilityTargetType.OPPONENT_CARD))
        {
            List<BoardUnitView> targets = GetHeavyUnitsOnBoard (_testBroker.GetPlayer (_opponent));

            if (targets.Count > 0)
            {
                target = targets[UnityEngine.Random.Range (0, targets.Count)].Model;
            }

            if (checkPlayerAlso && target == null &&
                ability.AbilityTargetTypes.Contains (Enumerators.AbilityTargetType.PLAYER_CARD))
            {
                target = _testBroker.GetPlayer (_opponent);

                targets = GetHeavyUnitsOnBoard (_testBroker.GetPlayer (_player));

                if (targets.Count > 0)
                {
                    target = targets[UnityEngine.Random.Range (0, targets.Count)].Model;
                }
            }
        }
    }

    private List<BoardUnitView> GetHeavyUnitsOnBoard (Player player)
    {
        return player.BoardCards.FindAll (x => x.Model.HasHeavy || x.Model.HasBuffHeavy);
    }

    private bool AddRandomTargetUnit (
            bool opponent, ref BoardObject target, bool lowHp = false, bool addAttackIgnore = false)
    {
        BoardUnitModel boardUnit = opponent ? GetRandomOpponentUnit () : GetRandomUnit (lowHp);
        if (boardUnit == null)
            return false;

        target = boardUnit;

        if (addAttackIgnore)
        {
            _attackedUnitTargets.Add (boardUnit);
        }

        return true;
    }

    private int GetPlayerAttackingValue ()
    {
        int power = 0;
        foreach (BoardUnitView creature in _testBroker.GetPlayer (_player).BoardCards)
        {
            if (creature.Model.CurrentHp > 0 && (creature.Model.NumTurnsOnBoard >= 1 || creature.Model.HasFeral))
            {
                power += creature.Model.CurrentDamage;
            }
        }

        return power;
    }

    private int GetOpponentAttackingValue ()
    {
        int power = 0;
        foreach (BoardUnitView card in _testBroker.GetPlayer (_opponent).BoardCards)
        {
            power += card.Model.CurrentDamage;
        }

        return power;
    }

    private List<BoardUnitModel> GetUnitsWithLowHp (List<BoardUnitModel> unitsToIgnore = null)
    {
        List<BoardUnitModel> finalList = new List<BoardUnitModel> ();

        List<BoardUnitModel> list = GetUnitsOnBoard ();

        foreach (BoardUnitModel item in list)
        {
            if (item.CurrentHp < item.MaxCurrentHp)
            {
                finalList.Add (item);
            }
        }

        if (unitsToIgnore != null)
        {
            finalList = finalList.FindAll (x => !unitsToIgnore.Contains (x));
        }

        finalList = finalList.OrderBy (x => x.CurrentHp).ThenBy (y => y.CurrentHp.ToString ().Length).ToList ();

        return finalList;
    }

    private List<WorkingCard> GetUnitCardsInHand ()
    {
        List<WorkingCard> list =
            _testBroker.GetPlayer (_player).CardsInHand.FindAll (x =>
                 x.LibraryCard.CardKind == Enumerators.CardKind.CREATURE);

        List<Card> cards = new List<Card> ();

        foreach (WorkingCard item in list)
        {
            cards.Add (_dataManager.CachedCardsLibraryData.GetCard (item.CardId));
        }

        cards = cards.OrderBy (x => x.Cost).ThenBy (y => y.Cost.ToString ().Length).ToList ();

        List<WorkingCard> sortedList = new List<WorkingCard> ();

        cards.Reverse ();

        foreach (Card item in cards)
        {
            sortedList.Add (list.Find (x => x.CardId == item.Id && !sortedList.Contains (x)));
        }

        list.Clear ();
        cards.Clear ();

        return sortedList;
    }

    private List<WorkingCard> GetSpellCardsInHand ()
    {
        return _testBroker.GetPlayer (_player).CardsInHand.FindAll (x =>
             x.LibraryCard.CardKind == Enumerators.CardKind.SPELL);
    }

    private List<BoardUnitModel> GetUnitsOnBoard ()
    {
        return
            _testBroker.GetPlayer (_player).BoardCards
                .FindAll (x => x.Model.CurrentHp > 0)
                .Select (x => x.Model)
                .ToList ();
    }

    private BoardUnitModel GetRandomUnit (bool lowHp = false, List<BoardUnitModel> unitsToIgnore = null)
    {
        List<BoardUnitModel> eligibleUnits;

        if (!lowHp)
        {
            eligibleUnits =
                _testBroker.GetPlayer (_player).BoardCards
                    .FindAll (x => x.Model.CurrentHp > 0 && !_attackedUnitTargets.Contains (x.Model))
                    .Select (x => x.Model)
                    .ToList ();
        }
        else
        {
            eligibleUnits =
                _testBroker.GetPlayer (_player).BoardCards
                    .FindAll (x => x.Model.CurrentHp < x.Model.MaxCurrentHp && !_attackedUnitTargets.Contains (x.Model))
                    .Select (x => x.Model)
                    .ToList ();
        }

        if (unitsToIgnore != null)
        {
            eligibleUnits = eligibleUnits.FindAll (x => !unitsToIgnore.Contains (x));
        }

        if (eligibleUnits.Count > 0)
        {
            return eligibleUnits[_random.Next (0, eligibleUnits.Count)];
        }

        return null;
    }

    private BoardUnitModel GetTargetOpponentUnit ()
    {
        List<BoardUnitModel> eligibleUnits =
            _testBroker.GetPlayer (_opponent).BoardCards
                .FindAll (x => x.Model.CurrentHp > 0)
                .Select (x => x.Model)
                .ToList ();

        if (eligibleUnits.Count > 0)
        {
            List<BoardUnitModel> heavyUnits = eligibleUnits.FindAll (x => x.IsHeavyUnit);
            if (heavyUnits.Count >= 1)
            {
                return heavyUnits[_random.Next (0, heavyUnits.Count)];
            }

            return eligibleUnits[_random.Next (0, eligibleUnits.Count)];
        }

        return null;
    }

    private BoardUnitModel GetRandomOpponentUnit (List<BoardUnitModel> unitsToIgnore = null)
    {
        List<BoardUnitModel> eligibleCreatures =
            _testBroker.GetPlayer (_opponent).BoardCards
                .Select (x => x.Model)
                .Where (x => x.CurrentHp > 0 && !_attackedUnitTargets.Contains (x))
                .ToList ();

        if (unitsToIgnore != null)
        {
            eligibleCreatures = eligibleCreatures.FindAll (x => !unitsToIgnore.Contains (x));
        }

        if (eligibleCreatures.Count > 0)
        {
            return eligibleCreatures[_random.Next (0, eligibleCreatures.Count)];
        }

        return null;
    }

    private bool OpponentHasHeavyUnits ()
    {
        List<BoardUnitModel> board =
            _testBroker.GetPlayer (_opponent).BoardCards
                .Select (x => x.Model)
                .ToList ();
        List<BoardUnitModel> eligibleCreatures = board.FindAll (x => x.CurrentHp > 0);
        if (eligibleCreatures.Count > 0)
        {
            List<BoardUnitModel> provokeCreatures = eligibleCreatures.FindAll (x => x.IsHeavyUnit);
            return provokeCreatures != null && provokeCreatures.Count >= 1;
        }

        return false;
    }

    private void DoBoardSkill (BoardSkill skill)
    {
        BoardObject target = null;

        Enumerators.AffectObjectType selectedObjectType = Enumerators.AffectObjectType.None;

        switch (skill.Skill.OverlordSkill)
        {
            case Enumerators.OverlordSkill.HARDEN:
            case Enumerators.OverlordSkill.STONE_SKIN:
            case Enumerators.OverlordSkill.DRAW:
                {
                    selectedObjectType = Enumerators.AffectObjectType.Player;
                    target = _testBroker.GetPlayer (_player);
                }

                break;
            case Enumerators.OverlordSkill.HEALING_TOUCH:
                {
                    List<BoardUnitModel> units = GetUnitsWithLowHp ();

                    if (units.Count > 0)
                    {
                        target = units[0];
                        selectedObjectType = Enumerators.AffectObjectType.Character;
                    }
                    else
                        return;
                }
                break;
            case Enumerators.OverlordSkill.MEND:
                {
                    target = _testBroker.GetPlayer (_player);
                    selectedObjectType = Enumerators.AffectObjectType.Player;

                    if (_testBroker.GetPlayer (_player).Defense > 13)
                    {
                        if (skill.Skill.ElementTargetTypes.Count > 0)
                        {
                            _unitsToIgnoreThisTurn =
                                _testBroker.GetPlayer (_player).BoardCards
                                .FindAll (x => !skill.Skill.ElementTargetTypes.Contains (x.Model.Card.LibraryCard.CardSetType))
                                .Select (x => x.Model)
                                .ToList ();
                        }

                        List<BoardUnitModel> units = GetUnitsWithLowHp (_unitsToIgnoreThisTurn);

                        if (units.Count > 0)
                        {
                            target = units[0];
                            selectedObjectType = Enumerators.AffectObjectType.Character;
                        }
                        else
                            return;
                    }
                    else
                        return;
                }

                break;
            case Enumerators.OverlordSkill.RABIES:
                {
                    _unitsToIgnoreThisTurn =
                        _testBroker.GetPlayer (_player).BoardCards.FindAll (x =>
                         skill.Skill.ElementTargetTypes.Count > 0 &&
                         !skill.Skill.ElementTargetTypes.Contains (x.Model.Card.LibraryCard.CardSetType) ||
                         x.Model.NumTurnsOnBoard > 0 || x.Model.HasFeral)
                            .Select (x => x.Model)
                            .ToList ();
                    BoardUnitModel unit = GetRandomUnit (false, _unitsToIgnoreThisTurn);

                    if (unit != null)
                    {
                        target = unit;
                        selectedObjectType = Enumerators.AffectObjectType.Character;
                    }
                    else
                        return;
                }

                break;
            case Enumerators.OverlordSkill.POISON_DART:
            case Enumerators.OverlordSkill.TOXIC_POWER:
            case Enumerators.OverlordSkill.ICE_BOLT:
            case Enumerators.OverlordSkill.FREEZE:
            case Enumerators.OverlordSkill.FIRE_BOLT:
                {
                    target = _testBroker.GetPlayer (_opponent);
                    selectedObjectType = Enumerators.AffectObjectType.Player;

                    BoardUnitModel unit = GetRandomOpponentUnit ();

                    if (unit != null)
                    {
                        target = unit;
                        selectedObjectType = Enumerators.AffectObjectType.Character;
                    }
                    else
                        return;
                }

                break;
            case Enumerators.OverlordSkill.PUSH:
                {
                    if (skill.Skill.ElementTargetTypes.Count > 0)
                    {
                        _unitsToIgnoreThisTurn =
                            _testBroker.GetPlayer (_player).BoardCards
                                .FindAll (x => !skill.Skill.ElementTargetTypes.Contains (x.Model.Card.LibraryCard.CardSetType))
                                .Select (x => x.Model)
                                .ToList ();
                    }

                    List<BoardUnitModel> units = GetUnitsWithLowHp (_unitsToIgnoreThisTurn);

                    if (units.Count > 0)
                    {
                        target = units[0];

                        _unitsToIgnoreThisTurn.Add ((BoardUnitModel)target);

                        selectedObjectType = Enumerators.AffectObjectType.Character;
                    }
                    else
                    {
                        BoardUnitModel unit = GetRandomOpponentUnit (_unitsToIgnoreThisTurn);

                        if (unit != null)
                        {
                            target = unit;

                            _unitsToIgnoreThisTurn.Add ((BoardUnitModel)target);

                            selectedObjectType = Enumerators.AffectObjectType.Character;
                        }
                        else
                            return;
                    }
                }

                break;
            default:
                throw new ArgumentOutOfRangeException (nameof (skill.Skill.OverlordSkill), skill.Skill.OverlordSkill, null);
        }

        skill.StartDoSkill ();

        switch (selectedObjectType)
        {
            case Enumerators.AffectObjectType.Player:
                    skill.FightTargetingArrow.SelectedPlayer = (Player)target;

                    break;
                case Enumerators.AffectObjectType.Character:
                    BoardUnitView selectedCardView = _battlegroundController.GetBoardUnitViewByModel ((BoardUnitModel)target);
                    skill.FightTargetingArrow.SelectedCard = selectedCardView;

                    break;
                case Enumerators.AffectObjectType.None:

                    break;
                default:
                    throw new ArgumentOutOfRangeException (nameof (selectedObjectType), selectedObjectType, null);
        }

        _boardArrowController.DoAutoTargetingArrowFromTo<OpponentBoardArrow> (skill.SelfObject.transform, target);

        _skillsController.DoSkillAction (skill, target);

        skill.EndDoSkill ();

        /* Action callback = () => {
            switch (selectedObjectType)
            {
                case Enumerators.AffectObjectType.Player:
                    skill.FightTargetingArrow.SelectedPlayer = (Player) target;
                    break;
                case Enumerators.AffectObjectType.Character:
                    BoardUnitView selectedCardView = _battlegroundController.GetBoardUnitViewByModel ((BoardUnitModel) target);
                    skill.FightTargetingArrow.SelectedCard = selectedCardView;
                    break;
                case Enumerators.AffectObjectType.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException (nameof (selectedObjectType), selectedObjectType, null);
            }

            skill.EndDoSkill ();
        };

        _boardArrowController.DoAutoTargetingArrowFromTo<OpponentBoardArrow> (skill.SelfObject.transform, target, 1f, action: callback); */
    }

    #endregion

    [UnityTest]
    [Timeout (500000)]
    public IEnumerator Test1_TutorialSkip ()
    {
        SetTestName ("Tutorial - Skip");

        #region Tutorial Skip

        yield return MainMenuTransition ("Button_Tutorial");

        yield return AssertCurrentPageName ("GameplayPage");

        yield return SkipTutorial ();

        yield return AssertCurrentPageName ("HordeSelectionPage");

        #endregion
    }

    [UnityTest]
    [Timeout (500000)]
    public IEnumerator Test2_TutorialNonSkip ()
    {
        SetTestName ("Tutorial - Non-Skip");

        #region Tutorial Non-Skip

        yield return MainMenuTransition ("Button_Tutorial");

        yield return AssertCurrentPageName ("GameplayPage");

        yield return PlayTutorial_Part1 ();

        yield return PlayTutorial_Part2 ();

        yield return AssertCurrentPageName ("HordeSelectionPage");

        #endregion
    }

    [UnityTest]
    [Timeout (500000)]
    public IEnumerator Test3_SoloGameplay ()
    {
        SetTestName ("Tutorial - Solo Gameplay");

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
    }

    [UnityTest]
    [Timeout (500000)]
    public IEnumerator Test4_SoloGameplayAIMoves ()
    {
        SetTestName ("Tutorial - Solo Gameplay (with AI moves)");

        #region Solo Gameplay

        yield return MainMenuTransition ("Button_Play");

        yield return AssertCurrentPageName ("PlaySelectionPage");

        yield return MainMenuTransition ("Button_SoloMode");

        yield return AssertCurrentPageName ("HordeSelectionPage");

        yield return MainMenuTransition ("Button_Battle");

        yield return AssertCurrentPageName ("GameplayPage");

        yield return SoloGameplay (true);

        yield return AssertCurrentPageName ("HordeSelectionPage");

        #endregion
    }

    [UnityTest]
    public IEnumerator TestN_Cleanup ()
    {
        // Nothing, just to ascertain cleanup

        yield return null;
    }
}
