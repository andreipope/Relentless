using System;
using System.Threading;
using LoomNetwork.CZB.Common;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LoomNetwork.CZB
{
    public class MainApp : MonoBehaviour
    {
        public delegate void MainAppDelegate(object param);

        public static int MainThreadId;

        public event MainAppDelegate OnLevelWasLoadedEvent;

        public event Action LateUpdateEvent;

        public event Action FixedUpdateEvent;

        public static MainApp Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            MainThreadId = Thread.CurrentThread.ManagedThreadId;
        }

        private void Start()
        {
            if (Instance == this)
            {
                GameClient.Instance.InitServices();

#if DEV_MODE
                GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.BACKGROUND, 128, Constants.BACKGROUND_SOUND_VOLUME, null, true, false, true);
                GameClient.Get<IDataManager>().StartLoadCache();
                GameClient.Get<IAppStateManager>().ChangeAppState(Enumerators.AppState.DECK_SELECTION);
#endif

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

        private void OnDestroy()
        {
            if (Instance == this)
            {
                GameClient.Instance.Dispose();
            }
        }

        private void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            if (Instance == this)
            {
                OnLevelWasLoadedEvent?.Invoke(arg0.buildIndex);
            }
        }
    }
}
