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
        
        private const int _cardsPerPack = 5;

        private const int _maxRequestRetryAttempt = 5;
        
        private BackendDataControlMediator _backendDataControlMediator;
        private ILoadObjectsManager _loadObjectsManager;  
        private IDataManager _dataManager;      
        private ContractManager _contractManager;
        
        public event Action OnConnectionStateNotConnect;
    
        public void Init()
        {           
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _backendDataControlMediator = GameClient.Get<BackendDataControlMediator>();
            _dataManager = GameClient.Get<IDataManager>();
            _contractManager = GameClient.Get<ContractManager>();   
            
            CardsReceived = new List<Card>();
            
            _contractManager.OnContractCreated += 
            (
                Enumerators.ContractType contractType, 
                EvmContract oldContract,
                EvmContract newContract
            ) => 
            {
                if (contractType != Enumerators.ContractType.FiatPurchase)
                {
                    newContract.Client.ReadClient.ConnectionStateChanged += RpcClientOnConnectionStateChanged;
                    newContract.Client.ReadClient.ConnectionStateChanged += RpcClientOnConnectionStateChanged;
                }
                
                if(contractType == Enumerators.ContractType.CardFaucet)
                {
                    if (oldContract != null)
                    {
                        oldContract.EventReceived -= ContractEventReceived;
                    }
                    newContract.EventReceived += ContractEventReceived;
                }
            };    
        }
        
        public void Update()
        {
        }
        
        public void Dispose()
        {
        }
        
        private void RpcClientOnConnectionStateChanged(IRpcClient sender, RpcConnectionState state)
        {
            if
            (
                state != RpcConnectionState.Connected &&
                state != RpcConnectionState.Connecting
            )
            {
                OnConnectionStateNotConnect?.Invoke();
            }
            
            UnitySynchronizationContext.Instance.Post(o =>
            {
                if (state != RpcConnectionState.Connected &&
                    state != RpcConnectionState.Connecting)
                {
                    string errorMsg =
                        "Your game client is now OFFLINE. Please check your internet connection and try again later.";
                    _contractManager.HandleNetworkExceptionFlow(new RpcClientException(errorMsg, 1, null));
                }
            }, null);
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
                        await _contractManager.GetContract
                        (
                            GetPackContractTypeFromId(packTypeId)
                        ) 
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

            int expectCardReceiveAmount = _cardsPerPack;
            int count = 0;
            
            while (true)
            {
                resultList = new List<Card>();
                CardsReceived.Clear();

                try
                {     
                    await CallApproveContract
                    (
                        await _contractManager.GetContract
                        (
                            GetPackContractTypeFromId(packTypeId)
                        )
                    );
                    await CallOpenPackContract(
                        await _contractManager.GetContract
                        (
                            Enumerators.ContractType.CardFaucet
                        ), 
                        packTypeId
                    );                    

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
                catch (Exception e)
                {
                    Log.Info($"[open{ (Enumerators.MarketplaceCardPackType)packTypeId }Packs] failed. e:{e.Message}");
                    await Task.Delay(TimeSpan.FromSeconds(1)); 
                }
                ++count;
                if(count >= _maxRequestRetryAttempt)
                {
                    throw new Exception($"{nameof(CallOpenPack)} with packTypeId {packTypeId}  failed after {count} attempts");
                }
                Log.Info($"Retry CallOpenPack: {count}");
            }
            return resultList;                                   
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
                result = await contract.StaticCallSimpleTypeOutputAsync<int>
                (
                    BalanceOfMethod,
                    Address.FromPublicKey(_contractManager.PublicKey).ToString()
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
        
        private Enumerators.ContractType GetPackContractTypeFromId(int packId)
        {
            switch((Enumerators.MarketplaceCardPackType)packId)
            {
                case Enumerators.MarketplaceCardPackType.Booster:
                    return Enumerators.ContractType.BoosterPack;
                    
                case Enumerators.MarketplaceCardPackType.Super:
                    return Enumerators.ContractType.SuperPack;
                    
                case Enumerators.MarketplaceCardPackType.Air:
                    return Enumerators.ContractType.AirPack;
                    
                case Enumerators.MarketplaceCardPackType.Earth:
                    return Enumerators.ContractType.EarthPack;
                    
                case Enumerators.MarketplaceCardPackType.Fire:
                    return Enumerators.ContractType.FirePack;
                    
                case Enumerators.MarketplaceCardPackType.Life:
                    return Enumerators.ContractType.LifePack;
                    
                case Enumerators.MarketplaceCardPackType.Toxic:
                    return Enumerators.ContractType.ToxicPack;
                    
                case Enumerators.MarketplaceCardPackType.Water:
                    return Enumerators.ContractType.WaterPack;
                    
                case Enumerators.MarketplaceCardPackType.Small:
                    return Enumerators.ContractType.SmallPack;
                    
                case Enumerators.MarketplaceCardPackType.Minion:
                    return Enumerators.ContractType.MinionPack;
                    
                default:
                    throw new Exception($"Not found ContractType from pack id {packId}");
            }
        }
    }

}
