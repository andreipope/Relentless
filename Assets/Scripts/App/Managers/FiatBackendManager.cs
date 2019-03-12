using System;
using System.CodeDom;
using System.Text;
using System.Net;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Numerics;
using System.Globalization;
using log4net;
using Loom.ZombieBattleground;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Common;
using UnityEngine;
using UnityEngine.Networking;
using Object = UnityEngine.Object;
using Newtonsoft.Json;
using Plugins.AsyncAwaitUtil.Source;

namespace Loom.ZombieBattleground
{
    public class FiatBackendManager : IService
    {
        private static readonly ILog Log = Logging.GetLog(nameof(FiatBackendManager));

        private BackendDataControlMediator _backendDataControlMediator;
    
        public void Init()
        {
            _backendDataControlMediator = GameClient.Get<BackendDataControlMediator>();
        }
        
        public void Update()
        {
        }
        
        public void Dispose()
        {
        }
        
        public async Task<FiatValidationResponse> CallFiatValidationGoogle(string productId, string purchaseToken, string transactionId, string storeName)
        {  
            Log.Info($"{nameof(CallFiatValidationGoogle)}");
            
            WebrequestCreationInfo webrequestCreationInfo = new WebrequestCreationInfo();
            webrequestCreationInfo.Method = WebRequestMethod.POST;
            webrequestCreationInfo.Url = PlasmaChainEndpointsContainer.FiatValidationURL;
            webrequestCreationInfo.ContentType = "application/json";
            
            FiatValidationGoogleRequest fiatValidationGoogleRequest = new FiatValidationGoogleRequest();
            fiatValidationGoogleRequest.productId = productId;
            fiatValidationGoogleRequest.purchaseToken = purchaseToken;
            fiatValidationGoogleRequest.transactionId = transactionId;
            fiatValidationGoogleRequest.storeName = storeName;
            webrequestCreationInfo.Data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(fiatValidationGoogleRequest));
            webrequestCreationInfo.Headers.Add("authorization", "Bearer " + _backendDataControlMediator.UserDataModel.AccessToken);
            
            HttpResponseMessage httpResponseMessage =
                await WebRequestUtils.CreateAndSendWebrequest(webrequestCreationInfo);

            if (!httpResponseMessage.IsSuccessStatusCode)
                throw new Exception($"{nameof(CallFiatValidationGoogle)} failed with error code {httpResponseMessage.StatusCode}");            
            
            string json = httpResponseMessage.ReadToEnd();          
            Log.Info(json);
            FiatValidationResponse response = JsonConvert.DeserializeObject<FiatValidationResponse>(json);
            Log.Info($"Finish {nameof(CallFiatValidationGoogle)}");
            return response;
        }
        
        public async Task<FiatValidationResponse> CallFiatValidationApple(string productId, string transactionId, string receiptData, string storeName)
        {  
            Log.Info($"{nameof(CallFiatValidationApple)}");
            
            WebrequestCreationInfo webrequestCreationInfo = new WebrequestCreationInfo();
            webrequestCreationInfo.Method = WebRequestMethod.POST;
            webrequestCreationInfo.Url = PlasmaChainEndpointsContainer.FiatValidationURL;
            webrequestCreationInfo.ContentType = "application/json";
            
            FiatValidationAppleRequest fiatValidationAppleRequest = new FiatValidationAppleRequest();
            fiatValidationAppleRequest.productId = productId;
            fiatValidationAppleRequest.transactionId = transactionId;
            fiatValidationAppleRequest.receiptData = receiptData;
            fiatValidationAppleRequest.storeName = storeName;
            webrequestCreationInfo.Data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(fiatValidationAppleRequest));
            webrequestCreationInfo.Headers.Add("authorization", "Bearer " + _backendDataControlMediator.UserDataModel.AccessToken);
            
            HttpResponseMessage httpResponseMessage =
                await WebRequestUtils.CreateAndSendWebrequest(webrequestCreationInfo);

            if (!httpResponseMessage.IsSuccessStatusCode)
                throw new Exception($"{nameof(CallFiatValidationApple)} failed with error code {httpResponseMessage.StatusCode}");                        

            string json = httpResponseMessage.ReadToEnd();                
            Log.Info(json);
            FiatValidationResponse response = JsonConvert.DeserializeObject<FiatValidationResponse>(json);
            Log.Info($"Finish {nameof(CallFiatValidationApple)}");
            return response;            
        }
        
        public async Task<List<FiatTransactionResponse>> CallFiatTransaction()
        {    
            Log.Info($"{nameof(CallFiatTransaction)}");
                 
            WebrequestCreationInfo webrequestCreationInfo = new WebrequestCreationInfo();
            webrequestCreationInfo.Url = PlasmaChainEndpointsContainer.FiatTransactionURL;
            webrequestCreationInfo.Headers.Add("authorization", "Bearer " + _backendDataControlMediator.UserDataModel.AccessToken);

            HttpResponseMessage httpResponseMessage =
                await WebRequestUtils.CreateAndSendWebrequest(webrequestCreationInfo);

            if (!httpResponseMessage.IsSuccessStatusCode)
                throw new Exception($"{nameof(CallFiatTransaction)} failed with error code {httpResponseMessage.StatusCode}");
        
            string json = httpResponseMessage.ReadToEnd();   
            Log.Info(json);
            
            List<FiatTransactionResponse> fiatResponseList = JsonConvert.DeserializeObject<List<FiatTransactionResponse>>(json);           
    
            foreach(FiatTransactionResponse reponse in fiatResponseList)
            {
                Log.Info("FiatTransactionResponse hash: " + reponse.VerifyHash.hash);
            }

            return fiatResponseList;
        }
        
        [Serializable]
        public class FiatClaimRequestBody
        {
            public int user_id;
            public int[] transaction_ids;
        }

        public async Task<bool> CallFiatClaim(int userId, List<int> transactionIds)
        {
            Log.Info($"{nameof(CallFiatClaim)}");
            
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

            HttpResponseMessage httpResponseMessage =
                await WebRequestUtils.CreateAndSendWebrequest(webrequestCreationInfo);

            if (!httpResponseMessage.IsSuccessStatusCode)
                throw new Exception($"{nameof(CallFiatClaim)} failed with error code {httpResponseMessage.StatusCode}");
                     
            return true;
        }
        
        public class FiatValidationGoogleRequest
        {
            public string productId;
            public string purchaseToken;    
            public string transactionId;    
            public string storeName;
        }
        
        public class FiatValidationAppleRequest
        {
            public string productId;
            public string transactionId;    
            public string receiptData;    
            public string storeName;
        }

        public class FiatValidationResponse
        {
            public string msg;
            public string transactionId;
            public bool success;
            public int txId;
        }
        
        public class FiatTransactionResponse
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
    }
}
