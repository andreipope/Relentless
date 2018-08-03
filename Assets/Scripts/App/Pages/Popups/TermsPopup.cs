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
    public class TermsPopup : IUIPopup
    {
        public GameObject Self
        {
            get { return _selfPage; }
        }

        public static Action OnHidePopupEvent;

        private ILoadObjectsManager _loadObjectsManager;
        private IUIManager _uiManager;
		private IDataManager _dataManager;
        private GameObject _selfPage;

		private TextMeshProUGUI _text;
        //private MenuButton _button;
		private MenuButtonNoGlow _gotItButton;
		private GameObject buttonImage;
		private Toggle _toggle;
		//private TextMeshProUGUI _buttonText;

        public void Init()
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _uiManager = GameClient.Get<IUIManager>();
			_dataManager = GameClient.Get<IDataManager> ();

            _selfPage = MonoBehaviour.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Popups/TermsPopup"));
            _selfPage.transform.SetParent(_uiManager.Canvas3.transform, false);

            //_button = _selfPage.transform.Find("Button").GetComponent<MenuButton>();
            _gotItButton = _selfPage.transform.Find("Button_GotIt").GetComponent<MenuButtonNoGlow>();

            //_button.onClickEvent.AddListener(Hide);
            _gotItButton.onClickEvent.AddListener(CloseButtonHandler);

			buttonImage = _selfPage.transform.Find ("ButtonImage").gameObject;

			_toggle = _selfPage.transform.Find ("Toggle").GetComponent<Toggle> ();

			_toggle.onValueChanged.AddListener(ToggleValueChanged);

			_gotItButton.gameObject.SetActive (false);
			buttonImage.SetActive (false);

            _text = _selfPage.transform.Find("Scroll View/Viewport/Content/Text_Message").GetComponent<TextMeshProUGUI>();

            Hide();
        }

		void ToggleValueChanged(bool change)
		{
			if (_toggle.isOn) {
				buttonImage.SetActive (true);
				_gotItButton.gameObject.SetActive (true);
			} else {
				buttonImage.SetActive (false);
				_gotItButton.gameObject.SetActive (false);
			}
		}


		public void Dispose()
		{
		}

        public void CloseButtonHandler()
        {
			_dataManager.CachedUserLocalData.agreedTerms = true;
			_dataManager.SaveAllCache ();
            GameClient.Get<ISoundManager>().PlaySound(Common.Enumerators.SoundType.CLICK, Constants.SFX_SOUND_VOLUME, false, false, true);
            Hide();
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