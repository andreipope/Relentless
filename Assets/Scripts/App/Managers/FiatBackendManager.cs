using System;
using System.CodeDom;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Numerics;
using System.Globalization;
using Loom.ZombieBattleground;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Common;
using UnityEngine;
using UnityEngine.Networking;
using Object = UnityEngine.Object;
using Newtonsoft.Json;

namespace Loom.ZombieBattleground
{
    public class FiatBackendManager : IService
    {
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
        
        public async Task<FiatValidationResponse> CallFiatValidation(string productId, string purchaseToken, string storeTxId, string storeName)
        {  
            WWWForm form = new WWWForm();
            form.AddField("productId", productId);       
            form.AddField("purchaseToken", purchaseToken);       
            form.AddField("transactionId", storeTxId);       
            form.AddField("storeName", storeName);

            UnityWebRequest request = UnityWebRequest.Post(PlasmaChainEndpointsContainer.FiatValidationURL,form);
            AddAuthorizationHeader(request);
            request.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");
            request.downloadHandler = new DownloadHandlerBuffer();
            
            await request.SendWebRequest();            
            string json = request.downloadHandler.text;          
            Debug.Log(json);        
            FiatValidationResponse response = JsonConvert.DeserializeObject<FiatValidationResponse>(json);
            return response;
        }
        
        public async Task<List<FiatTransactionResponse>> CallFiatTransaction()
        {
            UnityWebRequest request = new UnityWebRequest(PlasmaChainEndpointsContainer.FiatTransactionURL);
            AddAuthorizationHeader(request);
            request.downloadHandler = new DownloadHandlerBuffer();
            
            await request.SendWebRequest();
            string json = request.downloadHandler.text;          
            Debug.Log(json);   
            
            List<FiatTransactionResponse> fiatResponseList = JsonConvert.DeserializeObject<List<FiatTransactionResponse>>(json);           
    
            foreach(FiatTransactionResponse reponse in fiatResponseList)
            {
                Debug.Log("FiatTransactionResponse hash: " + reponse.VerifyHash.hash);
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
            Debug.Log("CallFiatClaim");

            UnityWebRequest request = new UnityWebRequest
            (
                PlasmaChainEndpointsContainer.FiatClaimURL,
                "POST"
            );
            FiatClaimRequestBody body = new FiatClaimRequestBody();
            body.user_id = userId;
            body.transaction_ids = transactionIds.ToArray();
            string jsonString = JsonUtility.ToJson(body);
            byte[] bodyRaw = Encoding.UTF8.GetBytes
            (
                jsonString                
            );
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            
            AddAuthorizationHeader(request);
            request.SetRequestHeader("Content-Type", "application/json");
            
            await request.SendWebRequest();
            Debug.Log("FiatClaim Response Code: " + request.responseCode);           
            return (request.responseCode == 200);
        }

        private void AddAuthorizationHeader(UnityWebRequest request)
        {
            request.SetRequestHeader
            (
                "Authorization",
                "Bearer " + _backendDataControlMediator.UserDataModel.AccessToken
            );
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
            public int Air;
            public int Earth;
            public int Fire;
            public int Life;
            public int Toxic;
            public int Water;
            public int Super;
            public int Small;
            public int Minion;
            public int TxID;
        }
        
        public class VerifyHash
        {
            public string hash;
            public string signature;
        }
    }
}
