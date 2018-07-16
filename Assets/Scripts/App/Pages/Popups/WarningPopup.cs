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
        //private MenuButton _button;
        private MenuButtonNoGlow _gotItButton;
		//private TextMeshProUGUI _buttonText;

        public void Init()
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _uiManager = GameClient.Get<IUIManager>();

            _selfPage = MonoBehaviour.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Popups/WarningPopup"));
            _selfPage.transform.SetParent(_uiManager.Canvas2.transform, false);

            //_button = _selfPage.transform.Find("Button").GetComponent<MenuButton>();
            _gotItButton = _selfPage.transform.Find("Button_GotIt").GetComponent<MenuButtonNoGlow>();

            //_button.onClickEvent.AddListener(Hide);
            _gotItButton.onClickEvent.AddListener(CloseButtonHandler);

            _text = _selfPage.transform.Find("Text_Message").GetComponent<TextMeshProUGUI>();

            Hide();
        }


		public void Dispose()
		{
		}

        public void CloseButtonHandler()
        {
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