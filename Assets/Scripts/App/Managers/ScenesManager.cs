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

        private bool _appInitLoaded;

        public event Action<Enumerators.AppState> SceneForAppStateWasLoadedEvent;

        public Enumerators.AppState CurrentAppStateScene { get; set; }

        public int SceneLoadingProgress { get; set; }

        public bool IsLoadedScene { get; set; }

        public bool IsAutoSceneSwitcher { get; set; }

        public void ChangeScene(Enumerators.AppState appState, bool force = false)
        {
            if (!force)
            {
                if (appState == Enumerators.AppState.NONE || CurrentAppStateScene == appState || _isLoadingStarted)
                    return;
            }

            IsLoadedScene = false;
            _isLoadingStarted = true;

            GameClient.Get<IAnalyticsManager>().LogScreen(appState.ToString());
            MainApp.Instance.StartCoroutine(LoadLevelAsync(appState));
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

            LevelLoadedHandler(SceneManager.GetActiveScene(), LoadSceneMode.Single);
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

        private void LevelLoadedHandler(Scene scene, LoadSceneMode loadSceneMode)
        {
            LevelLoadedHandlerInternal((Enumerators.AppState) Enum.Parse(typeof(Enumerators.AppState), scene.name));
        }

        private void LevelLoadedHandlerInternal(Enumerators.AppState appState)
        {
            CurrentAppStateScene = appState;
            if (CurrentAppStateScene == Enumerators.AppState.APP_INIT)
            {
                _appInitLoaded = true;
            }

            _isLoadingStarted = false;
            IsLoadedScene = true;
            SceneLoadingProgress = 0;

            SceneManager.SetActiveScene(SceneManager.GetSceneByName(appState.ToString()));
            SceneForAppStateWasLoadedEvent?.Invoke(CurrentAppStateScene);
        }

        private IEnumerator LoadLevelAsync(Enumerators.AppState appState)
        {
            if (_appInitLoaded && appState == Enumerators.AppState.APP_INIT)
            {
                // Unload all other scenes
                Scene[] scenes = new Scene[SceneManager.sceneCount];
                for (int i = 0; i < SceneManager.sceneCount; ++i)
                {
                    scenes[i] = SceneManager.GetSceneAt(i);
                }

                foreach (Scene scene in scenes)
                {
                    if (scene.name == Enumerators.AppState.APP_INIT.ToString())
                        continue;

                    AsyncOperation unloadSceneAsync = SceneManager.UnloadSceneAsync(scene);
                    while (!unloadSceneAsync.isDone)
                    {
                        yield return null;
                    }
                }

                LevelLoadedHandlerInternal(appState);
                yield break;
            }

            float delayTime = Constants.LoadingTimeBetweenGameplayAndAppInit;
            if (appState != Enumerators.AppState.APP_INIT)
            {
                delayTime = 0;
            }

            AsyncOperation loadAsyncOperation = SceneManager.LoadSceneAsync(appState.ToString(), LoadSceneMode.Additive);
            while (!loadAsyncOperation.isDone || delayTime > 0)
            {
                delayTime -= Time.deltaTime;
                SceneLoadingProgress = Mathf.RoundToInt(loadAsyncOperation.progress * 100f);
                if (delayTime > 0)
                {
                    SceneLoadingProgress = Mathf.Min(SceneLoadingProgress, 90);
                }

                yield return null;
            }

            SceneLoadingProgress = 100;
        }
    }
}
