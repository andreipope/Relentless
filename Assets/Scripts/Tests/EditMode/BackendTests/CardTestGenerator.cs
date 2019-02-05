using System.Collections;
using Loom.ZombieBattleground.BackendCommunication;
using UnityEngine.TestTools;

namespace Loom.ZombieBattleground.Test
{
    public class CardTestGenerator
    {
        [UnityTest]
        public IEnumerator Generate() {
            return TestHelper.TaskAsIEnumerator(async () => {
                MultiplayerDebugClient debugClient = new MultiplayerDebugClient();
                await debugClient.Start(contract => new ThreadedContractCallProxyWrapper(new DefaultContractCallProxy(contract)));


            });
        }
    }
}
