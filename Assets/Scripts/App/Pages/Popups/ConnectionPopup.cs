using System;
using System.Threading.Tasks;
using Loom.ZombieBattleground.Common;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Loom.ZombieBattleground
{
    public class ConnectionPopup : IUIPopup
    {
        public event Action CancelMatchmakingClicked;

        public Func<Task> ConnectFunc;

        private ILoadObjectsManager _loadObjectsManager;

        private IUIManager _uiManager;

        private Button _reconnectButton;

        private Button _quitButton;

        private Button _closeButton;

        private Button _cancelMatchmakingButton;

        private Transform _failedGroup;

        private Transform _connectingGroup;

        private Transform _matchMakingGroup;

        private ConnectionState _state;

        public GameObject Self { get; private set; }

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
            _matchMakingGroup = Self.transform.Find("Matchmaking_Group");
            _reconnectButton = _failedGroup.Find("Button_Reconnect").GetComponent<Button>();
            _quitButton = _failedGroup.Find("Button_Quit").GetComponent<Button>();
            _closeButton = _failedGroup.Find("Button_Close").GetComponent<Button>();
            _cancelMatchmakingButton = _matchMakingGroup.Find("Button_Cancel").GetComponent<Button>();

            _reconnectButton.onClick.AddListener(PressedReconnectHandler);
            _quitButton.onClick.AddListener(PressedQuitHandler);
            _closeButton.onClick.AddListener(PressedCloseHandler);
            _cancelMatchmakingButton.onClick.AddListener(PressedCancelMatchmakingHandler);

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
                }
                catch (Exception)
                {
                    if (GameClient.Get<IAppStateManager>().AppState != Enumerators.AppState.APP_INIT)
                    {
                        SetUIState(ConnectionState.ConnectionFailedInGame);
                    }
                    else
                    {
                        SetUIState(ConnectionState.ConnectionFailed);
                    }
                }
            }
        }

        public void ShowFailedInGame()
        {
            SetUIState(ConnectionState.ConnectionFailedInGame);
        }

        public void ShowLookingForMatch()
        {
            SetUIState(ConnectionState.LookingForMatch);
        }

        private void PressedQuitHandler()
        {
            Application.Quit();
        }

        private async void PressedReconnectHandler()
        {
            await ExecuteConnection();
        }

        private void PressedCloseHandler()
        {
            Hide();
        }

        private void PressedCancelMatchmakingHandler()
        {
            CancelMatchmakingClicked?.Invoke();
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
                    _failedGroup.gameObject.SetActive(false);
                    _matchMakingGroup.gameObject.SetActive(false);
                    break;
                case ConnectionState.ConnectionFailed:
                    _connectingGroup.gameObject.SetActive(false);
                    _failedGroup.gameObject.SetActive(true);
                    _matchMakingGroup.gameObject.SetActive(false);
                    _closeButton.gameObject.SetActive(true);
                    _reconnectButton.gameObject.SetActive(false);
                    _quitButton.gameObject.SetActive(false);
                    break;
                case ConnectionState.ConnectionFailedInGame:
                    _connectingGroup.gameObject.SetActive(false);
                    _failedGroup.gameObject.SetActive(true);
                    _matchMakingGroup.gameObject.SetActive(false);
                    _closeButton.gameObject.SetActive(false);
                    _reconnectButton.gameObject.SetActive(true);
                    _quitButton.gameObject.SetActive(true);
                    break;
                case ConnectionState.LookingForMatch:
                    _connectingGroup.gameObject.SetActive(false);
                    _failedGroup.gameObject.SetActive(false);
                    _matchMakingGroup.gameObject.SetActive(true);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private enum ConnectionState
        {
            Connecting,

            ConnectionFailed,

            ConnectionFailedInGame,

            LookingForMatch
        }
    }
}
