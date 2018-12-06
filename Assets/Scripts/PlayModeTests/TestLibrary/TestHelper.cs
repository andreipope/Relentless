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
using UnityEngine.UI;
using TMPro;
using UnityEngine.TestTools;

public class TestHelper
{
    /// <summary>
    /// To be in line with AI Brain, 1.1f was taken as value from AIController.
    /// </summary>
    private const float DefaultThinkTime = 0.1f;

    /// <summary>
    /// Delay between main menu transition clicks.
    /// </summary>
    private const float DefaultMainMenuTransitionDelay = 0f;

    private enum TesterType
    {
        Active,
        Passive
    }

    private List<string> _testerKeys = new List<string> {
        "1b84dd3a623c",
        "1e0575c58ee5"
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
    private GameObject _fakeCursorGameObject;

    private string _testName;
    private float _testStartTime;

    private string lastCheckedPageName;

    private TestBroker _testBroker;
    private Enumerators.MatchPlayer _player;
    private Enumerators.MatchPlayer _opponent;

    private float _positionalTolerance = 0.1f;

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

    GameplayQueueAction<object> _callAbilityAction;

    private Loom.ZombieBattleground.Player _currentPlayer, _opponentPlayer;

    private int pageTransitionWaitTime = 30;

    private string _recordedExpectedValue, _recordedActualValue;

    private float _waitStartTime, _turnStartTime;
    private float _waitAmount, _turnWaitAmount;

    private const int MinTurnForAttack = 0;
    public BoardCard CurrentSpellCard;

    private readonly System.Random _random = new System.Random();

    private List<BoardUnitModel> _attackedUnitTargets;
    private List<BoardUnitModel> _unitsToIgnoreThisTurn;

    private List<WorkingCard> _normalUnitCardInHand, _normalSpellCardInHand;

    private List<Loom.ZombieBattleground.Data.Card> _createdArmyCards;

    private List<string> _overlordNames = new List<string>() {
        "Brakuus",
        "Razu",
        "Vash'Kala",
        "Kalile",
        "Mhalik",
        "Valash"
    };

    private string _currentElementName = "";

    public int SelectedHordeIndex
    {
        get;
        private set;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="T:TestHelper"/> class.
    /// </summary>
    /// <param name="testerType">Tester type.</param>
    public TestHelper (int testerType = 0)
    {
        _testerType = (TesterType) testerType;
    }

    /// <summary>
    /// Sets the name of the test. It is used to log the result (time taken) at the end of the test.
    /// </summary>
    /// <param name="testName">Test name.</param>
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

    /// <summary>
    /// SetUp method to be used for most Solo and PvP tests. Logs in and sets up a number of stuff.
    /// </summary>
    public IEnumerator SetUp ()
    {
        // HACK: Unity sometimes log an harmless internal assert, but the testing framework trips on it
        LogAssert.ignoreFailingMessages = true;

        _testStartTime = Time.unscaledTime;

        if (!_initialized)
        {
            _testerKey = _testerKeys[(int) _testerType];

            _testScene = SceneManager.GetActiveScene ();
            _testerGameObject = _testScene.GetRootGameObjects ()[0];
            _testerGameObject.AddComponent<TestScriptProtector> ();

            yield return SceneManager.LoadSceneAsync ("APP_INIT", LoadSceneMode.Single);

            // RemoveGoogleAnalyticsModule ();

            yield return AddVirtualInputModule ();

            yield return SetCanvases ();

            #region Login

            yield return HandleLogin ();

            yield return LetsThink ();

            yield return AssertLoggedInOrLoginFailed (
                CloseTermsPopupIfRequired (),
                FailWithMessage ("Wasn't able to login. Try using USE_STAGING_BACKEND"));

            #endregion

            SetGameplayManagers ();

            _initialized = true;

            yield return LetsThink ();
        }

        yield return null;
    }

    /// <summary>
    /// TearDown method to be used to clear up everything after either a successful or unsuccessful test.
    /// </summary>
    /// <remarks>Generally is used only for the last test in the group.</remarks>
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

    /// <summary>
    /// TearDown method to be used to go back to MainMenuPage, so that other tests can take it from there and go further.
    /// </summary>
    /// <remarks>Generally is used for all tests in the group, except for the last one (where actual cleanup happens).</remarks>
    public IEnumerator TearDown_GoBackToMainScreen ()
    {
        while (lastCheckedPageName != "MainMenuPage")
        {
            yield return GoOnePageHigher ();

            yield return LetsThink ();
        }

        yield return null;
    }

    /// <summary>
    /// Goes one page higher in the page hierarchy, towards MainMenuPage.
    /// </summary>
    /// <remarks>Generally we need a number of these to actually get to the MainMenuPage.</remarks>
    public IEnumerator GoOnePageHigher ()
    {
        yield return new WaitUntil (() =>
        {
            if (canvas1GameObject != null && canvas1GameObject.transform.childCount >= 2)
            {
                return true;
            }

            return false;
        });
        string actualPageName = canvas1GameObject.transform.GetChild (1).name.Split ('(')[0];

        yield return AssertCurrentPageName (actualPageName);

        yield return LetsThink ();

        switch (actualPageName)
        {
            case "GameplayPage":
                if (GameObject.Find ("Button_Back") != null)
                {
                    yield return ClickGenericButton ("Button_Back");

                    yield return LetsThink ();

                    yield return RespondToYesNoOverlay (true);

                    yield return AssertCurrentPageName ("MainMenuPage");
                }
                else if (GameObject.Find ("Button_Settings") != null)
                {
                    yield return ClickGenericButton ("Button_Settings");

                    yield return LetsThink ();

                    yield return ClickGenericButton ("Button_QuitToMainMenu");

                    yield return LetsThink ();

                    yield return RespondToYesNoOverlay (true);

                    yield return AssertCurrentPageName ("MainMenuPage");
                }

                break;
            case "HordeSelectionPage":
                yield return MainMenuTransition ("Button_Back");

                yield return AssertCurrentPageName ("PlaySelectionPage");

                break;
            case "PlaySelectionPage":
                yield return MainMenuTransition ("Button_Back");

                yield return AssertCurrentPageName ("MainMenuPage");

                break;
            case "MainMenuPage":

                yield break;
            default:
                throw new ArgumentException ("Unhandled page: " + actualPageName);
        }

        yield return null;
    }

    /// <summary>
    /// Gets the time since the start of the test.
    /// </summary>
    public float GetTestTime ()
    {
        return Time.unscaledTime - _testStartTime;
    }

    /// <summary>
    /// Reports the test time.
    /// </summary>
    /// <remarks>Generally is used at the of the test, to report the time it took to run it.</remarks>
    public IEnumerator ReportTestTime ()
    {
        Debug.LogWarningFormat (
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

    /// <summary>
    /// Asserts if we've logged in or login failed, so that the test doesn't get stuck in the login screen in the case of issue and instead reports the issue.
    /// </summary>
    public IEnumerator AssertLoggedInOrLoginFailed (IEnumerator callback1, IEnumerator callback2)
    {
        yield return CombinedCheck (
            CheckCurrentPageName, "MainMenuPage", callback1,
            CheckIfLoginErrorOccured, "", callback2);

        yield return null;
    }

    /// <summary>
    /// Asserts if we were sent to tutorial. This is used to get out of tutorial, so that test can go on with its purpose.
    /// </summary>
    public IEnumerator AssertIfWentDirectlyToTutorial (IEnumerator callback1, IEnumerator callback2 = null)
    {
        yield return CombinedCheck (
            CheckCurrentPageName, "GameplayPage", callback1,
            CheckCurrentPageName, "PlaySelectionPage", callback2);
    }

    // @todo: Get this to working using an artificial timeout
    /// <summary>
    /// Asserts if PvP match is started or matchmaking has failed.
    /// </summary>
    /// <remarks>This currently doesn't work, as timeouts have been removed.</remarks>
    public IEnumerator AssertPvPStartedOrMatchmakingFailed (IEnumerator callback1, IEnumerator callback2)
    {
        WaitStart (60);

        yield return CombinedCheck (
            CheckCurrentPageName, "GameplayPage", callback1,
            // CheckIfMatchmakingErrorOccured, "", callback2);
            WaitTimeIsUp, "", callback2);

        yield return null;
    }

    public IEnumerator AssertMulliganPopupCameUp (IEnumerator callback1, IEnumerator callback2)
    {
        WaitStart (5);

        yield return CombinedCheck (
            CheckIfMulliganPopupCameUp, "", callback1,
            WaitTimeIsUp, "", callback2);
    }

    /// <summary>
    /// Is used whenever we need a combined check, instead of a single one.
    /// </summary>
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

    /// <summary>
    /// Checks if login box appeared.
    /// </summary>
    /// <returns><c>true</c>, if if login box appeared was checked, <c>false</c> otherwise.</returns>
    private bool CheckIfLoginBoxAppeared (string dummyparameter)
    {
        GameObject loginBox = GameObject.Find ("InputField_Beta");

        if (loginBox != null && loginBox.activeInHierarchy)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Checks if login error occured.
    /// </summary>
    /// <returns><c>true</c>, if if login error occured, <c>false</c> otherwise.</returns>
    private bool CheckIfLoginErrorOccured (string dummyParameter)
    {
        GameObject errorTextObject = GameObject.Find ("Beta_Group/Text_Error");

        if (errorTextObject != null && errorTextObject.activeInHierarchy)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Checks if matchmaking error occured.
    /// </summary>
    /// <returns><c>true</c>, if if matchmaking error (e.g. timeout) occured, <c>false</c> otherwise.</returns>
    private bool CheckIfMatchmakingErrorOccured (string dummyParameter)
    {
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

    private bool CheckIfMulliganPopupCameUp (string dummyParameter)
    {
        if (GameObject.Find ("MulliganPopup(Clone)") != null)
            return true;

        return false;
    }

    /// <summary>
    /// In case Terms Popup shows up, the method closes it.
    /// </summary>
    public IEnumerator CloseTermsPopupIfRequired ()
    {
        if (GameObject.Find ("TermsPopup(Clone)") != null)
        {
            if (GameObject.Find ("Toggle")?.GetComponent<Toggle> () != null)
            {
                GameObject.Find ("Toggle").GetComponent<Toggle> ().isOn = true;
                GameObject.Find ("Toggle").GetComponent<Toggle> ().onValueChanged.Invoke (true);
            }

            yield return LetsThink ();

            yield return ClickGenericButton ("Button_GotIt");
        }
    }

    /// <summary>
    /// Checks if the name of the current page is as expected.
    /// </summary>
    /// <returns><c>true</c>, if current page name is as expected, <c>false</c> otherwise.</returns>
    /// <param name="expectedPageName">Expected page name.</param>
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
                if (canvas1GameObject.transform.GetChild (1).name.Split ('(')[0] == expectedPageName)
                    return true;

                return false;
            }
        }

        return false;
    }

    /// <summary>
    /// Checks current page’s name and confirms that it’s correct with what was expected.
    /// </summary>
    /// <remarks>
    /// In case we decide to use this, we need to use it for every page. Using it for just a single one may not work as expected.
    /// </remarks>
    /// <example>
    /// yield return AssertCurrentPageName ("MainMenuPage");
    /// </example>
    /// <param name="expectedPageName">Page name</param>
    public IEnumerator AssertCurrentPageName (string expectedPageName, string errorTextName = "")
    {
        if (expectedPageName == lastCheckedPageName)
            yield break;

        WaitStart (pageTransitionWaitTime);
        bool transitionTimeout = false;

        GameObject errorTextObject = null;
        yield return new WaitUntil (() => {
            if (WaitTimeIsUp ())
            {
                transitionTimeout = true;

                return true;
            }

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

        if (transitionTimeout)
        {
           yield return FailWithMessage ($"Page transition took too long from {lastCheckedPageName} to {expectedPageName}");
        }

        string actualPageName = canvas1GameObject.transform.GetChild (1).name.Split ('(')[0];

        Assert.AreEqual (expectedPageName, actualPageName);

        lastCheckedPageName = actualPageName;

        yield return null;
    }

    /// <summary>
    /// Goes back by one page and clicks on "Play" button.
    /// </summary>
    /// <remarks>
    /// Used when tutorial is shown instead of letting the script to test what it is meant for.
    /// </remarks>
    public IEnumerator GoBackToMainAndPressPlay ()
    {
        yield return GoOnePageHigher ();

        yield return MainMenuTransition ("Button_Play");
    }

    /// <summary>
    /// Adds virtual input module to the scene to handle fake mouse movements and clicks.
    /// </summary>
    /// <returns></returns>
    public IEnumerator AddVirtualInputModule ()
    {
        GameObject testSetup = GameObject.Instantiate (Resources.Load<GameObject> ("Prefabs/TestSetup"));
        _fakeCursorGameObject = testSetup.transform.Find ("Canvas/FakeCursor").gameObject;
        _fakeCursorTransform = _fakeCursorGameObject.GetComponent<RectTransform> ();
        Camera uiCamera = testSetup.transform.Find ("UI Camera").GetComponent<Camera> ();

        UnityEngine.EventSystems.StandaloneInputModule inputModule = GameObject.FindObjectOfType<UnityEngine.EventSystems.StandaloneInputModule> ();
        _virtualInputModule = inputModule.gameObject.AddComponent<UnityEngine.EventSystems.VirtualInputModule> ();
        inputModule.enabled = false;
        _virtualInputModule.SetLinks (_fakeCursorTransform, uiCamera);

        yield return null;
    }

    /// <summary>
    /// Moves cursor to the location of the object
    /// </summary>
    /// <param name="objectName">Name of the object in the scene</param>
    /// <param name="duration">Movement duration</param>
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

    /// <summary>
    /// Clicks using the virtual mouse cursor.
    /// </summary>
    /// <remarks>
    /// Useful only on UI items.
    /// </remarks>
    public IEnumerator FakeClick ()
    {
        _virtualInputModule.Press ();

        yield return null;

        _virtualInputModule.Release ();

        yield return null;
    }

    /// <summary>
    /// Goes through list of buttons and checks if they are clickable.
    /// </summary>
    /// <param name="buttonNames">Button names.</param>
    public IEnumerator ButtonListClickCheck (string[] buttonNames)
    {
        foreach (string buttonName in buttonNames)
        {
            yield return ButtonClickCheck (buttonName);

            yield return LetsThink ();

            yield return null;
        }

        yield return null;
    }

    /// <summary>
    /// Checks if a button is clickable.
    /// </summary>
    /// <param name="buttonName">Button name.</param>
    public IEnumerator ButtonClickCheck (string buttonName)
    {
        GameObject targetGameObject = GameObject.Find (buttonName);

        if (targetGameObject != null)
        {
            if (targetGameObject.GetComponent<ButtonShiftingContent> () != null)
            {
                ButtonShiftingContent targetButton = targetGameObject.GetComponent<ButtonShiftingContent> ();

                bool buttonClickable = false;

                Button dummyButton = new GameObject ().AddComponent<Button> ();

                dummyButton.onClick = targetButton.onClick;
                targetButton.onClick = new Button.ButtonClickedEvent ();
                targetButton.onClick.AddListener (() => { buttonClickable = true; });

                yield return null;

                yield return MoveCursorToObject (buttonName, 1);
                yield return FakeClick ();

                yield return null;

                WaitStart (3);
                yield return new WaitUntil (() => buttonClickable || WaitTimeIsUp ());

                if (!buttonClickable)
                {
                    targetButton.onClick = dummyButton.onClick;
                    dummyButton.onClick = new Button.ButtonClickedEvent ();
                    dummyButton.onClick.RemoveAllListeners ();

                    Assert.Fail ("Button is not clickable: " + buttonName);
                }
                else
                {
                    targetButton.onClick = dummyButton.onClick;
                    dummyButton.onClick = new Button.ButtonClickedEvent ();
                    dummyButton.onClick.RemoveAllListeners ();

                    Debug.Log ("Checked button and it worked fine: " + buttonName);
                }

                yield return null;
            }
            else if (targetGameObject.GetComponent<MenuButtonNoGlow> () != null)
            {
                MenuButtonNoGlow targetButton = targetGameObject.GetComponent<MenuButtonNoGlow> ();

                bool buttonClickable = false;

                MenuButtonNoGlow dummyButton = new GameObject ().AddComponent<MenuButtonNoGlow> ();

                dummyButton.Clicked = targetButton.Clicked;
                targetButton.Clicked = new UnityEngine.Events.UnityEvent ();
                targetButton.Clicked.AddListener (() => { buttonClickable = true; });

                yield return null;

                yield return MoveCursorToObject (buttonName, 1);
                yield return FakeClick ();

                yield return null;

                WaitStart (3);
                yield return new WaitUntil (() => buttonClickable || WaitTimeIsUp ());

                if (!buttonClickable)
                {
                    targetButton.Clicked = dummyButton.Clicked;
                    dummyButton.Clicked = new Button.ButtonClickedEvent ();
                    dummyButton.Clicked.RemoveAllListeners ();

                    Assert.Fail ("Button is not clickable: " + buttonName);
                }
                else
                {
                    targetButton.Clicked = dummyButton.Clicked;
                    dummyButton.Clicked = new Button.ButtonClickedEvent ();
                    dummyButton.Clicked.RemoveAllListeners ();

                    Debug.Log ("Checked button and it worked fine: " + buttonName);
                }

                yield return null;
            }
            else if (targetGameObject.GetComponent<Button> () != null)
            {
                Button targetButton = targetGameObject.GetComponent<Button> ();

                bool buttonClickable = false;

                Button dummyButton = new GameObject ().AddComponent<Button> ();

                dummyButton.onClick = targetButton.onClick;
                targetButton.onClick = new Button.ButtonClickedEvent ();
                targetButton.onClick.AddListener (() => { buttonClickable = true; });

                yield return null;

                yield return MoveCursorToObject (buttonName, 1);
                yield return FakeClick ();

                yield return null;

                WaitStart (3);
                yield return new WaitUntil (() => buttonClickable || WaitTimeIsUp ());

                if (!buttonClickable)
                {
                    targetButton.onClick = dummyButton.onClick;
                    dummyButton.onClick = new Button.ButtonClickedEvent ();
                    dummyButton.onClick.RemoveAllListeners ();

                    Assert.Fail ("Button is not clickable: " + buttonName);
                }
                else
                {
                    targetButton.onClick = dummyButton.onClick;
                    dummyButton.onClick = new Button.ButtonClickedEvent ();
                    dummyButton.onClick.RemoveAllListeners ();

                    Debug.Log ("Checked button and it worked fine: " + buttonName);
                }

                yield return null;
            }
        }
        else
        {
            Assert.Fail ("Button wasn't found: " + buttonName);
        }

        yield return null;
    }

    /// <summary>
    /// Logs in into the game using one of the keys. Picks a correct one depending on whether it is an passive or active tester.
    /// </summary>
    /// <remarks>The login.</remarks>
    public IEnumerator HandleLogin ()
    {
#if !(UNITY_EDITOR || DEVELOPMENT_BUILD)
        yield return AssertCurrentPageName ("LoadingPage");

        GameObject pressAnyText = null;
        yield return new WaitUntil (() => { pressAnyText = GameObject.Find ("PressAnyText"); return pressAnyText != null; });
        pressAnyText.SetActive (false);
        GameClient.Get<IUIManager> ().DrawPopup<LoginPopup> ();

        yield return CombinedCheck (
            CheckIfLoginBoxAppeared, "", null,
            CheckCurrentPageName, "MainMenuPage", null);
#endif

        yield return null;
    }

    /// <summary>
    /// (Deprecated) Submits the tester key.
    /// </summary>
    private IEnumerator SubmitTesterKey ()
    {
        InputField testerKeyField = null;
        yield return new WaitUntil (() => { testerKeyField = GameObject.Find ("InputField_Beta")?.GetComponent<InputField> (); return testerKeyField != null; });

        testerKeyField.text = _testerKey;
        GameObject.Find ("Button_Beta").GetComponent<ButtonShiftingContent> ().onClick.Invoke ();

        yield return null;
    }

    /// <summary>
    /// Takes name of the gameObject that has Button or ButtonShiftingContent component and clicks it.
    /// </summary>
    /// <param name="buttonName">Name of the button to click</param>
    /// <param name="parentGameObject">(Optional) Parent object to look under</param>
    /// <param name="count">(Optional) Number of times to click</param>
    public IEnumerator ClickGenericButton (string buttonName, GameObject parentGameObject = null, int count = 1)
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

        yield return LetsThink (0.5f);

        if (count >= 2)
        {
            yield return ClickGenericButton (buttonName, parentGameObject, count - 1);
        }

        yield return null;
    }

    /// <summary>
    /// Checks if the button exists.
    /// </summary>
    /// <returns><c>true</c>, if button exists, <c>false</c> otherwise.</returns>
    /// <param name="buttonName">Button name.</param>
    public bool IsButtonExist (string buttonName)
    {
        return GameObject.Find (buttonName) != null;
    }

    /// <summary>
    /// Takes a transition path (list of buttons to click) and goes through them clicking each.
    /// </summary>
    /// <param name="transitionPath">Slash separated list of buttons</param>
    /// <param name="delay">(Optional) Delay between clicks</param>
    public IEnumerator MainMenuTransition (string transitionPath, float delay = DefaultMainMenuTransitionDelay)
    {
        foreach (string buttonName in transitionPath.Split ('/'))
        {
            yield return ClickGenericButton (buttonName);

            if (delay <= 0f)
            {
                yield return new WaitForEndOfFrame();
                yield return new WaitForEndOfFrame();
            }
            else
            {
                yield return new WaitForSeconds (delay);
            }
        }
    }

    /// <summary>
    /// Clicks on the overlay Yes/No button.
    /// </summary>
    /// <param name="isResponseYes">Is the response Yes?</param>
    public IEnumerator RespondToYesNoOverlay (bool isResponseYes)
    {
        string buttonName = isResponseYes ? "Button_Yes" : "Button_No";

        ButtonShiftingContent overlayButton = null;
        yield return new WaitUntil (() => { overlayButton = GameObject.Find (buttonName)?.GetComponent<ButtonShiftingContent> (); return overlayButton != null; });

        overlayButton.onClick.Invoke ();

        yield return null;
    }

    /// <summary>
    /// Waits until a page unloads.
    /// </summary>
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

    /// <summary>
    /// Sets tags to be used by the matchmaking system.
    /// </summary>
    /// <param name="tags">Tags</param>
    public void SetPvPTags (string[] tags)
    {
        if (tags == null || tags.Length <= 0)
        {
            _pvpManager.PvPTags = null;

            return;
        }

        _pvpManager.PvPTags = new List<string> ();
        foreach (string tag in tags)
        {
            _pvpManager.PvPTags.Add (tag);
        }
    }

    #endregion

    #region Adapted from AIController

    /// <summary>
    /// Initalizes the player.
    /// </summary>
    /// <remarks>Created to be in line with AIController.</remarks>
    public void InitalizePlayer ()
    {
        _attackedUnitTargets = new List<BoardUnitModel> ();
        _unitsToIgnoreThisTurn = new List<BoardUnitModel> ();

        _normalUnitCardInHand = new List<WorkingCard> ();
        _normalSpellCardInHand = new List<WorkingCard> ();

        _callAbilityAction = null; // _actionsQueueController.AddNewActionInToQueue (null);
    }

    /// <summary>
    /// Once the turn is started, goes through (AI) steps, to make logical moves.
    /// </summary>
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

    /// <summary>
    /// Once turn ends, clears up lists, to be used in next turn.
    /// </summary>
    public void TurnEndedHandler ()
    {
        _attackedUnitTargets.Clear ();
        _unitsToIgnoreThisTurn.Clear ();
    }

    /// <summary>
    /// AI step 1: Plays cards from hand to board
    /// </summary>
    /// <remarks>Logic taken from AIController.</remarks>
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
                yield return PlayCardFromHandToBoard (card);
                wasAction = true;
                yield return LetsThink ();
                yield return LetsThink ();
            }
        }

        foreach (WorkingCard card in _normalSpellCardInHand)
        {
            if (CardCanBePlayable (card) && CheckSpecialCardRules (card))
            {
                yield return PlayCardFromHandToBoard (card);
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
    /// <summary>
    /// AI step 2: Plays cards from board
    /// </summary>
    /// <remarks>Logic taken from AIController.</remarks>
    private IEnumerator UseUnitsOnBoard ()
    {
        List<BoardUnitModel> unitsOnBoard = new List<BoardUnitModel> ();
        List<BoardUnitModel> alreadyUsedUnits = new List<BoardUnitModel> ();

        if (IsGameEnded ())
            yield break;

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
                                Enumerators.AffectObjectType.Character,
                                attackedUnit.Card.InstanceId);

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
                        Enumerators.AffectObjectType.Player,
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
                            Enumerators.AffectObjectType.Player,
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
                                Enumerators.AffectObjectType.Character,
                                attackedCreature.Card.InstanceId);

                            yield return LetsThink ();
                        }
                        else
                        {
                            unit.DoCombat (_testBroker.GetPlayer (_opponent));
                            // PlayCardFromBoard (unit, _testBroker.GetPlayer (_opponent), null);

                            unit.OwnerPlayer.ThrowCardAttacked (
                                unit.Card,
                                Enumerators.AffectObjectType.Player,
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
    /// <summary>
    /// AI step 3: Uses player skills
    /// </summary>
    /// <remarks>Logic taken from AIController.</remarks>
    private IEnumerator UsePlayerSkills ()
    {
        bool wasAction = false;

        if (_testBroker.GetPlayer (_player).IsStunned)
            yield break;

        if (_testBroker.GetPlayerPrimarySkill (_player) != null && _testBroker.GetPlayerPrimarySkill (_player).IsSkillReady)
        {
            DoBoardSkill (_testBroker.GetPlayerPrimarySkill (_player));
            wasAction = true;
        }

        if (wasAction)
        {
            yield return LetsThink ();
        }

        wasAction = false;
        if (_testBroker.GetPlayerSecondarySkill (_player) != null && _testBroker.GetPlayerSecondarySkill (_player).IsSkillReady)
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
                    yield return PlayCardFromHandToBoard (card);
                    wasAction = true;
                    yield return LetsThink ();
                    yield return LetsThink ();
                }
            }

            yield return PlayCardFromHandToBoard (expensiveCard);

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

    /// <summary>
    /// Playes cards with specified indices from hand to board.
    /// </summary>
    /// <remarks>Was written specifically for tutorials, where we need to play cards with certain indices.</remarks>
    /// <param name="cardIndices">Card indices.</param>
    public IEnumerator PlayCardFromHandToBoard (int[] cardIndices)
    {
        foreach (int cardIndex in cardIndices)
        {
            BoardCard boardCard = _battlegroundController.PlayerHandCards[cardIndex];

            yield return PlayCardFromHandToBoard (boardCard.WorkingCard);

            yield return LetsThink ();
            yield return LetsThink ();
        }

        yield return null;
    }

    private IEnumerator PlayCardFromHandToBoard (WorkingCard card)
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

        Debug.LogWarning ("Target: " + ((target != null) ? target.Id.ToString () : "Null") + ", Need target: " + needTargetForAbility);

        switch (card.LibraryCard.CardKind)
        {
            case Enumerators.CardKind.CREATURE when _testBroker.GetBoardCards (_player).Count < _gameplayManager.OpponentPlayer.MaxCardsInPlay:
                if (_player == Enumerators.MatchPlayer.CurrentPlayer)
                {
                    BoardCard boardCard = _battlegroundController.PlayerHandCards.Find (x => x.WorkingCard.Equals (card));

                    _cardsController.PlayPlayerCard (_testBroker.GetPlayer (_player), boardCard, boardCard.HandBoardCard, PlayCardOnBoard => {
                        PlayerMove playerMove = new PlayerMove (Enumerators.PlayerActionType.PlayCardOnBoard, PlayCardOnBoard);
                        _gameplayManager.PlayerMoves.AddPlayerMove (playerMove);
                    }, target);

                    yield return null;

                    if (target == null && needTargetForAbility)
                    {
                        WaitStart (3);
                        yield return new WaitUntil (() => _boardArrowController.CurrentBoardArrow != null || WaitTimeIsUp ());
                        _boardArrowController.ResetCurrentBoardArrow ();

                        yield return LetsThink ();

                        WaitStart (3);
                        yield return new WaitUntil (() => _abilitiesController.CurrentActiveAbility != null || WaitTimeIsUp ());
                        _abilitiesController.CurrentActiveAbility.Ability.SelectedTargetAction ();
                        _abilitiesController.CurrentActiveAbility.Ability.DeactivateSelectTarget ();

                        yield return LetsThink ();
                    }
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
                            //todo: handle abilities here

                            PlayerMove playerMove = new PlayerMove (Enumerators.PlayerActionType.PlayCardOnBoard, PlayCardOnBoard);
                            _gameplayManager.PlayerMoves.AddPlayerMove (playerMove);
                        }, target);

                        yield return null;

                        if (target == null && needTargetForAbility)
                        {
                            WaitStart (3);
                            yield return new WaitUntil (() => _boardArrowController.CurrentBoardArrow != null || WaitTimeIsUp ());
                            _boardArrowController.ResetCurrentBoardArrow ();

                            yield return LetsThink ();

                            WaitStart (3);
                            yield return new WaitUntil (() => _abilitiesController.CurrentActiveAbility != null || WaitTimeIsUp ());
                            _abilitiesController.CurrentActiveAbility.Ability.SelectedTargetAction ();
                            _abilitiesController.CurrentActiveAbility.Ability.DeactivateSelectTarget ();

                            yield return LetsThink ();
                        }
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

        yield return null;
    }

    // todo: reconsider having this
    /// <summary>
    /// Plays cards with defined indices.
    /// </summary>
    /// <remarks>Was written specifically for tutorials, where we need to play cards with certain indices.</remarks>
    /// <param name="attackerCardIndices">Attacker card indices.</param>
    /// <param name="attackedCardIndices">Attacked card indices.</param>
    /// <param name="opponentPlayer">If set to <c>true</c> opponent player.</param>
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
        Loom.ZombieBattleground.Data.Card libraryCard = (Loom.ZombieBattleground.Data.Card) card.LibraryCard;

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

        /* foreach (WorkingCard workingCard in list)
        {
            Debug.Log ("+ " + workingCard.LibraryCard.MouldId + ": " + workingCard.LibraryCard.Name);
        } */

        List<Loom.ZombieBattleground.Data.Card> cards = new List<Loom.ZombieBattleground.Data.Card> ();

        foreach (WorkingCard item in list)
        {
            cards.Add (_dataManager.CachedCardsLibraryData.GetCardFromName (item.LibraryCard.Name));
        }

        cards = cards.OrderBy (x => x.Cost).ThenBy (y => y.Cost.ToString ().Length).ToList ();

        List<WorkingCard> sortedList = new List<WorkingCard> ();

        cards.Reverse ();

        foreach (Loom.ZombieBattleground.Data.Card item in cards)
        {
            // Debug.Log ("- " + item.MouldId + ": " + item.Name);

            sortedList.Add (list.Find (x => x.LibraryCard.Name == item.Name && !sortedList.Contains (x)));
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
    /// <summary>
    /// Uses a skill (potentially on specific object).
    /// </summary>
    /// <param name="skill">Skill.</param>
    /// <param name="overrideTarget">Override target.</param>
    /// <param name="selectedTargetType">Selected target type.</param>
    public void DoBoardSkill (
        BoardSkill skill,
        BoardObject overrideTarget = null,
        Enumerators.AffectObjectType selectedTargetType = Enumerators.AffectObjectType.None)
    {
        if (overrideTarget != null)
        {
            skill.StartDoSkill ();

            Action overrideCallback = () => {
                switch (selectedTargetType)
                {
                    case Enumerators.AffectObjectType.Player:
                        skill.FightTargetingArrow.SelectedPlayer = (Loom.ZombieBattleground.Player) overrideTarget;

                        Debug.Log ("Board skill: Player");

                        break;
                    case Enumerators.AffectObjectType.Character:
                        BoardUnitView selectedCardView = _battlegroundController.GetBoardUnitViewByModel ((BoardUnitModel) overrideTarget);
                        skill.FightTargetingArrow.SelectedCard = selectedCardView;

                        Debug.Log ("Board skill: Character");

                        break;
                }

                // _skillsController.DoSkillAction (skill, null, overrideTarget);
                skill.EndDoSkill ();
            };

            _boardArrowController.ResetCurrentBoardArrow ();

            skill.FightTargetingArrow = _boardArrowController.DoAutoTargetingArrowFromTo<OpponentBoardArrow> (skill.SelfObject.transform, overrideTarget, action: overrideCallback);

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

                        _unitsToIgnoreThisTurn.Add ((BoardUnitModel) target);

                        selectedObjectType = Enumerators.AffectObjectType.Character;
                    }
                    else
                    {
                        BoardUnitModel unit = GetRandomOpponentUnit (_unitsToIgnoreThisTurn);

                        if (unit != null)
                        {
                            target = unit;

                            _unitsToIgnoreThisTurn.Add ((BoardUnitModel) target);

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

        skill.StartDoSkill (true);

        Action callback = () =>
        {
            switch (selectedObjectType)
            {
                case Enumerators.AffectObjectType.Player:
                    Debug.Log ("Board skill: Player");

                    skill.FightTargetingArrow.SelectedPlayer = (Loom.ZombieBattleground.Player) target;

                    break;
                case Enumerators.AffectObjectType.Character:
                    Debug.Log ("Board skill: Character");

                    BoardUnitView selectedCardView = _battlegroundController.GetBoardUnitViewByModel ((BoardUnitModel) target);
                    skill.FightTargetingArrow.SelectedCard = selectedCardView;

                    break;
                case Enumerators.AffectObjectType.None:
                    Debug.Log ("Board skill: None");

                    break;
                default:
                    throw new ArgumentOutOfRangeException (nameof (selectedObjectType), selectedObjectType, null);
            }

            // todo fix this
            /* if (overrideTarget != null)
            {
                _skillsController.DoSkillAction (skill, null, overrideTarget);
            }
            else
            {
                _skillsController.DoSkillAction (skill, null, target);
            } */

            skill.EndDoSkill ();

            // _boardArrowController.ResetCurrentBoardArrow ();
        };

        skill.FightTargetingArrow = _boardArrowController.DoAutoTargetingArrowFromTo<OpponentBoardArrow> (skill.SelfObject.transform, target, action: callback);

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

    /// <summary>
    /// Waits for a specific amount of time.
    /// </summary>
    public IEnumerator LetsThink (float thinkTime = DefaultThinkTime)
    {
        if (thinkTime <= 0f)
        {
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
        }
        else
        {
            yield return new WaitForSeconds (thinkTime);
        }
    }

    /// <summary>
    /// Waits until the player order is decided and records the player overlord’s name in the process (in case we need it for assertion).
    /// </summary>
    public IEnumerator WaitUntilPlayerOrderIsDecided ()
    {
        yield return new WaitUntil (() => GameObject.Find ("PlayerOrderPopup(Clone)") != null);

        RecordActualOverlordName ();

        yield return new WaitUntil (() => GameObject.Find ("PlayerOrderPopup(Clone)") == null);

        yield return null;
    }

    /// <summary>
    /// Picks Mulligan Cards.
    /// </summary>
    /// <remarks>todo: Doesn't work, after the latest changes done to the way this is handled.</remarks>
    public IEnumerator DecideWhichCardsToPick ()
    {
        /* CardsController cardsController = _gameplayManager.GetController<CardsController> ();

        int highCardCounter = 0;

        Loom.ZombieBattleground.Player currentPlayer = _gameplayManager.CurrentPlayer;
        for (int i = currentPlayer.CardsPreparingToHand.Count - 1; i >= 0; i--)
        {
            BoardCard boardCard = currentPlayer.CardsPreparingToHand[i];
            WorkingCard workingCard = currentPlayer.CardsPreparingToHand[i];

            if ((workingCard.LibraryCard.CardKind == Enumerators.CardKind.SPELL) ||
                (highCardCounter >= 1 && workingCard.LibraryCard.Cost >= 4) ||
                workingCard.LibraryCard.Cost >= 8)
            {
                currentPlayer.CardsPreparingToHand[i].CardShouldBeChanged = !boardCard.CardShouldBeChanged;

                yield return LetsThink ();
            }
            else if (workingCard.LibraryCard.Cost >= 4)
            {
                highCardCounter++;

                yield return LetsThink ();
            }
        }

        yield return LetsThink (); */
        yield return LetsThink ();

        yield return ClickGenericButton ("Button_Keep");
    }

    /// <summary>
    /// Ends the turn for the player.
    /// </summary>
    public IEnumerator EndTurn ()
    {
        _battlegroundController.StopTurn ();
        GameObject.Find ("_1_btn_endturn").GetComponent<EndTurnButton> ().SetEnabled (false);

        Debug.Log ("Ended turn");

        yield return null;
    }

    /// <summary>
    /// Waits for player’s first turn, to start off playing. In case it is our turn, it does nothing, if not, it tracks until input is unblocked.
    /// </summary>
    public IEnumerator WaitUntilOurFirstTurn ()
    {
        if (IsGameEnded ())
            yield break;

        if (_gameplayManager.CurrentTurnPlayer.Id == _gameplayManager.CurrentPlayer.Id)
        {
            yield return null;
        }
        else
        {
            if (IsGameEnded ())
                yield break;

            yield return WaitUntilOurTurnStarts ();

            if (IsGameEnded ())
                yield break;

            yield return WaitUntilInputIsUnblocked ();
        }

        yield return LetsThink ();
    }

    public IEnumerator WaitUntilWeHaveACardAtHand ()
    {
        yield return new WaitUntil (() => _battlegroundController.PlayerHandCards.Count >= 1);

        yield return null;
    }

    /// <summary>
    /// Waits until AIBrain stops thinking.
    /// </summary>
    /// <remarks>Was written specifically for tutorials, where some steps require it.</remarks>
    public IEnumerator WaitUntilAIBrainStops ()
    {
        yield return new WaitUntil (() => _gameplayManager.GetController<AIController> ().IsBrainWorking == false);
    }

    /// <summary>
    /// Waits until player’s turn starts.
    /// </summary>
    public IEnumerator WaitUntilOurTurnStarts ()
    {
        yield return new WaitUntil (() => IsGameEnded () || GameObject.Find ("YourTurnPopup(Clone)") != null);

        yield return new WaitUntil (() => IsGameEnded () || GameObject.Find ("YourTurnPopup(Clone)") == null);
    }

    /// <summary>
    /// Waits until player can make a move.
    /// </summary>
    public IEnumerator WaitUntilInputIsUnblocked ()
    {
        yield return new WaitUntil (() => IsGameEnded () || _gameplayManager.IsLocalPlayerTurn ());
    }

    // todo: reconsider having this
    /// <summary>
    /// Uses primary skill on opponent player.
    /// </summary>
    /// <remarks>Was written specifically for tutorials.</remarks>
    public IEnumerator UseSkillToOpponentPlayer ()
    {
        DoBoardSkill (_testBroker.GetPlayerPrimarySkill (_player), _testBroker.GetPlayer (_opponent), Enumerators.AffectObjectType.Player);

        yield return LetsThink ();
        yield return LetsThink ();

        yield return null;
    }

    // todo: reconsider having this
    /// <summary>
    /// Plays all non-sleeping cards to attack the enemy player
    /// </summary>
    /// <remarks>Was written specifically for tutorials, where some steps require it.</remarks>
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
    /// <summary>
    /// Waits until the card is added to board.
    /// </summary>
    /// <remarks>Was written specifically for tutorials, where some steps require it.</remarks>
    /// <param name="boardName">Board name.</param>
    public IEnumerator WaitUntilCardIsAddedToBoard (string boardName)
    {
        Transform boardTransform = GameObject.Find (boardName).transform;
        int boardChildrenCount = boardTransform.childCount;

        yield return new WaitUntil (() => (boardChildrenCount < boardTransform.childCount) && (boardChildrenCount < _battlegroundController.OpponentBoardCards.Count));
    }

    /// <summary>
    /// Makes specified number of moves (if timeout allows).
    /// </summary>
    /// <param name="maxTurns">Max number of turns.</param>
    public IEnumerator MakeMoves (int maxTurns = 100)
    {
        // if it doesn't end in 100 moves, end the game anyway
        for (int turns = 1; turns <= maxTurns; turns++)
        {
            Debug.Log ("a 0");

            yield return TurnStartedHandler ();

            Debug.Log ("a 1");

            TurnEndedHandler ();

            if (IsGameEnded ())
                yield break;

            yield return EndTurn ();

            Debug.Log ("a 2");

            if (IsGameEnded ())
                break;

            yield return WaitUntilOurTurnStarts ();

            Debug.Log ("a 3");

            if (IsGameEnded ())
                break;

            yield return WaitUntilInputIsUnblocked ();

            Debug.Log ("a 4");

            if (IsGameEnded ())
                break;
        }
    }

    /// <summary>
    /// Checks if game has ended.
    /// </summary>
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

    /// <summary>
    /// Adds a Valash Deck and cards from Life group.
    /// </summary>
    public IEnumerator AddValashHorde ()
    {
        yield return ClickGenericButton ("Image_BaackgroundGeneral");

        yield return AssertCurrentPageName ("OverlordSelectionPage");

        yield return PickOverlord ("Valash", false);

        yield return PickOverlordAbility (0);

        yield return ClickGenericButton ("Canvas_BackLayer/Button_Continue");

        yield return AssertCurrentPageName ("HordeEditingPage");

        SetupArmyCards ();

        yield return SetDeckTitle ("Valash");

        yield return AddCardToHorde ("Life", "Azuraz", 4);

        yield return AddCardToHorde ("Life", "Bloomer", 4);

        yield return AddCardToHorde ("Life", "Zap", 4);

        yield return AddCardToHorde ("Life", "Amber", 4);

        yield return AddCardToHorde ("Life", "Bark", 4);

        yield return AddCardToHorde ("Life", "Puffer", 2);

        yield return AddCardToHorde ("Life", "Sapper", 2);

        yield return AddCardToHorde ("Life", "Keeper", 2);

        yield return AddCardToHorde ("Life", "Cactuz", 2);

        yield return AddCardToHorde ("Life", "EverlaZting", 2);

        yield return ClickGenericButton ("Button_Save");
    }

    /// <summary>
    /// Adss a Kalile Horde and cards from Air group.
    /// </summary>
    public IEnumerator AddKalileHorde ()
    {
        yield return ClickGenericButton ("Image_BaackgroundGeneral");

        yield return AssertCurrentPageName ("OverlordSelectionPage");

        yield return PickOverlord ("Kalile", false);

        yield return PickOverlordAbility (1);

        yield return ClickGenericButton ("Canvas_BackLayer/Button_Continue");

        yield return AssertCurrentPageName ("HordeEditingPage");

        SetupArmyCards ();

        yield return SetDeckTitle ("Kalile");

        yield return AddCardToHorde ("Air", "Whizpar", 4);

        yield return AddCardToHorde ("Air", "Soothsayer", 4);

        yield return AddCardToHorde ("Air", "FumeZ", 4);

        yield return AddCardToHorde ("Air", "Breezee", 4);

        yield return AddCardToHorde ("Air", "Banshee", 4);

        yield return AddCardToHorde ("Air", "Zhocker", 4);

        yield return AddCardToHorde ("Air", "Whiffer", 4);

        yield return AddCardToHorde ("Air", "Bouncer", 2);

        yield return ClickGenericButton ("Button_Save");
    }

    /// <summary>
    /// Adds a Razu Horde deck and cards from Fire group.
    /// </summary>
    public IEnumerator AddRazuHorde ()
    {
        yield return ClickGenericButton ("Image_BaackgroundGeneral");

        yield return AssertCurrentPageName ("OverlordSelectionPage");

        yield return PickOverlord ("Razu", true);

        yield return PickOverlordAbility (1);

        yield return ClickGenericButton ("Canvas_BackLayer/Button_Continue");

        yield return AssertCurrentPageName ("HordeEditingPage");

        SetupArmyCards ();

        yield return SetDeckTitle ("Razu");

        yield return AddCardToHorde ("Fire", "Pyromaz", 4);

        yield return AddCardToHorde ("Fire", "Quazi", 4);

        yield return AddCardToHorde ("Fire", "Ember", 4);

        yield return AddCardToHorde ("Fire", "Firewall", 4);

        yield return AddCardToHorde ("Fire", "BurZt", 4);

        yield return AddCardToHorde ("Fire", "Firecaller", 4);

        yield return AddCardToHorde ("Fire", "Burrrnn", 2);

        yield return AddCardToHorde ("Fire", "Werezomb", 2);

        yield return AddCardToHorde ("Fire", "Modo", 2);

        yield return ClickGenericButton ("Button_Save");
    }

    /// <summary>
    /// Creates list of cards according to the ones available on the page.
    /// </summary>
    public void SetupArmyCards ()
    {
        DeckBuilderCard[] deckBuilderCards = GameObject.FindObjectsOfType<DeckBuilderCard> ();

        if (deckBuilderCards == null || deckBuilderCards.Length == 0)
            return;

        _createdArmyCards = new List<Loom.ZombieBattleground.Data.Card> ();
        foreach (DeckBuilderCard deckBuilderCard in deckBuilderCards)
        {
            _createdArmyCards.Add ((Loom.ZombieBattleground.Data.Card) deckBuilderCard.Card);
        }
    }

    /// <summary>
    /// Picks an overlord, by the specified name.
    /// </summary>
    /// <param name="overlordName">Overlord name.</param>
    /// <param name="goRight">If set to <c>true</c> goes right, until finds what you set.</param>
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

    /// <summary>
    /// Picks the overlord ability.
    /// </summary>
    /// <param name="index">Index.</param>
    public IEnumerator PickOverlordAbility (int index)
    {
        GameObject abilitiesParent = GameObject.Find ("Abilities");

        if (index >= abilitiesParent.transform.childCount)
        {
            Assert.Fail ("Index higher than number of abilities");
        }

        if (abilitiesParent.transform.GetChild (index).GetComponent<Button> ().IsInteractable ())
        {
            abilitiesParent.transform.GetChild (index).GetComponent<Button> ().onClick.Invoke ();
        }

        yield return LetsThink ();
    }

    /// <summary>
    /// Sets the name/title of the deck.
    /// </summary>
    /// <param name="deckTitle">Deck title.</param>
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

        deckTitleInputField.text = deckTitle; // for visibility during testing
        deckTitleInputField.onEndEdit.Invoke (deckTitle); // for post deck creation result

        yield return LetsThink ();
    }

    private IEnumerator PickElement (string elementName)
    {
        Transform elementsParent = GameObject.Find ("ElementsToggles").transform;

        Toggle elementToggle = elementsParent.Find (elementName)?.GetComponent<Toggle> ();

        if (elementToggle != null)
        {
            if (elementName == _currentElementName)
            {
                if (elementName != "Fire")
                {
                    yield return PickElement ("Fire");
                }
                else
                {
                    yield return PickElement ("Water");
                }
            }

            elementToggle.onValueChanged.Invoke (true);

            _currentElementName = elementName;
        }

        yield return LetsThink ();

        SetupArmyCards ();

        yield return LetsThink ();
    }

    /// <summary>
    /// Adds a card to the Horde from specified element.
    /// </summary>
    /// <param name="elementName">Element name.</param>
    /// <param name="cardName">Card name.</param>
    /// <param name="count">Count.</param>
    public IEnumerator AddCardToHorde (string elementName, string cardName, int count = 1)
    {
        Loom.ZombieBattleground.Data.Card armyCard = _createdArmyCards.Find (x =>
            x.Name == cardName);

        if (armyCard == null)
        {
            yield return PickElement (elementName);
        }

        yield return AddCardToHorde2 (cardName, count);
    }

    //The method that actually adds the cards is AddCardToHorde2
    //AddCardToHorde is only used to check if in the current displayed element, there's the card we're looking for
    //If it's not, the correct element is picked so that the card can be found
    //Otherwise it can proceed directly to adding said cards

    private IEnumerator AddCardToHorde2 (string cardName, int count = 1)
    {
        int checkedPage;

        for (checkedPage = 0; checkedPage <= 4; checkedPage++)
        {
            Loom.ZombieBattleground.Data.Card armyCard = _createdArmyCards.Find (x =>
                x.Name == cardName);

            if (armyCard == null)
            {
                yield return ClickGenericButton ("Army/ArrowRightButton");

                yield return LetsThink ();

                SetupArmyCards ();

                yield return LetsThink ();

                continue;
            }

            Debug.Log ("Adding " + cardName + " (" + armyCard.Cost + ") x" + count);

            for (int counter = 0; counter < count; counter++)
            {
                _uiManager.GetPage<HordeEditingPage> ().AddCardToDeck (null, armyCard);

                yield return LetsThink ();
            }

            yield return LetsThink ();

            break;
        }

        yield return null;
    }

    /// <summary>
    /// Gets the number of Hordes.
    /// </summary>
    public int GetNumberOfHordes ()
    {
        GameObject hordesParent = GameObject.Find ("Panel_DecksContainer/Group");

        if (hordesParent != null)
        {
            return hordesParent.transform.childCount;
        }
        else
        {
            return -1;
        }
    }

    /// <summary>
    /// Selects a Horde by name.
    /// </summary>
    /// <param name="hordeName">Horde name.</param>
    public IEnumerator SelectAHordeByName (string hordeName)
    {
        GameObject hordesParent = GameObject.Find ("Panel_DecksContainer/Group");

        SelectedHordeIndex = -1;
        bool hordeSelected = false;

        for (int i = 0; i < GetNumberOfHordes () - 1; i++)
        {
            Transform selectedHordeTransform = hordesParent.transform.GetChild (i);

            if (selectedHordeTransform?.Find ("Panel_Description/Text_Description")?.GetComponent<TextMeshProUGUI> ()?.text != null &&
                selectedHordeTransform?.Find ("Panel_Description/Text_Description")?.GetComponent<TextMeshProUGUI> ()?.text == hordeName)
            {
                selectedHordeTransform.Find ("Button_Select").GetComponent<Button> ().onClick.Invoke ();

                SelectedHordeIndex = i;
                hordeSelected = true;
            }
        }

        if (!hordeSelected)
        {
            Assert.Fail ("Couldn't find Horde by that name");
        }

        yield return null;
    }

    /// <summary>
    /// Selects a Horde by index.
    /// </summary>
    /// <param name="index">Index.</param>
    public IEnumerator SelectAHordeByIndex (int index)
    {
        if (index + 1 >= GetNumberOfHordes ())
        {
            Assert.Fail ("Horde removal index is too high");
        }

        GameObject hordesParent = GameObject.Find ("Panel_DecksContainer/Group");
        Transform selectedHordeTransform = hordesParent.transform.GetChild (index);
        selectedHordeTransform.Find ("Button_Select").GetComponent<Button> ().onClick.Invoke ();

        yield return LetsThink ();
    }

    /// <summary>
    /// Removes a Horde by index.
    /// </summary>
    /// <param name="index">Index.</param>
    public IEnumerator RemoveAHorde (int index)
    {
        yield return SelectAHordeByIndex (index);

        GameObject.Find ("Button_Delete").GetComponent<Button> ().onClick.Invoke ();

        yield return LetsThink ();
    }

    /// <summary>
    /// Removes all Hordes except first.
    /// </summary>
    public IEnumerator RemoveAllHordesExceptDefault ()
    {
        for (int i = GetNumberOfHordes () - 2; i >= 1; i--)
        {
            yield return RemoveAHorde (1);

            yield return RespondToYesNoOverlay (true);

            yield return LetsThink ();
            yield return LetsThink ();
            yield return LetsThink ();
        }

        yield return null;
    }

    #endregion

    public void RecordExpectedOverlordName (int index)
    {
        GameObject hordesParent = GameObject.Find ("Panel_DecksContainer/Group");

        if (index >= hordesParent.transform.childCount)
        {
            Assert.Fail ("Horde index is too high");
        }

        Transform selectedHordeTransform = hordesParent.transform.GetChild (index);

        RecordAValue (selectedHordeTransform, "Panel_Description/Text_Description", RecordedValue.Expected);
    }

    public void RecordActualOverlordName ()
    {
        RecordAValue (null, "Text_PlayerOverlordName", RecordedValue.Actual);
    }

    private void RecordAValue (string value, RecordedValue recordedValue)
    {
        if (value == null || value.Length <= 1)
        {
            value = "";
        }

        switch (recordedValue)
        {
            case RecordedValue.Expected:
                _recordedExpectedValue = UppercaseFirst (value);

                break;
            case RecordedValue.Actual:
                _recordedActualValue = UppercaseFirst (value);

                break;
        }
    }

    /// <summary>
    /// Records a value (expected or actual) to be used for assertion.
    /// </summary>
    /// <param name="parentTransform">Parent transform.</param>
    /// <param name="objectName">Object name.</param>
    /// <param name="recordedValue">Recorded value.</param>
    public void RecordAValue (Transform parentTransform, string objectName, RecordedValue recordedValue)
    {
        if (parentTransform != null)
        {
            RecordAValue (parentTransform.Find (objectName)?.GetComponent<TextMeshProUGUI> ()?.text, recordedValue);
        }
        else
        {
            RecordAValue (GameObject.Find (objectName)?.GetComponent<TextMeshProUGUI> ()?.text, recordedValue);
        }
    }

    /// <summary>
    /// Checks and confirms that Overlord name is as expected.
    /// </summary>
    public void AssertOverlordName ()
    {
        if (_recordedExpectedValue.Length <= 0 || _recordedActualValue.Length <= 0 || _recordedExpectedValue == "Default")
        {
            Debug.Log ("One of the overlord names was null, so didn't check.");

            return;
        }

        Debug.LogFormat ("{0} vs {1}", _recordedExpectedValue, _recordedActualValue);

        Assert.AreEqual (_recordedExpectedValue, _recordedActualValue);
    }

    private string UppercaseFirst (string s)
    {
        return System.Globalization.CultureInfo.InvariantCulture.TextInfo.ToTitleCase (s.ToLower ());
    }

    public enum RecordedValue
    {
        Expected,
        Actual
    }

    #region PvP gameplay

    /// <summary>
    /// Plays a match and once the match finishes, presses on Continue button.
    /// </summary>
    public IEnumerator PlayAMatch (int maxTurns = 100)
    {
        yield return AssertCurrentPageName ("GameplayPage");

        InitalizePlayer ();

        yield return WaitUntilPlayerOrderIsDecided ();

        yield return AssertMulliganPopupCameUp (
            DecideWhichCardsToPick (),
            null);

        yield return WaitUntilOurFirstTurn ();

        yield return MakeMoves (maxTurns);

        Debug.LogWarning ("0");

        yield return ClickGenericButton ("Button_Continue");

        Debug.LogWarning ("1");

        yield return AssertCurrentPageName ("HordeSelectionPage");

        Debug.LogWarning ("2");
    }

    /// <summary>
    /// Presses OK or GotIt button if it's on.
    /// </summary>
    /// <remarks>Useful where you expect to get a popup with this button.</remarks>
    public IEnumerator PressOK ()
    {
        if (GameObject.Find ("Button_OK") != null)
            yield return ClickGenericButton ("Button_OK");
        else
            yield return ClickGenericButton ("Button_GotIt");
    }

    #endregion

    private AbilityBoardArrow GetAbilityBoardArrow ()
    {
        if (GameObject.FindObjectOfType<AbilityBoardArrow> () != null)
        {
            return GameObject.FindObjectOfType<AbilityBoardArrow> ();
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Starts the waiting process.
    /// </summary>
    /// <remarks>Useful in case you have concern of getting a response for a request. To be coupled with WaitTimeIsUp.</remarks>
    /// <param name="waitAmount">Wait amount.</param>
    private void WaitStart (int waitAmount)
    {
        _waitStartTime = Time.time;

        _waitAmount = waitAmount;
    }

    /// <summary>
    /// Checks if waiting amount has been reached
    /// </summary>
    /// <remarks>Useful in case you have concern of getting a response for a request. To be coupled with WaitStart.</remarks>
    /// <returns><c>true</c>, if time is up, <c>false</c> otherwise.</returns>
    private bool WaitTimeIsUp (string dummyParameter = "")
    {
        return Time.time > _waitStartTime + _waitAmount;
    }

    private void TurnWaitStart (int waitAmount)
    {
        _turnStartTime = Time.time;

        _turnWaitAmount = waitAmount;
    }

    private bool TurnTimeIsUp ()
    {
        return Time.time > _turnStartTime + _turnWaitAmount;
    }
}
