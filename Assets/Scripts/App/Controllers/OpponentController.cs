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
using Random = System.Random;

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

        private CancellationTokenSource _aiBrainCancellationTokenSource;

        private readonly Random _random = new Random();

        private PvPManager _pvpManager;

        // TODO : TODO : Find another solution, right now its tempraoray only....
        private bool _canPlayCardOnBoard;
        private PlayerActionCardPlay _playerActionCardPlay;

        public void Dispose()
        {
        }

        public void Init()
        {
            _gameplayManager = GameClient.Get<IGameplayManager>();
            _backendFacade = GameClient.Get<BackendFacade>();
            _backendDataControlMediator = GameClient.Get<BackendDataControlMediator>();
            _pvpManager = GameClient.Get<PvPManager>();

            _cardsController = _gameplayManager.GetController<CardsController>();
            _skillsController = _gameplayManager.GetController<SkillsController>();
            _battlegroundController = _gameplayManager.GetController<BattlegroundController>();
            _battleController = _gameplayManager.GetController<BattleController>();
            _boardArrowController = _gameplayManager.GetController<BoardArrowController>();
            _abilitiesController = _gameplayManager.GetController<AbilitiesController>();

            _gameplayManager.GameStarted += GameStartedHandler;
            _gameplayManager.GameEnded += GameEndedHandler;

            _pvpManager.OnCardPlayedAction += OnCardPlayedHandler;
        }

        public void ResetAll()
        {
            _aiBrainCancellationTokenSource?.Cancel();
        }

        public void Update()
        {
            /*if (_canPlayCardOnBoard)
            {
                _canPlayCardOnBoard = false;

                WorkingCard card = FromProtobufExtensions.FromProtobuf(_playerActionCardPlay.Card, _gameplayManager.OpponentPlayer);
                PlayCardOnBoard(card);
            }*/
        }


        public void InitializePlayer(int playerId)
        {
            _gameplayManager.OpponentPlayer = new Player(playerId, GameObject.Find("Opponent"), true);

            /*_fightTargetingArrowPrefab =
                _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/Gameplay/Arrow/AttackArrowVFX_Object");

            _attackedUnitTargets = new List<BoardUnit>();
            _unitsToIgnoreThisTurn = new List<BoardUnit>();

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
            */
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

        public void GotActionPlayCard(WorkingCard card)
        {
            //Player caller = _gameplayManager.GetPlayerById(model.CallerId);
            //Player fromDeckOfPlayer = _gameplayManager.GetPlayerById(model.FromDeckOfPlayerId);
            //Player targetPlayer = _gameplayManager.GetPlayerById(model.TargetId);

            _cardsController.PlayOpponentCard(_gameplayManager.OpponentPlayer, card, null, PlayCardCompleteHandler);
                //_cardsController.GetWorkingCardFromName(targetPlayer, model.CardName));

            //_gameplayManager.OpponentPlayer.Goo -= card.LibraryCard.Cost;
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
            BoardUnitView boardUnitView = _battlegroundController.GetBoardUnitViewByModel((BoardUnitModel) boardObject);

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
                        skill.FightTargetingArrow.SelectedPlayer = (Player) target;
                        break;
                    case Enumerators.AffectObjectType.CHARACTER:
                        skill.FightTargetingArrow.SelectedCard = _battlegroundController.GetBoardUnitViewByModel((BoardUnitModel) target);
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

        #region OldPVPLogic
        private void OnCardPlayedHandler(PlayerActionCardPlay cardPlay)
        {
            /*_canPlayCardOnBoard = true;
            _playerActionCardPlay = cardPlay;
            */

            WorkingCard card = FromProtobufExtensions.FromProtobuf(_playerActionCardPlay.Card, _gameplayManager.OpponentPlayer);
            GotActionPlayCard(card);
        }

        private void PlayCardCompleteHandler(WorkingCard card, object target)
        {
            /*WorkingCard workingCard =
                _gameplayManager.OpponentPlayer.CardsOnBoard[_gameplayManager.OpponentPlayer.CardsOnBoard.Count - 1];

            switch (card.LibraryCard.CardKind)
            {
                case Enumerators.CardKind.CREATURE:
                    {
                        BoardUnitView boardUnitElement = new BoardUnitView(new BoardUnitModel(), GameObject.Find("OpponentBoard").transform);
                        GameObject boardCreature = boardUnitElement.GameObject;
                        boardCreature.tag = SRTags.OpponentOwned;
                        boardCreature.transform.position = Vector3.zero;
                        boardUnitElement.Model.OwnerPlayer = card.Owner;

                        boardUnitElement.SetObjectInfo(workingCard);
                        _battlegroundController.OpponentBoardCards.Add(boardUnitElement);

                        boardCreature.transform.position +=
                            Vector3.up * 2f; // Start pos before moving cards to the opponents board

                        // PlayArrivalAnimation(boardCreature, libraryCard.cardType);
                        _gameplayManager.OpponentPlayer.BoardCards.Add(boardUnitElement);

                        _actionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
                        {
                            ActionType = Enumerators.ActionType.PlayCardFromHand,
                            Caller = boardUnitElement,
                            TargetEffects = new List<PastActionsPopup.TargetEffectParam>()
                        });

                        boardUnitElement.PlayArrivalAnimation();

                        _battlegroundController.UpdatePositionOfBoardUnitsOfOpponent(
                            () =>
                            {
                                bool createTargetArrow = false;

                                if (card.LibraryCard.Abilities != null && card.LibraryCard.Abilities.Count > 0)
                                {
                                    createTargetArrow =
                                        _abilitiesController.IsAbilityCanActivateTargetAtStart(
                                            card.LibraryCard.Abilities[0]);
                                }

                                if (target != null)
                                {
                                    CreateOpponentTarget(
                                        createTargetArrow,
                                        false,
                                        boardCreature.gameObject,
                                        target,
                                        () =>
                                        {
                                            _abilitiesController.CallAbility(card.LibraryCard, null, workingCard,
                                                Enumerators.CardKind.CREATURE, boardUnitElement, null, false, null, target);
                                        });
                                }
                                else
                                {
                                    _abilitiesController.CallAbility(card.LibraryCard, null, workingCard,
                                        Enumerators.CardKind.CREATURE, boardUnitElement, null, false, null);
                                }
                            });
                        break;
                    }
                case Enumerators.CardKind.SPELL:
                    {
                        GameObject spellCard = Object.Instantiate(_cardsController.ItemCardViewPrefab);
                        spellCard.transform.position = GameObject.Find("OpponentSpellsPivot").transform.position;

                        CurrentSpellCard = new SpellBoardCard(spellCard);

                        CurrentSpellCard.Init(workingCard);
                        CurrentSpellCard.SetHighlightingEnabled(false);

                        BoardSpell boardSpell = new BoardSpell(spellCard, workingCard);

                        spellCard.gameObject.SetActive(false);

                        bool createTargetArrow = false;

                        if (card.LibraryCard.Abilities != null && card.LibraryCard.Abilities.Count > 0)
                        {
                            createTargetArrow =
                                _abilitiesController.IsAbilityCanActivateTargetAtStart(card.LibraryCard.Abilities[0]);
                        }

                        if (target != null)
                        {
                            CreateOpponentTarget(
                                createTargetArrow,
                                false,
                                _gameplayManager.OpponentPlayer.AvatarObject,
                                target,
                                () =>
                                {
                                    _abilitiesController.CallAbility(card.LibraryCard, null, workingCard,
                                        Enumerators.CardKind.SPELL, boardSpell, null, false, null, target);
                                });
                        }
                        else
                        {
                            _abilitiesController.CallAbility(card.LibraryCard, null, workingCard,
                                Enumerators.CardKind.SPELL, boardSpell, null, false, null);
                        }

                        break;
                    }
            }*/
        }

        // rewrite
        /*private OpponentBoardArrow CreateOpponentTarget(
            bool createTargetArrow, bool isReverseArrow, GameObject startObj, object target, Action action)
        {
            if (!createTargetArrow)
            {
                action?.Invoke();
                return null;
            }

            OpponentBoardArrow targetingArrow =
                Object.Instantiate(_fightTargetingArrowPrefab).AddComponent<OpponentBoardArrow>();
            targetingArrow.Begin(startObj.transform.position);

            targetingArrow.SetTarget(target);

            MainApp.Instance.StartCoroutine(RemoveOpponentTargetingArrow(targetingArrow, action));

            return targetingArrow;
        }*/

        // rewrite
        /*private IEnumerator RemoveOpponentTargetingArrow(OpponentBoardArrow arrow, Action action)
        {
            yield return new WaitForSeconds(1f);
            arrow.Dispose();
            Object.Destroy(arrow.gameObject);

            action?.Invoke();
        }*/
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
