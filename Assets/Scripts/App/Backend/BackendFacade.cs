using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Numerics;
using Loom.Client;
using Loom.Google.Protobuf;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.Protobuf;
using Newtonsoft.Json;
using Plugins.AsyncAwaitUtil.Source;
using UnityEngine;
using System.Text;
using log4net;
using log4netUnitySupport;

namespace Loom.ZombieBattleground.BackendCommunication
{
    public class BackendFacade : IService
    {
        private IContractCallProxy _contractCallProxy;
        private Func<RawChainEventContract, IContractCallProxy> _contractCallProxyFactory;

        public delegate void ContractCreatedEventHandler(RawChainEventContract oldContract, RawChainEventContract newContract);

        public delegate void PlayerActionDataReceivedHandler(byte[] bytes);

        public event ContractCreatedEventHandler ContractCreated;

        public BackendEndpoint BackendEndpoint { get; private set; }

        public RawChainEventContract Contract { get; private set; }

        public bool IsConnected => Contract != null &&
            Contract.Client.ReadClient.ConnectionState == RpcConnectionState.Connected &&
            Contract.Client.WriteClient.ConnectionState == RpcConnectionState.Connected;

        public IContractCallProxy ContractCallProxy => _contractCallProxy;

        public ILog Log { get; }

        public ILog RpcLog { get; }

        public BackendFacade(BackendEndpoint backendEndpoint, Func<RawChainEventContract, IContractCallProxy> contractCallProxyFactory, ILog log, ILog rpcLog)
        {
            Log = log ?? throw new ArgumentNullException(nameof(log));
            RpcLog = rpcLog ?? throw new ArgumentNullException(nameof(rpcLog));
            SetBackendEndpoint(backendEndpoint);
            _contractCallProxyFactory = contractCallProxyFactory ?? throw new ArgumentNullException(nameof(contractCallProxyFactory));
        }

        public void SetBackendEndpoint(BackendEndpoint backendEndpoint)
        {
            BackendEndpoint = backendEndpoint ?? throw new ArgumentNullException(nameof(backendEndpoint));
            Log.Info("Reader Host: " + BackendEndpoint.ReaderHost);
            Log.Info("Writer Host: " + BackendEndpoint.WriterHost);
            Log.Info("Auth Host: " + BackendEndpoint.AuthHost);
            Log.Info("Vault Host: " + BackendEndpoint.VaultHost);
            Log.Info("Card Data Version: " + BackendEndpoint.DataVersion);
        }

        public void Init()
        {
        }

        public void Update()
        {
        }

        public void Dispose()
        {
            Contract?.Client?.Dispose();
            _contractCallProxy?.Dispose();
        }

        public async Task CreateContract(
            byte[] privateKey,
            DAppChainClientConfiguration clientConfiguration,
            Action<DAppChainClient> onClientCreatedCallback = null,
            IDAppChainClientCallExecutor chainClientCallExecutor = null
            )
        {
            byte[] publicKey = CryptoUtils.PublicKeyFromPrivateKey(privateKey);
            Address callerAddr = Address.FromPublicKey(publicKey);

            ILogger logger = RpcLog != null ? (ILogger) new UnityLoggerWrapper(RpcLog) : NullLogger.Instance;

            IRpcClient writer =
                RpcClientFactory
                    .Configure()
                    .WithLogger(logger)
                    .WithWebSocket(BackendEndpoint.WriterHost)
                    .Create();

            IRpcClient reader =
                RpcClientFactory
                    .Configure()
                    .WithLogger(logger)
                    .WithWebSocket(BackendEndpoint.ReaderHost)
                    .Create();

            DAppChainClient client = new DAppChainClient(
                writer,
                reader,
                clientConfiguration,
                chainClientCallExecutor
                )
            {
                Logger = logger
            };

            client.TxMiddleware = new TxMiddleware(new ITxMiddlewareHandler[]
            {
                new NonceTxMiddleware(publicKey, client),
                new SignedTxMiddleware(privateKey)
            });

            client.Configuration.AutoReconnect = false;
            onClientCreatedCallback?.Invoke(client);

            await client.ReadClient.ConnectAsync();
            await client.WriteClient.ConnectAsync();
            Address contractAddress = await client.ResolveContractAddressAsync("ZombieBattleground");
            RawChainEventContract oldContract = Contract;
            Contract = new RawChainEventContract(client, contractAddress, callerAddr);

            _contractCallProxy = _contractCallProxyFactory?.Invoke(Contract);
            ContractCreated?.Invoke(oldContract, Contract);
        }

