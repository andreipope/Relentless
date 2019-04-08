using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Common;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

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

