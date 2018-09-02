using System;
using System.Collections;
using LoomNetwork.CZB.Common;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LoomNetwork.CZB
{
    public sealed class ScenesManager : IService, IScenesManager
    {
        public event Action<Enumerators.AppState> SceneForAppStateWasLoadedEvent;

        private readonly bool _isLoadingScenesAsync = true;

        private bool _isLoadingStarted;

        private IAppStateManager _appStateManager;

        private IUIManager _uiManager;

        public Enumerators.AppState CurrentAppStateScene { get; set; }

        public int SceneLoadingProgress { get; set; }

        public bool IsLoadedScene { get; set; }

        public bool IsAutoSceneSwitcher { get; set; }

        public void ChangeScene(Enumerators.AppState appState)
        {
            if ((appState == Enumerators.AppState.NONE) || (CurrentAppStateScene == appState))

                return;

            IsLoadedScene = false;
            _isLoadingStarted = true;

            GameClient.Get<IAnalyticsManager>().LogScreen(appState.ToString());
            if (!_isLoadingScenesAsync)
            {
                SceneManager.LoadScene(appState.ToString());
            } else
            {
                MainApp.Instance.StartCoroutine(LoadLevelAsync(appState.ToString()));
            }
        }

        public void Dispose()
        {
            MainApp.Instance.OnLevelWasLoadedEvent -= OnLevelWasLoadedHandler;
        }

        public void Init()
        {
            IsAutoSceneSwitcher = false;

            MainApp.Instance.OnLevelWasLoadedEvent += OnLevelWasLoadedHandler;

            _appStateManager = GameClient.Get<IAppStateManager>();
            _uiManager = GameClient.Get<IUIManager>();

            OnLevelWasLoadedHandler(null);
        }

        public void Update()
        {
            if (IsAutoSceneSwitcher)
            {
                if ((CurrentAppStateScene != _appStateManager.AppState) && !_isLoadingStarted)
                {
                    ChangeScene(_appStateManager.AppState);
                }
            }
        }

        private void OnLevelWasLoadedHandler(object param)
        {
            CurrentAppStateScene = (Enumerators.AppState)Enum.Parse(typeof(Enumerators.AppState), SceneManager.GetActiveScene().name);
            _isLoadingStarted = false;
            IsLoadedScene = true;
            SceneLoadingProgress = 0;

            if (SceneForAppStateWasLoadedEvent != null)
            {
                SceneForAppStateWasLoadedEvent(CurrentAppStateScene);
            }
        }

        private IEnumerator LoadLevelAsync(string levelName)
        {
            AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(levelName);
            float delayTime = Constants.LOADING_TIME_BETWEEN_GAMEPLAY_AND_APP_INIT;
            if (levelName != Enumerators.AppState.APP_INIT.ToString())
            {
                delayTime = 0;
            }

            while (!asyncOperation.isDone || (delayTime > 0))
            {
                delayTime -= Time.deltaTime;
                SceneLoadingProgress = Mathf.RoundToInt(asyncOperation.progress * 100f);
                if (delayTime > 0)
                {
                    SceneLoadingProgress = Mathf.Min(SceneLoadingProgress, 90);
                }

                yield return null;
            }
        }
    }
}
