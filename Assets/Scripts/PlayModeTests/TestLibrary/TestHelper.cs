using Loom.ZombieBattleground;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.Protobuf;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using TMPro; // Used this trick to add it: https://forum.unity.com/threads/namespace-tmpro-not-found-in-test-runner-test.541387/#post-3570214

public class TestHelper
{
    private enum TesterType
    {
        Active,
        Passive
    }

    private List<string> _testerKeys = new List<string> {
        "f32ef9a2cfcb",
        "f12249ff43e4"
    };

    private TesterType _testerType = TesterType.Active;
    private string _testerKey;

    private bool _initialized = false;
    public bool Initialized
    {
        get { return _initialized; }
    }

    private Scene _testScene;
    private GameObject _testerGameObject;
    private UnityEngine.EventSystems.VirtualInputModule _virtualInputModule;
    private RectTransform _fakeCursorTransform;

    private string _testName;
    private float _testStartTime;

    private string lastCheckedPageName;

    private TestBroker _testBroker;
    private Enumerators.MatchPlayer _player;
    private Enumerators.MatchPlayer _opponent;

    private float _positionalTolerance = 5f;

    private IGameplayManager _gameplayManager;
    private IUIManager _uiManager;
    private IDataManager _dataManager;
    private IPvPManager _pvpManager;

    private BattlegroundController _battlegroundController;
    private SkillsController _skillsController;
    private CardsController _cardsController;
    private ActionsQueueController _actionsQueueController;
    private AbilitiesController _abilitiesController;
    private BoardArrowController _boardArrowController;
    private PlayerController _playerController;

    private GameObject canvas1GameObject, canvas2GameObject, canvas3GameObject;
    public GameObject Canvas1 { get { return canvas1GameObject; } }
    public GameObject Canvas2 { get { return canvas2GameObject; } }
    public GameObject Canvas3 { get { return canvas3GameObject; } }

    GameAction<object> _callAbilityAction;

    private Loom.ZombieBattleground.Player _currentPlayer, _opponentPlayer;

    public TestHelper (int testerType = 0)
    {
        _testerType = (TesterType)testerType;
    }

    public void SetTestName (string testName = "")
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

    public IEnumerator SetUp ()
    {
        _testStartTime = Time.unscaledTime;

        if (!_initialized)
        {
            _testerKey = _testerKeys[(int)_testerType];

            _testScene = SceneManager.GetActiveScene ();
            _testerGameObject = _testScene.GetRootGameObjects ()[0];
            _testerGameObject.AddComponent<TestScriptProtector> ();

            yield return SceneManager.LoadSceneAsync ("APP_INIT", LoadSceneMode.Single);

            // RemoveGoogleAnalyticsModule ();

            yield return AddVirtualInputModule ();

            yield return SetCanvases ();

            #region Login

            yield return AssertCurrentPageName ("LoadingPage");

            yield return HandleLogin ();

            yield return AssertLoggedInOrLoginFailed (
                null,
                FailWithMessage ("Wasn't able to login. Try using USE_STAGING_BACKEND"));

            #endregion

            SetGameplayManagers ();

            _initialized = true;
        }

        yield return null;
    }

    public IEnumerator TearDown_Cleanup ()
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

        yield return LetsThink ();

