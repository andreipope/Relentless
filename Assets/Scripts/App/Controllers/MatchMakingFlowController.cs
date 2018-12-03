using System;
using System.Linq;
using System.Threading.Tasks;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Protobuf;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class MatchMakingFlowController
    {
        private const string PlayerIsAlreadyInAMatch = "Player is already in a match";
        private const int WaitingTime = 5;

        private readonly BackendFacade _backendFacade;
        private readonly BackendDataControlMediator _backendDataControlMediator;
        private readonly IPvPManager _pvpManager;

        private MatchMakingState _state = MatchMakingState.WaitingPeriod;

        private float _currentWaitingTime;
        private long _deckId;

        public event Action<MatchMakingState> StateChanged;

        public MatchMakingState State => _state;

        public MatchMakingFlowController(BackendFacade backendFacade, BackendDataControlMediator backendDataControlMediator, IPvPManager pvpManager)
        {
            _backendFacade = backendFacade;
            _backendDataControlMediator = backendDataControlMediator;
            _pvpManager = pvpManager;
        }

        public async Task Start(long deckId)
        {
            _deckId = deckId;

            await SetStateAsync(MatchMakingState.RegisteringToPool);
            try
            {
                RegisterPlayerPoolResponse result = await _backendFacade.RegisterPlayerPool(
                    _backendDataControlMediator.UserDataModel.UserId,
                    deckId,
                    _pvpManager.CustomGameModeAddress
                );

                await SetStateAsync(MatchMakingState.WaitingPeriod);
            }
            catch (Exception e)
            {
                ErrorHandler(e);
            }
        }

        public async Task Stop()
        {

        }

        public async Task Update()
        {
            switch (_state)
            {
                case MatchMakingState.WaitingPeriod:
                {
                    _currentWaitingTime += Time.deltaTime;
                    if (_currentWaitingTime > WaitingTime)
                    {
                        await SetStateAsync(MatchMakingState.FindingMatch);
                    }

                    break;
                }
                case MatchMakingState.WaitingForOpponent:
                {
                    _currentWaitingTime += Time.deltaTime;
                    if (_currentWaitingTime > WaitingTime)
                    {
                        await SetStateAsync(MatchMakingState.ConfirmingWithOpponent);
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

                await SetStateAsync(MatchMakingState.WaitingForOpponent);
                await _backendFacade.SubscribeEvent(result.Match.Topics.ToList());
            }
            catch (Exception e)
            {
                ErrorHandler(e);
                await SetStateAsync(MatchMakingState.WaitingPeriod);
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
                            await SetStateAsync(MatchMakingState.AcceptingMatch);
                            await InitiateAcceptingMatch(result.Match.Id);
                        }
                        else
                        {
                            await SetStateAsync(MatchMakingState.WaitingForOpponent);
                        }
                    }
                }
                else
                {
                    await SetStateAsync(MatchMakingState.WaitingForOpponent);
                }
            }
            catch (Exception e)
            {
                ErrorHandler(e);
                await SetStateAsync(MatchMakingState.WaitingPeriod);
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
                            await SetStateAsync(MatchMakingState.AcceptingMatch);
                            await InitiateAcceptingMatch(result.Match.Id);
                            return;
                        }

                        if (opponentHasAccepted && !mustAccept)
                        {
                            Debug.Log("The Match is Starting!");
                            StartConfirmedMatch(result);
                        }
                        else
                        {
                            SetStateAsync(MatchMakingState.WaitingForOpponent);
                        }
                    }
                    else if (result.Match.Status == Match.Types.Status.Started) {
                        StartConfirmedMatch(result);
                    }
                    else
                    {
                        await _backendFacade.UnsubscribeEvent();
                        await Start(_deckId);
                    }
                }
                else
                {
                    await SetStateAsync(MatchMakingState.WaitingForOpponent);
                }
            }
            catch (Exception e)
            {
                ErrorHandler(e);
                await SetStateAsync(MatchMakingState.WaitingPeriod);
            }
        }

        private async void ErrorHandler (Exception exception) {
            Debug.LogWarning(exception.Message);

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
            }
        }

        private async Task SetStateAsync(MatchMakingState state)
        {
            _state = state;
            switch (_state)
            {
                case MatchMakingState.RegisteringToPool:
                    break;
                case MatchMakingState.WaitingPeriod:
                    _currentWaitingTime = 0;
                    break;
                case MatchMakingState.FindingMatch:
                    break;
                case MatchMakingState.AcceptingMatch:
                    break;
                case MatchMakingState.WaitingForOpponent:
                    _currentWaitingTime = 0;
                    break;
                case MatchMakingState.ConfirmingWithOpponent:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

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
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void StartConfirmedMatch(FindMatchResponse findMatchResponse)
        {
            GameClient.Get<IPvPManager>().MatchIsStarting(findMatchResponse);
        }

        public enum MatchMakingState
        {
            RegisteringToPool,

            WaitingPeriod,

            FindingMatch,

            AcceptingMatch,

            WaitingForOpponent,

            ConfirmingWithOpponent
        }
    }
}
