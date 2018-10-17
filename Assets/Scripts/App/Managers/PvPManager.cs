using System;
using System.Linq;
using System.Threading.Tasks;
using Loom.Client;
using Loom.Newtonsoft.Json;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Data;
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

        public event Action PlayerLeftGameActionReceived;

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

        public OpponentDeck OpponentDeck { get; set; }

        public int OpponentDeckIndex { get; set; }

        public Address? CustomGameModeAddress { get; set; }

        private IUIManager _uiManager;
        private BackendFacade _backendFacade;
        private BackendDataControlMediator _backendDataControlMediator;

        public void Init()
        {
            _uiManager = GameClient.Get<IUIManager>();
            _backendFacade = GameClient.Get<BackendFacade>();
            _backendDataControlMediator = GameClient.Get<BackendDataControlMediator>();

            _backendFacade.PlayerActionDataReceived += OnPlayerActionReceivedHandler;
        }

        public void Update()
        {
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
            string opponentId = string.Empty;
            for (int i = 0; i < InitialGameState.PlayerStates.Count; i++)
            {
                if (InitialGameState.PlayerStates[i].Id != _backendDataControlMediator.UserDataModel.UserId)
                {
                    opponentId = InitialGameState.PlayerStates[i].Id;
                    break;
                }
            }

            return opponentId;
        }

        public async Task FindMatch()
        {
            InitialGameState = null;

            FindMatchResponse findMatchResponse =
                await _backendFacade.FindMatch(
                    _backendDataControlMediator.UserDataModel.UserId,
                    _uiManager.GetPage<GameplayPage>().CurrentDeckId,
                    CustomGameModeAddress
                );
            Debug.LogWarning("FindMatchResponse:\n" + findMatchResponse);

            await _backendFacade.SubscribeEvent(findMatchResponse.Match.Topics.ToList());

            GetMatchResponse getMatchResponse = await _backendFacade.GetMatch(findMatchResponse.Match.Id);
            MatchMetadata = new MatchMetadata(
                findMatchResponse.Match.Id,
                findMatchResponse.Match.Topics,
                getMatchResponse.Match.Status
            );

            if (findMatchResponse.Match.Status != getMatchResponse.Match.Status)
            {
                Debug.Log(
                    $"findMatchResponse.Match.Status = {findMatchResponse.Match.Status}, " +
                    $"getMatchResponse.Match.Status = {getMatchResponse.Match.Status}"
                );
            }

            if (MatchMetadata.Status == Match.Types.Status.Started)
            {
                await LoadInitialGameState();
            }
        }

        private void OnPlayerActionReceivedHandler(byte[] data)
        {
            GameClient.Get<IQueueManager>().AddAction(
                async () =>
                {
                    PlayerActionEvent playerActionEvent = PlayerActionEvent.Parser.ParseFrom(data);
                    Debug.LogWarning("! " + playerActionEvent ); // todo delete

                    switch (playerActionEvent.Match.Status)
                    {
                        case Match.Types.Status.Created:
                            MatchCreatedActionReceived?.Invoke();
                            break;
                        case Match.Types.Status.Matching:
                            MatchingStartedActionReceived?.Invoke();
                            break;
                        case Match.Types.Status.Started:
                            // No need to reload if a match was found immediately
                            if (InitialGameState != null)
                            {
                                await LoadInitialGameState();
                            }

                            GameStartedActionReceived?.Invoke();
                            break;
                        case Match.Types.Status.Playing:
                            if (playerActionEvent.PlayerAction.PlayerId == _backendDataControlMediator.UserDataModel.UserId)
                                return;

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
                });
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
    }
}
