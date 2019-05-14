using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Numerics;
using System.Globalization;
using System.Threading.Tasks;
using Loom.Client;
using Loom.ZombieBattleground.Protobuf;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.Nethereum.ABI.FunctionEncoding.Attributes;

using System.Text;
using log4net;
using log4netUnitySupport;

namespace Loom.ZombieBattleground
{
    public class FiatPlasmaManager : IService
    {
        private static readonly ILog Log = Logging.GetLog(nameof(FiatPlasmaManager));
        private static readonly ILog RpcLog = Logging.GetLog(nameof(FiatPlasmaManager) + "Rpc");

        public event Action<ContractRequest> OnRequestPackSuccess;
        public event Action OnRequestPackFailed;

        private ContractRequest _cachePackRequestingParams;

        private const string RequestPackResponseEventName = "PurchaseSent";

        #region Contract
        private TextAsset _abiFiatPurchase;
        private EvmContract _fiatPurchaseContract;
        private bool IsConnected => _fiatPurchaseContract != null &&
            _fiatPurchaseContract.Client.ReadClient.ConnectionState == RpcConnectionState.Connected &&
            _fiatPurchaseContract.Client.WriteClient.ConnectionState == RpcConnectionState.Connected;
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
        
        private void RpcClientOnConnectionStateChanged(IRpcClient sender, RpcConnectionState state)
        {   
            UnitySynchronizationContext.Instance.Post(o =>
            {
                if (state != RpcConnectionState.Connected &&
                    state != RpcConnectionState.Connecting)
                {
                    string errorMsg =
                        "Your game client is now OFFLINE. Please check your internet connection and try again later.";
                    HandleNetworkExceptionFlow(new RpcClientException(errorMsg, 1, null));
                }
            }, null);
        }
        
        public async void HandleNetworkExceptionFlow(Exception exception)
        {
            if (!ScenePlaybackDetector.IsPlaying || UnitTestDetector.IsRunningUnitTests) {
                throw exception;
            }

            string message = "Handled network exception: ";
            if (exception is RpcClientException rpcClientException && rpcClientException.RpcClient is WebSocketRpcClient webSocketRpcClient)
            {
                message += $"[URL: {webSocketRpcClient.Url}] ";
            }
            message += exception;

            Log.Warn(message);
        }
        
        public async Task CallRequestPacksContract(FiatBackendManager.FiatTransactionResponse fiatResponse)
        {
            if(!IsConnected)
            {
                await GetFiatPurchaseContract();
            }            
            ContractRequest contractParams = ParseContractRequestFromFiatTransactionResponse(fiatResponse);                        
            await CallRequestPacksContract(_fiatPurchaseContract, contractParams);
        }

        private const string RequestPacksMethod = "requestPacks";
        
        private async Task CallRequestPacksContract(EvmContract contract, ContractRequest contractParams)
        {              
            if (contract == null)
            {
                throw new Exception("Contract not signed in!");
            }
            
            Log.Info( $"Calling smart contract [{RequestPacksMethod}]");            
            _cachePackRequestingParams = contractParams;
            
            try
            {
                Log.Info($"CallAsync method [{RequestPacksMethod}]");
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
            }
            catch (Exception e)
            {
                Log.Info($"smart contract [{RequestPacksMethod}] failed e:{e.Message}");
                throw new Exception($"smart contract [{RequestPacksMethod}] failed e:{e.Message}");
            }                                            
        }
        
        private void ContractEventReceived(object sender, EvmChainEventArgs e)
        {
            Log.InfoFormat("Received smart contract event: " + e.EventName);
            Log.InfoFormat("BlockHeight: " + e.BlockHeight);
            if (string.Equals(e.EventName, RequestPackResponseEventName))
            {
                OnRequestPackSuccess?.Invoke(_cachePackRequestingParams);
            }
            else 
            {
                OnRequestPackFailed?.Invoke();
            }
            _cachePackRequestingParams = null;
        }
        
        public async Task<EvmContract> GetFiatPurchaseContract()
        {
            if(_fiatPurchaseContract != null)
            {
                _fiatPurchaseContract.EventReceived -= ContractEventReceived;
                _fiatPurchaseContract.Client.ReadClient.ConnectionStateChanged -= RpcClientOnConnectionStateChanged;
                _fiatPurchaseContract.Client.WriteClient.ConnectionStateChanged -= RpcClientOnConnectionStateChanged;
                _fiatPurchaseContract?.Client?.Dispose();
            }
            
            _fiatPurchaseContract = await GetContract
            (
                PrivateKey,
                PublicKey,
                _abiFiatPurchase.ToString(),
                PlasmaChainEndpointsContainer.ContractAddressFiatPurchase
            );
            _fiatPurchaseContract.EventReceived += ContractEventReceived;
            _fiatPurchaseContract.Client.ReadClient.ConnectionStateChanged += RpcClientOnConnectionStateChanged;
            _fiatPurchaseContract.Client.WriteClient.ConnectionStateChanged += RpcClientOnConnectionStateChanged;
            return _fiatPurchaseContract;
        }

        private async Task<EvmContract> GetContract(byte[] privateKey, byte[] publicKey, string abi, string contractAddress)
        {
            ILogger logger = new UnityLoggerWrapper(RpcLog);

            IRpcClient writer = RpcClientFactory
                .Configure()
                .WithLogger(logger)
                .WithWebSocket(PlasmaChainEndpointsContainer.WebSocket)
                .Create();
    
            IRpcClient reader = RpcClientFactory
                .Configure()
                .WithLogger(logger)
                .WithWebSocket(PlasmaChainEndpointsContainer.QueryWS)
                .Create();
    
            DAppChainClientConfiguration clientConfiguration = new DAppChainClientConfiguration
            {
                CallTimeout = Constants.PlasmachainCallTimeout,
                StaticCallTimeout = Constants.PlasmachainCallTimeout
            };
            
            DAppChainClient client = new DAppChainClient
            (
                writer, 
                reader,
                clientConfiguration
            )
            { 
                Logger = Debug.unityLogger 
            };
    
            client.TxMiddleware = new TxMiddleware(new ITxMiddlewareHandler[]
            {
                new NonceTxMiddleware
                ( 
                    publicKey,
                    client
                ),
                new SignedTxMiddleware(privateKey)
            });
    
            client.Configuration.AutoReconnect = false;
            await client.ReadClient.ConnectAsync();
            await client.WriteClient.ConnectAsync();
    
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
            for (int i = 0; i < amountList.Count;++i)
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
