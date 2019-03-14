using System;
using System.Threading.Tasks;
using log4net;
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
        private static readonly ILog Log = Logging.GetLog(nameof(AppStateManager));

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
                    if (Constants.EnableNewUI)
                    {
                         _uiManager.SetPage<LoadingWithAnimationPage>();
                    }   
                    else
                    {
                        _uiManager.SetPage<LoadingPage>();
                    }
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
                    if(Constants.EnableNewUI)
                        _uiManager.SetPage<MainMenuWithNavigationPage>();                    
                    else
                        _uiManager.SetPage<MainMenuPage>();
                    break;
                case Enumerators.AppState.HERO_SELECTION:
                    if (Constants.EnableNewUI)
                    {
                        _uiManager.SetPage<HordeSelectionWithNavigationPage>();
                        HordeSelectionWithNavigationPage hordePage = _uiManager.GetPage<HordeSelectionWithNavigationPage>();
                        hordePage.ChangeTab(HordeSelectionWithNavigationPage.TAB.SELECT_OVERLORD);  
                    }
                    else
                        _uiManager.SetPage<OverlordSelectionPage>();
                    break;
                case Enumerators.AppState.HordeSelection:
                    if (Constants.EnableNewUI)                    
                        _uiManager.SetPage<HordeSelectionWithNavigationPage>();                    
                    else
                        _uiManager.SetPage<HordeSelectionPage>();                        
                        
                    CheckIfPlayAgainOptionShouldBeAvailable();
                    break;                    
                case Enumerators.AppState.ARMY:
                    if (Constants.EnableNewUI)
                        _uiManager.SetPage<ArmyWithNavigationPage>();
                    else
                        _uiManager.SetPage<ArmyPage>();
                    break;
                case Enumerators.AppState.DECK_EDITING:
                    if (Constants.EnableNewUI)                    
                        _uiManager.SetPage<HordeSelectionWithNavigationPage>();
                        //TODO Change tab to deck editing                     
                    else
                        _uiManager.SetPage<HordeEditingPage>();
                    break;
                case Enumerators.AppState.SHOP:                    
                    if (Constants.EnableShopPage)
                    {
                        if (string.IsNullOrEmpty(
                            _backendDataControlMediator.UserDataModel.AccessToken
                        ))
                        {   
                            LoginPopup loginPopup = _uiManager.GetPopup<LoginPopup>();
                            loginPopup.Show();
                            return;
                        }

                        if (Constants.EnableNewUI)
                            _uiManager.SetPage<ShopWithNavigationPage>();
                        else
                            _uiManager.SetPage<ShopPage>();
                    }
                    else
                    {
                        _uiManager.DrawPopup<WarningPopup>($"The Shop is Disabled\nfor version {BuildMetaInfo.Instance.DisplayVersionName}\n\n Thanks for helping us make this game Awesome\n\n-Loom Team");
                    }
                    break;
                case Enumerators.AppState.PACK_OPENER:
                    if (GameClient.Get<ITutorialManager>().IsTutorial || Constants.EnableShopPage)
                    {
                        if (Constants.EnableNewUI)
                            _uiManager.SetPage<PackOpenerPageWithNavigationBar>();
                        else
                            _uiManager.SetPage<PackOpenerPage>();
                        break;
                    }
                    else
                    {
                        _uiManager.DrawPopup<WarningPopup>($"The Pack Opener is Disabled\nfor version {BuildMetaInfo.Instance.DisplayVersionName}\n\n Thanks for helping us make this game Awesome\n\n-Loom Team");
                        return;
                    }
                case Enumerators.AppState.GAMEPLAY:
                    _uiManager.SetPage<GameplayPage>();
                    break;
                case Enumerators.AppState.CREDITS:
                    _uiManager.SetPage<CreditsPage>();
                    break;
                case Enumerators.AppState.PlaySelection:
                    if (Constants.EnableNewUI)                    
                        _uiManager.SetPage<MainMenuWithNavigationPage>();
                        //TODO Change tab to Play Selection                     
                    else
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

        private void CheckIfPlayAgainOptionShouldBeAvailable() 
        {
            if (AppState == Enumerators.AppState.GAMEPLAY && GameClient.Get<IMatchManager>().MatchType == Enumerators.MatchType.PVP)
            {
                _uiManager.DrawPopup<QuestionPopup>("Would you like to play another PvP game?");
                QuestionPopup popup = _uiManager.GetPopup<QuestionPopup>();
                popup.ConfirmationReceived -= DecideToPlayAgain;
                popup.ConfirmationReceived += DecideToPlayAgain;
            }
        }

        private void DecideToPlayAgain(bool decision)
        {
            if (decision)
            {
                HordeSelectionPage page = _uiManager.GetPage<HordeSelectionPage>();
                if (_uiManager.CurrentPage == page) {
                    page.BattleButtonOnClickHandler();
                }
            }

            QuestionPopup popup = _uiManager.GetPopup<QuestionPopup>();
            popup.ConfirmationReceived -= DecideToPlayAgain;
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
            if (_backendFacade?.Contract?.Client != null)
            {
                _backendFacade.Contract.Client.ReadClient.ConnectionStateChanged -= RpcClientOnConnectionStateChanged;
                _backendFacade.Contract.Client.WriteClient.ConnectionStateChanged -= RpcClientOnConnectionStateChanged;
            }
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
            UnitySynchronizationContext.Instance.Post(o =>
            {
                if (state != RpcConnectionState.Connected &&
                    state != RpcConnectionState.Connecting)
                {
                    HandleNetworkExceptionFlow(new RpcClientException($"Changed status of connection to server on: {state}", 1, null), false, true);
                }
            }, null);
        }

        private void UpdateConnectionStatus()
        {
            if (!_backendFacade.IsConnected)
            {
                ConnectionPopup connectionPopup = _uiManager.GetPopup<ConnectionPopup>();


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
                            Helpers.ExceptionReporter.SilentReportException(e);
                        }
                        connectionPopup.Hide();
                    };

                    connectionPopup.ConnectFunc = connectFunc;
                    connectionPopup.Show();
                    connectionPopup.ShowFailedInGame();
                }
            }
        }

        public void HandleNetworkExceptionFlow(Exception exception, bool leaveCurrentAppState = false, bool drawErrorMessage = true)
        {
            if (!ScenePlaybackDetector.IsPlaying || UnitTestDetector.IsRunningUnitTests) {
                throw exception;
            }

            string message = "Handled network exception: ";
            if (exception is RpcClientException rpcClientException && rpcClientException.RpcClient is WebSocketRpcClient webSocketRpcClient)
            {
                message += $"[URL: {webSocketRpcClient.Url}] ";
            }
            message += exception;

            Log.Warn(message);

            if (GameClient.Get<ITutorialManager>().IsTutorial || GameClient.Get<IGameplayManager>().IsTutorial)
            {
                if (!_backendFacade.IsConnected && !GameClient.Get<ITutorialManager>().CurrentTutorial.IsGameplayTutorial())
                {
                    UpdateConnectionStatus();
                }
                return;
            }

            _uiManager.HidePopup<WarningPopup>();
            _uiManager.GetPopup<MatchMakingPopup>().ForceCancelAndHide();
            _uiManager.HidePopup<CardInfoPopup>();
            _uiManager.HidePopup<ConnectionPopup>();
            _uiManager.HidePopup<TutorialAvatarPopup>();

            if (!leaveCurrentAppState)
            {
                if (AppState == Enumerators.AppState.GAMEPLAY)
                {
                    GameClient.Get<IGameplayManager>().EndGame(Enumerators.EndGameType.CANCEL);
                    GameClient.Get<IMatchManager>().FinishMatch(Enumerators.AppState.MAIN_MENU);
                }
                else
                {
                    ChangeAppState(Enumerators.AppState.MAIN_MENU);
                }
            }

            if (drawErrorMessage)
            {
                WarningPopup popup = _uiManager.GetPopup<WarningPopup>();
                popup.ConfirmationReceived += WarningPopupConfirmationReceived;

                _uiManager.DrawPopup<WarningPopup>(exception.Message);
            }
        }

        private void WarningPopupConfirmationReceived()
        {
            WarningPopup popup = _uiManager.GetPopup<WarningPopup>();
            popup.ConfirmationReceived -= WarningPopupConfirmationReceived;

            UpdateConnectionStatus();
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
