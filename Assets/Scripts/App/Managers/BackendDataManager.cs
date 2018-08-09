


using System;
using System.Threading.Tasks;
using LoomNetwork.CZB.Data;
using Loom.Newtonsoft.Json;

namespace LoomNetwork.CZB
{
    public partial class DataManager
    {
        public async void StartLoadBackend()
        {
            await GetDeckData();
            await GetHeroesData();
        }
        
        private async Task GetDeckData()
        {
            try
            {
                CachedDecksData = new DecksData();
                var listDecksResponse = await LoomManager.Instance.GetDecks(LoomManager.UserId);
                if (listDecksResponse != null)
                {
                    CustomDebug.Log(listDecksResponse.ToString());
                    CachedDecksData = JsonConvert.DeserializeObject<DecksData>(listDecksResponse.ToString());
                }
                else
                    CustomDebug.Log(" List Deck Response is Null == ");
            }
            catch (Exception ex)
            {
                CustomDebug.LogError("===== Deck Data Not Loaded from Backed ===== " + ex + " == Load from Resources ==");
            }
        }
        
        private async Task GetHeroesData()
        {
            try
            {
                CachedHeroesData = new HeroesData();
                var heroesList = await LoomManager.Instance.GetHeroesList(LoomManager.UserId);
                CustomDebug.Log(heroesList.ToString());
                CachedHeroesData = JsonConvert.DeserializeObject<HeroesData>(heroesList.ToString());
            }
            catch (Exception ex)
            {
                CustomDebug.LogError("===== Heroes List not Loaded ===== " + ex);
            }
        }

        private async Task GetCollectionData()
        {
            try
            {
                var getCollectionResponse = await LoomManager.Instance.GetCardCollection(LoomManager.UserId);
                CustomDebug.Log(getCollectionResponse.ToString());

                CachedCollectionData = getCollectionResponse.FromProtobuf();
            }
            catch (Exception ex)
            {
                CustomDebug.LogError("===== Card Collection Not Loaded ===== " + ex);
            }
        }

        private async Task GetCardLibraryData()
        {
            try
            {
                var listCardLibraryResponse = await LoomManager.Instance.GetCardLibrary();
                CustomDebug.Log(listCardLibraryResponse.ToString());
                CachedCardsLibraryData = listCardLibraryResponse.FromProtobuf();
            }
            catch (Exception ex)
            {
                CustomDebug.LogError("===== Card Library Not Loaded ===== " + ex);
            }
        }
    }
}
