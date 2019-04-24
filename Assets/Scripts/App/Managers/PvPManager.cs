using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using Loom.Client;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Protobuf;
using Loom.ZombieBattleground.Data;
using UnityEngine;
using DebugCheatsConfiguration = Loom.ZombieBattleground.BackendCommunication.DebugCheatsConfiguration;
using SystemText = System.Text;
using Loom.Google.Protobuf.Collections;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Loom.ZombieBattleground.Helpers;

namespace Loom.ZombieBattleground
{
    public class PvPManager : IService, IPvPManager
    {
        private static readonly ILog Log = Logging.GetLog(nameof(PvPManager));

        // matching actions
        public event Action MatchCreatedActionReceived;

        public event Action MatchingStartedActionReceived;

        public event Action<PlayerActionLeaveMatch> PlayerLeftGameActionReceived;

        // game status actions
        public event Action GameStartedActionReceived;

        public event Action GameEndedActionReceived;

        // gameplay actions
        public event Action<Protobuf.GameState> EndTurnActionReceived;

        public event Action<PlayerActionCardPlay> CardPlayedActionReceived;

        public event Action<PlayerActionCardAttack> CardAttackedActionReceived;

        public event Action<PlayerActionOverlordSkillUsed> OverlordSkillUsedActionReceived;

        public event Action<PlayerActionCardAbilityUsed> CardAbilityUsedActionReceived;

        public event Action<PlayerActionMulligan> MulliganProcessUsedActionReceived;

        public event Action<PlayerActionRankBuff> RankBuffActionReceived;

        public event Action<PlayerActionCheatDestroyCardsOnBoard> CheatDestroyCardsOnBoardActionReceived;

        public event Action<PlayerActionOutcome> PlayerActionOutcomeReceived;

        public event Action LeaveMatchReceived;

        public int CurrentActionIndex { get; set; }

        public MatchMetadata MatchMetadata { get; set; }

        public GameState InitialGameState { get; set; }

        public Address? CustomGameModeAddress { get; set; }

        public List<string> PvPTags { get; } = new List<string>();

        public DebugCheatsConfiguration DebugCheats { get; set; } = new DebugCheatsConfiguration();

        public MatchMakingFlowController MatchMakingFlowController => _matchMakingFlowController;

        public bool UseBackendGameLogic { get; set; }

        private BackendFacade _backendFacade;
        private BackendDataControlMediator _backendDataControlMediator;
        private INetworkActionManager _networkActionManager;
        private IGameplayManager _gameplayManager;

        private bool _keepAliveActive;
        private float _nextKeepAliveSendTimer;

        private readonly SemaphoreSlim _matchmakingBusySemaphore = new SemaphoreSlim(1);

        private UIMatchMakingFlowController _matchMakingFlowController;

        public void Init()
        {
            _backendFacade = GameClient.Get<BackendFacade>();
            _networkActionManager = GameClient.Get<INetworkActionManager>();
            _gameplayManager = GameClient.Get<IGameplayManager>();
            _backendDataControlMediator = GameClient.Get<BackendDataControlMediator>();
            _backendFacade.PlayerActionDataReceived += OnPlayerActionReceivedHandler;

            GameClient.Get<IGameplayManager>().GameEnded += GameEndedHandler;
        }

        public async void Update()
        {
            if (!_backendFacade.IsConnected)
                return;

            if (_keepAliveActive)
            {
                _nextKeepAliveSendTimer += Time.unscaledDeltaTime;
                if (_nextKeepAliveSendTimer > Constants.PvPCheckPlayerAvailableMaxTime)
                {
                    _nextKeepAliveSendTimer = 0f;

                    try
                    {
                        await _networkActionManager.ExecuteNetworkTask(() => _backendFacade.KeepAliveStatus(_backendDataControlMediator.UserDataModel.UserId, MatchMetadata.Id));
                    }
                    catch
                    {
                        return;
                    }
                }
            }

            if (_matchMakingFlowController != null)
            {
                await CallAndRestartMatchmakingOnException(() => _matchMakingFlowController.Update(Time.unscaledDeltaTime));
            }
        }

        public void Dispose()
        {
            // This is only used by test framework to restart the game, so it seems fine here to ignore the warning
#pragma warning disable 4014
            StopMatchmaking();
#pragma warning restore 4014
        }

        public bool IsFirstPlayer()
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
            Log.Debug(nameof(GameEndedHandler));

            try
            {
                ResetCheckPlayerStatus();
                await _backendFacade.UnsubscribeEvent();
            }
            catch(Exception e)
            {
                GameClient.Get<IAppStateManager>().HandleNetworkExceptionFlow(e, false, false);
            }
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

