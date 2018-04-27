using GrandDevs.CZB.Common;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GrandDevs.CZB
{
    public class WarningPopup : IUIPopup
    {
        public GameObject Self
        {
            get { return _selfPage; }
        }

        public static Action OnHidePopupEvent;

        private ILoadObjectsManager _loadObjectsManager;
        private IUIManager _uiManager;
        private GameObject _selfPage;

		private TextMeshProUGUI _text;
        private MenuButton _button;
        private MenuButtonNoGlow _closeButton;
		private TextMeshProUGUI _buttonText;

        public void Init()
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _uiManager = GameClient.Get<IUIManager>();

            _selfPage = MonoBehaviour.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Popups/WarningPopup"));
            _selfPage.transform.SetParent(_uiManager.Canvas.transform, false);

			_button = _selfPage.transform.Find("Button").GetComponent<MenuButton>();
			_closeButton = _selfPage.transform.Find("CloseButton").GetComponent<MenuButtonNoGlow>();

			_button.onClickEvent.AddListener(Hide);
			_closeButton.onClickEvent.AddListener(Hide);

			_text = _selfPage.transform.Find("Text").GetComponent<TextMeshProUGUI>();

            Hide();
        }


		public void Dispose()
		{
		}

		public void Hide()
        {
            OnHidePopupEvent?.Invoke();
            _selfPage.SetActive(false);
		}

        public void SetMainPriority()
        {
        }

        public void Show()
        {
            _selfPage.SetActive(true);
        }

        public void Show(object data)
        {
            _text.text = (string)data;

            Show();
        }

        public void Update()
        {

        }

    }
}