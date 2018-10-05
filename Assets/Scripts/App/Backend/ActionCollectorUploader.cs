using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.Protobuf;

namespace Loom.ZombieBattleground.BackendCommunication
{
    public class ActionCollectorUploader : IService
    {
        private IGameplayManager _gameplayManager;

        private IAnalyticsManager _analyticsManager;

        private PlayerEventListener _playerEventListener;

        private PlayerEventListener _opponentEventListener;

        public void Init()
        {
            _gameplayManager = GameClient.Get<IGameplayManager>();
            _analyticsManager = GameClient.Get<IAnalyticsManager>();

            _gameplayManager.GameInitialized += GameplayManagerGameInitialized;
            _gameplayManager.GameEnded += GameplayManagerGameEnded;
        }

        public void Update()
        {
        }

        public void Dispose()
        {
            _playerEventListener?.Dispose();
            _opponentEventListener?.Dispose();
        }

        private void GameplayManagerGameEnded(Enumerators.EndGameType obj)
        {
            _playerEventListener?.OnGameEndedEventHandler(obj);
            _playerEventListener?.Dispose();
            _opponentEventListener?.Dispose();

            _analyticsManager.NotifyFinishedMatch(obj);
        }

        private void GameplayManagerGameInitialized()
        {
            _playerEventListener?.Dispose();
            _opponentEventListener?.Dispose();

            _playerEventListener = new PlayerEventListener(_gameplayManager.CurrentPlayer, false);
            _opponentEventListener = new PlayerEventListener(_gameplayManager.OpponentPlayer, true);

            _playerEventListener.OnGameInitializedEventHandler();

            _analyticsManager.NotifyStartedMatch();
        }

        private class PlayerEventListener : IDisposable
        {
            private readonly BackendFacade _backendFacade;

            private readonly BackendDataControlMediator _backendDataControlMediator;

            private readonly BattlegroundController _battlegroundController;

            private readonly IPvPManager _pvpManager;
            
            private readonly SkillsController _skillsController;

            private readonly AbilitiesController _abilitiesController;

            public PlayerEventListener(Player player, bool isOpponent)
            {
                _backendFacade = GameClient.Get<BackendFacade>();
                _backendDataControlMediator = GameClient.Get<BackendDataControlMediator>();
                _battlegroundController = GameClient.Get<IGameplayManager>().GetController<BattlegroundController>();
                IDataManager dataManager = GameClient.Get<IDataManager>();
                _pvpManager = GameClient.Get<IPvPManager>();
                _abilitiesController = GameClient.Get<IGameplayManager>().GetController<AbilitiesController>();
                _skillsController = GameClient.Get<IGameplayManager>().GetController<SkillsController>();

                Player = player;
                IsOpponent = isOpponent;

                if (!_backendFacade.IsConnected)
                    return;

                IMatchManager matchManager = GameClient.Get<IMatchManager>();
                if (matchManager.MatchType == Enumerators.MatchType.LOCAL ||
                    matchManager.MatchType == Enumerators.MatchType.PVE ||
                    _pvpManager.MatchResponse == null)
                    return;

                if(!isOpponent)
                    _battlegroundController.TurnEnded += TurnEndedHandler;
                //_abilitiesController.AbilityUsed += AbilityUsedHandler;

                Player.CardPlayed += CardPlayedHandler;
                Player.CardAttacked += CardAttackedHandler;
                Player.LeaveMatch += LeaveMatchHandler;

                _skillsController.PlayerPrimarySkill.SkillUsed += SkillUsedHandler;
                _skillsController.PlayerSecondarySkill.SkillUsed += SkillUsedHandler;

            }

            public Player Player { get; }

            public bool IsOpponent { get; }

            public void Dispose()
            {
                UnsubscribeFromPlayerEvents();
            }

            public async void OnGameEndedEventHandler(Enumerators.EndGameType obj)
            {
                await UploadActionLogModel(CreateBasicActionLogModel("GameEnded").Add("EndGameType", obj.ToString()));
            }

            public async void OnGameInitializedEventHandler()
            {
                await UploadActionLogModel(CreateBasicActionLogModel("GameStarted"));
            }

            private void UnsubscribeFromPlayerEvents()
            {
                _battlegroundController.TurnEnded -= TurnEndedHandler;

                Player.CardPlayed -= CardPlayedHandler;
                Player.CardAttacked -= CardAttackedHandler;
            }

            private void CardPlayedHandler(WorkingCard card)
            {
                string playerId = _backendDataControlMediator.UserDataModel.UserId;
                PlayerAction playerAction = new PlayerAction
                {
                    ActionType = PlayerActionType.CardPlay,
                    PlayerId = playerId,
                    CardPlay = new PlayerActionCardPlay
                    {
                        Card = new CardInstance
                        {
                            InstanceId = card.Id,
                            Prototype = ToProtobufExtensions.GetCardPrototype(card),
                            Defence = card.Health,
                            Attack = card.Damage
                        }
                    }
                };

                _backendFacade.AddAction(_pvpManager.MatchResponse.Match.Id, playerAction);
            }

            private void TurnEndedHandler()
            {
                string playerId = _backendDataControlMediator.UserDataModel.UserId;
                PlayerAction playerAction = new PlayerAction
                {
                    ActionType = PlayerActionType.EndTurn,
                    PlayerId = playerId,
                    EndTurn = new PlayerActionEndTurn()
                };

                _backendFacade.AddAction(_pvpManager.MatchResponse.Match.Id, playerAction);
            }

