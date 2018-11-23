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
using Loom.ZombieBattleground.Data;
using UnityEngine;
using Card = Loom.ZombieBattleground.Data.Card;
using Deck = Loom.ZombieBattleground.Data.Deck;
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

        public event Action LeaveMatchReceived;

        public MatchMetadata MatchMetadata { get; set; }

        public GameState InitialGameState { get; set; }

        public Address? CustomGameModeAddress { get; set; }

        private IUIManager _uiManager;
        private IDataManager _dataManager;
        private BackendFacade _backendFacade;
        private BackendDataControlMediator _backendDataControlMediator;
        private IQueueManager _queueManager;
        private IGameplayManager _gameplayManager;

        private CancellationTokenSource _matchmakingCancellationTokenSource;
        private bool _isMatchmakingInProgress;
        private float _matchmakingTimeoutCounter;

        private bool _isWaitForTurnTimerStart;
        private float _waitForTurnTimer;

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
            _isWaitForTurnTimerStart = false;
            await _backendFacade.UnsubscribeEvent();
        }

        public async Task<bool> DebugFindMatch(Deck deck)
        {
            long? matchId = null;
            try
            {
                _matchmakingCancellationTokenSource?.Dispose();
                _matchmakingCancellationTokenSource = new CancellationTokenSource();
                _isMatchmakingInProgress = true;
                _matchmakingTimeoutCounter = 0;

                _queueManager.Active = false;
                _queueManager.Clear();

                InitialGameState = null;
                MatchMetadata = null;

                FindMatchResponse findMatchResponse =
                    await _backendFacade.DebugFindMatch(
                        _backendDataControlMediator.UserDataModel.UserId,
                        deck,
                        CustomGameModeAddress
                    );

                matchId = findMatchResponse.Match.Id;
                if (_matchmakingCancellationTokenSource.IsCancellationRequested)
                    return false;

                await _backendFacade.SubscribeEvent(findMatchResponse.Match.Topics.ToList());
                if (_matchmakingCancellationTokenSource.IsCancellationRequested)
                    return false;

                GetMatchResponse getMatchResponse = await _backendFacade.GetMatch(findMatchResponse.Match.Id);
                if (_matchmakingCancellationTokenSource.IsCancellationRequested)
                    return false;

                MatchMetadata = new MatchMetadata(
                    findMatchResponse.Match.Id,
                    findMatchResponse.Match.Topics,
                    getMatchResponse.Match.Status
                );

                if (MatchMetadata.Status == Match.Types.Status.Started)
                {
                    await LoadInitialGameState();
                    if (_matchmakingCancellationTokenSource.IsCancellationRequested)
                        return false;

                    _isMatchmakingInProgress = false;
                }
            }
            catch (Exception)
            {
                await StopMatchmaking(matchId);
                throw;
            }
            finally
            {
                _queueManager.Active = true;
            }

            return true;
        }


        public async Task CancelFindMatch()
        {
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
                await _backendFacade.CancelFindMatch(
                    _backendDataControlMediator.UserDataModel.UserId,
                    matchIdToCancel.Value
                );
            }

            _queueManager.Clear();
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
                        {
                            if (playerActionEvent.PlayerAction.ActionType == PlayerActionType.Types.Enum.EndTurn)
                            {
                                _isWaitForTurnTimerStart = true;
                            } else if (playerActionEvent.PlayerAction.ActionType == PlayerActionType.Types.Enum.Mulligan)
                            {
                                InitialPlayerState playerState = playerActionEvent.Match.PlayerStates.First(state =>
                                state.Id == _backendDataControlMediator.UserDataModel.UserId);
                                                                                                     
                                _gameplayManager.CurrentPlayer.CardsInDeck = new List<WorkingCard>();

                                /*
                                foreach (CardInstance cardInstance in playerState.CardsInDeck)
                                {
                                    _gameplayManager.CurrentPlayer.CardsInDeck.Add(cardInstance.FromProtobuf(_gameplayManager.CurrentPlayer));
                                }
                                */
                            }
                            return;
                        } else {
                            if (playerActionEvent.PlayerAction.ActionType == PlayerActionType.Types.Enum.Mulligan)
                            {
                                InitialPlayerState playerState = playerActionEvent.Match.PlayerStates.First(state =>
                                state.Id != _backendDataControlMediator.UserDataModel.UserId);

                                _gameplayManager.OpponentPlayer.CardsInDeck = new List<WorkingCard>();

                                /*
                                foreach (CardInstance cardInstance in playerState.)
                                {
                                    _gameplayManager.OpponentPlayer.CardsInDeck.Add(cardInstance.FromProtobuf(_gameplayManager.OpponentPlayer));
                                }

                                _gameplayManager.OpponentPlayer.CardsInHand = new List<WorkingCard>();

                                foreach (CardInstance cardInstance in playerState.CardsInHand)
                                {
                                    _gameplayManager.OpponentPlayer.CardsInHand.Add(cardInstance.FromProtobuf(_gameplayManager.OpponentPlayer));
                                }
                                */
                            }
                        }

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
            _isInternetBroken = false;
            GetGameStateResponse getGameStateResponse = await _backendFacade.GetGameState(MatchMetadata.Id);
            InitialGameState = getGameStateResponse.GameState;
            Debug.LogWarning("Initial game state:\n" + InitialGameState);
        }

        private void OnReceivePlayerActionType(PlayerActionEvent playerActionEvent)
        {
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
