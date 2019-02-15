using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Loom.ZombieBattleground.Test
{
    public static class TestUtility
    {
        public static IEnumerator AsyncTest(Func<Task> testAction, int? overrideTimeout = null)
        {
            int timeout =
                TestContext.CurrentTestExecutionContext.TestCaseTimeout <= 0 ?
                    Timeout.Infinite :
                    TestContext.CurrentTestExecutionContext.TestCaseTimeout;
            if (overrideTimeout != null && overrideTimeout.Value > 0)
            {
                timeout = overrideTimeout.Value;
            }

            return TestHelper.TaskAsIEnumerator(testAction, timeout);
        }
    }
}
