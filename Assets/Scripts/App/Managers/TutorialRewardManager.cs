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

namespace Loom.ZombieBattleground
{

    public class TutorialRewardManager : IService
    {
        #region Contract
        private TextAsset _abiTutorialReward;
        private EvmContract _tutorialRewardContract;
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
        
        private BackendDataControlMediator _backendDataControlMediator;
        private ILoadObjectsManager _loadObjectsManager;
        private IUIManager _uiManager;
        private BackendFacade _backendFacade;
    
        public void Init()
        {           
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _uiManager = GameClient.Get<IUIManager>();
            _backendDataControlMediator = GameClient.Get<BackendDataControlMediator>();
            _backendFacade = GameClient.Get<BackendFacade>();
            
            _abiTutorialReward = _loadObjectsManager.GetObjectByPath<TextAsset>("Data/abi/TutorialRewardABI");            
        }
        
        public void Update()
        {
        }
        
        public void Dispose()
        {
        }
        
        public async Task CallRewardTutorialFlow()
        {
            _uiManager.DrawPopup<LoadingFiatPopup>("CallRewardTutorialComplete...");
            TutorialRewardManager tutorialRewardManager = GameClient.Get<TutorialRewardManager>();

            RewardTutorialCompletedResponse response = null;
            try
            {
                response = await tutorialRewardManager.CallRewardTutorialComplete();
            }catch
            {
                Debug.Log("CallRewardTutorialComplete failed");
            }
            if (response != null)
            {
                try
                {
                    _uiManager.HidePopup<LoadingFiatPopup>();
                    _uiManager.DrawPopup<LoadingFiatPopup>("CallTutorialRewardContract...");
                    await tutorialRewardManager.CallTutorialRewardContract(response);
                }
                catch
                {
                    Debug.Log("CallTutorialRewardContract failed");
                }
            }            
               
            _uiManager.HidePopup<LoadingFiatPopup>();
            _uiManager.DrawPopup<RewardPopup>();
        }

        public async Task<RewardTutorialCompletedResponse> CallRewardTutorialComplete()
        { 
            RewardTutorialCompletedResponse response = await _backendFacade.GetRewardTutorialCompletedResponse();
            Debug.Log($"RewardTutorialCompletedResponse");
            
            Debug.Log($"Hash: {response.Hash}");
            Debug.Log($"R: {response.R}");
            Debug.Log($"S: {response.S}");
            Debug.Log($"V: {response.V}");
            Debug.Log($"RewardType: {response.RewardType}");   
            Debug.Log($"Amount: {BigUIntToSByte(response.Amount)}");                    
            
            return response;
        }

        public async Task CallTutorialRewardContract(RewardTutorialCompletedResponse rewardTutorialCompletedResponse)
        {            
            ContractRequest contractParams = ParseContractRequestFromRewardTutorialCompletedResponse(rewardTutorialCompletedResponse);
            _tutorialRewardContract = await GetContract
            (
                PrivateKey,
                PublicKey,
                _abiTutorialReward.ToString(),
                PlasmaChainEndpointsContainer.ContractAddressTutorialReward
            );
            await CallTutorialRewardContract(_tutorialRewardContract, contractParams);
        }
        
        private ContractRequest GenerateFakeContractRequest()
        {
            ContractRequest contractParams = new ContractRequest();
            contractParams.r = HexStringToByte("0x6408eb878d2c1617028dc9590d622d0bdfdb353255091c6c0c6325049b068269".Substring(2));
            contractParams.s = HexStringToByte("0x72d04e6d8831712a883d2895784eadd42d9722f96f9f7eca3a2ab1f68def4f31".Substring(2));
            contractParams.v = 28;
            contractParams.hash = HexStringToByte("0x995f062c0503dd3fb3f4dae00eb3c7ddc50cfdf45d270522675aa140e876725b".Substring(2));
            contractParams.amount = 1;
            return contractParams;
        }

        private async Task CallTutorialRewardContract(EvmContract contract, ContractRequest contractParams)
        {
            if (contract == null)
            {
                throw new Exception("Contract not signed in!");
            }
            Debug.Log($"Calling TutorialRewardContract [requestPacks]");
            await contract.CallAsync
            (
                "requestPacks",
                contractParams.r,
                contractParams.s,
                contractParams.v,
                contractParams.hash,
                contractParams.amount
            );
            
            Debug.Log($"Smart contract method [requestPacks] finished executing.");            
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
            public byte[] r;
            public byte[] s;
            public sbyte v;
            public byte[] hash;
            public int amount;            
        }
        #region Util
        private ContractRequest ParseContractRequestFromRewardTutorialCompletedResponse(RewardTutorialCompletedResponse rewardTutorialCompletedResponse)
        {
            string log = "<color=green>ContractRequest Params:</color>\n";
            string hash = rewardTutorialCompletedResponse.Hash.Substring(2);
            string r = rewardTutorialCompletedResponse.R.Substring(2);
            string s = rewardTutorialCompletedResponse.S.Substring(2);
            sbyte v = Convert.ToSByte(rewardTutorialCompletedResponse.V);
            int amount = (int)BigUIntToSByte(rewardTutorialCompletedResponse.Amount);
            
            log += "r: " + r + "\n";
            log += "s: " + s + "\n";
            log += "v: " + v + "\n";
            log += "hash: " + hash + "\n";        
            log += "amount: " + amount + "\n";
            Debug.Log(log);
    
            ContractRequest contractParams = new ContractRequest();
            contractParams.r = HexStringToByte(r);
            contractParams.s = HexStringToByte(s);
            contractParams.v = v;
            contractParams.hash = HexStringToByte(hash);
            contractParams.amount = amount;
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
    
        public Byte[] HexStringToByte(string str)
        {
            string hex = str; 
            byte[] bytes = new byte[hex.Length / 2];
            
            for (int i = 0; i < hex.Length; i += 2)
                bytes[i/2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }

        public int BigUIntToInt(Client.Protobuf.BigUInt bigNumber)
        {
            Array someArray = Convert.FromBase64String
            (
                bigNumber.Value.ToBase64()
            );
            Array.Reverse(someArray);
            
            byte[] bytes = (byte[])someArray;
            byte[] concatBytes = new byte[bytes.Length + 1];
            bytes.CopyTo(concatBytes, 0);
            (new byte[] { 0 }).CopyTo(concatBytes, bytes.Length);
            BigInteger bigInteger = new BigInteger(concatBytes);
            return (int)bigInteger;
        }
        
        public sbyte BigUIntToSByte(Client.Protobuf.BigUInt bigNumber)
        {
            Array someArray = Convert.FromBase64String
            (
                bigNumber.Value.ToBase64()
            );
            Array.Reverse(someArray);
            
            byte[] bytes = (byte[])someArray;
            byte[] concatBytes = new byte[bytes.Length + 1];
            bytes.CopyTo(concatBytes, 0);
            (new byte[] { 0 }).CopyTo(concatBytes, bytes.Length);
            BigInteger bigInteger = new BigInteger(concatBytes);
            return (sbyte)bigInteger;
        }
        #endregion
    }

}