using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Loom.Client;
using Loom.Google.Protobuf.Collections;
using Loom.Newtonsoft.Json;
using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Protobuf;
using Plugins.AsyncAwaitUtil.Source;
using UnityEngine;
using Deck = LoomNetwork.CZB.Data.Deck;
using ProtobufDeck = LoomNetwork.CZB.Protobuf.Deck;

namespace LoomNetwork.CZB.BackendCommunication
{
    public class BackendFacade : IService
    {
        public delegate void ContractCreatedEventHandler(Contract oldContract, Contract newContract);

        public event ContractCreatedEventHandler ContractCreated;

        public BackendFacade(string authBackendHost, string readerHost, string writerHost)
        {
            AuthBackendHost = authBackendHost;
            ReaderHost = readerHost;
            WriterHost = writerHost;

            Debug.Log($"Using auth backend {AuthBackendHost}");
            Debug.Log($"Using writer host {WriterHost}, reader host {ReaderHost}");
        }

        public string ReaderHost { get; set; }

        public string WriterHost { get; set; }

        public string AuthBackendHost { get; set; }

        public Contract Contract { get; private set; }

        public bool IsConnected => (Contract != null) && (Contract.Client.ReadClient.ConnectionState == RpcConnectionState.Connected) && (Contract.Client.WriteClient.ConnectionState == RpcConnectionState.Connected);

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

            IRpcClient writer = RpcClientFactory.Configure().WithLogger(Debug.unityLogger).WithWebSocket(WriterHost).Create();

            IRpcClient reader = RpcClientFactory.Configure().WithLogger(Debug.unityLogger).WithWebSocket(ReaderHost).Create();

            DAppChainClient client = new DAppChainClient(writer, reader) { Logger = Debug.unityLogger };

            client.TxMiddleware = new TxMiddleware(new ITxMiddlewareHandler[] { new NonceTxMiddleware(publicKey, client), new SignedTxMiddleware(privateKey) });

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
            GetCollectionRequest request = new GetCollectionRequest { UserId = userId };

            return await Contract.StaticCallAsync<GetCollectionResponse>(GetCardCollectionMethod, request);
        }

        #endregion

        #region Card Library

        private const string GetCardLibraryMethod = "ListCardLibrary";

        public async Task<ListCardLibraryResponse> GetCardLibrary()
        {
            ListCardLibraryRequest request = new ListCardLibraryRequest();

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
            ListDecksRequest request = new ListDecksRequest { UserId = userId };

            return await Contract.StaticCallAsync<ListDecksResponse>(GetDeckDataMethod, request);
        }

        public async Task DeleteDeck(string userId, long deckId, long lastModificationTimestamp)
        {
            DeleteDeckRequest request = new DeleteDeckRequest { UserId = userId, DeckId = deckId, LastModificationTimestamp = lastModificationTimestamp };

            await Contract.CallAsync(DeleteDeckMethod, request);
        }

        public async Task EditDeck(string userId, Deck deck, long lastModificationTimestamp)
        {
            EditDeckRequest request = EditDeckRequest(userId, deck, lastModificationTimestamp);

            await Contract.CallAsync(EditDeckMethod, request);
        }

        public async Task<long> AddDeck(string userId, Deck deck, long lastModificationTimestamp)
        {
            RepeatedField<CardCollection> cards = new RepeatedField<CardCollection>();

            for (int i = 0; i < deck.cards.Count; i++)
            {
                CardCollection cardInCollection = new CardCollection { CardName = deck.cards[i].cardName, Amount = deck.cards[i].amount };
                Debug.Log("Card in collection = " + cardInCollection.CardName + " , " + cardInCollection.Amount);
                cards.Add(cardInCollection);
            }

            CreateDeckRequest request = new CreateDeckRequest { UserId = userId, Deck = new ProtobufDeck { Name = deck.name, HeroId = deck.heroId, Cards = { cards } }, LastModificationTimestamp = lastModificationTimestamp };

            CreateDeckResponse createDeckResponse = await Contract.CallAsync<CreateDeckResponse>(AddDeckMethod, request);
            return createDeckResponse.DeckId;
        }

