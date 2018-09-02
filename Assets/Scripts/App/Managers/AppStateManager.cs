using System;
using LoomNetwork.CZB.Common;
using UnityEngine;

namespace LoomNetwork.CZB
{
    public sealed class AppStateManager : IService, IAppStateManager
    {
        private readonly bool _disableShop = true;

        private readonly bool _disablePacks = true;

        private readonly float _backButtonResetDelay = 0.5f;

        private IUIManager _uiManager;

        private IDataManager _dataManager;

        private IPlayerManager _playerManager;

        private ILocalizationManager _localizationManager;

        private IInputManager _inputManager;

        private IScenesManager _scenesManager;

        private float _backButtonTimer;

        private int _backButtonClicksCount;

        private bool _isBackButtonCounting;

        private Enumerators.AppState _previouseState;

        private Enumerators.AppState _previouseState2;

        public bool IsAppPaused { get; private set; }

        public Enumerators.AppState AppState { get; set; }

        public void ChangeAppState(Enumerators.AppState stateTo)
        {
            if (AppState == stateTo)

                return;

            switch (stateTo)
            {
                case Enumerators.AppState.AppInit:
                {
                    _uiManager.SetPage<LoadingPage>();
                    GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.Background, 128, Constants.BackgroundSoundVolume, null, true, false, true);
                }

                    break;
                case Enumerators.AppState.Login:
                    break;
                case Enumerators.AppState.MainMenu:
                {
                    // GameObject.Find("MainApp/Camera").SetActive(true);
                    // GameObject.Find("MainApp/Camera2").SetActive(true);
                    _uiManager.SetPage<MainMenuPage>();
                }

                    break;
                case Enumerators.AppState.HeroSelection:
                {
                    _uiManager.SetPage<HeroSelectionPage>();
                }

                    break;
                case Enumerators.AppState.DeckSelection:
                {
                    _uiManager.SetPage<HordeSelectionPage>();
                }

                    break;
                case Enumerators.AppState.Collection:
                {
                    _uiManager.SetPage<CollectionPage>();
                }

                    break;
                case Enumerators.AppState.DeckEditing:
                {
                    _uiManager.SetPage<DeckEditingPage>();
                }

                    break;
                case Enumerators.AppState.Shop:
                {
                    if (!_disableShop)
                    {
                        _uiManager.SetPage<ShopPage>();
                    }
                    else
                    {
                        _uiManager.DrawPopup<WarningPopup>($"The Shop is Disabled\nfor version {BuildMetaInfo.Instance.DisplayVersionName}\n\n Thanks for helping us make this game Awesome\n\n-Loom Team");
                        return;
                    }
                }

                    break;
                case Enumerators.AppState.PackOpener:
                {
                    if (!_disablePacks)
                    {
                        _uiManager.SetPage<PackOpenerPage>();
                    }
                    else
                    {
                        _uiManager.DrawPopup<WarningPopup>($"The Pack Opener is Disabled\nfor version {BuildMetaInfo.Instance.DisplayVersionName}\n\n Thanks for helping us make this game Awesome\n\n-Loom Team");
                        return;
                    }
                }

                    break;
                case Enumerators.AppState.Gameplay:
                {
                    _uiManager.SetPage<GameplayPage>();

                    // GameObject.Find("MainApp/Camera").SetActive(false);
                    // GameObject.Find("MainApp/Camera2").SetActive(false);

                    // GameNetworkManager.Instance.onlineScene = "GAMEPLAY";

                    // MatchMaker.Instance.StartMatch();

                    // GameClient.Get<ISoundManager>().PlaySound(Common.Enumerators.SoundType.BATTLEGROUND, 128, Constants.BACKGROUND_SOUND_VOLUME, null, true);
                    // _scenesManager.ChangeScene(Enumerators.AppState.GAMEPLAY);
                    /*MainApp.Instance.OnLevelWasLoadedEvent += (param) => {
                        GameNetworkManager.Instance.StartMatchMaker();
                        GameNetworkManager.Instance.isSinglePlayer = true;
                        GameNetworkManager.Instance.StartHost();
                    };*/
                }

                    break;
                case Enumerators.AppState.Credits:
                {
                    _uiManager.SetPage<CreditsPage>();
                }

                    break;
                default:
                    throw new NotImplementedException("Not Implemented " + stateTo + " state!");
            }

            if (AppState != Enumerators.AppState.Shop)
            {
                _previouseState = AppState;
            }
            else
            {
                _previouseState = Enumerators.AppState.MainMenu;
            }

            AppState = stateTo;
        }

        public void BackAppState()
        {
            ChangeAppState(_previouseState);
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
            _inputManager = GameClient.Get<IInputManager>();
            _scenesManager = GameClient.Get<IScenesManager>();
        }

        public void Update()
        {
            CheckBackButton();
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

            IsAppPaused = enablePause;
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
