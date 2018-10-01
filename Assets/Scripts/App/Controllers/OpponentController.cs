using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.Protobuf;
using UnityEngine;
using Newtonsoft.Json;
using Object = UnityEngine.Object;

namespace Loom.ZombieBattleground
{
    public class OpponentController : IController
    {
        private IGameplayManager _gameplayManager;
        private IDataManager _dataManager;
        private IPvPManager _pvpManager;
        private BackendFacade _backendFacade;
        private BackendDataControlMediator _backendDataControlMediator;

        private CardsController _cardsController;
        private BattlegroundController _battlegroundController;
        private SkillsController _skillsController;
        private BattleController _battleController;
        private BoardArrowController _boardArrowController;
        private AbilitiesController _abilitiesController;
        private ActionsQueueController _actionsQueueController;

        public void Dispose()
        {
        }

        public void Init()
        {
            _gameplayManager = GameClient.Get<IGameplayManager>();
            _backendFacade = GameClient.Get<BackendFacade>();
            _backendDataControlMediator = GameClient.Get<BackendDataControlMediator>();
            _pvpManager = GameClient.Get<IPvPManager>();

            _cardsController = _gameplayManager.GetController<CardsController>();
            _skillsController = _gameplayManager.GetController<SkillsController>();
            _battlegroundController = _gameplayManager.GetController<BattlegroundController>();
            _battleController = _gameplayManager.GetController<BattleController>();
            _boardArrowController = _gameplayManager.GetController<BoardArrowController>();
            _abilitiesController = _gameplayManager.GetController<AbilitiesController>();
            _actionsQueueController = _gameplayManager.GetController<ActionsQueueController>();

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
            _gameplayManager.OpponentPlayer = new Player(playerId, GameObject.Find("Opponent"), true);

            if (!_gameplayManager.IsSpecificGameplayBattleground)
            {
                List<string> playerDeck = new List<string>();
                OpponentDeck opponentDeck = _pvpManager.OpponentDeck;

                foreach (DeckCardData card in opponentDeck.Cards)
                {
                    for (int i = 0; i < card.Amount; i++)
                    {
                        playerDeck.Add(card.CardName);
                    }
                }

                _gameplayManager.OpponentPlayer.SetDeck(playerDeck);

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
        }

        private void GameEndedHandler(Enumerators.EndGameType endGameType)
        {
            _pvpManager.CardPlayedActionReceived -= OnCardPlayedHandler;
            _pvpManager.CardAttackedActionReceived -= OnCardAttackedHandler;
            _pvpManager.DrawCardActionReceived -= OnDrawCardHandler;
            _pvpManager.CardAbilityUsedActionReceived -= OnCardAbilityUsedHandler;
            _pvpManager.OverlordSkillUsedActionReceived -= OnOverlordSkillUsedHandler;
            _pvpManager.MulliganProcessUsedActionReceived -= OnMulliganProcessHandler;
        }


    #region requests

    public async Task ActionEndTurn(Player player)
        {
            if (!_backendFacade.IsConnected)
                return;

            EndTurnModel model = new EndTurnModel()
            {
                CallerId = player.Id
            };
        }

        public async Task ActionDrawCard(Player player, Player fromDeckOfPlayer, Player toPlayer, Enumerators.AffectObjectType affectObjectType, string cardName = null)
        {
            if (!_backendFacade.IsConnected)
                return;

            DrawCardModel model = new DrawCardModel()
            {
                CallerId = player.Id,
                CardName = cardName,
                FromDeckOfPlayerId = fromDeckOfPlayer.Id,
                TargetId = toPlayer.Id,
                AffectObjectType = affectObjectType
            };
        }

        public async Task ActionCardAttack(Player player, BoardUnitModel attacker, BoardObject target, Enumerators.AffectObjectType affectObjectType)
        {
            if (!_backendFacade.IsConnected)
                return;

            CardAttackModel model = new CardAttackModel()
            {
                CallerId = player.Id,
                CardId = attacker.Id,
                TargetId = target.Id,
                AffectObjectType = affectObjectType
            };
        }

        public async Task ActionUseCardAbility(Player player, Data.Card card, BoardObject boardObject, BoardObject target = null,
                                               Enumerators.AffectObjectType affectObjectType = Enumerators.AffectObjectType.NONE)
        {
            if (!_backendFacade.IsConnected)
                return;

            int targetId = -1;

            if (target != null)
            {
                targetId = target.Id;
            }

            UseCardAbilityModel model = new UseCardAbilityModel()
            {
                AffectObjectType = affectObjectType,
                TargetId = targetId,
                BoardObjectId = boardObject.Id,
                CardId = card.Id,
                CardKind = card.CardKind
            };
        }

        public async Task ActionUseOverlordSkill(Player player, BoardSkill skill, BoardObject target = null,
                                                 Enumerators.AffectObjectType affectObjectType = Enumerators.AffectObjectType.NONE)
        {
            if (!_backendFacade.IsConnected)
                return;

            int targetId = -1;

            if (target != null)
            {
                targetId = target.Id;
            }

            UseOverlordSkillModel model = new UseOverlordSkillModel()
            {
                CallerId = player.Id,
                SkillId = skill.Id,
                TargetId = targetId,
                AffectObjectType = affectObjectType
            };
        }

        public async Task ActionMulligan(Player player, List<WorkingCard> cards)
        {
            if (!_backendFacade.IsConnected)
                return;

            List<int> cardsIds = cards.Select(card => card.Id).ToList();

            MulliganModel model = new MulliganModel()
            {
                CallerId = player.Id,
                CardsIds = cardsIds
            };
        }
        #endregion


        #region event handlers

        private void OnCardPlayedHandler(PlayerActionCardPlay cardPlay)
        {
            GotActionPlayCard(FromProtobufExtensions.FromProtobuf(cardPlay.Card, _gameplayManager.OpponentPlayer));
        }

        private void OnCardAttackedHandler(PlayerActionCardAttack actionCardAttack)
        {
            // todo improve for players!
            WorkingCard attacker = FromProtobufExtensions.FromProtobuf(actionCardAttack.Attacker, _gameplayManager.OpponentPlayer);
            WorkingCard target = FromProtobufExtensions.FromProtobuf(actionCardAttack.Target, _gameplayManager.CurrentPlayer);

            GotActionCardAttack(new CardAttackModel()
            {
                CardId = attacker.Id,
                CallerId = _gameplayManager.OpponentPlayer.Id,
                TargetId = target.Id,
                AffectObjectType = Enumerators.AffectObjectType.CHARACTER
            });
        }

        private void OnDrawCardHandler(PlayerActionDrawCard actionDrawCard)
        {
            GotActionDrawCard(FromProtobufExtensions.FromProtobuf(actionDrawCard.CardInstance, _gameplayManager.OpponentPlayer));
        }

        private void OnCardAbilityUsedHandler(PlayerActionUseCardAbility actionUseCardAbility)
        {
        }

        private void OnOverlordSkillUsedHandler(PlayerActionUseOverlordSkill actionUseOverlordSkill)
        {

        }

        private void OnMulliganProcessHandler(PlayerActionMulligan actionMulligan)
        {

        }

        #endregion


        #region Actions

        public void GotActionEndTurn(EndTurnModel model)
        {
            _battlegroundController.EndTurn();
        }

        public void GotActionDrawCard(WorkingCard drawedCard)
        {
            _cardsController.AddCardToHandFromOtherPlayerDeck(drawedCard.Owner, drawedCard.Owner, drawedCard);
        }

        public void GotActionPlayCard(WorkingCard card)
        {
            _cardsController.PlayOpponentCard(_gameplayManager.OpponentPlayer, card, null, (workingCard, boardObject) =>
            {
                switch (workingCard.LibraryCard.CardKind)
                {
                    case Enumerators.CardKind.CREATURE:
                        {
                            BoardUnitView boardUnitViewElement = new BoardUnitView(new BoardUnitModel(), _battlegroundController.OpponentBoardObject.transform);
                            GameObject boardUnit = boardUnitViewElement.GameObject;
                            boardUnit.tag = SRTags.OpponentOwned;
                            boardUnit.transform.position = Vector3.zero;
                            boardUnitViewElement.Model.OwnerPlayer = workingCard.Owner;

                            boardUnitViewElement.SetObjectInfo(workingCard);

                            boardUnit.transform.position += Vector3.up * 2f; // Start pos before moving cards to the opponents board

                            _battlegroundController.OpponentBoardCards.Add(boardUnitViewElement);
                            _gameplayManager.OpponentPlayer.BoardCards.Add(boardUnitViewElement);

                            boardUnitViewElement.PlayArrivalAnimation();

                            _battlegroundController.UpdatePositionOfBoardUnitsOfOpponent();

                            _actionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
                            {
                                ActionType = Enumerators.ActionType.PlayCardFromHand,
                                Caller = boardUnitViewElement.Model,
                                TargetEffects = new List<PastActionsPopup.TargetEffectParam>()
                            });
                        }
                        break;
                    case Enumerators.CardKind.SPELL:
                        break;
                }
            });
        }

