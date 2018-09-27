using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using UnityEngine;
using Newtonsoft.Json;

namespace Loom.ZombieBattleground
{
    public class OpponentController : IController
    {
        private IGameplayManager _gameplayManager;
        private IDataManager _dataManager;
        private BackendFacade _backendFacade;
        private BackendDataControlMediator _backendDataControlMediator;

        private CardsController _cardsController;
        private BattlegroundController _battlegroundController;
        private SkillsController _skillsController;
        private BattleController _battleController;
        private BoardArrowController _boardArrowController;
        private AbilitiesController _abilitiesController;


        public void Dispose()
        {
        }

        public void Init()
        {
            _gameplayManager = GameClient.Get<IGameplayManager>();
            _backendFacade = GameClient.Get<BackendFacade>();
            _backendDataControlMediator = GameClient.Get<BackendDataControlMediator>();
            
            _cardsController = _gameplayManager.GetController<CardsController>();
            _skillsController = _gameplayManager.GetController<SkillsController>();
            _battlegroundController = _gameplayManager.GetController<BattlegroundController>();
            _battleController = _gameplayManager.GetController<BattleController>();
            _boardArrowController = _gameplayManager.GetController<BoardArrowController>();
            _abilitiesController = _gameplayManager.GetController<AbilitiesController>();

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
        }


        private void GameStartedHandler()
        {

        }

        private void GameEndedHandler(Enumerators.EndGameType endGameType)
        {

        }


        public void DoActionByType(Enumerators.PlayerAction action, string data)
        {
            switch (action)
            {
                case Enumerators.PlayerAction.EndTurn:
                    {
                        GotActionEndTurn(JsonConvert.DeserializeObject<EndTurnModel>(data));
                    }
                    break;
                case Enumerators.PlayerAction.CardAttack:
                    {
                        GotActionCardAttack(JsonConvert.DeserializeObject<CardAttackModel>(data));
                    }
                    break;
                case Enumerators.PlayerAction.DrawCard:
                    {
                        GotActionDrawCard(JsonConvert.DeserializeObject<DrawCardModel>(data));
                    }
                    break;
                case Enumerators.PlayerAction.Mulligan:
                    {
                        GotActionMulligan(JsonConvert.DeserializeObject<MulliganModel>(data));
                    }
                    break;
                case Enumerators.PlayerAction.UseCardAbility:
                    {
                        GotActionUseCardAbility(JsonConvert.DeserializeObject<UseCardAbilityModel>(data));

                    }
                    break;
                case Enumerators.PlayerAction.UseOverlordSkill:
                    {
                        GotActionUseOverlordSkill(JsonConvert.DeserializeObject<UseOverlordSkillModel>(data));
                    }
                    break;
                default: break;
            }
        }

        #region requests

        public async Task ActionEndTurn(Player player)
        {
            if (!_backendFacade.IsConnected)
                return;

            EndTurnModel model = new EndTurnModel()
            {
                Id = (int)Time.time,
                CallerId = player.Id
            };

            await _backendFacade.SendEndTurn(_backendDataControlMediator.UserDataModel.UserId, model);
        }

        public async Task ActionDrawCard(Player player, Player fromDeckOfPlayer, Player toPlayer, Enumerators.AffectObjectType affectObjectType, string cardName = null)
        {
            if (!_backendFacade.IsConnected)
                return;

            DrawCardModel model = new DrawCardModel()
            {
                Id = (int)Time.time,
                CallerId = player.Id,
                CardName = cardName,
                FromDeckOfPlayerId = fromDeckOfPlayer.Id,
                TargetId = toPlayer.Id,
                AffectObjectType = affectObjectType
            };

            await _backendFacade.SendDrawCard(_backendDataControlMediator.UserDataModel.UserId, model);
        }

        public async Task ActionCardAttack(Player player, BoardUnitModel attacker, BoardObject target, Enumerators.AffectObjectType affectObjectType)
        {
            if (!_backendFacade.IsConnected)
                return;

            CardAttackModel model = new CardAttackModel()
            {
                Id = (int)Time.time,
                CallerId = player.Id,
                CardId = attacker.Id,
                TargetId = target.Id,
                AffectObjectType = affectObjectType
            };

            await _backendFacade.SendCardAttack(_backendDataControlMediator.UserDataModel.UserId, model);
        }

