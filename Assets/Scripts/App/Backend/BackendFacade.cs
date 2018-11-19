using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Loom.Client;
using Loom.Google.Protobuf;
using Loom.Google.Protobuf.Collections;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.Protobuf;
using Newtonsoft.Json;
using Plugins.AsyncAwaitUtil.Source;
using UnityEngine;
using Deck = Loom.ZombieBattleground.Protobuf.Deck;

namespace Loom.ZombieBattleground.BackendCommunication
{
    public class BackendFacade : IService
    {
        private int _subscribeCount;

        public int SubscribeCount
        {
            get { return _subscribeCount; }
        }

        public delegate void ContractCreatedEventHandler(Contract oldContract, Contract newContract);

        public delegate void PlayerActionDataReceivedHandler(byte[] bytes);

        public BackendFacade(BackendEndpoint backendEndpoint)
        {
            BackendEndpoint = backendEndpoint;
        }

        public event ContractCreatedEventHandler ContractCreated;

        public BackendEndpoint BackendEndpoint { get; set; }

        public Contract Contract { get; private set; }

        public bool IsConnected => Contract != null &&
            Contract.Client.ReadClient.ConnectionState == RpcConnectionState.Connected &&
            Contract.Client.WriteClient.ConnectionState == RpcConnectionState.Connected;

        private IRpcClient reader;

        public void Init()
        {
            Debug.Log("Auth Host: " + BackendEndpoint.AuthHost);
            Debug.Log("Reader Host: " + BackendEndpoint.ReaderHost);
            Debug.Log("Writer Host: " + BackendEndpoint.WriterHost);
            Debug.Log("Card Data Version: " + BackendEndpoint.DataVersion);
        }

        public string DAppChainWalletAddress = string.Empty;

        public void Update()
        {
        }

        public void Dispose()
        {
        }

        public async Task CreateContract(byte[] privateKey)
        {
            byte[] publicKey = CryptoUtils.PublicKeyFromPrivateKey(privateKey);
            Address callerAddr = Address.FromPublicKey(publicKey);

#if DEBUG_RPC
            ILogger logger = Debug.unityLogger;
#else
            ILogger logger = NullLogger.Instance;
#endif

            IRpcClient writer =
                RpcClientFactory
                    .Configure()
                    .WithLogger(logger)
                    .WithWebSocket(BackendEndpoint.WriterHost)
                    .Create();

            reader =
                RpcClientFactory
                    .Configure()
                    .WithLogger(logger)
                    .WithWebSocket(BackendEndpoint.ReaderHost)
                    .Create();

            DAppChainClient client = new DAppChainClient(writer, reader)
            {
                Logger = logger
            };

            client.TxMiddleware = new TxMiddleware(new ITxMiddlewareHandler[]
            {
                new NonceTxMiddleware(publicKey, client), new SignedTxMiddleware(privateKey)
            });

            client.Configuration.AutoReconnect = false;

            await client.ReadClient.ConnectAsync();
            await client.WriteClient.ConnectAsync();
            Address contractAddr = await client.ResolveContractAddressAsync("ZombieBattleground");
            Contract oldContract = Contract;
            Contract = new Contract(client, contractAddr, callerAddr);
            ContractCreated?.Invoke(oldContract, Contract);
        }

        #region Card Collection

        private const string GetCardCollectionMethod = "GetCollection";

        public async Task<GetCollectionResponse> GetCardCollection(string userId)
        {
            GetCollectionRequest request = new GetCollectionRequest
            {
                UserId = userId
            };

            return await Contract.StaticCallAsync<GetCollectionResponse>(GetCardCollectionMethod, request);
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

            return await Contract.StaticCallAsync<ListCardLibraryResponse>(GetCardLibraryMethod, request);
        }

        #endregion

        #region Deck Management

        private const string GetAiDecksDataMethod = "GetAIDecks";

        private const string GetDeckDataMethod = "ListDecks";

        private const string DeleteDeckMethod = "DeleteDeck";

        private const string AddDeckMethod = "CreateDeck";

        private const string EditDeckMethod = "EditDeck";

