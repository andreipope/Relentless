using System;
using System.Collections.Generic;
using System.Linq;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.Protobuf;
using UnityEngine;
using InstanceId = Loom.ZombieBattleground.Data.InstanceId;

namespace Loom.ZombieBattleground
{
    public class OpponentController : IController
    {
        private IGameplayManager _gameplayManager;
        private IPvPManager _pvpManager;
        private BackendFacade _backendFacade;
        private BackendDataControlMediator _backendDataControlMediator;
        private IMatchManager _matchManager;

        private CardsController _cardsController;
        private BattlegroundController _battlegroundController;
        private BoardController _boardController;
        private SkillsController _skillsController;
        private BattleController _battleController;
        private BoardArrowController _boardArrowController;
        private AbilitiesController _abilitiesController;
        private ActionsQueueController _actionsQueueController;
        private RanksController _ranksController;

        public void Dispose()
        {
        }

        public void Init()
        {
            _gameplayManager = GameClient.Get<IGameplayManager>();
            _backendFacade = GameClient.Get<BackendFacade>();
            _backendDataControlMediator = GameClient.Get<BackendDataControlMediator>();
            _pvpManager = GameClient.Get<IPvPManager>();
            _matchManager = GameClient.Get<IMatchManager>();

            _cardsController = _gameplayManager.GetController<CardsController>();
            _skillsController = _gameplayManager.GetController<SkillsController>();
            _battlegroundController = _gameplayManager.GetController<BattlegroundController>();
            _battleController = _gameplayManager.GetController<BattleController>();
            _boardArrowController = _gameplayManager.GetController<BoardArrowController>();
            _abilitiesController = _gameplayManager.GetController<AbilitiesController>();
            _actionsQueueController = _gameplayManager.GetController<ActionsQueueController>();
            _ranksController = _gameplayManager.GetController<RanksController>();
            _boardController = _gameplayManager.GetController<BoardController>();

            _gameplayManager.GameStarted += GameStartedHandler;
            _gameplayManager.GameEnded += GameEndedHandler;

        }

        public void ResetAll()
        {
        }

        public void Update()
        {
        }

