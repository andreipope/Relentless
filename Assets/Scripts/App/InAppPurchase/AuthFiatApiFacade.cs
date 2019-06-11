using System;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Loom.ZombieBattleground.BackendCommunication;
using Newtonsoft.Json;
using Plugins.AsyncAwaitUtil.Source;

namespace Loom.ZombieBattleground.Iap
{
    public class AuthFiatApiFacade : IService
    {
        private static readonly ILog Log = Logging.GetLog(nameof(AuthFiatApiFacade));

        private BackendDataControlMediator _backendDataControlMediator;

        public void Init()
        {
            _backendDataControlMediator = GameClient.Get<BackendDataControlMediator>();
        }

        public void Update() { }

        public void Dispose() { }

        public async Task<ValidationResponse> RegisterTransactionAndValidate(FiatValidationData fiatValidationData)
        {
            Log.Info($"{nameof(RegisterTransactionAndValidate)}");

            WebrequestCreationInfo webrequestCreationInfo = new WebrequestCreationInfo();
            webrequestCreationInfo.Method = WebRequestMethod.POST;
            webrequestCreationInfo.Url = PlasmaChainEndpointsContainer.FiatValidationURL;
            webrequestCreationInfo.ContentType = "application/json";

            string fiatValidationDataJson = JsonConvert.SerializeObject(fiatValidationData);
            Log.Debug("Fiat validation data:\n" + JsonUtility.PrettyPrint(fiatValidationDataJson));
            webrequestCreationInfo.Data = Encoding.UTF8.GetBytes(fiatValidationDataJson);
            webrequestCreationInfo.Headers.Add("authorization", "Bearer " + _backendDataControlMediator.UserDataModel.AccessToken);

            HttpResponseMessage httpResponseMessage = await WebRequestUtils.CreateAndSendWebrequest(webrequestCreationInfo);
            httpResponseMessage.ThrowOnError(webrequestCreationInfo);

            string json = httpResponseMessage.ReadToEnd();
            Log.Info(json);
            ValidationResponse response = JsonConvert.DeserializeObject<ValidationResponse>(json);
            return response;
        }

        public async Task<List<TransactionResponse>> ListPendingTransactions()
        {
            Log.Info($"{nameof(ListPendingTransactions)}");

            WebrequestCreationInfo webrequestCreationInfo = new WebrequestCreationInfo();
            webrequestCreationInfo.Url = PlasmaChainEndpointsContainer.FiatTransactionURL;
            webrequestCreationInfo.Headers.Add("authorization", "Bearer " + _backendDataControlMediator.UserDataModel.AccessToken);

            HttpResponseMessage httpResponseMessage = await WebRequestUtils.CreateAndSendWebrequest(webrequestCreationInfo);
            httpResponseMessage.ThrowOnError(webrequestCreationInfo);
            Log.Debug(httpResponseMessage.ReadToEnd());

            string json = httpResponseMessage.ReadToEnd();
            List<TransactionResponse> fiatResponseList = JsonConvert.DeserializeObject<List<TransactionResponse>>(json);
            foreach (TransactionResponse response in fiatResponseList)
            {
                Log.Info("TransactionResponse hash: " + response.VerifyHash.hash);
            }

            return fiatResponseList;
        }

        public async Task<IReadOnlyList<StoreData>> GetProducts()
        {
            Log.Info($"{nameof(GetProducts)}");
            WebrequestCreationInfo webrequestCreationInfo = new WebrequestCreationInfo();
            webrequestCreationInfo.Url = PlasmaChainEndpointsContainer.FiatProductsURL;

            HttpResponseMessage httpResponseMessage = await WebRequestUtils.CreateAndSendWebrequest(webrequestCreationInfo);
            httpResponseMessage.ThrowOnError(webrequestCreationInfo);

            StoresDataList apiProductList = JsonConvert.DeserializeObject<StoresDataList>(httpResponseMessage.ReadToEnd());
            return apiProductList.products;
        }

        [Serializable]
        public class FiatClaimRequestBody
        {
            public int user_id;
            public int[] transaction_ids;
        }

        /// <summary>
        /// Removes the transactions from the database.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="transactionIds"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> Claim(int userId, IReadOnlyList<int> transactionIds)
        {
            Log.Info($"{nameof(Claim)}(userId = {userId}, transactionIds = {Utilites.FormatCallLogList(transactionIds)})");

            WebrequestCreationInfo webrequestCreationInfo = new WebrequestCreationInfo();
            webrequestCreationInfo.Method = WebRequestMethod.POST;
            webrequestCreationInfo.Url = PlasmaChainEndpointsContainer.FiatClaimURL;
            webrequestCreationInfo.ContentType = "application/json";

            FiatClaimRequestBody body = new FiatClaimRequestBody();
            body.user_id = userId;
            body.transaction_ids = transactionIds.ToArray();

            webrequestCreationInfo.Data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(body));
            webrequestCreationInfo.Headers.Add("accept", "application/json, text/plain, */*");
            webrequestCreationInfo.Headers.Add("authorization", "Bearer " + _backendDataControlMediator.UserDataModel.AccessToken);

            HttpResponseMessage httpResponseMessage = await WebRequestUtils.CreateAndSendWebrequest(webrequestCreationInfo);
            httpResponseMessage.ThrowOnError(webrequestCreationInfo);
            Log.Debug(httpResponseMessage.ReadToEnd());

            return true;
        }

        public class ValidationResponse
        {
            public string msg;
            public string transactionId;
            public bool success;
            public int txId;
        }

        public class TransactionResponse
        {
            public VerifyHash VerifyHash;
            public int UserId;
            public int Booster;
            public int Super;
            public int Air;
            public int Earth;
            public int Fire;
            public int Life;
            public int Toxic;
            public int Water;
            public int Small;
            public int Minion;
            public int Binance;
            public int TxID;
        }

        public class VerifyHash
        {
            public string hash;
            public string signature;
        }

        public class StoresDataList
        {
            public StoreData[] products;
        }

        public class StoreData
        {
            public string currency;
            public string store;
            public ProductData[] packs;
            public int unit_percent;
        }

        public class ProductData
        {
            public string uid;
            public string display_name;
            public string description;
            public string store_id;
            public int amount;
            public int price;
        }
    }
}
