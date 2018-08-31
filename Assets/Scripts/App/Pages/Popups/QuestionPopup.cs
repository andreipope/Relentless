// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



using LoomNetwork.CZB.Common;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace LoomNetwork.CZB
{
    public class QuestionPopup : IUIPopup
    {
        public event Action<bool> ConfirmationEvent;

		public GameObject Self
        {
            get { return _selfPage; }
        }

        private ILoadObjectsManager _loadObjectsManager;
        private IUIManager _uiManager;
        private GameObject _selfPage;

        private Button _backButton;

		private TextMeshProUGUI _text;
        //private MenuButton _button1,
        //                    _button2;
        private ButtonShiftingContent //_closeButton,
                                 _buttonYes,
                                 _buttonNo;
		private TextMeshProUGUI _buttonText;

        public void Init()
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _uiManager = GameClient.Get<IUIManager>();
        }


		public void Dispose()
		{
		}

		public void Hide()
		{
            if (_selfPage == null)
                return;

            _selfPage.SetActive (false);
            GameObject.Destroy (_selfPage);
            _selfPage = null;
		}

        public void SetMainPriority()
        {
        }

        public void Show()
        {
            _selfPage = MonoBehaviour.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Popups/QuestionPopup"));
            _selfPage.transform.SetParent(_uiManager.Canvas2.transform, false);

            _buttonYes = _selfPage.transform.Find("Button_Yes").GetComponent<ButtonShiftingContent>();
            _buttonNo = _selfPage.transform.Find("Button_No").GetComponent<ButtonShiftingContent>();
            _backButton = _selfPage.transform.Find("Button_Back").GetComponent<Button>();
            //_closeButton = _selfPage.transform.Find("CloseButton").GetComponent<MenuButtonNoGlow>();

            //_closeButton.onClickEvent.AddListener(Hide);
            _buttonYes.onClick.AddListener(ConfirmationButtonHandler);
            _buttonNo.onClick.AddListener(NoButtonOnClickHandler);
            _backButton.onClick.AddListener(BackButtonHandler);

            _text = _selfPage.transform.Find("Text_Message").GetComponent<TextMeshProUGUI>();
        }

        public void Show(object data)
        {
            Show();

            if (data is object[])
            {
                object[] param = (object[])data;
                _text.text = (string)param[0];
                _backButton.gameObject.SetActive((bool)param[1]);
            }
            else
            {
                _backButton.gameObject.SetActive(false);
                _text.text = (string)data;
            }
        }

        public void Update()
        {

        }

        private void ConfirmationButtonHandler()
        {
            GameClient.Get<ISoundManager>().PlaySound(Common.Enumerators.SoundType.CLICK, Constants.SFX_SOUND_VOLUME, false, false, true);

            ConfirmationEvent?.Invoke(true);

            Hide();
        }

        private void NoButtonOnClickHandler()
        {
            ConfirmationEvent?.Invoke(false);
            _uiManager.HidePopup<QuestionPopup>();
        }

        private void BackButtonHandler()
        {
            ConfirmationEvent = null;
            Hide();
        }
    }
}