#define DEBUG_SCENARIO_PLAYER

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Loom.ZombieBattleground.Protobuf;
using UnityEngine;

namespace Loom.ZombieBattleground.Test
{
    /// <summary>
    /// Plays automated scripted PvP matches.
    /// </summary>
    public class MatchScenarioPlayer : IDisposable
    {
        private readonly TestHelper _testHelper;
        private readonly IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> _turns;

        private readonly MultiplayerDebugClient _opponentClient;
        private readonly Queue<Func<Task>> _actionsQueue = new Queue<Func<Task>>();
        private readonly IPlayerActionTestProxy _localProxy;
        private readonly IPlayerActionTestProxy _opponentProxy;
        private readonly QueueProxyPlayerActionTestProxy _localQueueProxy;
        private readonly QueueProxyPlayerActionTestProxy _opponentQueueProxy;
        private readonly SemaphoreSlim _opponentClientTurnSemaphore = new SemaphoreSlim(1);

        private int _currentTurn;
        private bool _aborted;
        private Exception _abortException;

        public bool Completed { get; private set; }

        private QueueProxyPlayerActionTestProxy _lastQueueProxy;

        public MatchScenarioPlayer(TestHelper testHelper, IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns)
        {
            _testHelper = testHelper;
            _turns = turns;

            _opponentClient = _testHelper.GetOpponentDebugClient();

            _opponentProxy = new DebugClientPlayerActionTestProxy(_testHelper, _opponentClient);
            _localProxy = new LocalClientPlayerActionTestProxy(_testHelper);

            _localQueueProxy = new QueueProxyPlayerActionTestProxy(this, () => _actionsQueue, _localProxy);
            _opponentQueueProxy = new QueueProxyPlayerActionTestProxy(this, () => _actionsQueue, _opponentProxy);

            _opponentClient.BackendFacade.PlayerActionDataReceived += OnBackendFacadeOnPlayerActionDataReceived;
        }

        public async Task Play()
        {
#if DEBUG_SCENARIO_PLAYER
            Debug.Log("[ScenarioPlayer]: Play 1 - HandleOpponentClientTurn");
#endif

            // Special handling for the first turn
            await HandleOpponentClientTurn(true);

#if DEBUG_SCENARIO_PLAYER
            Debug.Log("[ScenarioPlayer]: Play 2 - PlayMoves");
#endif

            await _testHelper.PlayMoves(LocalPlayerTurnTaskGenerator);
            Completed = true;

#if DEBUG_SCENARIO_PLAYER
            Debug.Log("[ScenarioPlayer]: Play 3 - Finished");
#endif

            // Rethrow the exception here, to make sure it's thrown on the main thread,
            // so that the testing framework could react accordingly
            if (_abortException != null)
            {
                throw _abortException;
            }
        }

        public void AbortNextMoves()
        {
            Completed = true;
        }

        public void Dispose()
        {
            if (_opponentClient?.BackendFacade != null)
            {
                _opponentClient.BackendFacade.PlayerActionDataReceived -= OnBackendFacadeOnPlayerActionDataReceived;
            }
        }

        private async Task HandleOpponentClientTurn(bool isFirstTurn)
        {
            if (Completed)
                return;

            try
            {
                await _opponentClientTurnSemaphore.WaitAsync();
                bool isCurrentTurn = await _opponentProxy.GetIsCurrentTurn();
                if (isCurrentTurn)
                {
                    await PlayNextOpponentClientTurn(isFirstTurn);
                }
            }
            finally
            {
                _opponentClientTurnSemaphore.Release();
            }
        }

        private async Task PlayNextOpponentClientTurn(bool isFirstTurn)
        {
            if (Completed)
                return;

#if DEBUG_SCENARIO_PLAYER
            Debug.Log($"[ScenarioPlayer]: PlayNextOpponentClientTurn, current turn {_currentTurn}");
#endif
            bool success = CreateTurn(
                _opponentQueueProxy,
                true,
                proxy =>
                {
                    _actionsQueue.Enqueue(WaitForLocalPlayerTurn);
                });

            if (!success)
                return;

            // If the opponent had first turn, it would have submitted his turn before local client realized that
            if (!isFirstTurn)
            {
                _actionsQueue.Enqueue(WaitForLocalPlayerTurn);
            }

            await PlayQueue();
        }

        private Func<Task> LocalPlayerTurnTaskGenerator()
        {
#if DEBUG_SCENARIO_PLAYER
            Debug.Log($"[ScenarioPlayer]: LocalPlayerTurnTaskGenerator, current turn {_currentTurn}");
#endif
            if (!CreateTurn(_localQueueProxy, false))
                return null;

            return PlayQueue;
        }

        private bool CreateTurn(QueueProxyPlayerActionTestProxy queueProxy, bool isOpponent, Action<QueueProxyPlayerActionTestProxy> beforeTurnActionCallback = null)
        {
            if (_lastQueueProxy == queueProxy)
                throw new Exception("Multiple turns in a row from the same player are not allowed");

            if (!isOpponent && _currentTurn >= _turns.Count)
            {
                Completed = true;
                return false;
            }

            if (Completed)
                return false;

            _lastQueueProxy = queueProxy;
            Action<QueueProxyPlayerActionTestProxy> turnAction;
            if (isOpponent && _currentTurn >= _turns.Count)
            {
                // If the last turn happens to be by the opponent, local player task runner will get stuck waiting for its turn.
                // So just end the opponent turn to hand control to the local player runner.
                turnAction = proxy => {};
                Completed = true;
            }
            else
            {
                turnAction = _turns[_currentTurn];
            }

            beforeTurnActionCallback?.Invoke(queueProxy);
            try
            {
                turnAction(queueProxy);
            }
            catch (Exception e)
            {
                Completed = true;
                _abortException = e;

                // FIXME: this is not ideal, but allows the test to end gracefully.
                // A better option would be to cancel the TestHelper.PlayMoves, but that's a bit involved
                _opponentProxy.LeaveMatch();
                throw;
            }

            // EndTurn is added as last action in turn automatically
            queueProxy.EndTurn();

            _currentTurn++;
            return true;
        }

        private async Task PlayQueue()
        {
            while (_actionsQueue.Count > 0)
            {
                Func<Task> turnFunc = _actionsQueue.Dequeue();
                await turnFunc();
            }
        }

        private async Task WaitForLocalPlayerTurn()
        {
            while (_testHelper.GameplayManager.IsLocalPlayerTurn())
            {
                await new WaitForUpdate();
            }
        }

        private async void OnBackendFacadeOnPlayerActionDataReceived(byte[] bytes)
        {
            // Switch to main thread
            await new WaitForUpdate();

            if (Completed)
                return;

            try
            {
                MultiplayerDebugClient opponentClient = _testHelper.GetOpponentDebugClient();
                PlayerActionEvent playerActionEvent = PlayerActionEvent.Parser.ParseFrom(bytes);
                bool? isOpponentPlayer = playerActionEvent.PlayerAction != null ?
                    playerActionEvent.PlayerAction.PlayerId == opponentClient.UserDataModel.UserId :
                    (bool?) null;

                if (isOpponentPlayer != null && playerActionEvent.PlayerAction.ActionType == PlayerActionType.Types.Enum.EndTurn)
                {
                    await HandleOpponentClientTurn(false);
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}