        public async Task<ListDecksResponse> GetDecks(string userId)
        {
            ListDecksRequest request = new ListDecksRequest
            {
                UserId = userId
            };

            return await Contract.StaticCallAsync<ListDecksResponse>(GetDeckDataMethod, request);
        }

        public async Task<GetAIDecksResponse> GetAiDecks()
        {
            GetAIDecksRequest request = new GetAIDecksRequest
            {
                Version = BackendEndpoint.DataVersion
            };

            return await Contract.StaticCallAsync<GetAIDecksResponse>(GetAiDecksDataMethod, request);
        }

        public async Task DeleteDeck(string userId, long deckId)
        {
            DeleteDeckRequest request = new DeleteDeckRequest
            {
                UserId = userId,
                DeckId = deckId
            };

            await Contract.CallAsync(DeleteDeckMethod, request);
        }

        public async Task EditDeck(string userId, Data.Deck deck)
        {
            EditDeckRequest request = new EditDeckRequest
            {
                UserId = userId,
                Deck = deck.ToProtobuf(),
                Version = BackendEndpoint.DataVersion
            };

            await Contract.CallAsync(EditDeckMethod, request);
        }

        public async Task<long> AddDeck(string userId, Data.Deck deck)
        {
            CreateDeckRequest request = new CreateDeckRequest
            {
                UserId = userId,
                Deck = deck.ToProtobuf(),
                Version = BackendEndpoint.DataVersion
            };

            CreateDeckResponse createDeckResponse = await Contract.CallAsync<CreateDeckResponse>(AddDeckMethod, request);
            return createDeckResponse.DeckId;
        }

        #endregion

        #region Heroes

        private const string HeroesList = "ListHeroes";

        public async Task<ListHeroesResponse> GetHeroesList(string userId)
        {
            ListHeroesRequest request = new ListHeroesRequest
            {
                UserId = userId
            };

            return await Contract.StaticCallAsync<ListHeroesResponse>(HeroesList, request);
        }

        private const string GlobalHeroesList = "ListHeroLibrary";

        public async Task<ListHeroLibraryResponse> GetGlobalHeroesList()
        {
            ListHeroLibraryRequest request = new ListHeroLibraryRequest
            {
                Version = BackendEndpoint.DataVersion
            };

            return await Contract.StaticCallAsync<ListHeroLibraryResponse>(GlobalHeroesList, request);
        }

        #endregion

        #region Login

        private const string CreateAccountMethod = "CreateAccount";

        public async Task SignUp(string userId)
        {
            UpsertAccountRequest req = new UpsertAccountRequest
            {
                Version = BackendEndpoint.DataVersion,
                UserId = userId
            };

            await Contract.CallAsync(CreateAccountMethod, req);
        }

        #endregion

        #region Auth

        private const string AuthBetaKeyValidationEndPoint = "/user/beta/validKey";

        private const string AuthBetaConfigEndPoint = "/user/beta/config";

        public async Task<bool> CheckIfBetaKeyValid(string betaKey)
        {
            WebrequestCreationInfo webrequestCreationInfo = new WebrequestCreationInfo();
            webrequestCreationInfo.Url = BackendEndpoint.AuthHost + AuthBetaKeyValidationEndPoint + "?beta_key=" + betaKey;
            HttpResponseMessage httpResponseMessage =
                await WebRequestUtils.CreateAndSendWebrequest(webrequestCreationInfo);
            if (!httpResponseMessage.IsSuccessStatusCode)
                throw new Exception(
                    $"{nameof(CheckIfBetaKeyValid)} failed with error code {httpResponseMessage.StatusCode}");

            BetaKeyValidationResponse betaKeyValidationResponse =
                httpResponseMessage.DeserializeAsJson<BetaKeyValidationResponse>();
            return betaKeyValidationResponse.IsValid;
        }

