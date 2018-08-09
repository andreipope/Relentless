


using System;
using System.Threading.Tasks;
using LoomNetwork.CZB.Data;
using Loom.Newtonsoft.Json;
using UnityEngine;

namespace LoomNetwork.CZB
{
    public partial class DataManager
    {
        private async Task GetCollectionData()
        {
            try
            {
                var getCollectionResponse = await LoomManager.Instance.GetCardCollection(LoomManager.Instance.UserDataModel.UserId);
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
