using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Protobuf;
using Loom.Google.Protobuf;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using Loom.ZombieBattleground.Common;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class NetworkMessageSendManager : IService, INetworkMessageSendManager
    {
        private static readonly ILog Log = Logging.GetLog(nameof(NetworkMessageSendManager));

        private readonly Queue<Func<Task>> _tasks = new Queue<Func<Task>>();
        private BackendFacade _backendFacade;

        public bool Active { get; set; }

        public void Init()
        {
            _backendFacade = GameClient.Get<BackendFacade>();
        }

        public void Clear()
        {
            _tasks.Clear();
        }

        public async void Update()
        {
            if (!Active)
                return;

            while (_tasks.Count > 0)
            {
                await _tasks.Dequeue().Invoke();
            }
        }

        public void EnqueueMessage(IMessage request)
        {
            AddTask(async () =>
            {
                if (!_backendFacade.IsConnected)
                {
                    Log.Warn($"Tried to send {request} when Connection state is Disconnected.");
                    return;
                }

                switch (request)
                {
                    case PlayerActionRequest playerActionMessage:
                        await ExecuteNetworkAction(() => _backendFacade.SendPlayerAction(playerActionMessage));
                        break;
                    case EndMatchRequest endMatchMessage:
                        await ExecuteNetworkAction(() => _backendFacade.SendEndMatchRequest(endMatchMessage));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException($"Unknown action type: {request.GetType()}");
                }
            });
        }

        public void Dispose()
        {
            Clear();
        }

        private void AddTask(Func<Task> taskFunc)
        {
            if (taskFunc == null)
            {
                Log.Warn("Incoming Task is null");
                return;
            }

            _tasks.Enqueue(taskFunc);
        }

        private async Task ExecuteNetworkAction(Func<Task> taskFunc)
        {
            try
            {
                await taskFunc();
            }
            catch (TimeoutException exception)
            {
                Helpers.ExceptionReporter.SilentReportException(exception);
                Log.Warn(" Time out == " + exception);
                GameClient.Get<IAppStateManager>().HandleNetworkExceptionFlow(exception);
            }
            catch (Client.RpcClientException exception)
            {
                Helpers.ExceptionReporter.SilentReportException(exception);
                Log.Warn(" RpcException == " + exception);
                GameClient.Get<IAppStateManager>().HandleNetworkExceptionFlow(exception);
            }
            catch (Exception exception)
            {
                Helpers.ExceptionReporter.SilentReportException(exception);
                Log.Warn(" other == " + exception);
                ShowConnectionPopup();
            }
        }

        private void ShowConnectionPopup()
        {
            IUIManager uiManager = GameClient.Get<IUIManager>();
            IGameplayManager gameplayManager = GameClient.Get<IGameplayManager>();
            ConnectionPopup connectionPopup = uiManager.GetPopup<ConnectionPopup>();

            if (gameplayManager.CurrentPlayer == null)
            {
                return;
            }

            if (connectionPopup.Self == null)
            {
                Func<Task> connectFuncInGame = () =>
                {
                    GameClient.Get<INetworkMessageSendManager>().Clear();
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
