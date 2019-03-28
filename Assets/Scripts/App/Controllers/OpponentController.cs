using System;
using System.Collections.Generic;
using System.Linq;
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
                                    .OrderBy(card => card.InstanceId)
                                    .ToArray()
                                )
                        );

                        isMainTurnSecond = GameClient.Get<IPvPManager>().IsFirstPlayer();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                BoardUnitModel[] boardUnitModels = deck.Select(card => new BoardUnitModel(card)).ToArray();
                player.PlayerCardsController.SetCardsInDeck(boardUnitModels);

                _battlegroundController.UpdatePositionOfCardsInOpponentHand();
            }
        }

        private void GameStartedHandler()
        {
            _pvpManager.CardPlayedActionReceived += OnCardPlayedHandler;
            _pvpManager.CardAttackedActionReceived += OnCardAttackedHandler;
            _pvpManager.OverlordSkillUsedActionReceived += OnOverlordSkillUsedHandler;
            _pvpManager.LeaveMatchReceived += OnLeaveMatchHandler;
            _pvpManager.RankBuffActionReceived += OnRankBuffHandler;
            _pvpManager.CheatDestroyCardsOnBoardActionReceived += OnCheatDestroyCardsOnBoardActionHandler;
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
            _pvpManager.OverlordSkillUsedActionReceived -= OnOverlordSkillUsedHandler;
            _pvpManager.LeaveMatchReceived -= OnLeaveMatchHandler;
            _pvpManager.RankBuffActionReceived -= OnRankBuffHandler;
            _pvpManager.CheatDestroyCardsOnBoardActionReceived -= OnCheatDestroyCardsOnBoardActionHandler;
            _pvpManager.PlayerLeftGameActionReceived -= OnPlayerLeftGameActionHandler;
        }

        private void OnPlayerLeftGameActionHandler()
        {
            _gameplayManager.OpponentPlayer.PlayerDie();
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

        private void GotActionEndTurn(EndTurnModel model)
        {
            if (_gameplayManager.IsGameEnded)
                return;

            _gameplayManager.GetController<ActionsQueueController>().AddNewActionInToQueue((parameter, completeCallback) =>
            {
                _battlegroundController.EndTurn();

                completeCallback?.Invoke();

            }, Enumerators.QueueActionType.StopTurn);
        }

        private void GotActionPlayCard(InstanceId cardId, ItemPosition position)
        {
            if (_gameplayManager.IsGameEnded)
                return;

            _gameplayManager.GetController<ActionsQueueController>().AddNewActionInToQueue((parameter, completeCallback) =>
            {
                BoardUnitView boardUnitViewElement = null;
                _cardsController.PlayOpponentCard(_gameplayManager.OpponentPlayer,
                    cardId,
                    null,
                    boardUnitModel =>
                    {
                        switch (boardUnitModel.Prototype.Kind)
                        {
                            case Enumerators.CardKind.CREATURE:
                                boardUnitViewElement = new BoardUnitView(boardUnitModel, _battlegroundController.OpponentBoardObject.transform);
                                GameObject boardUnit = boardUnitViewElement.GameObject;
                                boardUnitViewElement.Model.Card.Owner = boardUnitModel.Owner;
                                boardUnitViewElement.Model.Card.TutorialObjectId = boardUnitModel.TutorialObjectId;

                                boardUnit.tag = SRTags.OpponentOwned;
                                boardUnit.transform.position = Vector3.up * 2f; // Start pos before moving cards to the opponents board
                                boardUnit.SetActive(false);

                                _gameplayManager.OpponentPlayer.PlayerCardsController.AddCardToBoard(boardUnitModel, position);
                                _battlegroundController.RegisterBoardUnitView(
                                    _gameplayManager.OpponentPlayer,
                                    boardUnitViewElement,
                                    InternalTools.GetSafePositionToInsert(position, _gameplayManager.OpponentPlayer.CardsOnBoard)
                                    );

                                _actionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam
                                {
                                    ActionType = Enumerators.ActionType.PlayCardFromHand,
                                    Caller = boardUnitViewElement.Model,
                                    TargetEffects = new List<PastActionsPopup.TargetEffectParam>()
                                });

                                break;
                            case Enumerators.CardKind.ITEM:
                                BoardItem item = new BoardItem(null, boardUnitModel); // todo improve it with game Object aht will be aniamted
                                _gameplayManager.OpponentPlayer.BoardItemsInUse.Insert(ItemPosition.End, item);
                                item.Model.Owner = _gameplayManager.OpponentPlayer;
                                _actionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam
                                {
                                    ActionType = Enumerators.ActionType.PlayCardFromHand,
                                    Caller = item,
                                    TargetEffects = new List<PastActionsPopup.TargetEffectParam>()
                                });

                                // TODO: make sure this works later
                                //_gameplayManager.OpponentPlayer.BoardItemsInUse.Remove(item);
                                break;
                        }

                        _gameplayManager.OpponentPlayer.CurrentGoo -= boardUnitModel.InstanceCard.Cost;
                    },
                    (workingCard, boardObject) =>
                    {
                        switch (workingCard.Prototype.Kind)
                        {
                            case Enumerators.CardKind.CREATURE:
                                boardUnitViewElement.GameObject.SetActive(true);
                                boardUnitViewElement.PlayArrivalAnimation(playUniqueAnimation: true);
                                _boardController.UpdateCurrentBoardOfPlayer(_gameplayManager.OpponentPlayer, null);
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

            _gameplayManager.GetController<ActionsQueueController>().AddNewActionInToQueue((parameter, completeCallback) =>
            {
                BoardUnitModel attackerUnit = _battlegroundController.GetBoardUnitModelByInstanceId(model.CardId);
                BoardObject target = _battlegroundController.GetTargetByInstanceId(model.TargetId, false);

                if (attackerUnit == null || target == null || attackerUnit is default(BoardUnitModel) || attackerUnit is default(BoardUnitModel))
                {
                    ExceptionReporter.LogExceptionAsWarning(Log, new Exception($"[Out of sync] GotActionCardAttack Has Error: attackerUnit: {attackerUnit}; target: {target}"));
                    return;
                }

                Action callback = () =>
                {
                    attackerUnit.DoCombat(target);
                };

                BoardUnitView attackerUnitView = _battlegroundController.GetBoardUnitViewByModel<BoardUnitView>(attackerUnit);

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

            List<BoardUnitModel> units = new List<BoardUnitModel>();

            foreach (BoardObject boardObject in _battlegroundController.GetTargetsByInstanceId(targets))
            {
                if (boardObject != null && boardObject is BoardUnitModel)
                {
                    units.Add(boardObject as BoardUnitModel);
                }
                else
                {
                    ExceptionReporter.LogExceptionAsWarning(Log, new Exception($"[Out of sync] BoardObject {boardObject} is null or not equal to BoardUnitModel"));
                }
            }

            BoardUnitModel boardUnitModel = _battlegroundController.GetBoardUnitModelByInstanceId(card);
            if (boardUnitModel == null)
                ExceptionReporter.LogExceptionAsWarning(Log, new Exception($"Board unit with instance ID {card} not found"));

            _ranksController.BuffAllyManually(units, boardUnitModel);
        }

        private void GotCheatDestroyCardsOnBoard(IEnumerable<InstanceId> cards)
        {
            foreach (InstanceId cardId in cards)
            {
                BoardUnitModel card = (BoardUnitModel)_battlegroundController.GetTargetByInstanceId(cardId);
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
    }

    #region models
    public class EndTurnModel
    {
        public InstanceId CallerId;
    }

    public class UseOverlordSkillModel
    {
        public SkillId SkillId;
        public List<Unit> Targets;
    }

    public class CardAttackModel
    {
        public InstanceId CardId;
        public InstanceId TargetId;
    }

    public class TargetUnitModel
    {
        public InstanceId Target;
        public Enumerators.AffectObjectType AffectObjectType;
    }

    #endregion
}
