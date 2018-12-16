#define DEBUG_SCENARIO_PLAYER

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Loom.ZombieBattleground.Protobuf;
using UnityEngine;

namespace Loom.ZombieBattleground.Test
{
    internal class ScenarioPlayer
    {
        private readonly TestHelper _testHelper;
        private readonly IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> _turns;
        private readonly LocalClientPlayerActionTestProxy _localProxy;
        private readonly DebugClientPlayerActionTestProxy _opponentProxy;
        private readonly QueueProxyPlayerActionTestProxy _queueProxy = new QueueProxyPlayerActionTestProxy();
        private int _currentTurn;

        public ScenarioPlayer(TestHelper testHelper, IReadOnlyList<Action<QueueProxyPlayerActionTestProxy>> turns)
        {
            _testHelper = testHelper;
            _turns = turns;

            MultiplayerDebugClient opponentClient = _testHelper.GetOpponentDebugClient();

            _opponentProxy = new DebugClientPlayerActionTestProxy(_testHelper, opponentClient);
            _localProxy = new LocalClientPlayerActionTestProxy(_testHelper);

            opponentClient.BackendFacade.PlayerActionDataReceived += OnBackendFacadeOnPlayerActionDataReceived;
        }

        public IEnumerator Play()
        {
#if DEBUG_SCENARIO_PLAYER
            Debug.Log("[ScenarioPlayer]: Play 1 - HandleOpponentClientTurn");
#endif

            yield return TestHelper.TaskAsIEnumerator(() => HandleOpponentClientTurn(true));

#if DEBUG_SCENARIO_PLAYER
            Debug.Log("[ScenarioPlayer]: Play 2 - PlayMoves");
#endif

            yield return _testHelper.PlayMoves(LocalPlayerTurnTaskGenerator);

#if DEBUG_SCENARIO_PLAYER
            Debug.Log("[ScenarioPlayer]: Play 3 - Finished");
#endif
        }

        private async Task HandleOpponentClientTurn(bool isFirstTurn)
        {
            bool isCurrentTurn = await _opponentProxy.GetIsCurrentTurn();
            if (isCurrentTurn)
            {
                await PlayNextOpponentClientTurn(isFirstTurn);
            }
        }

        private async Task PlayNextOpponentClientTurn(bool isFirstTurn)
        {
#if DEBUG_SCENARIO_PLAYER
            Debug.Log($"[ScenarioPlayer]: PlayNextDebugClientTurn, current turn {_currentTurn}");
#endif
            CreateTurn(_opponentProxy);
            if (!isFirstTurn)
            {
                _queueProxy.Queue.Enqueue(async () =>
                {
                    //while (_testHelper.GameplayManager.IsLocalPlayerTurn())
                    while (_testHelper.GameplayManager.IsLocalPlayerTurn())
                    {
                        await new WaitForEndOfFrame();
                    }
                });
            }

            await PlayQueue();
        }

        private Func<Task> LocalPlayerTurnTaskGenerator()
        {
#if DEBUG_SCENARIO_PLAYER
            Debug.Log($"[ScenarioPlayer]: LocalPlayerTurnTaskGenerator, current turn {_currentTurn}");
#endif
            if (!CreateTurn(_localProxy))
                return null;

            return PlayQueue;
        }

        private bool CreateTurn(IPlayerActionTestProxy currentProxy)
        {
            if (_queueProxy.CurrentProxy == currentProxy)
                throw new Exception("Multiple turns in a row is not possible");

            if (_currentTurn == _turns.Count)
                return false;

            Action<QueueProxyPlayerActionTestProxy> turnAction = _turns[_currentTurn];
            _queueProxy.CurrentProxy = currentProxy;
            turnAction(_queueProxy);

            // End turn automatically as last action in turn
            _queueProxy.EndTurn();

            NextTurn();

            return true;
        }

        private async Task PlayQueue()
        {
            while (_queueProxy.Queue.Count > 0)
            {
                Func<Task> turnFunc = _queueProxy.Queue.Dequeue();
                await turnFunc();
            }
        }

        private void NextTurn()
        {
            _currentTurn++;
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
