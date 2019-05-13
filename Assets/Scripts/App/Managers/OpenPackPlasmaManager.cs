using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Threading.Tasks;
using System.Numerics;
using Loom.Client;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.Nethereum.ABI.FunctionEncoding.Attributes;

using System.Text;
using log4net;
using log4netUnitySupport;

namespace Loom.ZombieBattleground
{

    public class OpenPackPlasmaManager : IService 
    {    
        private static readonly ILog Log = Logging.GetLog(nameof(OpenPackPlasmaManager));
        private static readonly ILog RpcLog = Logging.GetLog(nameof(OpenPackPlasmaManager) + "Rpc");
        
        public List<Card> CardsReceived { get; private set; }
        
        #region Contract
        private TextAsset _abiCardFaucet;
        private TextAsset[] _abiPacks;
        private EvmContract _cardFaucetContract;
        private List<EvmContract> _packContractList;
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

        private const int _cardsPerPack = 5;

        private const int _maxRequestRetryAttempt = 5;
        
        private BackendDataControlMediator _backendDataControlMediator;
        private ILoadObjectsManager _loadObjectsManager;  
        private IDataManager _dataManager;      
    
        public void Init()
        {           
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _backendDataControlMediator = GameClient.Get<BackendDataControlMediator>();
            _dataManager = GameClient.Get<IDataManager>();
            CardsReceived = new List<Card>();            
                      
            _abiCardFaucet = _loadObjectsManager.GetObjectByPath<TextAsset>("Data/abi/CardFaucetABI");
            Enumerators.MarketplaceCardPackType[] packTypes = (Enumerators.MarketplaceCardPackType[])Enum.GetValues(typeof(Enumerators.MarketplaceCardPackType));
            _abiPacks = new TextAsset[packTypes.Length];
            for (int i = 0;i < packTypes.Length;++i)
            {
                _abiPacks[i] = _loadObjectsManager.GetObjectByPath<TextAsset>($"Data/abi/{packTypes[i].ToString()}PackABI");
            }
            _packContractList = new List<EvmContract>();
        }
        
        public void Update()
        {
        }
        
        public void Dispose()
        {
        }
        
        private string GetContractAddress(int packTypeId)
        {
            switch( (Enumerators.MarketplaceCardPackType)packTypeId)
            {
                case Enumerators.MarketplaceCardPackType.Booster:
                    return PlasmaChainEndpointsContainer.ContractAddressBoosterPack;
                case Enumerators.MarketplaceCardPackType.Super:
                    return PlasmaChainEndpointsContainer.ContractAddressSuperPack;
                case Enumerators.MarketplaceCardPackType.Air:
                    return PlasmaChainEndpointsContainer.ContractAddressAirPack;
                case Enumerators.MarketplaceCardPackType.Earth:
                    return PlasmaChainEndpointsContainer.ContractAddressEarthPack;
                case Enumerators.MarketplaceCardPackType.Fire:
                    return PlasmaChainEndpointsContainer.ContractAddressFirePack;
                case Enumerators.MarketplaceCardPackType.Life:
                    return PlasmaChainEndpointsContainer.ContractAddressLifePack;
                case Enumerators.MarketplaceCardPackType.Toxic:
                    return PlasmaChainEndpointsContainer.ContractAddressToxicPack;
                case Enumerators.MarketplaceCardPackType.Water:
                    return PlasmaChainEndpointsContainer.ContractAddressWaterPack;
                case Enumerators.MarketplaceCardPackType.Small:
                    return PlasmaChainEndpointsContainer.ContractAddressSmallPack;
                case Enumerators.MarketplaceCardPackType.Minion:
                    return PlasmaChainEndpointsContainer.ContractAddressMinionPack;
                default:
                    break;
            }
            return "";
        }
        
        public async Task<int> CallPackBalanceContract(int packTypeId)
        {        
            Log.Info($"CallPackBalanceContract { ((Enumerators.MarketplaceCardPackType)packTypeId).ToString() }");            

            int amount;
            int count = 0;            
            while (true)
            {
                try
                {
                    amount = await CallBalanceContract
                    (
                        _packContractList[packTypeId]
                    );
                    break;
                }
                catch
                {
                    Log.Info($"smart contract [balanceOf] error or reverted");
                    await Task.Delay(TimeSpan.FromSeconds(1)); 
                }
                ++count;
                if(count > _maxRequestRetryAttempt)
                {
                    throw new Exception($"{nameof(CallPackBalanceContract)} with packTypeId {packTypeId}  failed after {count} attempts");
                }
                Log.Info($"Retry CallPackBalance: {count}");
            }
            return amount;            
        }

