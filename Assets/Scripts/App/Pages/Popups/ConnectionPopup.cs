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
        public Func<Task> ConnectFuncInGameplay;

        private ILoadObjectsManager _loadObjectsManager;

        private IUIManager _uiManager;

        private Button _reconnectButton;

        private Button _quitButton;

        private Button _closeButton;

        private Button _cancelMatchmakingButton;

        private Transform _failedGroup;

        private Transform _connectingGroup;

        private Transform _matchMakingGroup;

        private GameObject _background;

        private Image _fadeImage;

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
            ConnectFuncInGameplay = null;

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
            Self.transform.SetParent(_uiManager.Canvas3.transform, false);

            _background = Self.transform.Find("Background").gameObject;
            _fadeImage = Self.GetComponent<Image>();
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

        public async Task ExecuteConnection(ConnectionState state = ConnectionState.Connecting)
        {
            Task task = ConnectFunc?.Invoke();
            if (task != null)
            {
                try
                {
                    SetUIState(state);
                    await task;
                }
                catch (Exception e)
                {
                    Helpers.ExceptionReporter.SilentReportException(e);

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

        public async Task ExecuteConnectionFailedInGamePlay()
        {
            if (ConnectFuncInGameplay != null)
            {
                await ConnectFuncInGameplay.Invoke();
            }
        }

        public void ShowFailedInGame()
        {
            SetUIState(ConnectionState.ConnectionFailedInGame);
        }

        public void ShowFailedInGamePlay()
        {
            SetUIState(ConnectionState.ConnectionFailedInGameplay);
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

        private async void PressedCloseHandler()
        {
            if (ConnectFuncInGameplay != null)
            {
                await ExecuteConnectionFailedInGamePlay();
            }
            else
            {
                Hide();
            }
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
            _background.SetActive(true);
            _fadeImage.enabled = true;
            switch (_state)
            {
                case ConnectionState.FirstConnect:
                    _background.SetActive(false);
                    _fadeImage.enabled = false;
                    _connectingGroup.gameObject.SetActive(false);
                    _failedGroup.gameObject.SetActive(false);
                    _matchMakingGroup.gameObject.SetActive(false);
                    break;

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
                case ConnectionState.ConnectionFailedInGameplay:
                    _connectingGroup.gameObject.SetActive(false);
                    _failedGroup.gameObject.SetActive(true);
                    _matchMakingGroup.gameObject.SetActive(false);
                    _closeButton.gameObject.SetActive(true);
                    _reconnectButton.gameObject.SetActive(false);
                    _quitButton.gameObject.SetActive(false);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public enum ConnectionState
        {
            FirstConnect,

            Connecting,

            ConnectionFailed,

            ConnectionFailedInGame,

            LookingForMatch,

            ConnectionFailedInGameplay
        }
    }
}
