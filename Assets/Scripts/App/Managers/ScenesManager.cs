using System;
using System.Collections;
using Loom.ZombieBattleground.Common;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Loom.ZombieBattleground
{
    public sealed class ScenesManager : IService, IScenesManager
    {
        private bool _isLoadingStarted;

        private IAppStateManager _appStateManager;

        public event Action<Enumerators.AppState> SceneForAppStateWasLoadedEvent;

        public Enumerators.AppState CurrentAppStateScene { get; set; }

        public int SceneLoadingProgress { get; set; }

        public bool IsLoadedScene { get; set; }

        public bool IsAutoSceneSwitcher { get; set; }

        public void ChangeScene(Enumerators.AppState appState, bool force = false)
        {
            if (!force)
            {
                if (appState == Enumerators.AppState.NONE || CurrentAppStateScene == appState)
                    return;
            }

            IsLoadedScene = false;
            _isLoadingStarted = true;

            GameClient.Get<IAnalyticsManager>().LogScreen(appState.ToString());
            MainApp.Instance.StartCoroutine(LoadLevelAsync(appState.ToString()));
        }

        public void Dispose()
        {
            MainApp.Instance.LevelLoaded -= LevelLoadedHandler;
        }

        public void Init()
        {
            IsAutoSceneSwitcher = false;

            MainApp.Instance.LevelLoaded += LevelLoadedHandler;

            _appStateManager = GameClient.Get<IAppStateManager>();

            LevelLoadedHandler(null);
        }

        public void Update()
        {
            if (IsAutoSceneSwitcher)
            {
                if (CurrentAppStateScene != _appStateManager.AppState && !_isLoadingStarted)
                {
                    ChangeScene(_appStateManager.AppState);
                }
            }
        }

        private void LevelLoadedHandler(object param)
        {
            CurrentAppStateScene =
                (Enumerators.AppState) Enum.Parse(typeof(Enumerators.AppState), SceneManager.GetActiveScene().name);
            _isLoadingStarted = false;
            IsLoadedScene = true;
            SceneLoadingProgress = 0;

            SceneForAppStateWasLoadedEvent?.Invoke(CurrentAppStateScene);

            GameClient.Get<IAnalyticsManager>().Reset();
        }

        private IEnumerator LoadLevelAsync(string levelName)
        {
            AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(levelName);
            float delayTime = Constants.LoadingTimeBetweenGameplayAndAppInit;
            if (levelName != Enumerators.AppState.APP_INIT.ToString())
            {
                delayTime = 0;
            }

            while (!asyncOperation.isDone || delayTime > 0)
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