            private void LeaveMatchHandler()
            {
                string playerId = _backendDataControlMediator.UserDataModel.UserId;
                PlayerAction playerAction = new PlayerAction
                {
                    ActionType = PlayerActionType.LeaveMatch,
                    PlayerId = playerId,
                    LeaveMatch = new PlayerActionLeaveMatch()
                };

                _backendFacade.AddAction(_pvpManager.MatchResponse.Match.Id, playerAction);
            }

            private ActionLogModel CreateBasicActionLogModel(string eventName)
            {
                return
                    new ActionLogModel()
                        .Add("UserId", _backendDataControlMediator.UserDataModel.UserId)
                        .Add("CurrentTurnPlayer", IsOpponent ? "Opponent" : "Player")
                        .Add("Event", eventName);
            }


            private void CardAttackedHandler(WorkingCard attacker, AffectObjectType type, int instanceId)
            {
                string playerId = _backendDataControlMediator.UserDataModel.UserId;
                PlayerAction playerAction = new PlayerAction
                {
                    ActionType = PlayerActionType.CardAttack,
                    PlayerId = playerId,
                    CardAttack = new PlayerActionCardAttack
                    {
                        Attacker = new CardInstance
                        {
                            InstanceId = attacker.Id,
                            Prototype = ToProtobufExtensions.GetCardPrototype(attacker),
                            Defence = attacker.Health,
                            Attack = attacker.Damage
                        },
                        AffectObjectType = type,
                        Target = new Unit
                        {
                            InstanceId = instanceId
                        }
                    }
                };

                _backendFacade.AddAction(_pvpManager.MatchResponse.Match.Id, playerAction);
            }

            private void AbilityUsedHandler(WorkingCard card, CardKind cardKind,
                                                  AffectObjectType affectObjectType, BoardObject target = null)
            {
                int instanceId = -1;

                if (target != null)
                {
                    if (target is Player player)
                    {
                        instanceId = player.Id;
                    }
                    else if (target is BoardUnitModel unit)
                    {
                        instanceId = unit.Card.Id;
                    }
                }

                string playerId = _backendDataControlMediator.UserDataModel.UserId;
                PlayerAction playerAction = new PlayerAction
                {
                    ActionType = PlayerActionType.CardAbilityUsed,
                    PlayerId = playerId,
                    CardAbilityUsed = new PlayerActionCardAbilityUsed()
                    {
                        AffectObjectType = affectObjectType,
                        Target = new Unit()
                        {
                            InstanceId = instanceId
                        },
                        CardKind = cardKind,
                        Card = new CardInstance
                        {
                            InstanceId = card.Id,
                            Prototype = ToProtobufExtensions.GetCardPrototype(card),
                            Defence = card.Health,
                            Attack = card.Damage
                        }
                    }
                };

                _backendFacade.AddAction(_pvpManager.MatchResponse.Match.Id, playerAction);
            }

            private void MulliganHandler(List<WorkingCard> cards)
            {
                string playerId = _backendDataControlMediator.UserDataModel.UserId;
                PlayerAction playerAction = new PlayerAction
                {
                    ActionType = PlayerActionType.Mulligan,
                    PlayerId = playerId,
                    Mulligan = new PlayerActionMulligan
                    {
                        // TODO : cant able to set the mulligan cards, no setter in zb protobuf
                        //MulliganedCards = GetMulliganCards(cards)
                    }
                };

                _backendFacade.AddAction(_pvpManager.MatchResponse.Match.Id, playerAction);
            }


            private void SkillUsedHandler(BoardSkill skill, BoardObject target)
            {
                string playerId = _backendDataControlMediator.UserDataModel.UserId;
                AffectObjectType affectObjectType = target is Player ? AffectObjectType.Player : AffectObjectType.Character;
                Unit targetUnit = null;

                if(target is BoardUnitModel unit)
                {
                    targetUnit = new Unit() { InstanceId = unit.Card.Id };
                }
                else if(target is Player player)
                {
                    targetUnit = new Unit() { InstanceId = player.Id == 0 ? 1 : 0 };
                }

                PlayerAction playerAction = new PlayerAction
                {
                    ActionType = PlayerActionType.OverlordSkillUsed,
                    PlayerId = playerId,
                    OverlordSkillUsed = new PlayerActionOverlordSkillUsed
                    {
                        SkillId = skill.Id,
                        AffectObjectType = affectObjectType,     
                        Target = targetUnit
                    }
                };

                _backendFacade.AddAction(_pvpManager.MatchResponse.Match.Id, playerAction);
            }

            private async Task UploadActionLogModel(ActionLogModel model)
            {
                if (!_backendFacade.IsConnected)
                    return;

                await _backendFacade.UploadActionLog(_backendDataControlMediator.UserDataModel.UserId, model);
            }

            private object WorkingCardToSimpleRepresentation(WorkingCard card)
            {
                return new
                {
                    instanceId = card.Id,
                    cardId = card.CardId,
                    name = card.LibraryCard.Name,
                    health = card.Health,
                    damage = card.Damage,
                    type = card.Type.ToString()
                };
            }
        }
    }
}