        public async Task ActionUseCardAbility(Player player, Card card, BoardObject boardObject, BoardObject target = null,
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
                Id = (int)Time.time,
                CallerId = player.Id,
                AffectObjectType = affectObjectType,
                TargetId = targetId,
                BoardObjectId = boardObject.Id,
                CardId = card.Id,
                CardKind = card.CardKind
            };

            await _backendFacade.SendUseCardAbility(_backendDataControlMediator.UserDataModel.UserId, model);
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
                Id = (int)Time.time,
                CallerId = player.Id,
                SkillId = skill.Id,
                TargetId = targetId,
                AffectObjectType = affectObjectType
            };

            await _backendFacade.SendUseOverlordSkill(_backendDataControlMediator.UserDataModel.UserId, model);
        }

        public async Task ActionMulligan(Player player, List<WorkingCard> cards)
        {
            if (!_backendFacade.IsConnected)
                return;

            List<int> cardsIds = cards.Select(card => card.Id).ToList();

            MulliganModel model = new MulliganModel()
            {
                Id = (int)Time.time,
                CallerId = player.Id,
                CardsIds = cardsIds
            };

            await _backendFacade.SendMulligan(_backendDataControlMediator.UserDataModel.UserId, model);
        }
        #endregion

        #region responses

        public void GotActionEndTurn(EndTurnModel model)
        {
            Player caller = _gameplayManager.GetPlayerById(model.CallerId);

            _battlegroundController.EndTurn();
        }

        public void GotActionDrawCard(DrawCardModel model)
        {
            Player caller = _gameplayManager.GetPlayerById(model.CallerId);
            Player fromDeckOfPlayer = _gameplayManager.GetPlayerById(model.FromDeckOfPlayerId);
            Player targetPlayer = _gameplayManager.GetPlayerById(model.TargetId);

            _cardsController.AddCardToHandFromOtherPlayerDeck(fromDeckOfPlayer, targetPlayer, _cardsController.GetWorkingCardFromName(targetPlayer, model.CardName));
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

            BoardUnitView attackerUnitView = _battlegroundController.GetBoardUnitView(attackerUnit);
            _boardArrowController.DoAutoTargetingArrowFromTo<OpponentBoardArrow>(attackerUnitView.Transform, target, action: callback);
        }

        public void GotActionUseCardAbility(UseCardAbilityModel model)
        {
            BoardObject target = _battlegroundController.GetTargetById(model.TargetId, model.AffectObjectType);
            Card libraryCard = _dataManager.CachedCardsLibraryData.Cards.Find(card => card.Id == model.CardId);
            BoardObject boardObject = _battlegroundController.GetBoardObjectById(model.BoardObjectId);
            BoardUnitView boardUnitView = _battlegroundController.GetBoardUnitView(boardObject as BoardUnitModel);

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
                if (model.AffectObjectType == Enumerators.AffectObjectType.PLAYER)
                {
                    skill.FightTargetingArrow.SelectedPlayer = target as Player;
                }
                else if (model.AffectObjectType == Enumerators.AffectObjectType.CHARACTER)
                {
                    skill.FightTargetingArrow.SelectedCard = _battlegroundController.GetBoardUnitView(target as BoardUnitModel);
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
        public int Id;

        public int CallerId;
    }

    public class MulliganModel
    {
        public int Id;

        public int CallerId;
        public List<int> CardsIds;
    }

    public class DrawCardModel
    {
        public int Id;

        public string CardName;
        public int CallerId;
        public int FromDeckOfPlayerId;
        public int TargetId;
        public Enumerators.AffectObjectType AffectObjectType;
    }

    public class UseOverlordSkillModel
    {
        public int Id;

        public int SkillId;
        public int CallerId;
        public int TargetId;
        public Enumerators.AffectObjectType AffectObjectType;
    }

    public class UseCardAbilityModel
    {
        public int Id;

        public int CardId;
        public Enumerators.CardKind CardKind;
        public int BoardObjectId;
        public int CardAbilityId;
        public int CallerId;
        public int TargetId;
        public Enumerators.AffectObjectType AffectObjectType;
    }

    public class CardAttackModel
    {
        public int Id;

        public int CardId;
        public int CallerId;
        public int TargetId;
        public Enumerators.AffectObjectType AffectObjectType;
    }

    #endregion
}
