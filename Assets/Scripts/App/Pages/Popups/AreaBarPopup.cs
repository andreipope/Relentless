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

        private GameObject _groupLogin,
                           _groupPlayerInfo;

        #region IUIPopup

        public void Init()
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _uiManager = GameClient.Get<IUIManager>();
            _stateManager = GameClient.Get<IAppStateManager>();
            _soundManager = GameClient.Get<ISoundManager>();
            _backendDataControlMediator = GameClient.Get<BackendDataControlMediator>();
            
            _uiManager.GetPopup<SettingsWithCreditsPopup>().OnLoginButtonDisplayUpdate += UpdateLoginButtonDisplay;
            LoginPopup.OnShowPopupEvent += () => UpdateLoginButtonDisplay(true);
            LoginPopup.OnHidePopupEvent += () => UpdateLoginButtonDisplay(false);
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
            Self.transform.SetParent(_uiManager.Canvas.transform, false);

            _groupLogin = Self.transform.Find("Group_Login").gameObject;
            _groupPlayerInfo = Self.transform.Find("Group_PlayerInfo").gameObject;
            
            _buttonLogin = _groupLogin.transform.Find("Button_Login").GetComponent<Button>();
            _buttonLogin.onClick.AddListener(ButtonLoginHandler);
            
            _buttonSettings = Self.transform.Find("Button_Setting").GetComponent<Button>();
            _buttonSettings.onClick.AddListener(ButtonSettingHandler);
            
            _textPlayerName = _groupPlayerInfo.transform.Find("Text_PlayerName").GetComponent<TextMeshProUGUI>();
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
                    if (!_groupLogin.activeSelf)
                    {
                        _groupLogin.SetActive(true);
                        _groupPlayerInfo.SetActive(false);
                        _textPlayerName.text = "Guest Player";
                    }
                }
                else
                {
                    if (_groupLogin.activeSelf)
                    {
                        _groupLogin.SetActive(false);
                        _groupPlayerInfo.SetActive(true);
                        _textPlayerName.text = _backendDataControlMediator.UserDataModel.UserId;
                    }
                }
            }
        }

        #endregion
        
        #region Buttons Handlers

        private void ButtonSettingHandler()
        {
            if (GameClient.Get<ITutorialManager>().BlockAndReport(_buttonSettings.name))
                return;

            _uiManager.DrawPopup<SettingsWithCreditsPopup>(true);
        }
        
        private void ButtonLoginHandler()
        {
            if (GameClient.Get<ITutorialManager>().BlockAndReport(_buttonLogin.name))
                return;

            _soundManager.PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);

            LoginPopup popup = _uiManager.GetPopup<LoginPopup>();
            popup.Show();
        }

        #endregion
        
        private void UpdateLoginButtonDisplay(bool status)
        {
            if (Self != null)
            {
                _buttonLogin.gameObject.SetActive(!status);
            }
        }
    }
}
