using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Loom.Client;
using Loom.Google.Protobuf;
using Loom.Google.Protobuf.Collections;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Protobuf;
using Newtonsoft.Json;
using Plugins.AsyncAwaitUtil.Source;
using UnityEngine;
using UnityEngine.Events;

namespace Loom.ZombieBattleground.BackendCommunication
{
    public class BackendFacade : IService
    {
        public delegate void ContractCreatedEventHandler(Contract oldContract, Contract newContract);

        public delegate void PlayerActionHandler(byte[] bytes);

        private const string ContractDataVersion = "v1";

        public BackendFacade(string authBackendHost, string readerHost, string writerHost)
        {
            AuthBackendHost = authBackendHost;
            ReaderHost = readerHost;
            WriterHost = writerHost;
        }

        public event ContractCreatedEventHandler ContractCreated;

        public event PlayerActionHandler PlayerActionEvent;

        public string ReaderHost { get; set; }

        public string WriterHost { get; set; }

        public string AuthBackendHost { get; set; }

        public Contract Contract { get; private set; }

        private IRpcClient reader;

        public bool IsConnected => Contract != null &&
            Contract.Client.ReadClient.ConnectionState == RpcConnectionState.Connected &&
            Contract.Client.WriteClient.ConnectionState == RpcConnectionState.Connected;

        public void Init()
        {
        }

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

            IRpcClient writer = RpcClientFactory.Configure().WithLogger(Debug.unityLogger).WithWebSocket(WriterHost)
                .Create();

            reader = RpcClientFactory.Configure().WithLogger(Debug.unityLogger).WithWebSocket(ReaderHost)
                .Create();

            DAppChainClient client = new DAppChainClient(writer, reader)
            {
                Logger = Debug.unityLogger
            };

            client.TxMiddleware = new TxMiddleware(new ITxMiddlewareHandler[]
            {
                new NonceTxMiddleware(publicKey, client), new SignedTxMiddleware(privateKey)
            });

            client.AutoReconnect = false;

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
                Version = GameClient.Get<IDataManager>().CachedConfigData.cardsDataVersion
            };

