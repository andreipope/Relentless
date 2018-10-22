using System;
using System.Collections.Generic;
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

        public List<CardInstance> OpponentCardsInHand { get; set; }

        public List<CardInstance> OpponentCardsInDeck { get; set; }

        public List<CardInstance> PlayerCardsInHand { get; set; }

        public List<CardInstance> PlayerCardsInDeck { get; set; }

        public int OpponentDeckIndex { get; set; }

        public Address? CustomGameModeAddress { get; set; }

        private IUIManager _uiManager;
        private BackendFacade _backendFacade;
        private BackendDataControlMediator _backendDataControlMediator;
        private IQueueManager _queueManager;

        public void Init()
        {
            _uiManager = GameClient.Get<IUIManager>();
            _backendFacade = GameClient.Get<BackendFacade>();
            _queueManager = GameClient.Get<IQueueManager>();
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
            try
            {
                _queueManager.Active = false;
                _queueManager.Clear();

                InitialGameState = null;

                OpponentCardsInHand = new List<CardInstance>();
                OpponentCardsInDeck = new List<CardInstance>();
                PlayerCardsInHand = new List<CardInstance>();
                PlayerCardsInDeck = new List<CardInstance>();
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
            catch (Exception)
            {
                _queueManager.Clear();
                throw;
            }
            finally
            {
                _queueManager.Active = true;
            }
        }

        private void OnPlayerActionReceivedHandler(byte[] data)
        {
            Action action = async () =>
            {
                string jsonStr = SystemText.Encoding.UTF8.GetString(data);

                Debug.LogWarning("Action json recieve = " + jsonStr); // todo delete

                if (!jsonStr.ToLower().Contains("actiontype") && jsonStr.ToLower().Contains("winnerid"))
                {
                    MatchEndEvent matchEndEvent = JsonConvert.DeserializeObject<MatchEndEvent>(jsonStr);

                    Debug.LogError(matchEndEvent.MatchId + " , " + matchEndEvent.UserId + " , " + matchEndEvent.WinnerId);
                    GameClient.Get<IQueueManager>().StopNetworkThread();
                    return;
                }

                PlayerActionEvent playerActionEvent = JsonConvert.DeserializeObject<PlayerActionEvent>(jsonStr);

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
                        if (InitialGameState == null)
                        {
                            await LoadInitialGameState();
                        }

                        Debug.LogWarning("Match Starting");

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
            };

            GameClient.Get<IQueueManager>().AddAction(action);
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
                    Debug.LogError("== Recieved msg for card Play == ");
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
