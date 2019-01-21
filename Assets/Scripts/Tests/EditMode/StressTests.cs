using System;
using NUnit.Framework;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Loom.Client;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Protobuf;
using UnityEngine.TestTools;
using Debug = UnityEngine.Debug;

namespace Loom.ZombieBattleground.Test
{
    public class StressTests
    {
        private static int[] MatchmakeTestCases = {
            2,
            10,
            /*30,
            50,
            70,
            100,
            200,
            300*/
        };

        private readonly Queue<Func<Task>> _failedTestsCleanupTasks = new Queue<Func<Task>>();

        [UnityTest]
        [Timeout(30000)]
        public IEnumerator Matchmake([ValueSource(nameof(MatchmakeTestCases))] int clientCount)
        {
            return TestUtility.AsyncTest(async () =>
            {
                await MatchmakingTestBase(clientCount, null);
            });
        }

        [UnityTest]
        [Timeout(60000)]
        public IEnumerator MatchmakeAndDoTurns([ValueSource(nameof(MatchmakeTestCases))] int clientCount)
        {
            int turnCount = 20;

            return TestUtility.AsyncTest(() => MatchmakingTestBase(clientCount,
                async clients =>
                {
                    ConcurrentDictionary<MultiplayerDebugClient, int> clientToTurns = new ConcurrentDictionary<MultiplayerDebugClient, int>();

                    async Task EndTurnIfCurrentTurn(MultiplayerDebugClient client)
                    {
                        int clientTurnCount = clientToTurns.GetOrAdd(client, 0);
                        if (clientTurnCount == turnCount)
                            return;

                        GetGameStateResponse gameStateResponse =
                            await client.BackendFacade.GetGameState(client.MatchMakingFlowController.MatchMetadata.Id);
                        GameState gameState = gameStateResponse.GameState;
                        if (gameState.PlayerStates[gameState.CurrentPlayerIndex].Id == client.UserDataModel.UserId)
                        {
                            Debug.Log("ending turn " + clientTurnCount);
                            clientTurnCount++;
                            clientToTurns[client] = clientTurnCount;

                            await client.BackendFacade.SendPlayerAction(
                                client.MatchRequestFactory.CreateAction(
                                    client.PlayerActionFactory.EndTurn()
                                )
                            );
                        }
                    }

                    foreach (MultiplayerDebugClient client in clients)
                    {
                        client.BackendFacade.PlayerActionDataReceived += async bytes =>
                        {
                            PlayerActionEvent playerActionEvent = PlayerActionEvent.Parser.ParseFrom(bytes);
                            bool? isLocalPlayer =
                                playerActionEvent.PlayerAction != null ?
                                    playerActionEvent.PlayerAction.PlayerId == client.UserDataModel.UserId :
                                    (bool?) null;

                            if (isLocalPlayer != null)
                            {
                                await EndTurnIfCurrentTurn(client);
                            }
                        };
                    }

                    await Task.WhenAll(
                        clients
                            .Select(EndTurnIfCurrentTurn)
                            .ToArray()
                    );

                    while (true)
                    {
                        await Task.Delay(200);

                        bool allPlayed = true;
                        foreach (KeyValuePair<MultiplayerDebugClient,int> pair in clientToTurns)
                        {
                            if (pair.Value != turnCount)
                            {
                                allPlayed = false;
                                break;
                            }
                        }

                        if (allPlayed)
                            break;
                    }
                }));
        }

        private async Task MatchmakingTestBase(int clientCount, Func<List<MultiplayerDebugClient>, Task> onEndCallback = null)
        {
            int counter = 0;
            List<MultiplayerDebugClient> clients =
                Enumerable.Range(0, clientCount)
                    .Select(_ => new MultiplayerDebugClient(TestContext.CurrentContext.Test.Name + "_" + counter++.ToString()))
                    .ToList();

            Assert.AreEqual(clients.Count, clients.Select(client => client.UserDataModel.UserId).Distinct().ToArray().Length);
            Assert.AreEqual(clients.Count, clients.Select(client => CryptoUtils.BytesToHexString(client.UserDataModel.PrivateKey)).Distinct().ToArray().Length);

            async Task Cleanup()
            {
                await Task.WhenAll(clients.Select(client => client.Reset()).ToArray());
            }

            _failedTestsCleanupTasks.Enqueue(Cleanup);

            try
            {
                clients.ForEach(client => client.DeckId = 1);

                await Task.WhenAll(
                    clients
                        .Select(client =>
                        {
                            Func<Task> t = async () =>
                            {
                                await client.Start(
                                    enabledLogs: false,
                                    chainClientCallExecutor: new DumbDAppChainClientCallExecutor(
                                        new DAppChainClientConfigurationProvider(new DAppChainClientConfiguration())),
                                    contractCallProxyFactory: contract => new ThreadedTimeMetricsContractCallProxy(contract, true, false)
                                );
                            };

                            return t();
                        })
                        .ToArray()
                );

                Assert.AreEqual(clients.Count, clients.Select(client => client.UserDataModel.UserId).Distinct().ToArray().Length);

                Debug.Log($"Created {clientCount} clients");

                int confirmationCount = 0;
                Action<MultiplayerDebugClient, MatchMetadata> onMatchConfirmed = (client, metadata) =>
                {
                    confirmationCount++;
                    Debug.Log("Got confirmation " + confirmationCount);

                    client.MatchRequestFactory = new MatchRequestFactory(metadata.Id);
                    client.PlayerActionFactory = new PlayerActionFactory(client.UserDataModel.UserId);
                };

                clients.ForEach(client => client.MatchMakingFlowController.MatchConfirmed += metadata => onMatchConfirmed(client, metadata));

                await Task.WhenAll(
                    clients
                        .Select(client => client.MatchMakingFlowController.Start(1, null, null, false, null))
                        .ToArray()
                );

                Debug.Log($"Started {clientCount} clients");

                while (confirmationCount != clientCount)
                {
                    await Task.Delay(200);
                    await Task.WhenAll(
                        clients
                            .Select(client => client.Update())
                            .ToArray()
                    );
                }

                Assert.AreEqual(clientCount, confirmationCount);

                if (onEndCallback != null)
                {
                    await onEndCallback(clients);
                }
            }
            catch(Exception e)
            {
                Helpers.ExceptionReporter.LogException(e);
            }
            finally
            {
                await Cleanup();

                Debug.Log($"Stopped {clientCount} clients");
            }
        }

        [UnityTearDown]
        public IEnumerator Cleanup()
        {
            return TestUtility.AsyncTest(async () =>
            {
                while (_failedTestsCleanupTasks.Count > 0)
                {
                    try
                    {
                        Func<Task> func = _failedTestsCleanupTasks.Dequeue();
                        await func();
                    }
                    catch (Exception e)
                    {
                        Helpers.ExceptionReporter.LogException(e);
                        Debug.LogException(e);
                    }
                }
            }, 10000);
        }
    }
}
