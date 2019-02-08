using System;
using Loom.ZombieBattleground.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using Loom.ZombieBattleground.BackendCommunication;

namespace Loom.ZombieBattleground
{
    public class MainMenuWithNavigationPage : IUIElement
    {
        private IUIManager _uiManager;

        private ILoadObjectsManager _loadObjectsManager;

        private IAppStateManager _stateManager;
        
        private ISoundManager _soundManager;
        
        private IPlayerManager _playerManager;
        
        private IDataManager _dataManager;
        
        private GameObject _selfPage;
        
        private Button _buttonLogin;

        private Button _buttonPlay;
        
        private BackendDataControlMediator _backendDataControlMediator;
        
        private bool _isReturnToTutorial;
        
        public void Init()
        {
            _uiManager = GameClient.Get<IUIManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _stateManager = GameClient.Get<IAppStateManager>();
            _soundManager = GameClient.Get<ISoundManager>();
            _playerManager = GameClient.Get<IPlayerManager>();
            _dataManager = GameClient.Get<IDataManager>();
            _backendDataControlMediator = GameClient.Get<BackendDataControlMediator>();
        }
        
        public void Update()
        {
            if (_selfPage != null)
            {
                if (!Constants.AlwaysGuestLogin && 
                    _backendDataControlMediator.UserDataModel != null && 
                    (!_backendDataControlMediator.UserDataModel.IsRegistered || !_backendDataControlMediator.UserDataModel.IsValid))
                {
                    if (!_buttonLogin.gameObject.activeSelf)
                    {
                        _buttonLogin.gameObject.SetActive(true);
                    }
                }
                else
                {
                    if (_buttonLogin.gameObject.activeSelf)
                    {
                        _buttonLogin.gameObject.SetActive(false);
                    }
                }
            }
        }

        public void Show()
        {
            _selfPage = Object.Instantiate(
                _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Pages/MainMenuWithNavigationPage"));
            _selfPage.transform.SetParent(_uiManager.Canvas.transform, false);
            
            _buttonLogin = _selfPage.transform.Find("Button_Login").GetComponent<Button>();
            _buttonPlay = _selfPage.transform.Find("Anchor_BottomRight/Scaler/Panel_BattleSwitch/Button_Battle").GetComponent<Button>();
            
            _buttonLogin.onClick.AddListener(PressedLoginHandler);
            _buttonPlay.onClick.AddListener(OnClickPlay);
            
            _isReturnToTutorial = GameClient.Get<ITutorialManager>().UnfinishedTutorial;
            _uiManager.DrawPopup<SideMenuPopup>(SideMenuPopup.MENU.BATTLE);
        }
        
        public void Hide()
        {
            if (_selfPage == null)
                return;

            _selfPage.SetActive(false);
            Object.Destroy(_selfPage);
            _selfPage = null;

            OnHide();
        }

        public void Dispose()
        {
        }

        private void PressedLoginHandler() 
        {
            //if (GameClient.Get<ITutorialManager>().IsButtonBlockedInTutorial(_buttonBuy.name) || _isReturnToTutorial)
            //{
            //    GameClient.Get<ITutorialManager>().ReportActivityAction(Enumerators.TutorialActivityAction.IncorrectButtonTapped);
            //    return;
            //}

            _soundManager.PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);

            LoginPopup popup = _uiManager.GetPopup<LoginPopup>();
            popup.Show();
        }

        private void OpenAlertDialog(string msg)
        {
            _uiManager.DrawPopup<WarningPopup>(msg);
        }
        
        private void OnHide()
        {
            _uiManager.HidePopup<SideMenuPopup>();
        }

        #region Buttons Handlers

        private void OnClickPlay()
        {
            if (GameClient.Get<ITutorialManager>().IsButtonBlockedInTutorial(_buttonPlay.name))
            {
                GameClient.Get<ITutorialManager>().ReportActivityAction(Enumerators.TutorialActivityAction.IncorrectButtonTapped);
                return;
            }
            else if (_isReturnToTutorial)
            {
                GameClient.Get<ITutorialManager>().ReportActivityAction(Enumerators.TutorialActivityAction.BattleStarted);

                GameClient.Get<IMatchManager>().FindMatch();
                return;
            }

            _soundManager.PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);

            _stateManager.ChangeAppState(Enumerators.AppState.PlaySelection);
        }

        #endregion

    }
}
