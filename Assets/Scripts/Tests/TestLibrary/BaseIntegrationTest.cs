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

        #region Setup & TearDown

        /*[UnitySetUp]
        public virtual IEnumerator PerTestSetup()
        {
            return AsyncTest(async () =>
            {
                await TestHelper.PerTestSetup();

                TestHelper.DebugCheats.CopyFrom(new DebugCheatsConfiguration());
            });
        }

        [UnityTearDown]
        public virtual IEnumerator PerTestTearDown()
        {
            return AsyncTest(async () =>
            {
                TestHelper.DebugCheats.CopyFrom(new DebugCheatsConfiguration());

                if (false && TestContext.CurrentContext.Test.Name == "TestN_Cleanup")
                {
                    await TestHelper.TearDown_Cleanup();
                }
                else
                {
                    await TestHelper.TearDown();
                }

                /*await _testHelper.PerTestTearDown();

                _testHelper.ReportTestTime();#1#
            });
        }*/

        #endregion

        protected IEnumerator AsyncTest(Func<Task> taskFunc)
        {
            return IntegrationTestRunner.Instance.AsyncTest(taskFunc);
            /*return TestHelper.TaskAsIEnumerator(async () =>
            {
                try
                {
                    await taskFunc();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
                finally
                {
                    TestHelper.TestEndHandler();
                }
            });*/
        }
    }
}
