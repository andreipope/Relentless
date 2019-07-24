using System;
using log4net;
using Loom.ZombieBattleground.Common;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using ZombieBattleground.Editor.Runtime;
#endif

namespace Loom.ZombieBattleground
{
    public class MainApp : MonoBehaviour
    {
        private static readonly ILog Log = Logging.GetLog(nameof(MainApp));

        public delegate void MainAppDelegate(object param);

        public event Action<Scene, LoadSceneMode> LevelLoaded;

        public event Action LateUpdateEvent;

        public event Action FixedUpdateEvent;

#if UNITY_EDITOR
        public event Action OnDrawGizmosCalled;
#endif

        public event Action<Action<bool>> ApplicationWantsToQuit;

        public static MainApp Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Start()
        {
            if (Instance == this)
            {
                try
                {
                    GameClient.Instance.InitServices();
                }
                catch (Exception)
                {
                    // Nothing we can do here, just crash quickly
                    Destroy(gameObject);
                    throw;
                }

                GameClient.Get<IAppStateManager>().ChangeAppState(Enumerators.AppState.APP_INIT);

                SceneManager.sceneLoaded += SceneManager_sceneLoaded;
                Application.wantsToQuit += OnApplicationWantsToQuit;

                GameClient.Get<IAnalyticsManager>().StartSession();
            }
        }

        private void Update()
        {
            if (Instance == this)
            {
                GameClient.Instance.Update();
            }
        }

        private void LateUpdate()
        {
            if (Instance == this)
            {
                LateUpdateEvent?.Invoke();
            }
        }

        private void FixedUpdate()
        {
            if (Instance == this)
            {
                FixedUpdateEvent?.Invoke();
            }
        }

        private void OnDestroy()
        {
            // FIXME: Mixpanel crashes when sending events during app shutdown
            //GameClient.Get<IAnalyticsManager>().SetEvent(AnalyticsManager.EventQuitToDesktop);

            if (Instance == this)
            {
                GameClient.Instance.Dispose();
                Instance = null;
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!GizmosGuiHelper.CanRenderGui() || (Camera.main != Camera.current && Camera.current.name != "SceneCamera"))
                return;

            OnDrawGizmosCalled?.Invoke();
        }
#endif

        private void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            if (Instance == this)
            {
                LevelLoaded?.Invoke(arg0, arg1);
            }
        }

        private bool OnApplicationWantsToQuit()
        {
            Log.Info("OnApplicationWantsToQuit()");
            bool wantsToQuit = true;
            ApplicationWantsToQuit?.Invoke(val => wantsToQuit = val);
            Log.Info("OnApplicationWantsToQuit returns " + wantsToQuit);
            return wantsToQuit;
        }
    }
}
