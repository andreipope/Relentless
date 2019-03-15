using System;
using NUnit.Framework;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using Loom.Client;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Protobuf;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using Debug = UnityEngine.Debug;
using Random = System.Random;

namespace Loom.ZombieBattleground.Test
{
    [Ignore("hangs sometimes")]
    public class StressTests
    {
        private static readonly ILog Log = Logging.GetLog(nameof(StressTests));

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
        [Timeout(int.MaxValue)]
        public IEnumerator Matchmake([ValueSource(nameof(MatchmakeTestCases))] int clientCount)
        {
            return AsyncTestRunner.Instance.RunAsyncTest(async () =>
            {
                await MatchmakingTestBase(clientCount, null);
            }, 120);
        }

        [UnityTest]
        [Timeout(int.MaxValue)]
        //[Ignore("")]
        public IEnumerator MatchmakeAndDoTurns([ValueSource(nameof(MatchmakeTestCases))] int clientCount)
        {
            int turnCount = 20;

            return AsyncTestRunner.Instance.RunAsyncTest(() => MatchmakingTestBase(
                clientCount,
                async clients =>
                {
                    double startTime = Utilites.GetTimestamp();
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
                            Log.Info($"[{client.UserDataModel.UserId}] Ending turn " + clientTurnCount);
                            clientTurnCount++;
                            clientToTurns[client] = clientTurnCount;

                            await client.BackendFacade.SendPlayerAction(
                                client.MatchRequestFactory.CreateAction(
                                    client.PlayerActionFactory.EndTurn()
                                )
                            );

                            Log.Info($"[{client.UserDataModel.UserId}] Made {clientTurnCount} turns");
                        }
                    }

                    foreach (MultiplayerDebugClient client in clients)
                    {
                        AsyncTestRunner.Instance.ThrowIfCancellationRequested();
                        client.BackendFacade.PlayerActionDataReceived += async bytes =>
                        {
                            PlayerActionEvent playerActionEvent = PlayerActionEvent.Parser.ParseFrom(bytes);
                            bool? isLocalPlayer =
                                playerActionEvent.PlayerAction != null ?
                                    playerActionEvent.PlayerAction.PlayerId == client.UserDataModel.UserId :
                                    (bool?) null;

                            if (isLocalPlayer != null)
                            {
                                await TaskThreadedWrapper(() => EndTurnIfCurrentTurn(client));
                            }
                        };
                    }

                    await Task.WhenAll(
                        clients
                            .Select(client => TaskThreadedWrapper(() => EndTurnIfCurrentTurn(client)))
                            .ToArray()
                    );

                    while (true)
                    {
                        AsyncTestRunner.Instance.ThrowIfCancellationRequested();
                        await Task.Delay(200, AsyncTestRunner.Instance.CurrentTestCancellationToken);

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

                    Log.InfoFormat("completed in {0:F2}s", Utilites.GetTimestamp() - startTime);
                }), 60f);
        }

        private async Task MatchmakingTestBase(int clientCount, Func<List<MultiplayerDebugClient>, Task> onEndCallback = null)
        {
            int counter = 0;
            Random random = new Random();
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
c
            try
            {
                clients.ForEach(client => client.DeckId = 1);

                await Task.WhenAll(
                    clients
                        .Select(client =>
                        {
                            float delay = CalculateFuzzDelay(clientCount, 10f, random);
                            return TaskThreadedWrapper(async () =>
                            {
                                Log.Info("Waiting for " + delay + "s");
                                await Task.Delay((int) (delay * 1000f));
                                DAppChainClientConfiguration clientConfiguration = new DAppChainClientConfiguration
                                {
                                    CallTimeout = 15000,
                                    StaticCallTimeout = 15000
                                };
                                await client.Start(
                                    contract => new CustomContractCallProxy(contract, true, false),
                                    clientConfiguration,
                                    enabledLogs: false,
                                    chainClientCallExecutor: new DefaultDAppChainClientCallExecutor(clientConfiguration)
                                );
                                client.MatchMakingFlowController.ActionWaitingTime = 5;
                            });
                        })
                        .ToArray()
                );

                Assert.AreEqual(clients.Count, clients.Select(client => client.UserDataModel.UserId).Distinct().ToArray().Length);
                Log.Info($"Created {clientCount} clients");

                int confirmationCount = 0;
                Action<MultiplayerDebugClient, MatchMetadata> onMatchConfirmed = (client, metadata) =>
                {
                    confirmationCount++;
                    Log.Info("Got confirmation " + confirmationCount);

                    client.MatchRequestFactory = new MatchRequestFactory(metadata.Id);
                    client.PlayerActionFactory = new PlayerActionFactory(client.UserDataModel.UserId);
                };

                clients.ForEach(client => client.MatchMakingFlowController.MatchConfirmed += metadata => onMatchConfirmed(client, metadata));

                await Task.WhenAll(
                    clients
                        .Select(client =>
                        {
                            float delay = CalculateFuzzDelay(clientCount, 60f, random);
                            return TaskThreadedWrapper(async () =>
                            {
                                Log.Info("waiting for " + delay + "s");
                                await Task.Delay((int) (delay * 1000f));
                                await client.MatchMakingFlowController.Start(1, null, null, false, null);
                            });
                        })
                        .ToArray()
                );

                Log.Info($"Started {clientCount} clients");

                while (confirmationCount != clientCount)
                {
                    AsyncTestRunner.Instance.ThrowIfCancellationRequested();
                    await Task.Delay(100, AsyncTestRunner.Instance.CurrentTestCancellationToken);
                    await Task.WhenAll(
                        clients
                            .Select(client => TaskThreadedWrapper(client.Update))
                            .ToArray()
                    );
                }

                Assert.AreEqual(clientCount, confirmationCount);

                if (onEndCallback != null)
                {
                    await onEndCallback(clients);
                }
            }
            finally
            {
                await Cleanup();

                Log.Info($"Stopped {clientCount} clients");
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
                        Log.Error("Exception during test cleanup", e);
                    }
                }
            }, 10000);
        }

        private static float CalculateFuzzDelay(int clientCount, float scale, Random random)
        {
            float delay = (float) random.NextDouble() * scale * Mathf.Max(0.25f, clientCount / 300f);
            return delay;
        }

        private static async Task TaskThreadedWrapper(Func<Task> taskFunc, Action<Exception> onExceptionCallback = null)
        {
            try
            {
                await Task.Run(taskFunc);
            }
            catch (Exception e)
            {
                Log.Error("", e);
                onExceptionCallback?.Invoke(e);
                ExceptionDispatchInfo.Capture(e).Throw();
            }
        }
    }
}
