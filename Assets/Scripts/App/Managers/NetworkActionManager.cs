using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Protobuf;
using Loom.Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using log4net;
using Loom.ZombieBattleground.Common;

namespace Loom.ZombieBattleground
{
    public class NetworkActionManager : IService, INetworkActionManager
    {
        private static readonly ILog Log = Logging.GetLog(nameof(NetworkActionManager));

        private readonly Queue<Func<Task>> _tasks = new Queue<Func<Task>>();
        private BackendFacade _backendFacade;
        private IAppStateManager _appStateManager;
        private bool _isUpdating;

        public bool Active { get; set; }

        public int QueuedTaskCount => _tasks?.Count ?? 0;

        public void Init()
        {
            _backendFacade = GameClient.Get<BackendFacade>();
            _appStateManager = GameClient.Get<IAppStateManager>();
        }

        public void Clear()
        {
            _tasks.Clear();
        }

        public async void Update()
        {
            if (!Active)
                return;

            if (_isUpdating)
                return;

            _isUpdating = true;
            while (_tasks.Count > 0)
            {
                await _tasks.Peek().Invoke();
                _tasks.Dequeue();
            }

            _isUpdating = false;
        }

        public void EnqueueMessage(IMessage request)
        {
            EnqueueNetworkTask(async () =>
            {
                switch (request)
                {
                    case PlayerActionRequest playerActionMessage:
                        await _backendFacade.SendPlayerAction(playerActionMessage);
                        break;
                    case EndMatchRequest endMatchMessage:
                        await _backendFacade.SendEndMatchRequest(endMatchMessage);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException($"Unknown action type: {request.GetType()}");
                }
            });
        }

        public void EnqueueNetworkTask(
            Func<Task> taskFunc,
            Func<Exception, Task> onUnknownExceptionCallbackFunc = null,
            Func<Exception, Task> onNetworkExceptionCallbackFunc = null,
            Func<Task> onCompletedCallbackFunc = null,
            bool leaveCurrentAppState = false,
            bool drawErrorMessage = true
            )
        {
            if (taskFunc == null)
                throw new ArgumentNullException(nameof(taskFunc));

            _tasks.Enqueue(async () =>
            {
                if (!_backendFacade.IsConnected)
                {
                    Log.Warn("Tried to execute network action when Connection state is Disconnected.");
                    return;
                }

                await ExecuteNetworkAction(taskFunc, onUnknownExceptionCallbackFunc, onNetworkExceptionCallbackFunc, onCompletedCallbackFunc, leaveCurrentAppState, drawErrorMessage);
            });
        }


        public void Dispose()
        {
            Clear();
        }

        private async Task ExecuteNetworkAction(
            Func<Task> taskFunc,
            Func<Exception, Task> onUnknownExceptionCallbackFunc = null,
            Func<Exception, Task> onNetworkExceptionCallbackFunc = null,
            Func<Task> onCompletedCallbackFunc = null,
            bool leaveCurrentAppState = false,
            bool drawErrorMessage = true)
        {
            try
            {
                await taskFunc();
            }
            catch (TimeoutException exception)
            {
                Helpers.ExceptionReporter.SilentReportException(exception);
                Log.Warn(" Time out == " + exception);
                _appStateManager.HandleNetworkExceptionFlow(exception, leaveCurrentAppState, drawErrorMessage);
                if (onNetworkExceptionCallbackFunc != null)
                {
                    await onNetworkExceptionCallbackFunc(exception);
                }
            }
            catch (Client.RpcClientException exception)
            {
                Helpers.ExceptionReporter.SilentReportException(exception);
                Log.Warn(" RpcException == " + exception);
                _appStateManager.HandleNetworkExceptionFlow(exception, leaveCurrentAppState, drawErrorMessage);
                if (onNetworkExceptionCallbackFunc != null)
                {
                    await onNetworkExceptionCallbackFunc(exception);
                }
            }
            catch (Exception exception)
            {
                Helpers.ExceptionReporter.SilentReportException(exception);
                Log.Warn(" other == " + exception);

                if (onUnknownExceptionCallbackFunc != null)
                {
                    await onUnknownExceptionCallbackFunc(exception);
                }
                else
                {
                    ShowConnectionPopup();
                }
            }

            if (onCompletedCallbackFunc != null)
            {
                await onCompletedCallbackFunc();
            }
        }

        private void ShowConnectionPopup()
        {
            IUIManager uiManager = GameClient.Get<IUIManager>();
            IGameplayManager gameplayManager = GameClient.Get<IGameplayManager>();
            ConnectionPopup connectionPopup = uiManager.GetPopup<ConnectionPopup>();

            if (gameplayManager.CurrentPlayer == null)
                return;

            if (connectionPopup.Self == null)
            {
                Func<Task> connectFuncInGame = () =>
                {
                    Clear();
                    gameplayManager.CurrentPlayer.ThrowLeaveMatch();
                    gameplayManager.EndGame(Enumerators.EndGameType.CANCEL);
                    GameClient.Get<IMatchManager>().FinishMatch(Enumerators.AppState.MAIN_MENU);
                    connectionPopup.Hide();
                    return Task.CompletedTask;
                };

                connectionPopup.ConnectFuncInGameplay = connectFuncInGame;
                connectionPopup.Show();
                connectionPopup.ShowFailedInGamePlay();
            }
        }
    }
}
