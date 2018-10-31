using NUnit.Framework;
using System.Collections;
using Loom.ZombieBattleground;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.BackendCommunication;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

public class TutorialTests
{
    private const string _testerKey = "f12249ff43e4";

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

    private IEnumerator MainMenuTransition (string transitionPath, float delay = 0.5f)
    {
        GameObject menuButtonGameObject;
        ButtonShiftingContent menuButton;
        foreach (string buttonName in transitionPath.Split ('/'))
        {
            menuButtonGameObject = null;
            yield return new WaitUntil (() => {
                menuButtonGameObject = GameObject.Find (buttonName);

                if (menuButtonGameObject == null)
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

            yield return new WaitForSeconds (delay);
        }
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

    [UnityTest]
    [Timeout (100000)]
    public IEnumerator SkipTutorial ()
    {
        yield return TestSetup ("APP_INIT", "SkipTutorial");

        yield return AddVirtualInputModule ();

        yield return HandleLogin ();

        yield return MainMenuTransition ("Button_Play/Button_Back");

        // yield return MainMenuTransition ("Button_Tutorial");

        /* Vector2 from = new Vector2 (10f, 10f);
        Vector2 to = new Vector2 (600f, 600f);
        float moveTime = 5f;

        Vector2 position = from;
        float moveTimeCounter = 0f;
        while (Vector2.Distance (position, to) >= 50f)
        {
            position = Vector2.Lerp (from, to, moveTimeCounter / moveTime);
            _fakeCursorTransform.anchoredPosition = position;

            moveTimeCounter = Mathf.Min (moveTime, moveTimeCounter + Time.deltaTime);

            yield return null;
        }

        _virtualInputModule.Press ();

        yield return null;

        _virtualInputModule.Release (); */

        yield return new WaitForSeconds (10);

        yield return TestTearDown ();
    }
}
