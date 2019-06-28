using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UnityEngine;
using System.Threading.Tasks;
using Loom.Client;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.BackendCommunication;
using log4net;
using log4netUnitySupport;
using Loom.Nethereum.ABI.FunctionEncoding.Attributes;
using Loom.Nethereum.Contracts;
using Loom.Nethereum.Hex.HexTypes;
using Loom.Nethereum.RPC.Eth.DTOs;
using Loom.ZombieBattleground.Data;
using Newtonsoft.Json;
using UnityEngine.Assertions;

namespace Loom.ZombieBattleground.Iap
{
    public class PlasmachainBackendFacade : IService
    {
        private static readonly ILog Log = Logging.GetLog(nameof(PlasmachainBackendFacade));
        private static readonly ILog RpcLog = Logging.GetLog(nameof(PlasmachainBackendFacade) + "Rpc");

        private const int CardsPerPack = 5;

        private const string RequestPacksMethod = "requestPacks";

        private const string BalanceOfMethod = "balanceOf";

        private const string TokensOwnedMethod = "tokensOwned";

        private const string ApproveMethod = "approve";

        private const string OpenPackMethod = "openBoosterPack";

        private BackendDataControlMediator _backendDataControlMediator;

        private ILoadObjectsManager _loadObjectsManager;

        private IDataManager _dataManager;

        private Dictionary<IapContractType, TextAsset> _abiDictionary;

        private byte[] UserPrivateKey => _backendDataControlMediator.UserDataModel.PrivateKey;

        private byte[] UserPublicKey => _backendDataControlMediator.UserDataModel.PublicKey;

        private Address UserPlasmachainAddress => new Address(_backendDataControlMediator.UserDataModel.Address.LocalAddress, EndpointsConfiguration.ChainId);

        public PlasmachainEndpointsConfiguration EndpointsConfiguration { get; private set; }

        public PlasmachainBackendFacade(PlasmachainEndpointsConfiguration endpointsConfiguration)
        {
            SetEndpoints(endpointsConfiguration);
        }

        public void SetEndpoints(PlasmachainEndpointsConfiguration endpointsConfiguration)
        {
            EndpointsConfiguration = endpointsConfiguration ?? throw new ArgumentNullException(nameof(endpointsConfiguration));
            Log.Info("Endpoints: " + JsonConvert.SerializeObject(EndpointsConfiguration, Formatting.Indented));
        }

        public void Init()
        {
            _backendDataControlMediator = GameClient.Get<BackendDataControlMediator>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _dataManager = GameClient.Get<IDataManager>();

            InitAbiTextAssets();
        }

        public void Update() { }

        public void Dispose() { }

