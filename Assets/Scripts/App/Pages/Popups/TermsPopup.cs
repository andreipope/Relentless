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

		private TextMeshProUGUI _text,
                                _titleText;
		private ButtonShiftingContent _gotItButton;
		private Toggle _toggle;

        public void Init()
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _uiManager = GameClient.Get<IUIManager>();
			_dataManager = GameClient.Get<IDataManager> ();

            _selfPage = MonoBehaviour.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Popups/TermsPopup"));
            _selfPage.transform.SetParent(_uiManager.Canvas3.transform, false);

            _gotItButton = _selfPage.transform.Find("Button_GotIt").GetComponent<ButtonShiftingContent>();
            _gotItButton.onClick.AddListener(CloseButtonHandler);

			_toggle = _selfPage.transform.Find ("Toggle").GetComponent<Toggle> ();
			_toggle.onValueChanged.AddListener(ToggleValueChanged);

            _text = _selfPage.transform.Find("Message").GetComponent<TextMeshProUGUI>();
            _titleText = _selfPage.transform.Find("Title").GetComponent<TextMeshProUGUI>();

            _gotItButton.gameObject.SetActive(false);

            _titleText.text = "UPDATE ver. " + Constants.CURRENT_VERSION;

            Hide();
        }

		void ToggleValueChanged(bool change)
		{
			if (_toggle.isOn) {
				_gotItButton.gameObject.SetActive (true);
			} else {
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