            return await Contract.StaticCallAsync<ListCardLibraryResponse>(GetCardLibraryMethod, request);
        }

        #endregion

        #region Deck Management

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

        public async Task DeleteDeck(string userId, long deckId, long lastModificationTimestamp)
        {
            DeleteDeckRequest request = new DeleteDeckRequest
            {
                UserId = userId,
                DeckId = deckId,
                LastModificationTimestamp = lastModificationTimestamp
            };

            await Contract.CallAsync(DeleteDeckMethod, request);
        }

        public async Task EditDeck(string userId, Data.Deck deck, long lastModificationTimestamp)
        {
            EditDeckRequest request = EditDeckRequest(userId, deck, lastModificationTimestamp);

            await Contract.CallAsync(EditDeckMethod, request);
        }

        public async Task<long> AddDeck(string userId, Data.Deck deck, long lastModificationTimestamp)
        {
            RepeatedField<CardCollection> cards = new RepeatedField<CardCollection>();

            for (int i = 0; i < deck.Cards.Count; i++)
            {
                CardCollection cardInCollection = new CardCollection
                {
                    CardName = deck.Cards[i].CardName,
                    Amount = deck.Cards[i].Amount
                };
                Debug.Log("Card in collection = " + cardInCollection.CardName + " , " + cardInCollection.Amount);
                cards.Add(cardInCollection);
            }

            CreateDeckRequest request = new CreateDeckRequest
            {
                UserId = userId,
                Deck = new Deck
                {
                    Name = deck.Name,
                    HeroId = deck.HeroId,
                    Cards =
                    {
                        cards
                    }
                },
                LastModificationTimestamp = lastModificationTimestamp,
                Version = GameClient.Get<IDataManager>().CachedConfigData.cardsDataVersion
            };

            CreateDeckResponse createDeckResponse = await Contract.CallAsync<CreateDeckResponse>(AddDeckMethod, request);
            return createDeckResponse.DeckId;
        }

        private static EditDeckRequest EditDeckRequest(string userId, Data.Deck deck, long lastModificationTimestamp)
        {
            RepeatedField<CardCollection> cards = new RepeatedField<CardCollection>();

            for (int i = 0; i < deck.Cards.Count; i++)
            {
                CardCollection cardInCollection = new CardCollection
                {
                    CardName = deck.Cards[i].CardName,
                    Amount = deck.Cards[i].Amount
                };
                Debug.Log("Card in collection = " + cardInCollection.CardName + " , " + cardInCollection.Amount);
                cards.Add(cardInCollection);
            }

            EditDeckRequest request = new EditDeckRequest
            {
                UserId = userId,
                Deck = new Deck
                {
                    Id = deck.Id,
                    Name = deck.Name,
                    HeroId = deck.HeroId,
                    Cards =
                    {
                        cards
                    }
                },
                LastModificationTimestamp = lastModificationTimestamp,
                Version = GameClient.Get<IDataManager>().CachedConfigData.cardsDataVersion
            };
            return request;
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

        #endregion

        #region Login

        private const string CreateAccountMethod = "CreateAccount";

        public async Task SignUp(string userId)
        {
            UpsertAccountRequest req = new UpsertAccountRequest
            {
                Version = ContractDataVersion,
                UserId = userId
            };

            await Contract.CallAsync(CreateAccountMethod, req);
        }

        #endregion

        #region Turn Logs

        private const string UploadActionLogMethod = "UploadHistory"; // just a random method for now

        public async Task UploadActionLog(string userId, ActionLogModel actionLogModel)
        {
            string actionLogModelJson = JsonConvert.SerializeObject(actionLogModel, Formatting.Indented);
            Dictionary<string, object> actionLogModelJsonDictionary =
                JsonConvert.DeserializeObject<Dictionary<string, object>>(actionLogModelJson);
            actionLogModelJson =
                JsonConvert.SerializeObject(actionLogModelJsonDictionary[nameof(ActionLogModel.LogData)],
                    Formatting.Indented);
            await Task.Delay(1000);
        }

        #endregion

        #region Auth

        private const string AuthBetaKeyValidationEndPoint = "/user/beta/validKey";

        private const string AuthBetaConfigEndPoint = "/user/beta/config";

        public async Task<bool> CheckIfBetaKeyValid(string betaKey)
        {
            WebrequestCreationInfo webrequestCreationInfo = new WebrequestCreationInfo();
            webrequestCreationInfo.Url = AuthBackendHost + AuthBetaKeyValidationEndPoint + "?beta_key=" + betaKey;
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
            webrequestCreationInfo.Url = AuthBackendHost + AuthBetaConfigEndPoint + "?beta_key=" + betaKey;
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
         private const string EndMatchMethod = "EndMatch";
         private const string SendPlayerActionMethod = "SendPlayerAction";
         private const string GetGameStateMethod = "GetGameState";

         public UnityAction<byte[]> PlayerActionEventListner;

         public async Task<FindMatchResponse> FindMatch(string userId, long deckId, Address? customGameModeAddress)
         {
             Client.Protobuf.Address requestCustomGameAddress = null;
             if (customGameModeAddress != null)
             {
                 requestCustomGameAddress = GetProtobufAddressFromAddress(customGameModeAddress.Value);
             }
             FindMatchRequest request = new FindMatchRequest
             {
                 UserId = userId,
                 DeckId = deckId,
                 CustomGame = requestCustomGameAddress
             };

            const int timeout = 120000;

            return await Contract.CallAsync<FindMatchResponse>(FindMatchMethod, request, timeout);
         }

        public async Task<EndMatchResponse> EndMatch(string userId, int matchId, string winnerId)
        {
            EndMatchRequest request = new EndMatchRequest
            {
                UserId = userId,
                MatchId = matchId,
                WinnerId = winnerId
            };

            return await Contract.CallAsync<EndMatchResponse>(EndMatchMethod, request);
        }


         public async Task<GetGameStateResponse> GetGameState(int matchId)
         {
             GetGameStateRequest request = new GetGameStateRequest
             {
                 MatchId = matchId
             };

             return await Contract.CallAsync<GetGameStateResponse>(GetGameStateMethod, request);
         }

        public void SubscribeEvent(List<string> topics)
         {
             EventHandler<JsonRpcEventData> handler = (sender, e) =>
             {
                 PlayerActionEventListner?.Invoke(e.Data);
             };
             reader.SubscribeAsync(handler, topics);
         }

         public void UnSubscribeEvent()
         {
             EventHandler<JsonRpcEventData> handler = (sender, e) =>{ };
             reader.UnsubscribeAsync(handler);

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

        public async Task SendAction(PlayerActionRequest request)
        {
            await Contract.CallAsync(SendPlayerActionMethod, request);
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
                Address = GetProtobufAddressFromAddress(address)
            };

            return await Contract.StaticCallAsync<GetCustomGameModeCustomUiResponse>(GetGameModeCustomUiMethod, request);
        }

        public async Task CallCustomGameModeFunction(Address address, string functionName)
        {
            CallCustomGameModeFunctionRequest request = new CallCustomGameModeFunctionRequest
            {
                Address = GetProtobufAddressFromAddress(address),
                FunctionName = functionName
            };

            await Contract.CallAsync(CallCustomGameModeFunctionMethod, request);
        }

        #endregion

        private static Client.Protobuf.Address GetProtobufAddressFromAddress(Address address)
        {
            Client.Protobuf.Address protobufAddress;
            protobufAddress = new Client.Protobuf.Address
            {
                ChainId = address.ChainId,
                Local = ByteString.CopyFrom(address.ToByteArray())
            };
            return protobufAddress;
        }
    }
}