        private static EditDeckRequest EditDeckRequest(string userId, Deck deck, long lastModificationTimestamp)
        {
            RepeatedField<CardCollection> cards = new RepeatedField<CardCollection>();

            for (int i = 0; i < deck.cards.Count; i++)
            {
                CardCollection cardInCollection = new CardCollection { CardName = deck.cards[i].cardName, Amount = deck.cards[i].amount };
                Debug.Log("Card in collection = " + cardInCollection.CardName + " , " + cardInCollection.Amount);
                cards.Add(cardInCollection);
            }

            EditDeckRequest request = new EditDeckRequest { UserId = userId, Deck = new ProtobufDeck { Id = deck.id, Name = deck.name, HeroId = deck.heroId, Cards = { cards } }, LastModificationTimestamp = lastModificationTimestamp };
            return request;
        }

        #endregion

        #region Heroes

        private const string HeroesList = "ListHeroes";

        public async Task<ListHeroesResponse> GetHeroesList(string userId)
        {
            ListHeroesRequest request = new ListHeroesRequest { UserId = userId };

            return await Contract.StaticCallAsync<ListHeroesResponse>(HeroesList, request);
        }

        #endregion

        #region Login

        private const string CreateAccountMethod = "CreateAccount";

        public async Task SignUp(string userId)
        {
            UpsertAccountRequest req = new UpsertAccountRequest { UserId = userId };

            await Contract.CallAsync(CreateAccountMethod, req);
        }

        #endregion

        #region Turn Logs

        private const string UploadActionLogMethod = "UploadHistory"; // just a random method for now

        public async Task UploadActionLog(string userId, ActionLogModel actionLogModel)
        {
            string actionLogModelJson = JsonConvert.SerializeObject(actionLogModel, Formatting.Indented);
            Dictionary<string, object> actionLogModelJsonDictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(actionLogModelJson);
            actionLogModelJson = JsonConvert.SerializeObject(actionLogModelJsonDictionary[nameof(ActionLogModel.LogData)], Formatting.Indented);
            Debug.Log("Logging action: \n" + actionLogModelJson);
            await Task.Delay(1000);

            /*var req = new UpsertAccountRequest {
                            UserId = userId,
                            //we'll also put all our collected strings in the HistoryData List
                        };*/

            // await Contract.CallAsync(CreateAccountMethod, req);
        }

        #endregion

        #region Auth

        private const string AuthBetaKeyValidationEndPoint = "/user/beta/validKey";

        private const string AuthBetaConfigEndPoint = "/user/beta/config";

        public async Task<bool> CheckIfBetaKeyValid(string betaKey)
        {
            WebrequestCreationInfo webrequestCreationInfo = new WebrequestCreationInfo();
            webrequestCreationInfo.Url = AuthBackendHost + AuthBetaKeyValidationEndPoint + "?beta_key=" + betaKey;
            HttpResponseMessage httpResponseMessage = await WebRequestUtils.CreateAndSendWebrequest(webrequestCreationInfo);
            if (!httpResponseMessage.IsSuccessStatusCode)
            {
                throw new Exception($"{nameof(CheckIfBetaKeyValid)} failed with error code {httpResponseMessage.StatusCode}");
            }

            BetaKeyValidationResponse betaKeyValidationResponse = httpResponseMessage.DeserializeAsJson<BetaKeyValidationResponse>();
            return betaKeyValidationResponse.IsValid;
        }

        public async Task<BetaConfig> GetBetaConfig(string betaKey)
        {
            WebrequestCreationInfo webrequestCreationInfo = new WebrequestCreationInfo();
            webrequestCreationInfo.Url = AuthBackendHost + AuthBetaConfigEndPoint + "?beta_key=" + betaKey;
            HttpResponseMessage httpResponseMessage = await WebRequestUtils.CreateAndSendWebrequest(webrequestCreationInfo);
            if (!httpResponseMessage.IsSuccessStatusCode)
            {
                throw new Exception($"{nameof(GetBetaConfig)} failed with error code {httpResponseMessage.StatusCode}");
            }

            BetaConfig betaConfig = JsonConvert.DeserializeObject<BetaConfig>(
                httpResponseMessage.ReadToEnd(),

                // FIXME: backend should return valid version numbers at all times
                new VersionConverterWithFallback(Version.Parse(Constants.CURRENT_VERSION_BASE)));
            return betaConfig;
        }

        private struct BetaKeyValidationResponse
        {
            [JsonProperty(PropertyName = "is_valid")]
            public bool IsValid;
        }

        #endregion
    }
}