        #region Card Collection

        private const string GetCardCollectionMethod = "GetCollection";

        public async Task<GetCollectionResponse> GetCardCollection(string userId)
        {
            GetCollectionRequest request = new GetCollectionRequest
            {
                UserId = userId,
                Version = BackendEndpoint.DataVersion
            };

            return await _contractCallProxy.CallAsync<GetCollectionResponse>(GetCardCollectionMethod, request);
        }

        #endregion

        #region Card Library

        private const string GetCardLibraryMethod = "ListCardLibrary";

        public async Task<ListCardLibraryResponse> GetCardLibrary()
        {
            ListCardLibraryRequest request = new ListCardLibraryRequest
            {
                Version = BackendEndpoint.DataVersion
            };

            return await _contractCallProxy.StaticCallAsync<ListCardLibraryResponse>(GetCardLibraryMethod, request);
        }

        private const string RequestUserFullCardCollectionSyncMethod = "RequestUserFullCardCollectionSync";

        public async Task RequestUserFullCardCollectionSync(string userId)
        {
            RequestUserFullCardCollectionSyncRequest request = new RequestUserFullCardCollectionSyncRequest
            {
                UserId = userId
            };

            await _contractCallProxy.CallAsync(RequestUserFullCardCollectionSyncMethod, request);
        }

        #endregion

        #region Deck Management

        private const string GetAiDecksDataMethod = "GetAIDecks";

        private const string ListDecksDataMethod = "ListDecks";

        private const string DeleteDeckMethod = "DeleteDeck";

        private const string AddDeckMethod = "CreateDeck";

        private const string EditDeckMethod = "EditDeck";

        public async Task<ListDecksResponse> ListDecks(string userId)
        {
            ListDecksRequest request = new ListDecksRequest
            {
                UserId = userId,
                Version = BackendEndpoint.DataVersion
            };

            return await _contractCallProxy.CallAsync<ListDecksResponse>(ListDecksDataMethod, request);
        }

        public async Task<GetAIDecksResponse> GetAiDecks()
        {
            GetAIDecksRequest request = new GetAIDecksRequest
            {
                Version = BackendEndpoint.DataVersion
            };

            return await _contractCallProxy.StaticCallAsync<GetAIDecksResponse>(GetAiDecksDataMethod, request);
        }

        public async Task DeleteDeck(string userId, DeckId deckId)
        {
            DeleteDeckRequest request = new DeleteDeckRequest
            {
                UserId = userId,
                DeckId = deckId.Id,
                Version = BackendEndpoint.DataVersion
            };

            await _contractCallProxy.CallAsync(DeleteDeckMethod, request);
        }

        public async Task EditDeck(string userId, Data.Deck deck)
        {
            EditDeckRequest request = new EditDeckRequest
            {
                UserId = userId,
                Deck = deck.ToProtobuf(),
                Version = BackendEndpoint.DataVersion
            };

            await _contractCallProxy.CallAsync(EditDeckMethod, request);
        }

        public async Task<long> AddDeck(string userId, Data.Deck deck)
        {
            CreateDeckRequest request = new CreateDeckRequest
            {
                UserId = userId,
                Deck = deck.ToProtobuf(),
                Version = BackendEndpoint.DataVersion
            };

            CreateDeckResponse createDeckResponse = await _contractCallProxy.CallAsync<CreateDeckResponse>(AddDeckMethod, request);
            return createDeckResponse.DeckId;
        }

        #endregion

        #region Overlords

        private const string ListOverlordUserInstancesMethod = "ListOverlordUserInstances";
        private const string GetOverlordUserInstanceMethod = "GetOverlordUserInstance";

