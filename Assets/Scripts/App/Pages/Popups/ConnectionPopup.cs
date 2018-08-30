// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



using LoomNetwork.CZB.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Threading.Tasks;
using Loom.Client;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace LoomNetwork.CZB
{
    public class ConnectionPopup : IUIPopup
    {
        public GameObject Self
        {
            get { return _selfPage; }
        }

        public Func<Task> ConnectFunc;

        private ILoadObjectsManager _loadObjectsManager;
        private IUIManager _uiManager;
        private GameObject _selfPage;
	    private IDataManager _dataManager;

		private Button _reconnectButton;
		private Button _closeButton;
	    private Transform _failedGroup;
	    private Transform _connectingGroup;
	    
		private ConnectionState _state;

	    public void Init()
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _uiManager = GameClient.Get<IUIManager>();
	        _dataManager = GameClient.Get<IDataManager>();
        }


		public void Dispose()
		{
		}

	    private async void PressedReconnectHandler () {
		    await ExecuteConnection();
	    }

		private async void PressedCloseHandler () {
			Hide ();
		}

        public void Hide()
        {
	        ConnectFunc = null;

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
            _selfPage = MonoBehaviour.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Popups/ConnectionPopup"));
            _selfPage.transform.SetParent(_uiManager.Canvas2.transform, false);

            _failedGroup = _selfPage.transform.Find("Failed_Group");
            _connectingGroup = _selfPage.transform.Find("Connecting_Group");
            _reconnectButton = _failedGroup.Find("Button_Reconnect").GetComponent<Button>();
			_closeButton = _failedGroup.Find("Button_Close").GetComponent<Button>();

            _reconnectButton.onClick.AddListener(PressedReconnectHandler);
			_closeButton.onClick.AddListener (PressedCloseHandler);

            _state = ConnectionState.Connecting;
            SetUIState(ConnectionState.Connecting);
        }

		public void Show(object data)
		{
			Show();
		}

	    public void Update() {
	    }

	    public async Task ExecuteConnection() {
		    Task task = ConnectFunc?.Invoke();
		    if (task != null)
		    {
			    try
			    {
				    SetUIState(ConnectionState.Connecting);
				    await task;
			    } catch (Exception e)
			    {
					if (GameClient.Get<IAppStateManager> ().AppState == Common.Enumerators.AppState.MAIN_MENU) {
						SetUIState (ConnectionState.ConnectionFailedOnMenu);
					} else {
						SetUIState (ConnectionState.ConnectionFailed);
					}
			    }
		    }
	    }

		public void ShowFailedOnMenu () {
			SetUIState(ConnectionState.ConnectionFailedOnMenu);
		}
			
	    private void SetUIState(ConnectionState state) {
		    _state = state;
		    _connectingGroup.gameObject.SetActive(false);
		    _failedGroup.gameObject.SetActive(false);
		    switch (_state)
		    {
			    case ConnectionState.Connecting:
				    _connectingGroup.gameObject.SetActive(true);
				    break;
				case ConnectionState.ConnectionFailed:
					_failedGroup.gameObject.SetActive (true);
					_closeButton.gameObject.SetActive (false);
					_reconnectButton.gameObject.SetActive (true);
				    break;
				case ConnectionState.ConnectionFailedOnMenu:
					_failedGroup.gameObject.SetActive (true);
					_closeButton.gameObject.SetActive (true);
					_reconnectButton.gameObject.SetActive (false);
					break;
			    default:
				    throw new ArgumentOutOfRangeException();
		    }
	    }

	    private enum ConnectionState
	    {
		    Connecting,
		    ConnectionFailed,
			ConnectionFailedOnMenu
	    }
    }
}




