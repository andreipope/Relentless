using System;
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
        public event Action MatchCreatedActionRecieved;
        public event Action MatchingStartedActionRecieved;
        public event Action PlayerLeftGameActionRecived;

        // game status actions
        public event Action GameStartedActionRecieved;
        public event Action GameEndedActioRecieved;

        // gameplay actions
        public event Action EndTurnActionRecieved;
        public event Action<PlayerActionCardPlay> CardPlayedActionRecieved;
        public event Action<PlayerActionCardAttack> CardAttackedActionRecieved;
        public event Action<PlayerActionUseOverlordSkill> OverlordSkillUsedActionRecieved;
        public event Action<PlayerActionUseCardAbility> CardAbilityUsedActionRecieved;
        public event Action<PlayerActionMulligan> MulliganProcessUsedActionRecieved;
        public event Action<PlayerActionDrawCard> DrawCardActionRecieved;

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

            _backendFacade.PlayerActionEvent += OnGetPlayerActionEventListener;
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

        private void OnGetPlayerActionEventListener(byte[] data)
        {
            string jsonStr = SystemText.Encoding.UTF8.GetString(data);

            Debug.LogWarning(jsonStr); // todo delete

            PlayerActionEvent playerActionEvent = JsonConvert.DeserializeObject<PlayerActionEvent>(jsonStr);
            MatchResponse.Match = playerActionEvent.Match.Clone();

            switch (playerActionEvent.Match.Status)
            {
                case Match.Types.Status.Created:
                    MatchCreatedActionRecieved?.Invoke();
                    break;
                case Match.Types.Status.Matching:
                    MatchingStartedActionRecieved?.Invoke();
                    break;
                case Match.Types.Status.Started:
                    GameStartedActionRecieved?.Invoke();
                    break;
                case Match.Types.Status.Playing:
                    if (playerActionEvent.UserId == _backendDataControlMediator.UserDataModel.UserId)
                        return;

                    OnReceivePlayerActionType(playerActionEvent);
                    break;
                case Match.Types.Status.PlayerLeft:
                    PlayerLeftGameActionRecived?.Invoke();
                    break;
                case Match.Types.Status.Ended:
                    GameEndedActioRecieved?.Invoke();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(playerActionEvent.Match.Status), playerActionEvent.Match.Status.ToString() + " not found");
            }
        }


        private void OnReceivePlayerActionType(PlayerActionEvent playerActionEvent)
        {
            switch (playerActionEvent.PlayerActionType)
            {
                case PlayerActionType.NoneAction:
                    break;
                case PlayerActionType.EndTurn:
                    EndTurnActionRecieved?.Invoke();
                    break;
                case PlayerActionType.Mulligan:
                    MulliganProcessUsedActionRecieved?.Invoke(playerActionEvent.PlayerAction.Mulligan);
                    break;
                case PlayerActionType.CardPlay:
                    CardPlayedActionRecieved?.Invoke(playerActionEvent.PlayerAction.CardPlay);
                    break;
                case PlayerActionType.CardAttack:
                    CardAttackedActionRecieved?.Invoke(playerActionEvent.PlayerAction.CardAttack);
                    break;
                case PlayerActionType.UseCardAbility:
                    //  OnCardAbilityUsedAction?.Invoke(playerActionEvent.PlayerAction.UseCardAbility);
                    break;
                case PlayerActionType.UseOverlordSkill:
                    //   OnOverlordSkillUsedAction?.Invoke(playerActionEvent.PlayerAction.UseOverlordSkill);
                    break;
                case PlayerActionType.DrawCard:
                    DrawCardActionRecieved?.Invoke(playerActionEvent.PlayerAction.DrawCard);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(playerActionEvent.PlayerActionType), playerActionEvent.PlayerActionType.ToString() + " not found");
            }
        }
    }
}
