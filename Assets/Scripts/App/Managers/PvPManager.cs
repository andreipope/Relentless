using System;
using System.Collections.Generic;
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

        public event Action LeaveMatchReceived;

        private BackendFacade _backendFacade;
        private BackendDataControlMediator _backendDataControlMediator;

        public FindMatchResponse MatchResponse { get; set; }
        public GetGameStateResponse GameStateResponse { get; set; }

        public OpponentDeck OpponentDeck { get; set; }
        public int OpponentDeckIndex { get; set; }

        public void Init()
        {
            _backendFacade = GameClient.Get<BackendFacade>();
            _backendDataControlMediator = GameClient.Get<BackendDataControlMediator>();

            _backendFacade.PlayerActionEventListner += OnGetPlayerActionEventListener;

        }

        public void Update()
        {
            
        }

        public void Dispose()
        {

        }

        public bool IsCurrentPlayer()
        {
            if (MatchResponse.Match.PlayerStates[GameStateResponse.GameState.CurrentPlayerIndex].Id ==
                _backendDataControlMediator.UserDataModel.UserId)
                return true;

            return false;
        }

        public string GetOpponentUserId()
        {
            string opponentId = string.Empty;
            for (int i = 0; i < MatchResponse.Match.PlayerStates.Count; i++)
            {
                if (MatchResponse.Match.PlayerStates[i].Id != _backendDataControlMediator.UserDataModel.UserId)
                {
                    opponentId = MatchResponse.Match.PlayerStates[i].Id;
                    break;
                }
            }

            return opponentId;
        }

        private void OnGetPlayerActionEventListener(byte[] data)
        {
            GameClient.Get<IQueueManager>().AddAction(
            () =>
            {
                string jsonStr = SystemText.Encoding.UTF8.GetString(data);

                Debug.LogWarning(jsonStr); // todo delete

                PlayerActionEvent playerActionEvent = JsonConvert.DeserializeObject<PlayerActionEvent>(jsonStr);
                MatchResponse.Match = playerActionEvent.Match.Clone();

                switch (playerActionEvent.Match.Status)
                {
                    case Match.Types.Status.Created:
                        MatchCreatedActionReceived?.Invoke();
                        break;
                    case Match.Types.Status.Matching:
                        MatchingStartedActionReceived?.Invoke();
                        break;
                    case Match.Types.Status.Started:
                        GameStartedActionReceived?.Invoke();
                        break;
                    case Match.Types.Status.Playing:
                        {
                            if (playerActionEvent.UserId == _backendDataControlMediator.UserDataModel.UserId)
                                return;

                            OnReceivePlayerActionType(playerActionEvent);
                        }
                        break;
                    case Match.Types.Status.PlayerLeft:
                        PlayerLeftGameActionReceived?.Invoke();
                        break;
                    case Match.Types.Status.Ended:
                        GameEndedActionReceived?.Invoke();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(playerActionEvent.Match.Status), playerActionEvent.Match.Status.ToString() + " not found");
                }
            });
        }


        private void OnReceivePlayerActionType(PlayerActionEvent playerActionEvent)
        {
            switch (playerActionEvent.PlayerActionType)
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
                default:
                    throw new ArgumentOutOfRangeException(nameof(playerActionEvent.PlayerActionType), playerActionEvent.PlayerActionType.ToString() + " not found");
            }
        }
    }
}
