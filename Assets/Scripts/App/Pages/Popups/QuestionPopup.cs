using GrandDevs.CZB.Common;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GrandDevs.CZB
{
    public class QuestionPopup : IUIPopup
    {
        public event Action ConfirmationEvent;

		public GameObject Self
        {
            get { return _selfPage; }
        }

        private ILoadObjectsManager _loadObjectsManager;
        private IUIManager _uiManager;
        private GameObject _selfPage;

		private TextMeshProUGUI _text;
        //private MenuButton _button1,
        //                    _button2;
        private MenuButtonNoGlow //_closeButton,
                                 _buttonYes,
                                 _buttonNo;
		private TextMeshProUGUI _buttonText;

        public void Init()
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _uiManager = GameClient.Get<IUIManager>();

            _selfPage = MonoBehaviour.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Popups/QuestionPopup"));
            _selfPage.transform.SetParent(_uiManager.Canvas2.transform, false);

            _buttonYes = _selfPage.transform.Find("Button_Yes").GetComponent<MenuButtonNoGlow>();
            _buttonNo = _selfPage.transform.Find("Button_No").GetComponent<MenuButtonNoGlow>();
			//_closeButton = _selfPage.transform.Find("CloseButton").GetComponent<MenuButtonNoGlow>();

			//_closeButton.onClickEvent.AddListener(Hide);
            _buttonYes.onClickEvent.AddListener(ConfirmationButtonHandler);
            _buttonNo.onClickEvent.AddListener(Hide);


			_text = _selfPage.transform.Find("Text_Message").GetComponent<TextMeshProUGUI>();

            Hide();
        }


		public void Dispose()
		{
		}

		public void Hide()
		{
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

        private void ConfirmationButtonHandler()
        {
            GameClient.Get<ISoundManager>().PlaySound(Common.Enumerators.SoundType.CLICK);
            if (ConfirmationEvent != null)
				ConfirmationEvent();
            Hide();
        }

    }
}