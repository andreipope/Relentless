using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Loom.Client;
using log4net;
using Newtonsoft.Json;
using OneOf;
using OneOf.Types;

namespace Loom.ZombieBattleground.Iap
{
    public class FiatPlasmaManager : IService
    {
        private static readonly ILog Log = Logging.GetLog(nameof(FiatPlasmaManager));
        private static readonly ILog RpcLog = Logging.GetLog(nameof(FiatPlasmaManager) + "Rpc");

        private ContractManager _contractManager;

        public void Init()
        {
            _contractManager = GameClient.Get<ContractManager>();
        }

        public void Update() { }

        public void Dispose() { }

        public async Task<OneOf<Success, IapException>> ClaimPacks(DAppChainClient client, AuthFiatApiFacade.TransactionResponse fiatResponse)
        {
            try
            {
                EvmContract evmContract = _contractManager.GetContract(client, IapContractType.FiatPurchase);
                RequestPacksRequest requestPacksRequest = CreateContractRequestFromTransactionResponse(fiatResponse);
                await CallRequestPacksContract(evmContract, requestPacksRequest);
            }
            catch (Exception e)
            {
                return new IapException($"{nameof(ClaimPacks)} failed", e);
            }

            return new Success();
        }

        private const string RequestPacksMethod = "requestPacks";

        private async Task CallRequestPacksContract(EvmContract contract, RequestPacksRequest requestPacksRequest)
        {
            Log.Info($"{nameof(CallRequestPacksContract)}, ContractRequest:\n" + JsonConvert.SerializeObject(requestPacksRequest));
            await contract.CallAsync(
                RequestPacksMethod,
                requestPacksRequest.UserId,
                requestPacksRequest.r,
                requestPacksRequest.s,
                requestPacksRequest.v,
                requestPacksRequest.hash,
                requestPacksRequest.amount,
                requestPacksRequest.TxID
            );
            Log.Info($"Smart contract method [{RequestPacksMethod}] finished executing.");
        }

        private RequestPacksRequest CreateContractRequestFromTransactionResponse(AuthFiatApiFacade.TransactionResponse fiatResponse)
        {
            string r = fiatResponse.VerifyHash.signature.SubstringIndexed(2, 66);
            string s = fiatResponse.VerifyHash.signature.SubstringIndexed(66, 130);
            string v = fiatResponse.VerifyHash.signature.SubstringIndexed(130, 132);

            RequestPacksRequest request = new RequestPacksRequest
            {
                UserId = fiatResponse.UserId,
                r = CryptoUtils.HexStringToBytes(r),
                s = CryptoUtils.HexStringToBytes(s),
                v = (byte) Int32.Parse(v, NumberStyles.AllowHexSpecifier),
                hash = CryptoUtils.HexStringToBytes(fiatResponse.VerifyHash.hash),
                amount = new []
                {
                    fiatResponse.Booster,
                    fiatResponse.Super,
                    fiatResponse.Air,
                    fiatResponse.Earth,
                    fiatResponse.Fire,
                    fiatResponse.Life,
                    fiatResponse.Toxic,
                    fiatResponse.Water,
                    fiatResponse.Small,
                    fiatResponse.Minion,
                    fiatResponse.Binance
                },
                TxID = fiatResponse.TxID
            };
            return request;
        }

        private struct RequestPacksRequest
        {
            public int UserId;
            public byte[] r;
            public byte[] s;
            public byte v;
            public byte[] hash;
            public int[] amount;
            public int TxID;
        }
    }
}
