using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Loom.Client;
using Loom.Newtonsoft.Json;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Protobuf;
using UnityEngine;
using Card = Loom.ZombieBattleground.Data.Card;
using Deck = Loom.ZombieBattleground.Data.Deck;
using Random = UnityEngine.Random;
using SystemText = System.Text;

namespace Loom.ZombieBattleground
{
    public class PvPManager : IService, IPvPManager
    {
        // matching actions
        public event Action MatchCreatedActionReceived;

        public event Action MatchingStartedActionReceived;

        public event Action PlayerLeftGameActionReceived;

        public event Action MatchingFailed;

        // game status actions
        public event Action GameStartedActionReceived;

        public event Action GameEndedActionReceived;

        // gameplay actions
        public event Action EndTurnActionReceived;

        public event Action<PlayerActionCardPlay> CardPlayedActionReceived;

        public event Action<PlayerActionCardAttack> CardAttackedActionReceived;

        public event Action<PlayerActionOverlordSkillUsed> OverlordSkillUsedActionReceived;

        public event Action<PlayerActionCardAbilityUsed> CardAbilityUsedActionReceived;

        public event Action<PlayerActionMulligan> MulliganProcessUsedActionReceived;

        public event Action<PlayerActionDrawCard> DrawCardActionReceived;

        public event Action<PlayerActionRankBuff> RankBuffActionReceived;

        public event Action<PlayerActionOutcome> PlayerActionOutcomeReceived;

        public event Action LeaveMatchReceived;

        public MatchMetadata MatchMetadata { get; set; }

        public GameState InitialGameState { get; set; }

        public Address? CustomGameModeAddress { get; set; }

        public List<string> PvPTags { get; set; }

        public MatchMakingFlowController MatchMakingFlowController => _matchMakingFlowController;

        public bool UseBackendLogic { get; set; }

        private BackendFacade _backendFacade;
        private BackendDataControlMediator _backendDataControlMediator;
        private IQueueManager _queueManager;

        private bool _isWaitForTurnTimerStart;
        private float _waitForTurnTimer;

        private SemaphoreSlim _matchmakingBusySemaphore = new SemaphoreSlim(1);

        private MatchMakingFlowController _matchMakingFlowController;

        public void Init()
        {
            _backendFacade = GameClient.Get<BackendFacade>();
            _queueManager = GameClient.Get<IQueueManager>();
            _backendDataControlMediator = GameClient.Get<BackendDataControlMediator>();
            _backendFacade.PlayerActionDataReceived += OnPlayerActionReceivedHandler;

            if (PvPTags == null)
            {
                PvPTags = new List<string> ();
            }

            GameClient.Get<IGameplayManager>().GameEnded += GameEndedHandler;
        }

        public async void Update()
        {
            if (_isWaitForTurnTimerStart)
            {
                _waitForTurnTimer += Time.deltaTime;
                if (_waitForTurnTimer > Constants.PvPWaitForTurnMaxTime)
                {
                    ResetWaitForTurnTimer();
                    await _backendFacade.CheckPlayerStatus(MatchMetadata.Id);
                }
            }

            if (_matchMakingFlowController != null)
            {
                await _matchMakingFlowController.Update();
            }
        }

        public void Dispose()
        {
        }

        public bool IsCurrentPlayer()
        {
            return InitialGameState.PlayerStates[InitialGameState.CurrentPlayerIndex].Id ==
                _backendDataControlMediator.UserDataModel.UserId;
        }

        public string GetOpponentUserId()
        {
            for (int i = 0; i < InitialGameState.PlayerStates.Count; i++)
            {
                if (InitialGameState.PlayerStates[i].Id != _backendDataControlMediator.UserDataModel.UserId)
                {
                    return InitialGameState.PlayerStates[i].Id;
                }
            }

            return "";
        }

        private async void GameEndedHandler(Enumerators.EndGameType obj)
        {
            _isWaitForTurnTimerStart = false;
            await _backendFacade.UnsubscribeEvent();
        }

        public async Task StartMatchmaking(int deckId)
        {
            await _matchmakingBusySemaphore.WaitAsync();

            try
            {
                if (_matchMakingFlowController != null)
                {
                    await _matchMakingFlowController.Stop();
                }

                _matchMakingFlowController = new MatchMakingFlowController(
                    _backendFacade,
                    _backendDataControlMediator.UserDataModel
                );

                _matchMakingFlowController.MatchConfirmed += MatchMakingFlowControllerOnMatchConfirmed;
                await _matchMakingFlowController.Start(deckId, CustomGameModeAddress, null, UseBackendLogic);
            }
            finally
            {
                _matchmakingBusySemaphore.Release();
            }
        }

        public async Task StopMatchmaking()
        {
            await _matchmakingBusySemaphore.WaitAsync();

            try
            {
                _queueManager.Active = false;
                _matchMakingFlowController.MatchConfirmed -= MatchMakingFlowControllerOnMatchConfirmed;
                await _matchMakingFlowController.Stop();
                _matchMakingFlowController = null;

                await _backendFacade.UnsubscribeEvent();
                if (MatchMetadata?.Id != null)
                {
                    await _backendFacade.CancelFindMatch(
                        _backendDataControlMediator.UserDataModel.UserId,
                        MatchMetadata.Id
                    );
                }

                _queueManager.Clear();
            }
            finally
            {
                _matchmakingBusySemaphore.Release();
            }
        }

