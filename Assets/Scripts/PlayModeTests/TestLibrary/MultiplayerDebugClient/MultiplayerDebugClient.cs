using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Loom.Client;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.Protobuf;
using Newtonsoft.Json;
using UnityEngine;
using Card = Loom.ZombieBattleground.Data.Card;
using DebugCheatsConfiguration = Loom.ZombieBattleground.BackendCommunication.DebugCheatsConfiguration;

namespace Loom.ZombieBattleground.Editor.Tools
{
    [Serializable]
    public class MultiplayerDebugClient
    {
        private BackendFacade _backendFacade;
        private UserDataModel _userDataModel;
        private MatchMakingFlowController _matchMakingFlowController;
        private MatchRequestFactory _matchRequestFactory;
        private PlayerActionFactory _playerActionFactory;
        private List<Card> _cardLibrary;

        private bool _useBackendGameLogic = true;
        private List<string> _pvpTags = new List<string>();
        private DebugCheatsConfiguration _debugCheats = new DebugCheatsConfiguration();

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
                Logger = Debug.unityLogger
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
            matchMakingFlowController.ActionWaitingTime = 0.3f;
            onMatchMakingFlowControllerCreated?.Invoke(matchMakingFlowController);

            BackendFacade = backendFacade;
            MatchMakingFlowController = matchMakingFlowController;
        }

        public async Task Reset()
        {
            if (BackendFacade != null)
            {
                BackendFacade.Contract?.Client.Dispose();

                BackendFacade = null;

                await MatchMakingFlowController.Stop();
                MatchMakingFlowController = null;
            }
        }
    }
}