        SceneManager.SetActiveScene (_testScene);
        yield return SceneManager.UnloadSceneAsync (currentScene);
    }

    public IEnumerator TearDown_GoBackToMainScreen ()
    {
        if (IsButtonExist ("Button_Back"))
        {
            yield return MainMenuTransition ("Button_Back");

            yield return AssertCurrentPageName ("PlaySelectionPage");

            yield return MainMenuTransition ("Button_Back");

            yield return AssertCurrentPageName ("MainMenuPage");
        }

        yield return null;
    }

    public IEnumerator ReportTestTime ()
    {
        Debug.LogFormat (
           "\"{0}\" test successfully finished in {1} seconds.",
           _testName,
           Time.unscaledTime - _testStartTime
       );

        yield return LetsThink ();
    }

    private void SetGameplayManagers ()
    {
        _testBroker = new TestBroker ();
        _player = Enumerators.MatchPlayer.CurrentPlayer;
        _opponent = Enumerators.MatchPlayer.OpponentPlayer;

        _gameplayManager = GameClient.Get<IGameplayManager> ();
        _uiManager = GameClient.Get<IUIManager> ();
        _dataManager = GameClient.Get<IDataManager> ();
        _pvpManager = GameClient.Get<IPvPManager> ();

        _battlegroundController = _gameplayManager.GetController<BattlegroundController> ();
        _skillsController = _gameplayManager.GetController<SkillsController> ();
        _cardsController = _gameplayManager.GetController<CardsController> ();
        _actionsQueueController = _gameplayManager.GetController<ActionsQueueController> ();
        _abilitiesController = _gameplayManager.GetController<AbilitiesController> ();
        _boardArrowController = _gameplayManager.GetController<BoardArrowController> ();
        _playerController = _gameplayManager.GetController<PlayerController> ();

        _currentPlayer = _gameplayManager.CurrentPlayer;
        _opponentPlayer = _gameplayManager.OpponentPlayer;
    }

    private IEnumerator SetCanvases ()
    {
        canvas1GameObject = null;

        yield return new WaitUntil (() => GameObject.Find ("Canvas1") != null);

        canvas1GameObject = GameObject.Find ("Canvas1");
        canvas2GameObject = GameObject.Find ("Canvas2");
        canvas3GameObject = GameObject.Find ("Canvas3");

        yield return null;
    }

    public IEnumerator GoBackToMainPage ()
    {
        yield return new WaitUntil (() => {
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

        if (canvas1GameObject.transform.GetChild (1).name.Split ('(')[0] != "MainMenuPage")
        {
            yield return ClickGenericButton ("Button_Back");

            yield return GoBackToMainPage ();
        }

        yield return null;
    }

    private IEnumerator FailWithMessage (string message)
    {
        Assert.Fail (message);

        yield return null;
    }

    private IEnumerator PassWithMessage (string message)
    {
        Assert.Pass (message);

        yield return null;
    }

    public IEnumerator AssertLoggedInOrLoginFailed (IEnumerator callback1, IEnumerator callback2)
    {
        yield return CombinedCheck (
            CheckCurrentPageName, "MainMenuPage", callback1,
            CheckIfLoginErrorOccured, "", callback2);

        yield return null;
    }

    public IEnumerator AssertPvPStartedOrMatchmakingFailed (IEnumerator callback1, IEnumerator callback2)
    {
        yield return CombinedCheck (
            CheckCurrentPageName, "GameplayPage", callback1,
            CheckIfMatchmakingErrorOccured, "", callback2);

        yield return null;
    }

    private IEnumerator CombinedCheck (
        Func<string, bool> check1, string parameter1, IEnumerator callback1,
        Func<string, bool> check2, string parameter2, IEnumerator callback2)
    {
        bool outcomeDecided = false;

        while (outcomeDecided == false)
        {
            if (check1 (parameter1))
            {
                outcomeDecided = true;

                if (callback1 != null)
                    yield return callback1;
            }
            else if (check2 (parameter2))
            {
                outcomeDecided = true;

                if (callback2 != null)
                    yield return callback2;
            }

            yield return null;
        }

        yield return null;
    }

    private bool CheckIfLoginErrorOccured (string dummyParameter)
    {
        GameObject errorTextObject = GameObject.Find ("Beta_Group/Text_Error");

        if (errorTextObject != null && errorTextObject.activeInHierarchy)
        {
            return true;
        }

        return false;
    }

    private bool CheckIfMatchmakingErrorOccured (string dummyParameter)
    {
        // Initially
        // Canvas2 / ConnectionPopup(Clone)

        // Then
        // ConnectionPopup is removed, WarningPopup is added
        // Canvas3 / WarningPopup(Clone)

        if (canvas3GameObject != null && canvas3GameObject.transform.childCount >= 2)
        {
            if (canvas3GameObject.transform.GetChild (1).name.Split ('(')[0] == "WarningPopup")
            {
                return true;
            }

            return false;
        }

        return false;
    }

    private bool CheckCurrentPageName (string expectedPageName)
    {
        if (canvas1GameObject != null && canvas1GameObject.transform.childCount >= 2)
        {
            if (canvas1GameObject.transform.GetChild (1).name.Split ('(')[0] == lastCheckedPageName)
            {
                return false;
            }
            else
            {
                Assert.AreEqual (
                    canvas1GameObject.transform.GetChild (1).name.Split ('(')[0],
                    expectedPageName);

                return true;
            }
        }

        return false;
    }

    public IEnumerator AssertCurrentPageName (string expectedPageName, string errorTextName = "")
    {
        GameObject errorTextObject = null;
        yield return new WaitUntil (() => {
            if (errorTextName.Length >= 1)
            {
                errorTextObject = GameObject.Find (errorTextName);

                if (errorTextObject != null && errorTextObject.activeInHierarchy)
                {
                    Assert.Fail ("Wasn't able to login. Try using USE_STAGING_BACKEND");

                    return true;
                }
            }

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

    public IEnumerator AddVirtualInputModule ()
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

    public IEnumerator MoveCursorToObject (string objectName, float duration)
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

            interpolation = Mathf.Min (interpolation + Time.unscaledTime, duration);

            yield return null;
        }
    }

    public IEnumerator FakeClick ()
    {
        _virtualInputModule.Press ();

        yield return null;

        _virtualInputModule.Release ();

        yield return null;
    }

    public IEnumerator HandleLogin ()
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

    public IEnumerator ClickGenericButton (string buttonName, GameObject parentGameObject = null)
    {
        GameObject menuButtonGameObject = null;

        yield return new WaitUntil (() => {
            if (parentGameObject != null)
            {
                menuButtonGameObject = parentGameObject.transform.Find (buttonName)?.gameObject;
            }
            else
            {
                menuButtonGameObject = GameObject.Find (buttonName);
            }

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

    public bool IsButtonExist (string buttonName)
    {
        return GameObject.Find (buttonName) != null;
    }

    public IEnumerator MainMenuTransition (string transitionPath, float delay = 2f)
    {
        foreach (string buttonName in transitionPath.Split ('/'))
        {
            yield return ClickGenericButton (buttonName);

            yield return new WaitForSeconds (delay);
        }
    }

    public IEnumerator RespondToYesNoOverlay (bool isResponseYes)
    {
        string buttonName = isResponseYes ? "Button_Yes" : "Button_No";

        ButtonShiftingContent overlayButton = null;
        yield return new WaitUntil (() => { overlayButton = GameObject.Find (buttonName)?.GetComponent<ButtonShiftingContent> (); return overlayButton != null; });

        overlayButton.onClick.Invoke ();

        yield return null;
    }

    public IEnumerator WaitUntilPageUnloads ()
    {
        yield return new WaitUntil (() => {
            if (canvas1GameObject != null && canvas1GameObject.transform.childCount <= 1)
            {
                return true;
            }

            return false;
        });
    }

    #region Interactions with PvP module

    public void SetPvPTags (string[] tags)
    {
        _pvpManager.PvPTags.Clear ();

        foreach (string tag in tags)
        {
            _pvpManager.PvPTags.Append<string> (tag);
        }
    }

    #endregion

    #region Adapted from AIController

    private const int MinTurnForAttack = 0;
    public BoardCard CurrentSpellCard;

    private readonly System.Random _random = new System.Random ();

    private List<BoardUnitModel> _attackedUnitTargets;
    private List<BoardUnitModel> _unitsToIgnoreThisTurn;

    private List<WorkingCard> _normalUnitCardInHand, _normalSpellCardInHand;

    public void InitalizePlayer ()
    {
        _attackedUnitTargets = new List<BoardUnitModel> ();
        _unitsToIgnoreThisTurn = new List<BoardUnitModel> ();

        _normalUnitCardInHand = new List<WorkingCard> ();
        _normalSpellCardInHand = new List<WorkingCard> ();

        _callAbilityAction = null; // _actionsQueueController.AddNewActionInToQueue (null);
    }

    public IEnumerator TurnStartedHandler ()
    {
        yield return LetsThink ();

        if (IsGameEnded ())
            yield break;

        yield return PlayCardsFromHand ();

        if (IsGameEnded ())
            yield break;

        yield return LetsThink ();
        yield return LetsThink ();
        yield return LetsThink ();

        yield return UseUnitsOnBoard ();

        if (IsGameEnded ())
            yield break;

        yield return UsePlayerSkills ();

        if (IsGameEnded ())
            yield break;

        if (_testBroker.GetPlayer (_player).SelfHero.HeroElement == Enumerators.SetType.FIRE)
        {
            yield return UseUnitsOnBoard ();

            if (IsGameEnded ())
                yield break;

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

    public void TurnEndedHandler ()
    {
        _attackedUnitTargets.Clear ();
        _unitsToIgnoreThisTurn.Clear ();
    }

    // AI step 1
    public IEnumerator PlayCardsFromHand ()
    {
        yield return CheckGooCard ();

        List<WorkingCard> cardsInHand = new List<WorkingCard> ();
        cardsInHand.AddRange (_normalUnitCardInHand);

        bool wasAction = false;
        foreach (WorkingCard card in cardsInHand)
        {
            if (_testBroker.GetPlayer (_player).BoardCards.Count >= _testBroker.GetPlayer (_player).MaxCardsInPlay)
            {
                break;
            }

            if (CardCanBePlayable (card) && CheckSpecialCardRules (card))
            {
                PlayCardFromHandToBoard (card);
                wasAction = true;
                yield return LetsThink ();
                yield return LetsThink ();
            }
        }

        foreach (WorkingCard card in _normalSpellCardInHand)
        {
            if (CardCanBePlayable (card) && CheckSpecialCardRules (card))
            {
                PlayCardFromHandToBoard (card);
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

        Debug.Log ("Played cards from hand");
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
                if (IsGameEnded ())
                    break;

                while (UnitCanBeUsable (unit))
                {
                    BoardUnitModel attackedUnit = GetTargetOpponentUnit ();
                    if (attackedUnit != null)
                    {
                        unit.DoCombat (attackedUnit);
                        // PlayCardFromBoard (unit, null, attackedUnit);

                        if (_player == Enumerators.MatchPlayer.CurrentPlayer)
                        {
                            unit.OwnerPlayer.ThrowCardAttacked (
                                unit.Card,
                                AffectObjectType.Character,
                                attackedUnit.Card.Id);

                            /* if (target == SelectedPlayer)
                            {
                                creature.Model.OwnerPlayer.ThrowCardAttacked (creature.Model.Card, AffectObjectType.Player, -1);
                            }
                            else
                            {
                                creature.Model.OwnerPlayer.ThrowCardAttacked (creature.Model.Card, AffectObjectType.Character, SelectedCard.Model.Card.Id);
                            } */
                        }

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

        BoardUnitModel attackedCreature = null;

        int totalValue = GetPlayerAttackingValue ();
        if (totalValue >= _testBroker.GetPlayer (_player).Defense)
        {
            foreach (BoardUnitModel unit in unitsOnBoard)
            {
                if (IsGameEnded ())
                    break;

                while (UnitCanBeUsable (unit))
                {
                    unit.DoCombat (_testBroker.GetPlayer (_opponent));
                    // PlayCardFromBoard (unit, _testBroker.GetPlayer (_opponent), null);

                    unit.OwnerPlayer.ThrowCardAttacked (
                        unit.Card,
                        AffectObjectType.Player,
                        -1);

                    yield return LetsThink ();
                }
            }
        }
        else
        {
            foreach (BoardUnitModel unit in unitsOnBoard)
            {
                if (IsGameEnded ())
                    break;

                while (UnitCanBeUsable (unit))
                {
                    if (GetPlayerAttackingValue () > GetOpponentAttackingValue ())
                    {
                        unit.DoCombat (_testBroker.GetPlayer (_opponent));
                        // PlayCardFromBoard (unit, _testBroker.GetPlayer (_opponent), null);

                        unit.OwnerPlayer.ThrowCardAttacked (
                            unit.Card,
                            AffectObjectType.Player,
                            -1);

                        yield return LetsThink ();
                    }
                    else
                    {
                        attackedCreature = GetRandomOpponentUnit ();

                        if (attackedCreature != null)
                        {
                            unit.DoCombat (attackedCreature);
                            // PlayCardFromBoard (unit, null, attackedCreature);

                            unit.OwnerPlayer.ThrowCardAttacked (
                                unit.Card,
                                AffectObjectType.Character,
                                attackedCreature.Card.Id);

                            yield return LetsThink ();
                        }
                        else
                        {
                            unit.DoCombat (_testBroker.GetPlayer (_opponent));
                            // PlayCardFromBoard (unit, _testBroker.GetPlayer (_opponent), null);

                            unit.OwnerPlayer.ThrowCardAttacked (
                                unit.Card,
                                AffectObjectType.Player,
                                -1);

                            yield return LetsThink ();
                        }
                    }
                }
            }
        }

        yield return null;

        Debug.Log ("Played cards from board");
    }

    // todo: review
    // AI step 3
    private IEnumerator UsePlayerSkills ()
    {
        bool wasAction = false;

        if (_testBroker.GetPlayer (_player).IsStunned)
            yield break;

        if (_testBroker.GetPlayerPrimarySkill (_player).IsSkillReady)
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
                    PlayCardFromHandToBoard (card);
                    wasAction = true;
                    yield return LetsThink ();
                    yield return LetsThink ();
                }
            }

            PlayCardFromHandToBoard (expensiveCard);

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
                    // Smart enough HP to use goo carriers
                    if (ability.Value * 2 >= _testBroker.GetPlayer (_player).Defense)
                    {
                        return false;
                    }
                }
            }
        }

        return true;
    }

    public IEnumerator PlayCardFromHandToBoard (int[] cardIndices)
    {
        foreach (int cardIndex in cardIndices)
        {
            BoardCard boardCard = _battlegroundController.PlayerHandCards[cardIndex];

            PlayCardFromHandToBoard (boardCard.WorkingCard);

            yield return LetsThink ();
            yield return LetsThink ();
        }

        yield return null;
    }

    private void PlayCardFromHandToBoard (WorkingCard card)
    {
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
                if (_player == Enumerators.MatchPlayer.CurrentPlayer)
                {
                    BoardCard boardCard = _battlegroundController.PlayerHandCards.Find (x => x.WorkingCard.Equals (card));

                    _cardsController.PlayPlayerCard (_testBroker.GetPlayer (_player), boardCard, boardCard.HandBoardCard, PlayCardOnBoard => {
                        PlayerMove playerMove = new PlayerMove (Enumerators.PlayerActionType.PlayCardOnBoard, PlayCardOnBoard);
                        _gameplayManager.PlayerMoves.AddPlayerMove (playerMove);

                        if (target != null)
                        {
                            foreach (AbilitiesController.ActiveAbility activeAbility in _abilitiesController.ActiveAbilities)
                            {
                                if (target == _testBroker.GetPlayer (_opponent))
                                {
                                    Debug.LogWarning ("- Target: Opponent Player");

                                    activeAbility.Ability.TargetPlayer = _testBroker.GetPlayer (_opponent);
                                    // activeAbility.Ability.TargettingArrow.SelectedPlayer = _testBroker.GetPlayer (_opponent);
                                }
                                else if (target == _testBroker.GetPlayer (_player))
                                {
                                    Debug.LogWarning ("- Target: Current Player");

                                    activeAbility.Ability.TargetPlayer = _testBroker.GetPlayer (_player);
                                    // activeAbility.Ability.TargettingArrow.SelectedPlayer = _testBroker.GetPlayer (_player);
                                }
                                else
                                {
                                    Debug.LogWarning ("- Target: Card");

                                    BoardUnitView targetBoardUnitView = _battlegroundController.PlayerBoardCards.Find (x => x.Model.Id.Equals (target.Id));

                                    if (targetBoardUnitView == null)
                                    {
                                        targetBoardUnitView = _battlegroundController.OpponentBoardCards.Find (x => x.Model.Id.Equals (target.Id));
                                    }

                                    activeAbility.Ability.TargetUnit = targetBoardUnitView.Model;
                                    // activeAbility.Ability.TargettingArrow.SelectedCard = targetBoardUnitView;
                                }

                                if (activeAbility.Ability.TargetUnit != null ||
                                    activeAbility.Ability.TargetPlayer != null)
                                {
                                    activeAbility.Ability.SelectedTargetAction (true);
                                }
                                else
                                {
                                    Debug.LogWarning ("-- No target");
                                }

                            }
                        } 
                    });

                    /* if (target != null)
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

                        _abilitiesController.CallAbility (
                            card.LibraryCard,
                            null,
                            workingCard,
                            Enumerators.CardKind.CREATURE,
                            boardUnitViewElement.Model,
                            null,
                            false,
                            null,
                            null,
                            target);

                        /* Action callback = () => {
                            _abilitiesController.CallAbility (card.LibraryCard, null, workingCard, Enumerators.CardKind.CREATURE, boardUnitViewElement.Model, null, false, null, _callAbilityAction, target);
                        };

                        _boardArrowController.DoAutoTargetingArrowFromTo<OpponentBoardArrow> (boardUnit.transform, target, action: callback); */
                    // }
                }
                else
                {
                    _testBroker.GetPlayer (_player).RemoveCardFromHand (card);
                    _testBroker.GetPlayer (_player).AddCardToBoard (card);

                    _cardsController.PlayOpponentCard (_testBroker.GetPlayer (_player), card, target, PlayCardCompleteHandler);
                }

                _cardsController.DrawCardInfo (card);

                break;
            case Enumerators.CardKind.SPELL:
                if ((target != null && needTargetForAbility) || !needTargetForAbility)
                {
                    _testBroker.GetPlayer (_player).RemoveCardFromHand (card);
                    _testBroker.GetPlayer (_player).AddCardToBoard (card);

                    if (_player == Enumerators.MatchPlayer.CurrentPlayer)
                    {
                        BoardCard boardCard = _battlegroundController.PlayerHandCards.Find (x => x.WorkingCard.Equals (card));

                        _cardsController.PlayPlayerCard (_testBroker.GetPlayer (_player), boardCard, boardCard.HandBoardCard, PlayCardOnBoard => {
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

    // todo: reconsider having this
    public IEnumerator PlayCardFromBoardToOpponent (
        int[] attackerCardIndices,
        int[] attackedCardIndices,
        bool opponentPlayer = false)
    {
        for (int i = 0; i < attackerCardIndices.Length; i++)
        {
            int attackerCardIndex = attackerCardIndices[i];

            BoardUnitView attackerBoardUnitView = _battlegroundController.PlayerBoardCards[attackerCardIndex];

            if (opponentPlayer)
            {
                attackerBoardUnitView.Model.DoCombat (_gameplayManager.OpponentPlayer);
            }
            else
            {
                int attackedCardIndex = attackedCardIndices[i];

                BoardUnitView attackedBoardUnitView = _battlegroundController.OpponentBoardCards[attackedCardIndex];

                attackerBoardUnitView.Model.DoCombat (attackedBoardUnitView.Model);
            }
        }

        yield return LetsThink ();

        yield return null;
    }

    private IEnumerator PlayCardFromBoard (
        BoardUnitModel boardUnitModel,
        Loom.ZombieBattleground.Player targetPlayer,
        BoardUnitModel targetCreatureModel)
    {
        WorkingCard workingCard = boardUnitModel.Card;

        BoardUnitView boardUnitView = new BoardUnitView (new BoardUnitModel (), _testBroker.GetPlayerBoardGameObject (_player).transform);
        boardUnitView.Model.OwnerPlayer = workingCard.Owner;
        boardUnitView.SetObjectInfo (workingCard);

        Debug.LogWarning ("0");

        if (_player == Enumerators.MatchPlayer.CurrentPlayer)
        {
            boardUnitView.SetSelectedUnit (true);

            GameObject boardUnit = boardUnitView.GameObject;

            BattleBoardArrow fightTargetingArrow = _boardArrowController.BeginTargetingArrowFrom<BattleBoardArrow> (boardUnit.transform);
            fightTargetingArrow.TargetsType = new List<Enumerators.SkillTargetType>
            {
                Enumerators.SkillTargetType.OPPONENT,
                Enumerators.SkillTargetType.OPPONENT_CARD
            };
            fightTargetingArrow.BoardCards = _gameplayManager.OpponentPlayer.BoardCards;
            fightTargetingArrow.Owner = boardUnitView;

            _battlegroundController.DestroyCardPreview ();
            _playerController.IsCardSelected = true;

            if (targetPlayer != null)
            {
                Debug.LogWarning ("1");

                fightTargetingArrow.OnPlayerSelected (targetPlayer);
            }
            else if (targetCreatureModel != null)
            {
                Debug.LogWarning ("2");

                WorkingCard targetWorkingCard = targetCreatureModel.Card;

                BoardUnitView targetCreatureView = new BoardUnitView (targetCreatureModel, _testBroker.GetPlayerBoardGameObject (_opponent).transform);
                boardUnitView.Model.OwnerPlayer = targetWorkingCard.Owner;
                boardUnitView.SetObjectInfo (targetWorkingCard);

                fightTargetingArrow.OnCardSelected (targetCreatureView);
            }
            else
                Debug.LogWarning ("3");

            yield return LetsThink ();

            fightTargetingArrow.End (boardUnitView);
            _playerController.IsCardSelected = false;
        }
        else
        {
            Debug.LogWarning ("4");

            BoardObject target = null;
            if (targetPlayer != null)
            {
                target = targetPlayer;
            }
            else
            {
                target = targetCreatureModel;
            }

            boardUnitView.Model.DoCombat (target);
        }

        yield return null;
    }

    private void PlayCardCompleteHandler (WorkingCard card, BoardObject target)
    {
        WorkingCard workingCard = null;

        if (_gameplayManager.OpponentPlayer.CardsOnBoard.Count > 0)
        {
            workingCard = _gameplayManager.OpponentPlayer.CardsOnBoard[_gameplayManager.OpponentPlayer.CardsOnBoard.Count - 1];
        }

        if (workingCard == null || card == null)
            return;

        switch (card.LibraryCard.CardKind)
        {
            case Enumerators.CardKind.CREATURE:
                {
                    BoardUnitView boardUnitViewElement = new BoardUnitView (new BoardUnitModel (), GameObject.Find ("OpponentBoard").transform);
                    GameObject boardUnit = boardUnitViewElement.GameObject;
                    boardUnit.tag = SRTags.OpponentOwned;
                    boardUnit.transform.position = Vector3.zero;
                    boardUnitViewElement.Model.OwnerPlayer = card.Owner;

                    boardUnitViewElement.SetObjectInfo (workingCard);
                    _battlegroundController.OpponentBoardCards.Add (boardUnitViewElement);

                    boardUnit.transform.position +=
                        Vector3.up * 2f; // Start pos before moving cards to the opponents board

                    _gameplayManager.OpponentPlayer.BoardCards.Add (boardUnitViewElement);

                    _actionsQueueController.PostGameActionReport (new PastActionsPopup.PastActionParam ()
                    {
                        ActionType = Enumerators.ActionType.PlayCardFromHand,
                        Caller = boardUnitViewElement.Model,
                        TargetEffects = new List<PastActionsPopup.TargetEffectParam> ()
                    });

                    boardUnitViewElement.PlayArrivalAnimation ();

                    _abilitiesController.ResolveAllAbilitiesOnUnit (boardUnitViewElement.Model, false);

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
                                    _abilitiesController.CallAbility (card.LibraryCard, null, workingCard, Enumerators.CardKind.CREATURE, boardUnitViewElement.Model, null, false, null, _callAbilityAction, target);
                                };

                                _boardArrowController.DoAutoTargetingArrowFromTo<OpponentBoardArrow> (boardUnit.transform, target, action: callback);
                            }
                            else
                            {
                                _abilitiesController.CallAbility (card.LibraryCard, null, workingCard,
                                    Enumerators.CardKind.CREATURE, boardUnitViewElement.Model, null, false, null, _callAbilityAction);
                            }
                        });
                    break;
                }
            case Enumerators.CardKind.SPELL:
                {
                    GameObject spellCard = UnityEngine.Object.Instantiate (_cardsController.ItemCardViewPrefab);
                    spellCard.transform.position = GameObject.Find ("OpponentSpellsPivot").transform.position;

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
                            _abilitiesController.CallAbility (card.LibraryCard, null, workingCard, Enumerators.CardKind.SPELL, boardSpell, null, false, null, _callAbilityAction, target);
                        };

                        _boardArrowController.DoAutoTargetingArrowFromTo<OpponentBoardArrow> (_gameplayManager.OpponentPlayer.AvatarObject.transform, target, action: callback);
                    }
                    else
                    {
                        _abilitiesController.CallAbility (card.LibraryCard, null, workingCard, Enumerators.CardKind.SPELL, boardSpell, null, false, null, _callAbilityAction);
                    }

                    break;
                }
        }
    }

    private BoardObject GetAbilityTarget (WorkingCard card)
    {
        Loom.ZombieBattleground.Data.Card libraryCard = card.LibraryCard;

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
                        if (_testBroker.GetPlayer (_opponent).BoardCards.Count > 1 ||
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
            target = _testBroker.GetPlayer (_opponent);
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

    private List<BoardUnitView> GetHeavyUnitsOnBoard (Loom.ZombieBattleground.Player player)
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

        List<Loom.ZombieBattleground.Data.Card> cards = new List<Loom.ZombieBattleground.Data.Card> ();

        foreach (WorkingCard item in list)
        {
            cards.Add (_dataManager.CachedCardsLibraryData.GetCard (item.CardId));
        }

        cards = cards.OrderBy (x => x.Cost).ThenBy (y => y.Cost.ToString ().Length).ToList ();

        List<WorkingCard> sortedList = new List<WorkingCard> ();

        cards.Reverse ();

        foreach (Loom.ZombieBattleground.Data.Card item in cards)
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

    // todo: review
    public void DoBoardSkill (BoardSkill skill, BoardObject overrideTarget = null, Enumerators.AffectObjectType selectedTargetType = Enumerators.AffectObjectType.None)
    {
        if (overrideTarget != null)
        {
            skill.StartDoSkill ();

            switch (selectedTargetType)
            {
                case Enumerators.AffectObjectType.Player:
                    skill.FightTargetingArrow.SelectedPlayer = (Loom.ZombieBattleground.Player) overrideTarget;

                    Debug.Log ("Board skill: Player");

                    break;
                case Enumerators.AffectObjectType.Character:
                    BoardUnitView selectedCardView = _battlegroundController.GetBoardUnitViewByModel ((BoardUnitModel)overrideTarget);
                    skill.FightTargetingArrow.SelectedCard = selectedCardView;

                    Debug.Log ("Board skill: Character");

                    break;
            }

            _boardArrowController.DoAutoTargetingArrowFromTo<OpponentBoardArrow> (skill.SelfObject.transform, overrideTarget);

            _skillsController.DoSkillAction (skill, null, overrideTarget);
            skill.EndDoSkill ();

            return;
        }

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
                skill.FightTargetingArrow.SelectedPlayer = (Loom.ZombieBattleground.Player)target;

                Debug.Log ("Board skill: Player");

                break;
            case Enumerators.AffectObjectType.Character:
                BoardUnitView selectedCardView = _battlegroundController.GetBoardUnitViewByModel ((BoardUnitModel)target);
                skill.FightTargetingArrow.SelectedCard = selectedCardView;

                Debug.Log ("Board skill: Character");

                break;
            case Enumerators.AffectObjectType.None:
                Debug.Log ("Board skill: None");

                break;
            default:
                throw new ArgumentOutOfRangeException (nameof (selectedObjectType), selectedObjectType, null);
        }

        // _boardArrowController.DoAutoTargetingArrowFromTo<OpponentBoardArrow> (skill.SelfObject.transform, target);

        // todo fix this
        if (overrideTarget != null)
        {
            _skillsController.DoSkillAction (skill, null, overrideTarget);
        }
        else
        {
            _skillsController.DoSkillAction (skill, null, target);
        }

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

    // Some thinking - delay between general actions
    public IEnumerator LetsThink ()
    {
        yield return new WaitForSeconds (1.1f);
    }

    public IEnumerator WaitUntilPlayerOrderIsDecided ()
    {
        yield return new WaitUntil (() => GameObject.Find ("PlayerOrderPopup(Clone)") != null);

        yield return new WaitUntil (() => GameObject.Find ("PlayerOrderPopup(Clone)") == null);

        yield return null;
    }

    public IEnumerator DecideWhichCardsToPick ()
    {
        CardsController cardsController = _gameplayManager.GetController<CardsController> ();

        int highCardCounter = 0;

        Loom.ZombieBattleground.Player currentPlayer = _gameplayManager.CurrentPlayer;
        for (int i = currentPlayer.CardsPreparingToHand.Count - 1; i >= 0; i--)
        {
            BoardCard boardCard = currentPlayer.CardsPreparingToHand[i];

            if ((boardCard.LibraryCard.CardKind == Enumerators.CardKind.SPELL) ||
                (highCardCounter >= 1 && boardCard.LibraryCard.Cost >= 4) ||
                boardCard.LibraryCard.Cost >= 8)
            {
                currentPlayer.CardsPreparingToHand[i].CardShouldBeChanged = !boardCard.CardShouldBeChanged;

                yield return LetsThink ();
            }
            else if (boardCard.LibraryCard.Cost >= 4)
            {
                highCardCounter++;

                yield return LetsThink ();
            }
        }

        yield return LetsThink ();
        yield return LetsThink ();

        yield return ClickGenericButton ("Button_Keep");
    }

    public IEnumerator EndTurn ()
    {
        _battlegroundController.StopTurn ();
        GameObject.Find ("_1_btn_endturn").GetComponent<EndTurnButton> ().SetEnabled (false);

        Debug.Log ("Ended turn");

        yield return null;
    }

    public IEnumerator WaitUntilOurFirstTurn ()
    {
        if (_gameplayManager.CurrentTurnPlayer.Id == _gameplayManager.CurrentPlayer.Id)
        {
            yield return null;
        }
        else
        {
            yield return WaitUntilOurTurnStarts ();

            yield return WaitUntilInputIsUnblocked ();
        }

        yield return LetsThink ();
    }

    public IEnumerator WaitUntilAIBrainStops ()
    {
        yield return new WaitUntil (() => _gameplayManager.GetController<AIController> ().IsBrainWorking == false);
    }

    public IEnumerator WaitUntilOurTurnStarts ()
    {
        yield return new WaitUntil (() => IsGameEnded () || GameObject.Find ("YourTurnPopup(Clone)") != null);

        yield return new WaitUntil (() => IsGameEnded () || GameObject.Find ("YourTurnPopup(Clone)") == null);
    }

    public IEnumerator WaitUntilInputIsUnblocked ()
    {
        yield return new WaitUntil (() => IsGameEnded () || _gameplayManager.IsLocalPlayerTurn ());
    }

    // todo: reconsider having this
    public IEnumerator UseSkillToOpponentPlayer ()
    {
        DoBoardSkill (_testBroker.GetPlayerPrimarySkill (_player), _testBroker.GetPlayer (_opponent), Enumerators.AffectObjectType.Player);

        yield return LetsThink ();
        yield return LetsThink ();

        yield return null;
    }

    // todo: reconsider having this
    public IEnumerator PlayNonSleepingCardsFromBoardToOpponentPlayer ()
    {
        foreach (BoardUnitView boardUnitView in _battlegroundController.PlayerBoardCards)
        {
            if (boardUnitView.Model.IsPlayable)
            {
                boardUnitView.Model.DoCombat (_gameplayManager.OpponentPlayer);

                yield return LetsThink ();
                yield return LetsThink ();
            }

            yield return null;
        }

        yield return null;
    }

    // todo: reconsider having this
    public IEnumerator WaitUntilCardIsAddedToBoard (string boardName)
    {
        Transform boardTransform = GameObject.Find (boardName).transform;
        int boardChildrenCount = boardTransform.childCount;

        yield return new WaitUntil (() => (boardChildrenCount < boardTransform.childCount) && (boardChildrenCount < _battlegroundController.OpponentBoardCards.Count));
    }

    public IEnumerator MakeMoves ()
    {
        // if it doesn't end in 100 moves, end the game anyway
        for (int turns = 1; turns <= 100; turns++)
        {
            yield return TurnStartedHandler ();

            TurnEndedHandler ();

            if (IsGameEnded ())
                yield break;

            if (IsGameEnded ())
                break;

            yield return EndTurn ();

            if (IsGameEnded ())
                break;

            yield return WaitUntilOurTurnStarts ();

            if (IsGameEnded ())
                break;

            yield return WaitUntilInputIsUnblocked ();

            if (IsGameEnded ())
                break;
        }
    }

    public bool IsGameEnded ()
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

    #region Horde Creation / Editing

    private CollectionData _collectionData;
    private List<Loom.ZombieBattleground.Data.Card> _createdArmyCards, _createdHordeCards;

    private List<string> _overlordNames = new List<string> () {
        "Brakuus",
        "Razu",
        "Vash'Kala",
        "Kalile",
        "Mhalik",
        "Valash"
    };

    public IEnumerator AddRazuHorde ()
    {
        yield return ClickGenericButton ("Image_BaackgroundGeneral");

        yield return AssertCurrentPageName ("OverlordSelectionPage");

        yield return PickOverlord ("Razu", true);

        yield return PickOverlordAbility (1);

        yield return ClickGenericButton ("Canvas_BackLayer/Button_Continue");

        yield return AssertCurrentPageName ("HordeEditingPage");

        SetupArmyCards ();

        yield return SetDeckTitle ("Razu Deck");

        yield return AddCardToHorde ("Pyromaz");
        yield return AddCardToHorde ("Pyromaz");
        yield return AddCardToHorde ("Pyromaz");
        yield return AddCardToHorde ("Pyromaz");
        // yield return AddCardToHorde ("Pyromaz", true);

        yield return AddCardToHorde ("Quazi");
        yield return AddCardToHorde ("Quazi");
        yield return AddCardToHorde ("Quazi");
        yield return AddCardToHorde ("Quazi");

        yield return AddCardToHorde ("Ember");
        yield return AddCardToHorde ("Ember");
        yield return AddCardToHorde ("Ember");
        yield return AddCardToHorde ("Ember");

        yield return AddCardToHorde ("Firewall");
        yield return AddCardToHorde ("Firewall");
        yield return AddCardToHorde ("Firewall");
        yield return AddCardToHorde ("Firewall");

        yield return AddCardToHorde ("BurZt");
        yield return AddCardToHorde ("BurZt");
        yield return AddCardToHorde ("BurZt");
        yield return AddCardToHorde ("BurZt");

        yield return ClickGenericButton ("Army/ArrowRightButton");

        yield return LetsThink ();

        SetupArmyCards ();

        yield return LetsThink ();

        yield return AddCardToHorde ("BlaZter");

        yield return ClickGenericButton ("Horde/ArrowRightButton");

        yield return AddCardToHorde ("BlaZter");
        yield return AddCardToHorde ("BlaZter");
        yield return AddCardToHorde ("BlaZter");

        yield return AddCardToHorde ("Burrrnn");
        yield return AddCardToHorde ("Burrrnn");

        yield return AddCardToHorde ("Cynderman");
        yield return AddCardToHorde ("Cynderman");

        yield return AddCardToHorde ("Werezomb");
        yield return AddCardToHorde ("Werezomb");

        yield return ClickGenericButton ("Button_Save");
    }

    public void SetupArmyCards ()
    {
        FillCollectionData ();

        DeckBuilderCard[] deckBuilderCards = GameObject.FindObjectsOfType<DeckBuilderCard> ();

        if (deckBuilderCards == null || deckBuilderCards.Length == 0)
            return;

        _createdArmyCards = new List<Loom.ZombieBattleground.Data.Card> ();
        foreach (DeckBuilderCard deckBuilderCard in deckBuilderCards)
        {
            _createdArmyCards.Add (deckBuilderCard.Card);
        }
    }

    public IEnumerator PickOverlord (string overlordName, bool goRight = true)
    {
        int selectedIndex = 0;

        while (_overlordNames[selectedIndex] != overlordName)
        {
            if (goRight)
            {
                yield return ClickGenericButton ("Button_RightArrow");

                selectedIndex = (selectedIndex + 1) % _overlordNames.Count;
            }
            else
            {
                yield return ClickGenericButton ("Button_LeftArrow");

                selectedIndex = (selectedIndex + 6 - 1) % _overlordNames.Count;
            }

            yield return LetsThink ();
            yield return LetsThink ();
        }

        yield return ClickGenericButton ("Button_Continue");

        yield return null;
    }

    public IEnumerator PickOverlordAbility (int index)
    {
        GameObject abilitiesParent = GameObject.Find ("Abilities");

        if (index >= abilitiesParent.transform.childCount)
        {
            Assert.Fail ("Index higher than number of abilities");
        }

        abilitiesParent.transform.GetChild (index).gameObject.GetComponent<Toggle> ().isOn = true;

        yield return LetsThink ();
    }

    public IEnumerator SetDeckTitle (string deckTitle)
    {
        GameObject deckTitleInput = GameObject.Find ("DeckTitleInputText");

        if (deckTitleInput == null)
        {
            Assert.Fail ("DeckTitleInputText doesn't exist");
        }

        TMP_InputField deckTitleInputField = deckTitleInput.GetComponent<TMP_InputField> ();

        if (deckTitleInputField == null)
        {
            Assert.Fail ("TextMeshPro InputField doesn't exist");
        }

        deckTitleInputField.onEndEdit.Invoke (deckTitle);

        yield return LetsThink ();
    }

    public IEnumerator AddCardToHorde (string cardName, bool overQuota = false)
    {
        Loom.ZombieBattleground.Data.Card armyCard = _createdArmyCards.Find (x =>
                             x.Name == cardName);

        Debug.Log ("Adding " + cardName + " (" + armyCard.Cost + ")");

        _uiManager.GetPage<HordeEditingPage> ().AddCardToDeck (null, armyCard);

        yield return LetsThink ();

        if (overQuota)
        {
            ClickGenericButton ("Button_GotIt");

            yield return LetsThink ();
        }

        yield return null;
    }

    public IEnumerator SelectAHorde (int index)
    {
        GameObject hordesParent = GameObject.Find ("Panel_DecksContainer/Group");

        if (index + 1 >= hordesParent.transform.childCount)
        {
            Assert.Fail ("Horde removal index is too high");
        }

        Transform removalHordeTransform = hordesParent.transform.GetChild (index);
        removalHordeTransform.Find ("Button_Select").GetComponent<Button> ().onClick.Invoke ();

        yield return LetsThink ();
    }

    public IEnumerator RemoveAHorde (int index)
    {
        yield return SelectAHorde (index);

        GameObject.Find ("Button_Delete").GetComponent<Button> ().onClick.Invoke ();

        yield return LetsThink ();
    }

    private void FillCollectionData ()
    {
        _collectionData = new CollectionData ();
        _collectionData.Cards = new List<CollectionCardData> ();

        _collectionData.Cards.Clear ();
        CollectionCardData cardData;
        foreach (CollectionCardData card in _dataManager.CachedCollectionData.Cards)
        {
            cardData = new CollectionCardData ();
            cardData.Amount = card.Amount;
            cardData.CardName = card.CardName;

            _collectionData.Cards.Add (cardData);
        }
    }

    #endregion

    private string _overlordName;

    public void RecordOverlordName ()
    {
        // implement this
    }

    public void AssertOverlordName ()
    {
        string overlordName = ""; // implement this

        if (_overlordName != overlordName)
        {
            Assert.Fail ("Overlord in the game is different than the one selected.");
        }
    }
}