                _matchMakingFlowController = new UIMatchMakingFlowController(
                    _backendFacade,
                    _backendDataControlMediator.UserDataModel
                );

                _matchMakingFlowController.MatchConfirmed += MatchMakingFlowControllerOnMatchConfirmed;

                await CallAndRestartMatchmakingOnException(() =>
                    _matchMakingFlowController.Start(deckId, CustomGameModeAddress, PvPTags, UseBackendGameLogic, DebugCheats));
            }
            catch(Exception e)
            {
                Helpers.ExceptionReporter.SilentReportException(e);
            }
            finally
            {
                _matchmakingBusySemaphore.Release();
            }
        }

        public async Task StopMatchmaking()
        {
            Log.Debug(nameof(StopMatchmaking));
            await _matchmakingBusySemaphore.WaitAsync();

            try
            {
                _networkActionManager.Clear();
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

                _networkActionManager.Clear();
            }
            catch(Exception e)
            {
                Log.Warn("", e);
                Helpers.ExceptionReporter.SilentReportException(e);
            }
            finally
            {
                _matchmakingBusySemaphore.Release();
                ResetCheckPlayerStatus();
            }
        }

        private async void MatchMakingFlowControllerOnMatchConfirmed(MatchMetadata matchMetadata)
        {
            Log.Debug($"{nameof(MatchMakingFlowControllerOnMatchConfirmed)}(MatchMetadata matchMetadata = {matchMetadata})");
            _matchMakingFlowController.MatchConfirmed -= MatchMakingFlowControllerOnMatchConfirmed;

            _networkActionManager.Clear();

            InitialGameState = null;

            MatchMetadata = matchMetadata;
            
            // No need to reload if a match was found immediately
            if (InitialGameState == null)
            {
                try
                {
                    await _networkActionManager.EnqueueNetworkTask(LoadInitialGameState, keepCurrentAppState: true);
                }
                catch (Exception e)
                {
                    Log.Warn(e);
                    return;
                }
            }

            Log.Info("Match Starting");

            GameStartedActionReceived?.Invoke();

            _keepAliveActive = true;
        }

        private async Task CallAndRestartMatchmakingOnException(Func<Task> func)
        {
            try
            {
                await _networkActionManager.EnqueueNetworkTask(
                    func,
                    onUnknownExceptionCallbackFunc: async exception =>
                    {
                        Log.Info("Exception not handled, restarting matchmaking:" + exception.Message);
                        await Task.Delay(1000); // avoids endless loop on repeated exceptions
                        await CallAndRestartMatchmakingOnException(() => _matchMakingFlowController.Restart());
                    }
                );
            }
            catch (Exception e)
            {
                Log.Warn(e);
            }
        }

        private async void OnPlayerActionReceivedHandler(byte[] data)
        {
            Func<Task> taskFunc = async () =>
            {
                PlayerActionEvent playerActionEvent = PlayerActionEvent.Parser.ParseFrom(data);
                CurrentActionIndex = (int)playerActionEvent.CurrentActionIndex;
                Log.Debug("[Incoming Player Action]\r\n" + Utilites.JsonPrettyPrint(playerActionEvent.ToString()));

                if (playerActionEvent.Block != null)
                {
                    foreach (HistoryData historyData in playerActionEvent.Block.List)
                    {
                        HistoryEndGame endGameData = historyData.EndGame;
                        if (endGameData != null)
                        {
                            Log.Info(endGameData.MatchId + " , " + endGameData.UserId + " , " + endGameData.WinnerId);
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
                        foreach (PlayerActionOutcome playerActionOutcome in playerActionEvent.PlayerAction.ActionOutcomes)
                        {
                            Log.Info(playerActionOutcome.ToString());
                            PlayerActionOutcomeReceived?.Invoke(playerActionOutcome);
                        }

                        if (playerActionEvent.PlayerAction.PlayerId == _backendDataControlMediator.UserDataModel.UserId)
                        {
                            if (Constants.MulliganEnabled && !DebugCheats.SkipMulligan && playerActionEvent.PlayerAction.ActionType == PlayerActionType.Types.Enum.Mulligan)
                            {
                               List<BoardUnitModel> finalCardsInHand = new List<BoardUnitModel>();
                               int cardsRemoved = 0;
                               bool found;
                               foreach (BoardUnitModel cardInHand in _gameplayManager.CurrentPlayer.CardsPreparingToHand) 
                               {
                                   found = false;
                                   foreach (Protobuf.InstanceId cardNotMulligan in playerActionEvent.PlayerAction.Mulligan.MulliganedCards)
                                   {
                                       if (cardNotMulligan.Id == cardInHand.InstanceId.Id) 
                                       {
                                           finalCardsInHand.Add(cardInHand);
                                           found = true;
                                           break;
                                       }
                                   }
                                   if (!found) 
                                   {
                                       _gameplayManager.CurrentPlayer.PlayerCardsController.AddCardToDeck(cardInHand);
                                       cardsRemoved++;
                                   }
                               }

                               for (int i = 0; i < cardsRemoved; i++)
                               {
                                   finalCardsInHand.Add(_gameplayManager.CurrentPlayer.CardsInDeck[i]);
                               }

                               _gameplayManager.CurrentPlayer.PlayerCardsController.SetCardsPreparingToHand(finalCardsInHand);

                               GameClient.Get<IUIManager>().GetPopup<WaitingForPlayerPopup>().Show("Waiting for the opponent...");

                               return;
                            } else if (playerActionEvent.PlayerAction.ActionType == PlayerActionType.Types.Enum.CheatDestroyCardsOnBoard)
                            {
                                OnReceivePlayerActionType(playerActionEvent);
                            }
                            else
                            {
                                return;
                            }
                        } else {
                            if (Constants.MulliganEnabled && !DebugCheats.SkipMulligan && playerActionEvent.PlayerAction.ActionType == PlayerActionType.Types.Enum.Mulligan)
                            {
                                //TO DO: fix issue with initialization on opponent hand
                                List<BoardUnitModel> cardsToRemove = new List<BoardUnitModel>();
                                bool found;
                                foreach (BoardUnitModel cardInHand in _gameplayManager.OpponentPlayer.CardsInHand)
                                {
                                    found = false;
                                    foreach (Protobuf.InstanceId cardNotMulligan in playerActionEvent.PlayerAction.Mulligan.MulliganedCards)
                                    {
                                        if (cardNotMulligan.Id == cardInHand.InstanceId.Id)
                                        {
                                            found = true;
                                            break;
                                        }
                                    }
                                    if (!found)
                                    {
                                        cardsToRemove.Add(cardInHand);
                                    }
                                }

                                BattlegroundController battlegroundController = _gameplayManager.GetController<BattlegroundController>();

                                foreach (BoardUnitModel card in cardsToRemove)
                                {
                                    _gameplayManager.OpponentPlayer.PlayerCardsController.RemoveCardFromHand(card);
                                    OpponentHandCard opponentHandCard = battlegroundController.OpponentHandCards.FirstOrDefault(x => x.Model.InstanceId == card.InstanceId);
                                    battlegroundController.OpponentHandCards.Remove(opponentHandCard);
                                    opponentHandCard.Dispose();
                                    _gameplayManager.OpponentPlayer.PlayerCardsController.AddCardToDeck(card);
                                }

                                for (int i = 0; i < cardsToRemove.Count; i++)
                                {
                                    _gameplayManager.OpponentPlayer.PlayerCardsController.AddCardFromDeckToHand(_gameplayManager.OpponentPlayer.CardsInDeck[0]);
                                }
                            }
                        }

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

            try
            {
                await _networkActionManager.EnqueueNetworkTask(taskFunc);
            }
            catch
            {
                // No additional handling
            }
        }

        private async Task LoadInitialGameState()
        {
            GetGameStateResponse getGameStateResponse = await _backendFacade.GetGameState(MatchMetadata.Id);
            InitialGameState = getGameStateResponse.GameState;
            Log.Debug("Initial game state:\n" + InitialGameState);
            Log.Debug("Use backend game logic: " + MatchMetadata.UseBackendGameLogic);
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

        private void OnReceivePlayerActionType(PlayerActionEvent playerActionEvent)
        {
            switch (playerActionEvent.PlayerAction.ActionType)
            {
                case PlayerActionType.Types.Enum.None:
                    break;
                case PlayerActionType.Types.Enum.EndTurn:
                    EndTurnActionReceived?.Invoke(playerActionEvent.PlayerAction.ControlGameState);
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
                case PlayerActionType.Types.Enum.LeaveMatch:
                    ResetCheckPlayerStatus();
                    LeaveMatchReceived?.Invoke();
                    break;
                case PlayerActionType.Types.Enum.RankBuff:
                    RankBuffActionReceived?.Invoke(playerActionEvent.PlayerAction.RankBuff);
                    break;
                case PlayerActionType.Types.Enum.CheatDestroyCardsOnBoard:
                    CheatDestroyCardsOnBoardActionReceived?.Invoke(playerActionEvent.PlayerAction.CheatDestroyCardsOnBoard);
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
            Log.Info($"{nameof(ResetCheckPlayerStatus)} ({nameof(_keepAliveActive)} = {_keepAliveActive}, {nameof(_nextKeepAliveSendTimer)} = {_nextKeepAliveSendTimer})");
            _keepAliveActive = false;
            _nextKeepAliveSendTimer = 0f;
        }
    }
}
