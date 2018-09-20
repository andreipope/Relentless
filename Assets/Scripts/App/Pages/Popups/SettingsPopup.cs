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

        private Dropdown _qualityDropdown,
                         _resolutionDropdown,
                         _screenModeDropdown;

        private Slider _sfxVolumeDropdown,
                       _musicVolumeDropdown;

        public GameObject Self { get; private set; }

        public void Init()
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _uiManager = GameClient.Get<IUIManager>();
            _gameplayManager = GameClient.Get<IGameplayManager>();
            _soundManager = GameClient.Get<ISoundManager>();
            _appStateManager = GameClient.Get<IAppStateManager>();
            _applicationSettingsManager = GameClient.Get<IApplicationSettingsManager>();
        }

        public void Dispose()
        {
        }

        public void Hide()
        {
            if (Self == null)
                return;

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
            Self.transform.SetParent(_uiManager.Canvas2.transform, false);

            _quitToMenuButton = Self.transform.Find("Button_QuitToMainMenu").GetComponent<ButtonShiftingContent>();
            _quitToDesktopButton = Self.transform.Find("Button_QuitToDesktop").GetComponent<ButtonShiftingContent>();
            _settingsButton = Self.transform.Find("Button_Settings").GetComponent<ButtonShiftingContent>();
            _closeButton = Self.transform.Find("Button_Close").GetComponent<ButtonShiftingContent>();

            _qualityDropdown = Self.transform.Find("Dropdown_Quality").GetComponent<Dropdown>();
            _resolutionDropdown = Self.transform.Find("Dropdown_Resolution").GetComponent<Dropdown>();
            _screenModeDropdown = Self.transform.Find("Dropdown_ScreenMode").GetComponent<Dropdown>();

            _sfxVolumeDropdown = Self.transform.Find("Slider_SFXVolume").GetComponent<Slider>();
            _musicVolumeDropdown = Self.transform.Find("Slider_MusicVolume").GetComponent<Slider>();


            _quitToMenuButton.onClick.AddListener(QuitToMenuButtonHandler);
            _quitToDesktopButton.onClick.AddListener(QuitToDesktopButtonHandler);
            _settingsButton.onClick.AddListener(SettingsButtonHandler);
            _closeButton.onClick.AddListener(CloseButtonHandler);

            _qualityDropdown.onValueChanged.AddListener(QualityChangedHandler);
            _resolutionDropdown.onValueChanged.AddListener(ResolutionChangedHandler);
            _screenModeDropdown.onValueChanged.AddListener(ScreenModeChangedHandler);

            _sfxVolumeDropdown.onValueChanged.AddListener(SFXVolumeChangedHandler);
            _musicVolumeDropdown.onValueChanged.AddListener(MusicVolumeChangedHandler);

            _gameplayManager.IsGameplayInputBlocked = true;

            FillInfo();
        }

        public void Show(object data)
        {
            Show();
        }

        public void Update()
        {
        }


        private void FillInfo()
        {
            _qualityDropdown.ClearOptions();
            _resolutionDropdown.ClearOptions();
            _screenModeDropdown.ClearOptions();

            List<string> data = new List<string>();
            int length = Enum.GetNames(typeof(Enumerators.QualityLevel)).Length;

            for(int i =0; i < length; i++)
            {
                data.Add(InternalTools.ProccesEnumToString(((Enumerators.QualityLevel)i).ToString()));
            }
            _qualityDropdown.AddOptions(data);

            data.Clear();
            length = Enum.GetNames(typeof(Enumerators.ScreenMode)).Length;

            for (int i = 0; i < length; i++)
            {
                data.Add(InternalTools.ProccesEnumToString(((Enumerators.ScreenMode)i).ToString()));
            }
            _screenModeDropdown.AddOptions(data);

            data.Clear();
            length = _applicationSettingsManager.Resolutions.Count;

            for (int i = 0; i < length; i++)
            {
                data.Add(_applicationSettingsManager.Resolutions[i].name);
            }
            _resolutionDropdown.AddOptions(data);

            _qualityDropdown.value = (int)_applicationSettingsManager.CurrentQualityLevel;
            _screenModeDropdown.value = (int)_applicationSettingsManager.CurrentScreenMode;
            _resolutionDropdown.value = _applicationSettingsManager.Resolutions.IndexOf(_applicationSettingsManager.CurrentResolution);

            _sfxVolumeDropdown.value = _soundManager.SFXVolume;
            _musicVolumeDropdown.value = _soundManager.MusicVolume;
        }


        private void CloseButtonHandler()
        {
            _uiManager.HidePopup<SettingsPopup>();
        }

        private void QuitToMenuButtonHandler()
        {
            Action[] actions = new Action[2];
            actions[0] = () =>
            {
                _uiManager.HidePopup<YourTurnPopup>();

                _gameplayManager.EndGame(Enumerators.EndGameType.CANCEL);
                GameClient.Get<IMatchManager>().FinishMatch(Enumerators.AppState.MAIN_MENU);

                _soundManager.StopPlaying(Enumerators.SoundType.TUTORIAL);
                _soundManager.CrossfaidSound(Enumerators.SoundType.BACKGROUND, null, true);
            };
            actions[1] = () => { };

            _uiManager.DrawPopup<ConfirmationPopup>(actions);
            _soundManager.PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
        }

        private void QuitToDesktopButtonHandler()
        {
            _appStateManager.QuitApplication();
        }

        private void SettingsButtonHandler()
        {
            _uiManager.HidePopup<SettingsPopup>();
        }

        private void QualityChangedHandler(int index)
        {
            _applicationSettingsManager.SetQuality((Enumerators.QualityLevel)index);
        }

        private void ResolutionChangedHandler(int index)
        {
            _applicationSettingsManager.SetResolution(_applicationSettingsManager.Resolutions[index].resolution);
        }

        private void ScreenModeChangedHandler(int index)
        {
            _applicationSettingsManager.SetScreenMode((Enumerators.ScreenMode)index);
        }

        private void SFXVolumeChangedHandler(float value)
        {
            _soundManager.SetSoundVolume(value);
        }

        private void MusicVolumeChangedHandler(float value)
        {
            _soundManager.SetMusicVolume(value);
        }
    }
}
