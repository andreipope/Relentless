using System;
using System.Collections.Generic;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Common;
using TMPro;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Loom.ZombieBattleground
{
    public class AreaBarPopup : IUIPopup
    {
        public GameObject Self { get; private set; }
        
        private ILoadObjectsManager _loadObjectsManager;

        private IUIManager _uiManager;
        
        private IAppStateManager _stateManager;
        
        private ISoundManager _soundManager;
        
        private BackendDataControlMediator _backendDataControlMediator;
        
        private Button _buttonLogin, 
                       _buttonSettings;

        private TextMeshProUGUI _textPlayerName;

        #region IUIPopup

        public void Init()
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _uiManager = GameClient.Get<IUIManager>();
            _stateManager = GameClient.Get<IAppStateManager>();
            _soundManager = GameClient.Get<ISoundManager>();
            _backendDataControlMediator = GameClient.Get<BackendDataControlMediator>();
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
        }

        public void SetMainPriority()
        {
        }

        public void Show()
        {
            if (Self != null)
                return;

            Self = Object.Instantiate(
                _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Popups/AreaBarPopup"));
            Self.transform.SetParent(_uiManager.Canvas2.transform, false);
            
            _buttonLogin = Self.transform.Find("Scaler/Button_Login").GetComponent<Button>();
            _buttonLogin.onClick.AddListener(ButtonLoginHandler);
            
            _buttonSettings = Self.transform.Find("Scaler/Button_Setting").GetComponent<Button>();
            _buttonSettings.onClick.AddListener(ButtonSettingHandler);
            
            _textPlayerName = Self.transform.Find("Scaler/Text_PlayerName").GetComponent<TextMeshProUGUI>();
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

        #endregion
        
        #region Buttons Handlers

        private void ButtonSettingHandler()
        {
            if (GameClient.Get<ITutorialManager>().IsButtonBlockedInTutorial(_buttonSettings.name))
            {
                GameClient.Get<ITutorialManager>().ReportActivityAction(Enumerators.TutorialActivityAction.IncorrectButtonTapped);
                return;
            }
            _uiManager.DrawPopup<MySettingPopup>(true);
        }
        
        private void ButtonLoginHandler()
        {
            if (GameClient.Get<ITutorialManager>().IsButtonBlockedInTutorial(_buttonLogin.name))
            {
                GameClient.Get<ITutorialManager>().ReportActivityAction(Enumerators.TutorialActivityAction.IncorrectButtonTapped);
                return;
            }
            _soundManager.PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);

            LoginPopup popup = _uiManager.GetPopup<LoginPopup>();
            popup.Show();
        }

        #endregion
    }
}