        public async Task<BetaConfig> GetBetaConfig(string betaKey)
        {
            WebrequestCreationInfo webrequestCreationInfo = new WebrequestCreationInfo();
            webrequestCreationInfo.Url = BackendEndpoint.AuthHost + AuthBetaConfigEndPoint + "?beta_key=" + betaKey;
            HttpResponseMessage httpResponseMessage =
                await WebRequestUtils.CreateAndSendWebrequest(webrequestCreationInfo);
            if (!httpResponseMessage.IsSuccessStatusCode)
                throw new Exception($"{nameof(GetBetaConfig)} failed with error code {httpResponseMessage.StatusCode}");

            BetaConfig betaConfig = JsonConvert.DeserializeObject<BetaConfig>(
                httpResponseMessage.ReadToEnd(),

                // FIXME: backend should return valid version numbers at all times
                new VersionConverterWithFallback(Version.Parse(Constants.CurrentVersionBase)));
            return betaConfig;
        }

        private struct BetaKeyValidationResponse
        {
            [JsonProperty(PropertyName = "is_valid")]
            public bool IsValid;
        }

        #endregion

        #region PVP

        private const string FindMatchMethod = "FindMatch";
        private const string DebugFindMatchMethod = "DebugFindMatch";
        private const string CancelFindMatchMethod = "CancelFindMatch";
        private const string EndMatchMethod = "EndMatch";
        private const string SendPlayerActionMethod = "SendPlayerAction";
        private const string GetGameStateMethod = "GetGameState";
        private const string GetMatchMethod = "GetMatch";
        private const string CheckGameStatusMethod = "CheckGameStatus";
        private const string RegisterPlayerPoolMethod = "RegisterPlayerPool";
        private const string AcceptMatchMethod = "AcceptMatch";

        public PlayerActionDataReceivedHandler PlayerActionDataReceived;

        public async Task<AcceptMatchResponse> AcceptMatch(string userId, long matchId)
        {
            AcceptMatchRequest request = new AcceptMatchRequest
            {
                UserId = userId,
                MatchId = matchId
            };

            return await Contract.CallAsync<AcceptMatchResponse>(AcceptMatchMethod, request);
        }

        public async Task<RegisterPlayerPoolResponse> RegisterPlayerPool(string userId, long deckId, Address? customGameModeAddress)
        {
            RegisterPlayerPoolRequest request = new RegisterPlayerPoolRequest
            {
                UserId = userId,
                DeckId = deckId,
                Version = BackendEndpoint.DataVersion,
                RandomSeed = (long)Time.time,
                Tags = { },
                CustomGame = customGameModeAddress?.ToProtobufAddress()
            };

            return await Contract.CallAsync<RegisterPlayerPoolResponse>(RegisterPlayerPoolMethod, request);
        }

        public async Task<FindMatchResponse> FindMatch(string userId)
        {
            FindMatchRequest request = new FindMatchRequest
            {
                UserId = userId
            };

            return await Contract.CallAsync<FindMatchResponse>(FindMatchMethod, request);
        }

        public async Task<FindMatchResponse> DebugFindMatch(string userId, Loom.ZombieBattleground.Data.Deck deck, Address? customGameModeAddress)
        {
            Client.Protobuf.Address requestCustomGameAddress = null;
            if (customGameModeAddress != null)
            {
                requestCustomGameAddress = customGameModeAddress.Value.ToProtobufAddress();
            }

            DebugFindMatchRequest request = new DebugFindMatchRequest
            {
                UserId = userId,
                Deck = deck.GetDeck(),
                CustomGame = requestCustomGameAddress,
                Version = BackendEndpoint.DataVersion
            };

            return await Contract.CallAsync<FindMatchResponse>(DebugFindMatchMethod, request);
        }

        public async Task<CancelFindMatchResponse> CancelFindMatch(string userId, long matchId)
        {
            CancelFindMatchRequest request = new CancelFindMatchRequest
            {
                UserId = userId,
                MatchId = matchId
            };

            return await Contract.CallAsync<CancelFindMatchResponse>(CancelFindMatchMethod, request);
        }

        public async Task<CancelFindMatchResponse> CancelFindMatchRelatedToUserId(string userId)
        {
            CancelFindMatchRequest request = new CancelFindMatchRequest
            {
                UserId = userId
            };

            return await Contract.CallAsync<CancelFindMatchResponse>(CancelFindMatchMethod, request);
        }

        public async Task<GetGameStateResponse> GetGameState(long matchId)
        {
            GetGameStateRequest request = new GetGameStateRequest
            {
                MatchId = matchId
            };

            return await Contract.StaticCallAsync<GetGameStateResponse>(GetGameStateMethod, request);
        }

