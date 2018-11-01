using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Loom.Client;
using Loom.Newtonsoft.Json;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.Protobuf;
using UnityEngine;
using Card = Loom.ZombieBattleground.Data.Card;
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

        private CancellationTokenSource _matchmakingCancellationTokenSource;
        private bool _isMatchmakingInProgress;
        private float _matchmakingTimeoutCounter;

        private bool _isWaitForTurnTimerStart;
        private float _waitForTurnTimer;

        public void Init()
        {
            _uiManager = GameClient.Get<IUIManager>();
            _dataManager = GameClient.Get<IDataManager>();
            _backendFacade = GameClient.Get<BackendFacade>();
            _queueManager = GameClient.Get<IQueueManager>();
            _backendDataControlMediator = GameClient.Get<BackendDataControlMediator>();

            _backendFacade.PlayerActionDataReceived += OnPlayerActionReceivedHandler;
        }

        public async void Update()
        {
            if (_isMatchmakingInProgress)
            {
                _matchmakingTimeoutCounter += Time.deltaTime;
                if (_matchmakingTimeoutCounter > Constants.MatchmakingTimeOut)
                {
                    await StopMatchmaking(MatchMetadata?.Id);
                    MatchingFailed?.Invoke();
                }
            }

            if (_isWaitForTurnTimerStart)
            {
                _waitForTurnTimer += Time.deltaTime;
                if (_waitForTurnTimer > Constants.PvPWaitForTurnMaxTime)
                {
                    ResetWaitForTurnTimer();
                    await _backendFacade.CheckPlayerStatus(MatchMetadata.Id);
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

        public async Task<bool> FindMatch()
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
                    await _backendFacade.FindMatch(
                        _backendDataControlMediator.UserDataModel.UserId,
                        _uiManager.GetPage<GameplayPage>().CurrentDeckId,
                        CustomGameModeAddress
                    );
                Debug.LogWarning("FindMatchResponse:\n" + findMatchResponse);
                matchId = findMatchResponse.Match.Id;
                if (_matchmakingCancellationTokenSource.IsCancellationRequested)
                    return false;

                await _backendFacade.SubscribeEvent(findMatchResponse.Match.Topics.ToList());
                Debug.LogWarning("SubscribeEvent complete");
                if (_matchmakingCancellationTokenSource.IsCancellationRequested)
                    return false;

                GetMatchResponse getMatchResponse = await _backendFacade.GetMatch(findMatchResponse.Match.Id);
                Debug.LogWarning("GetMatch complete, status: " + getMatchResponse.Match.Status);
                if (_matchmakingCancellationTokenSource.IsCancellationRequested)
                    return false;

                MatchMetadata = new MatchMetadata(
                    findMatchResponse.Match.Id,
                    findMatchResponse.Match.Topics,
                    getMatchResponse.Match.Status
                );

                if (MatchMetadata.Status == Match.Types.Status.Started)
                {
                    Debug.LogWarning("Status == Started, loading initial state immediately");
                    await LoadInitialGameState();
                    if (_matchmakingCancellationTokenSource.IsCancellationRequested)
                        return false;

                    _isMatchmakingInProgress = false;

                    // if its not player turn, start timer to check later if other player left the game or not
                    if (!IsCurrentPlayer())
                    {
                        _isWaitForTurnTimerStart = true;
                    }

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

        public WorkingCard GetWorkingCardFromCardInstance(CardInstance cardInstance, Player ownerPlayer)
        {
            Card card = _dataManager.CachedCardsLibraryData.GetCardFromName(cardInstance.Prototype.Name).Clone();
            // FIXME: fill with Prototype data when backend supports that
            /*card.Damage = cardInstance.Prototype.InitialDamage;
            card.Health = cardInstance.Prototype.InitialDefence;*/
            card.Damage = cardInstance.Attack;
            card.Health = cardInstance.Defense;
            card.Cost = cardInstance.GooCost;

            WorkingCard workingCard =
                new WorkingCard(
                    card,
                    ownerPlayer,
                    cardInstance.InstanceId
                );

            workingCard.Health = workingCard.InitialHealth = cardInstance.Defense;
            workingCard.Damage = workingCard.InitialDamage = cardInstance.Attack;

            return workingCard;
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

        private void OnPlayerActionReceivedHandler(byte[] data)
        {
            Func<Task> taskFunc = async () =>
            {
                string jsonStr = SystemText.Encoding.UTF8.GetString(data);

                Debug.LogWarning("Action json recieve = " + jsonStr); // todo delete

                PlayerActionEvent playerActionEvent = JsonConvert.DeserializeObject<PlayerActionEvent>(jsonStr);
                foreach(HistoryData historyData in playerActionEvent.Block.List)
                {
                    HistoryEndGame endGameData = historyData.EndGame;
                    if(endGameData != null)
                    {
                        Debug.LogError(endGameData.MatchId + " , " + endGameData.UserId + " , " + endGameData.WinnerId);
                        await _backendFacade.UnsubscribeEvent();
                        return;
                    }
                }

                switch (playerActionEvent.Match.Status)
                {
                    case Match.Types.Status.Created:
                        MatchCreatedActionReceived?.Invoke();
                        break;
                    case Match.Types.Status.Matching:
                        MatchingStartedActionReceived?.Invoke();
                        break;
                    case Match.Types.Status.Started:
                        _isMatchmakingInProgress = false;

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

                        break;
                    case Match.Types.Status.Playing:
                        if (playerActionEvent.PlayerAction.PlayerId == _backendDataControlMediator.UserDataModel.UserId)
                        {
                            if (playerActionEvent.PlayerAction.ActionType == PlayerActionType.EndTurn)
                            {
                                _isWaitForTurnTimerStart = true;
                            }
                            return;
                        }

                        OnReceivePlayerActionType(playerActionEvent);
                        break;
                    case Match.Types.Status.PlayerLeft:
                        PlayerLeftGameActionReceived?.Invoke();
                        break;
                    case Match.Types.Status.Ended:
                        GameEndedActionReceived?.Invoke();
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
        }

        private void OnReceivePlayerActionType(PlayerActionEvent playerActionEvent)
        {
            switch (playerActionEvent.PlayerAction.ActionType)
            {
                case PlayerActionType.NoneAction:
                    break;
                case PlayerActionType.EndTurn:
                    ResetWaitForTurnTimer();
                    EndTurnActionReceived?.Invoke();
                    break;
                case PlayerActionType.Mulligan:
                    MulliganProcessUsedActionReceived?.Invoke(playerActionEvent.PlayerAction.Mulligan);
                    break;
                case PlayerActionType.CardPlay:
                    CardPlayedActionReceived?.Invoke(playerActionEvent.PlayerAction.CardPlay);
                    break;
                case PlayerActionType.CardAttack:
                    CardAttackedActionReceived?.Invoke(playerActionEvent.PlayerAction.CardAttack);
                    break;
                case PlayerActionType.CardAbilityUsed:
                    CardAbilityUsedActionReceived?.Invoke(playerActionEvent.PlayerAction.CardAbilityUsed);
                    break;
                case PlayerActionType.OverlordSkillUsed:
                    OverlordSkillUsedActionReceived?.Invoke(playerActionEvent.PlayerAction.OverlordSkillUsed);
                    break;
                case PlayerActionType.DrawCard:
                    DrawCardActionReceived?.Invoke(playerActionEvent.PlayerAction.DrawCard);
                    break;
                case PlayerActionType.LeaveMatch:
                    LeaveMatchReceived?.Invoke();
                    break;
                case PlayerActionType.RankBuff:
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
