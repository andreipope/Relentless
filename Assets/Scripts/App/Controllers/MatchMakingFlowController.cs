using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Protobuf;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class MatchMakingFlowController
    {
        public event Action<MatchMakingState> StateChanged;

        public event Action<MatchMetadata> MatchConfirmed;

        private const string PlayerIsAlreadyInAMatch = "Player is already in a match";
        private const int WaitingTime = 5;

        private readonly BackendFacade _backendFacade;
        private readonly BackendDataControlMediator _backendDataControlMediator;
        private readonly IPvPManager _pvpManager;
        private readonly CancellationTokenSource _cancellationTokenSource;

        private MatchMakingState _state = MatchMakingState.WaitingPeriod;

        private float _currentWaitingTime;
        private long _deckId;

        public MatchMakingState State => _state;

        public bool IsMatchmakingInProcess
        {
            get
            {
                switch (_state)
                {
                    case MatchMakingState.NotStarted:
                    case MatchMakingState.Canceled:
                    case MatchMakingState.Confirmed:
                        return false;
                    case MatchMakingState.RegisteringToPool:
                    case MatchMakingState.WaitingPeriod:
                    case MatchMakingState.FindingMatch:
                    case MatchMakingState.AcceptingMatch:
                    case MatchMakingState.WaitingForOpponent:
                    case MatchMakingState.ConfirmingWithOpponent:
                        return true;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public MatchMakingFlowController(
            BackendFacade backendFacade,
            BackendDataControlMediator backendDataControlMediator,
            IPvPManager pvpManager
            )
        {
            _backendFacade = backendFacade;
            _backendDataControlMediator = backendDataControlMediator;
            _pvpManager = pvpManager;

            _cancellationTokenSource = new CancellationTokenSource();
        }

        public async Task Start(long deckId)
        {
            _deckId = deckId;

            await SetState(MatchMakingState.RegisteringToPool);
            try
            {
                RegisterPlayerPoolResponse result = await _backendFacade.RegisterPlayerPool(
                    _backendDataControlMediator.UserDataModel.UserId,
                    deckId,
                    _pvpManager.CustomGameModeAddress
                );

                await SetState(MatchMakingState.WaitingPeriod);
            }
            catch (Exception e)
            {
                ErrorHandler(e);
            }
        }

        public async Task Stop()
        {
            _cancellationTokenSource.Cancel();
            await SetState(MatchMakingState.Canceled);
        }

        public async Task Update()
        {
            if (await CancelIfNeededAndSetCanceledState())
                return;

            switch (_state)
            {
                case MatchMakingState.WaitingPeriod:
                {
                    _currentWaitingTime += Time.deltaTime;
                    if (_currentWaitingTime > WaitingTime)
                    {
                        _currentWaitingTime = 0f;
                        await SetState(MatchMakingState.FindingMatch);
                    }

                    break;
                }
                case MatchMakingState.WaitingForOpponent:
                {
                    _currentWaitingTime += Time.deltaTime;
                    if (_currentWaitingTime > WaitingTime)
                    {
                        _currentWaitingTime = 0f;
                        await SetState(MatchMakingState.ConfirmingWithOpponent);
                    }

                    break;
                }
            }
        }

        private async Task InitiateAcceptingMatch (long matchId) {
            try
            {
                AcceptMatchResponse result = await _backendFacade.AcceptMatch(
                    _backendDataControlMediator.UserDataModel.UserId,
                    matchId
                );

                Debug.LogWarning(result.ToString());

                await SetState(MatchMakingState.WaitingForOpponent);
                await _backendFacade.SubscribeEvent(result.Match.Topics.ToList());
            }
            catch (Exception e)
            {
                ErrorHandler(e);
                await SetState(MatchMakingState.WaitingPeriod);
            }
        }

        private async Task InitiateFindingMatch()
        {
            try
            {
                FindMatchResponse result = await _backendFacade.FindMatch(
                    _backendDataControlMediator.UserDataModel.UserId
                );

                if (result.Match != null)
                {
                    if (result.Match.Status == Match.Types.Status.Matching)
                    {
                        bool mustAccept = false;
                        for (int i = 0; i < result.Match.PlayerStates.Count; i++)
                        {
                            if (result.Match.PlayerStates[i].Id == _backendDataControlMediator.UserDataModel.UserId)
                            {
                                if (!result.Match.PlayerStates[i].MatchAccepted)
                                {
                                    mustAccept = true;
                                }
                            }
                        }

                        if (mustAccept)
                        {
                            await SetState(MatchMakingState.AcceptingMatch);
                            await InitiateAcceptingMatch(result.Match.Id);
                        }
                        else
                        {
                            await SetState(MatchMakingState.WaitingForOpponent);
                        }
                    }
                }
                else
                {
                    await SetState(MatchMakingState.WaitingForOpponent);
                }
            }
            catch (Exception e)
            {
                ErrorHandler(e);
                await SetState(MatchMakingState.WaitingPeriod);
            }
        }

        private async Task CheckIfOpponentIsReady () {
            try
            {
                FindMatchResponse result = await _backendFacade.FindMatch(
                    _backendDataControlMediator.UserDataModel.UserId
                );

                Debug.LogWarning(result.ToString());

                if (result.Match != null)
                {
                    if (result.Match.Status == Match.Types.Status.Matching)
                    {
                        bool mustAccept = false;
                        bool opponentHasAccepted = false;
                        for (int i = 0; i < result.Match.PlayerStates.Count; i++)
                        {
                            if (result.Match.PlayerStates[i].Id == _backendDataControlMediator.UserDataModel.UserId)
                            {
                                if (!result.Match.PlayerStates[i].MatchAccepted)
                                {
                                    mustAccept = true;
                                }
                            }
                            else
                            {
                                if (result.Match.PlayerStates[i].MatchAccepted)
                                {
                                    opponentHasAccepted = true;
                                }
                            }
                        }

                        if (mustAccept)
                        {
                            await SetState(MatchMakingState.AcceptingMatch);
                            await InitiateAcceptingMatch(result.Match.Id);
                            return;
                        }

                        if (opponentHasAccepted && !mustAccept)
                        {
                            Debug.Log("The Match is Starting!");
                            await StartConfirmedMatch(result);
                        }
                        else
                        {
                            await SetState(MatchMakingState.WaitingForOpponent);
                        }
                    }
                    else if (result.Match.Status == Match.Types.Status.Started) {
                        await StartConfirmedMatch(result);
                    }
                    else
                    {
                        await _backendFacade.UnsubscribeEvent();
                        await Start(_deckId);
                    }
                }
                else
                {
                    await SetState(MatchMakingState.WaitingForOpponent);
                }
            }
            catch (Exception e)
            {
                ErrorHandler(e);
                await SetState(MatchMakingState.WaitingPeriod);
            }
        }

        private async void ErrorHandler (Exception exception) {
            // Just restart the entire process
            // FIXME: why does this error still occur, though?
            if (exception.Message.Contains(PlayerIsAlreadyInAMatch))
            {
                try
                {
                    CancelFindMatchResponse result = await _backendFacade.CancelFindMatchRelatedToUserId(
                        _backendDataControlMediator.UserDataModel.UserId
                    );

                    await Start(_deckId);
                }
                catch (Exception e)
                {
                    ErrorHandler(e);
                }

                return;
            }

            Debug.LogWarning(exception.Message);
        }

        private async Task SetState(MatchMakingState state)
        {
            if (await CancelIfNeededAndSetCanceledState())
                return;

            await SetStateUnchecked(state);
        }

        private async Task SetStateUnchecked(MatchMakingState state)
        {
            Debug.Log("Matchmaking state: " + state);
            _state = state;
            StateChanged?.Invoke(state);

            switch (_state)
            {
                case MatchMakingState.RegisteringToPool:
                    break;
                case MatchMakingState.WaitingPeriod:
                    break;
                case MatchMakingState.FindingMatch:
                    await InitiateFindingMatch();
                    break;
                case MatchMakingState.AcceptingMatch:
                    break;
                case MatchMakingState.WaitingForOpponent:
                    break;
                case MatchMakingState.ConfirmingWithOpponent:
                    await CheckIfOpponentIsReady();
                    break;
                case MatchMakingState.Confirmed:
                    break;
                case MatchMakingState.NotStarted:
                    break;
                case MatchMakingState.Canceled:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private async Task StartConfirmedMatch(FindMatchResponse findMatchResponse)
        {
            await SetState(MatchMakingState.Confirmed);
            MatchMetadata matchMetadata = new MatchMetadata(
                findMatchResponse.Match.Id,
                findMatchResponse.Match.Topics,
                findMatchResponse.Match.Status,
                findMatchResponse.Match.UseBackendGameLogic
            );

            MatchConfirmed?.Invoke(matchMetadata);
        }

        private async Task<bool> CancelIfNeededAndSetCanceledState()
        {
            if (!_cancellationTokenSource.IsCancellationRequested)
                return false;

            if (_state == MatchMakingState.NotStarted ||
                _state == MatchMakingState.Canceled)
                return true;

            await SetStateUnchecked(MatchMakingState.Canceled);
            return true;
        }

        public enum MatchMakingState
        {
            NotStarted,

            Canceled,

            RegisteringToPool,

            WaitingPeriod,

            FindingMatch,

            AcceptingMatch,

            WaitingForOpponent,

            ConfirmingWithOpponent,

            Confirmed
        }
    }
}
