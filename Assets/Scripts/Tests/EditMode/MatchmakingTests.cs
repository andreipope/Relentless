using System;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Loom.Client;
using Loom.ZombieBattleground.BackendCommunication;
using UnityEngine.TestTools;
using Debug = UnityEngine.Debug;

namespace Loom.ZombieBattleground.Test
{
    public class MatchmakingTests
    {
        private static int[] MatchmakeTestCases = {
            2,
            10,
            30,
            50,
            70,
            100,
            200,
            300
        };

        private readonly Queue<Func<Task>> _failedTestsCleanupTasks = new Queue<Func<Task>>();

        [UnityTest]
        [Timeout(30000)]
        public IEnumerator Matchmake([ValueSource(nameof(MatchmakeTestCases))] int clientCount)
        {
            return TestUtility.AsyncTest(async () =>
            {
                List<MultiplayerDebugClient> clients =
                    Enumerable.Range(0, clientCount)
                        .Select(_ => new MultiplayerDebugClient())
                        .ToList();

                async Task Cleanup()
                {
                    await Task.WhenAll(clients.Select(client => client.Reset()).ToArray());
                }

                _failedTestsCleanupTasks.Enqueue(Cleanup);

                try
                {
                    clients.ForEach(client => client.DeckId = 1);

                    Random random = new Random();
                    int counter = 1;

                    await Task.WhenAll(
                        clients
                            .Select(client =>
                            {
                                Func<Task> t = async () =>
                                {
                                    Stopwatch sw = Stopwatch.StartNew();
                                    await client.Start(
                                        TestContext.CurrentContext.Test.Name + "_" + random.Next(int.MinValue, int.MaxValue).ToString(),
                                        enabledLogs: false,
                                        chainClientCallExecutor: new DumbDAppChainClientCallExecutor(new DAppChainClientConfigurationProvider(new DAppChainClientConfiguration()))
                                    );
                                    sw.Stop();
                                    Debug.Log($"Started client {counter} in {sw.ElapsedMilliseconds} ms");
                                    counter++;
                                };

                                return t();
                            })
                            .ToArray()
                    );

                    Assert.AreEqual(clients.Count, clients.Select(client => client.UserDataModel.UserId).Distinct().ToArray().Length);

                    Debug.Log($"Created {clientCount} clients");

                    int confirmationCount = 0;
                    Action<MatchMetadata> onMatchConfirmed = metadata =>
                    {
                        confirmationCount++;
                        Debug.Log("Got confirmation " + confirmationCount);
                    };

                    clients.ForEach(client => client.MatchMakingFlowController.MatchConfirmed += onMatchConfirmed);

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
                }
                finally
                {
                    await Cleanup();

                    Debug.Log($"Stopped {clientCount} clients");
                }
            });
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
                        Debug.LogException(e);
                    }
                }
            }, 10000);
        }
    }
}
