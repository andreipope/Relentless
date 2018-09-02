using System;
using LoomNetwork.CZB.Common;
using UnityEngine;

namespace LoomNetwork.CZB
{
    public sealed class AppStateManager : IService, IAppStateManager
    {
        private const bool DisableShop = true;

        private const bool DisablePacks = true;

        private const float BackButtonResetDelay = 0.5f;

        private IUIManager _uiManager;

        private float _backButtonTimer;

        private int _backButtonClicksCount;

        private bool _isBackButtonCounting;

        private Enumerators.AppState _previousState;

        private Enumerators.AppState _previousState2;

        public bool IsAppPaused { get; private set; }

        public Enumerators.AppState AppState { get; set; }

        public void ChangeAppState(Enumerators.AppState stateTo)
        {
            if (AppState == stateTo)
                return;

            switch (stateTo)
            {
                case Enumerators.AppState.APP_INIT:
                {
                    _uiManager.SetPage<LoadingPage>();
                    GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.BACKGROUND, 128, Constants.BackgroundSoundVolume, null, true);
                }

                    break;
                case Enumerators.AppState.LOGIN:
                    break;
                case Enumerators.AppState.MAIN_MENU:
                    _uiManager.SetPage<MainMenuPage>();
                    break;
                case Enumerators.AppState.HERO_SELECTION:
                    _uiManager.SetPage<HeroSelectionPage>();
                    break;
                case Enumerators.AppState.DECK_SELECTION:
                    _uiManager.SetPage<HordeSelectionPage>();
                    break;
                case Enumerators.AppState.COLLECTION:
                    _uiManager.SetPage<CollectionPage>();
                    break;
                case Enumerators.AppState.DECK_EDITING:
                    _uiManager.SetPage<DeckEditingPage>();
                    break;
                case Enumerators.AppState.SHOP:
                    if (!DisableShop)
                    {
                        _uiManager.SetPage<ShopPage>();
                    }
                    else
                    {
                        _uiManager.DrawPopup<WarningPopup>($"The Shop is Disabled\nfor version {BuildMetaInfo.Instance.DisplayVersionName}\n\n Thanks for helping us make this game Awesome\n\n-Loom Team");
                        return;
                    }

                    break;
                case Enumerators.AppState.PACK_OPENER:
                {
                    if (!DisablePacks)
                    {
                        _uiManager.SetPage<PackOpenerPage>();
                    }
                    else
                    {
                        _uiManager.DrawPopup<WarningPopup>($"The Pack Opener is Disabled\nfor version {BuildMetaInfo.Instance.DisplayVersionName}\n\n Thanks for helping us make this game Awesome\n\n-Loom Team");
                        return;
                    }

                    break;
                }
                case Enumerators.AppState.GAMEPLAY:
                    _uiManager.SetPage<GameplayPage>();
                    break;
                case Enumerators.AppState.CREDITS:
                    _uiManager.SetPage<CreditsPage>();
                    break;
                default:
                    throw new NotImplementedException("Not Implemented " + stateTo + " state!");
            }

            _previousState = AppState != Enumerators.AppState.SHOP ? AppState : Enumerators.AppState.MAIN_MENU;

            AppState = stateTo;
        }

        public void BackAppState()
        {
            ChangeAppState(_previousState);
        }

        public void Dispose()
        {
        }

        public void Init()
        {
            _uiManager = GameClient.Get<IUIManager>();
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

                if (_backButtonTimer >= BackButtonResetDelay)
                {
                    _backButtonTimer = 0f;
                    _backButtonClicksCount = 0;
                    _isBackButtonCounting = false;
                }
            }
        }
    }
}
