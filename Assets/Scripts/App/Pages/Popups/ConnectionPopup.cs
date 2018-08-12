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
	    private Transform _failedGroup;
	    private Transform _connectingGroup;
	    
		private ConnectionState _state;

	    public void Init()
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _uiManager = GameClient.Get<IUIManager>();
	        _dataManager = GameClient.Get<IDataManager>();

            _selfPage = MonoBehaviour.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Popups/ConnectionPopup"));
            _selfPage.transform.SetParent(_uiManager.Canvas2.transform, false);

	        _failedGroup = _selfPage.transform.Find("Failed_Group");
	        _connectingGroup = _selfPage.transform.Find("Connecting_Group");
			_reconnectButton = _failedGroup.Find("Button_Reconnect").GetComponent<Button>();

			_reconnectButton.onClick.AddListener(PressedReconnectHandler);

            Hide();
        }


		public void Dispose()
		{
		}

	    private async void PressedReconnectHandler () {
		    await ExecuteConnection();
	    }

        public void Hide()
        {
            _selfPage.SetActive(false);

	        ConnectFunc = null;
        }

        public void SetMainPriority()
        {
        }

        public void Show()
        {
			_selfPage.SetActive(true);
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
				    SetUIState(ConnectionState.ConnectionFailed);
			    }
		    }
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
				    _failedGroup.gameObject.SetActive(true);
				    break;
			    default:
				    throw new ArgumentOutOfRangeException();
		    }
	    }

	    private enum ConnectionState
	    {
		    Connecting,
		    ConnectionFailed
	    }
    }
}




