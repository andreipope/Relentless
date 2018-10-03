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
        //public event Action<PlayerActionUseOverlordSkill> OverlordSkillUsedActionReceived;
        //public event Action<PlayerActionUseCardAbility> CardAbilityUsedActionReceived;
        public event Action<PlayerActionMulligan> MulliganProcessUsedActionReceived;
        public event Action<PlayerActionDrawCard> DrawCardActionReceived;

        private BackendFacade _backendFacade;
        private BackendDataControlMediator _backendDataControlMediator;

        private volatile Queue<Action> _mainThreadActionsToDo;

        public FindMatchResponse MatchResponse { get; set; }
        public GetGameStateResponse GameStateResponse { get; set; }

        public OpponentDeck OpponentDeck { get; set; }
        public int OpponentDeckIndex { get; set; }

        public void Init()
        {
            _backendFacade = GameClient.Get<BackendFacade>();
            _backendDataControlMediator = GameClient.Get<BackendDataControlMediator>();

            _backendFacade.PlayerActionEventListner += OnGetPlayerActionEventListener;

            _mainThreadActionsToDo = new Queue<Action>();
        }

        public void Update()
        {
            if (_mainThreadActionsToDo.Count > 0)
            {
                _mainThreadActionsToDo.Dequeue().Invoke();
            }
        }

        public void Dispose()
        {
            _mainThreadActionsToDo.Clear();
        }

        public bool IsCurrentPlayer()
        {
            if (MatchResponse.Match.PlayerStates[GameStateResponse.GameState.CurrentPlayerIndex].Id ==
                _backendDataControlMediator.UserDataModel.UserId)
                return true;

            return false;
        }

        private void OnGetPlayerActionEventListener(byte[] data)
        {
            _mainThreadActionsToDo.Enqueue(() =>
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
                /*case PlayerActionType.UseCardAbility:
                    //  OnCardAbilityUsedAction?.Invoke(playerActionEvent.PlayerAction.UseCardAbility);
                    break;
                case PlayerActionType.UseOverlordSkill:
                    //   OnOverlordSkillUsedAction?.Invoke(playerActionEvent.PlayerAction.UseOverlordSkill);
                    break;*/
                case PlayerActionType.DrawCard:
                    DrawCardActionReceived?.Invoke(playerActionEvent.PlayerAction.DrawCard);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(playerActionEvent.PlayerActionType), playerActionEvent.PlayerActionType.ToString() + " not found");
            }
        }
    }
}
