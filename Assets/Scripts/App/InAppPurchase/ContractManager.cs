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

namespace Loom.ZombieBattleground.Iap
{
    public class ContractManager : IService
    {
        private static readonly ILog Log = Logging.GetLog(nameof(ContractManager));
        private static readonly ILog RpcLog = Logging.GetLog(nameof(ContractManager) + "Rpc");

        private BackendDataControlMediator _backendDataControlMediator;

        private ILoadObjectsManager _loadObjectsManager;

        private Dictionary<IapContractType, TextAsset> _abiDictionary;

        private byte[] UserPrivateKey => _backendDataControlMediator.UserDataModel.PrivateKey;

        public byte[] UserPublicKey => CryptoUtils.PublicKeyFromPrivateKey(UserPrivateKey);

        public Address UserPlasmachainAddress => Address.FromPublicKey(UserPublicKey, PlasmaChainEndpointsContainer.Chainid);

        public void Init()
        {
            _backendDataControlMediator = GameClient.Get<BackendDataControlMediator>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();

            InitAbiTextAssets();
        }

        public void Update() { }

        public void Dispose() { }

        public async Task<DAppChainClient> GetConnectedClient()
        {
            Log.Debug("Creating PlasmaChain client");

            DAppChainClient client = CreateClient();
            client.TxMiddleware = new TxMiddleware(new ITxMiddlewareHandler[]
            {
                new NonceTxMiddleware(UserPublicKey, client),
                new SignedTxMiddleware(UserPrivateKey)
            });

            if (client.ReadClient.ConnectionState != RpcConnectionState.Connected)
            {
                await client.ReadClient.ConnectAsync();
            }

            if (client.WriteClient.ConnectionState != RpcConnectionState.Connected)
            {
                await client.WriteClient.ConnectAsync();
            }

            return client;
        }

        public EvmContract GetContract(DAppChainClient client, IapContractType contractType)
        {
            if (client == null)
                throw new ArgumentNullException(nameof(client));

            return new EvmContract(
                client,
                Address.FromString(GetContractAddress(contractType), PlasmaChainEndpointsContainer.Chainid),
                UserPlasmachainAddress,
                _abiDictionary[contractType].text);
        }

        private DAppChainClient CreateClient()
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
                AutoReconnect = false,
                CallTimeout = Constants.PlasmachainCallTimeout,
                StaticCallTimeout = Constants.PlasmachainCallTimeout
            };

            return new LoggingDAppChainClient(writer, reader, clientConfiguration)
            {
                Logger = logger
            };
        }

        private void InitAbiTextAssets()
        {
            _abiDictionary = new Dictionary<IapContractType, TextAsset>
            {
                {
                    IapContractType.FiatPurchase, _loadObjectsManager.GetObjectByPath<TextAsset>("Data/abi/FiatPurchaseABI")
                },
                {
                    IapContractType.CardFaucet, _loadObjectsManager.GetObjectByPath<TextAsset>("Data/abi/CardFaucetABI")
                }
            };

            (IapContractType contractType, Enumerators.MarketplaceCardPackType cardPackType)[] contractTypeToCardPackType =
            {
                (IapContractType.BoosterPack, Enumerators.MarketplaceCardPackType.Booster),
                (IapContractType.SuperPack, Enumerators.MarketplaceCardPackType.Super),
                (IapContractType.AirPack, Enumerators.MarketplaceCardPackType.Air),
                (IapContractType.EarthPack, Enumerators.MarketplaceCardPackType.Earth),
                (IapContractType.FirePack, Enumerators.MarketplaceCardPackType.Fire),
                (IapContractType.LifePack, Enumerators.MarketplaceCardPackType.Life),
                (IapContractType.ToxicPack, Enumerators.MarketplaceCardPackType.Toxic),
                (IapContractType.WaterPack, Enumerators.MarketplaceCardPackType.Water),
                (IapContractType.SmallPack, Enumerators.MarketplaceCardPackType.Small),
                (IapContractType.MinionPack, Enumerators.MarketplaceCardPackType.Minion)
            };
            for (int i = 0; i < contractTypeToCardPackType.Length; ++i)
            {
                _abiDictionary.Add(
                    contractTypeToCardPackType[i].contractType,
                    _loadObjectsManager.GetObjectByPath<TextAsset>($"Data/abi/{contractTypeToCardPackType[i].cardPackType.ToString()}PackABI")
                );
            }
        }

        private string GetContractAddress(IapContractType contractType)
        {
            switch (contractType)
            {
                case IapContractType.FiatPurchase:
                    return PlasmaChainEndpointsContainer.ContractAddressFiatPurchase;
                case IapContractType.CardFaucet:
                    return PlasmaChainEndpointsContainer.ContractAddressCardFaucet;
                case IapContractType.BoosterPack:
                    return PlasmaChainEndpointsContainer.ContractAddressBoosterPack;
                case IapContractType.SuperPack:
                    return PlasmaChainEndpointsContainer.ContractAddressSuperPack;
                case IapContractType.AirPack:
                    return PlasmaChainEndpointsContainer.ContractAddressAirPack;
                case IapContractType.EarthPack:
                    return PlasmaChainEndpointsContainer.ContractAddressEarthPack;
                case IapContractType.FirePack:
                    return PlasmaChainEndpointsContainer.ContractAddressFirePack;
                case IapContractType.LifePack:
                    return PlasmaChainEndpointsContainer.ContractAddressLifePack;
                case IapContractType.ToxicPack:
                    return PlasmaChainEndpointsContainer.ContractAddressToxicPack;
                case IapContractType.WaterPack:
                    return PlasmaChainEndpointsContainer.ContractAddressWaterPack;
                case IapContractType.SmallPack:
                    return PlasmaChainEndpointsContainer.ContractAddressSmallPack;
                case IapContractType.MinionPack:
                    return PlasmaChainEndpointsContainer.ContractAddressMinionPack;
                default:
                    throw new ArgumentOutOfRangeException(nameof(contractType), contractType, null);
            }
        }

        private class LoggingDAppChainClient : DAppChainClient
        {
            public LoggingDAppChainClient(
                IRpcClient writeClient,
                IRpcClient readClient,
                DAppChainClientConfiguration configuration = null,
                IDAppChainClientCallExecutor callExecutor = null)
                : base(writeClient, readClient, configuration, callExecutor)
            {
            }

            public override void Dispose()
            {
                base.Dispose();
                Log.Debug("Disposing PlasmaChain client");
            }
        }
    }

}
