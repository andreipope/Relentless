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
using Loom.ZombieBattleground.Iap;

namespace Loom.ZombieBattleground.Iap
{
    public class FiatPlasmaManager : IService
    {
        private static readonly ILog Log = Logging.GetLog(nameof(FiatPlasmaManager));
        private static readonly ILog RpcLog = Logging.GetLog(nameof(FiatPlasmaManager) + "Rpc");

        public event Action<ContractRequest> OnRequestPackSuccess;
        public event Action OnRequestPackFailed;
        public event Action OnConnectionStateNotConnect;

        private ContractRequest _cachePackRequestingParams;

        private const string RequestPackResponseEventName = "PurchaseSent";

        private ContractManager _contractManager;

        private BackendDataControlMediator _backendDataControlMediator;

        private ILoadObjectsManager _loadObjectsManager;

        public void Init()
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _backendDataControlMediator = GameClient.Get<BackendDataControlMediator>();
            _contractManager = GameClient.Get<ContractManager>();
            _contractManager.OnContractCreated +=
            (
                IapContractType contractType,
                EvmContract oldContract,
                EvmContract newContract
            ) =>
            {
                if (contractType == IapContractType.FiatPurchase)
                {
                    newContract.Client.ReadClient.ConnectionStateChanged += RpcClientOnConnectionStateChanged;
                    newContract.Client.ReadClient.ConnectionStateChanged += RpcClientOnConnectionStateChanged;
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

        public async Task ClaimPacks(AuthFiatApiFacade.TransactionResponse fiatResponse)
        {
            ContractRequest contractParams = ParseContractRequestFromTransactionResponse(fiatResponse);
            await CallRequestPacksContract
            (
                await _contractManager.GetContract
                (
                    IapContractType.FiatPurchase
                ),
                contractParams
            );
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

        private const string RequestPacksMethod = "requestPacks";

        private async Task CallRequestPacksContract(EvmContract contract, ContractRequest contractParams)
        {
            if (contract == null)
            {
                throw new Exception("Contract not signed in!");
            }

            Log.Info( $"Calling smart contract [{RequestPacksMethod}]");
            _cachePackRequestingParams = contractParams;

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
        private ContractRequest ParseContractRequestFromTransactionResponse(AuthFiatApiFacade.TransactionResponse fiatResponse)
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