        public async Task<GetMatchResponse> GetMatch(long matchId)
        {
            GetMatchRequest request = new GetMatchRequest
            {
                MatchId = matchId
            };

            return await Contract.StaticCallAsync<GetMatchResponse>(GetMatchMethod, request);
        }

        public async Task SubscribeEvent(List<string> topics)
         {
            //TODO Remove the logs once we fix the multiple subscription issue once and for all
            Debug.Log("Subscribing to Event - Current Subscriptions = " + _subscribeCount);
            for (int i = _subscribeCount; i > 0; i--) {
                await UnsubscribeEvent();
            }

            await reader.SubscribeAsync(EventHandler, topics);
            _subscribeCount++;
            Debug.Log("Final Subscriptions = " + _subscribeCount);
        }

         public async Task UnsubscribeEvent()
         {
            //TODO Remove the logs once we fix the multiple subscription issue once and for all
            if (_subscribeCount > 0)
            {
                Debug.Log("Unsubscribing from Event - Current Subscriptions = " + _subscribeCount);
                await reader.UnsubscribeAsync(EventHandler);
                _subscribeCount--;
                Debug.Log("Final Subscriptions = " + _subscribeCount);
            } 
            else 
            {
                Debug.Log("Tried to Unsubscribe, count <= 0 = " + _subscribeCount);
            }
            GameClient.Get<IQueueManager>().Clear();
        }

        public void EventHandler(object sender, JsonRpcEventData e)
        {
            PlayerActionDataReceived?.Invoke(e.Data);
        }

        public void AddAction(long matchId, PlayerAction playerAction)
        {
            PlayerActionRequest request = new PlayerActionRequest
            {
                MatchId = matchId,
                PlayerAction = playerAction
            };

            GameClient.Get<IQueueManager>().AddAction(request);
        }

        public void EndMatch(string userId, int matchId, string winnerId)
        {
            EndMatchRequest request = new EndMatchRequest
            {
                UserId = userId,
                MatchId = matchId,
                WinnerId = winnerId
            };

            GameClient.Get<IQueueManager>().AddAction(request);
        }

        public async Task SendAction(IMessage request)
        {
            switch (request)
            {
                case PlayerActionRequest playerActionMessage:
                    await Contract.CallAsync(SendPlayerActionMethod, playerActionMessage);
                    break;

                case EndMatchRequest endMatchMessage:
                    await Contract.CallAsync(EndMatchMethod, endMatchMessage);
                    break;
            }
        }

        public async Task<CheckGameStatusResponse> CheckPlayerStatus(long matchId)
        {
            CheckGameStatusRequest request = new CheckGameStatusRequest
            {
                MatchId = matchId
            };

            return await Contract.CallAsync<CheckGameStatusResponse>(CheckGameStatusMethod, request);
        }

        #endregion

        #region Custom Game Modes

        private const string ListGameModesMethod = "ListGameModes";
        private const string CallCustomGameModeFunctionMethod = "CallCustomGameModeFunction";
        private const string GetGameModeCustomUiMethod = "GetGameModeCustomUi";

        public async Task<GameModeList> GetCustomGameModeList()
        {
            ListGameModesRequest request = new ListGameModesRequest();
            return await Contract.StaticCallAsync<GameModeList>(ListGameModesMethod, request);
        }

        public async Task<GetCustomGameModeCustomUiResponse> GetGameModeCustomUi(Address address)
        {
            GetCustomGameModeCustomUiRequest request = new GetCustomGameModeCustomUiRequest
            {
                Address = address.ToProtobufAddress()
            };

            return await Contract.StaticCallAsync<GetCustomGameModeCustomUiResponse>(GetGameModeCustomUiMethod, request);
        }

        public async Task CallCustomGameModeFunction(Address address, byte[] callData)
        {
            CallCustomGameModeFunctionRequest request = new CallCustomGameModeFunctionRequest
            {
                Address = address.ToProtobufAddress(),
                CallData = ByteString.CopyFrom(callData)
            };

            await Contract.CallAsync(CallCustomGameModeFunctionMethod, request);
        }

        #endregion
    }
}
