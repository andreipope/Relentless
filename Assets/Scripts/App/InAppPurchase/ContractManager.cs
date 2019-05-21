using System;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using Loom.Client;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.BackendCommunication;
using log4net;
using log4netUnitySupport;
using Loom.ZombieBattleground.Iap;

namespace Loom.ZombieBattleground
{
    public class ContractManager : IService
    {
        private static readonly ILog Log = Logging.GetLog(nameof(ContractManager));
        private static readonly ILog RpcLog = Logging.GetLog(nameof(ContractManager) + "Rpc");
        
        private BackendDataControlMediator _backendDataControlMediator;
        
        private ILoadObjectsManager _loadObjectsManager;
        
        private Dictionary<IapContractType, TextAsset> _abiDictionary;
        
        private Dictionary<IapContractType, string> _contractAddressDictionary;

        private Dictionary<IapContractType, EvmContract> _contractDictionary;
        
        public event Action<IapContractType, EvmContract, EvmContract> OnContractCreated;

        private byte[] PrivateKey
        {
            get
            {
                return _backendDataControlMediator.UserDataModel.PrivateKey;
            }
        }
        
        public byte[] PublicKey
        {
            get 
            { 
                return CryptoUtils.PublicKeyFromPrivateKey(PrivateKey); 
            }
        }
        
        public void Init()
        {
            _backendDataControlMediator = GameClient.Get<BackendDataControlMediator>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();

            _contractDictionary = new Dictionary<IapContractType, EvmContract>();
            
            InitContractAddress();
            InitABITextAssets();            
        }
        
        public void Update()
        {
        }
        
        public void Dispose()
        {
            if (_contractDictionary != null)
            {
                foreach (KeyValuePair<IapContractType, EvmContract> kvp in _contractDictionary)
                {                    
                    kvp.Value?.Client?.Dispose();
                }
                _contractDictionary.Clear();
            }
        }
        
        public bool IsContractConnected(EvmContract contract)
        {
            return contract != null &&
                contract.Client.ReadClient.ConnectionState == RpcConnectionState.Connected &&
                contract.Client.WriteClient.ConnectionState == RpcConnectionState.Connected;
        }
        
        public async Task<EvmContract> GetContract(IapContractType contractType)
        {
            EvmContract newContract,
                        oldContract;
            
            if(_contractDictionary.ContainsKey(contractType))
            {
                if 
                (
                    IsContractConnected
                    (
                        _contractDictionary[contractType]
                    )
                )
                {
                    return _contractDictionary[contractType];
                }
                else
                {
                    oldContract = _contractDictionary[contractType];
                    _contractDictionary[contractType]?.Client.Dispose();
                    _contractDictionary.Remove(contractType);
                }
            }
            else
            {
                oldContract = null;
            }

            newContract = await CreateContract
            (
                PrivateKey,
                PublicKey,
                _abiDictionary[contractType].ToString(),
                _contractAddressDictionary[contractType]
            );
            _contractDictionary.Add
            (
                contractType,
                newContract
            );
            
            OnContractCreated?.Invoke(contractType, oldContract, newContract);

            return newContract;
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
        
        private async Task<EvmContract> CreateContract(byte[] privateKey, byte[] publicKey, string abi, string contractAddress)
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
        
        private void InitABITextAssets()
        {
            _abiDictionary = new Dictionary<IapContractType, TextAsset>();
            _abiDictionary.Add
            (
                IapContractType.FiatPurchase,
                _loadObjectsManager.GetObjectByPath<TextAsset>
                (
                    "Data/abi/FiatPurchaseABI"
                )
            );
            _abiDictionary.Add
            (
                IapContractType.CardFaucet,
                _loadObjectsManager.GetObjectByPath<TextAsset>
                (
                    "Data/abi/CardFaucetABI"
                )
            );
            Enumerators.MarketplaceCardPackType[] packTypes = (Enumerators.MarketplaceCardPackType[])Enum.GetValues(typeof(Enumerators.MarketplaceCardPackType));
            List<IapContractType> packContractTypes = new List<IapContractType>
            {
                IapContractType.BoosterPack,
                IapContractType.SuperPack,
                IapContractType.AirPack,
                IapContractType.EarthPack,
                IapContractType.FirePack,
                IapContractType.LifePack,
                IapContractType.ToxicPack,
                IapContractType.WaterPack,
                IapContractType.SmallPack,
                IapContractType.MinionPack
            };
            for (int i = 0;i < packTypes.Length;++i)
            {
                _abiDictionary.Add
                (
                    packContractTypes[i],
                    _loadObjectsManager.GetObjectByPath<TextAsset>
                    (
                        $"Data/abi/{packTypes[i].ToString()}PackABI"
                    )
                );
            }
        }
        
        private void InitContractAddress()
        {
            _contractAddressDictionary = new Dictionary<IapContractType, string>();

            _contractAddressDictionary.Add
            (
                IapContractType.FiatPurchase,
                PlasmaChainEndpointsContainer.ContractAddressFiatPurchase
            );
            _contractAddressDictionary.Add
            (
                IapContractType.CardFaucet,
                PlasmaChainEndpointsContainer.ContractAddressCardFaucet
            );
            
            _contractAddressDictionary.Add
            (
                IapContractType.BoosterPack,
                PlasmaChainEndpointsContainer.ContractAddressBoosterPack
            );
            _contractAddressDictionary.Add
            (
                IapContractType.SuperPack,
                PlasmaChainEndpointsContainer.ContractAddressSuperPack
            );
            _contractAddressDictionary.Add
            (
                IapContractType.AirPack,
                PlasmaChainEndpointsContainer.ContractAddressAirPack
            );
            _contractAddressDictionary.Add
            (
                IapContractType.EarthPack,
                PlasmaChainEndpointsContainer.ContractAddressEarthPack
            );
            _contractAddressDictionary.Add
            (
                IapContractType.FirePack,
                PlasmaChainEndpointsContainer.ContractAddressFirePack
            );
            _contractAddressDictionary.Add
            (
                IapContractType.LifePack,
                PlasmaChainEndpointsContainer.ContractAddressLifePack
            );
            _contractAddressDictionary.Add
            (
                IapContractType.ToxicPack,
                PlasmaChainEndpointsContainer.ContractAddressToxicPack
            );
            _contractAddressDictionary.Add
            (
                IapContractType.WaterPack,
                PlasmaChainEndpointsContainer.ContractAddressWaterPack
            );
            _contractAddressDictionary.Add
            (
                IapContractType.SmallPack,
                PlasmaChainEndpointsContainer.ContractAddressSmallPack
            );
            _contractAddressDictionary.Add
            (
                IapContractType.MinionPack,
                PlasmaChainEndpointsContainer.ContractAddressMinionPack
            );
        }
    }
}
