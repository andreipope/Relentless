using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Helpers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Loom.ZombieBattleground
{
    public class SettingsWithCreditsPopup : IUIPopup
    {
        private ILoadObjectsManager _loadObjectsManager;
        private IUIManager _uiManager;
        private IGameplayManager _gameplayManager;
        private ISoundManager _soundManager;
        private IAppStateManager _appStateManager;
        private IApplicationSettingsManager _applicationSettingsManager;
        private ITutorialManager _tutorialManager;
        private BackendDataControlMediator _backendDataControlMediator;

        public Action<bool> OnLoginButtonDisplayUpdate;
        
        private GameObject _panelVideoSettings,
                           _groupLogin;

        private Button _buttonClose,
                       _buttonLogin,
                       _buttonLogout,
                       _buttonQuitToMainMenu,
                       _buttonQuitToDesktop,
                       _buttonLeaveMatch,
                       _buttonHelp,
                       _buttonSupport,
                       _buttonCredits;
                                      
        private Slider _sfxVolumeSlider,
                       _musicVolumeSlider;
                       
#if !UNITY_ANDROID && !UNITY_IOS
        private TMP_Dropdown _resolutionDropdown;
        private TMP_Dropdown _screenModeDropdown;
#endif

        private const float ScrollSensitivityForWindows = 25f;

        private bool _infoDataFilled;

        public GameObject Self { get; private set; }

        private Resolution _cachePreviousFrameResolution;
        
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
#if !UNITY_ANDROID && !UNITY_IOS
            ApplicationSettingsManager.OnResolutionChanged += RefreshSettingPopup;
#endif
            _cachePreviousFrameResolution = Screen.currentResolution;
            ApplicationSettingsManager.OnResolutionChanged += FixSliderAndDropdownZPosition;
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
            OnLoginButtonDisplayUpdate?.Invoke(false);
        }

        public void SetMainPriority()
        {
        }

        public void Show()
        {
            Self = Object.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Popups/SettingsWithCreditsPopup"));
            Self.transform.SetParent(_uiManager.Canvas3.transform, false);

            _infoDataFilled = false;
            _cachePreviousFrameResolution = Screen.currentResolution;

            _buttonClose = Self.transform.Find("Button_Close").GetComponent<Button>();
            _buttonClose.onClick.AddListener(ButtonCloseHandler);
            
            _groupLogin = Self.transform.Find("Group_Login").gameObject;
            
            _buttonLogin = Self.transform.Find("Group_Login/Button_Login").GetComponent<Button>();
            _buttonLogin.onClick.AddListener(ButtonLoginHandler);
            
            _buttonLogout = Self.transform.Find("Group_Login/Button_Logout").GetComponent<Button>();
            _buttonLogout.onClick.AddListener(ButtonLogoutHandler);
            
            _buttonQuitToMainMenu = Self.transform.Find("Button_QuitToMainMenu").GetComponent<Button>();
            _buttonQuitToMainMenu.onClick.AddListener(ButtonQuitToMainMenuHandler);
            _buttonQuitToMainMenu.gameObject.SetActive(false);
            
            _buttonQuitToDesktop = Self.transform.Find("Button_QuitToDeskTop").GetComponent<Button>();
            _buttonQuitToDesktop.onClick.AddListener(ButtonQuitToDesktopHandler);
            
            _buttonLeaveMatch = Self.transform.Find("Tray_Right/Button_LeaveMatch").GetComponent<Button>();
            _buttonLeaveMatch.onClick.AddListener(ButtonLeaveMatchHandler);
            
            _buttonHelp = Self.transform.Find("Tray_Right/Button_Help").GetComponent<Button>();
            _buttonHelp.onClick.AddListener(ButtonHelpHandler);
            
            _buttonSupport = Self.transform.Find("Tray_Right/Button_Support").GetComponent<Button>();
            _buttonSupport.onClick.AddListener(ButtonSupportHandler);
            
            _buttonCredits = Self.transform.Find("Tray_Right/Button_Credits").GetComponent<Button>();
            _buttonCredits.onClick.AddListener(ButtonCreditsHandler);
            
            _sfxVolumeSlider = Self.transform.Find("Panel_Group/GroupAudio/Slider_SFXVolume").GetComponent<Slider>();
            _musicVolumeSlider = Self.transform.Find("Panel_Group/GroupAudio/Slider_MusicVolume").GetComponent<Slider>();     
            
            _sfxVolumeSlider.onValueChanged.AddListener(SFXVolumeChangedHandler);
            _musicVolumeSlider.onValueChanged.AddListener(MusicVolumeChangedHandler);
            
            _gameplayManager.IsGameplayInputBlocked = true;
            
            if (_appStateManager.AppState == Enumerators.AppState.GAMEPLAY)
            {
                _buttonCredits.gameObject.SetActive(false);
                _appStateManager.SetPausingApp(true);
            }
            
            _buttonLeaveMatch.gameObject.SetActive(_appStateManager.AppState == Enumerators.AppState.GAMEPLAY);
            _groupLogin.SetActive(_appStateManager.AppState != Enumerators.AppState.GAMEPLAY);
          
            _panelVideoSettings = Self.transform.Find("Panel_Group/GroupVideo").gameObject;
            
#if UNITY_ANDROID || UNITY_IOS
            _buttonQuitToDesktop.transform.Find("Shifted/Text").GetComponent<TextMeshProUGUI>().text = "QUIT";
            _panelVideoSettings.SetActive(false);
#else
            _resolutionDropdown = _panelVideoSettings.transform.Find("Dropdown_Resolution").GetComponent<TMP_Dropdown>();
            _screenModeDropdown = _panelVideoSettings.transform.Find("Dropdown_ScreenMode").GetComponent<TMP_Dropdown>();
            #if UNITY_STANDALONE_WIN
            _resolutionDropdown.transform.Find("Template").GetComponent<ScrollRect>().scrollSensitivity = ScrollSensitivityForWindows;
            _screenModeDropdown.transform.Find("Template").GetComponent<ScrollRect>().scrollSensitivity = ScrollSensitivityForWindows;
            #endif
#endif
            FixSliderAndDropdownZPosition();
            FillInfo();
            LoadSettingData();

            #if !UNITY_ANDROID && !UNITY_IOS
            _resolutionDropdown.onValueChanged.AddListener(ResolutionChangedHandler);
            _screenModeDropdown.onValueChanged.AddListener(ScreenModeChangedHandler);
            #endif

            OnLoginButtonDisplayUpdate?.Invoke(true);
        }

        public void Show(object data)
        {
            Show();
        }

        public void Update()
        {  
            if (Self == null)
                return;

            if (_appStateManager.AppState == Enumerators.AppState.GAMEPLAY)
                return;            
            
            DetectIfMonitorResolutionWasChanged();
            
            if (!Constants.AlwaysGuestLogin && 
                _backendDataControlMediator.UserDataModel != null && 
                (!_backendDataControlMediator.UserDataModel.IsRegistered || !_backendDataControlMediator.UserDataModel.IsValid))
            {
                if (!_buttonLogin.gameObject.activeSelf)
                {
                    _buttonLogin.gameObject.SetActive(true);
                    _buttonLogout.gameObject.SetActive(false);
                    OnLoginButtonDisplayUpdate?.Invoke(true);
                }
            }
            else
            {
                if (_buttonLogin.gameObject.activeSelf)
                {
                    _buttonLogin.gameObject.SetActive(false);
                    _buttonLogout.gameObject.SetActive(true);    
                    OnLoginButtonDisplayUpdate?.Invoke(false);  
                }
            }         
        }

        private void DetectIfMonitorResolutionWasChanged()
        {
#if !UNITY_ANDROID && !UNITY_IOS
            if
            ( 
                _cachePreviousFrameResolution.height != Screen.currentResolution.height ||
                _cachePreviousFrameResolution.width != Screen.currentResolution.width 
            )
            {
                _applicationSettingsManager.FillResolutions();
                ResolutionInfo resolutionInfo = _applicationSettingsManager.AddResolution(new Resolution
                {
                    width = Screen.width,
                    height = Screen.height
                });
                FillInfo();
                _resolutionDropdown.value = _applicationSettingsManager.Resolutions.IndexOf(resolutionInfo);
            }

            _cachePreviousFrameResolution = Screen.currentResolution;
#endif
        }
        
        private void FixSliderAndDropdownZPosition()
        {
            if (_musicVolumeSlider == null || _sfxVolumeSlider == null)
                return;

#if !UNITY_ANDROID && !UNITY_IOS
            if (_resolutionDropdown == null || _screenModeDropdown == null)
                return;
#endif

            List<Transform> transformsList = new List<Transform>()
            {
                _musicVolumeSlider.transform,
                _sfxVolumeSlider.transform
#if !UNITY_ANDROID && !UNITY_IOS
                ,
                _resolutionDropdown.transform,
                _screenModeDropdown.transform
#endif
            };
            Vector3 pos;
            foreach(Transform tran in transformsList)
            {
                pos = tran.localPosition;
                pos.z = 0f;
                tran.localPosition = pos;
            }
        }

        private void FillInfo()
        {            
#if !UNITY_ANDROID && !UNITY_IOS
            _resolutionDropdown.ClearOptions();
            _screenModeDropdown.ClearOptions();

            List<string> data = new List<string>();

            int length = Enum.GetNames(typeof(Enumerators.ScreenMode)).Length;

            for (int i = 0; i < length; i++)
            {
#if UNITY_STANDALONE_WIN
                if ((Enumerators.ScreenMode)i == Enumerators.ScreenMode.BorderlessWindow)
                    continue;
#endif
                data.Add(InternalTools.ProccesEnumToString(((Enumerators.ScreenMode)i).ToString()));
            }
            _screenModeDropdown.AddOptions(data);

            data.Clear();
            length = _applicationSettingsManager.Resolutions.Count;            

            for (int i = 0; i < length; i++)
            {
                data.Add(_applicationSettingsManager.Resolutions[i].Name);
            }
            _resolutionDropdown.AddOptions(data);
#endif 
            _infoDataFilled = true;
        }
        
        private void LoadSettingData()
        {
#if !UNITY_ANDROID && !UNITY_IOS
            _screenModeDropdown.value = (int)_applicationSettingsManager.CurrentScreenMode;
            _resolutionDropdown.value = _applicationSettingsManager.Resolutions.IndexOf(_applicationSettingsManager.CurrentResolution);
#endif
            
            _sfxVolumeSlider.value = _soundManager.SoundVolume;
            _musicVolumeSlider.value = _soundManager.MusicVolume;
        }
        
#if !UNITY_ANDROID && !UNITY_IOS
        private async void ResolutionChangedHandler(int index)
        {
            if (_infoDataFilled)
            {
                PlayClickSound();

                _applicationSettingsManager.SetResolution(_applicationSettingsManager.Resolutions[index]);
                
                Hide();
                GameClient.Get<IUIManager>().DrawPopup<LoadingFiatPopup>("Apply settings ...");
                await Task.Delay(TimeSpan.FromSeconds
                (
                    ApplicationSettingsManager.WaitForResolutionChangeFinishAnimating
                ));
                GameClient.Get<IUIManager>().HidePopup<LoadingFiatPopup>();
                Show();                
            }
        }

        private void ScreenModeChangedHandler(int index)
        {
            if (_infoDataFilled)
            {
                PlayClickSound();
                
                _applicationSettingsManager.SetScreenMode((Enumerators.ScreenMode)index);
            }
        }
#endif
        
        private void ButtonLoginHandler()
        {
            PlayClickSound();
            Hide();
            LoginPopup popup = _uiManager.GetPopup<LoginPopup>();
            popup.Show();
        }
        
        private void ButtonLogoutHandler()
        {
            PlayClickSound();
            _uiManager.GetPopup<QuestionPopup>().ConfirmationReceived += ConfirmLogout;
            _uiManager.DrawPopup<QuestionPopup>("Would you like to logout?"); 
        }
        
        private void ConfirmLogout(bool status)
        {
            _uiManager.GetPopup<QuestionPopup>().ConfirmationReceived -= ConfirmLogout;
            if(status)
            {
                Hide();
                LoginPopup popup = _uiManager.GetPopup<LoginPopup>();
                popup.Logout();
            }
        }
        
        private void ButtonQuitToMainMenuHandler()
        {
            PlayClickSound();
            HandleQuitToMainMenu();          
        }
        
        private void ButtonQuitToDesktopHandler()
        {
            PlayClickSound();
            _appStateManager.QuitApplication();
        }

        private void ButtonCloseHandler()
        {
            PlayClickSound();
            _uiManager.HidePopup<SettingsWithCreditsPopup>();
        }
        
        private void ButtonLeaveMatchHandler()
        {
            PlayClickSound();
            Action[] actions = new Action[2];
            actions[0] = () =>
            {
                if (_gameplayManager.IsGameEnded)
                {
                    HandleQuitToMainMenu();
                    return;
                }

                _gameplayManager.CurrentPlayer?.ThrowLeaveMatch();

                _gameplayManager.EndGame(Enumerators.EndGameType.CANCEL);

                HandleQuitToMainMenu();
            };

            _uiManager.DrawPopup<ConfirmationPopup>(actions);
        }
        
        private void ButtonHelpHandler()
        {
            PlayClickSound();
            _uiManager.GetPopup<QuestionPopup>().ConfirmationReceived += ConfirmRedirectHelpLink;
            _uiManager.DrawPopup<QuestionPopup>("Would you like to redirect to help link?");            
        }
        
        private void ConfirmRedirectHelpLink(bool status)
        {
            _uiManager.GetPopup<QuestionPopup>().ConfirmationReceived -= ConfirmRedirectHelpLink;
            if(status)
            {
                Application.OpenURL(Constants.HelpLink);
            }
        }

        private void ButtonSupportHandler()
        {
            PlayClickSound();
            _uiManager.GetPopup<QuestionPopup>().ConfirmationReceived += ConfirmRedirectSupportLink;
            _uiManager.DrawPopup<QuestionPopup>("Would you like to redirect to support link?");
        }
        
        private void ConfirmRedirectSupportLink(bool status)
        {
            _uiManager.GetPopup<QuestionPopup>().ConfirmationReceived -= ConfirmRedirectSupportLink;
            if(status)
            {
                Application.OpenURL(Constants.SupportLink);
            }
        }
        
        private void ButtonCreditsHandler()
        {
            PlayClickSound();
            Hide();
            _uiManager.DrawPopup<CreditPopup>();
        }
        
        private void SFXVolumeChangedHandler(float value)
        {
            if (_infoDataFilled)
            {
                _soundManager.SetSoundVolume(value);
            }
        }

        private void MusicVolumeChangedHandler(float value)
        {
            if (_infoDataFilled)
            {
                _soundManager.SetMusicVolume(value);
            }
        }
        
        private void PlayClickSound()
        {
            _soundManager.PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
        }

        private void HandleQuitToMainMenu()
        {
            if (_gameplayManager.GetController<CardsController>().CardDistribution)
            {
                _uiManager.HidePopup<MulliganPopup>();
            }

            Hide();

            _uiManager.HidePopup<YourTurnPopup>();

            if(_tutorialManager.IsTutorial)
            {
                _tutorialManager.UnfinishedTutorial = true;
            }
            GameClient.Get<IMatchManager>().FinishMatch(Enumerators.AppState.MAIN_MENU);

            _soundManager.StopPlaying(Enumerators.SoundType.TUTORIAL);
            _soundManager.CrossfaidSound(Enumerators.SoundType.BACKGROUND, null, true);
        }
        
        private void RefreshSettingPopup()
        {
#if !UNITY_ANDROID && !UNITY_IOS
            if (Self != null)
            {
                _screenModeDropdown.value = (int)_applicationSettingsManager.CurrentScreenMode;
                _resolutionDropdown.value = _applicationSettingsManager.Resolutions.IndexOf(_applicationSettingsManager.CurrentResolution);
            }
#endif
        }

    }
}