using System;
using Loom.ZombieBattleground.Common;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Loom.ZombieBattleground
{
    public class MainApp : MonoBehaviour
    {
        public delegate void MainAppDelegate(object param);

        public event MainAppDelegate LevelLoaded;

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
        }

        private void Start()
        {
            if (Instance == this)
            {
                GameClient.Instance.InitServices();

#if DEV_MODE
                GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.BACKGROUND, 128, Constants.BackgroundSoundVolume, null, true, false, true);
                GameClient.Get<IDataManager>().StartLoadCache();
                GameClient.Get<IAppStateManager>().ChangeAppState(Enumerators.AppState.HordeSelection);
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
                LevelLoaded?.Invoke(arg0.buildIndex);
            }
        }
    }
}
