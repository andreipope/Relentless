using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
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
        private IRpcClient _reader;
        private IContractCallProxy _contractCallProxy;
        private Func<Contract, IContractCallProxy> _contractCallProxyFactory;

        public delegate void ContractCreatedEventHandler(Contract oldContract, Contract newContract);

        public delegate void PlayerActionDataReceivedHandler(byte[] bytes);

        public event ContractCreatedEventHandler ContractCreated;

        public BackendEndpoint BackendEndpoint { get; set; }

        public Contract Contract { get; private set; }

        public bool IsConnected => Contract != null &&
            Contract.Client.ReadClient.ConnectionState == RpcConnectionState.Connected &&
            Contract.Client.WriteClient.ConnectionState == RpcConnectionState.Connected;

        public IContractCallProxy ContractCallProxy => _contractCallProxy;

        public ILog Log { get; }

        public ILog RpcLog { get; }

        public BackendFacade(BackendEndpoint backendEndpoint, Func<Contract, IContractCallProxy> contractCallProxyFactory, ILog log, ILog rpcLog)
        {
            BackendEndpoint = backendEndpoint ?? throw new ArgumentNullException(nameof(backendEndpoint));
            _contractCallProxyFactory = contractCallProxyFactory ?? throw new ArgumentNullException(nameof(contractCallProxyFactory));
            Log = log ?? throw new ArgumentNullException(nameof(log));
            RpcLog = rpcLog ?? throw new ArgumentNullException(nameof(rpcLog));
        }

        public void Init()
        {
            Log.Info("Auth Host: " + BackendEndpoint.AuthHost);
            Log.Info("Reader Host: " + BackendEndpoint.ReaderHost);
            Log.Info("Writer Host: " + BackendEndpoint.WriterHost);
            Log.Info("Vault Host: " + BackendEndpoint.VaultHost);
            Log.Info("Card Data Version: " + BackendEndpoint.DataVersion);
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

            _reader =
                RpcClientFactory
                    .Configure()
                    .WithLogger(logger)
                    .WithWebSocket(BackendEndpoint.ReaderHost)
                    .Create();

            DAppChainClient client = new DAppChainClient(
                writer,
                _reader,
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
            Address contractAddr = await client.ResolveContractAddressAsync("ZombieBattleground");
            Contract oldContract = Contract;
            Contract = new Contract(client, contractAddr, callerAddr);

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

        public async Task SignUp(string userId)
        {
            UpsertAccountRequest req = new UpsertAccountRequest
            {
                Version = BackendEndpoint.DataVersion,
                UserId = userId
            };

            await _contractCallProxy.CallAsync(CreateAccountMethod, req);
        }

        #endregion

        #region Auth

        private const string userInfoEndPoint = "/user/info";

        private const string loginEndPoint = "/auth/email/login";

        private const string signupEndPoint = "/auth/email/game_signup";

        private const string forgottenPasswordEndPoint = "/auth/mlink/generate";

        private const string createVaultTokenEndPoint = "/auth/loom-userpass/create_token";

        private const string accessVaultEndPoint = "/entcubbyhole/loomauth";

        private const string createVaultTokenForNon2FAUsersEndPoint = "/auth/loom-simple-userpass/create_token";

        public async Task<UserInfo> GetUserInfo(string accessToken)
        {
            WebrequestCreationInfo webrequestCreationInfo = new WebrequestCreationInfo();
            webrequestCreationInfo.Url = BackendEndpoint.AuthHost + userInfoEndPoint;
            webrequestCreationInfo.Headers.Add("authorization", "Bearer " + accessToken);

            HttpResponseMessage httpResponseMessage = await WebRequestUtils.CreateAndSendWebrequest(webrequestCreationInfo);
            httpResponseMessage.ThrowOnError(webrequestCreationInfo);

            UserInfo userInfo = JsonConvert.DeserializeObject<UserInfo>(
                httpResponseMessage.ReadToEnd(),

                // FIXME: backend should return valid version numbers at all times
                new VersionConverterWithFallback(Version.Parse(Constants.CurrentVersionBase))
            );

            return userInfo;
        }

        public async Task<LoginData> InitiateLogin(string email, string password)
        {
            WebrequestCreationInfo webrequestCreationInfo = new WebrequestCreationInfo();
            webrequestCreationInfo.Method = WebRequestMethod.POST;
            webrequestCreationInfo.Url = BackendEndpoint.AuthHost + loginEndPoint;
            webrequestCreationInfo.ContentType = "application/json;charset=UTF-8";

            LoginRequest loginRequest = new LoginRequest();
            loginRequest.email = email;
            loginRequest.password = password;
            webrequestCreationInfo.Data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(loginRequest));
            webrequestCreationInfo.Headers.Add("accept", "application/json, text/plain, */*");
            webrequestCreationInfo.Headers.Add("authority", "auth.loom.games");

            HttpResponseMessage httpResponseMessage = await WebRequestUtils.CreateAndSendWebrequest(webrequestCreationInfo);
            httpResponseMessage.ThrowOnError(webrequestCreationInfo);

            Log.Debug(httpResponseMessage.ReadToEnd());
            LoginData loginData = JsonConvert.DeserializeObject<LoginData>(
                httpResponseMessage.ReadToEnd());
            return loginData;
        }

        public async Task<RegisterData> InitiateRegister(string email, string password)
        {
            WebrequestCreationInfo webrequestCreationInfo = new WebrequestCreationInfo();
            webrequestCreationInfo.Method = WebRequestMethod.POST;
            webrequestCreationInfo.Url = BackendEndpoint.AuthHost + signupEndPoint;
            webrequestCreationInfo.ContentType = "application/json;charset=UTF-8";

            LoginRequest loginRequest = new LoginRequest();
            loginRequest.email = email;
            loginRequest.password = password;
            webrequestCreationInfo.Data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(loginRequest));
            webrequestCreationInfo.Headers.Add("accept", "application/json, text/plain, */*");
            webrequestCreationInfo.Headers.Add("authority", "auth.loom.games");

            HttpResponseMessage httpResponseMessage = await WebRequestUtils.CreateAndSendWebrequest(webrequestCreationInfo);
            httpResponseMessage.ThrowOnError(webrequestCreationInfo);

            RegisterData registerData = JsonConvert.DeserializeObject<RegisterData>(
                httpResponseMessage.ReadToEnd());
            return registerData;
        }

        public async Task<bool> InitiateForgottenPassword(string email)
        {
            WebrequestCreationInfo webrequestCreationInfo = new WebrequestCreationInfo();
            webrequestCreationInfo.Url = BackendEndpoint.AuthHost + forgottenPasswordEndPoint + "?email=" + email + "&kind=signup";

            HttpResponseMessage httpResponseMessage = await WebRequestUtils.CreateAndSendWebrequest(webrequestCreationInfo);
            httpResponseMessage.ThrowOnError(webrequestCreationInfo);

            return true;
        }

        public async Task<CreateVaultTokenData> CreateVaultToken(string otp, string accessToken)
        {
            WebrequestCreationInfo webrequestCreationInfo = new WebrequestCreationInfo();
            webrequestCreationInfo.Method = WebRequestMethod.POST;
            webrequestCreationInfo.Url = BackendEndpoint.VaultHost + createVaultTokenEndPoint;
            webrequestCreationInfo.ContentType = "application/json;charset=UTF-8";

            VaultTokenRequest vaultTokenRequest = new VaultTokenRequest();
            vaultTokenRequest.authy_token = otp;
            vaultTokenRequest.access_token = accessToken;

            webrequestCreationInfo.Data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(vaultTokenRequest));
            webrequestCreationInfo.Headers.Add("accept", "application/json, text/plain, */*");

            HttpResponseMessage httpResponseMessage = await WebRequestUtils.CreateAndSendWebrequest(webrequestCreationInfo);
            httpResponseMessage.ThrowOnError(webrequestCreationInfo);
            Log.Debug(httpResponseMessage.ReadToEnd());

            CreateVaultTokenData vaultTokenData = JsonConvert.DeserializeObject<CreateVaultTokenData>(
                httpResponseMessage.ReadToEnd());
            return vaultTokenData;
        }

        public async Task<CreateVaultTokenData> CreateVaultTokenForNon2FAUsers(string accessToken)
        {
            WebrequestCreationInfo webrequestCreationInfo = new WebrequestCreationInfo();
            webrequestCreationInfo.Method = WebRequestMethod.POST;
            webrequestCreationInfo.Url = BackendEndpoint.VaultHost + createVaultTokenForNon2FAUsersEndPoint;
            webrequestCreationInfo.ContentType = "application/json;charset=UTF-8";

            VaultTokenNon2FARequest vaultTokenRequest = new VaultTokenNon2FARequest();
            vaultTokenRequest.access_token = accessToken;

            webrequestCreationInfo.Data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(vaultTokenRequest));
            Log.Debug(JsonConvert.SerializeObject(vaultTokenRequest));
            webrequestCreationInfo.Headers.Add("accept", "application/json, text/plain, */*");

            HttpResponseMessage httpResponseMessage = await WebRequestUtils.CreateAndSendWebrequest(webrequestCreationInfo);
            httpResponseMessage.ThrowOnError(webrequestCreationInfo);
            Log.Debug(httpResponseMessage.ReadToEnd());

            CreateVaultTokenData vaultTokenData = JsonConvert.DeserializeObject<CreateVaultTokenData>(
                httpResponseMessage.ReadToEnd());
            return vaultTokenData;
        }

        public async Task<GetVaultDataResponse> GetVaultData(string vaultToken)
        {
            WebrequestCreationInfo webrequestCreationInfo = new WebrequestCreationInfo();
            webrequestCreationInfo.Method = WebRequestMethod.GET;
            webrequestCreationInfo.Url = BackendEndpoint.VaultHost + accessVaultEndPoint;
            webrequestCreationInfo.ContentType = "application/json;charset=UTF-8";

            webrequestCreationInfo.Headers.Add("accept", "application/json, text/plain, */*");
            webrequestCreationInfo.Headers.Add("X-Vault-Token", vaultToken);

            HttpResponseMessage httpResponseMessage =
                await WebRequestUtils.CreateAndSendWebrequest(webrequestCreationInfo);

            if (!httpResponseMessage.IsSuccessStatusCode)
            {
                if (httpResponseMessage.StatusCode.ToString() == Constants.VaultEmptyErrorCode)
                {
                    throw new Exception(httpResponseMessage.StatusCode.ToString());
                }
                else
                {
                    httpResponseMessage.ThrowOnError(webrequestCreationInfo);
                }
            }
            Log.Debug(httpResponseMessage.ReadToEnd());


            GetVaultDataResponse getVaultDataResponse = JsonConvert.DeserializeObject<GetVaultDataResponse>(
                httpResponseMessage.ReadToEnd());
            return getVaultDataResponse;
        }

        public async Task<bool> SetVaultData(string vaultToken, string privateKey)
        {
            WebrequestCreationInfo webrequestCreationInfo = new WebrequestCreationInfo();
            webrequestCreationInfo.Method = WebRequestMethod.POST;
            webrequestCreationInfo.Url = BackendEndpoint.VaultHost + accessVaultEndPoint;
            webrequestCreationInfo.ContentType = "application/json;charset=UTF-8";

            VaultPrivateKeyRequest vaultPrivateKeyRequest = new VaultPrivateKeyRequest();
            vaultPrivateKeyRequest.privatekey = privateKey;

            webrequestCreationInfo.Data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(vaultPrivateKeyRequest));
            webrequestCreationInfo.Headers.Add("accept", "application/json, text/plain, */*");
            webrequestCreationInfo.Headers.Add("X-Vault-Token", vaultToken);

            HttpResponseMessage httpResponseMessage = await WebRequestUtils.CreateAndSendWebrequest(webrequestCreationInfo);
            Log.Debug(httpResponseMessage.ReadToEnd());
            httpResponseMessage.ThrowOnError(webrequestCreationInfo);

            return true;
        }

        public async Task<BackendEndpoint> GetServerURLs()
        {
            const string queryURLsEndPoint = "/zbversion";

            WebrequestCreationInfo webrequestCreationInfo = new WebrequestCreationInfo();
            webrequestCreationInfo.Url = "https://auth.loom.games" + queryURLsEndPoint + "?version=" + Constants.CurrentVersionBase + "&environment=production";

            Log.Debug(webrequestCreationInfo.Url);

            HttpResponseMessage httpResponseMessage = await WebRequestUtils.CreateAndSendWebrequest(webrequestCreationInfo);
            httpResponseMessage.ThrowOnError(webrequestCreationInfo);
            Log.Debug(httpResponseMessage.ReadToEnd());

            ServerUrlsResponse serverInfo = JsonConvert.DeserializeObject<ServerUrlsResponse>(
                httpResponseMessage.ReadToEnd()
            );

            return new BackendEndpoint(
                serverInfo.version.auth_url,
                serverInfo.version.read_url,
                serverInfo.version.write_url,
                serverInfo.version.vault_url,
                serverInfo.version.data_version,
                serverInfo.version.is_maintenace_mode,
                serverInfo.version.is_force_update,
                false
            );
        }

        private struct ServerUrlsResponse
        {
            public ServerUrlsData version;
        }

        private struct ServerUrlsData
        {
            public int id;
            public int major;
            public int minor;
            public int patch;
            public string environment;
            public string auth_url;
            public string read_url;
            public string write_url;
            public string vault_url;
            public string data_version;
            public bool is_maintenace_mode;
            public bool is_force_update;
        }

        private struct LoginRequest
        {
            public string email;
            public string password;
        }

        private struct VaultTokenRequest
        {
            public string authy_token;
            public string access_token;
        }

        private struct VaultTokenNon2FARequest
        {
            public string access_token;
        }

        private struct VaultPrivateKeyRequest
        {
            public string privatekey;
        }

        private struct BetaKeyValidationResponse
        {
            [JsonProperty(PropertyName = "is_valid")]
            public bool IsValid;
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

        public async Task SubscribeEvent(IList<string> topics)
        {
            await UnsubscribeEvent();
            await _reader.SubscribeAsync(EventHandler, topics);
        }

        public async Task UnsubscribeEvent()
        {
            try
            {
                await _reader.UnsubscribeAsync(EventHandler);
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
        private byte[] previousData;
        private void EventHandler(object sender, JsonRpcEventData e)
        {
            if (previousData == null || !previousData.SequenceEqual(e.Data)) {
                previousData = e.Data;
                PlayerActionDataReceived?.Invoke(e.Data);
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

#region RewardTutorial
        private const string RewardTutorialCompletedMethod = "RewardTutorialCompleted";
        public async Task<RewardTutorialCompletedResponse> GetRewardTutorialCompletedResponse()
        {
            Log.Debug("GetRewardTutorialCompletedResponse");
            RewardTutorialCompletedRequest request = new RewardTutorialCompletedRequest();
            return await _contractCallProxy.CallAsync<RewardTutorialCompletedResponse>(RewardTutorialCompletedMethod, request);
        }

        private const string RewardTutorialClaimMethod = "ConfirmRewardTutorialClaimed";
        public async Task<RewardTutorialClaimed> ConfirmRewardTutorialClaimed()
        {
            ConfirmRewardTutorialClaimedRequest request = new ConfirmRewardTutorialClaimedRequest();
            return await _contractCallProxy.CallAsync<RewardTutorialClaimed>(RewardTutorialClaimMethod, request);
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
    }
}