        public async Task<List<Card>> CallOpenPack(int packTypeId)
        {
            List<Card> resultList;
            EvmContract packContract = _packContractList[packTypeId];

            int expectCardReceiveAmount = _cardsPerPack;
            int count = 0;
            
            while (true)
            {
                resultList = new List<Card>();
                CardsReceived.Clear();

                try
                {
                    await CallBalanceContract(packContract);
                    await CallApproveContract(packContract);
                    await CallOpenPackContract(_cardFaucetContract, packTypeId);
                    await CallBalanceContract(packContract);

                    double timeOut = 29.99;
                    double interval = 1.0;
                    while (timeOut > 0.0 && CardsReceived.Count < expectCardReceiveAmount)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(interval));
                        timeOut -= interval;
                    }


                    foreach (Card card in CardsReceived)
                    {
                        resultList.Add(card);
                    }
                    break;
                }
                catch
                {
                    Log.Info($"smart contract [open{ (Enumerators.MarketplaceCardPackType)packTypeId }Packs] error or reverted");
                    await Task.Delay(TimeSpan.FromSeconds(1)); 
                }
                ++count;
                if(count > _maxRequestRetryAttempt)
                {
                    throw new Exception($"{nameof(CallOpenPack)} with packTypeId {packTypeId}  failed after {count} attempts");
                }
                Log.Info($"Retry CallOpenPack: {count}");
            }
            return resultList;                                   
        }
        
        public async Task CreateCardFaucetContract()
        {
            if(_cardFaucetContract != null)
            {
                _cardFaucetContract.EventReceived -= ContractEventReceived;
            }
            _cardFaucetContract = await GetContract
            (
                PrivateKey,
                PublicKey,
                _abiCardFaucet.ToString(),
                PlasmaChainEndpointsContainer.ContractAddressCardFaucet
            );
            _cardFaucetContract.EventReceived += ContractEventReceived;            
        }

        public async Task CreatePacksContract()
        {
            _packContractList.Clear();
            Enumerators.MarketplaceCardPackType[] packTypes = (Enumerators.MarketplaceCardPackType[])Enum.GetValues
            (
                typeof(Enumerators.MarketplaceCardPackType)
            );
            for(int i = 0; i < packTypes.Length; ++i)
            {
                _packContractList.Add
                (
                    await GetContract
                    (
                        PrivateKey,
                        PublicKey,
                        _abiPacks[i].ToString(),
                        GetContractAddress(i)
                    )
                );
            }
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

        private const string BalanceOfMethod = "balanceOf";
        
        public async Task<int> CallBalanceContract(EvmContract contract)
        {
            if (contract == null)
            {
                throw new Exception("Contract not signed in!");
            }
            
            Log.Info($"Calling smart contract [{BalanceOfMethod}]");
            int result;
            try
            {
                result = await contract.StaticCallSimpleTypeOutputAsync<int>(
                    BalanceOfMethod,
                     Address.FromPublicKey(PublicKey).ToString()
                );
                Log.Info("<color=green>" + "balanceOf RESULT: " + result + "</color>");
                Log.Info($"Smart contract method [{BalanceOfMethod}] finished executing.");
            }
            catch (Exception e)
            {
                throw new Exception($"Requesting [{BalanceOfMethod}] failed e:{e.Message}");
            }
            
            return result;
        }
        
        private const string ApproveMethod = "approve";
        
        public async Task CallApproveContract(EvmContract contract)
        {
            if (contract == null)
            {
                throw new Exception("Contract not signed in!");
            }
            Log.Info("Calling smart contract [approve]");
    
            int amountToApprove = 1;
        
            await contract.CallAsync(ApproveMethod, PlasmaChainEndpointsContainer.ContractAddressCardFaucet , amountToApprove);
        
            Log.Info($"Smart contract method [{ApproveMethod}] finished executing.");
        }
        
        private const string OpenPackMethod = "openBoosterPack";
        
        public async Task CallOpenPackContract(EvmContract contract, int packTypeId)
        {
            if (contract == null)
            {
                throw new Exception("Contract not signed in!");
            }
            Log.Info( $"Calling smart contract [{OpenPackMethod}] with packId: {packTypeId}");
        
            await contract.CallAsync(OpenPackMethod, packTypeId);
        
            Log.Info($"Smart contract method [{OpenPackMethod}] with packId: {packTypeId} finished executing.");
        }
        
        [Event("GeneratedCard")]
        public class OnOpenPackEvent
        {
            [Parameter("uint256", "cardId", 1)]
            public BigInteger CardId { get; set; }
            
            [Parameter("uint256", "boosterType", 2)]
            public BigInteger BoosterType { get; set; }
        }

        private void ContractEventReceived(object sender, EvmChainEventArgs e)
        {
            Log.InfoFormat("Received smart contract event: " + e.EventName);
            OnOpenPackEvent onOpenPackEvent = e.DecodeEventDto<OnOpenPackEvent>();
            Log.Info($"<color=red>CardId: {onOpenPackEvent.CardId}, BoosterType: {onOpenPackEvent.BoosterType}</color>");

            if ((int)onOpenPackEvent.CardId % 10 == 0)
            {
                MouldId mouldId = new MouldId((int)onOpenPackEvent.CardId / 10);
                Card card;
                try
                {
                    card = _dataManager.CachedCardsLibraryData.GetCardFromMouldId(mouldId);
                    Log.Info($"<color=blue>MouId: {mouldId}, card.MouldId:{card.MouldId}, card.Name:{card.Name}</color>");
                }
                catch
                {
                    Log.Info($"Not found card with MouldId:{mouldId}");
                    card = null;
                }
                CardsReceived.Add(card);
            }
        }                   
         
    }

}
