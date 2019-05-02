using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Protobuf;
using Loom.Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using log4net;
using Loom.ZombieBattleground.Common;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class NetworkActionManager : IService, INetworkActionManager
    {
        private static readonly ILog Log = Logging.GetLog(nameof(NetworkActionManager));

        private readonly Queue<(Func<Task> funcTask, TaskCompletionSource<bool> completedTask)> _tasks = new Queue<(Func<Task> funcTask, TaskCompletionSource<bool> completedTask)>();
        private BackendFacade _backendFacade;
        private IAppStateManager _appStateManager;
        private bool _isExecutingTask;

        public int QueuedTaskCount => (_tasks?.Count ?? 0) + (_isExecutingTask ? 1 : 0);

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
            while (_tasks.Count > 0)
            {
                _isExecutingTask = true;

                (Func<Task> funcTask, TaskCompletionSource<bool> completedTask) = _tasks.Dequeue();
                try
                {
                    await funcTask();
                    completedTask.SetResult(true);
                }
                catch (Exception e)
                {
                    completedTask.SetException(e);
                }

                _isExecutingTask = false;
            }
        }

        public Task EnqueueMessage(IMessage request)
        {
            return EnqueueNetworkTask(async () =>
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

        public Task EnqueueNetworkTask(
            Func<Task> taskFunc,
            Func<Exception, Task> onUnknownExceptionCallbackFunc = null,
            Func<Exception, Task> onNetworkExceptionCallbackFunc = null,
            bool keepCurrentAppState = false,
            bool drawErrorMessage = true,
            bool ignoreConnectionState = false
            )
        {
            Func<Task> wrappedTaskFunc = WrapNetworkTask(
                taskFunc,
                onUnknownExceptionCallbackFunc,
                onNetworkExceptionCallbackFunc,
                keepCurrentAppState,
                drawErrorMessage,
                ignoreConnectionState);

            TaskCompletionSource<bool> completedTask = new TaskCompletionSource<bool>();
            _tasks.Enqueue((wrappedTaskFunc, completedTask));
            return completedTask.Task;
        }

        public Task ExecuteNetworkTask(
            Func<Task> taskFunc,
            Func<Exception, Task> onUnknownExceptionCallbackFunc = null,
            Func<Exception, Task> onNetworkExceptionCallbackFunc = null,
            bool keepCurrentAppState = false,
            bool drawErrorMessage = true,
            bool ignoreConnectionState = false
        )
        {
            return WrapNetworkTask(
                taskFunc,
                onUnknownExceptionCallbackFunc,
                onNetworkExceptionCallbackFunc,
                keepCurrentAppState,
                drawErrorMessage,
                ignoreConnectionState)();
        }

        public void Dispose()
        {
            Clear();
        }

        private Func<Task> WrapNetworkTask(
            Func<Task> taskFunc,
            Func<Exception, Task> onUnknownExceptionCallbackFunc,
            Func<Exception, Task> onNetworkExceptionCallbackFunc,
            bool keepCurrentAppState,
            bool drawErrorMessage,
            bool ignoreConnectionState
        )
        {
            if (taskFunc == null)
                throw new ArgumentNullException(nameof(taskFunc));

            return async () =>
            {
                if (!ignoreConnectionState && !_backendFacade.IsConnected)
                {
                    Log.Warn($"Tried to execute network action ({taskFunc.Target}[{taskFunc.Method}]) when Connection state is Disconnected.");
                    return;
                }

                await ExecuteNetworkAction(taskFunc,
                    onUnknownExceptionCallbackFunc,
                    onNetworkExceptionCallbackFunc,
                    keepCurrentAppState,
                    drawErrorMessage);
            };
        }

        private async Task ExecuteNetworkAction(
            Func<Task> taskFunc,
            Func<Exception, Task> onUnknownExceptionCallbackFunc = null,
            Func<Exception, Task> onNetworkExceptionCallbackFunc = null,
            bool keepCurrentAppState = false,
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
                _appStateManager.HandleNetworkExceptionFlow(exception, keepCurrentAppState, drawErrorMessage);
                if (onNetworkExceptionCallbackFunc != null)
                {
                    await onNetworkExceptionCallbackFunc(exception);
                }

                throw;
            }
            catch (Client.RpcClientException exception)
            {
                Helpers.ExceptionReporter.SilentReportException(exception);
                Log.Warn(" RpcException == " + exception);
                _appStateManager.HandleNetworkExceptionFlow(exception, keepCurrentAppState, drawErrorMessage);
                if (onNetworkExceptionCallbackFunc != null)
                {
                    await onNetworkExceptionCallbackFunc(exception);
                }

                throw;
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

                throw;
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