        public async Task<ListOverlordUserInstancesResponse> ListOverlordUserInstances(string userId)
        {
            ListOverlordUserInstancesRequest request = new ListOverlordUserInstancesRequest
            {
                UserId = userId,
                Version = BackendEndpoint.DataVersion
            };

            return await _contractCallProxy.StaticCallAsync<ListOverlordUserInstancesResponse>(ListOverlordUserInstancesMethod, request);
        }

        public async Task<GetOverlordUserInstanceResponse> GetOverlordUserInstance(string userId, OverlordId overlordId)
        {
            GetOverlordUserInstanceRequest request = new GetOverlordUserInstanceRequest
            {
                UserId = userId,
                OverlordId = overlordId.Id,
                Version = BackendEndpoint.DataVersion
            };

            return await _contractCallProxy.StaticCallAsync<GetOverlordUserInstanceResponse>(GetOverlordUserInstanceMethod, request);
        }

        private const string GetOverlordLevelingDataMethod = "GetOverlordLevelingData";

        public async Task<GetOverlordLevelingDataResponse> GetOverlordLevelingData()
        {
            GetOverlordLevelingDataRequest request = new GetOverlordLevelingDataRequest
            {
                Version = BackendEndpoint.DataVersion
            };

            return await _contractCallProxy.StaticCallAsync<GetOverlordLevelingDataResponse>(GetOverlordLevelingDataMethod, request);
        }

        #endregion

        #region Login

        private const string CreateAccountMethod = "CreateAccount";
        private const string LoginMethod = "Login";

        public async Task SignUp(string userId)
        {
            UpsertAccountRequest req = new UpsertAccountRequest
            {
                Version = BackendEndpoint.DataVersion,
                UserId = userId
            };

            await _contractCallProxy.CallAsync(CreateAccountMethod, req);
        }

        public async Task Login(string userId)
        {
            Protobuf.LoginRequest req = new Protobuf.LoginRequest
            {
                Version = BackendEndpoint.DataVersion,
                UserId = userId
            };

            await _contractCallProxy.CallAsync(LoginMethod, req);
        }

        #endregion

        #region PVP

        private const string FindMatchMethod = "FindMatch";
        private const string CancelFindMatchMethod = "CancelFindMatch";
        private const string EndMatchMethod = "EndMatch";
        private const string SendPlayerActionMethod = "SendPlayerAction";
        private const string GetGameStateMethod = "GetGameState";
        private const string GetMatchMethod = "GetMatch";
        private const string RegisterPlayerPoolMethod = "RegisterPlayerPool";
        private const string AddSoloExperienceMethod = "AddSoloExperience";
        private const string AcceptMatchMethod = "AcceptMatch";
        private const string KeepAliveStatusMethod = "KeepAlive";

        public event PlayerActionDataReceivedHandler PlayerActionDataReceived;

        public async Task<AcceptMatchResponse> AcceptMatch(string userId, long matchId)
        {
            AcceptMatchRequest request = new AcceptMatchRequest
            {
                UserId = userId,
                MatchId = matchId
            };

            return await _contractCallProxy.CallAsync<AcceptMatchResponse>(AcceptMatchMethod, request);
        }

        public async Task<RegisterPlayerPoolResponse> RegisterPlayerPool(
            string userId,
            DeckId deckId,
            Address? customGameModeAddress,
            IList<string> pvpTags,
            bool useBackendGameLogic,
            DebugCheatsConfiguration debugCheats = null)
        {
            RegisterPlayerPoolRequest request = new RegisterPlayerPoolRequest
            {
                RegistrationData = new PlayerProfileRegistrationData
                {
                    UserId = userId,
                    DeckId = deckId.Id,
                    Version = BackendEndpoint.DataVersion,
                    Tags =
                    {
                        pvpTags ?? Array.Empty<string>()
                    },
                    CustomGame = customGameModeAddress?.ToProtobufAddress(),
                    UseBackendGameLogic = useBackendGameLogic,
                    DebugCheats = debugCheats.ToProtobuf()
                }
            };

            return await _contractCallProxy.CallAsync<RegisterPlayerPoolResponse>(RegisterPlayerPoolMethod, request);
        }

        public async Task<FindMatchResponse> FindMatch(string userId, IList<string> pvpTags)
        {
            FindMatchRequest request = new FindMatchRequest
            {
                UserId = userId,
                Tags =
                {
                    pvpTags ?? Array.Empty<string>()
                }
            };

            return await _contractCallProxy.CallAsync<FindMatchResponse>(FindMatchMethod, request);
        }

