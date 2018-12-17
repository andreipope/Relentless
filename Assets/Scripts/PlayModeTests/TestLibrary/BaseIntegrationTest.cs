using System;
using System.Collections;
using System.Threading.Tasks;
using Loom.ZombieBattleground.BackendCommunication;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Loom.ZombieBattleground.Test
{
    public class BaseIntegrationTest
    {
        protected TestHelper TestHelper = TestHelper.Instance;

        #region Setup & TearDown

        [UnitySetUp]
        public virtual IEnumerator PerTestSetup()
        {
            yield return  TestHelper.TaskAsIEnumerator(TestHelper.PerTestSetup());

            TestHelper.DebugCheatsConfiguration = new DebugCheatsConfiguration();
        }

        [UnityTearDown]
        public virtual IEnumerator PerTestTearDown()
        {
            return AsyncTest(async () =>
            {
                TestHelper.DebugCheatsConfiguration = new DebugCheatsConfiguration();

                if (false && TestContext.CurrentContext.Test.Name == "TestN_Cleanup")
                {
                    await TestHelper.TearDown_Cleanup();
                }
                else
                {
                    await TestHelper.TearDown_GoBackToMainScreen();
                }

                TestHelper.ReportTestTime();

                /*await _testHelper.PerTestTearDown();

                _testHelper.ReportTestTime();*/
            });
        }

        #endregion

        protected IEnumerator AsyncTest(Func<Task> taskFunc)
        {
            return TestHelper.TaskAsIEnumerator(taskFunc);
        }
    }
}
