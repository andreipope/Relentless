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

                _battlegroundController.TurnEnded += TurnEndedHandler;
                _abilitiesController.AbilityUsed += AbilityUsedHandler;

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

            private async void CardPlayedHandler(WorkingCard card)
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

                await _backendFacade.SendAction(_pvpManager.MatchResponse.Match.Id, playerAction);
            }

            private async void BoardChangedHandler(int obj)
            {
                await
                    UploadActionLogModel(CreateBasicActionLogModel("BoardChanged")
                        .Add("CardsOnBoard", Player.CardsOnBoard.Select(WorkingCardToSimpleRepresentation).ToArray()));
            }

            private async void GraveyardChangedHandler(int obj)
            {
                await
                    UploadActionLogModel(CreateBasicActionLogModel("GraveyardChanged")
                        .Add("CardsOnBoard",
                            Player.CardsInGraveyard.Select(WorkingCardToSimpleRepresentation).ToArray()));
            }

            private async void HandChangedHandler(int obj)
            {
                await
                    UploadActionLogModel(CreateBasicActionLogModel("HandChanged")
                        .Add("CardsOnBoard", Player.CardsInHand.Select(WorkingCardToSimpleRepresentation).ToArray()));
            }

            private async void DeckChangedHandler(int obj)
            {
                await
                    UploadActionLogModel(CreateBasicActionLogModel("DeckChanged")
                        .Add("CardsOnBoard", Player.CardsInDeck.Select(WorkingCardToSimpleRepresentation).ToArray()));
            }

            private async void PlayerVialGooChangedHandler(int obj)
            {
                await UploadActionLogModel(CreateBasicActionLogModel("GooOnCurrentTurnChanged").Add("Goo", obj));
            }

            private async void PlayerGooChangedHandler(int obj)
            {
                await UploadActionLogModel(CreateBasicActionLogModel("GooChanged").Add("Goo", obj));
            }

            private async void PlayerHpChangedHandler(int obj)
            {
                await UploadActionLogModel(CreateBasicActionLogModel("HealthChanged").Add("Health", obj));
            }

            private async void TurnStartedHandler()
            {
                await UploadActionLogModel(CreateBasicActionLogModel("TurnStart"));
            }

            private async void TurnEndedHandler()
            {
                string playerId = _backendDataControlMediator.UserDataModel.UserId;
                PlayerAction playerAction = new PlayerAction
                {
                    ActionType = PlayerActionType.EndTurn,
                    PlayerId = playerId,
                    EndTurn = new PlayerActionEndTurn()
                };

                await _backendFacade.SendAction(_pvpManager.MatchResponse.Match.Id, playerAction);
            }

            private async void LeaveMatchHandler()
            {
                string playerId = _backendDataControlMediator.UserDataModel.UserId;
                PlayerAction playerAction = new PlayerAction
                {
                    ActionType = PlayerActionType.LeaveMatch,
                    PlayerId = playerId,
                    LeaveMatch = new PlayerActionLeaveMatch()
                };

                await _backendFacade.SendAction(_pvpManager.MatchResponse.Match.Id, playerAction);
            }

            private ActionLogModel CreateBasicActionLogModel(string eventName)
            {
                return
                    new ActionLogModel()
                        .Add("UserId", _backendDataControlMediator.UserDataModel.UserId)
                        .Add("CurrentTurnPlayer", IsOpponent ? "Opponent" : "Player")
                        .Add("Event", eventName);
            }


            private async void CardAttackedOnPlayerHandler(WorkingCard attacker, Player player)
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
                        }
                    }
                };

                await _backendFacade.SendAction(_pvpManager.MatchResponse.Match.Id, playerAction);
            }

            private async void CardAttackedHandler(WorkingCard attacker, AffectObjectType type, int instanceId)
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

                await _backendFacade.SendAction(_pvpManager.MatchResponse.Match.Id, playerAction);
            }

            private async void AbilityUsedHandler(WorkingCard card, Enumerators.AbilityType abilityType, CardKind cardKind,
                                                  AffectObjectType affectObjectType, List<BoardObject> targets = null)
            {
                await Task.Delay(2000);

                List<Unit> targetUnits = new List<Unit>();

                Unit targetUnit;
                if (targets != null)
                {
                    foreach(BoardObject boardObject in targets)
                    {
                        targetUnit = new Unit();

                        if (boardObject is BoardUnitModel model)
                        {
                            targetUnit = new Unit()
                            {
                                InstanceId = model.Card.Id,
                                AffectObjectType =  AffectObjectType.Character
                            };
                        }
                        else if (boardObject is Player player)
                        {
                            targetUnit = new Unit()
                            {
                                InstanceId = player.Id == 0 ? 1 : 0,
                                AffectObjectType = AffectObjectType.Player
                            };
                        }
                        else if(boardObject is HandBoardCard handCard)
                        {
                            targetUnit = new Unit()
                            {
                                InstanceId = handCard.Id,
                                AffectObjectType = AffectObjectType.Card
                            };
                        }

                        targetUnits.Add(targetUnit);
                    }
                }

                PlayerActionCardAbilityUsed CardAbilityUsed = new PlayerActionCardAbilityUsed()
                {
                    CardKind = cardKind,
                    AbilityType = abilityType.ToString(),
                    Card = new CardInstance
                    {
                        InstanceId = card.Id,
                        Prototype = ToProtobufExtensions.GetCardPrototype(card),
                        Defence = card.Health,
                        Attack = card.Damage
                    }
                };
                CardAbilityUsed.Targets.Add(targetUnits);

                string playerId = _backendDataControlMediator.UserDataModel.UserId;
                PlayerAction playerAction = new PlayerAction
                {
                    ActionType = PlayerActionType.CardAbilityUsed,
                    PlayerId = playerId,
                    CardAbilityUsed = CardAbilityUsed

                };

                await _backendFacade.SendAction(_pvpManager.MatchResponse.Match.Id, playerAction);
            }

            private async void MulliganHandler(List<WorkingCard> cards)
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

                await _backendFacade.SendAction(_pvpManager.MatchResponse.Match.Id, playerAction);
            }


            private async void SkillUsedHandler(BoardSkill skill, BoardObject target)
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

                await _backendFacade.SendAction(_pvpManager.MatchResponse.Match.Id, playerAction);
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
