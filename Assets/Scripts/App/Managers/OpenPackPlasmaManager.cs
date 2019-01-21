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

namespace Loom.ZombieBattleground
{

    public class OpenPackPlasmaManager : IService 
    {    
        public List<Card> CardsReceived { get; private set; }
        
        #region Contract
        private TextAsset _abiCardFaucet;
        private TextAsset _abiBoosterPack;
        
        private EvmContract _cardFaucetContract;
        private EvmContract _boosterPackContract;    
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
            get { return CryptoUtils.PublicKeyFromPrivateKey(PrivateKey); }
        }
        #endregion
        
        private const int _boosterPackIndex = 0;

        private const int _cardsPerPack = 5;
        
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
            _abiBoosterPack = _loadObjectsManager.GetObjectByPath<TextAsset>("Data/abi/BoosterPackABI");            
        }
        
        public void Update()
        {
        }
        
        public void Dispose()
        {
        }
        
        public async Task<int> CallPackBalanceContract()
        {
             _boosterPackContract = await GetContract(
                PrivateKey,
                PublicKey,
                _abiBoosterPack.ToString(),
                PlasmaChainEndpointsContainer.ContractAddressBoosterPack
            );
            int amount = await CallBalanceContract(_boosterPackContract);
            return amount;
        }

        public async Task<List<Card>> CallOpenPack(int packToOpenAmount)
        {
             _cardFaucetContract = await GetContract(
                PrivateKey,
                PublicKey,
                _abiCardFaucet.ToString(),
                PlasmaChainEndpointsContainer.ContractAddressCardFaucet
            );
            _boosterPackContract = await GetContract(
                PrivateKey,
                PublicKey,
                _abiBoosterPack.ToString(),
                PlasmaChainEndpointsContainer.ContractAddressBoosterPack
            );

            _cardFaucetContract.EventReceived += ContractEventReceived;
        
            CardsReceived.Clear();
            int expectCardReceiveAmount = packToOpenAmount * _cardsPerPack;

            for (int i = 0; i < packToOpenAmount; ++i)
            {
                await CallBalanceContract(_boosterPackContract);
                await CallApproveContract(_boosterPackContract);
                await CallOpenPackContract(_cardFaucetContract, _boosterPackIndex);
                await CallBalanceContract(_boosterPackContract);
            }

            double timeOut = 4.99;
            double interval = 1.0;
            while( timeOut > 0.0 && CardsReceived.Count < expectCardReceiveAmount )
            {                
                await Task.Delay(TimeSpan.FromSeconds(interval));
                timeOut -= interval;
            }

            return CardsReceived;                                   
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
        
        public async Task<int> CallBalanceContract(EvmContract contract)
        {
            if (contract == null)
            {
                throw new Exception("Contract not signed in!");
            }
            
            Debug.Log("Calling smart contract [balanceOf]");
            int result = await contract.StaticCallSimpleTypeOutputAsync<int>(
                "balanceOf",
                 Address.FromPublicKey(PublicKey).ToString()
            );        
            Debug.Log("<color=green>" + "balanceOf RESULT: " + result + "</color>");
        
            Debug.Log("Smart contract method [balanceOf] finished executing.");
            return result;
        }
        
        public async Task CallApproveContract(EvmContract contract)
        {
            if (contract == null)
            {
                throw new Exception("Contract not signed in!");
            }
            Debug.Log("Calling smart contract [approve]");
    
            int amountToApprove = 1;
        
            await contract.CallAsync("approve", PlasmaChainEndpointsContainer.ContractAddressCardFaucet , amountToApprove);
        
            Debug.Log("Smart contract method [approve] finished executing.");
        }
        
        public async Task CallOpenPackContract(EvmContract contract, int packIndex)
        {
            if (contract == null)
            {
                throw new Exception("Contract not signed in!");
            }
            Debug.Log( $"Calling smart contract [openBoosterPack] {packIndex}");
        
            await contract.CallAsync("openBoosterPack", packIndex);
        
            Debug.Log($"Smart contract method [openBoosterPack] {packIndex} finished executing.");
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
            Debug.LogFormat("Received smart contract event: " + e.EventName);
            OnOpenPackEvent onOpenBoosterPackEvent = e.DecodeEventDto<OnOpenPackEvent>();
            Debug.Log($"<color=red>CardId: {onOpenBoosterPackEvent.CardId}, BoosterType: {onOpenBoosterPackEvent.BoosterType}</color>");

            if ((int)onOpenBoosterPackEvent.CardId % 10 == 0)
            {
                int mouldId = (int)onOpenBoosterPackEvent.CardId / 10;
                Card card = _dataManager.CachedCardsLibraryData.GetCardFromMouldId(mouldId);
                Debug.Log($"<color=blue>MouId: {mouldId}, card.MouldId:{card.MouldId}, card.Name:{card.Name}</color>");
                CardsReceived.Add(card);
            }
        }                   
         
    }

}