        public void InitializePlayer(InstanceId instanceId)
        {
            Player player = new Player(instanceId, GameObject.Find("Opponent"), true);
            _gameplayManager.OpponentPlayer = player;

            if (!_gameplayManager.IsSpecificGameplayBattleground)
            {
                List<WorkingCard> deck = new List<WorkingCard>();

                bool isMainTurnSecond;
                switch (_matchManager.MatchType)
                {
                    case Enumerators.MatchType.PVP:
                        foreach (CardInstance cardInstance in player.PvPPlayerState.CardsInDeck)
                        {
                            deck.Add(cardInstance.FromProtobuf(player));
                        }

                        Debug.Log(
                            $"Player ID {instanceId}, local: {player.IsLocalPlayer}, added CardsInDeck:\n" +
                            String.Join(
                                "\n",
                                (IList<WorkingCard>) deck
                                    .OrderBy(card => card.InstanceId)
                                    .ToArray()
                                )
                        );

                        isMainTurnSecond = GameClient.Get<IPvPManager>().IsCurrentPlayer();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                player.SetDeck(deck, isMainTurnSecond);

                _battlegroundController.UpdatePositionOfCardsInOpponentHand();
            }
        }

        private void GameStartedHandler()
        {
            _pvpManager.CardPlayedActionReceived += OnCardPlayedHandler;
            _pvpManager.CardAttackedActionReceived += OnCardAttackedHandler;
            _pvpManager.CardAbilityUsedActionReceived += OnCardAbilityUsedHandler;
            _pvpManager.OverlordSkillUsedActionReceived += OnOverlordSkillUsedHandler;
            _pvpManager.LeaveMatchReceived += OnLeaveMatchHandler;
            _pvpManager.RankBuffActionReceived += OnRankBuffHandler;
            _pvpManager.CheatDestroyCardsOnBoardActionReceived += OnCheatDestroyCardsOnBoardActionHandler;
            _pvpManager.PlayerLeftGameActionReceived += OnPlayerLeftGameActionHandler;
            _pvpManager.PlayerActionOutcomeReceived += OnPlayerActionOutcomeReceived;
        }

        private void OnPlayerLeftGameActionHandler(PlayerActionLeaveMatch leaveMatchAction)
        {
            if (leaveMatchAction.Winner == _backendDataControlMediator.UserDataModel.UserId)
            {
                _gameplayManager.OpponentPlayer.PlayerDie();
            }
            else
            {
                _gameplayManager.CurrentPlayer.PlayerDie();
            }
        }
        private void GameEndedHandler(Enumerators.EndGameType endGameType)
        {
            _pvpManager.CardPlayedActionReceived -= OnCardPlayedHandler;
            _pvpManager.CardAttackedActionReceived -= OnCardAttackedHandler;
            _pvpManager.CardAbilityUsedActionReceived -= OnCardAbilityUsedHandler;
            _pvpManager.OverlordSkillUsedActionReceived -= OnOverlordSkillUsedHandler;
            _pvpManager.LeaveMatchReceived -= OnLeaveMatchHandler;
            _pvpManager.RankBuffActionReceived -= OnRankBuffHandler;
            _pvpManager.CheatDestroyCardsOnBoardActionReceived -= OnCheatDestroyCardsOnBoardActionHandler;
            _pvpManager.PlayerLeftGameActionReceived -= OnPlayerLeftGameActionHandler;
            _pvpManager.PlayerActionOutcomeReceived -= OnPlayerActionOutcomeReceived;
        }

        private void OnPlayerActionOutcomeReceived(PlayerActionOutcome outcome)
        {
            if (!_pvpManager.UseBackendGameLogic)
                return;

            switch (outcome.OutcomeCase)
            {
                case PlayerActionOutcome.OutcomeOneofCase.None:
                    break;
                case PlayerActionOutcome.OutcomeOneofCase.Rage:
                    PlayerActionOutcome.Types.CardAbilityRageOutcome rageOutcome = outcome.Rage;
                    BoardUnitModel boardUnit = _battlegroundController.GetBoardUnitById(rageOutcome.InstanceId.FromProtobuf());

                    boardUnit.BuffedDamage = rageOutcome.NewAttack;
                    boardUnit.CurrentDamage = rageOutcome.NewAttack;

                    boardUnit.BuffedDamage = rageOutcome.NewAttack;
                    boardUnit.CurrentDamage = rageOutcome.NewAttack;
                    break;
                case PlayerActionOutcome.OutcomeOneofCase.PriorityAttack:
                    // TODO
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void OnPlayerLeftGameActionHandler()
        {
            _gameplayManager.OpponentPlayer.PlayerDie();
        }

        #region event handlers

        private void OnCardPlayedHandler(PlayerActionCardPlay cardPlay)
        {
            GotActionPlayCard(cardPlay.Card.FromProtobuf(), cardPlay.Position);
        }

        private void OnLeaveMatchHandler()
        {
            _gameplayManager.OpponentPlayer.PlayerDie();
        }

        private void OnCardAttackedHandler(PlayerActionCardAttack actionCardAttack)
        {
            GotActionCardAttack(new CardAttackModel
            {
                AffectObjectType = (Enumerators.AffectObjectType) actionCardAttack.Target.AffectObjectType,
                CardId = actionCardAttack.Attacker.FromProtobuf(),
                TargetId = actionCardAttack.Target.InstanceId.FromProtobuf()
            });
        }

        private void OnCardAbilityUsedHandler(PlayerActionCardAbilityUsed actionUseCardAbility)
        {
            GotActionUseCardAbility(new UseCardAbilityModel
            {
                Card = actionUseCardAbility.Card.FromProtobuf(),
                Targets = actionUseCardAbility.Targets.Select(t => t.FromProtobuf()).ToList(),
                AbilityType = (Enumerators.AbilityType) actionUseCardAbility.AbilityType
            });
        }

        private void OnOverlordSkillUsedHandler(PlayerActionOverlordSkillUsed actionUseOverlordSkill)
        {
            GotActionUseOverlordSkill(new UseOverlordSkillModel
            {
                SkillId = new SkillId(actionUseOverlordSkill.SkillId),
                TargetId = actionUseOverlordSkill.Target.InstanceId.FromProtobuf(),
                AffectObjectType = (Enumerators.AffectObjectType) actionUseOverlordSkill.Target.AffectObjectType
            });
        }

        private void OnRankBuffHandler(PlayerActionRankBuff actionRankBuff)
        {
            GotActionRankBuff(
                actionRankBuff.Card.FromProtobuf(),
                actionRankBuff.Targets.Select(t => t.FromProtobuf()).ToList()
                );
        }

        private void OnCheatDestroyCardsOnBoardActionHandler(PlayerActionCheatDestroyCardsOnBoard actionCheatDestroyCardsOnBoard)
        {
            GotCheatDestroyCardsOnBoard(actionCheatDestroyCardsOnBoard.DestroyedCards.Select(id => id.FromProtobuf()));
        }

        #endregion


        #region Actions

        private void GotActionEndTurn(EndTurnModel model)
        {
            if (_gameplayManager.IsGameEnded)
                return;

            _battlegroundController.EndTurn();
        }

        private void GotActionPlayCard(InstanceId cardId, int position)
        {
            if (_gameplayManager.IsGameEnded)
                return;

            _cardsController.PlayOpponentCard(_gameplayManager.OpponentPlayer, cardId, null, (workingCard, boardObject) =>
            {
                switch (workingCard.LibraryCard.CardKind)
                {
                    case Enumerators.CardKind.CREATURE:
                        BoardUnitView boardUnitViewElement = new BoardUnitView(new BoardUnitModel(), _battlegroundController.OpponentBoardObject.transform);
                        GameObject boardUnit = boardUnitViewElement.GameObject;
                        boardUnit.tag = SRTags.OpponentOwned;
                        boardUnit.transform.position = Vector3.zero;
                        boardUnitViewElement.Model.OwnerPlayer = workingCard.Owner;
                        boardUnitViewElement.SetObjectInfo(workingCard);
                        boardUnitViewElement.Model.TutorialObjectId = workingCard.TutorialObjectId;

                        boardUnit.transform.position += Vector3.up * 2f; // Start pos before moving cards to the opponents board

                        _battlegroundController.OpponentBoardCards.Insert(Mathf.Clamp(position, 0, _battlegroundController.OpponentBoardCards.Count), boardUnitViewElement);
                        _gameplayManager.OpponentPlayer.BoardCards.Insert(Mathf.Clamp(position, 0, _gameplayManager.OpponentPlayer.BoardCards.Count), boardUnitViewElement);

                        boardUnitViewElement.PlayArrivalAnimation(playUniqueAnimation: true);

                        _boardController.UpdateCurrentBoardOfPlayer(_gameplayManager.OpponentPlayer, null);

                        _actionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
                        {
                            ActionType = Enumerators.ActionType.PlayCardFromHand,
                            Caller = boardUnitViewElement.Model,
                            TargetEffects = new List<PastActionsPopup.TargetEffectParam>()
                        });

                        _abilitiesController.ResolveAllAbilitiesOnUnit(boardUnitViewElement.Model);
                        break;
                    case Enumerators.CardKind.SPELL:
                        BoardSpell spell = new BoardSpell(null, workingCard); // todo improve it with game Object aht will be aniamted
                        _gameplayManager.OpponentPlayer.BoardSpellsInUse.Add(spell);
                        spell.OwnerPlayer = _gameplayManager.OpponentPlayer;
                        _actionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
                        {
                            ActionType = Enumerators.ActionType.PlayCardFromHand,
                            Caller = spell,
                            TargetEffects = new List<PastActionsPopup.TargetEffectParam>()
                        });
                        break;
                }

                _gameplayManager.OpponentPlayer.CurrentGoo -= workingCard.InstanceCard.Cost;
            });
        }

        private void GotActionCardAttack(CardAttackModel model)
        {
            if (_gameplayManager.IsGameEnded)
                return;

            BoardUnitModel attackerUnit = _battlegroundController.GetBoardUnitById(model.CardId);
            BoardObject target = _battlegroundController.GetTargetById(model.TargetId, model.AffectObjectType);

            if(attackerUnit == null || target == null)
            {
                Helpers.ExceptionReporter.LogException("GotActionCardAttack Has Error: attackerUnit: " + attackerUnit + "; target: " + target);
                return;
            }

            Action callback = () =>
            {
                attackerUnit.DoCombat(target);
            };

            BoardUnitView attackerUnitView = _battlegroundController.GetBoardUnitViewByModel(attackerUnit);

            if (attackerUnitView != null)
            {
                _boardArrowController.DoAutoTargetingArrowFromTo<OpponentBoardArrow>(attackerUnitView.Transform, target, action: callback);
            }
            else
            {
                Debug.LogWarning("Attacker with card Id " + model.CardId + " not found on this client in match.");
            }
        }

        private void GotActionUseCardAbility(UseCardAbilityModel model)
        {
            if (_gameplayManager.IsGameEnded)
                return;

            BoardObject boardObjectCaller = _battlegroundController.GetBoardObjectById(model.Card);

            if (boardObjectCaller == null)
            {
                // FIXME: why do we have recursion here??
                GameClient.Get<IQueueManager>().AddTask(async () =>
                {
                    await new WaitForUpdate();
                    GotActionUseCardAbility(model);
                });

                return;
            }

            List<ParametrizedAbilityBoardObject> parametrizedAbilityObjects = new List<ParametrizedAbilityBoardObject>();

            foreach(Unit unit in model.Targets)
            {
                parametrizedAbilityObjects.Add(new ParametrizedAbilityBoardObject(
                    _battlegroundController.GetTargetById(unit.InstanceId, unit.AffectObjectType),
                    new ParametrizedAbilityParameters
                    {
                        Attack = unit.Parameter.Attack,
                        Defense = unit.Parameter.Defense,
                        CardName = unit.Parameter.CardName,
                    }
                ));
            }

            WorkingCard workingCard;
            switch (boardObjectCaller)
            {
                case BoardSpell boardSpell:
                    workingCard = boardSpell.Card;
                    break;
                case BoardUnitModel boardUnitModel:
                    workingCard = boardUnitModel.Card;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(boardObjectCaller));
            }

            _abilitiesController.PlayAbilityFromEvent(model.AbilityType,
                                                      boardObjectCaller,
                                                      parametrizedAbilityObjects,
                                                      workingCard,
                                                      _gameplayManager.OpponentPlayer);
        }

