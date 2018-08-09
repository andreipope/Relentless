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
    public class LoginPopup : IUIPopup
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
        private ButtonShiftingContent _loginButton;
		private TextMeshProUGUI _buttonText;

		private string _state;
		private float _time;
		private string _initialText;

        public void Init()
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _uiManager = GameClient.Get<IUIManager>();
	        _dataManager = GameClient.Get<IDataManager>();

            _selfPage = MonoBehaviour.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Popups/LoginPopup"));
            _selfPage.transform.SetParent(_uiManager.Canvas2.transform, false);

			_loginButton = _selfPage.transform.Find("Button_Login").GetComponent<ButtonShiftingContent>();
			_loginButton.onClick.AddListener(PressedLoginHandler);

			_buttonText = _loginButton.GetComponentInChildren<TextMeshProUGUI> ();

            _text = _selfPage.transform.Find("Text_Message").GetComponent<TextMeshProUGUI>();
			_initialText = _text.text;

            Hide();
        }


		public void Dispose()
		{
		}

		public async void PressedLoginHandler () 
		{
			Debug.Log(" == Pressed Login handler called == " + _state);
			GameClient.Get<ISoundManager>().PlaySound(Common.Enumerators.SoundType.CLICK, Constants.SFX_SOUND_VOLUME, false, false, true);
			if (_state == "login") {
				//Here we will being the login procedure
				await LoomManager.Instance.SetUser();
				_dataManager.StartLoadBackend(SuccessfulLogin);
				
				_buttonText.text = "CANCEL";
				_text.text = "Waiting...";
				_state = "waiting";
				//popup can only be Hide() once login is successful, and we go to the main menu page
			} else if (_state == "waiting") {
				//Interrupt the login process
				_buttonText.text = "LOGIN";
				_text.text = _initialText;
				_state = "login";
			}
		}

		private void SuccessfulLogin () 
		{
			
			GameClient.Get<IAppStateManager>().ChangeAppState(Common.Enumerators.AppState.MAIN_MENU);
			Hide ();
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
			_time = 0;
			_state = "login";
            _selfPage.SetActive(true);
        }

		public void Show(object data)
		{
			_time = 0;
			_state = "login";
			_text.text = (string)data;
			_selfPage.SetActive(true);
		}

        public void Update()
        {
			//this is just for testing purposes of the popup, remove and let the login process handle hiding
			/*if (_state == "waiting") {
				_time += Time.deltaTime;
				if (_time > 3) {
					_state = "done";
					SuccessfulLogin ();
				}
			}*/
        }

    }
}