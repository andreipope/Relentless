using System;
using System.Collections.Generic;
using System.Linq;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.Protobuf;
using UnityEngine;

namespace Loom.ZombieBattleground.BackendCommunication
{
    public class ActionCollectorUploader : IService
    {
        private IGameplayManager _gameplayManager;

        private IMatchManager _matchManager;

        private IAnalyticsManager _analyticsManager;

        private BackendFacade _backendFacade;

        private BackendDataControlMediator _backendDataControlMediator;

        private PlayerEventListener _playerEventListener;

        private PlayerEventListener _opponentEventListener;

        public void Init()
        {
            _gameplayManager = GameClient.Get<IGameplayManager>();
            _matchManager = GameClient.Get<IMatchManager>();
            _analyticsManager = GameClient.Get<IAnalyticsManager>();
            _backendFacade = GameClient.Get<BackendFacade>();
            _backendDataControlMediator = GameClient.Get<BackendDataControlMediator>();

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
            _playerEventListener?.Dispose();
            _opponentEventListener?.Dispose();

            _analyticsManager.NotifyFinishedMatch(obj);

            if (!_gameplayManager.IsTutorial)
            {
                Dictionary<string, object> eventParameters = new Dictionary<string, object>();
                eventParameters.Add(AnalyticsManager.PropertyMatchDuration, _gameplayManager.MatchDuration.GetTimeDiffrence());
                eventParameters.Add(AnalyticsManager.PropertyMatchType, _matchManager.MatchType.ToString());
                if (obj == Enumerators.EndGameType.CANCEL)
                {
                    _analyticsManager.SetEvent(AnalyticsManager.EventQuitMatch, eventParameters);
                }
                else
                {
                    eventParameters.Add(AnalyticsManager.PropertyMatchResult, obj.ToString());
                    _analyticsManager.SetEvent(AnalyticsManager.EventEndedMatch, eventParameters);
                }
            }
        }

        private void GameplayManagerGameInitialized()
        {
            _playerEventListener?.Dispose();
            _opponentEventListener?.Dispose();

            _playerEventListener = new PlayerEventListener(_gameplayManager.CurrentPlayer, false);
            _opponentEventListener = new PlayerEventListener(_gameplayManager.OpponentPlayer, true);

            _analyticsManager.NotifyStartedMatch();

            if (!_gameplayManager.IsTutorial)
            {
                Dictionary<string, object> eventParameters = new Dictionary<string, object>();
                eventParameters.Add(AnalyticsManager.PropertyTimeToFindOpponent, _matchManager.MatchType == Enumerators.MatchType.PVP ? _matchManager.FindOpponentTime.GetTimeDiffrence() : "0");
                eventParameters.Add(AnalyticsManager.PropertyMatchType, _matchManager.MatchType.ToString());
                _analyticsManager.SetEvent(AnalyticsManager.EventStartedMatch, eventParameters);
            }
        }

        private class PlayerEventListener : IDisposable
        {
            private readonly BackendFacade _backendFacade;

            private readonly IQueueManager _queueManager;

            private readonly BackendDataControlMediator _backendDataControlMediator;

            private readonly BattlegroundController _battlegroundController;

            private readonly IPvPManager _pvpManager;

            private readonly SkillsController _skillsController;

            private readonly AbilitiesController _abilitiesController;

            private readonly RanksController _ranksController;

            private readonly MatchRequestFactory _matchRequestFactory;
            private readonly PlayerActionFactory _playerActionFactory;

            public PlayerEventListener(Player player, bool isOpponent)
            {
                _backendFacade = GameClient.Get<BackendFacade>();
                _queueManager = GameClient.Get<IQueueManager>();
                _backendDataControlMediator = GameClient.Get<BackendDataControlMediator>();
                _pvpManager = GameClient.Get<IPvPManager>();
                _battlegroundController = GameClient.Get<IGameplayManager>().GetController<BattlegroundController>();
                _abilitiesController = GameClient.Get<IGameplayManager>().GetController<AbilitiesController>();
                _skillsController = GameClient.Get<IGameplayManager>().GetController<SkillsController>();
                _ranksController = GameClient.Get<IGameplayManager>().GetController<RanksController>();

                Player = player;
                IsOpponent = isOpponent;

                if (!_backendFacade.IsConnected)
                    return;

                IMatchManager matchManager = GameClient.Get<IMatchManager>();
                if (matchManager.MatchType == Enumerators.MatchType.LOCAL ||
                    matchManager.MatchType == Enumerators.MatchType.PVE ||
                    _pvpManager.InitialGameState == null)
                    return;

                _matchRequestFactory = new MatchRequestFactory(_pvpManager.MatchMetadata.Id);
                _playerActionFactory = new PlayerActionFactory(_backendDataControlMediator.UserDataModel.UserId);

                if (!isOpponent)
                {
                    _battlegroundController.TurnEnded += TurnEndedHandler;

                    _abilitiesController.AbilityUsed += AbilityUsedHandler;

                    Player.DrawCard += DrawCardHandler;
                    Player.CardPlayed += CardPlayedHandler;
                    Player.CardAttacked += CardAttackedHandler;
                    Player.LeaveMatch += LeaveMatchHandler;
                    GameClient.Get<IUIManager>().GetPopup<MulliganPopup>().MulliganCards += MulliganHandler;

                    if (_skillsController.PlayerPrimarySkill != null)
                    {
                        _skillsController.PlayerPrimarySkill.SkillUsed += SkillUsedHandler;
                    }

                    if (_skillsController.PlayerSecondarySkill != null)
                    {
                        _skillsController.PlayerSecondarySkill.SkillUsed += SkillUsedHandler;
                    }

                    _ranksController.RanksUpdated += RanksUpdatedHandler;
                }
            }

