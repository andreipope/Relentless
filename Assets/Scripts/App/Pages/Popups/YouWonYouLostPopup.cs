using System;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using UnityEngine.Experimental.PlayerLoop;

namespace Loom.ZombieBattleground
{
    public class YouWonYouLostPopup : IUIPopup
    {
        public GameObject Self { get; private set; }
        
        private IUIManager _uiManager;

        private ILoadObjectsManager _loadObjectsManager;

        private IAppStateManager _stateManager;

        private ISoundManager _soundManager;

        private Button _buttonRematch,
                       _buttonContinue;
        
        #region IUIPopup
        
        public void Init()
        {
            _uiManager = GameClient.Get<IUIManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _stateManager = GameClient.Get<IAppStateManager>();
            _soundManager = GameClient.Get<ISoundManager>();
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
                _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Popups/YouWonYouLostPopup"));
            Self.transform.SetParent(_uiManager.Canvas2.transform, false);
            
            _buttonRematch = Self.transform.Find("Scaler/Button_Rematch").GetComponent<Button>();                        
            _buttonRematch.onClick.AddListener(ButtonRematchHandler);
            _buttonContinue = Self.transform.Find("Scaler/Button_Continue").GetComponent<Button>();
            _buttonContinue.onClick.AddListener(ButtonContinueHandler);
        }
        
        public void Show(object data)
        {
            Show();
        }

        public void Update()
        {
        }

        #endregion

        #region Buttons Handlers

        private void ButtonRematchHandler()
        {
        }
        
        private void ButtonContinueHandler()
        {

        }

        #endregion
    }
}