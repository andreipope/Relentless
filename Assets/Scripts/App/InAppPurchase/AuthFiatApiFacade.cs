using System;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
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

        public async Task<List<TransactionReceipt>> ListPendingTransactions()
        {
            Log.Info($"{nameof(ListPendingTransactions)}");

            WebrequestCreationInfo webrequestCreationInfo = new WebrequestCreationInfo();
            webrequestCreationInfo.Url = PlasmaChainEndpointsContainer.FiatTransactionURL;
            webrequestCreationInfo.Headers.Add("authorization", "Bearer " + _backendDataControlMediator.UserDataModel.AccessToken);

            HttpResponseMessage httpResponseMessage = await WebRequestUtils.CreateAndSendWebrequest(webrequestCreationInfo);
            httpResponseMessage.ThrowOnError(webrequestCreationInfo);
            Log.Debug(httpResponseMessage.ReadToEnd());

            string json = httpResponseMessage.ReadToEnd();
            List<TransactionReceipt> fiatResponseList = JsonConvert.DeserializeObject<List<TransactionReceipt>>(json);
            foreach (TransactionReceipt response in fiatResponseList)
            {
                Log.Info("TransactionResponse hash: " + response.VerifyHash.Hash);
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
            return apiProductList.Products;
        }

        /// <summary>
        /// Removes the transactions from the database.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="transactionIds"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> Claim(BigInteger userId, BigInteger[] transactionIds)
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

        public class TransactionReceipt
        {
            public VerifySignResult VerifyHash { get; }

            public BigInteger UserId { get; }

            public uint Booster { get; }

            public uint Super { get; }

            public uint Air { get; }

            public uint Earth { get; }

            public uint Fire { get; }

            public uint Life { get; }

            public uint Toxic { get; }

            public uint Water { get; }

            public uint Small { get; }

            public uint Minion { get; }

            public uint Binance { get; }

            public BigInteger TxID { get; }

            public TransactionReceipt(
                VerifySignResult verifyHash,
                BigInteger userId,
                uint booster, 
                uint super,
                uint air,
                uint earth,
                uint fire,
                uint life,
                uint toxic,
                uint water,
                uint small,
                uint minion,
                uint binance,
                BigInteger txId)
            {
                VerifyHash = verifyHash;
                UserId = userId;
                Booster = booster;
                Super = super;
                Air = air;
                Earth = earth;
                Fire = fire;
                Life = life;
                Toxic = toxic;
                Water = water;
                Small = small;
                Minion = minion;
                Binance = binance;
                TxID = txId;
            }

            public class VerifySignResult
            {
                [JsonProperty("hash")]
                [JsonConverter(typeof(ByteArrayToHexConverter))]
                public byte[] Hash { get; }

                [JsonProperty("signature")]
                [JsonConverter(typeof(ByteArrayToHexConverter))]
                public byte[] Signature { get; }

                [JsonConstructor]
                public VerifySignResult(byte[] hash, byte[] signature)
                {
                    Hash = hash;
                    Signature = signature;
                }
            }
        }

        public class StoreData
        {
            [JsonProperty("currency")]
            public string Currency { get; }
            
            [JsonProperty("store")]
            public string Store { get; }
            
            [JsonProperty("packs")]
            public IReadOnlyList<ProductData> Packs { get; }
            
            [JsonProperty("unit_percent")]
            public int UnitPercent { get; }

            [JsonConstructor]
            public StoreData(string currency, string store, IReadOnlyList<ProductData> packs, int unitPercent)
            {
                Currency = currency;
                Store = store;
                Packs = packs;
                UnitPercent = unitPercent;
            }
        }

        public class ProductData
        {
            [JsonProperty("uid")]
            public string Uid { get; }
            
            [JsonProperty("display_name")]
            
            public string DisplayName { get; }
            
            [JsonProperty("description")]
            public string Description { get; }
            
            [JsonProperty("store_id")]
            public string StoreId { get; }
            
            [JsonProperty("amount")]
            public int Amount { get; }
            
            [JsonProperty("price")]
            public int Price { get; }

            [JsonConstructor]
            public ProductData(string uid, string displayName, string description, string storeId, int amount, int price)
            {
                Uid = uid;
                DisplayName = displayName;
                Description = description;
                StoreId = storeId;
                Amount = amount;
                Price = price;
            }
        }

        private class FiatClaimRequestBody
        {
            public BigInteger user_id;
            public BigInteger[] transaction_ids;
        }
        
        private class StoresDataList
        {
            [JsonProperty("products")]
            public IReadOnlyList<StoreData> Products { get; }

            public StoresDataList(IReadOnlyList<StoreData> products)
            {
                Products = products;
            }
        }
    }

    public class ValidationResponse
    {
        [JsonProperty("msg")]
        public string Message { get; }

        [JsonProperty("transactionId")]
        public string TransactionId { get; }

        [JsonProperty("success")]
        public bool Success { get; }

        [JsonProperty("txId")]
        public uint TxId { get; }

        public ValidationResponse(string message, string transactionId, bool success, uint txId)
        {
            Message = message;
            TransactionId = transactionId;
            Success = success;
            TxId = txId;
        }
    }
}
