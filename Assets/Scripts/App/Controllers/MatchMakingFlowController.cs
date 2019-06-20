using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using Loom.Client;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.Protobuf;
using DebugCheatsConfiguration = Loom.ZombieBattleground.BackendCommunication.DebugCheatsConfiguration;

namespace Loom.ZombieBattleground {
    /// <summary>
    /// Executes the matchmaking sequences, notices about state changes and successful match.
    /// </summary>
    public class MatchMakingFlowController
    {
        private static readonly ILog Log = Logging.GetLog(nameof(MatchMakingFlowController));

        protected readonly BackendFacade _backendFacade;
        protected readonly UserDataModel _userDataModel;
        private CancellationTokenSource _cancellationTokenSource;
        private MatchMakingState _state = MatchMakingState.NotStarted;
        private MatchMetadata _matchMetadata;
        private float _currentWaitingTime;
        private DeckId _deckId;
        private Address? _customGameModeAddress;
        private IList<string> _tags;
        private bool _useBackendGameLogic;
        private DebugCheatsConfiguration _debugCheats;

        // TODO: use a setup object?
        public MatchMakingFlowController(
            BackendFacade backendFacade,
            UserDataModel userDataModel)
        {
            _backendFacade = backendFacade;
            _userDataModel = userDataModel;

            _cancellationTokenSource = new CancellationTokenSource();
        }

        private const string PlayerIsAlreadyInAMatch = "Player is already in a match";
        private const string PlayerIsNotInPool = "Player not found in player pool";

        public event Action<MatchMakingState> StateChanged;
        public event Action<MatchMetadata> MatchConfirmed;

        public float ActionWaitingTime { get; set; } = 3.5f;

        public MatchMakingState State => _state;

        public MatchMetadata MatchMetadata
        {
            get
            {
                if (_state != MatchMakingState.Confirmed)
                    throw new Exception($"Must be in {nameof(MatchMakingState.Confirmed)} state");

                return _matchMetadata;
            }
        }

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

        protected CancellationTokenSource CancellationTokenSource => _cancellationTokenSource;

        public async Task Start(
            DeckId deckId,
            Address? customGameModeAddress,
            IList<string> tags,
            bool useBackendGameLogic,
            DebugCheatsConfiguration debugCheats)
        {
            Log.Debug("Start");
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();

            _matchMetadata = null;
            _deckId = deckId;
            _customGameModeAddress = customGameModeAddress;
            _tags = tags;
            _useBackendGameLogic = useBackendGameLogic;
            _debugCheats = debugCheats;

            await SetState(MatchMakingState.RegisteringToPool);
            await RegisterPlayerToPool();
        }

        public async Task Stop()
        {
            Log.Debug("Stop");
            _cancellationTokenSource.Cancel();
            await SetState(MatchMakingState.Canceled);
        }

        public Task Restart()
        {
            Log.Debug("Restart");
            return Start(_deckId, _customGameModeAddress, _tags, _useBackendGameLogic, _debugCheats);
        }

        /// <remarks>Gets deltaTime from outside because Time.deltaTime is always 0 in Editor.</remarks>
        public async Task Update(float deltaTime)
        {
            if (await CancelIfNeededAndSetCanceledState())
                return;

            switch (_state)
            {
                case MatchMakingState.WaitingPeriod:
                {
                    _currentWaitingTime += deltaTime;
                    if (_currentWaitingTime > ActionWaitingTime)
                    {
                        _currentWaitingTime = 0f;
                        await SetState(MatchMakingState.FindingMatch);
                    }

                    break;
                }
                case MatchMakingState.WaitingForOpponent:
                {
                    _currentWaitingTime += deltaTime;
                    if (_currentWaitingTime > ActionWaitingTime)
                    {
                        _currentWaitingTime = 0f;
                        await SetState(MatchMakingState.ConfirmingWithOpponent);
                    }

                    break;
                }
            }
        }

        protected async Task RegisterPlayerToPool()
        {
            try
            {
                Log.Debug("RegisterPlayerToPool");
                RegisterPlayerPoolResponse result = await _backendFacade.RegisterPlayerPool(
                    _userDataModel.UserId,
                    _deckId,
                    _customGameModeAddress,
                    _tags,
                    _useBackendGameLogic,
                    _debugCheats
                );

                await SetState(MatchMakingState.WaitingPeriod);
            }
            catch (Exception e)
            {
                await ErrorFirstChanceHandler(e);
            }
        }

        protected async Task InitiateAcceptingMatch (long matchId) {
            try
            {
                Log.Debug("InitiateAcceptingMatch");
                AcceptMatchResponse result = await _backendFacade.AcceptMatch(
                    _userDataModel.UserId,
                    matchId
                );

                await SetState(MatchMakingState.WaitingForOpponent);
                await _backendFacade.SubscribeEvent(result.Match.Topics.ToList());
            }
            catch (Exception e)
            {
                await ErrorFirstChanceHandler(e);
                await SetState(MatchMakingState.WaitingPeriod);
            }
        }

