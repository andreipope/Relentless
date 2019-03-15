using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using log4net;
using log4net.Core;
using Loom.Client;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.Protobuf;
using Newtonsoft.Json;
using UnityEngine;
using Card = Loom.ZombieBattleground.Data.Card;
using Debug = UnityEngine.Debug;
using DebugCheatsConfiguration = Loom.ZombieBattleground.BackendCommunication.DebugCheatsConfiguration;
using ILogger = log4net.Core.ILogger;
using Object = UnityEngine.Object;

namespace Loom.ZombieBattleground.Test
{
    /// <summary>
    /// Implements a basic independent game client. Useful for simulating remote players in the same process.
    /// </summary>
    [Serializable]
    public class MultiplayerDebugClient
    {
        private const float KeepAliveInterval = Constants.PvPCheckPlayerAvailableMaxTime;

        private BackendFacade _backendFacade;
        private UserDataModel _userDataModel;
        private MatchMakingFlowController _matchMakingFlowController;
        private MatchRequestFactory _matchRequestFactory;
        private PlayerActionFactory _playerActionFactory;
        private List<Card> _cardLibrary;

        private bool _useBackendGameLogic;
        private List<string> _pvpTags = new List<string>();
        private DebugCheatsConfiguration _debugCheats = new DebugCheatsConfiguration();
        private Address? _customGameAddress;
        private long _deckId;

        private double? _lastTimeSinceStartup;
        private double _keepAliveTimer;

        [JsonIgnore]
        public BackendFacade BackendFacade
        {
            get => _backendFacade;
            set => _backendFacade = value;
        }

        [JsonIgnore]
        public MatchMakingFlowController MatchMakingFlowController
        {
            get => _matchMakingFlowController;
            set => _matchMakingFlowController = value;
        }

        [JsonIgnore]
        public MatchRequestFactory MatchRequestFactory
        {
            get => _matchRequestFactory;
            set => _matchRequestFactory = value;
        }

        [JsonIgnore]
        public PlayerActionFactory PlayerActionFactory
        {
            get => _playerActionFactory;
            set => _playerActionFactory = value;
        }

        public List<Card> CardLibrary
        {
            get => _cardLibrary;
            set => _cardLibrary = value;
        }

        public UserDataModel UserDataModel
        {
            get => _userDataModel;
            set => _userDataModel = value;
        }

        public bool UseBackendGameLogic
        {
            get => _useBackendGameLogic;
            set => _useBackendGameLogic = value;
        }

        public List<string> PvPTags
        {
            get => _pvpTags;
            set => _pvpTags = value;
        }

        public Address? CustomGameAddress
        {
            get => _customGameAddress;
            set => _customGameAddress = value;
        }

        public long DeckId
        {
            get => _deckId;
            set => _deckId = value;
        }

        public DebugCheatsConfiguration DebugCheats
        {
            get => _debugCheats;
            set => _debugCheats = value;
        }

        public MultiplayerDebugClient(string name = null)
        {
            UserDataModel = new UserDataModel(
                "DebugClient_" +
                (name != null ? name + "_" : "") +
                Guid.NewGuid(),
                CryptoUtils.GeneratePrivateKey()
            );
        }

        public async Task Start(
            Func<Contract, IContractCallProxy> contractCallProxyFactory,
            DAppChainClientConfiguration clientConfiguration,
            Action<MatchMakingFlowController> onMatchMakingFlowControllerCreated = null,
            Action<BackendFacade> onBackendFacadeCreated = null,
            Action<DAppChainClient> onClientCreatedCallback = null,
            IDAppChainClientCallExecutor chainClientCallExecutor = null,
            bool enabledLogs = true)
        {
            await Reset();

            TaggedLoggerWrapper taggedLoggerWrapper = new TaggedLoggerWrapper(Logging.GetLog(nameof(BackendFacade)).Logger, UserDataModel.UserId);
            ILog log = new LogImpl(taggedLoggerWrapper);
            BackendFacade backendFacade = new BackendFacade(GameClient.GetDefaultBackendEndpoint(), contractCallProxyFactory, log, log);
            backendFacade.Init();
            onBackendFacadeCreated?.Invoke(backendFacade);
            await backendFacade.CreateContract(UserDataModel.PrivateKey, clientConfiguration, onClientCreatedCallback, chainClientCallExecutor);

            try
            {
                await backendFacade.SignUp(UserDataModel.UserId);
            }
            catch (TxCommitException e) when (e.Message.Contains("user already exists"))
            {
                // Ignore
            }

            ListCardLibraryResponse listCardLibraryResponse = await backendFacade.GetCardLibrary();
            if (listCardLibraryResponse != null)
            {
                CardLibrary = listCardLibraryResponse.Cards.Select(card => card.FromProtobuf()).ToList();
            }

            MatchMakingFlowController matchMakingFlowController = new MatchMakingFlowController(backendFacade, UserDataModel);
            matchMakingFlowController.ActionWaitingTime = 1f;
            onMatchMakingFlowControllerCreated?.Invoke(matchMakingFlowController);

            BackendFacade = backendFacade;
            MatchMakingFlowController = matchMakingFlowController;
        }

        public async Task Reset()
        {
            _keepAliveTimer = 0f;
            if (BackendFacade != null)
            {
                BackendFacade.Dispose();

                BackendFacade = null;

                await MatchMakingFlowController.Stop();
                MatchMakingFlowController = null;
            }
        }

        public async Task Update()
        {
#if UNITY_EDITOR
            double timeSinceStartup = Utilites.GetTimestamp();
#else
            double timeSinceStartup = Time.realtimeSinceStartup;
#endif
            if (_lastTimeSinceStartup == null)
            {
                _lastTimeSinceStartup = timeSinceStartup;
            }

            double deltaTime = timeSinceStartup - _lastTimeSinceStartup.Value;
            _lastTimeSinceStartup = timeSinceStartup;

            if (MatchMakingFlowController != null && _backendFacade.IsConnected)
            {
                await MatchMakingFlowController.Update((float) deltaTime);

                if (MatchMakingFlowController.State == MatchMakingFlowController.MatchMakingState.Confirmed)
                {
                    _keepAliveTimer -= deltaTime;
                    if (_keepAliveTimer <= 0f)
                    {
                        _keepAliveTimer = KeepAliveInterval;
                        await BackendFacade.KeepAliveStatus(UserDataModel.UserId, MatchMakingFlowController.MatchMetadata.Id);
                    }
                }
            }
        }
    }
}
