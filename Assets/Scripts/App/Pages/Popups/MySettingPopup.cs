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
                       _buttonLeaveMatch,
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
            
            _buttonLeaveMatch = Self.transform.Find("Scaler/Button_LeaveMatch").GetComponent<Button>();
            _buttonLeaveMatch.onClick.AddListener(ButtonLeaveMatchHandler);
            _buttonLeaveMatch.onClick.AddListener(PlayClickSound);
            
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
            if (_appStateManager.AppState == Enumerators.AppState.GAMEPLAY)
            {
                _appStateManager.SetPausingApp(true);
            }
        }

        public void Show(object data)
        {
            Show();
        }

        public void Update()
        {
            if (Self == null)
                return;

            _buttonLeaveMatch.gameObject.SetActive(_appStateManager.AppState == Enumerators.AppState.GAMEPLAY);
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
        
        private void ButtonLeaveMatchHandler()
        {
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
            _uiManager.GetPopup<QuestionPopup>().ConfirmationReceived += ConfirmRedirectHelpLink;
            _uiManager.DrawPopup<QuestionPopup>("Do you want to redirect to help link?");            
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
            _uiManager.GetPopup<QuestionPopup>().ConfirmationReceived += ConfirmRedirectSupportLink;
            _uiManager.DrawPopup<QuestionPopup>("Do you want to redirect to support link?");
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
            Hide();
            _uiManager.DrawPopup<CreditPopup>();
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
    }
}