        public async Task<CancelFindMatchResponse> CancelFindMatch(string userId, long matchId)
        {
            CancelFindMatchRequest request = new CancelFindMatchRequest
            {
                UserId = userId,
                MatchId = matchId
            };

            return await _contractCallProxy.CallAsync<CancelFindMatchResponse>(CancelFindMatchMethod, request);
        }

        public async Task<CancelFindMatchResponse> CancelFindMatchRelatedToUserId(string userId)
        {
            CancelFindMatchRequest request = new CancelFindMatchRequest
            {
                UserId = userId
            };

            return await _contractCallProxy.CallAsync<CancelFindMatchResponse>(CancelFindMatchMethod, request);
        }

        public async Task<GetGameStateResponse> GetGameState(long matchId)
        {
            GetGameStateRequest request = new GetGameStateRequest
            {
                MatchId = matchId
            };

            return await _contractCallProxy.StaticCallAsync<GetGameStateResponse>(GetGameStateMethod, request);
        }

        public async Task<GetMatchResponse> GetMatch(long matchId)
        {
            GetMatchRequest request = new GetMatchRequest
            {
                MatchId = matchId
            };

            return await _contractCallProxy.StaticCallAsync<GetMatchResponse>(GetMatchMethod, request);
        }

        public async Task SubscribeToEvents(IList<string> topics)
        {
            await UnsubscribeFromAllEvents();
            Contract.EventReceived += EventHandler;
            await Contract.Client.SubscribeToEvents(topics);
        }

        public async Task UnsubscribeFromAllEvents()
        {
            Contract.EventReceived -= EventHandler;
            try
            {
                await Contract.Client.UnsubscribeFromAllEvents();
            }
            catch (RpcClientException rpcClientException) when (rpcClientException.Message.Contains("Subscription not found"))
            {
            }
            catch (Exception e)
            {
                Helpers.ExceptionReporter.SilentReportException(e);
                GameClient.Get<IAppStateManager>().HandleNetworkExceptionFlow(e);
            }
        }

        public async Task SendPlayerAction(PlayerActionRequest request)
        {
            await _contractCallProxy.CallAsync(SendPlayerActionMethod, request);
        }

        public async Task SendEndMatchRequest(EndMatchRequest request)
        {
            await _contractCallProxy.CallAsync(EndMatchMethod, request);
        }

        public async Task<KeepAliveResponse> KeepAliveStatus(string userId, long matchId)
        {
            KeepAliveRequest request = new KeepAliveRequest
            {
                MatchId = matchId,
                UserId = userId
            };

            return await _contractCallProxy.CallAsync<KeepAliveResponse>(KeepAliveStatusMethod, request);
        }

        public async Task<AddSoloExperienceResponse> AddSoloExperience(
            string userId,
            OverlordId overlordId,
            DeckId deckId,
            long experience,
            bool isWin
            )
        {
            AddSoloExperienceRequest request = new AddSoloExperienceRequest
            {
                Version = BackendEndpoint.DataVersion,
                UserId = userId,
                OverlordId = overlordId.Id,
                Experience = experience,
                IsWin = isWin,
                DeckId = deckId.Id
            };

            return await _contractCallProxy.CallAsync<AddSoloExperienceResponse>(AddSoloExperienceMethod, request);
        }

        //attempt to implement a one message action policy
        private byte[] _previousEventData;

        private void EventHandler(object sender, RawChainEventArgs rawChainEventArgs)
        {
            if (_previousEventData == null || !_previousEventData.SequenceEqual(rawChainEventArgs.Data)) {
                _previousEventData = rawChainEventArgs.Data;
                PlayerActionDataReceived?.Invoke(rawChainEventArgs.Data);
            }
        }

#endregion

        #region Custom Game Modes

        private const string ListGameModesMethod = "ListGameModes";
        private const string CallCustomGameModeFunctionMethod = "CallCustomGameModeFunction";
        private const string GetGameModeCustomUiMethod = "GetGameModeCustomUi";