        private void GotActionUseOverlordSkill(UseOverlordSkillModel model)
        {
            if (_gameplayManager.IsGameEnded)
                return;

            BoardSkill skill = _battlegroundController.GetSkillById(_gameplayManager.OpponentPlayer, model.SkillId);
            BoardObject target = _battlegroundController.GetTargetById(model.TargetId, model.AffectObjectType);

            if (target == null)
            {
                Helpers.ExceptionReporter.LogException("GotActionUseOverlordSkill Has Error: target: " + target);
                return;
            }

            Action callback = () =>
            {
                switch (model.AffectObjectType)
                {
                    case Enumerators.AffectObjectType.Player:
                        skill.FightTargetingArrow.SelectedPlayer = (Player)target;
                        break;
                    case Enumerators.AffectObjectType.Character:
                        skill.FightTargetingArrow.SelectedCard = _battlegroundController.GetBoardUnitViewByModel((BoardUnitModel)target);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(model.AffectObjectType), model.AffectObjectType, null);
                }

                skill.EndDoSkill();
            };

            skill.StartDoSkill();

            skill.FightTargetingArrow = _boardArrowController.DoAutoTargetingArrowFromTo<OpponentBoardArrow>(skill.SelfObject.transform, target, action: callback);
        }

        private void GotActionMulligan(MulliganModel model)
        {
            if (_gameplayManager.IsGameEnded)
                return;

            // todo implement logic..
        }