        public void GotActionCardAttack(CardAttackModel model)
        {
            Player caller = _gameplayManager.GetPlayerById(model.CallerId);
            BoardUnitModel attackerUnit = _battlegroundController.GetBoardUnitById(caller, model.CardId);
            BoardObject target = _battlegroundController.GetTargetById(model.TargetId, model.AffectObjectType);

            Action callback = () =>
            {
                attackerUnit.DoCombat(target);
            };

            BoardUnitView attackerUnitView = _battlegroundController.GetBoardUnitViewByModel(attackerUnit);
            _boardArrowController.DoAutoTargetingArrowFromTo<OpponentBoardArrow>(attackerUnitView.Transform, target, action: callback);
        }

        public void GotActionUseCardAbility(UseCardAbilityModel model)
        {
            BoardObject target = _battlegroundController.GetTargetById(model.TargetId, model.AffectObjectType);
            Data.Card libraryCard = _dataManager.CachedCardsLibraryData.Cards.Find(card => card.Id == model.CardId);
            BoardObject boardObject = _battlegroundController.GetBoardObjectById(model.BoardObjectId);
            BoardUnitView boardUnitView = _battlegroundController.GetBoardUnitViewByModel((BoardUnitModel)boardObject);

            Transform transform = model.CardKind == Enumerators.CardKind.SPELL ?
                                 _gameplayManager.OpponentPlayer.AvatarObject.transform : boardUnitView.Transform;

            WorkingCard workingCard = _gameplayManager.OpponentPlayer.CardsOnBoard[_gameplayManager.OpponentPlayer.CardsOnBoard.Count - 1];

            if (target != null)
            {
                Action callback = () =>
                {
                    _abilitiesController.CallAbility(libraryCard, null, workingCard, model.CardKind, boardObject, null, false, null, target);
                };

                _boardArrowController.DoAutoTargetingArrowFromTo<OpponentBoardArrow>(transform, target, action: callback);
            }
            else
            {
                _abilitiesController.CallAbility(libraryCard, null, workingCard, model.CardKind, boardObject, null, false, null);
            }
        }

        public void GotActionUseOverlordSkill(UseOverlordSkillModel model)
        {
            Player caller = _gameplayManager.GetPlayerById(model.CallerId);
            BoardSkill skill = _battlegroundController.GetSkillById(caller, model.SkillId);
            BoardObject target = _battlegroundController.GetTargetById(model.TargetId, model.AffectObjectType);

            Action callback = () =>
            {
                switch (model.AffectObjectType)
                {
                    case Enumerators.AffectObjectType.PLAYER:
                        skill.FightTargetingArrow.SelectedPlayer = (Player)target;
                        break;
                    case Enumerators.AffectObjectType.CHARACTER:
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
            // todo implement logic..
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
        public int CallerId;
        public int TargetId;
        public Enumerators.AffectObjectType AffectObjectType;
    }

    public class UseCardAbilityModel
    {
        public int CardId;
        public Enumerators.CardKind CardKind;
        public int BoardObjectId;
        public int CardAbilityId;
        public int TargetId;
        public Enumerators.AffectObjectType AffectObjectType;
    }

    public class CardAttackModel
    {
        public int CardId;
        public int CallerId;
        public int TargetId;
        public Enumerators.AffectObjectType AffectObjectType;
    }

    #endregion
}
