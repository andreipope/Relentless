using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
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
    public class PlasmaChainBackendFacade : IService
    {
        private static readonly ILog Log = Logging.GetLog(nameof(PlasmaChainBackendFacade));
        private static readonly ILog RpcLog = Logging.GetLog(nameof(PlasmaChainBackendFacade) + "Rpc");

        private const int CardsPerPack = 5;

        private const string RequestPacksMethod = "requestPacks";

        private const string BalanceOfMethod = "balanceOf";

        private const string ApproveMethod = "approve";

        private const string OpenPackMethod = "openBoosterPack";

        private BackendDataControlMediator _backendDataControlMediator;

        private ILoadObjectsManager _loadObjectsManager;

        private IDataManager _dataManager;

        private Dictionary<IapContractType, TextAsset> _abiDictionary;

        private byte[] UserPrivateKey => _backendDataControlMediator.UserDataModel.PrivateKey;

        public byte[] UserPublicKey => CryptoUtils.PublicKeyFromPrivateKey(UserPrivateKey);

        public Address UserPlasmaChainAddress => Address.FromPublicKey(UserPublicKey, PlasmaChainEndpointsContainer.Chainid);

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

        public async Task<int> GetPackTypeBalance(DAppChainClient client, Enumerators.MarketplaceCardPackType packType)
        {
            Log.Info($"{nameof(GetPackTypeBalance)}(packType = {packType})");

            EvmContract packTypeContract = GetContract(client, GetPackContractTypeFromId(packType));
            int amount = await packTypeContract.StaticCallSimpleTypeOutputAsync<int>(
                BalanceOfMethod,
                UserPlasmaChainAddress.LocalAddress
            );

            Log.Info($"{nameof(GetPackTypeBalance)}(packType = {packType}) returned {amount}");
            return amount;
        }

        public async Task<IReadOnlyList<Card>> CallOpenPack(DAppChainClient client, Enumerators.MarketplaceCardPackType packType)
        {
            Log.Info($"{nameof(GetPackTypeBalance)}(packType = {packType})");
            EvmContract cardFaucetContract = GetContract(client, IapContractType.CardFaucet);
            EvmContract packContract = GetContract(client, GetPackContractTypeFromId(packType));

            const int amountToApprove = 1;
            await packContract.CallAsync(ApproveMethod, PlasmaChainEndpointsContainer.ContractAddressCardFaucet, amountToApprove);
            BroadcastTxResult openPackTxResult = await cardFaucetContract.CallAsync(OpenPackMethod, packType);
            byte[] openPackTxHash = openPackTxResult.DeliverTx.Data;

            async Task<List<EventLog<T>>> GetEvents<T>(string eventName) where T : new()
            {
                // Get all events since call to OpenPackMethod
                EvmEvent<T> evmEvent = cardFaucetContract.GetEvent<T>(eventName);
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
                    return null;
                }

                // Filter out events not belonging to the call we just made
                changes = changes.Where(log => openPackTxHash.SequenceEqual(CryptoUtils.HexStringToBytes(log.Log.TransactionHash))).ToList();
                return changes;
            }

            const int maxRetryCount = 10;
            const int retryDelay = 1500;

            // We only have the transaction hash at this point, but it might still be mining.
            // Poll the result multiple times until we have one.
            List<EventLog<GeneratedCardEvent>> generatedCardEvents = null;
            for (int i = 0; i < maxRetryCount; i++)
            {
                generatedCardEvents = await GetEvents<GeneratedCardEvent>("GeneratedCard");
                if (generatedCardEvents != null && generatedCardEvents.Count != 0)
                    break;

                Log.Warn($"Retrying getting GeneratedCard events, attempt {i + 1}/{maxRetryCount}");
                await Task.Delay(retryDelay);
            }

            Assert.IsNotNull(generatedCardEvents);
            if (generatedCardEvents.Count == 0)
                throw new Exception("Exhausted attempts to get generated cards");

            // No need to retry those, if getting GeneratedCard succeeds, these will succeed too
            List<EventLog<UpgradedCardToBackerEditionEvent>> upgradedCardToBackerEditionEvents =
                await GetEvents<UpgradedCardToBackerEditionEvent>("UpgradedCardToBE");
            List<EventLog<UpgradedCardToLimitedEditionEvent>> upgradedCardToLimitedEditionEvents =
                await GetEvents<UpgradedCardToLimitedEditionEvent>("UpgradedCardToLE");

            Log.Debug($"{nameof(GetPackTypeBalance)}: generatedCardEvents = {Utilites.FormatCallLogList(generatedCardEvents.Select(e => e.Event))}");
            Log.Debug($"{nameof(GetPackTypeBalance)}: upgradedCardToBackerEditionEvents = {Utilites.FormatCallLogList(upgradedCardToBackerEditionEvents.Select(e => e.Event))}");
            Log.Debug($"{nameof(GetPackTypeBalance)}: upgradedCardToLimitedEditionEvents = {Utilites.FormatCallLogList(upgradedCardToLimitedEditionEvents.Select(e => e.Event))}");

            // Calculate list of card token IDs, accounting for card upgrades
            List<BigInteger> cardTokenIds = new List<BigInteger>();
            for (int i = 0; i < generatedCardEvents.Count; i++)
            {
                EventLog<GeneratedCardEvent> generatedCardEvent = generatedCardEvents[i];
                EventLog<GeneratedCardEvent> nextGeneratedCardEvent = i == generatedCardEvents.Count - 1 ? null : generatedCardEvents[i + 1];
                EventLog<UpgradedCardToBackerEditionEvent> nextUpgradedCardToBackerEditionEvent =
                    upgradedCardToBackerEditionEvents.FirstOrDefault(log => log.Log.LogIndex.Value > generatedCardEvent.Log.LogIndex.Value);
                EventLog<UpgradedCardToLimitedEditionEvent> nextUpgradedCardToLimitedEditionEvent =
                    upgradedCardToLimitedEditionEvents.FirstOrDefault(log => log.Log.LogIndex.Value > generatedCardEvent.Log.LogIndex.Value);

                BigInteger? upgradeEventLogIndex =
                    nextUpgradedCardToBackerEditionEvent?.Log.LogIndex.Value ??
                    nextUpgradedCardToLimitedEditionEvent?.Log.LogIndex.Value;
                BigInteger? upgradedCardTokenId =
                    nextUpgradedCardToBackerEditionEvent?.Event.CardTokenId ??
                    nextUpgradedCardToLimitedEditionEvent?.Event.CardTokenId;

                if (nextGeneratedCardEvent != null)
                {
                    if (upgradeEventLogIndex != null)
                    {
                        cardTokenIds.Add(
                            upgradeEventLogIndex.Value < nextGeneratedCardEvent.Log.LogIndex.Value ?
                                upgradedCardTokenId.Value :
                                generatedCardEvent.Event.CardTokenId
                        );
                    }
                    else
                    {
                        cardTokenIds.Add(generatedCardEvent.Event.CardTokenId);
                    }
                }
                else
                {
                    cardTokenIds.Add(upgradedCardTokenId ?? generatedCardEvent.Event.CardTokenId);
                }
            }

            Log.Debug($"{nameof(GetPackTypeBalance)}: cardTokenIds = {Utilites.FormatCallLogList(cardTokenIds)}");

            // Convert card token IDs to actual cards
            List<Card> cards = new List<Card>();
            foreach (BigInteger cardTokenId in cardTokenIds)
            {
                CardKey cardKey = CardKey.FromCardTokenId((long) cardTokenId);
                Log.Info($"{nameof(GetPackTypeBalance)}: Parsed CardKey {cardKey}");
                (bool found, Card card) = _dataManager.CachedCardsLibraryData.TryGetCardFromCardKey(cardKey, true);
                if (found)
                {
                    Log.Info($"{nameof(GetPackTypeBalance)}: Found matching card {card}");
                    cards.Add(card);
                }
                else
                {
                    Log.Warn($"{nameof(GetPackTypeBalance)}: Unknown card with CardKey {cardKey}");
                    cards.Add(null);
                }
            }

            cards = cards.Where(card => card != null).ToList();
            Log.Info($"{nameof(GetPackTypeBalance)} returned {Utilites.FormatCallLogList(cards)}");
            return cards;
        }

        private EvmContract GetContract(DAppChainClient client, IapContractType contractType)
        {
            if (client == null)
                throw new ArgumentNullException(nameof(client));

            return new EvmContract(
                client,
                Address.FromString(GetContractAddress(contractType), PlasmaChainEndpointsContainer.Chainid),
                UserPlasmaChainAddress,
                _abiDictionary[contractType].text);
        }

        private DAppChainClient CreateClient()
        {
            Log.Debug("Creating PlasmaChain client");

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
                CallTimeout = Constants.PlasmaChainCallTimeout,
                StaticCallTimeout = Constants.PlasmaChainCallTimeout
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

        private static string GetContractAddress(IapContractType contractType)
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
                TxID = fiatReceipt.TxID
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
