using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Threading.Tasks;
using Loom.Client;
using Loom.ZombieBattleground.Protobuf;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.Nethereum.ABI.FunctionEncoding.Attributes;

using System.Text;

namespace Loom.ZombieBattleground
{
    public class FiatPlasmaManager : IService
    {
        #region Contract
        private TextAsset _abiFiatPurchase;
        private const string _contractAddressFiatPurchase = "0x603d5461471f0aa882546a6d38ef70ce522f5923";    
        private EvmContract _fiatPurchaseContract;
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
    
        #region Endpoint
        private const string _chainid = "default";
        private const string _endPointWebSocket = "wss://test-z-us1.dappchains.com/websocket";
        private const string _endPointQueryWS = "wss://test-z-us1.dappchains.com/queryws";
        #endregion
        
        private BackendDataControlMediator _backendDataControlMediator;
        private ILoadObjectsManager _loadObjectsManager;        
    
        public async void Init()
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
        
        public async void CallRequestPacksContract(ContractRequest contractParams)
        {
            _fiatPurchaseContract = await GetContract
            (
                _privateKey,
                _publicKey,
                _abiFiatPurchase.ToString(),
                _contractAddressFiatPurchase
            );
            _fiatPurchaseContract.EventReceived += ContractEventReceived; 
            CallRequestPacksContract(_fiatPurchaseContract, contractParams);
        }
        
        private async void CallRequestPacksContract(EvmContract contract, ContractRequest contractParams)
        {    
            string methodName = "requestPacks";
        
            if (contract == null)
            {
                throw new Exception("Contract not signed in!");
            }
            Debug.Log( $"Calling smart contract [requestPacks]");
            
            await contract.CallAsync
            (
                methodName, 
                contractParams.UserId,
                contractParams.r,
                contractParams.s,
                contractParams.v,
                contractParams.hash,
                contractParams.amount,
                contractParams.TxID
            );
        
            Debug.Log($"Smart contract method [requestPacks] finished executing.");
        }
        
        private void ContractEventReceived(object sender, EvmChainEventArgs e)
        {
            Debug.LogFormat("Received smart contract event: " + e.EventName);
            Debug.LogFormat("BlockHeight: " + e.BlockHeight);                  
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
    }
}