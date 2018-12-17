using System;
using NUnit.Framework;
using System.Collections;
using System.Threading.Tasks;
using Loom.ZombieBattleground.BackendCommunication;
using UnityEngine.TestTools;
using Debug = UnityEngine.Debug;

namespace Loom.ZombieBattleground.Test
{
    public class MatchmakingTests
    {
        [UnityTest]
        [Timeout(10000)]
        public IEnumerator Test_A_Matchmake()
        {
            return TestUtility.AsyncTest(async () =>
            {
                MultiplayerDebugClient client1 = new MultiplayerDebugClient();
                MultiplayerDebugClient client2 = new MultiplayerDebugClient();

                client1.DeckId = client2.DeckId = 1;

                Task client1Start = client1.Start(TestContext.CurrentContext.Test.Name);
                Task client2Start = client2.Start(TestContext.CurrentContext.Test.Name);
                await Task.WhenAll(client1Start, client2Start);

                int confirmations = 0;
                Action<MatchMetadata> onMatchConfirmed = metadata =>
                {
                    confirmations++;
                    Debug.Log("Got confirmation " + confirmations);
                };
                client1.MatchMakingFlowController.MatchConfirmed += onMatchConfirmed;
                client2.MatchMakingFlowController.MatchConfirmed += onMatchConfirmed;

                Task client1MatchmakingStart = client1.MatchMakingFlowController.Start(1, null, null, false, null);
                Task client2MatchmakingStart = client2.MatchMakingFlowController.Start(1, null, null, false, null);
                await Task.WhenAll(client1MatchmakingStart, client2MatchmakingStart);

                while (confirmations != 2)
                {
                    await Task.Delay(100);
                    Task client1Update = client1.Update();
                    Task client2Update = client2.Update();
                    await Task.WhenAll(client1Update, client2Update);

                }
            });
        }
    }
}
