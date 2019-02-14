using System.Collections;
using Loom.ZombieBattleground.Common;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Loom.ZombieBattleground.Test
{
    public class LevelUpTests : BaseIntegrationTest
    {
        [UnityTest]
        [Timeout(500000)]
        public IEnumerator CreateHordeAndCancel()
        {
            return AsyncTest(async () =>
            {
                IUIManager uiManager = GameClient.Get<IUIManager>();

                await TestHelper.LetsThink();
                uiManager.DrawPopup<LevelUpPopup>();
                await TestHelper.LetsThink(10);
            });
        }
    }
}
