using System;
using System.Collections;
using System.Threading.Tasks;

namespace Loom.ZombieBattleground.Test
{
    public class BaseIntegrationTest
    {
        protected TestHelper TestHelper => TestHelper.Instance;

        protected IEnumerator AsyncTest(Func<Task> taskFunc, float timeout = 300)
        {
            return AsyncTestRunner.Instance.RunAsyncTest(taskFunc, timeout);
        }
    }
}

