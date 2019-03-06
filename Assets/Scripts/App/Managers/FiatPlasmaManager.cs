using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Numerics;
using System.Globalization;
using System.Threading.Tasks;
using Loom.Client;
using Loom.ZombieBattleground.Protobuf;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.Nethereum.ABI.FunctionEncoding.Attributes;

using System.Text;
using log4net;

namespace Loom.ZombieBattleground
{
    public class FiatPlasmaManager : IService
    {
        private static readonly ILog Log = Logging.GetLog(nameof(FiatPlasmaManager));

        #region Contract
        private TextAsset _abiFiatPurchase;
        private EvmContract _fiatPurchaseContract;
        #endregion
        
        #region Key
        private byte[] PrivateKey
        {
            get
            {
                return _backendDataControlMediator.UserDataModel.PrivateKey;
            }
        }
        
        private byte[] PublicKey
        {
            get 
            { 
                return CryptoUtils.PublicKeyFromPrivateKey(PrivateKey); 
            }
        }
        #endregion
        
        private BackendDataControlMediator _backendDataControlMediator;
        
        private ILoadObjectsManager _loadObjectsManager;   
        
        private bool _isEventTriggered = false;      
        
        private string _eventResponse;           
        
        private const int MaxRequestRetryAttempt = 5;
    
        public void Init()
        {           
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _backendDataControlMediator = GameClient.Get<BackendDataControlMediator>();
            
            _abiFiatPurchase = _loadObjectsManager.GetObjectByPath<TextAsset>("Data/abi/FiatPurchaseABI");            
        }
        
        public void Update()
        {
        }
        
        public void Dispose()
        {
        }
        
         public async Task<string> CallRequestPacksContract(FiatBackendManager.FiatTransactionResponse fiatResponse)
        {
            ContractRequest contractParams = ParseContractRequestFromFiatTransactionResponse(fiatResponse);
            _fiatPurchaseContract = await GetContract
            (
                PrivateKey,
                PublicKey,
                _abiFiatPurchase.ToString(),
                PlasmaChainEndpointsContainer.ContractAddressFiatPurchase
            );
            _fiatPurchaseContract.EventReceived += ContractEventReceived;
            string responseEvent = "";
            responseEvent = await CallRequestPacksContract(_fiatPurchaseContract, contractParams);             
            return responseEvent;
        }

        private const string RequestPacksMethod = "requestPacks";
        
        private async Task<string> CallRequestPacksContract(EvmContract contract, ContractRequest contractParams)
        {              
            if (contract == null)
            {
                throw new Exception("Contract not signed in!");
            }
            Log.Info( $"Calling smart contract [{RequestPacksMethod}]");
            
            int count = 0;
            while (true)
            {
                _isEventTriggered = false;
                _eventResponse = ""; 
                try
                {

                    await contract.CallAsync
                    (
                        RequestPacksMethod,
                        contractParams.UserId,
                        contractParams.r,
                        contractParams.s,
                        contractParams.v,
                        contractParams.hash,
                        contractParams.amount,
                        contractParams.TxID
                    );
                    Log.Info($"Smart contract method [{RequestPacksMethod}] finished executing.");
                    for (int i = 0; i < 10; ++i)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(1));
                        Log.Info($"<color=green>Wait {i + 1} sec</color>");
                        if (_isEventTriggered)
                        {
                            return _eventResponse;
                        }
                    }
                    Log.Info($"Wait for [requestPacks] response too long");
                }
                catch
                {
                    Log.Info($"smart contract [{RequestPacksMethod}] error or reverted");
                    await Task.Delay(TimeSpan.FromSeconds(1)); 
                }
                ++count;
                Log.Info($"Retry {RequestPacksMethod}: {count}");
                if(count > MaxRequestRetryAttempt)
                {
                    throw new Exception($"{nameof(CallRequestPacksContract)} failed after {count} attempts");
                }
            }
            return "";
        }
        
        private void ContractEventReceived(object sender, EvmChainEventArgs e)
        {
            Log.InfoFormat("Received smart contract event: " + e.EventName);
            Log.InfoFormat("BlockHeight: " + e.BlockHeight);
            _isEventTriggered = true;
            _eventResponse = e.EventName;
        }
        
        private async Task<EvmContract> GetContract(byte[] privateKey, byte[] publicKey, string abi, string contractAddress)
        {        
            IRpcClient writer = RpcClientFactory
                .Configure()
                .WithLogger(Debug.unityLogger)
                .WithWebSocket(PlasmaChainEndpointsContainer.WebSocket)
                .Create();
    
            IRpcClient reader = RpcClientFactory
                .Configure()
                .WithLogger(Debug.unityLogger)
                .WithWebSocket(PlasmaChainEndpointsContainer.QueryWS)
                .Create();
    
            DAppChainClient client = new DAppChainClient(writer, reader)
                { Logger = Debug.unityLogger };
    
            client.TxMiddleware = new TxMiddleware(new ITxMiddlewareHandler[]
            {
                new NonceTxMiddleware
                ( 
                    publicKey,
                    client
                ),
                new SignedTxMiddleware(privateKey)
            });
    
            Address contractAddr = Address.FromString(contractAddress, PlasmaChainEndpointsContainer.Chainid);
            Address callerAddr = Address.FromPublicKey(publicKey, PlasmaChainEndpointsContainer.Chainid);    
    
            return new EvmContract(client, contractAddr, callerAddr, abi);
        }
        
        public class ContractRequest
        {
            public int UserId;
            public byte[] r;
            public byte[] s;
            public sbyte v;
            public byte[] hash;
            public int []amount;
            public int TxID;
            
        }    
        
#region Util
        private ContractRequest ParseContractRequestFromFiatTransactionResponse(FiatBackendManager.FiatTransactionResponse fiatResponse)
        {
            string log = "ContractRequest Params: \n";
            int UserId = fiatResponse.UserId;
            string hash = fiatResponse.VerifyHash.hash;
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
            amountList.Add( fiatResponse.Binance);

            log += "UserId: " + UserId + "\n";
            log += "r: " + r + "\n";
            log += "s: " + s + "\n";
            log += "v: " + v + "\n";
            log += "hash: " + hash + "\n";
            string amountStr = "[";
            for(int i=0; i<amountList.Count;++i)
            {
                amountStr += amountList[i] + " ";
            }
            amountStr += "]";
            log += "amount: " + amountStr + "\n";
            log += "TxID: " + TxID + "\n";
            Log.Info(log);
    
            ContractRequest contractParams = new ContractRequest();
            contractParams.UserId = UserId;
            contractParams.r = CryptoUtils.HexStringToBytes(r);
            contractParams.s = CryptoUtils.HexStringToBytes(s);
            contractParams.v = (sbyte)v;
            contractParams.hash = CryptoUtils.HexStringToBytes(hash);
            contractParams.amount = amountList.ToArray();
            contractParams.TxID = TxID;
            return contractParams;
        }
        public string Slice(string source, int start, int end)
        {
            if (end < 0) 
            {
                end = source.Length + end;
            }
            int len = end - start;
            return source.Substring(start, len);
        }
        
        public BigInteger HexStringToBigInteger(string hexString)
        {
            BigInteger b = BigInteger.Parse(hexString,NumberStyles.AllowHexSpecifier);
            return b;
        }
#endregion     
    }
}
