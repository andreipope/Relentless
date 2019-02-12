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
        [Timeout(500000)]
        public IEnumerator CreateAHorde()
        {
            return AsyncTest(async () =>
            {
                await TestHelper.ClickGenericButton("Button_Play");

                await TestHelper.AssertIfWentDirectlyToTutorial(
                    TestHelper.GoBackToMainAndPressPlay);

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

        private void PopulateDeckWithCardsFromIndex (int index, int amount = 5) 
        {
            IGameplayManager _gameplayManager = GameClient.Get<IGameplayManager>();
            IDataManager _dataManager = GameClient.Get<IDataManager>();

            _gameplayManager.CurrentPlayerDeck.Cards = new List<Data.DeckCardData>();

            for (int i = 0; i < amount; i++)
            {
                if (index >= _dataManager.CachedCardsLibraryData.Cards.Count) {
                    index = 0;
                }

                _gameplayManager.CurrentPlayerDeck.AddCard(_dataManager.CachedCardsLibraryData.Cards[index].Name);

                index++;
            }
        }

        private async Task SkipTutorial(bool twoSteps = true)
        {
            await new WaitForSeconds(8);
            await TestHelper.ClickGenericButton("Button_Skip");

            await TestHelper.RespondToYesNoOverlay(true);

            if (twoSteps)
            {
                await TestHelper.ClickGenericButton("Button_Skip");

                await TestHelper.RespondToYesNoOverlay(true);
            }

            await new WaitForUpdate();
        }
    }
}