        private void GotActionRankBuff(InstanceId card, IList<Unit> targets)
        {
            if (_gameplayManager.IsGameEnded)
                return;

            List<BoardUnitView> units = _battlegroundController.GetTargetsById(targets)
                .Cast<BoardUnitModel>()
                .Select(x => _battlegroundController.GetBoardUnitViewByModel(x)).ToList();

            WorkingCard workingCard = _battlegroundController.GetWorkingCardById(card);
            if (workingCard == null)
                throw new Exception($"Board unit with instance ID {card} not found");

            _ranksController.BuffAllyManually(units, workingCard);
        }

        private void GotCheatDestroyCardsOnBoard(IEnumerable<InstanceId> cards)
        {
            foreach (InstanceId cardId in cards)
            {
                BoardUnitModel card = (BoardUnitModel) _battlegroundController.GetTargetById(cardId, Enumerators.AffectObjectType.Character);
                if (card == null)
                {
                    Debug.LogError($"Card {cardId} not found on board");
                }
                else
                {
                    card.Die(withDeathEffect: false);
                }
            }
        }

        #endregion
    }

    #region models
    public class EndTurnModel
    {
        public InstanceId CallerId;
    }

    public class MulliganModel
    {
        public InstanceId CallerId;
        public List<InstanceId> CardsIds;
    }

    public class DrawCardModel
    {
        public string CardName;
        public InstanceId CallerId;
        public InstanceId FromDeckOfPlayerId;
        public InstanceId TargetId;
        public Enumerators.AffectObjectType AffectObjectType;
    }


    public class UseOverlordSkillModel
    {
        public SkillId SkillId;
        public InstanceId TargetId;
        public Enumerators.AffectObjectType AffectObjectType;
    }

    public class UseCardAbilityModel
    {
        public InstanceId Card;
        public Enumerators.AbilityType AbilityType;
        public List<Unit> Targets;
    }

    public class CardAttackModel
    {
        public InstanceId CardId;
        public InstanceId TargetId;
        public Enumerators.AffectObjectType AffectObjectType;
    }

    public class TargetUnitModel
    {
        public InstanceId Target;
        public Enumerators.AffectObjectType AffectObjectType;
    }

    #endregion
}
