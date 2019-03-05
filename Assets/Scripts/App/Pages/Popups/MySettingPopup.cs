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
    public class MySettingPopup : IUIPopup
    {
        private ILoadObjectsManager _loadObjectsManager;
        private IUIManager _uiManager;
        private IGameplayManager _gameplayManager;
        private ISoundManager _soundManager;
        private IAppStateManager _appStateManager;
        private IApplicationSettingsManager _applicationSettingsManager;
        private ITutorialManager _tutorialManager;

        private Button _buttonClose;
                                      
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
            _buttonClose.onClick.AddListener(CloseButtonHandler);
            _buttonClose.onClick.AddListener(PlayClickSound);
            
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
        }


        private void FillInfo()
        {
            _initialInit = true;
            _sfxVolumeSlider.value = _soundManager.SoundVolume;
            _musicVolumeSlider.value = _soundManager.MusicVolume;

            _initialInit = false;
        }


        private void CloseButtonHandler()
        {

            _uiManager.HidePopup<MySettingPopup>();
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
