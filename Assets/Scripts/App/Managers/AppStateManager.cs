﻿using GrandDevs.CZB.Common;
using System;
using UnityEngine;


namespace GrandDevs.CZB
{
    public sealed class AppStateManager : IService, IAppStateManager
    {
        private IUIManager _uiManager;
        private IDataManager _dataManager;
        private IPlayerManager _playerManager;
        private ILocalizationManager _localizationManager;
        private INotificationManager _notificationsManager;
		private IInputManager _inputManager;
        private IScenesManager _scenesManager;

        private float _backButtonTimer,
                      _backButtonResetDelay = 0.5f;
        private int _backButtonClicksCount;
        private bool _isBackButtonCounting;

        private bool _isAppPaused = false;

        private Enumerators.AppState _previouseState;
        private Enumerators.AppState _previouseState2;
        public Enumerators.AppState AppState { get; set; }

        public bool IsAppPaused 
        { 
            get
            {
                return _isAppPaused;
            } 
        }

        public void Dispose()
        {

        }

        public void Init()
        {
            _uiManager = GameClient.Get<IUIManager>();
            _dataManager = GameClient.Get<IDataManager>();
            _playerManager = GameClient.Get<IPlayerManager>();
            _localizationManager = GameClient.Get<ILocalizationManager>();
            _notificationsManager = GameClient.Get<INotificationManager>();
			_inputManager = GameClient.Get<IInputManager>();
            _scenesManager = GameClient.Get<IScenesManager>();
        }

        public void Update()
        {
            CheckBackButton();
        }

        public void ChangeAppState(Enumerators.AppState stateTo)
        {
            if (AppState == stateTo)
                return;

            switch (stateTo)
            {
                case Enumerators.AppState.APP_INIT:
                    {
                        _uiManager.SetPage<LoadingPage>();
                        _dataManager.StartLoadCache();
                        GameClient.Get<ISoundManager>().PlaySound(Common.Enumerators.SoundType.BACKGROUND, 128, Constants.BACKGROUND_SOUND_VOLUME, null, true);
                    }
                    break;
                case Enumerators.AppState.LOGIN:
                    break;
                case Enumerators.AppState.MAIN_MENU:
                    {
                        //GameObject.Find("MainApp/Camera").SetActive(true);
                        //GameObject.Find("MainApp/Camera2").SetActive(true);
                        _uiManager.SetPage<MainMenuPage>();
                    }
                    break;
                case Enumerators.AppState.HERO_SELECTION:
					{
                        _uiManager.SetPage<HeroSelectionPage>();
					}
					break;
                case Enumerators.AppState.DECK_SELECTION:
					{
						_uiManager.SetPage<DeckSelectionPage>();
					}
					break;
                case Enumerators.AppState.COLLECTION:
					{
                        _uiManager.SetPage<CollectionPage>();
					}
					break;
                case Enumerators.AppState.DECK_EDITING:
                    {
                        _uiManager.SetPage<DeckEditingPage>();
                    }
                    break;
                case Enumerators.AppState.SHOP:
                    {
                        _uiManager.SetPage<ShopPage>();
                    }
                    break;
                case Enumerators.AppState.PACK_OPENER:
                    {
                        _uiManager.SetPage<PackOpenerPage>();
                    }
                    break;
                case Enumerators.AppState.GAMEPLAY:
                    {
                        _uiManager.HideAllPages();

                        //GameObject.Find("MainApp/Camera").SetActive(false);
                        //GameObject.Find("MainApp/Camera2").SetActive(false);


                        // GameNetworkManager.Instance.onlineScene = "GAMEPLAY";

                        if (MonoBehaviour.FindObjectOfType<GameNetworkManager>() == null)
                            MonoBehaviour.Instantiate(Resources.Load<GameObject>("Prefabs/GameNetworkManager"));

                        GameNetworkManager.Instance.StartMatchMaker();
                        GameNetworkManager.Instance.isSinglePlayer = true;
                        GameNetworkManager.Instance.StartHost();

                        MatchMaker.Instance.StartMatch();

                        //GameClient.Get<ISoundManager>().PlaySound(Common.Enumerators.SoundType.BATTLEGROUND, 128, Constants.BACKGROUND_SOUND_VOLUME, null, true);
                        //_scenesManager.ChangeScene(Enumerators.AppState.GAMEPLAY);
                        /*MainApp.Instance.OnLevelWasLoadedEvent += (param) => {
							GameNetworkManager.Instance.StartMatchMaker();
							GameNetworkManager.Instance.isSinglePlayer = true;
							GameNetworkManager.Instance.StartHost();
                        };*/
                    }
                    break;
                case Enumerators.AppState.CREDITS:
                    {
                        _uiManager.SetPage<CreditsPage>();
                    }
                    break;
                default:
                    throw new NotImplementedException("Not Implemented " + stateTo.ToString() + " state!");
            }

            if (AppState != Enumerators.AppState.SHOP)
                _previouseState = AppState;
            else
                _previouseState = Enumerators.AppState.MAIN_MENU;

            AppState = stateTo;
        }

        public void BackAppState()
        {
            ChangeAppState(_previouseState); 
        }

        public void PauseGame(bool enablePause)
        {
            if (enablePause)
            {
                Time.timeScale = 0;
            }
            else
            {
                Time.timeScale = 1;
            }

            _isAppPaused = enablePause;
        }

        private void CheckBackButton()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                _isBackButtonCounting = true;
                _backButtonClicksCount++;
                _backButtonTimer = 0f;

                if (_backButtonClicksCount >= 2)
                {
                    Application.Quit();
                }
            }

            if (_isBackButtonCounting)
            {
                _backButtonTimer += Time.deltaTime;

                if (_backButtonTimer >= _backButtonResetDelay)
                {
                    _backButtonTimer = 0f;
                    _backButtonClicksCount = 0;
                    _isBackButtonCounting = false;
                }
            }
        }
    }
}