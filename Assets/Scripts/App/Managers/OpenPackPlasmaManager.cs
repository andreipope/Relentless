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
          
        private const string _contractAddressCardFaucet = "0xd80d93138f1121e3b5153ffe3cb9ae7408a576bb";  
        private const string _contractAddressBoosterPack = "0x2fa54683d976c72806d2e54d1d61a476848e4da9"; 
           
        private EvmContract _cardFaucetContract;
        private EvmContract _boosterPackContract;
        
        private const string _chainid = "default";
        private const string _endPointWebSocket = "wss://test-z-us1.dappchains.com/websocket";
        private const string _endPointQueryWS = "wss://test-z-us1.dappchains.com/queryws";       
        #endregion
        
        #region Key
        private byte[] _privateKey
        {
            get
            {
                return _backendDataControlMediator.UserDataModel.PrivateKey;
            }
        }
        
        private byte[] _publicKey
        {
            get { return CryptoUtils.PublicKeyFromPrivateKey(_privateKey); }
        }
        #endregion
        
        private const int _boosterPackIndex = 0;

        private const int _cardsPerPack = 5;
        
        private BackendDataControlMediator _backendDataControlMediator;
        private ILoadObjectsManager _loadObjectsManager;  
        private IDataManager _dataManager;      
    
        public async void Init()
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
                _privateKey,
                _publicKey,
                _abiBoosterPack.ToString(),
                _contractAddressBoosterPack
            );
            int amount = await CallBalanceContract(_boosterPackContract);
            return amount;
        }

        public async Task<List<Card>> CallOpenPack(int packToOpenAmount)
        {
             _cardFaucetContract = await GetContract(
                _privateKey,
                _publicKey,
                _abiCardFaucet.ToString(),
                _contractAddressCardFaucet
            );
            _boosterPackContract = await GetContract(
                _privateKey,
                _publicKey,
                _abiBoosterPack.ToString(),
                _contractAddressBoosterPack
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
            var writer = RpcClientFactory
                .Configure()
                .WithLogger(Debug.unityLogger)
                .WithWebSocket(_endPointWebSocket)
                .Create();
    
            var reader = RpcClientFactory
                .Configure()
                .WithLogger(Debug.unityLogger)
                .WithWebSocket(_endPointQueryWS)
                .Create();
    
            var client = new DAppChainClient(writer, reader)
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
    
            var contractAddr = Address.FromString(contractAddress, _chainid);
            var callerAddr = Address.FromPublicKey(publicKey, _chainid);    
    
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
                 Address.FromPublicKey(_publicKey).ToString()
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
        
            await contract.CallAsync("approve", _contractAddressCardFaucet , amountToApprove);
        
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
                int mouId = (int)onOpenBoosterPackEvent.CardId / 10;
                Card card = _dataManager.CachedCardsLibraryData.GetCardFromMouId(mouId);
                Debug.Log($"<color=blue>MouId: {mouId}, card.MouldId:{card.MouldId}, card.Name:{card.Name}</color>");
                CardsReceived.Add(card);
            }
        }                   
         
    }

}