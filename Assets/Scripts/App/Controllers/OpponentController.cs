using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.Protobuf;
using UnityEngine;

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

            _gameplayManager.GameStarted += GameStartedHandler;
            _gameplayManager.GameEnded += GameEndedHandler;

        }

        public void ResetAll()
        {
        }

        public void Update()
        {
        }

        public void InitializePlayer(int playerId)
        {
            Player player = new Player(playerId, GameObject.Find("Opponent"), true);
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
            _pvpManager.DrawCardActionReceived += OnDrawCardHandler;
            _pvpManager.CardAbilityUsedActionReceived += OnCardAbilityUsedHandler;
            _pvpManager.OverlordSkillUsedActionReceived += OnOverlordSkillUsedHandler;
            _pvpManager.MulliganProcessUsedActionReceived += OnMulliganProcessHandler;
            _pvpManager.LeaveMatchReceived += OnLeaveMatchHandler;
            _pvpManager.RankBuffActionReceived += OnRankBuffHandler;
            _pvpManager.PlayerLeftGameActionReceived += OnPlayerLeftGameActionHandler;
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
            _pvpManager.DrawCardActionReceived -= OnDrawCardHandler;
            _pvpManager.CardAbilityUsedActionReceived -= OnCardAbilityUsedHandler;
            _pvpManager.OverlordSkillUsedActionReceived -= OnOverlordSkillUsedHandler;
            _pvpManager.MulliganProcessUsedActionReceived -= OnMulliganProcessHandler;
            _pvpManager.LeaveMatchReceived -= OnLeaveMatchHandler;
            _pvpManager.RankBuffActionReceived -= OnRankBuffHandler;
            _pvpManager.PlayerLeftGameActionReceived -= OnPlayerLeftGameActionHandler;

        }

        #region event handlers

        private void OnCardPlayedHandler(PlayerActionCardPlay cardPlay)
        {
            GotActionPlayCard(cardPlay.Card.FromProtobuf(_gameplayManager.OpponentPlayer),
                              cardPlay.Position);
        }

        private void OnLeaveMatchHandler()
        {
            _gameplayManager.OpponentPlayer.PlayerDie();
        }

        private void OnCardAttackedHandler(PlayerActionCardAttack actionCardAttack)
        {
            GotActionCardAttack(new CardAttackModel()
            {
                AffectObjectType = Utilites.CastStringTuEnum<Enumerators.AffectObjectType>(actionCardAttack.AffectObjectType.ToString(), true),
                CardId = actionCardAttack.Attacker.InstanceId,
                TargetId = actionCardAttack.Target.InstanceId
            });
        }

        private void OnDrawCardHandler(PlayerActionDrawCard actionDrawCard)
        {
            GotActionDrawCard(actionDrawCard.CardInstance.FromProtobuf(_gameplayManager.OpponentPlayer));
        }

        private void OnCardAbilityUsedHandler(PlayerActionCardAbilityUsed actionUseCardAbility)
        {           
            GotActionUseCardAbility(new UseCardAbilityModel()
            {
                CardKind = Utilites.CastStringTuEnum<Enumerators.CardKind>(actionUseCardAbility.CardKind.ToString()),
                Card = actionUseCardAbility.Card.FromProtobuf(_gameplayManager.OpponentPlayer),
                Targets = actionUseCardAbility.Targets.Select(t => t.FromProtobuf()).ToList(),
                AbilityType = Utilites.CastStringTuEnum<Enumerators.AbilityType>(actionUseCardAbility.AbilityType)
            });
        }

        private void OnOverlordSkillUsedHandler(PlayerActionOverlordSkillUsed actionUseOverlordSkill)
        {
            GotActionUseOverlordSkill(new UseOverlordSkillModel()
            {
                SkillId = (int)actionUseOverlordSkill.SkillId,
                TargetId = actionUseOverlordSkill.Target.InstanceId,
                AffectObjectType = Utilites.CastStringTuEnum<Enumerators.AffectObjectType>(actionUseOverlordSkill.AffectObjectType.ToString(), true)
            });
        }

        private void OnMulliganProcessHandler(PlayerActionMulligan actionMulligan)
        {

        }

        private void OnRankBuffHandler(PlayerActionRankBuff actionRankBuff)
        {
            GotActionRankBuff(
                actionRankBuff.Card.FromProtobuf(_gameplayManager.OpponentPlayer),
                actionRankBuff.Targets.Select(t => t.FromProtobuf()).ToList()
                );
        }

        #endregion


        #region Actions

        public void GotActionEndTurn(EndTurnModel model)
        {
            if (_gameplayManager.IsGameEnded)
                return;

            _battlegroundController.EndTurn();
        }

        public void GotActionDrawCard(WorkingCard drawedCard)
        {
            if (_gameplayManager.IsGameEnded)
                return;

            _cardsController.AddCardToHandFromOtherPlayerDeck(drawedCard.Owner, drawedCard.Owner, drawedCard);
        }

        public void GotActionPlayCard(WorkingCard card, int position)
        {
            if (_gameplayManager.IsGameEnded)
                return;

            _cardsController.PlayOpponentCard(_gameplayManager.OpponentPlayer, card, null, (workingCard, boardObject) =>
            {
                // TODO : try to find issue when this can be
                if (_gameplayManager.IsGameEnded || workingCard == null || boardObject == null)
                {
                    Debug.LogError("got exeption: IsGameEnded " + _gameplayManager.IsGameEnded + " | workingCard " + workingCard + " | boardObject " + boardObject);
                    return;
                }

                switch (workingCard.LibraryCard.CardKind)
                {
                    case Enumerators.CardKind.CREATURE:
                        BoardUnitView boardUnitViewElement = new BoardUnitView(new BoardUnitModel(), _battlegroundController.OpponentBoardObject.transform);
                        GameObject boardUnit = boardUnitViewElement.GameObject;
                        boardUnit.tag = SRTags.OpponentOwned;
                        boardUnit.transform.position = Vector3.zero;
                        boardUnitViewElement.Model.OwnerPlayer = workingCard.Owner;
                        boardUnitViewElement.SetObjectInfo(workingCard);

                        boardUnit.transform.position += Vector3.up * 2f; // Start pos before moving cards to the opponents board

                        _battlegroundController.OpponentBoardCards.Insert(Mathf.Clamp(position, 0, _battlegroundController.OpponentBoardCards.Count), boardUnitViewElement);
                        _gameplayManager.OpponentPlayer.BoardCards.Insert(Mathf.Clamp(position, 0, _gameplayManager.OpponentPlayer.BoardCards.Count), boardUnitViewElement);

                        boardUnitViewElement.PlayArrivalAnimation(playUniqueAnimation: true);

                        _battlegroundController.UpdatePositionOfBoardUnitsOfOpponent();

                        _actionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
                        {
                            ActionType = Enumerators.ActionType.PlayCardFromHand,
                            Caller = boardUnitViewElement.Model,
                            TargetEffects = new List<PastActionsPopup.TargetEffectParam>()
                        });

                        _abilitiesController.ResolveAllAbilitiesOnUnit(boardUnitViewElement.Model);
                        break;
                    case Enumerators.CardKind.SPELL:
                        BoardSpell spell = new BoardSpell(null, card); // todo improve it with game Object aht will be aniamted
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

                _gameplayManager.OpponentPlayer.CurrentGoo -= card.InstanceCard.Cost;
            });
        }

        public void GotActionCardAttack(CardAttackModel model)
        {
            if (_gameplayManager.IsGameEnded)
                return;

            BoardUnitModel attackerUnit = _battlegroundController.GetBoardUnitById(_gameplayManager.OpponentPlayer, model.CardId);
            BoardObject target = _battlegroundController.GetTargetById(model.TargetId, model.AffectObjectType);

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
                Debug.LogError("Attacker with card Id " + model.CardId + " not found on this client in match.");
            }
        }

        public void GotActionUseCardAbility(UseCardAbilityModel model)
        {
            if (_gameplayManager.IsGameEnded)
                return;

            BoardObject boardObjectCaller = _battlegroundController.GetBoardObjectById(model.Card.InstanceId);

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
                parametrizedAbilityObjects.Add(new ParametrizedAbilityBoardObject()
                {
                    BoardObject = _battlegroundController.GetTargetById(unit.InstanceId,
                             Utilites.CastStringTuEnum<Enumerators.AffectObjectType>(unit.AffectObjectType.ToString(), true)),
                    Parameters = new ParametrizedAbilityBoardObject.AbilityParameters()
                    {
                        Attack = unit.Parameter.Attack,
                        Defense = unit.Parameter.Defense,
                        CardName = unit.Parameter.CardName,
                    }
                });
            }

            _abilitiesController.PlayAbilityFromEvent(model.AbilityType,
                                                      boardObjectCaller,
                                                      parametrizedAbilityObjects,
                                                      model.Card,
                                                      _gameplayManager.OpponentPlayer);
        }

        public void GotActionUseOverlordSkill(UseOverlordSkillModel model)
        {
            if (_gameplayManager.IsGameEnded)
                return;

            BoardSkill skill = _battlegroundController.GetSkillById(_gameplayManager.OpponentPlayer, model.SkillId);
            BoardObject target = _battlegroundController.GetTargetById(model.TargetId, model.AffectObjectType);

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

        public void GotActionMulligan(MulliganModel model)
        {
            if (_gameplayManager.IsGameEnded)
                return;

            // todo implement logic..
        }

        public void GotActionRankBuff(WorkingCard card, IList<Unit> targets)
        {
            if (_gameplayManager.IsGameEnded)
                return;

            List<BoardUnitView> units = _battlegroundController.GetTargetsById(targets)
                .Cast<BoardUnitModel>()
                .Select(x => _battlegroundController.GetBoardUnitViewByModel(x)).ToList();

            _ranksController.BuffAllyManually(units, card);
        }

        #endregion
    }

    #region models
    public class EndTurnModel
    {
        public int CallerId;
    }

    public class MulliganModel
    {
        public int CallerId;
        public List<int> CardsIds;
    }

    public class DrawCardModel
    {
        public string CardName;
        public int CallerId;
        public int FromDeckOfPlayerId;
        public int TargetId;
        public Enumerators.AffectObjectType AffectObjectType;
    }


    public class UseOverlordSkillModel
    {
        public int SkillId;
        public int TargetId;
        public Enumerators.AffectObjectType AffectObjectType;
    }

    public class UseCardAbilityModel
    {
        public WorkingCard Card;
        public Enumerators.CardKind CardKind;
        public Enumerators.AbilityType AbilityType;
        public List<Unit> Targets;
    }

    public class CardAttackModel
    {
        public int CardId;
        public int TargetId;
        public Enumerators.AffectObjectType AffectObjectType;
    }

    public class TargetUnitModel
    {
        public int Target;
        public Enumerators.AffectObjectType AffectObjectType;
    }

    #endregion
}