        protected async Task InitiateFindingMatch()
        {
            try
            {
                Log.Debug("InitiateFindingMatch");
                FindMatchResponse result = await _backendFacade.FindMatch(
                    _userDataModel.UserId,
                    _tags
                );

                if (result.Match != null)
                {
                    if (result.Match.Status == Match.Types.Status.Matching)
                    {
                        bool mustAccept = false;
                        for (int i = 0; i < result.Match.PlayerStates.Count; i++)
                        {
                            if (result.Match.PlayerStates[i].Id == _userDataModel.UserId)
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
                await ErrorFirstChanceHandler(e);
                await SetState(MatchMakingState.WaitingPeriod);
            }
        }

        protected async Task CheckIfOpponentIsReady() {
            try
            {
                Log.Debug("CheckIfOpponentIsReady");
                FindMatchResponse result = await _backendFacade.FindMatch(
                    _userDataModel.UserId,
                    _tags
                );

                if (result.Match != null)
                {
                    if (result.Match.Status == Match.Types.Status.Matching)
                    {
                        bool mustAccept = false;
                        bool opponentHasAccepted = false;
                        for (int i = 0; i < result.Match.PlayerStates.Count; i++)
                        {
                            if (result.Match.PlayerStates[i].Id == _userDataModel.UserId)
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
                            Log.Info("The Match is Starting!");
                            await ConfirmMatch(result);
                        }
                        else
                        {
                            await SetState(MatchMakingState.WaitingForOpponent);
                        }
                    }
                    else if (result.Match.Status == Match.Types.Status.Started) {
                        await ConfirmMatch(result);
                    }
                    else
                    {
                        await _backendFacade.UnsubscribeEvent();
                        await Restart();
                    }
                }
                else
                {
                    await SetState(MatchMakingState.WaitingForOpponent);
                }
            }
            catch (Exception e)
            {
                await ErrorFirstChanceHandler(e);
                await SetState(MatchMakingState.WaitingPeriod);
            }
        }

        protected async Task SetState(MatchMakingState state)
        {
            if (await CancelIfNeededAndSetCanceledState())
                return;

            Log.Debug("SetState: " + state);
            await SetStateUnchecked(state);
        }

        protected async Task SetStateUnchecked(MatchMakingState state)
        {
            _state = state;
            StateChanged?.Invoke(state);

            switch (_state)
            {
                case MatchMakingState.RegisteringToPool:
                    break;
                case MatchMakingState.WaitingPeriod:
                    _currentWaitingTime = 0f;
                    break;
                case MatchMakingState.FindingMatch:
                    await InitiateFindingMatch();
                    break;
                case MatchMakingState.AcceptingMatch:
                    break;
                case MatchMakingState.WaitingForOpponent:
                    _currentWaitingTime = 0f;
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

        protected async Task ConfirmMatch(FindMatchResponse findMatchResponse)
        {
            _matchMetadata = new MatchMetadata(
                findMatchResponse.Match.Id,
                findMatchResponse.Match.Topics,
                findMatchResponse.Match.Status,
                findMatchResponse.Match.UseBackendGameLogic
            );

            await SetState(MatchMakingState.Confirmed);

            Log.Debug("MatchConfirmed");
            
            MTwister.RandomInit((uint)findMatchResponse.Match.RandomSeed);

            MatchConfirmed?.Invoke(_matchMetadata);
        }

        protected async Task<bool> CancelIfNeededAndSetCanceledState()
        {
            if (!_cancellationTokenSource.IsCancellationRequested)
                return false;

            if (_state == MatchMakingState.NotStarted ||
                _state == MatchMakingState.Canceled)
                return true;

            await SetStateUnchecked(MatchMakingState.Canceled);
            return true;
        }

        protected virtual async Task ErrorFirstChanceHandler (Exception exception)
        {
            if (_cancellationTokenSource.IsCancellationRequested)
                return;

            // Just restart the entire process
            // FIXME: why does this error still occur, though?
            if (IsKnownIgnorableException(exception))
            {
                try
                {
                    await _backendFacade.CancelFindMatchRelatedToUserId(
                        _userDataModel.UserId
                    );

                    await Restart();
                }
                catch(TimeoutException e)
                {
                    await ErrorSecondChanceHandler(e);
                }
                catch (Exception e)
                {
                    await ErrorFirstChanceHandler(e);
                }
            }
            else
            {
                await ErrorSecondChanceHandler(exception);
            }
        }

        protected virtual Task ErrorSecondChanceHandler (Exception exception)
        {
            throw exception;
        }

        protected bool IsKnownIgnorableException(Exception exception)
        {
            return
                exception.Message.Contains(PlayerIsAlreadyInAMatch) ||
                exception.Message.Contains(PlayerIsNotInPool);
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
