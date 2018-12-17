using System.Collections;
using Loom.ZombieBattleground.BackendCommunication;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Loom.ZombieBattleground.Test
{
    public class BaseIntegrationTest
    {
        protected TestHelper _testHelper = TestHelper.Instance;

        #region Setup & TearDown

        [UnitySetUp]
        public virtual IEnumerator PerTestSetup()
        {
            yield return _testHelper.PerTestSetup();

            _testHelper.DebugCheatsConfiguration = new DebugCheatsConfiguration();

        }

        [UnityTearDown]
        public virtual IEnumerator PerTestTearDown()
        {
            _testHelper.DebugCheatsConfiguration = new DebugCheatsConfiguration();

            if (false && TestContext.CurrentContext.Test.Name == "TestN_Cleanup")
            {
                yield return _testHelper.TearDown_Cleanup();
            }
            else
            {
                yield return _testHelper.TearDown_GoBackToMainScreen();
            }

            _testHelper.ReportTestTime();

            /*yield return _testHelper.PerTestTearDown();

            _testHelper.ReportTestTime();*/
        }

        #endregion

        [UnityTest]
        public IEnumerator TestN_Cleanup()
        {
            // Nothing, just to ascertain cleanup

            yield return null;
        }
    }
}
