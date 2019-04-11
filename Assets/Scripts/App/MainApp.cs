using System;
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
        public delegate void MainAppDelegate(object param);

        public event Action<Scene, LoadSceneMode> LevelLoaded;

        public event Action LateUpdateEvent;

        public event Action FixedUpdateEvent;

        public event Action OnDrawGizmosCalled;

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
                GameClient.Instance.InitServices();

                GameClient.Get<IAppStateManager>().ChangeAppState(Enumerators.AppState.APP_INIT);

                SceneManager.sceneLoaded += SceneManager_sceneLoaded;

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

        private async void OnDestroy()
        {
            await GameClient.Get<IAppStateManager>().SendLeaveMatchIfInPlay();

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

        private void OnApplicationQuit()
        {
            GameClient.Get<IAnalyticsManager>().SetEvent(AnalyticsManager.EventQuitToDesktop);
        }
    }
}
