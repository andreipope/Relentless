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
        public const string FiatValidationURL = "https://stage-auth.loom.games/fiat/validate";
        public const string FiatTransactionURL = "https://stage-auth.loom.games/fiat/transaction";
        private BackendDataControlMediator _backendDataControlMediator;
        private FiatPlasmaManager _fiatPlasmaManager;
    
        public void Init()
        {
            _backendDataControlMediator = GameClient.Get<BackendDataControlMediator>();
            _fiatPlasmaManager = GameClient.Get<FiatPlasmaManager>();
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

            UnityWebRequest request = UnityWebRequest.Post(FiatValidationURL,form);
            AddAuthorizationHeader(request);
            request.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");
            request.downloadHandler = new DownloadHandlerBuffer();
            
            await request.SendWebRequest();            
            string json = request.downloadHandler.text;          
            Debug.Log(json);        
            FiatValidationResponse response = JsonConvert.DeserializeObject<FiatValidationResponse>(json);
            return response;
        }
        
        public async void CallFiatTransaction()
        {
            UnityWebRequest request = new UnityWebRequest(FiatTransactionURL);
            AddAuthorizationHeader(request);
            request.downloadHandler = new DownloadHandlerBuffer();
            
            await request.SendWebRequest();
            string json = request.downloadHandler.text;          
            Debug.Log(json);   
            
            List<FiatTransactionResponse> fiatResponseList = JsonConvert.DeserializeObject<List<FiatTransactionResponse>>(json);
            if (fiatResponseList.Count <= 0)
                return;
    
            foreach( var reponse in fiatResponseList)
            {
                CallRequestPacksContract(reponse);
            }            
        }
        
        private void CallRequestPacksContract(FiatTransactionResponse fiatResponse)
        {
            int UserId = fiatResponse.UserId;
            string hash = fiatResponse.VerifyHash.hash.Substring(2);
            int TxID = fiatResponse.TxID;
            string sig = fiatResponse.VerifyHash.signature;
            string r = Slice(sig, 2, 66);
            string s = "" + Slice(sig, 66, 130);
            string vStr = Slice(sig, 130, 132);
            BigInteger v = HexStringToBigInteger(vStr);
            
            List<int> amountList = new List<int>();
            amountList.Add( fiatResponse.Booster);
            amountList.Add( fiatResponse.Super);
            amountList.Add( fiatResponse.Air);
            amountList.Add( fiatResponse.Earth);
            amountList.Add( fiatResponse.Fire);
            amountList.Add( fiatResponse.Life);
            amountList.Add( fiatResponse.Toxic);
            amountList.Add( fiatResponse.Water);        
            amountList.Add( fiatResponse.Small);
            amountList.Add( fiatResponse.Minion);
    
            Debug.Log("UserId:" + UserId);
            Debug.Log("r:" + r);
            Debug.Log("s:" + s);
            Debug.Log("v:" + v);
            Debug.Log("hash:" + hash);
            Debug.Log("TxID:" + TxID);
    
            FiatPlasmaManager.ContractRequest contractParams = new FiatPlasmaManager.ContractRequest();
            contractParams.UserId = UserId;
            contractParams.r = HexStringToByte(r);
            contractParams.s = HexStringToByte(s);
            contractParams.v = (sbyte)v;
            contractParams.hash = HexStringToByte(hash);
            contractParams.amount = amountList.ToArray();
            contractParams.TxID = TxID;
            _fiatPlasmaManager.CallRequestPacksContract(contractParams);
    
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
            public string store_tx_id;
            public bool success;
            public int tx_id;
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

        #region Util
        private string Slice(string source, int start, int end)
        {
            if (end < 0) 
            {
                end = source.Length + end;
            }
            int len = end - start;
            return source.Substring(start, len);
        }
        
        private BigInteger HexStringToBigInteger(string hexString)
        {
            BigInteger b = BigInteger.Parse(hexString,NumberStyles.AllowHexSpecifier);
            return b;
        }
    
        private Byte[] HexStringToByte(string str)
        {
            string hex = str; 
            byte[] bytes = new byte[hex.Length / 2];
            
            for (int i = 0; i < hex.Length; i += 2)
                bytes[i/2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }
        #endregion
    }
}
