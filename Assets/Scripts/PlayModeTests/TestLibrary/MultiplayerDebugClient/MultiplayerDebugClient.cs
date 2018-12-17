using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Loom.Client;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.Protobuf;
using Newtonsoft.Json;
using UnityEngine;
using Card = Loom.ZombieBattleground.Data.Card;
using DebugCheatsConfiguration = Loom.ZombieBattleground.BackendCommunication.DebugCheatsConfiguration;
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

        private double _lastKeepAliveTime;

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

        [JsonIgnore]
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

        public async Task Start(
            string name = null,
            Action<MatchMakingFlowController> onMatchMakingFlowControllerCreated = null,
            Action<BackendFacade> onBackendFacadeCreated = null)
        {
            await Reset();

            UserDataModel = new UserDataModel(
                "DebugClient_" +
                (name != null ? name + "_" : "") +
                UnityEngine.Random.Range(int.MinValue, int.MaxValue).ToString().Replace("-", "0") + Time.frameCount,
                CryptoUtils.GeneratePrivateKey()
            );

            BackendFacade backendFacade = new BackendFacade(GameClient.GetDefaultBackendEndpoint())
            {
                //Logger = new Logger(new PrefixUnityLogger($"[{UserDataModel.UserId}] "))
            };
            backendFacade.Init();
            onBackendFacadeCreated?.Invoke(backendFacade);
            await backendFacade.CreateContract(UserDataModel.PrivateKey);
            try
            {
                await backendFacade.SignUp(UserDataModel.UserId);
            }
            catch (TxCommitException e) when (e.Message.Contains("user already exists"))
            {
                // Ignore
            }

            ListCardLibraryResponse listCardLibraryResponse = await backendFacade.GetCardLibrary();
            CardLibrary = listCardLibraryResponse.Cards.Select(card => card.FromProtobuf()).ToList();

            MatchMakingFlowController matchMakingFlowController = new MatchMakingFlowController(backendFacade, UserDataModel);
            matchMakingFlowController.ActionWaitingTime = 1f;
            onMatchMakingFlowControllerCreated?.Invoke(matchMakingFlowController);

            BackendFacade = backendFacade;
            MatchMakingFlowController = matchMakingFlowController;
        }

        public async Task Reset()
        {
            _lastKeepAliveTime = 0f;
            if (BackendFacade != null)
            {
                BackendFacade.Contract?.Client.Dispose();

                BackendFacade = null;

                await MatchMakingFlowController.Stop();
                MatchMakingFlowController = null;
            }
        }

        public async Task Update()
        {
            if (MatchMakingFlowController != null)
            {
                await MatchMakingFlowController.Update();

                if (MatchMakingFlowController.State == MatchMakingFlowController.MatchMakingState.Confirmed)
                {
#if UNITY_EDITOR
                    double timeSinceStartup = UnityEditor.EditorApplication.timeSinceStartup;
#else
                    double timeSinceStartup = Time.realtimeSinceStartup;
#endif
                    if (timeSinceStartup - _lastKeepAliveTime >= KeepAliveInterval)
                    {
                        _lastKeepAliveTime = timeSinceStartup;
                        await BackendFacade.KeepAliveStatus(UserDataModel.UserId, MatchMakingFlowController.MatchMetadata.Id);
                    }
                }
            }
        }

        private class PrefixUnityLogger : ILogHandler
        {
            private readonly string _prefix;

            public PrefixUnityLogger(string prefix)
            {
                _prefix = prefix;
            }

            public void LogFormat(LogType logType, Object context, string format, params object[] args)
            {
                Debug.unityLogger.LogFormat(logType, context, _prefix + format, args);
            }

            public void LogException(Exception exception, Object context)
            {
                Debug.unityLogger.LogException(exception, context);
            }
        }
    }
}
