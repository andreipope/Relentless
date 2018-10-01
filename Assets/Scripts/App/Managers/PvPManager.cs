using System;
using Loom.Newtonsoft.Json;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.Protobuf;
using UnityEngine;
using SystemText = System.Text;

namespace Loom.ZombieBattleground
{
    public class PvPManager : IService, IPVPManagaer
    {
        // matching actions
        public event Action OnMatchCreated;
        public event Action OnMatchingStarted;
        public event Action OnPlayerLeftGame;

        // game status actions
        public event Action OnGameStarted;
        public event Action OnGameEnded;

        // gameplay actions
        public event Action OnGetEndTurnAction;
        public event Action<PlayerActionCardPlay> OnCardPlayedAction;
        public event Action<PlayerActionCardAttack> OnCardAttackedAction;
        public event Action<PlayerActionUseOverlordSkill> OnOverlordSkillUsedAction;
        public event Action<PlayerActionUseCardAbility> OnCardAbilityUsedAction;
        public event Action<PlayerActionMulligan> OnMulliganProcessUsedAction;
        public event Action<PlayerActionDrawCard> OnDrawCardAction;

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
                    OnMatchCreated?.Invoke();
                    break;
                case Match.Types.Status.Matching:
                    OnMatchingStarted?.Invoke();
                    break;
                case Match.Types.Status.Started:
                    OnGameStarted?.Invoke();
                    break;
                case Match.Types.Status.Playing:
                    if (playerActionEvent.UserId == _backendDataControlMediator.UserDataModel.UserId)
                        return;

                    OnReceivePlayerActionType(playerActionEvent);
                    break;
                case Match.Types.Status.PlayerLeft:
                    OnPlayerLeftGame?.Invoke();
                    break;
                case Match.Types.Status.Ended:
                    OnGameEnded?.Invoke();
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
                    OnGetEndTurnAction?.Invoke();
                    break;
                case PlayerActionType.Mulligan:
                    OnMulliganProcessUsedAction?.Invoke(playerActionEvent.PlayerAction.Mulligan);
                    break;
                case PlayerActionType.CardPlay:
                    OnCardPlayedAction?.Invoke(playerActionEvent.PlayerAction.CardPlay);
                    break;
                case PlayerActionType.CardAttack:
                    OnCardAttackedAction?.Invoke(playerActionEvent.PlayerAction.CardAttack);
                    break;
                case PlayerActionType.UseCardAbility:
                    //  OnCardAbilityUsedAction?.Invoke(playerActionEvent.PlayerAction.UseCardAbility);
                    break;
                case PlayerActionType.UseOverlordSkill:
                    //   OnOverlordSkillUsedAction?.Invoke(playerActionEvent.PlayerAction.UseOverlordSkill);
                    break;
                case PlayerActionType.DrawCard:
                    OnDrawCardAction?.Invoke(playerActionEvent.PlayerAction.DrawCard);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(playerActionEvent.PlayerActionType), playerActionEvent.PlayerActionType.ToString() + " not found");
            }
        }
    }
}
