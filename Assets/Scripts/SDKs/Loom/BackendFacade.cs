using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using Loom.Client;
using Loom.Google.Protobuf.Collections;
using Loom.Newtonsoft.Json;
using LoomNetwork.CZB.Protobuf;
using UnityEngine;
using Random = System.Random;
using Deck = LoomNetwork.CZB.Data.Deck;
using ProtobufDeck = LoomNetwork.CZB.Protobuf.Deck;

namespace LoomNetwork.CZB.BackendCommunication
{
    public class BackendFacade : IService
    {
        private const string UserDataFileName = "UserData.json";

        public delegate void ContractCreatedEventHandler(Contract oldContract, Contract newContract);

        public event ContractCreatedEventHandler ContractCreated;

#if UNITY_EDITOR
        public string WriterHost { get; set; } = "ws://127.0.0.1:46657/websocket";
        public string ReaderHost { get; set; } = "ws://127.0.0.1:9999/queryws";
#else
        public string WriterHost { get; set; } = "ws://battleground-testnet-asia1.dappchains.com:46657/websocket";
        public string ReaderHost { get; set; } = "ws://battleground-testnet-asia1.dappchains.com:9999/queryws";
#endif

        public UserDataModel UserDataModel { get; set; }

        public Contract Contract { get; private set; }

        public bool IsConnected =>
            Contract != null &&
            Contract.Client.ReadClient.ConnectionState == RpcConnectionState.Connected &&
            Contract.Client.WriteClient.ConnectionState == RpcConnectionState.Connected;

        protected string UserDataFilePath => Path.Combine(Application.persistentDataPath, UserDataFileName);

        public async Task LoadUserDataModelAndCreateContract()
        {
            LoadUserDataModel();
            Debug.Log("User Id: " + UserDataModel.UserId);
            await CreateContract(UserDataModel.PrivateKey);
        }

        public async Task CreateContract(byte[] privateKey)
        {
            var publicKey = CryptoUtils.PublicKeyFromPrivateKey(privateKey);
            var callerAddr = Address.FromPublicKey(publicKey);

            var writer = RpcClientFactory.Configure()
                .WithLogger(Debug.unityLogger)
                .WithWebSocket(WriterHost)
                .Create();

            var reader = RpcClientFactory.Configure()
                .WithLogger(Debug.unityLogger)
                .WithWebSocket(ReaderHost)
                .Create();

            var client = new DAppChainClient(writer, reader)
                { Logger = Debug.unityLogger };

            client.TxMiddleware = new TxMiddleware(new ITxMiddlewareHandler[]
            {
                new NonceTxMiddleware(publicKey, client),
                new SignedTxMiddleware(privateKey)
            });

            var contractAddr = await client.ResolveContractAddressAsync("ZombieBattleground");
            Contract oldContract = Contract;
            Contract = new Contract(client, contractAddr, callerAddr);
            ContractCreated?.Invoke(oldContract, Contract);
        }

        public bool LoadUserDataModel(bool force = false)
        {
            if (UserDataModel != null && !force)
                return true;

            if (!File.Exists(UserDataFilePath))
                return false;

            UserDataModel = JsonConvert.DeserializeObject<UserDataModel>(File.ReadAllText(UserDataFilePath));
            return true;
        }

        public bool SetUserDataModel(UserDataModel userDataModel)
        {
            if (userDataModel == null)
                throw new ArgumentNullException(nameof(userDataModel));

            File.WriteAllText(UserDataFilePath, JsonConvert.SerializeObject(userDataModel));
            UserDataModel = userDataModel;
            return true;
        }

        #region Card Collection

        private const string GetCardCollectionMethod = "GetCollection";

        public async Task<GetCollectionResponse> GetCardCollection(string userId)
        {
            var request = new GetCollectionRequest
                { UserId = userId };

            return await Contract.StaticCallAsync<GetCollectionResponse>(GetCardCollectionMethod, request);
        }

        #endregion

        #region Card Library

        private const string GetCardLibraryMethod = "ListCardLibrary";

        public async Task<ListCardLibraryResponse> GetCardLibrary()
        {
            var request = new ListCardLibraryRequest();

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
            var request = new ListDecksRequest { UserId = userId };

            return await Contract.StaticCallAsync<ListDecksResponse>(GetDeckDataMethod, request);
        }

        public async Task DeleteDeck(string userId, long deckId)
        {
            var request = new DeleteDeckRequest
            {
                UserId = userId,
                DeckId = deckId
            };

            await Contract.CallAsync(DeleteDeckMethod, request);
        }

        public async Task EditDeck(string userId, Deck deck)
        {
            EditDeckRequest request = EditDeckRequest(userId, deck);

            await Contract.CallAsync(EditDeckMethod, request);
        }

        public async Task<long> AddDeck(string userId, Deck deck)
        {
            var cards = new RepeatedField<CardCollection>();

            for (var i = 0; i < deck.cards.Count; i++)
            {
                var cardInCollection = new CardCollection
                {
                    CardName = deck.cards[i].cardName,
                    Amount = deck.cards[i].amount
                };
                Debug.Log("Card in collection = " + cardInCollection.CardName + " , " + cardInCollection.Amount);
                cards.Add(cardInCollection);
            }

            var request = new CreateDeckRequest
            {
                UserId = userId,
                Deck = new ProtobufDeck
                {
                    Name = deck.name,
                    HeroId = deck.heroId,
                    Cards = { cards }
                }
            };

            return (await Contract.CallAsync<CreateDeckResponse>(AddDeckMethod, request)).DeckId;
        }

        private static EditDeckRequest EditDeckRequest(string userId, Deck deck)
        {
            var cards = new RepeatedField<CardCollection>();

            for (var i = 0; i < deck.cards.Count; i++)
            {
                var cardInCollection = new CardCollection
                {
                    CardName = deck.cards[i].cardName,
                    Amount = deck.cards[i].amount
                };
                Debug.Log("Card in collection = " + cardInCollection.CardName + " , " + cardInCollection.Amount);
                cards.Add(cardInCollection);
            }

            var request = new EditDeckRequest
            {
                UserId = userId,
                Deck = new ProtobufDeck
                {
                    Id = deck.id,
                    Name = deck.name,
                    HeroId = deck.heroId,
                    Cards = { cards }
                }
            };
            return request;
        }

        #endregion

        #region Heroes

        private const string HeroesList = "ListHeroes";

        public async Task<ListHeroesResponse> GetHeroesList(string userId)
        {
            var request = new ListHeroesRequest
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
            var req = new UpsertAccountRequest {
                UserId = userId
            };

            await Contract.CallAsync(CreateAccountMethod, req);
        }

        #endregion

        #region Turn Logs

        private const string UploadActionLogMethod = "UploadHistory"; //just a random method for now

        public async Task UploadActionLog(string userId, ActionLogModel actionLogModel)
        {
            string actionLogModelJson = JsonConvert.SerializeObject(actionLogModel, Formatting.Indented);
            Dictionary<string, object> actionLogModelJsonDictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(actionLogModelJson);
            actionLogModelJson = JsonConvert.SerializeObject(actionLogModelJsonDictionary[nameof(ActionLogModel.LogData)]);
            Debug.Log("Logging action: \n" + actionLogModelJson);
            await Task.Delay(1000);
            /*var req = new UpsertAccountRequest {
                UserId = userId,
                //we'll also put all our collected strings in the HistoryData List
            };*/

            //await Contract.CallAsync(CreateAccountMethod, req);
        }

        #endregion

        public void Init()
        {
            
        }

        public void Update()
        {
            
        }

        public void Dispose()
        {
            
        }
    }

}
