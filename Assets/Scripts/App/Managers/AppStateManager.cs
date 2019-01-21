using System;
using System.Threading.Tasks;
using Loom.Client;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Common;
using Unity.Cloud.UserReporting;
using Unity.Cloud.UserReporting.Plugin;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public sealed class AppStateManager : IService, IAppStateManager
    {
        private const float BackButtonResetDelay = 0.5f;

        private IUIManager _uiManager;

        private float _backButtonTimer;

        private int _backButtonClicksCount;

        private bool _isBackButtonCounting;

        private Enumerators.AppState _previousState;

        private BackendFacade _backendFacade;

        private BackendDataControlMediator _backendDataControlMediator;

        public bool IsAppPaused { get; private set; }

        public Enumerators.AppState AppState { get; set; }

        public void ChangeAppState(Enumerators.AppState stateTo, bool force = false)
        {
            if (!force)
            {
                if (AppState == stateTo)
                    return;
            }

            switch (stateTo)
            {
                case Enumerators.AppState.APP_INIT:
                    GameClient.Get<ITimerManager>().Dispose();
                    _uiManager.SetPage<LoadingPage>();
                    GameClient.Get<ISoundManager>().PlaySound(
                        Enumerators.SoundType.BACKGROUND,
                        128,
                        Constants.BackgroundSoundVolume,
                        null,
                        true);

                    break;
                case Enumerators.AppState.LOGIN:
                    break;
                case Enumerators.AppState.MAIN_MENU:
                    _uiManager.SetPage<MainMenuPage>();
                    if (AppState == Enumerators.AppState.GAMEPLAY && GameClient.Get<IMatchManager>().MatchType == Enumerators.MatchType.PVP)
                    {
                        _uiManager.DrawPopup<QuestionPopup>("Would you like to play another PvP game?");
                        QuestionPopup popup = _uiManager.GetPopup<QuestionPopup>();
                        popup.ConfirmationReceived += DecideToPlayAgain;
                    }
                    break;
                case Enumerators.AppState.HERO_SELECTION:
                    _uiManager.SetPage<OverlordSelectionPage>();
                    break;
                case Enumerators.AppState.HordeSelection:
                    _uiManager.SetPage<HordeSelectionPage>();
                    break;
                case Enumerators.AppState.ARMY:
                    _uiManager.SetPage<ArmyPage>();
                    break;
                case Enumerators.AppState.DECK_EDITING:
                    _uiManager.SetPage<HordeEditingPage>();
                    break;
                case Enumerators.AppState.SHOP:
                    _uiManager.SetPage<ShopPage>();
                    break;
                case Enumerators.AppState.PACK_OPENER:
                    _uiManager.SetPage<PackOpenerPage>();
                    break;
                case Enumerators.AppState.GAMEPLAY:
                    _uiManager.SetPage<GameplayPage>();
                    break;
                case Enumerators.AppState.CREDITS:
                    _uiManager.SetPage<CreditsPage>();
                    break;
                case Enumerators.AppState.PlaySelection:
                    _uiManager.SetPage<PlaySelectionPage>();
                    break;
                case Enumerators.AppState.PvPSelection:
                    _uiManager.SetPage<PvPSelectionPage>();
                    break;
                case Enumerators.AppState.CustomGameModeList:
                    _uiManager.SetPage<CustomGameModeListPage>();
                    break;
                case Enumerators.AppState.CustomGameModeCustomUi:
                    _uiManager.SetPage<CustomGameModeCustomUiPage>();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(stateTo), stateTo, null);
            }

            _previousState = AppState != Enumerators.AppState.SHOP ? AppState : Enumerators.AppState.MAIN_MENU;

            AppState = stateTo;

            UnityUserReporting.CurrentClient.LogEvent(UserReportEventLevel.Info, "App state: " + AppState);
        }

        private void DecideToPlayAgain(bool decision)
        {
            if (decision) 
            {
                QuestionPopup popup = _uiManager.GetPopup<QuestionPopup>();
                popup.ConfirmationReceived -= DecideToPlayAgain;
                GameClient.Get<IMatchManager>().FindMatch();
            }
        }

        public void SetPausingApp(bool mustPause) {
            if (!mustPause) 
            {
                IsAppPaused = false;
                AudioListener.pause = false;
            } 
            else 
            {
                IsAppPaused = true;
                AudioListener.pause = true;
            }
        }

        public void BackAppState()
        {
            ChangeAppState(_previousState);
        }

        public void Dispose()
        {
        }

        public void Init()
        {
            _uiManager = GameClient.Get<IUIManager>();

            _backendFacade = GameClient.Get<BackendFacade>();
            _backendDataControlMediator = GameClient.Get<BackendDataControlMediator>();

            _backendFacade.ContractCreated += LoomManagerOnContractCreated;
        }

        public void Update()
        {
            CheckBackButton();
        }

        public void QuitApplication()
        {
            GameClient.Get<ITimerManager>().Dispose();
            Application.Quit();
        }
        
        private void RpcClientOnConnectionStateChanged(IRpcClient sender, RpcConnectionState state)
        {
            UnitySynchronizationContext.Instance.Post(o => UpdateConnectionStatus(), null);
        }

        private void UpdateConnectionStatus()
        {
            ConnectionPopup connectionPopup = _uiManager.GetPopup<ConnectionPopup>();
            if (!_backendFacade.IsConnected)
            {
                if (connectionPopup.Self == null)
                {
                    Func<Task> connectFunc = async () =>
                    {
                        try
                        {
                            await _backendDataControlMediator.LoginAndLoadData();
                        }
                        catch(Exception e)
                        {
                            Helpers.ExceptionReporter.LogException(e);
                        }
                        connectionPopup.Hide();
                    };

                    connectionPopup.ConnectFunc = connectFunc;
                    connectionPopup.Show();
                    connectionPopup.ShowFailedInGame();
                }
            }
        }

        private void LoomManagerOnContractCreated(Contract oldContract, Contract newContract)
        {
            if (oldContract != null)
            {
                oldContract.Client.ReadClient.ConnectionStateChanged -= RpcClientOnConnectionStateChanged;
                oldContract.Client.WriteClient.ConnectionStateChanged -= RpcClientOnConnectionStateChanged;
            }

            newContract.Client.ReadClient.ConnectionStateChanged += RpcClientOnConnectionStateChanged;
            newContract.Client.WriteClient.ConnectionStateChanged += RpcClientOnConnectionStateChanged;

            UpdateConnectionStatus();
        }

        private void CheckBackButton()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                _isBackButtonCounting = true;
                _backButtonClicksCount++;
                _backButtonTimer = 0f;

                if (_backButtonClicksCount >= 2)
                {
                    if (_uiManager.GetPopup<ConfirmationPopup>().Self == null)
                    {
                        Action[] actions = new Action[2];
                        actions[0] = () =>
                        {
                            Application.Quit();
                        };
                        actions[1] = () => { };

                        _uiManager.DrawPopup<ConfirmationPopup>(actions);
                    }
                }
            }

            if (_isBackButtonCounting)
            {
                _backButtonTimer += Time.deltaTime;

                if (_backButtonTimer >= BackButtonResetDelay)
                {
                    _backButtonTimer = 0f;
                    _backButtonClicksCount = 0;
                    _isBackButtonCounting = false;
                }
            }
        }
    }
}