        public async Task<DAppChainClient> GetConnectedClient()
        {
            DAppChainClient client = CreateClient();
            client.TxMiddleware = new TxMiddleware(new ITxMiddlewareHandler[]
            {
                new NonceTxMiddleware(UserPublicKey, client), new SignedTxMiddleware(UserPrivateKey)
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

        public async Task ClaimPacks(DAppChainClient client, AuthFiatApiFacade.TransactionReceipt fiatReceipt)
        {
            EvmContract evmContract = GetContract(client, IapContractType.FiatPurchase);
            RequestPacksRequest requestPacksRequest = CreateContractRequestFromTransactionResponse(fiatReceipt);
            await CallRequestPacksContract(evmContract, requestPacksRequest);
        }

        public async Task<uint> GetPackTypeBalance(DAppChainClient client, Enumerators.MarketplaceCardPackType packType)
        {
            Log.Info($"{nameof(GetPackTypeBalance)}(packType = {packType})");

            EvmContract packTypeContract = GetContract(client, GetPackContractTypeFromId(packType));
            uint amount = await packTypeContract.StaticCallSimpleTypeOutputAsync<uint>(
                BalanceOfMethod,
                UserPlasmachainAddress.LocalAddress
            );

            Log.Info($"{nameof(GetPackTypeBalance)}(packType = {packType}) returned {amount}");
            return amount;
        }

        public async Task<IReadOnlyList<CollectionCardData>> GetCardsOwned(DAppChainClient client)
        {
            Log.Info($"{nameof(GetCardsOwned)}()");

            EvmContract packTypeContract = GetContract(client, IapContractType.ZbgCard);
            TokensOwnedFunctionResult result =
                await packTypeContract.StaticCallDtoTypeOutputAsync<TokensOwnedFunctionResult>(
                    TokensOwnedMethod,
                    UserPlasmachainAddress.LocalAddress
                );

            CollectionCardData[] cards = new CollectionCardData[result.Indexes.Count];
            for (int i = 0; i < result.Indexes.Count; i++)
            {
                cards[i] = new CollectionCardData(CardKey.FromCardTokenId((long) result.Indexes[i]), (int) result.Balances[i]);
            }

            Log.Info($"{nameof(GetPackTypeBalance)}() returned {Utilites.FormatCallLogList(cards)}");
            return cards;
        }

        public async Task<IReadOnlyList<CardKey>> CallOpenPack(DAppChainClient client, Enumerators.MarketplaceCardPackType packType)
        {
            Log.Info($"{nameof(GetPackTypeBalance)}(packType = {packType})");
            EvmContract zbgCardContract = GetContract(client, IapContractType.ZbgCard);
            EvmContract cardFaucetContract = GetContract(client, IapContractType.CardFaucet);
            EvmContract packContract = GetContract(client, GetPackContractTypeFromId(packType));

            const int amountToApprove = 1;
            await packContract.CallAsync(ApproveMethod, EndpointsConfiguration.CardFaucetContractAddress.LocalAddress, amountToApprove);
            BroadcastTxResult openPackTxResult = await cardFaucetContract.CallAsync(OpenPackMethod, packType);
            byte[] openPackTxHash = openPackTxResult.DeliverTx.Data;
            Log.Debug($"{nameof(CallOpenPack)}: openPackTxHash = {CryptoUtils.BytesToHexString(openPackTxHash)}");

            async Task<IReadOnlyList<EventLog<T>>> GetEvents<T>(string eventName) where T : new()
            {
                // Get all events since call to OpenPackMethod
                EvmEvent<T> evmEvent = zbgCardContract.GetEvent<T>(eventName);
                NewFilterInput filterInput =
                    evmEvent.CreateFilterInput(
                        new BlockParameter(new HexBigInteger(openPackTxResult.Height)),
                        BlockParameter.CreatePending()
                    );
                List<EventLog<T>> changes;
                try
                {
                    changes = await evmEvent.GetAllChanges(filterInput);
                }
                catch (RpcClientException e) when (e.Message.Contains("to block before end block"))
                {
                    Log.Debug($"{nameof(CallOpenPack)}: got 'to block before end block', will retry");
                    return Array.Empty<EventLog<T>>();
                }

                // Filter out events not belonging to the call we just made
                changes = changes.Where(log => openPackTxHash.SequenceEqual(CryptoUtils.HexStringToBytes(log.Log.TransactionHash))).ToList();
                return changes;
            }

            const int maxRetryCount = 10;
            const int retryDelay = 1500;

            // We only have the transaction hash at this point, but it might still be mining.
            // Poll the result multiple times until we have one.
            IReadOnlyList<EventLog<TransferWithQuantityEvent>> transferWithQuantityEvents = null;
            for (int i = 0; i < maxRetryCount; i++)
            {
                transferWithQuantityEvents = await GetEvents<TransferWithQuantityEvent>("TransferWithQuantity");
                if (transferWithQuantityEvents.Count == 0)
                {
                    // HACK: event renamed on prod plasmachain, so get TransferToken events and convert them
                    IReadOnlyList<EventLog<TransferTokenEvent>> transferTokenEvents = await GetEvents<TransferTokenEvent>("TransferToken");
                    transferWithQuantityEvents =
                        transferTokenEvents
                            .Select(t => new EventLog<TransferWithQuantityEvent>(t.Event.ConvertToTransferWithQuantityEvent(), t.Log))
                            .ToList();
                }

                if (transferWithQuantityEvents.Count == CardsPerPack)
                    break;

                Log.Warn(
                    $"{nameof(CallOpenPack)}: Retrying getting {nameof(TransferWithQuantityEvent)} events " +
                    $"(got {Utilites.FormatCallLogList(transferWithQuantityEvents)}), attempt {i + 1}/{maxRetryCount}");
                await Task.Delay(retryDelay);
            }

            if (transferWithQuantityEvents == null || transferWithQuantityEvents.Count == 0)
                throw new Exception("Exhausted attempts to get generated cards");

            Log.Debug($"{nameof(CallOpenPack)}: transferEvents = {Utilites.FormatCallLogList(transferWithQuantityEvents.Select(e => e.Event))}");

            // Get card keys
            List<BigInteger> cardTokenIds = transferWithQuantityEvents.Select(evt => evt.Event.TokenId).ToList();
            Log.Debug($"{nameof(CallOpenPack)}: cardTokenIds = {Utilites.FormatCallLogList(cardTokenIds)}");

            List<CardKey> cardKeys =
                cardTokenIds
                    .Select(cardTokenId => CardKey.FromCardTokenId((long) cardTokenId))
                    .ToList();

            Log.Info($"{nameof(CallOpenPack)}: returned {Utilites.FormatCallLogList(cardKeys)}");
            return cardKeys;
        }

        private EvmContract GetContract(DAppChainClient client, IapContractType contractType)
        {
            if (client == null)
                throw new ArgumentNullException(nameof(client));

            Log.Debug($"Using {contractType} contract at {GetContractAddress(contractType)}");
            return new EvmContract(
                client,
                GetContractAddress(contractType),
                UserPlasmachainAddress,
                _abiDictionary[contractType].text);
        }

        private DAppChainClient CreateClient()
        {
            Log.Debug("Creating PlasmaChain client");

            ILogger logger = new UnityLoggerWrapper(RpcLog);

            IRpcClient writer = RpcClientFactory
                .Configure()
                .WithLogger(logger)
                .WithWebSocket(EndpointsConfiguration.WriterHost)
                //.WithHttp(PlasmachainEndpointsContainer.HttpRpc)
                .Create();

            IRpcClient reader = RpcClientFactory
                .Configure()
                .WithLogger(logger)
                .WithWebSocket(EndpointsConfiguration.ReaderHost)
                //.WithHttp(PlasmachainEndpointsContainer.HttpQuery)
                .Create();

            DAppChainClientConfiguration clientConfiguration = new DAppChainClientConfiguration
            {
                AutoReconnect = false,
                CallTimeout = Constants.PlasmachainCallTimeout,
                StaticCallTimeout = Constants.PlasmachainCallTimeout
            };

            return new LoggingDAppChainClient(
                writer,
                reader,
                clientConfiguration,
                new NotifyingDAppChainClientCallExecutor(clientConfiguration)
            )
            {
                Logger = logger
            };
        }

        private async Task CallRequestPacksContract(EvmContract contract, RequestPacksRequest requestPacksRequest)
        {
            Log.Info($"{nameof(CallRequestPacksContract)}, ContractRequest:\n" + JsonConvert.SerializeObject(requestPacksRequest));
            await contract.CallAsync(
                RequestPacksMethod,
                requestPacksRequest.UserId,
                requestPacksRequest.R,
                requestPacksRequest.S,
                requestPacksRequest.V,
                requestPacksRequest.Hash,
                requestPacksRequest.Amount,
                requestPacksRequest.TxID
            );
            Log.Info($"Smart contract method [{RequestPacksMethod}] finished executing.");
        }

        private void InitAbiTextAssets()
        {
            (IapContractType contractType, string path)[] contractTypeToPath =
            {
                (IapContractType.ZbgCard, "Data/abi/MigratedZBGCardABI"),
                (IapContractType.FiatPurchase, "Data/abi/FiatPurchaseABI"),
                (IapContractType.CardFaucet, "Data/abi/CardFaucetABI")
            };

            _abiDictionary = new Dictionary<IapContractType, TextAsset>();
            foreach ((IapContractType contractType, string path) in contractTypeToPath)
            {
                TextAsset textAsset = _loadObjectsManager.GetObjectByPath<TextAsset>(path);
                if (textAsset == null)
                    throw new Exception("Unable to load ABI at path " + path);

                _abiDictionary.Add(contractType, textAsset);
            }

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
                string path = $"Data/abi/{contractTypeToCardPackType[i].cardPackType.ToString()}PackABI";
                TextAsset textAsset = _loadObjectsManager.GetObjectByPath<TextAsset>(path);
                if (textAsset == null)
                    throw new Exception("Unable to load ABI at path " + path);

                _abiDictionary.Add(contractTypeToCardPackType[i].contractType, textAsset);
            }
        }

        private Address GetContractAddress(IapContractType contractType)
        {
            switch (contractType)
            {
                case IapContractType.ZbgCard:
                    return EndpointsConfiguration.ZbgCardContractAddress;
                case IapContractType.FiatPurchase:
                    return EndpointsConfiguration.FiatPurchaseContractAddress;
                case IapContractType.CardFaucet:
                    return EndpointsConfiguration.CardFaucetContractAddress;
                case IapContractType.BoosterPack:
                    return EndpointsConfiguration.BoosterPackContractAddress;
                case IapContractType.SuperPack:
                    return EndpointsConfiguration.SuperPackContractAddress;
                case IapContractType.AirPack:
                    return EndpointsConfiguration.AirPackContractAddress;
                case IapContractType.EarthPack:
                    return EndpointsConfiguration.EarthPackContractAddress;
                case IapContractType.FirePack:
                    return EndpointsConfiguration.FirePackContractAddress;
                case IapContractType.LifePack:
                    return EndpointsConfiguration.LifePackContractAddress;
                case IapContractType.ToxicPack:
                    return EndpointsConfiguration.ToxicPackContractAddress;
                case IapContractType.WaterPack:
                    return EndpointsConfiguration.WaterPackContractAddress;
                case IapContractType.SmallPack:
                    return EndpointsConfiguration.SmallPackContractAddress;
                case IapContractType.MinionPack:
                    return EndpointsConfiguration.MinionPackContractAddress;
                default:
                    throw new ArgumentOutOfRangeException(nameof(contractType), contractType, null);
            }
        }

        private static IapContractType GetPackContractTypeFromId(Enumerators.MarketplaceCardPackType packId)
        {
            switch (packId)
            {
                case Enumerators.MarketplaceCardPackType.Booster:
                    return IapContractType.BoosterPack;
                case Enumerators.MarketplaceCardPackType.Super:
                    return IapContractType.SuperPack;
                case Enumerators.MarketplaceCardPackType.Air:
                    return IapContractType.AirPack;
                case Enumerators.MarketplaceCardPackType.Earth:
                    return IapContractType.EarthPack;
                case Enumerators.MarketplaceCardPackType.Fire:
                    return IapContractType.FirePack;
                case Enumerators.MarketplaceCardPackType.Life:
                    return IapContractType.LifePack;
                case Enumerators.MarketplaceCardPackType.Toxic:
                    return IapContractType.ToxicPack;
                case Enumerators.MarketplaceCardPackType.Water:
                    return IapContractType.WaterPack;
                case Enumerators.MarketplaceCardPackType.Small:
                    return IapContractType.SmallPack;
                case Enumerators.MarketplaceCardPackType.Minion:
                    return IapContractType.MinionPack;
                default:
                    throw new Exception($"Not found ContractType from pack id {packId}");
            }
        }

        private static RequestPacksRequest CreateContractRequestFromTransactionResponse(AuthFiatApiFacade.TransactionReceipt fiatReceipt)
        {
            byte[] signature = fiatReceipt.VerifyHash.Signature;
            byte[] r = new byte[32];
            Array.Copy(signature, 0, r, 0, 32);
            byte[] s = new byte[32];
            Array.Copy(signature, 32, s, 0, 32);
            byte v = signature[signature.Length - 1];

            RequestPacksRequest request = new RequestPacksRequest
            {
                UserId = fiatReceipt.UserId,
                R = r,
                S = s,
                V = v,
                Hash = fiatReceipt.VerifyHash.Hash,
                Amount = new[]
                {
                    fiatReceipt.Booster,
                    fiatReceipt.Super,
                    fiatReceipt.Air,
                    fiatReceipt.Earth,
                    fiatReceipt.Fire,
                    fiatReceipt.Life,
                    fiatReceipt.Toxic,
                    fiatReceipt.Water,
                    fiatReceipt.Small,
                    fiatReceipt.Minion,
                    fiatReceipt.Binance
                },
                TxID = fiatReceipt.TxId
            };
            return request;
        }

        private class LoggingDAppChainClient : DAppChainClient
        {
            public LoggingDAppChainClient(
                IRpcClient writeClient,
                IRpcClient readClient,
                DAppChainClientConfiguration configuration = null,
                IDAppChainClientCallExecutor callExecutor = null)
                : base(writeClient, readClient, configuration, callExecutor) { }

            public override void Dispose()
            {
                base.Dispose();
                Log.Debug("Disposing PlasmaChain client");
            }
        }

        [FunctionOutput]
        public class TokensOwnedFunctionResult
        {
            [Parameter("uint256[]", "indexes")]
            public List<BigInteger> Indexes { get; set; }

            [Parameter("uint256[]", "balances")]
            public List<BigInteger> Balances { get; set; }
        }

        [Event("GeneratedCard")]
        private class GeneratedCardEvent
        {
            [Parameter("uint256", "cardId")]
            public BigInteger CardTokenId { get; set; }

            [Parameter("uint256", "boosterType", 2)]
            public BigInteger BoosterType { get; set; }

            public override string ToString()
            {
                return $"{nameof(CardTokenId)}: {CardTokenId}, {nameof(BoosterType)}: {BoosterType}";
            }
        }

        [Event("UpgradedCardToBE")]
        private class UpgradedCardToBackerEditionEvent
        {
            [Parameter("uint256", "cardId")]
            public BigInteger CardTokenId { get; set; }

            public override string ToString()
            {
                return $"{nameof(CardTokenId)}: {CardTokenId}";
            }
        }

        [Event("UpgradedCardToLE")]
        private class UpgradedCardToLimitedEditionEvent
        {
            [Parameter("uint256", "cardId")]
            public BigInteger CardTokenId { get; set; }

            public override string ToString()
            {
                return $"{nameof(CardTokenId)}: {CardTokenId}";
            }
        }

        // event Transfer(address indexed from, address indexed to, uint256 indexed tokenId);
        [Event("Transfer")]
        private class TransferEvent
        {
            [Parameter("address", "from", 1, true)]
            public string From { get; set; }

            [Parameter("address", "to", 2, true)]
            public string To { get; set; }

            [Parameter("uint256", "tokenId", 3, true)]
            public BigInteger TokenId { get; set; }

            public override string ToString()
            {
                return $"({nameof(From)}: {From}, {nameof(To)}: {To}, {nameof(TokenId)}: {TokenId})";
            }
        }

        // event BatchTransfer(address indexed from, address indexed to, uint256[] tokenTypes, uint256[] amounts);
        [Event("BatchTransfer")]
        private class BatchTransferEvent
        {
            [Parameter("address", "from", 1, true)]
            public string From { get; set; }

            [Parameter("address", "to", 2, true)]
            public string To { get; set; }

            [Parameter("uint256[]", "tokenTypes", 3, false)]
            public BigInteger[] TokenTypes { get; set; }

            [Parameter("uint256[]", "amounts", 4, false)]
            public BigInteger[] Amounts { get; set; }

            public override string ToString()
            {
                return
                    $"({nameof(From)}: {From}, " +
                    $"{nameof(To)}: {To}, " +
                    $"{nameof(TokenTypes)}: {Utilites.FormatCallLogList(TokenTypes)}, " +
                    $"{nameof(Amounts)}: {Utilites.FormatCallLogList(Amounts)})";
            }
        }

        // event TransferWithQuantity(address indexed from, address indexed to, uint256 indexed tokenId, uint256 quantity);
        [Event("TransferWithQuantity")]
        private class TransferWithQuantityEvent
        {
            [Parameter("address", "from", 1, false)]
            public string From { get; set; }

            [Parameter("address", "to", 2, false)]
            public string To { get; set; }

            [Parameter("uint256", "tokenId", 3, true)]
            public BigInteger TokenId { get; set; }

            [Parameter("uint256", "amount", 4, false)]
            public BigInteger Amount { get; set; }

            public override string ToString()
            {
                return $"({nameof(From)}: {From}, {nameof(To)}: {To}, {nameof(TokenId)}: {TokenId}, {nameof(Amount)}: {Amount})";
            }
        }

        // event TransferToken(indexed address from, indexed address to, uint256 indexed tokenId, uint256 amount);
        [Event("TransferToken")]
        private class TransferTokenEvent
        {
            [Parameter("address", "from", 1, true)]
            public string From { get; set; }

            [Parameter("address", "to", 2, true)]
            public string To { get; set; }

            [Parameter("uint256", "tokenId", 3, true)]
            public BigInteger TokenId { get; set; }

            [Parameter("uint256", "amount", 4, false)]
            public BigInteger Amount { get; set; }

            public TransferWithQuantityEvent ConvertToTransferWithQuantityEvent()
            {
                return new TransferWithQuantityEvent
                {
                    From = From,
                    To = To,
                    TokenId = TokenId,
                    Amount = Amount
                };
            }

            public override string ToString()
            {
                return $"({nameof(From)}: {From}, {nameof(To)}: {To}, {nameof(TokenId)}: {TokenId}, {nameof(Amount)}: {Amount})";
            }
        }

        private struct RequestPacksRequest
        {
            public BigInteger UserId;
            public byte[] R;
            public byte[] S;
            public byte V;
            public byte[] Hash;
            public uint[] Amount;
            public BigInteger TxID;
        }
    }
}
