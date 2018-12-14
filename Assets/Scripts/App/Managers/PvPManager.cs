using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Loom.Client;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Protobuf;
using UnityEngine;
using SystemText = System.Text;

namespace Loom.ZombieBattleground
{
    public class PvPManager : IService, IPvPManager
    {
        // matching actions
        public event Action MatchCreatedActionReceived;

        public event Action MatchingStartedActionReceived;

        public event Action<PlayerActionLeaveMatch> PlayerLeftGameActionReceived;

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

        public event Action LeaveMatchReceived;

        public MatchMetadata MatchMetadata { get; set; }

        public GameState InitialGameState { get; set; }

        public Address? CustomGameModeAddress { get; set; }

        private bool _isCheckPlayerAvailableTimerStart;

        public List<string> PvPTags { get; set; }

        private IUIManager _uiManager;
        private IDataManager _dataManager;
        private BackendFacade _backendFacade;
        private BackendDataControlMediator _backendDataControlMediator;
        private IQueueManager _queueManager;
        private IGameplayManager _gameplayManager;

        private CancellationTokenSource _matchmakingCancellationTokenSource;
        private bool _isMatchmakingInProgress;
        private float _matchmakingTimeoutCounter;

        private float _checkPlayerTimer;

        private bool _isInternetBroken = false;
        private float _checkInternetInterval = 5f;
        private float _elapsedInternetCheckTime;

        public void Init()
        {
            _uiManager = GameClient.Get<IUIManager>();
            _dataManager = GameClient.Get<IDataManager>();
            _backendFacade = GameClient.Get<BackendFacade>();
            _queueManager = GameClient.Get<IQueueManager>();
            _backendDataControlMediator = GameClient.Get<BackendDataControlMediator>();
            _gameplayManager = GameClient.Get<IGameplayManager>();
            _backendFacade.PlayerActionDataReceived += OnPlayerActionReceivedHandler;

            //TODO uncomment this line once we decide to start using KeepAlive
            //_gameplayManager.GameInitialized += GameInitializedHandler;

            if (PvPTags == null)
            {
                PvPTags = new List<string> ();
            }

            GameClient.Get<IGameplayManager>().GameEnded += GameEndedHandler;
        }

        public async void Update()
        {
            if (_isCheckPlayerAvailableTimerStart && !_gameplayManager.IsGameEnded) 
            {
                _checkPlayerTimer += Time.deltaTime;
                if (_checkPlayerTimer > Constants.PvPCheckPlayerAvailableMaxTime)
                {
                    _checkPlayerTimer = 0f;

                    try
                    {
                        await _backendFacade.KeepAliveStatus(_backendDataControlMediator.UserDataModel.UserId, MatchMetadata.Id);
                    }
                    catch(Exception ex)
                    {
                        Debug.LogWarning($"keep alive error: {ex.Message} ->> {ex.StackTrace}");
                    }
                }
            }

            if (_gameplayManager.CurrentPlayer != null && !_isInternetBroken)
            {
                _elapsedInternetCheckTime += Time.deltaTime;
                if (_elapsedInternetCheckTime >= _checkInternetInterval)
                {
                    if (Application.internetReachability == NetworkReachability.NotReachable)
                    {
                        _isInternetBroken = true;
                        _backendFacade.ShowConnectionPopup();

                    }
                    _elapsedInternetCheckTime = 0f;
                }
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
            ResetCheckPlayerStatus();
            await _backendFacade.UnsubscribeEvent();
        }

        private void GameInitializedHandler()
        {
            _isCheckPlayerAvailableTimerStart = true;
        }

        public async Task CancelFindMatch()
        {
            ResetCheckPlayerStatus();
            await StopMatchmaking(MatchMetadata?.Id);
        }

        private async Task StopMatchmaking(long? matchIdToCancel)
        {
            _queueManager.Active = false;
            _isMatchmakingInProgress = false;
            _matchmakingCancellationTokenSource?.Cancel();

            await _backendFacade.UnsubscribeEvent();
            if (matchIdToCancel != null)
            {
                try
                {
                    await _backendFacade.CancelFindMatch(
                        _backendDataControlMediator.UserDataModel.UserId,
                        matchIdToCancel.Value
                    );
                }
                catch(Exception e)
                {
                    Debug.Log("save deck exception === " + e.Message);
                }
            }

            _queueManager.Clear();
            ResetCheckPlayerStatus();
        }

        //TODO This method is a start to simplify and clean up
        public async void MatchIsStarting (FindMatchResponse findMatchResponse) {
            _matchmakingTimeoutCounter = 0;

            _queueManager.Active = false;
            _queueManager.Clear();

            InitialGameState = null;
            MatchMetadata = null;

            MatchMetadata = new MatchMetadata(
                findMatchResponse.Match.Id,
                findMatchResponse.Match.Topics,
                findMatchResponse.Match.Status
            );

            // No need to reload if a match was found immediately
            if (InitialGameState == null)
            {
                await LoadInitialGameState();
            }

            Debug.LogWarning("Match Starting");

            GameStartedActionReceived?.Invoke();

            _queueManager.Active = true;
        }

        private void OnPlayerActionReceivedHandler(byte[] data)
        {
            Func<Task> taskFunc = async () =>
            {
                PlayerActionEvent playerActionEvent = PlayerActionEvent.Parser.ParseFrom(data);
                Debug.LogWarning("! " + playerActionEvent); // todo delete

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
                        if (playerActionEvent.PlayerAction.PlayerId == _backendDataControlMediator.UserDataModel.UserId)
                            return;

                        OnReceivePlayerActionType(playerActionEvent);
                        break;
                    case Match.Types.Status.PlayerLeft:
                        OnReceivePlayerLeftAction(playerActionEvent);
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

        private void OnReceivePlayerLeftAction(PlayerActionEvent playerActionEvent)
        {
            switch (playerActionEvent.PlayerAction.ActionType)
            {
                case PlayerActionType.Types.Enum.LeaveMatch:
                    ResetCheckPlayerStatus();
                    PlayerLeftGameActionReceived?.Invoke(playerActionEvent.PlayerAction.LeaveMatch);
                    break;
            }
        }

        private async Task LoadInitialGameState()
        {
            _isInternetBroken = false;

            try
            {
                GetGameStateResponse getGameStateResponse = await _backendFacade.GetGameState(MatchMetadata.Id);
                InitialGameState = getGameStateResponse.GameState;

                Debug.LogWarning("Initial game state:\n" + InitialGameState);
            }
            catch(Exception e)
            {
                Debug.Log("save deck exception === " + e.Message);
            }
        }

        private void OnReceivePlayerActionType(PlayerActionEvent playerActionEvent)
        {
            switch (playerActionEvent.PlayerAction.ActionType)
            {
                case PlayerActionType.Types.Enum.None:
                    break;
                case PlayerActionType.Types.Enum.EndTurn:
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
                    _gameplayManager.GetController<ActionsQueueController>().ClearActions();
                    ResetCheckPlayerStatus();
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

        private void ResetCheckPlayerStatus()
        {
            _isCheckPlayerAvailableTimerStart = false;
            _checkPlayerTimer = 0f;
        }
    }
}
