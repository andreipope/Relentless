// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/

using System;
using System.Threading.Tasks;
using LoomNetwork.CZB.Common;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace LoomNetwork.CZB
{
    public class ConnectionPopup : IUIPopup
    {
        public Func<Task> ConnectFunc;

        private ILoadObjectsManager _loadObjectsManager;

        private IUIManager _uiManager;

        private IDataManager _dataManager;

        private Button _reconnectButton;

        private Button _closeButton;

        private Transform _failedGroup;

        private Transform _connectingGroup;

        private ConnectionState _state;

        public GameObject Self { get; private set; }

        public void Init()
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _uiManager = GameClient.Get<IUIManager>();
            _dataManager = GameClient.Get<IDataManager>();
        }

        public void Dispose()
        {
        }

        public void Hide()
        {
            ConnectFunc = null;

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
            Self = Object.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Popups/ConnectionPopup"));
            Self.transform.SetParent(_uiManager.Canvas2.transform, false);

            _failedGroup = Self.transform.Find("Failed_Group");
            _connectingGroup = Self.transform.Find("Connecting_Group");
            _reconnectButton = _failedGroup.Find("Button_Reconnect").GetComponent<Button>();
            _closeButton = _failedGroup.Find("Button_Close").GetComponent<Button>();

            _reconnectButton.onClick.AddListener(PressedReconnectHandler);
            _closeButton.onClick.AddListener(PressedCloseHandler);

            _state = ConnectionState.Connecting;
            SetUIState(ConnectionState.Connecting);
        }

        public void Show(object data)
        {
            Show();
        }

        public void Update()
        {
        }

        public async Task ExecuteConnection()
        {
            Task task = ConnectFunc?.Invoke();
            if (task != null)
            {
                try
                {
                    SetUIState(ConnectionState.Connecting);
                    await task;
                } catch (Exception e)
                {
                    if (GameClient.Get<IAppStateManager>().AppState == Enumerators.AppState.MAIN_MENU)
                    {
                        SetUIState(ConnectionState.ConnectionFailedOnMenu);
                    } else
                    {
                        SetUIState(ConnectionState.ConnectionFailed);
                    }
                }
            }
        }

        public void ShowFailedOnMenu()
        {
            SetUIState(ConnectionState.ConnectionFailedOnMenu);
        }

        private async void PressedReconnectHandler()
        {
            await ExecuteConnection();
        }

        private async void PressedCloseHandler()
        {
            Hide();
        }

        private void SetUIState(ConnectionState state)
        {
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
                    _closeButton.gameObject.SetActive(false);
                    _reconnectButton.gameObject.SetActive(true);
                    break;
                case ConnectionState.ConnectionFailedOnMenu:
                    _failedGroup.gameObject.SetActive(true);
                    _closeButton.gameObject.SetActive(true);
                    _reconnectButton.gameObject.SetActive(false);
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
