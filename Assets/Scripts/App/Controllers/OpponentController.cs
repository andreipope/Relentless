using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using log4net;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.Helpers;
using Loom.ZombieBattleground.Protobuf;
using UnityEngine;
using AbilityData = Loom.ZombieBattleground.Data.AbilityData;
using Card = Loom.ZombieBattleground.Data.Card;
using InstanceId = Loom.ZombieBattleground.Data.InstanceId;
using Object = UnityEngine.Object;

namespace Loom.ZombieBattleground
{
    public class OpponentController : IController
    {
        private static readonly ILog Log = Logging.GetLog(nameof(OpponentController));

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
        private ActionsReportController _actionsReportController;
        private RanksController _ranksController;

        public IReadOnlyList<CardModel> BoardItemsInUse => _boardItemsInUse;

        private UniqueList<CardModel> _boardItemsInUse;

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
            _actionsReportController = _gameplayManager.GetController<ActionsReportController>();
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
            _boardItemsInUse = new UniqueList<CardModel>();

            if (!_gameplayManager.IsSpecificGameplayBattleground ||
                (_gameplayManager.IsTutorial &&
                GameClient.Get<ITutorialManager>().CurrentTutorial.TutorialContent.ToGameplayContent().
                SpecificBattlegroundInfo.DisabledInitialization))
            {
                List<WorkingCard> deck = new List<WorkingCard>();

                bool isMainTurnSecond;
                switch (_matchManager.MatchType)
                {
                    case Enumerators.MatchType.PVP:
                        foreach (CardInstance cardInstance in player.InitialPvPPlayerState.CardsInDeck)
                        {
                            deck.Add(cardInstance.FromProtobuf(player));
                        }

                        Log.Info(
                            $"Player ID {instanceId}, local: {player.IsLocalPlayer}, added CardsInDeck:\n" +
                            String.Join(
                                "\n",
                                (IList<WorkingCard>)deck
                                    .OrderBy(card => card.InstanceId.Id)
                                    .ToArray()
                                )
                        );

                        isMainTurnSecond = GameClient.Get<IPvPManager>().IsFirstPlayer();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                CardModel[] cardModels = deck.Select(card => new CardModel(card)).ToArray();
                player.PlayerCardsController.SetCardsInDeck(cardModels);

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
                    CardModel card = _battlegroundController.GetCardModelByInstanceId(rageOutcome.InstanceId.FromProtobuf());

                    card.BuffedDamage = rageOutcome.NewDamage;
                    card.CurrentDamage = rageOutcome.NewDamage;
                    break; 

                case PlayerActionOutcome.OutcomeOneofCase.PriorityAttack:
                    // TODO
                    break;


                case PlayerActionOutcome.OutcomeOneofCase.AttackOverlord:
                    PlayerActionOutcome.Types.CardAbilityAttackOverlordOutcome attackOverlordOutcome = outcome.AttackOverlord;

                    AttackOverlordOutcome attackPlayerOutcome = new AttackOverlordOutcome
                    {
                        PlayerInstanceId = attackOverlordOutcome.InstanceId.FromProtobuf(),
                        Damage = attackOverlordOutcome.Damage,
                        NewDefence = attackOverlordOutcome.NewDefense
                    };

                    AttackOverlordAbility attackOverlordAbility = new AttackOverlordAbility();
                    attackOverlordAbility.ActivateAbility(attackPlayerOutcome);
                    break;


                case PlayerActionOutcome.OutcomeOneofCase.Reanimate:
                    PlayerActionOutcome.Types.CardAbilityReanimateOutcome reanimateAbilityOutcome = outcome.Reanimate;
                    ReAnimateAbility(reanimateAbilityOutcome);
                    break;

                case PlayerActionOutcome.OutcomeOneofCase.ChangeStat:
                    PlayerActionOutcome.Types.CardAbilityChangeStatOutcome changeStatOutcome = outcome.ChangeStat;

                    card = _battlegroundController.GetCardModelByInstanceId(changeStatOutcome.InstanceId.FromProtobuf());

                    if (changeStatOutcome.Stat == Stat.Types.Enum.Damage)
                    {
                        IBoardObject targetObject =
                            _battlegroundController.GetBoardObjectByInstanceId(changeStatOutcome.TargetInstanceId
                                .FromProtobuf());

                        CardModel unitModel =
                            _battlegroundController.GetCardModelByInstanceId(
                                changeStatOutcome.InstanceId.FromProtobuf());

                        switch (targetObject)
                        {
                            case Player targetPlayer:
                                _battleController.AttackPlayerByUnit(unitModel, targetPlayer);
                                break;
                            case CardModel targetCardModel:
                                _battleController.AttackUnitByUnit(unitModel, targetCardModel);
                                break;
                        }

                        card.BuffedDamage = changeStatOutcome.NewDamage;
                        card.CurrentDamage = changeStatOutcome.NewDamage;
                    }
                    else if (changeStatOutcome.Stat == Stat.Types.Enum.Defense)
                    {
                        card.BuffedDefense = changeStatOutcome.NewDefense;
                        card.CurrentDefense = changeStatOutcome.NewDefense;
                    }

                    break;

                case PlayerActionOutcome.OutcomeOneofCase.ReplaceUnitsWithTypeOnStrongerOnes:
                    PlayerActionOutcome.Types.CardAbilityReplaceUnitsWithTypeOnStrongerOnesOutcome replaceUnitWithTypeStatOutcome = outcome.ReplaceUnitsWithTypeOnStrongerOnes;
                    ReplaceUnitsWithTypeOnStrongerOnes(replaceUnitWithTypeStatOutcome);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void ReplaceUnitsWithTypeOnStrongerOnes(PlayerActionOutcome.Types.CardAbilityReplaceUnitsWithTypeOnStrongerOnesOutcome replaceUnitWithTypeStatOutcome)
        {
            List<BoardUnitView> oldCardList = new List<BoardUnitView>();
            for (int i = 0; i < replaceUnitWithTypeStatOutcome.OldInstanceIds.Count; i++)
            {
                InstanceId id = replaceUnitWithTypeStatOutcome.OldInstanceIds[i].FromProtobuf();
                CardModel unitModel = _battlegroundController.GetCardModelByInstanceId(id);
                BoardUnitView unit = _battlegroundController.GetCardViewByModel<BoardUnitView>(unitModel);
                oldCardList.Add(unit);
            }
            ClearOldUnitsOnBoard(oldCardList);

            for (int i = 0; i < replaceUnitWithTypeStatOutcome.NewCardInstances.Count; i++)
            {
                Player owner = _gameplayManager.CurrentPlayer;
                if (replaceUnitWithTypeStatOutcome.NewCardInstances[i].CardInstance.Owner != _backendDataControlMediator.UserDataModel.UserId)
                    owner = _gameplayManager.OpponentPlayer;

                ItemPosition itemPosition = new ItemPosition(replaceUnitWithTypeStatOutcome.NewCardInstances[i].Position);
                Card prototype = replaceUnitWithTypeStatOutcome.NewCardInstances[i].CardInstance.Prototype.FromProtobuf();
                BoardUnitView unitView = owner.PlayerCardsController.SpawnUnitOnBoard(prototype.Name, itemPosition);
                if (unitView != null)
                {
                    AddUnitToBoardCards(owner, itemPosition, unitView);
                }
            }
        }

        private void ClearOldUnitsOnBoard(List<BoardUnitView> boardUnits)
        {
            foreach (BoardUnitView unit in boardUnits)
            {
                unit.Model.OwnerPlayer.PlayerCardsController.RemoveCardFromBoard(unit.Model);

                unit.Dispose();
            }
        }

        private void AddUnitToBoardCards(Player owner, ItemPosition position, BoardUnitView unit)
        {
            _battlegroundController.RegisterCardView(unit, owner, position);
        }

        private void ReAnimateAbility(PlayerActionOutcome.Types.CardAbilityReanimateOutcome reanimateAbilityOutcome)
        {
            Player owner = _gameplayManager.CurrentPlayer;
            if (reanimateAbilityOutcome.NewCardInstance.Owner != _backendDataControlMediator.UserDataModel.UserId)
                owner = _gameplayManager.OpponentPlayer;

            Card prototype = reanimateAbilityOutcome.NewCardInstance.Prototype.FromProtobuf();

            WorkingCard card = new WorkingCard(prototype, prototype, owner, reanimateAbilityOutcome.NewCardInstance.InstanceId.FromProtobuf());
            CardModel cardModel = new CardModel(card);
            BoardUnitView unit = CreateBoardUnit(cardModel, owner);

            owner.PlayerCardsController.AddCardToBoard(cardModel, ItemPosition.End);

            _battlegroundController.RegisterCardView(unit, owner);

            if (owner.IsLocalPlayer)
            {
                _battlegroundController.RegisterCardView(unit, _gameplayManager.CurrentPlayer);
            }
            else
            {
                _battlegroundController.RegisterCardView(unit, _gameplayManager.OpponentPlayer);
            }

            _boardController.UpdateCurrentBoardOfPlayer(owner, null);

            // TODO : have to see... how to invoke this
            //InvokeActionTriggered(unit);
            AbilityData abilityData = AbilitiesController.GetAbilityDataByType(Enumerators.AbilityType.REANIMATE_UNIT);
            AbilityBase ability = new ReanimateAbility(prototype.Kind, abilityData);
            AbilityViewBase abilityView = new ReanimateAbilityView((ReanimateAbility)ability);
            ability.InvokeActionTriggered(unit);
        }

        private BoardUnitView CreateBoardUnit(CardModel cardModel, Player owner)
        {
            GameObject playerBoard = owner.IsLocalPlayer ?
                _battlegroundController.PlayerBoardObject :
                _battlegroundController.OpponentBoardObject;

            BoardUnitView boardUnitView = new BoardUnitView(cardModel, playerBoard.transform);
            boardUnitView.Transform.tag = owner.IsLocalPlayer ? SRTags.PlayerOwned : SRTags.OpponentOwned;
            boardUnitView.Transform.parent = playerBoard.transform;
            boardUnitView.Transform.position = new Vector2(2f * owner.CardsOnBoard.Count, owner.IsLocalPlayer ? -1.66f : 1.66f);
            boardUnitView.Model.Card.Owner = owner;

            if (owner != _gameplayManager.CurrentTurnPlayer)
            {
                boardUnitView.Model.IsPlayable = true;
            }

            boardUnitView.PlayArrivalAnimation();

            _gameplayManager.CanDoDragActions = true;

            return boardUnitView;
        }

        #region event handlers

        private void OnCardPlayedHandler(PlayerActionCardPlay cardPlay)
        {
            GotActionPlayCard(cardPlay.Card.FromProtobuf(), (ItemPosition) cardPlay.Position);
        }

        private void OnLeaveMatchHandler()
        {
            _gameplayManager.OpponentPlayer.PlayerDie();
        }

        private void OnCardAttackedHandler(PlayerActionCardAttack actionCardAttack)
        {
            GotActionCardAttack(new CardAttackModel
            {
                CardId = actionCardAttack.Attacker.FromProtobuf(),
                TargetId = actionCardAttack.Target.InstanceId.FromProtobuf()
            });
        }

        private async void OnCardAbilityUsedHandler(PlayerActionCardAbilityUsed actionUseCardAbility)
        {
            await GotActionUseCardAbility(new UseCardAbilityModel
            {
                Card = actionUseCardAbility.Card.FromProtobuf(),
                Targets = actionUseCardAbility.Targets.Select(t => t.FromProtobuf()).ToList(),
                AbilityType = (Enumerators.AbilityType)actionUseCardAbility.AbilityType,
            });
        }

        private void OnOverlordSkillUsedHandler(PlayerActionOverlordSkillUsed actionUseOverlordSkill)
        {
            GotActionUseOverlordSkill(new UseOverlordSkillModel
            {
                SkillId = new SkillId(actionUseOverlordSkill.SkillId),
                Targets = actionUseOverlordSkill.Targets.Select(t => t.FromProtobuf()).ToList(),
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

        private void GotActionPlayCard(InstanceId cardId, ItemPosition position)
        {
            if (_gameplayManager.IsGameEnded)
                return;

            _gameplayManager.GetController<ActionsQueueController>().AddNewActionInToQueue(completeCallback =>
            {
                BoardUnitView boardUnitViewElement = null;
                _cardsController.PlayOpponentCard(_gameplayManager.OpponentPlayer,
                    cardId,
                    null,
                    cardModel =>
                    {
                        switch (cardModel.Prototype.Kind)
                        {
                            case Enumerators.CardKind.CREATURE:
                                boardUnitViewElement = new BoardUnitView(cardModel, _battlegroundController.OpponentBoardObject.transform);
                                GameObject boardUnit = boardUnitViewElement.GameObject;

                                boardUnit.tag = SRTags.OpponentOwned;
                                boardUnit.transform.position = Vector3.up * 2f; // Start pos before moving cards to the opponents board
                                boardUnit.SetActive(false);

                                _gameplayManager.OpponentPlayer.PlayerCardsController.AddCardToBoard(cardModel, position);
                                _battlegroundController.RegisterCardView(
                                    boardUnitViewElement,
                                    _gameplayManager.OpponentPlayer,
                                    InternalTools.GetSafePositionToInsert(position, _gameplayManager.OpponentPlayer.CardsOnBoard)
                                    );

                                _actionsReportController.PostGameActionReport(new PastActionsPopup.PastActionParam
                                {
                                    ActionType = Enumerators.ActionType.PlayCardFromHand,
                                    Caller = boardUnitViewElement.Model,
                                    TargetEffects = new List<PastActionsPopup.TargetEffectParam>()
                                });

                                _abilitiesController.ResolveAllAbilitiesOnUnit(boardUnitViewElement.Model);

                                break;
                            case Enumerators.CardKind.ITEM:
                                GameObject itemGo = Object.Instantiate(_cardsController.ItemCardViewPrefab);
                                ItemBoardCardView item = new ItemBoardCardView(itemGo, cardModel);
                                AddBoardItemInUse(cardModel);

                                item.Model.Owner = _gameplayManager.OpponentPlayer;
                                item.Model.Owner.PlayerCardsController.AddCardToGraveyard(item.Model);
                                _actionsReportController.PostGameActionReport(new PastActionsPopup.PastActionParam
                                {
                                    ActionType = Enumerators.ActionType.PlayCardFromHand,
                                    Caller = cardModel,
                                    TargetEffects = new List<PastActionsPopup.TargetEffectParam>()
                                });

                                break;
                        }

                        _gameplayManager.OpponentPlayer.CurrentGoo -= cardModel.InstanceCard.Cost;
                    },
                    (workingCard, boardObject) =>
                    {
                        switch (workingCard.Prototype.Kind)
                        {
                            case Enumerators.CardKind.CREATURE:
                                boardUnitViewElement.GameObject.SetActive(true);
                                _boardController.UpdateCurrentBoardOfPlayer(_gameplayManager.OpponentPlayer, null);
                                boardUnitViewElement.PlayArrivalAnimation(playUniqueAnimation: true);
                                break;
                        }

                        completeCallback?.Invoke();
                    }
                );
            }, Enumerators.QueueActionType.CardPlay);
        }

        private void GotActionCardAttack(CardAttackModel model)
        {
            if (_gameplayManager.IsGameEnded)
                return;

            _gameplayManager.GetController<ActionsQueueController>().AddNewActionInToQueue(completeCallback =>
            {
                CardModel attackerUnit = _battlegroundController.GetCardModelByInstanceId(model.CardId);
                IBoardObject target = _battlegroundController.GetTargetByInstanceId(model.TargetId, false);

                if (attackerUnit == null || target == null)
                {
                    ExceptionReporter.LogExceptionAsWarning(Log, new Exception($"[Out of sync] GotActionCardAttack Has Error: attackerUnit: {attackerUnit}; target: {target}"));
                    return;
                }

                Action callback = () =>
                {
                    attackerUnit.DoCombat(target);
                };

                BoardUnitView attackerUnitView = _battlegroundController.GetCardViewByModel<BoardUnitView>(attackerUnit);

                if (attackerUnitView != null)
                {
                    _boardArrowController.DoAutoTargetingArrowFromTo<OpponentBoardArrow>(attackerUnitView.Transform, target, action: callback);
                }
                else
                {
                    Log.Warn("Attacker with card Id " + model.CardId + " not found on this client in match.");
                }

                completeCallback?.Invoke();

            }, Enumerators.QueueActionType.UnitCombat);
        }

        private async Task GotActionUseCardAbility(UseCardAbilityModel model)
        {
            if (_gameplayManager.IsGameEnded)
                return;

            IBoardObject boardObjectCaller = _battlegroundController.GetBoardObjectByInstanceId(model.Card, false);

            // HACK for items in use
            if (boardObjectCaller == null)
            {
                boardObjectCaller =
                    _boardItemsInUse
                        .FirstOrDefault(x => x.Card.InstanceId == model.Card);
            }

            if (boardObjectCaller == null || _gameplayManager.OpponentPlayer.CardsInHand.Contains(boardObjectCaller))
            {
                // FIXME: why do we have recursion here??
                await new WaitForUpdate();
                await GotActionUseCardAbility(model);

                return;
            }

            _gameplayManager.GetController<ActionsQueueController>().AddNewActionInToQueue(completeCallback =>
            {
                List<ParametrizedAbilityBoardObject> parametrizedAbilityObjects = new List<ParametrizedAbilityBoardObject>();

                foreach (Unit unit in model.Targets)
                {
                    parametrizedAbilityObjects.Add(new ParametrizedAbilityBoardObject(
                        _battlegroundController.GetTargetByInstanceId(unit.InstanceId),
                        new ParametrizedAbilityParameters
                        {
                            Attack = unit.Parameter.Attack,
                            Defense = unit.Parameter.Defense,
                            CardName = unit.Parameter.CardName,
                        }
                    ));
                }

                CardModel cardModel;
                switch (boardObjectCaller)
                {
                    case CardModel tempCardModel:
                        cardModel = tempCardModel;
                        break;
                    default:
                        Log.Warn(new ArgumentOutOfRangeException($"{nameof(boardObjectCaller)} has type: {boardObjectCaller?.GetType().ToString()}"));
                        return;
                }

                if (cardModel.Prototype.Kind == Enumerators.CardKind.ITEM)
                {
                    RemoveBoardItemInUse(cardModel);
                }

                _abilitiesController.PlayAbilityFromEvent(
                    model.AbilityType,
                    boardObjectCaller,
                    parametrizedAbilityObjects,
                    cardModel,
                    _gameplayManager.OpponentPlayer,
                    completeCallback);

            }, Enumerators.QueueActionType.AbilityUsage);
        }

        private void GotActionUseOverlordSkill(UseOverlordSkillModel model)
        {
            if (_gameplayManager.IsGameEnded)
                return;

            BoardSkill skill = _battlegroundController.GetSkillById(_gameplayManager.OpponentPlayer, model.SkillId);

            List<ParametrizedAbilityBoardObject> parametrizedAbilityObjects = new List<ParametrizedAbilityBoardObject>();

            foreach (Unit unit in model.Targets)
            {
                parametrizedAbilityObjects.Add(new ParametrizedAbilityBoardObject(
                    _battlegroundController.GetTargetByInstanceId(unit.InstanceId),
                    new ParametrizedAbilityParameters
                    {
                        Attack = unit.Parameter.Attack,
                        Defense = unit.Parameter.Defense,
                        CardName = unit.Parameter.CardName,
                    }
                ));
            }

            skill.UseSkillFromEvent(parametrizedAbilityObjects);
        }

        private void GotActionRankBuff(InstanceId card, IList<Unit> targets)
        {
            if (_gameplayManager.IsGameEnded)
                return;

            List<CardModel> units = new List<CardModel>();

            foreach (IBoardObject boardObject in _battlegroundController.GetTargetsByInstanceId(targets))
            {
                if (boardObject != null && boardObject is CardModel)
                {
                    units.Add(boardObject as CardModel);
                }
                else
                {
                    ExceptionReporter.LogExceptionAsWarning(Log, new Exception($"[Out of sync] IBoardObject {boardObject} is null or not equal to CardModel"));
                }
            }

            CardModel cardModel = _battlegroundController.GetCardModelByInstanceId(card);
            if (cardModel == null)
                ExceptionReporter.LogExceptionAsWarning(Log, new Exception($"Board unit with instance ID {card} not found"));

            if (Constants.RankSystemEnabled)
            {
                _ranksController.BuffAllyManually(units, cardModel);
            }
        }

        private void GotCheatDestroyCardsOnBoard(IEnumerable<InstanceId> cards)
        {
            foreach (InstanceId cardId in cards)
            {
                CardModel card = (CardModel)_battlegroundController.GetTargetByInstanceId(cardId);
                if (card == null)
                {
                    Log.Error($"Card {cardId} not found on board");
                }
                else
                {
                    card.Die(withDeathEffect: false);
                }
            }
        }

        #endregion

        #region Items in Use

        private void AddBoardItemInUse(CardModel cardModel)
        {
            Log.Info($"{nameof(AddBoardItemInUse)}(CardModel cardModel = {cardModel})");
            _boardItemsInUse.Add(cardModel);
        }


        private void RemoveBoardItemInUse(CardModel cardModel)
        {
            Log.Info($"{nameof(RemoveBoardItemInUse)}(CardModel cardModel = {cardModel})");
            bool removed = _boardItemsInUse.Remove(cardModel);
            if (!removed)
            {
                Log.Warn($"{nameof(RemoveBoardItemInUse)}: attempted to remove model '{cardModel}' which wasn't on the list");
            }
        }

        #endregion

    }


    #region models

    public class UseOverlordSkillModel
    {
        public SkillId SkillId;
        public List<Unit> Targets;
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
    }

    #endregion
}
