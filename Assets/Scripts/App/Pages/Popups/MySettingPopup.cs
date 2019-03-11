using System;
using System.Collections.Generic;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Helpers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Loom.ZombieBattleground
{
    public class MySettingPopup : IUIPopup
    {
        private ILoadObjectsManager _loadObjectsManager;
        private IUIManager _uiManager;
        private IGameplayManager _gameplayManager;
        private ISoundManager _soundManager;
        private IAppStateManager _appStateManager;
        private IApplicationSettingsManager _applicationSettingsManager;
        private ITutorialManager _tutorialManager;
        private BackendDataControlMediator _backendDataControlMediator;

        private Button _buttonClose,
                       _buttonLogin,
                       _buttonLogoff,
                       _buttonHelp,
                       _buttonSupport,
                       _buttonCredits;
                                      
        private Slider _sfxVolumeSlider,
                       _musicVolumeSlider;

        private bool _initialInit = true;

        public GameObject Self { get; private set; }

        public void Init()
        {

            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _uiManager = GameClient.Get<IUIManager>();
            _gameplayManager = GameClient.Get<IGameplayManager>();
            _soundManager = GameClient.Get<ISoundManager>();
            _appStateManager = GameClient.Get<IAppStateManager>();
            _applicationSettingsManager = GameClient.Get<IApplicationSettingsManager>();
            _tutorialManager = GameClient.Get<ITutorialManager>();
            _backendDataControlMediator = GameClient.Get<BackendDataControlMediator>();
        }

        public void Dispose()
        {
        }

        public void Hide()
        {
            if (Self == null)
                return;

            _appStateManager.SetPausingApp(false);
            Self.SetActive(false);
            Object.Destroy(Self);
            Self = null;

            _gameplayManager.IsGameplayInputBlocked = false;
        }

        public void SetMainPriority()
        {
        }

        public void Show()
        {
            Self = Object.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Popups/MySettingsPopup"));
            Self.transform.SetParent(_uiManager.Canvas3.transform, false);

            _buttonClose = Self.transform.Find("Scaler/Button_Close").GetComponent<Button>();
            _buttonClose.onClick.AddListener(ButtonCloseHandler);
            _buttonClose.onClick.AddListener(PlayClickSound);
            
            _buttonLogin = Self.transform.Find("Scaler/Button_Login").GetComponent<Button>();
            _buttonLogin.onClick.AddListener(ButtonLoginHandler);
            _buttonLogin.onClick.AddListener(PlayClickSound);
            
            _buttonLogoff = Self.transform.Find("Scaler/Button_Logoff").GetComponent<Button>();
            _buttonLogoff.onClick.AddListener(ButtonLogoffHandler);
            _buttonLogoff.onClick.AddListener(PlayClickSound);
            
            _buttonHelp = Self.transform.Find("Scaler/Button_Help").GetComponent<Button>();
            _buttonHelp.onClick.AddListener(ButtonHelpHandler);
            _buttonHelp.onClick.AddListener(PlayClickSound);
            
            _buttonSupport = Self.transform.Find("Scaler/Button_Support").GetComponent<Button>();
            _buttonSupport.onClick.AddListener(ButtonSupportHandler);
            _buttonSupport.onClick.AddListener(PlayClickSound);
            
            _buttonCredits = Self.transform.Find("Scaler/Button_Credits").GetComponent<Button>();
            _buttonCredits.onClick.AddListener(ButtonCreditsHandler);
            _buttonCredits.onClick.AddListener(PlayClickSound);
            
            _sfxVolumeSlider = Self.transform.Find("Scaler/Group_Sounds/Slider_SFXVolume").GetComponent<Slider>();
            _musicVolumeSlider = Self.transform.Find("Scaler/Group_Music/Slider_MusicVolume").GetComponent<Slider>();     
            
            _sfxVolumeSlider.onValueChanged.AddListener(SFXVolumeChangedHandler);
            _musicVolumeSlider.onValueChanged.AddListener(MusicVolumeChangedHandler);

            _gameplayManager.IsGameplayInputBlocked = true;

            FillInfo();
            _appStateManager.SetPausingApp(true);
        }

        public void Show(object data)
        {
            Show();
        }

        public void Update()
        {
            if (Self != null)
            {
                if (!Constants.AlwaysGuestLogin && 
                    _backendDataControlMediator.UserDataModel != null && 
                    (!_backendDataControlMediator.UserDataModel.IsRegistered || !_backendDataControlMediator.UserDataModel.IsValid))
                {
                    if (!_buttonLogin.gameObject.activeSelf)
                    {
                        _buttonLogin.gameObject.SetActive(true);
                        _buttonLogoff.gameObject.SetActive
                        (
                            !_buttonLogin.gameObject.activeSelf
                        );
                    }
                }
                else
                {
                    if (_buttonLogin.gameObject.activeSelf)
                    {
                        _buttonLogin.gameObject.SetActive(false);
                        _buttonLogoff.gameObject.SetActive
                        (
                            !_buttonLogin.gameObject.activeSelf
                        );
                    }
                }
            }
        }


        private void FillInfo()
        {
            _initialInit = true;
            _sfxVolumeSlider.value = _soundManager.SoundVolume;
            _musicVolumeSlider.value = _soundManager.MusicVolume;

            _initialInit = false;
        }


        private void ButtonCloseHandler()
        {
            _uiManager.HidePopup<MySettingPopup>();
        }
        
        private void ButtonLoginHandler()
        {
            Hide();
            LoginPopup popup = _uiManager.GetPopup<LoginPopup>();
            popup.Show();
        }
        
        private void ButtonLogoffHandler()
        {
            Hide();
            LoginPopup popup = _uiManager.GetPopup<LoginPopup>();
            popup.Logout();
        }
        
        private void ButtonHelpHandler()
        {
            Application.OpenURL(Constants.HelpLink);
        }
        
        private void ButtonSupportHandler()
        {
            Application.OpenURL(Constants.SupportLink);
        }
        
        private void ButtonCreditsHandler()
        {
            Hide();
            GameClient.Get<IAppStateManager>().ChangeAppState(Enumerators.AppState.CREDITS);
        }
        
        private void SFXVolumeChangedHandler(float value)
        {
            if (!_initialInit)
            {
                _soundManager.SetSoundVolume(value);
            }
        }

        private void MusicVolumeChangedHandler(float value)
        {
            if (!_initialInit)
            {
                _soundManager.SetMusicVolume(value);
            }
        }
        
        private void PlayClickSound()
        {
            _soundManager.PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
        }
    }
}
