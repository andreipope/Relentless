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
    public class MatchScenarioPlayer
    {
        private readonly TestHelper _testHelper;
        private readonly IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> _turns;
        private readonly Queue<Func<Task>> _actionsQueue = new Queue<Func<Task>>();
        private readonly IPlayerActionTestProxy _localProxy;
        private readonly IPlayerActionTestProxy _opponentProxy;
        private readonly QueueProxyPlayerActionTestProxy _localQueueProxy;
        private readonly QueueProxyPlayerActionTestProxy _opponentQueueProxy;
        private readonly SemaphoreSlim _opponentClientTurnSemaphore = new SemaphoreSlim(1);

        private int _currentTurn;
        private QueueProxyPlayerActionTestProxy _lastQueueProxy;

        public MatchScenarioPlayer(TestHelper testHelper, IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns)
        {
            _testHelper = testHelper;
            _turns = turns;

            MultiplayerDebugClient opponentClient = _testHelper.GetOpponentDebugClient();

            _opponentProxy = new DebugClientPlayerActionTestProxy(_testHelper, opponentClient);
            _localProxy = new LocalClientPlayerActionTestProxy(_testHelper);

            _localQueueProxy = new QueueProxyPlayerActionTestProxy(() => _actionsQueue, _localProxy);
            _opponentQueueProxy = new QueueProxyPlayerActionTestProxy(() => _actionsQueue, _opponentProxy);

            opponentClient.BackendFacade.PlayerActionDataReceived += OnBackendFacadeOnPlayerActionDataReceived;
        }

        public async Task Play()
        {
#if DEBUG_SCENARIO_PLAYER
            Debug.Log("[ScenarioPlayer]: Play 1 - HandleOpponentClientTurn");
#endif

            // Special handling for the first turn
            await TestHelper.TaskAsIEnumerator(() => HandleOpponentClientTurn(true));

#if DEBUG_SCENARIO_PLAYER
            Debug.Log("[ScenarioPlayer]: Play 2 - PlayMoves");
#endif

            await _testHelper.PlayMoves(LocalPlayerTurnTaskGenerator);

#if DEBUG_SCENARIO_PLAYER
            Debug.Log("[ScenarioPlayer]: Play 3 - Finished");
#endif
        }

        private async Task HandleOpponentClientTurn(bool isFirstTurn)
        {
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
#if DEBUG_SCENARIO_PLAYER
            Debug.Log($"[ScenarioPlayer]: PlayNextDebugClientTurn, current turn {_currentTurn}");
#endif
            bool success = CreateTurn(_opponentQueueProxy,
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
            if (!CreateTurn(_localQueueProxy))
                return null;

            return PlayQueue;
        }

        private bool CreateTurn(QueueProxyPlayerActionTestProxy queueProxy, Action<QueueProxyPlayerActionTestProxy> beforeTurnActionCallback = null)
        {
            if (_lastQueueProxy == queueProxy)
                throw new Exception("Multiple turns in a row from the same player are not allowed");

            if (_currentTurn == _turns.Count)
                return false;

            _lastQueueProxy = queueProxy;
            Action<QueueProxyPlayerActionTestProxy> turnAction = _turns[_currentTurn];
            beforeTurnActionCallback?.Invoke(queueProxy);
            turnAction(queueProxy);

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
                await new WaitForEndOfFrame();
            }
        }

        private async void OnBackendFacadeOnPlayerActionDataReceived(byte[] bytes)
        {
            // Switch to main thread
            await new WaitForUpdate();

            try
            {
                MultiplayerDebugClient opponentClient = _testHelper.GetOpponentDebugClient();
                PlayerActionEvent playerActionEvent = PlayerActionEvent.Parser.ParseFrom(bytes);
                bool? isLocalPlayer = playerActionEvent.PlayerAction != null ?
                    playerActionEvent.PlayerAction.PlayerId == opponentClient.UserDataModel.UserId :
                    (bool?) null;

                if (isLocalPlayer != null)
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