            private void DrawCardHandler(BoardUnitModel boardUnitModel)
            {
                /*string playerId = _backendDataControlMediator.UserDataModel.UserId;
                PlayerAction playerAction = new PlayerAction
                {
                    ActionType = PlayerActionType.Types.Enum.DrawCard,
                    PlayerId = playerId,
                    DrawCard = new PlayerActionDrawCard
                    {
                        CardInstance = new CardInstance
                        {
                            InstanceId = card.Id,
                            Prototype = ToProtobufExtensions.GetCardPrototype(card),
                            Defense = card.Health,
                            Attack = card.Damage
                        }
                    }
                };

                _backendFacade.AddAction(_pvpManager.MatchMetadata.Id, playerAction);*/
            }

            public Player Player { get; }

            public bool IsOpponent { get; }

            public void Dispose()
            {
                UnsubscribeFromPlayerEvents();
            }

            private void UnsubscribeFromPlayerEvents()
            {
                if (!IsOpponent)
                {
                    _battlegroundController.TurnEnded -= TurnEndedHandler;

                    _abilitiesController.AbilityUsed -= AbilityUsedHandler;

                    Player.DrawCard -= DrawCardHandler;
                    Player.CardPlayed -= CardPlayedHandler;
                    Player.CardAttacked -= CardAttackedHandler;
                    Player.LeaveMatch -= LeaveMatchHandler;

                    if (_skillsController.PlayerPrimarySkill != null)
                    {
                        _skillsController.PlayerPrimarySkill.SkillUsed -= SkillUsedHandler;
                    }

                    if (_skillsController.PlayerSecondarySkill != null)
                    {
                        _skillsController.PlayerSecondarySkill.SkillUsed -= SkillUsedHandler;
                    }

                    _ranksController.RanksUpdated -= RanksUpdatedHandler;
                }
            }

            private void CardPlayedHandler(BoardUnitModel boardUnitModel, int position)
            {
                AddAction(_playerActionFactory.CardPlay(boardUnitModel.InstanceId, position));
            }

            private void TurnEndedHandler()
            {
                PlayerAction playerAction = _playerActionFactory.EndTurn();

                if (Constants.GameStateValidationEnabled)
                {
                    // TODO: remove when we are confident about the lack of de-sync
                    playerAction.ControlGameState = GameStateConstructor.Create().CreateCurrentGameStateFromOnlineGame(true);
                }

                AddAction(playerAction);
            }

            private void LeaveMatchHandler()
            {
                AddAction(_playerActionFactory.LeaveMatch());
            }

            private void CardAttackedHandler(BoardUnitModel attacker, Data.InstanceId instanceId)
            {
                AddAction(_playerActionFactory.CardAttack(attacker.InstanceId, instanceId));
            }

            private void AbilityUsedHandler(
                BoardUnitModel boardUnitModel,
                Enumerators.AbilityType abilityType,
                List<ParametrizedAbilityBoardObject> targets = null)
            {
                AddAction(_playerActionFactory.CardAbilityUsed(boardUnitModel.InstanceId, abilityType, targets));
            }

            private void MulliganHandler(List<WorkingCard> cards)
            {
                AddAction(_playerActionFactory.Mulligan(cards.Select(card => card.InstanceId)));
            }

            private void SkillUsedHandler(BoardSkill skill, List<ParametrizedAbilityBoardObject> targets = null)
            {
                AddAction(_playerActionFactory.OverlordSkillUsed(skill.SkillId, targets));
            }

            private void RanksUpdatedHandler(WorkingCard card, List<BoardUnitView> units)
            {
                AddAction(_playerActionFactory.RankBuff(card.InstanceId, units.Select(unit => unit.Model.Card.InstanceId).ToList()));
            }

            private void AddAction(PlayerAction playerAction)
            {
                _queueManager.AddAction(_matchRequestFactory.CreateAction(playerAction));
            }
        }
    }
}
