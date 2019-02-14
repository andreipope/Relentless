using System.Collections;
using NUnit.Framework;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections.Generic;
using Loom.ZombieBattleground.Common;

namespace Loom.ZombieBattleground.Test
{
    public class SanityChecks : BaseIntegrationTest
    {
        [UnityTest]
        [Timeout(int.MaxValue)]
        public IEnumerator CreateAHorde()
        {
            return AsyncTest(async () =>
            {
                await TestHelper.ClickGenericButton("Button_Play");

                await TestHelper.AssertCurrentPageName(Enumerators.AppState.PlaySelection);
                await TestHelper.ClickGenericButton("Button_SoloMode");
                await TestHelper.AssertCurrentPageName(Enumerators.AppState.HordeSelection);

                await TestHelper.SelectAHordeByName("Razu", false);
                if (TestHelper.SelectedHordeIndex != -1)
                {
                    await TestHelper.RemoveAHorde(TestHelper.SelectedHordeIndex);
                }

                await TestHelper.AddRazuHorde();
                await TestHelper.AssertCurrentPageName(Enumerators.AppState.HordeSelection);
            });
        }
    }
}
