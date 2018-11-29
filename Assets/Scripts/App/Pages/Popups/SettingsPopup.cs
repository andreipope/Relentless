using System;
using System.Collections.Generic;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Helpers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Loom.ZombieBattleground
{
    public class SettingsPopup : IUIPopup
    {

        private ILoadObjectsManager _loadObjectsManager;
        private IUIManager _uiManager;
        private IGameplayManager _gameplayManager;
        private ISoundManager _soundManager;
        private IAppStateManager _appStateManager;
        private IApplicationSettingsManager _applicationSettingsManager;

        private ButtonShiftingContent _quitToMenuButton,
                                      _quitToDesktopButton,
                                      _settingsButton,
                                      _closeButton;
#if !UNITY_ANDROID && !UNITY_IOS
        private TMP_Dropdown _resolutionDropdown;
        private TMP_Dropdown _screenModeDropdown;
#endif
        private Slider _sfxVolumeDropdown,
                       _musicVolumeDropdown;

        private GameObject _panelVideoSettingsObject,
                           _panelAudioSettingsObject;

        private bool _initialInit = true;

        private bool _fromMainMenu = false;

        public GameObject Self { get; private set; }

        private ILocalizationManager _localizationManager;

        private TextMeshProUGUI _settingTextMesh;
        private TextMeshProUGUI _videoTextMesh;
        private TextMeshProUGUI _audioTextMesh;
        private TextMeshProUGUI _resolutionTextMesh;
        private TextMeshProUGUI _fullScreenTextMesh;
        private TextMeshProUGUI _sfxVolumeTextMesh;
        private TextMeshProUGUI _screenModeTextMesh;
        private TextMeshProUGUI _musicVolumeTextMesh;
        private TextMeshProUGUI _quitToDesktopTextMesh;
        private TextMeshProUGUI _quitToMainMenuTextMesh;

        public void Init()
        {

            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _uiManager = GameClient.Get<IUIManager>();
            _gameplayManager = GameClient.Get<IGameplayManager>();
            _soundManager = GameClient.Get<ISoundManager>();
            _appStateManager = GameClient.Get<IAppStateManager>();
            _applicationSettingsManager = GameClient.Get<IApplicationSettingsManager>();
            _localizationManager = GameClient.Get<ILocalizationManager>();

            _localizationManager.LanguageWasChangedEvent += LanguageWasChangedEventHandler;
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
            Self = Object.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Popups/SettingsPopup"));
            Self.transform.SetParent(_uiManager.Canvas3.transform, false);

            _quitToMenuButton = Self.transform.Find("Button_QuitToMainMenu").GetComponent<ButtonShiftingContent>();
            _quitToDesktopButton = Self.transform.Find("Button_QuitToDesktop").GetComponent<ButtonShiftingContent>();
            _settingsButton = Self.transform.Find("Button_Settings").GetComponent<ButtonShiftingContent>();
            _closeButton = Self.transform.Find("Button_Close").GetComponent<ButtonShiftingContent>();

            _panelVideoSettingsObject = Self.transform.Find("Panel_Settings/Panel_VideoSettings").gameObject;
            _panelAudioSettingsObject = Self.transform.Find("Panel_Settings/Panel_AudioSettings").gameObject;

            _settingTextMesh = Self.transform.Find("Image_TitleBackground/Text").GetComponent<TextMeshProUGUI>();
            _videoTextMesh = _panelVideoSettingsObject.transform.Find("Panel_Video/Text").GetComponent<TextMeshProUGUI>();
            _resolutionTextMesh = _panelVideoSettingsObject.transform.Find("Text_Resolution").GetComponent<TextMeshProUGUI>();
            _screenModeTextMesh = _panelVideoSettingsObject.transform.Find("Text_ScreenMode").GetComponent<TextMeshProUGUI>();
            _fullScreenTextMesh = _panelVideoSettingsObject.transform.Find("Dropdown_ScreenMode/Label").GetComponent<TextMeshProUGUI>();
            _audioTextMesh = _panelAudioSettingsObject.transform.Find("Panel_Audio/Text").GetComponent<TextMeshProUGUI>();
            _sfxVolumeTextMesh = _panelAudioSettingsObject.transform.Find("Text_SFXVolume").GetComponent<TextMeshProUGUI>();
            _musicVolumeTextMesh = _panelAudioSettingsObject.transform.Find("Text_MusicVolume").GetComponent<TextMeshProUGUI>();
            _quitToDesktopTextMesh = _quitToDesktopButton.transform.Find("Text").GetComponent<TextMeshProUGUI>();
            _quitToMainMenuTextMesh = _quitToMenuButton.transform.Find("Text").GetComponent<TextMeshProUGUI>();

#if !UNITY_ANDROID && !UNITY_IOS
            _resolutionDropdown = _panelVideoSettingsObject.transform.Find("Dropdown_Resolution").GetComponent<TMP_Dropdown>();
            _screenModeDropdown = _panelVideoSettingsObject.transform.Find("Dropdown_ScreenMode").GetComponent<TMP_Dropdown>();
#endif

            _sfxVolumeDropdown = _panelAudioSettingsObject.transform.Find("Slider_SFXVolume").GetComponent<Slider>();
            _musicVolumeDropdown = _panelAudioSettingsObject.transform.Find("Slider_MusicVolume").GetComponent<Slider>();

            _quitToMenuButton.onClick.AddListener(QuitToMenuButtonHandler);
            _quitToDesktopButton.onClick.AddListener(QuitToDesktopButtonHandler);
            _settingsButton.onClick.AddListener(SettingsButtonHandler);
            _closeButton.onClick.AddListener(CloseButtonHandler);

#if !UNITY_ANDROID && !UNITY_IOS
            _resolutionDropdown.onValueChanged.AddListener(ResolutionChangedHandler);
            _screenModeDropdown.onValueChanged.AddListener(ScreenModeChangedHandler);
#endif
            _sfxVolumeDropdown.onValueChanged.AddListener(SFXVolumeChangedHandler);
            _musicVolumeDropdown.onValueChanged.AddListener(MusicVolumeChangedHandler);

            _gameplayManager.IsGameplayInputBlocked = true;

            FillInfo();
            _appStateManager.SetPausingApp(true);

#if UNITY_ANDROID || UNITY_IOS
            _panelVideoSettingsObject.SetActive(false);
#endif

            UpdateLocalization();
        }

        public void Show(object data)
        {
            Show();

            if (data is bool isFromMainMenu && isFromMainMenu)
            {
                _fromMainMenu = isFromMainMenu;

                _quitToMenuButton.gameObject.SetActive(false);
                _settingsButton.gameObject.SetActive(false);
            }
        }

        public void Update()
        {
        }

        private void LanguageWasChangedEventHandler(Enumerators.Language obj)
        {
            UpdateLocalization();
        }

        private void UpdateLocalization()
        {
            if (Self == null)
                return;

            _settingTextMesh.text = _localizationManager.GetUITranslation(LocalizationKeys.SettingsText.ToString());
            _videoTextMesh.text = _localizationManager.GetUITranslation(LocalizationKeys.VideoText.ToString());
            _fullScreenTextMesh.text = _localizationManager.GetUITranslation(LocalizationKeys.FullScreenText.ToString());
            _audioTextMesh.text = _localizationManager.GetUITranslation(LocalizationKeys.AudioText.ToString());
            _resolutionTextMesh.text = _localizationManager.GetUITranslation(LocalizationKeys.ResolutionText.ToString());
            _sfxVolumeTextMesh.text = _localizationManager.GetUITranslation(LocalizationKeys.SfxVolumeText.ToString());
            _screenModeTextMesh.text = _localizationManager.GetUITranslation(LocalizationKeys.ScreenModeText.ToString());
            _musicVolumeTextMesh.text = _localizationManager.GetUITranslation(LocalizationKeys.MusicVolumeText.ToString());
            _quitToDesktopTextMesh.text = _localizationManager.GetUITranslation(LocalizationKeys.QuitToDesktopText.ToString());
            _quitToMainMenuTextMesh.text = _localizationManager.GetUITranslation(LocalizationKeys.QuitToMainMenuText.ToString());
        }


        private void FillInfo()
        {
            _initialInit = true;
#if !UNITY_ANDROID && !UNITY_IOS
            _resolutionDropdown.ClearOptions();
            _screenModeDropdown.ClearOptions();

            List<string> data = new List<string>();

            int length = Enum.GetNames(typeof(Enumerators.ScreenMode)).Length;

            for (int i = 0; i < length; i++)
            {
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


            _screenModeDropdown.value = (int)_applicationSettingsManager.CurrentScreenMode;
            _resolutionDropdown.value = _applicationSettingsManager.Resolutions.IndexOf(_applicationSettingsManager.CurrentResolution);
#endif
            _sfxVolumeDropdown.value = _soundManager.SoundVolume;
            _musicVolumeDropdown.value = _soundManager.MusicVolume;

            _initialInit = false;
        }


        private void CloseButtonHandler()
        {
            _soundManager.PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);

            _uiManager.HidePopup<SettingsPopup>();
        }

        private void QuitToMenuButtonHandler()
        {
            _soundManager.PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);

            Action[] actions = new Action[2];
            actions[0] = () =>
            {
                if (_gameplayManager.GetController<CardsController>().CardDistribution)
                {
                    _uiManager.HidePopup<MulliganPopup>();
                }

                _uiManager.HidePopup<SettingsPopup>();

                _uiManager.HidePopup<YourTurnPopup>();

                _gameplayManager.CurrentPlayer.ThrowLeaveMatch();

                _gameplayManager.EndGame(Enumerators.EndGameType.CANCEL);
                GameClient.Get<IMatchManager>().FinishMatch(Enumerators.AppState.MAIN_MENU);

                _soundManager.StopPlaying(Enumerators.SoundType.TUTORIAL);
                _soundManager.CrossfaidSound(Enumerators.SoundType.BACKGROUND, null, true);
            };

            _uiManager.DrawPopup<ConfirmationPopup>(actions);
            _soundManager.PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
        }

        private void QuitToDesktopButtonHandler()
        {
            _soundManager.PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
            _appStateManager.QuitApplication();
        }

        private void SettingsButtonHandler()
        {
            _soundManager.PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);

            _uiManager.HidePopup<SettingsPopup>();
        }
#if !UNITY_ANDROID && !UNITY_IOS
        private void ResolutionChangedHandler(int index)
        {
            if (!_initialInit)
            {
                _soundManager.PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);

                _applicationSettingsManager.SetResolution(_applicationSettingsManager.Resolutions[index]);
            }
        }

        private void ScreenModeChangedHandler(int index)
        {
            if (!_initialInit)
            {
                _soundManager.PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);

                _applicationSettingsManager.SetScreenMode((Enumerators.ScreenMode)index);
            }
        }
#endif
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

    }
}