        public async Task<GameModeList> GetCustomGameModeList()
        {
            ListGameModesRequest request = new ListGameModesRequest();
            return await _contractCallProxy.StaticCallAsync<GameModeList>(ListGameModesMethod, request);
        }

        public async Task<GetCustomGameModeCustomUiResponse> GetGameModeCustomUi(Address address)
        {
            GetCustomGameModeCustomUiRequest request = new GetCustomGameModeCustomUiRequest
            {
                Address = address.ToProtobufAddress()
            };

            return await _contractCallProxy.StaticCallAsync<GetCustomGameModeCustomUiResponse>(GetGameModeCustomUiMethod, request);
        }

        public async Task CallCustomGameModeFunction(Address address, byte[] callData)
        {
            CallCustomGameModeFunctionRequest request = new CallCustomGameModeFunctionRequest
            {
                Address = address.ToProtobufAddress(),
                CallData = ByteString.CopyFrom(callData)
            };

            await _contractCallProxy.CallAsync(CallCustomGameModeFunctionMethod, request);
        }

#endregion

        #region Notifications
        private const string GetNotificationsMethod = "GetNotifications";
        private const string ClearNotificationsMethod = "ClearNotifications";

        public async Task<GetNotificationsResponse> GetNotifications(string userId)
        {
            GetNotificationsRequest request = new GetNotificationsRequest
            {
                UserId = userId
            };

            return await _contractCallProxy.StaticCallAsync<GetNotificationsResponse>(GetNotificationsMethod, request);
        }

        public async Task<ClearNotificationsResponse> ClearNotifications(string userId, IEnumerable<int> notificationIds)
        {
            ClearNotificationsRequest request = new ClearNotificationsRequest
            {
                UserId = userId,
                NotificationIds = { notificationIds }
            };

            return await _contractCallProxy.CallAsync<ClearNotificationsResponse>(ClearNotificationsMethod, request);
        }

        #endregion

        #region Rewards

        public async Task<GetPendingMintingTransactionReceiptsResponse> GetPendingMintingTransactionReceipts(string userId)
        {
            GetPendingMintingTransactionReceiptsRequest request = new GetPendingMintingTransactionReceiptsRequest
            {
                UserId = userId,
            };

            return await _contractCallProxy.StaticCallAsync<GetPendingMintingTransactionReceiptsResponse>("GetPendingMintingTransactionReceipts", request);
        }

        public async Task ConfirmPendingMintingTransactionReceipt(string userId, BigInteger txId)
        {
            ConfirmPendingMintingTransactionReceiptRequest request = new ConfirmPendingMintingTransactionReceiptRequest
            {
                UserId = userId,
                TxId = txId.ToProtobufUInt()
            };

            await _contractCallProxy.CallAsync("ConfirmPendingMintingTransactionReceipt", request);
        }

        #endregion

        #region Debug

        public async Task<string> DebugGetUserIdByAddress(Address address)
        {
            DebugGetUserIdByAddressRequest request = new DebugGetUserIdByAddressRequest
            {
                Address = address.ToProtobufAddress()
            };

            return (await _contractCallProxy.StaticCallAsync<UserIdContainer>("DebugGetUserIdByAddress", request)).UserId;
        }

        public async Task<DebugGetPendingCardAmountChangeItemsResponse> DebugGetPendingCardAmountChangeItems(Address address)
        {
            DebugGetPendingCardAmountChangeItemsRequest request = new DebugGetPendingCardAmountChangeItemsRequest
            {
                Address = address.ToProtobufAddress()
            };

            return await _contractCallProxy.StaticCallAsync<DebugGetPendingCardAmountChangeItemsResponse>("DebugGetPendingCardAmountChangeItems", request);
        }

        public async Task<DebugMintBoosterPackReceiptResponse> DebugMintBoosterPackReceipt(BigInteger userId, int boosterAmount)
        {
            DebugMintBoosterPackReceiptRequest request = new DebugMintBoosterPackReceiptRequest
            {
                UserId = userId.ToProtobufUInt(),
                BoosterAmount = boosterAmount
            };

            return await _contractCallProxy.CallAsync<DebugMintBoosterPackReceiptResponse>("DebugMintBoosterPackReceipt", request);
        }

        #endregion
    }
}
