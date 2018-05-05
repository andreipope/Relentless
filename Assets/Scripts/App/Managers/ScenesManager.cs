using UnityEngine;
using System.Collections;
using System;
#if UNITY_5_3_OR_NEWER
using UnityEngine.SceneManagement;
#endif
using GrandDevs.CZB.Common;

namespace GrandDevs.CZB
{
    public sealed class ScenesManager : IService, IScenesManager
    {
        public event Action<Enumerators.AppState> SceneForAppStateWasLoadedEvent;

        private bool _isLoadingScenesAsync = true;
        private bool _isLoadingStarted = false;

        private IAppStateManager _appStateManager;
        private IUIManager _uiManager;

        public Enumerators.AppState CurrentAppStateScene { get; set; }
        public int SceneLoadingProgress { get; set; }

        public bool IsLoadedScene { get; set; }
        public bool IsAutoSceneSwitcher { get; set; }


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
                if (CurrentAppStateScene != _appStateManager.AppState && !_isLoadingStarted)
                    ChangeScene(_appStateManager.AppState);
            }
        }

        public void ChangeScene(Enumerators.AppState appState)
        {
            if (appState == Enumerators.AppState.APP_INIT ||
                appState == Enumerators.AppState.NONE)
                return;

            IsLoadedScene = false;
            _isLoadingStarted = true;

            if (!_isLoadingScenesAsync)
            {
#if UNITY_5_3_OR_NEWER
                SceneManager.LoadScene(appState.ToString());
#else
                Application.LoadLevel(appState.ToString());
#endif
            }
            else
                MainApp.Instance.StartCoroutine(LoadLevelAsync(appState.ToString()));
        }

        private void OnLevelWasLoadedHandler(object param)
        {
#if UNITY_5_3_OR_NEWER
            CurrentAppStateScene = (Enumerators.AppState)Enum.Parse(typeof(Enumerators.AppState), SceneManager.GetActiveScene().name);
#else
            CurrentAppStateScene = (Enumerators.AppState)Enum.Parse(typeof(Enumerators.AppState), Application.loadedLevelName);
#endif
            if(CurrentAppStateScene == Enumerators.AppState.GAMEPLAY)
            {
                _uiManager.SetPage<GameplayPage>();
                _uiManager.DrawPopup<PreparingForBattlePopup>();
            }


            _isLoadingStarted = false;
            IsLoadedScene = true;
            SceneLoadingProgress = 0;

            if (SceneForAppStateWasLoadedEvent != null)
                SceneForAppStateWasLoadedEvent(CurrentAppStateScene);
        }

        private IEnumerator LoadLevelAsync(string levelName)
        {
#if UNITY_5_3_OR_NEWER
            AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(levelName);
#else
            AsyncOperation asyncOperation = Application.LoadLevelAsync(levelName);
#endif
            while (!asyncOperation.isDone)
            {
                SceneLoadingProgress = Mathf.RoundToInt(asyncOperation.progress * 100f);
                yield return null;
            }
        }
    }
}