        private async void MatchMakingFlowControllerOnMatchConfirmed(MatchMetadata matchMetadata)
        {
            _queueManager.Active = false;
            _queueManager.Clear();

            InitialGameState = null;

            MatchMetadata = matchMetadata;

            // No need to reload if a match was found immediately
            if (InitialGameState == null)
            {
                await LoadInitialGameState();
            }

            Debug.LogWarning("Match Starting");

            GameStartedActionReceived?.Invoke();

            // if its not player turn, start timer to check later if other player left the game or not
            if (!IsCurrentPlayer())
            {
                _isWaitForTurnTimerStart = true;
            }

            _queueManager.Active = true;
        }

        private void OnPlayerActionReceivedHandler(byte[] data)
        {
            Func<Task> taskFunc = async () =>
            {
                PlayerActionEvent playerActionEvent = PlayerActionEvent.Parser.ParseFrom(data);
                Debug.LogWarning(playerActionEvent); // todo delete

                if (playerActionEvent.Block != null)
                {
                    foreach (HistoryData historyData in playerActionEvent.Block.List)
                    {
                        HistoryEndGame endGameData = historyData.EndGame;
                        if (endGameData != null)
                        {
                            Debug.Log(endGameData.MatchId + " , " + endGameData.UserId + " , " + endGameData.WinnerId);
                            await _backendFacade.UnsubscribeEvent();
                            return;
                        }
                    }
                }

                switch (playerActionEvent.Match.Status)
                {
                    case Match.Types.Status.Created:
                        MatchCreatedActionReceived?.Invoke();
                        break;
                    case Match.Types.Status.Matching:
                        bool matchCanStart = true;
                        for (int i = 0; i < 2; i++)
                        {
                            if (!playerActionEvent.Match.PlayerStates[i].MatchAccepted)
                            {
                                matchCanStart = false;
                                break;
                            }
                        }
                        if (matchCanStart)
                        {
                            MatchingStartedActionReceived?.Invoke();
                        }
                        break;
                    case Match.Types.Status.Started:
                        //Should not handle this anymore through events for now
                        break;
                    case Match.Types.Status.Playing:
                        OnReceivePlayerActionType(playerActionEvent);
                        break;
                    case Match.Types.Status.PlayerLeft:
                        PlayerLeftGameActionReceived?.Invoke();
                        break;
                    case Match.Types.Status.Ended:
                        GameEndedActionReceived?.Invoke();
                        break;
                    case Match.Types.Status.Canceled:
                        break;
                    case Match.Types.Status.Timedout:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(
                            nameof(playerActionEvent.Match.Status),
                            playerActionEvent.Match.Status + " not found"
                        );
                }
            };

            GameClient.Get<IQueueManager>().AddTask(taskFunc);
        }

        private async Task LoadInitialGameState()
        {
            GetGameStateResponse getGameStateResponse = await _backendFacade.GetGameState(MatchMetadata.Id);
            InitialGameState = getGameStateResponse.GameState;
            Debug.LogWarning("Initial game state:\n" + InitialGameState);
            Debug.LogWarning("Use backend game logic: " + MatchMetadata.UseBackendGameLogic);
        }

        private void OnReceivePlayerActionType(PlayerActionEvent playerActionEvent)
        {
            foreach (PlayerActionOutcome playerActionOutcome in playerActionEvent.PlayerAction.ActionOutcomes)
            {
                Debug.Log(playerActionOutcome.ToString());
                PlayerActionOutcomeReceived?.Invoke(playerActionOutcome);
            }

            if (playerActionEvent.PlayerAction.PlayerId == _backendDataControlMediator.UserDataModel.UserId)
            {
                if (playerActionEvent.PlayerAction.ActionType == PlayerActionType.Types.Enum.EndTurn)
                {
                    _isWaitForTurnTimerStart = true;
                }
                return;
            }

            switch (playerActionEvent.PlayerAction.ActionType)
            {
                case PlayerActionType.Types.Enum.None:
                    break;
                case PlayerActionType.Types.Enum.EndTurn:
                    ResetWaitForTurnTimer();
                    EndTurnActionReceived?.Invoke();
                    break;
                case PlayerActionType.Types.Enum.Mulligan:
                    MulliganProcessUsedActionReceived?.Invoke(playerActionEvent.PlayerAction.Mulligan);
                    break;
                case PlayerActionType.Types.Enum.CardPlay:
                    CardPlayedActionReceived?.Invoke(playerActionEvent.PlayerAction.CardPlay);
                    break;
                case PlayerActionType.Types.Enum.CardAttack:
                    CardAttackedActionReceived?.Invoke(playerActionEvent.PlayerAction.CardAttack);
                    break;
                case PlayerActionType.Types.Enum.CardAbilityUsed:
                    CardAbilityUsedActionReceived?.Invoke(playerActionEvent.PlayerAction.CardAbilityUsed);
                    break;
                case PlayerActionType.Types.Enum.OverlordSkillUsed:
                    OverlordSkillUsedActionReceived?.Invoke(playerActionEvent.PlayerAction.OverlordSkillUsed);
                    break;
                case PlayerActionType.Types.Enum.DrawCard:
                    DrawCardActionReceived?.Invoke(playerActionEvent.PlayerAction.DrawCard);
                    break;
                case PlayerActionType.Types.Enum.LeaveMatch:
                    LeaveMatchReceived?.Invoke();
                    break;
                case PlayerActionType.Types.Enum.RankBuff:
                    RankBuffActionReceived?.Invoke(playerActionEvent.PlayerAction.RankBuff);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(playerActionEvent.PlayerAction.ActionType),
                        playerActionEvent.PlayerAction.ActionType + " not found"
                    );
            }
        }

        private void ResetWaitForTurnTimer()
        {
            _isWaitForTurnTimerStart = false;
            _waitForTurnTimer = 0f;
        }
    }
}
