using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Common;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Loom.ZombieBattleground.Test
{
    public class BaseIntegrationTest
    {
        protected readonly TestHelper TestHelper = TestHelper.Instance;

        #region Setup & TearDown

        [UnitySetUp]
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
                    await TestHelper.TearDown_GoBackToMainScreen();
                }

                /*await _testHelper.PerTestTearDown();

                _testHelper.ReportTestTime();*/
            });
        }

        #endregion

        protected IEnumerator AsyncTest(Func<Task> taskFunc)
        {
            return TestHelper.TaskAsIEnumerator(async () =>
            {
                try
                {
                    await taskFunc();
                }
                finally
                {
                    TestHelper.TestEndHandler();
                }
            });
        }
        
        protected async Task StartOnlineMatch(int selectedHordeIndex = 0, bool createOpponent = true, IList<string> tags = null)
        {
            await TestHelper.HandleLogin();

            await TestHelper.MainMenuTransition("Button_Play");
            await TestHelper.AssertIfWentDirectlyToTutorial(TestHelper.GoBackToMainAndPressPlay);

            await TestHelper.AssertCurrentPageName(Enumerators.AppState.PlaySelection);
            await TestHelper.MainMenuTransition("Button_PvPMode");
            await TestHelper.AssertCurrentPageName(Enumerators.AppState.PvPSelection);
            await TestHelper.MainMenuTransition("Button_CasualType");
            await TestHelper.AssertCurrentPageName(Enumerators.AppState.HordeSelection);

            await TestHelper.SelectAHordeByIndex(selectedHordeIndex);
            TestHelper.RecordExpectedOverlordName(selectedHordeIndex);

            if (tags == null)
            {
                tags = new List<string>();
            }

            tags.Insert(0, "pvpTest");
            tags.Insert(1, TestHelper.GetTestName());

            TestHelper.SetPvPTags(tags);
            TestHelper.DebugCheats.Enabled = true;
            TestHelper.DebugCheats.CustomRandomSeed = 0;

            await TestHelper.LetsThink();

            await TestHelper.MainMenuTransition("Button_Battle");

            if (createOpponent)
            {
                await TestHelper.CreateAndConnectOpponentDebugClient();
            }
        }
    }
}
