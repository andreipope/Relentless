using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.Helpers;
using Newtonsoft.Json;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Loom.ZombieBattleground
{
    public class AbilitiesController : IController
    {
        private static readonly ILog Log = Logging.GetLog(nameof(AbilitiesController));

        public delegate void AbilityUsedEventHandler(
            BoardUnitModel boardUnitModel,
            Enumerators.AbilityType abilityType,
            List<ParametrizedAbilityBoardObject> targets = null);
        public event AbilityUsedEventHandler AbilityUsed;

        private readonly object _lock = new object();

        private IGameplayManager _gameplayManager;

        private ITutorialManager _tutorialManager;

        private CardsController _cardsController;

        private PlayerController _playerController;

        private BattlegroundController _battlegroundController;

        private ActionsQueueController _actionsQueueController;

        private BoardArrowController _boardArrowController;

        private BoardController _boardController;

        private ulong _castedAbilitiesIds;

        private List<ActiveAbility> _activeAbilities;

        public bool BlockEndTurnButton { get; private set; }

        public bool HasPredefinedChoosableAbility { get; set; }

        public int PredefinedChoosableAbilityId { get; set; } = -1;

        public void Init()
        {
            _activeAbilities = new List<ActiveAbility>();

            _gameplayManager = GameClient.Get<IGameplayManager>();
            _tutorialManager = GameClient.Get<ITutorialManager>();

            _cardsController = _gameplayManager.GetController<CardsController>();
            _playerController = _gameplayManager.GetController<PlayerController>();
            _battlegroundController = _gameplayManager.GetController<BattlegroundController>();
            _actionsQueueController = _gameplayManager.GetController<ActionsQueueController>();
            _boardArrowController = _gameplayManager.GetController<BoardArrowController>();
            _boardController = _gameplayManager.GetController<BoardController>();
        }

        public void ResetAll()
        {
            Reset();
        }

        public void Update()
        {
            lock (_lock)
            {
                for (int i = 0; i < _activeAbilities.Count; i++)
                {
                    _activeAbilities[i].Ability.Update();
                }
            }
        }

        public void Dispose()
        {
            Reset();
        }

        public void Reset()
        {
            lock (_lock)
            {
                foreach (ActiveAbility item in _activeAbilities)
                {
                    item.Ability.Dispose();
                }

                _activeAbilities.Clear();
            }

            _castedAbilitiesIds = 0;

            PredefinedChoosableAbilityId = -1;
            HasPredefinedChoosableAbility = false;
        }

        public void DeactivateAbility(ulong id)
        {
            lock (_lock)
            {
                ActiveAbility item = _activeAbilities.Find(x => x.Id == id);
                if (_activeAbilities.Contains(item))
                {
                    _activeAbilities.Remove(item);
                }

                if (item != null)
                {
                    item.Ability?.Dispose();
                }
            }
        }

        public List<AbilityBase> GetAbilitiesConnectedToUnit(BoardUnitModel unit)
        {
            return _activeAbilities.FindAll(x => x.Ability.TargetUnit == unit || x.Ability.AbilityUnitOwner == unit).Select(y => y.Ability).ToList();
        }

        public ActiveAbility CreateActiveAbility(
            AbilityData abilityData,
            Enumerators.CardKind kind,
            object boardObject,
            Player caller,
            BoardUnitModel boardUnitModel)
        {
            lock (_lock)
            {
                CreateAbilityByType(kind, abilityData, out AbilityBase ability, out AbilityViewBase abilityView);
                ActiveAbility activeAbility = new ActiveAbility
                {
                    Id = _castedAbilitiesIds++,
                    Ability = ability,
                    AbilityView = abilityView
                };

                activeAbility.Ability.ActivityId = activeAbility.Id;
                activeAbility.Ability.PlayerCallerOfAbility = caller;
                activeAbility.Ability.BoardUnitModel = boardUnitModel;

                switch(boardObject)
                {
                    case BoardCardView card:
                        activeAbility.Ability.BoardCardView = card;
                        break;
                    case BoardUnitModel model:
                        activeAbility.Ability.AbilityUnitOwner = model;
                        break;
                    case BoardItem item:
                        activeAbility.Ability.BoardItem = item;
                        break;
                    case BoardUnitView view:
                        activeAbility.Ability.AbilityUnitOwner = view.Model;
                        break;
                    case Player player:
                        break;
                    case null:
                        break;
                    default:
                        throw new NotImplementedException($"boardObject with type {boardObject.GetType().ToString()} not implemented!");
                }
                _activeAbilities.Add(activeAbility);

                return activeAbility;
            }
        }

        public bool HasTargets(AbilityData ability)
        {
            if (ability.Targets.Count > 0)
            {
                return true;
            }

            return false;
        }

        public bool IsAbilityActive(AbilityData ability)
        {
            if (ability.Activity == Enumerators.AbilityActivity.ACTIVE)
            {
                return true;
            }

            return false;
        }

        public bool IsAbilityCallsAtStart(AbilityData ability)
        {
            if (ability.Trigger == Enumerators.AbilityTrigger.ENTRY)
            {
                return true;
            }

            return false;
        }

        public bool IsAbilityCanActivateTargetAtStart(AbilityData ability)
        {
            if (HasTargets(ability) && IsAbilityCallsAtStart(ability) && IsAbilityActive(ability))
            {
                return true;
            }

            return false;
        }

        public bool CheckActivateAvailability(Enumerators.CardKind kind, AbilityData ability, Player localPlayer)
        {
            bool available = false;

            Player opponent = localPlayer.Equals(_gameplayManager.CurrentPlayer) ?
                _gameplayManager.OpponentPlayer :
                _gameplayManager.CurrentPlayer;

            lock (_lock)
            {
                foreach (Enumerators.Target item in ability.Targets)
                {
                    switch (item)
                    {
                        case Enumerators.Target.OPPONENT_CARD:
                            if (opponent.CardsOnBoard.Count > 0)
                            {
                                available = true;
                            }
                            break;
                        case Enumerators.Target.PLAYER_CARD:
                            if (localPlayer.CardsOnBoard.Count > 1 || kind == Enumerators.CardKind.ITEM)
                            {
                                available = true;
                            }
                            break;
                        case Enumerators.Target.PLAYER:
                        case Enumerators.Target.OPPONENT:
                        case Enumerators.Target.ALL:
                            available = true;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(item), item, null);
                    }
                }
            }

            return available;
        }

        public int GetStatModificatorByAbility(BoardUnitModel attacker, BoardUnitModel attacked, bool isAttackking)
        {
            int value = 0;

            IReadOnlyCard attackedCard = attacked.Card.Prototype;
            IReadOnlyCard attackerCard = attacker.Card.Prototype;

            List<AbilityData> abilities;

            if (isAttackking)
            {
                abilities = attackerCard.Abilities.FindAll(x =>
                    x.Ability == Enumerators.AbilityType.MODIFICATOR_STATS);

                for (int i = 0; i < abilities.Count; i++)
                {
                    if (attackedCard.Faction == abilities[i].Faction &&
                        abilities[i].Trigger == Enumerators.AbilityTrigger.PERMANENT)
                    {
                        value += abilities[i].Value;
                    }
                }
            }

            abilities = attackerCard.Abilities.FindAll(x =>
                x.Ability == Enumerators.AbilityType.ADDITIONAL_DAMAGE_TO_HEAVY_IN_ATTACK);

            for (int i = 0; i < abilities.Count; i++)
            {
                if (attacked.IsHeavyUnit)
                {
                    value += abilities[i].Value;
                }
            }

            return value;
        }

        public bool HasSpecialUnitOnBoard(BoardUnitModel boardUnitModel, AbilityData ability)
        {
            return GetUnitsFromTargets(boardUnitModel, ability).FindAll(item => item.InitialUnitType == ability.TargetCardType).Count > 0;
        }

        public bool HasUnitsWithoutTargetUnitType(BoardUnitModel boardUnitModel, AbilityData ability)
        {
            return GetUnitsFromTargets(boardUnitModel, ability).FindAll(item => item.InitialUnitType != ability.TargetUnitType).Count > 0;
        }

        public bool HasSpecialUnitStatusOnBoard(BoardUnitModel boardUnitModel, AbilityData ability)
        {
            return GetUnitsFromTargets(boardUnitModel, ability).FindAll(item => item.UnitSpecialStatus == ability.TargetUnitSpecialStatus).Count > 0;
        }

        public bool HasSpecialUnitFactionOnMainBoard(BoardUnitModel boardUnitModel, AbilityData ability)
        {
            if (boardUnitModel.Owner.CardsOnBoard.
                FindAll(x => x.Card.Prototype.Faction == ability.TargetFaction && x != boardUnitModel).Count > 0)
                return true;

            return false;
        }

        public bool CanTakeControlUnit(BoardUnitModel boardUnitModel, AbilityData ability)
        {
            if (ability.Ability == Enumerators.AbilityType.TAKE_CONTROL_ENEMY_UNIT &&
                ability.Trigger == Enumerators.AbilityTrigger.ENTRY &&
                ability.Activity == Enumerators.AbilityActivity.ACTIVE &&
                boardUnitModel.Owner.CardsOnBoard.Count >= boardUnitModel.Owner.MaxCardsInPlay)
                return false;

            return true;
        }

        public bool HasUnitsOnBoardThatCostMoreThan(BoardUnitModel boardUnitModel, AbilityData ability)
        {
            return GetUnitsFromTargets(boardUnitModel, ability).FindAll(item => item.CurrentCost > boardUnitModel.CurrentCost).Count > 0;
        }

        public IReadOnlyList<BoardUnitModel> GetUnitsFromTargets(BoardUnitModel boardUnitModel, AbilityData ability)
        {
            if (ability.Targets.Count == 0)
                return new List<BoardUnitModel>();

            Player opponent = boardUnitModel.Owner == _gameplayManager.CurrentPlayer ?
               _gameplayManager.OpponentPlayer :
               _gameplayManager.CurrentPlayer;
            Player player = boardUnitModel.Owner;

            foreach (Enumerators.Target target in ability.Targets)
            {
                switch (target)
                {
                    case Enumerators.Target.PLAYER_CARD:
                        return player.CardsOnBoard;
                    case Enumerators.Target.OPPONENT_CARD:
                        return opponent.CardsOnBoard;
                }
            }

            return new List<BoardUnitModel>();
        }

        public bool OverlordDefenseEqualOrLess(BoardUnitModel boardUnitModel, AbilityData ability)
        {
            return boardUnitModel.Owner.Defense <= ability.Defense;
        }

        private ActiveAbility _activeAbility;
        public ActiveAbility CurrentActiveAbility
        {
            get { return _activeAbility; }
        }

        public void CallAbility(
            BoardCardView card,
            BoardUnitModel boardUnitModel,
            Enumerators.CardKind kind,
            BoardObject boardObject,
            Action<BoardCardView> action,
            bool isPlayer,
            Action<bool> onCompleteCallback,
            GameplayQueueAction<object> actionInQueue,
            BoardObject target = null,
            HandBoardCard handCard = null,
            bool skipEntryAbilities = false)
        {
            IReadOnlyCard prototype = boardUnitModel.Card.Prototype;

            IReadOnlyCardInstanceSpecificData instance = boardUnitModel.Card.InstanceCard;

            GameplayQueueAction<object> abilityHelperAction = null;

            actionInQueue.Action = (parameter, completeCallback) =>
               {
                   ResolveAllAbilitiesOnUnit(boardObject, false);

                   Action abilityEndAction = () =>
                   {
                       bool canUseAbility = false;
                       _activeAbility = null;
                       foreach (AbilityData item in instance.Abilities)
                       {
                           _activeAbility = CreateActiveAbility(item, kind, boardObject, boardUnitModel.OwnerPlayer, boardUnitModel);

                           if (IsAbilityCanActivateTargetAtStart(item))
                           {
                               canUseAbility = true;
                           }
                           else
                           {
                               _activeAbility.Ability.Activate();
                           }
                       }

                           if (handCard != null && isPlayer)
                           {
                               handCard.GameObject.SetActive(false);
                           }

                       if (canUseAbility)
                       {
                           AbilityData ability = instance.Abilities.First(IsAbilityCanActivateTargetAtStart);

                           if (CheckAbilityOnTarget(boardUnitModel, ability))
                           {
                               CallPermanentAbilityAction(isPlayer, action, card, target, _activeAbility, kind);

                               onCompleteCallback?.Invoke(true);

                               ResolveAllAbilitiesOnUnit(boardObject);

                               abilityHelperAction?.ForceActionDone();
                               abilityHelperAction = null;

                               completeCallback?.Invoke();
                               return;
                           }

                           if (CheckActivateAvailability(kind, ability, boardUnitModel.Owner))
                           {
                               _activeAbility.Ability.Activate();

                               if (isPlayer && target != null)
                               {
                                   switch (target)
                                   {
                                       case BoardUnitModel unit:
                                           _activeAbility.Ability.TargetUnit = unit;
                                           break;
                                       case Player player:
                                           _activeAbility.Ability.TargetPlayer = player;
                                           break;
                                       case null:
                                           break;
                                       default:
                                           throw new ArgumentOutOfRangeException (nameof (target), target, null);
                                   }

                                   _activeAbility.Ability.SelectedTargetAction (true);

                                   _tutorialManager.ReportActivityAction(Enumerators.TutorialActivityAction.CardWithAbilityPlayed, boardObject);

                                   _boardController.UpdateWholeBoard(() =>
                                   {
                                       onCompleteCallback?.Invoke(true);

                                       ResolveAllAbilitiesOnUnit(boardObject);

                                       abilityHelperAction?.ForceActionDone();
                                       abilityHelperAction = null;

                                       completeCallback?.Invoke();
                                   });
                               }
                               else if (isPlayer)
                               {
                                   BlockEndTurnButton = true;

                                   _activeAbility.Ability.ActivateSelectTarget(
                                       callback: () =>
                                       {
                                           _tutorialManager.ReportActivityAction(Enumerators.TutorialActivityAction.PlayerOverlordCardPlayed);
                                           GameClient.Get<IOverlordExperienceManager>().ReportExperienceAction(Common.Enumerators.ExperienceActionType.PlayCard, false);

                                           boardUnitModel.Owner.PlayerCardsController.RemoveCardFromHand(boardUnitModel, true);
                                           boardUnitModel.Owner.PlayerCardsController.AddCardToGraveyard(boardUnitModel);

                                           if (card.Model.Card.Prototype.Kind == Enumerators.CardKind.CREATURE)
                                           {
                                               boardUnitModel.Owner.PlayerCardsController.AddCardToBoard(boardUnitModel, (ItemPosition)card.FuturePositionOnBoard);

                                               InternalTools.DoActionDelayed(card.Dispose, 0.5f);

                                               ProceedWithCardToGraveyard(card);
                                           }
                                           else
                                           {
                                               boardUnitModel.Owner.PlayerCardsController.AddCardToGraveyard(boardUnitModel);

                                               handCard.GameObject.SetActive(true);

                                               InternalTools.DoActionDelayed(() =>
                                               {
                                                   _cardsController.RemoveCard(card);
                                                   boardUnitModel.Owner.PlayerCardsController.RemoveCardFromBoard(boardUnitModel);
                                               }, 0.5f);

                                               InternalTools.DoActionDelayed(() =>
                                               {
                                                   ProceedWithCardToGraveyard(card);
                                               }, 1.5f);
                                           }

                                           BlockEndTurnButton = false;

                                           action?.Invoke(card);

                                           onCompleteCallback?.Invoke(true);

                                           ResolveAllAbilitiesOnUnit(boardObject);

                                           abilityHelperAction?.ForceActionDone();
                                           abilityHelperAction = null;

                                           completeCallback?.Invoke();
                                       },
                                       failedCallback: () =>
                                       {
                                           instance.Abilities = prototype.Abilities.Select(a => new AbilityData(a)).ToList();

                                           card.Model.Card.Owner.CurrentGoo += card.Model.CurrentCost;

                                           handCard.GameObject.SetActive(true);
                                           handCard.ResetToHandAnimation();
                                           handCard.CheckStatusOfHighlight();

                                           boardUnitModel.SetPicture();

                                           boardUnitModel.Owner.PlayerCardsController.AddCardFromBoardToHand(card.Model);
                                           _battlegroundController.PlayerHandCards.Insert(ItemPosition.End, card);

                                           BoardUnitView boardUnitView = _battlegroundController.GetBoardUnitViewByModel<BoardUnitView>(boardUnitModel);
                                           if (boardUnitView != null)
                                           {
                                               _battlegroundController.UnregisterBoardUnitView(boardUnitModel.Owner, boardUnitView);
                                           }

                                           _battlegroundController.UpdatePositionOfCardsInPlayerHand();

                                           _playerController.IsCardSelected = false;


                                           onCompleteCallback?.Invoke(false);

                                           BlockEndTurnButton = false;

                                           ResolveAllAbilitiesOnUnit(boardObject);

                                           abilityHelperAction?.ForceActionDone();
                                           abilityHelperAction = null;

                                           completeCallback?.Invoke();
                                       });
                               }
                               else
                               {
                                   _tutorialManager.ReportActivityAction(Enumerators.TutorialActivityAction.EnemyOverlordCardPlayed);

                                   switch (target)
                                   {
                                       case BoardUnitModel unit:
                                           _activeAbility.Ability.TargetUnit = unit;
                                           break;
                                       case Player player:
                                           _activeAbility.Ability.TargetPlayer = player;
                                           break;
                                       case null:
                                           break;
                                       default:
                                           throw new ArgumentOutOfRangeException(nameof(target), target, null);
                                   }

                                   _activeAbility.Ability.SelectedTargetAction(true);

                                   _tutorialManager.ReportActivityAction(Enumerators.TutorialActivityAction.CardWithAbilityPlayed, boardObject);

                                   _boardController.UpdateWholeBoard(() =>
                                   {

                                       onCompleteCallback?.Invoke(true);

                                       ResolveAllAbilitiesOnUnit(boardObject);

                                       abilityHelperAction?.ForceActionDone();
                                       abilityHelperAction = null;

                                       completeCallback?.Invoke();
                                   });
                               }
                           }
                           else
                           {
                               CallPermanentAbilityAction(isPlayer, action, card, target, _activeAbility, kind);
                               onCompleteCallback?.Invoke(true);

                               ResolveAllAbilitiesOnUnit(boardObject);

                               abilityHelperAction?.ForceActionDone();
                               abilityHelperAction = null;

                               completeCallback?.Invoke();
                           }
                       }
                       else
                       {
                           CallPermanentAbilityAction(isPlayer, action, card, target, _activeAbility, kind);
                           onCompleteCallback?.Invoke(true);

                           ResolveAllAbilitiesOnUnit(boardObject);

                           abilityHelperAction?.ForceActionDone();
                           abilityHelperAction = null;

                           completeCallback?.Invoke();
                       }
                   };

                   AbilityData choosableAbility = instance.Abilities.FirstOrDefault(x => x.HasChoosableAbilities());

                   if (choosableAbility != null)
                   {
                       if (HasPredefinedChoosableAbility)
                       {
                           instance.Abilities[instance.Abilities.IndexOf(choosableAbility)] =
                                       choosableAbility.ChoosableAbilities[PredefinedChoosableAbilityId].AbilityData;
                           abilityEndAction.Invoke();

                           PredefinedChoosableAbilityId = -1;
                           HasPredefinedChoosableAbility = false;
                       }
                       else
                       {
                           if (isPlayer)
                           {
                               Action<AbilityData.ChoosableAbility> callback = null;

                               callback = (selectedChoosableAbility) =>
                               {
                                   boardUnitModel.SetPicture(string.Empty, selectedChoosableAbility.Attribute);

                                    instance.Abilities[instance.Abilities.IndexOf(choosableAbility)] = selectedChoosableAbility.AbilityData;
                                    abilityEndAction.Invoke();
                                    _cardsController.CardForAbilityChoosed -= callback;
                               };


                               abilityHelperAction = _actionsQueueController.AddNewActionInToQueue(null,
                                                                                   Enumerators.QueueActionType.AbilityUsageBlocker,
                                                                                   blockQueue: true);




                               _cardsController.CardForAbilityChoosed += callback;
                               _cardsController.CreateChoosableCardsForAbilities(choosableAbility.ChoosableAbilities, boardUnitModel);
                           }
                           else
                           {
                               // TODO: improve functionality for the AI
                               instance.Abilities[instance.Abilities.IndexOf(choosableAbility)] = choosableAbility.ChoosableAbilities[0].AbilityData;
                               abilityEndAction.Invoke();
                           }
                       }
                   }
                   else
                   {
                       abilityEndAction.Invoke();
                   }
               };
        }

        public bool CheckAbilityOnTarget(BoardUnitModel boardUnitModel, AbilityData ability = null)
        {
            if (boardUnitModel == null)
                return false;

            if(ability == null)
            {
                ability = boardUnitModel.Card.InstanceCard.Abilities.FirstOrDefault(IsAbilityCanActivateTargetAtStart);
            }

            if (ability == null || ability is default(AbilityData))
                return false;

            return ability.TargetCardType != Enumerators.CardType.UNDEFINED &&
                                   !HasSpecialUnitOnBoard(boardUnitModel, ability) ||
                                   ability.TargetUnitSpecialStatus != Enumerators.UnitSpecialStatus.NONE &&
                                   !HasSpecialUnitStatusOnBoard(boardUnitModel, ability) ||
                                   (ability.SubTrigger == Enumerators.AbilitySubTrigger.IfHasUnitsWithFactionInPlay &&
                                   ability.TargetFaction != Enumerators.Faction.Undefined &&
                                   !HasSpecialUnitFactionOnMainBoard(boardUnitModel, ability)) ||
                                   !CanTakeControlUnit(boardUnitModel, ability) ||
                                   (ability.SubTrigger == Enumerators.AbilitySubTrigger.CardCostMoreThanCostOfThis &&
                                   !HasUnitsOnBoardThatCostMoreThan(boardUnitModel, ability)) ||
                                   (ability.SubTrigger == Enumerators.AbilitySubTrigger.OverlordDefenseEqualOrLess &&
                                    !OverlordDefenseEqualOrLess(boardUnitModel, ability)) ||
                                    (ability.TargetUnitType != Enumerators.CardType.UNDEFINED &&
                                     !HasUnitsWithoutTargetUnitType(boardUnitModel, ability) &&
                                     ability.Activity == Enumerators.AbilityActivity.ACTIVE &&
                                     ability.Trigger == Enumerators.AbilityTrigger.ENTRY);
        }

        public void InvokeUseAbilityEvent(
            BoardUnitModel boardUnitModel,
            Enumerators.AbilityType abilityType,
            List<ParametrizedAbilityBoardObject> targets)
        {
            if (!CanHandleAbiityUseEvent(boardUnitModel))
                return;

            AbilityUsed?.Invoke(boardUnitModel, abilityType, targets);
        }

        public void BuffUnitByAbility(Enumerators.AbilityType ability,
                                    object target,
                                    Enumerators.CardKind cardKind,
                                    BoardUnitModel boardUnitModel,
                                    Player owner,
                                    bool callAbilityUsageEvent = false)
        {
            ActiveAbility activeAbility =
                CreateActiveAbility(GetAbilityDataByType(ability), cardKind, target, owner, boardUnitModel);
            activeAbility.Ability.IgnoreAbilityUsageEvent = true;
            activeAbility.Ability.Activate();
        }

        private bool CanHandleAbiityUseEvent(BoardUnitModel boardUnitModel)
        {
            if (!_gameplayManager.IsLocalPlayerTurn() || boardUnitModel == null || !boardUnitModel.Card.Owner.IsLocalPlayer)
                return false;

            return true;
        }

        public void CallAbilitiesInHand(BoardCardView boardCardView, BoardUnitModel boardUnitModel)
        {
            List<AbilityData> handAbilities =
                boardUnitModel.Card.InstanceCard.Abilities.FindAll(x => x.Trigger.Equals(Enumerators.AbilityTrigger.IN_HAND));
            foreach (AbilityData ability in handAbilities)
            {
                CreateActiveAbility(ability, boardUnitModel.Card.Prototype.Kind, boardCardView, boardUnitModel.Card.Owner, boardUnitModel)
                    .Ability
                    .Activate();
            }
        }

        private bool _PvPToggleFirstLastAbility = true;

        public void PlayAbilityFromEvent(Enumerators.AbilityType ability, BoardObject abilityCaller,
                                         List<ParametrizedAbilityBoardObject> targets, BoardUnitModel boardUnitModel, Player owner, Action completeCallback = null)
        {
            //FIXME Hard: This is an hack to fix Ghoul without changing the backend API.
            //We should absolutely change the backend API to support an index field.
            //That will tell us directly which one of multiple abilities with the same name we should use for a card.
            AbilityData abilityData;

            AbilityData subAbilitiesData = boardUnitModel.InstanceCard.Abilities.FirstOrDefault(x => x.ChoosableAbilities.Count > 0);

            if (subAbilitiesData != null)
            {
                abilityData = subAbilitiesData.ChoosableAbilities.Find(x => x.AbilityData.Ability == ability).AbilityData;
            }
            else
            {
                if (_PvPToggleFirstLastAbility)
                {
                    abilityData = boardUnitModel.InstanceCard.Abilities.FirstOrDefault(x => x.Ability == ability);
                    _PvPToggleFirstLastAbility = false;
                }
                else
                {
                    abilityData = boardUnitModel.InstanceCard.Abilities.LastOrDefault(x => x.Ability == ability);
                    _PvPToggleFirstLastAbility = true;
                }
            }

            if (abilityData == null || (abilityData is default(AbilityData)))
            {
                Log?.Warn($"abilityData: '{abilityData}' is null or default when trying to play it from event.");
                return;
            }

            ActiveAbility activeAbility = CreateActiveAbility(abilityData, boardUnitModel.Prototype.Kind, abilityCaller, owner, boardUnitModel);

            activeAbility.Ability.PredefinedTargets = targets;
            activeAbility.Ability.IsPVPAbility = true;

            if (targets.Count > 0 && activeAbility.Ability.AbilityActivity == Enumerators.AbilityActivity.ACTIVE)
            {
                switch (targets[0].BoardObject)
                {
                    case BoardUnitModel unit:
                        activeAbility.Ability.TargetUnit = unit;
                        break;
                    case Player player:
                        activeAbility.Ability.TargetPlayer = player;
                        break;
                    case null:
                        break;
                }

                Transform from = owner.AvatarObject.transform;

                if (abilityCaller is BoardUnitModel unitModel)
                {
                    BoardUnitView boardUnitView = _battlegroundController.GetBoardUnitViewByModel<BoardUnitView>(unitModel);
                    if (boardUnitView != null)
                        from = boardUnitView.Transform;
                }

                Action callback = () =>
                {
                    activeAbility.Ability.SelectedTargetAction(true);

                    _boardController.UpdateWholeBoard(null);

                    completeCallback?.Invoke();
                };

                if (from != null && targets[0].BoardObject != null)
                {
                    _boardArrowController.DoAutoTargetingArrowFromTo<OpponentBoardArrow>(from, targets[0].BoardObject, action: callback);
                }
                else
                {
                    callback();
                }
            }
            else
            {
                completeCallback?.Invoke();
            }

            activeAbility.Ability.Activate();
        }

        public void ActivateAbilitiesOnCard(BoardObject abilityCaller, BoardUnitModel boardUnitModel, Player owner)
        {
            foreach(AbilityData abilityData in boardUnitModel.InstanceCard.Abilities )
            {
                ActiveAbility activeAbility;
                if ((abilityData.Trigger != Enumerators.AbilityTrigger.ENTRY &&
                     abilityData.Activity == Enumerators.AbilityActivity.PASSIVE) || CanActivateAbility(abilityData))
                {
                    activeAbility = CreateActiveAbility(abilityData, boardUnitModel.Prototype.Kind, abilityCaller, owner, boardUnitModel);
                    activeAbility.Ability.Activate();
                }
            }
        }

        private bool CanActivateAbility(AbilityData abilityData)
        {
            if (abilityData.Trigger == Enumerators.AbilityTrigger.ENTRY &&
                abilityData.Activity == Enumerators.AbilityActivity.PASSIVE)
            {
                switch (abilityData.Ability)
                {
                    case Enumerators.AbilityType.BLOCK_TAKE_DAMAGE:
                    case Enumerators.AbilityType.EXTRA_GOO_IF_UNIT_IN_PLAY:
                    case Enumerators.AbilityType.SET_ATTACK_AVAILABILITY:
                        return true;
                    case Enumerators.AbilityType.CHANGE_STAT:
                        if (abilityData.SubTrigger == Enumerators.AbilitySubTrigger.NumberOfUnspentGoo)
                            return true;
                        break;
                }
            }

            return false;
        }

        private void CreateAbilityByType(Enumerators.CardKind cardKind, AbilityData abilityData, out AbilityBase ability, out AbilityViewBase abilityView)
        {
            ability = null;
            abilityView = null;

            switch (abilityData.Ability)
            {
                case Enumerators.AbilityType.HEAL:
                    ability = new HealTargetAbility(cardKind, abilityData);
                    abilityView = new HealTargetAbilityView((HealTargetAbility)ability);
                    break;
                case Enumerators.AbilityType.DAMAGE_TARGET:
                    ability = new DamageTargetAbility(cardKind, abilityData);
                    abilityView = new DamageTargetAbilityView((DamageTargetAbility)ability);
                    break;
                case Enumerators.AbilityType.DAMAGE_TARGET_ADJUSTMENTS:
                    ability = new DamageTargetAdjustmentsAbility(cardKind, abilityData);
                    abilityView = new DamageTargetAdjustmentsAbilityView((DamageTargetAdjustmentsAbility)ability);
                    break;
                case Enumerators.AbilityType.ADD_GOO_VIAL:
                    ability = new AddGooVialsAbility(cardKind, abilityData);
                    abilityView = new AddGooVialsAbilityView((AddGooVialsAbility)ability);
                    break;
                case Enumerators.AbilityType.MODIFICATOR_STATS:
                    ability = new ModificateStatAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.MASSIVE_DAMAGE:
                    ability = new MassiveDamageAbility(cardKind, abilityData);
                    abilityView = new MassiveDamageAbilityView((MassiveDamageAbility)ability);
                    break;
                case Enumerators.AbilityType.CHANGE_STAT:
                    ability = new ChangeStatAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.STUN:
                    ability = new StunAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.STUN_OR_DAMAGE_ADJUSTMENTS:
                    ability = new StunOrDamageAdjustmentsAbility(cardKind, abilityData);
                    abilityView = new StunOrDamageAdjustmentsAbilityView((StunOrDamageAdjustmentsAbility)ability);
                    break;
                case Enumerators.AbilityType.SUMMON:
                    ability = new SummonsAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.CARD_RETURN:
                    ability = new ReturnToHandAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.CHANGE_STAT_OF_CREATURES_BY_TYPE:
                    ability = new ChangeUnitsOfTypeStatAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.ATTACK_NUMBER_OF_TIMES_PER_TURN:
                    ability = new AttackNumberOfTimesPerTurnAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.DRAW_CARD:
                    ability = new DrawCardAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.DEVOUR_ZOMBIES_AND_COMBINE_STATS:
                    ability = new DevourZombiesAndCombineStatsAbility(cardKind, abilityData);
                    abilityView = new DevourZombiesAndCombineStatsAbilityView((DevourZombiesAndCombineStatsAbility)ability);
                    break;
                case Enumerators.AbilityType.DESTROY_UNIT_BY_TYPE:
                    ability = new DestroyUnitByTypeAbility(cardKind, abilityData);
                    abilityView = new DestroyUnitByTypeAbilityView((DestroyUnitByTypeAbility)ability);
                    break;
                case Enumerators.AbilityType.LOWER_COST_OF_CARD_IN_HAND:
                    ability = new LowerCostOfCardInHandAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.OVERFLOW_GOO:
                    ability = new OverflowGooAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.LOSE_GOO:
                    ability = new LoseGooAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.DISABLE_NEXT_TURN_GOO:
                    ability = new DisableNextTurnGooAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.RAGE:
                    ability = new RageAbility(cardKind, abilityData);
                    abilityView = new RageAbilityView((RageAbility)ability);
                    break;
                case Enumerators.AbilityType.FREEZE_UNITS:
                    ability = new FreezeUnitsAbility(cardKind, abilityData);
                    abilityView = new FreezeUnitsAbilityView((FreezeUnitsAbility)ability);
                    break;
                case Enumerators.AbilityType.TAKE_DAMAGE_RANDOM_ENEMY:
                    ability = new TakeDamageRandomEnemyAbility(cardKind, abilityData);
                    abilityView = new TakeDamageRandomEnemyAbilityView((TakeDamageRandomEnemyAbility)ability);
                    break;
                case Enumerators.AbilityType.TAKE_CONTROL_ENEMY_UNIT:
                    ability = new TakeControlEnemyUnitAbility(cardKind, abilityData);
                    abilityView = new TakeControlEnemyUnitAbilityView((TakeControlEnemyUnitAbility)ability);
                    break;
                case Enumerators.AbilityType.GUARD:
                    ability = new ShieldAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.DESTROY_FROZEN_UNIT:
                    ability = new DestroyFrozenZombieAbility(cardKind, abilityData);
                    abilityView = new DestroyFrozenZombieAbilityView((DestroyFrozenZombieAbility)ability);
                    break;
                case Enumerators.AbilityType.USE_ALL_GOO_TO_INCREASE_STATS:
                    ability = new UseAllGooToIncreaseStatsAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.FIRST_UNIT_IN_PLAY:
                    ability = new FirstUnitInPlayAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.ALLY_UNITS_OF_TYPE_IN_PLAY_GET_STATS:
                    ability = new AllyUnitsOfTypeInPlayGetStatsAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.DAMAGE_ENEMY_UNITS_AND_FREEZE_THEM:
                    ability = new DamageEnemyUnitsAndFreezeThemAbility(cardKind, abilityData);
                    abilityView = new DamageEnemyUnitsAndFreezeThemAbilityView((DamageEnemyUnitsAndFreezeThemAbility)ability);
                    break;
                case Enumerators.AbilityType.RETURN_UNITS_ON_BOARD_TO_OWNERS_DECKS:
                    ability = new ReturnUnitsOnBoardToOwnersDecksAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.TAKE_UNIT_TYPE_TO_ADJACENT_ALLY_UNITS:
                    ability = new TakeUnitTypeToAdjacentAllyUnitsAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.ENEMY_THAT_ATTACKS_BECOME_FROZEN:
                    ability = new EnemyThatAttacksBecomeFrozenAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.TAKE_UNIT_TYPE_TO_ALLY_UNIT:
                    ability = new TakeUnitTypeToAllyUnitAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.REVIVE_DIED_UNITS_OF_TYPE_FROM_MATCH:
                    ability = new ReviveDiedUnitsOfTypeFromMatchAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.CHANGE_STAT_UNTILL_END_OF_TURN:
                    ability = new ChangeStatUntillEndOfTurnAbility(cardKind, abilityData);
                    abilityView = new ChangeStatUntillEndOfTurnAbilityView((ChangeStatUntillEndOfTurnAbility)ability);
                    break;
                case Enumerators.AbilityType.ATTACK_OVERLORD:
                    ability = new AttackOverlordAbility(cardKind, abilityData);
                    abilityView = new AttackOverlordAbilityView((AttackOverlordAbility)ability);
                    break;
                case Enumerators.AbilityType.ADJACENT_UNITS_GET_HEAVY:
                    ability = new AdjacentUnitsGetHeavyAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.FREEZE_NUMBER_OF_RANDOM_ALLY:
                    ability = new FreezeNumberOfRandomAllyAbility(cardKind, abilityData);
                    abilityView = new FreezeNumberOfRandomAllyAbilityView((FreezeNumberOfRandomAllyAbility)ability);
                    break;
                case Enumerators.AbilityType.DEAL_DAMAGE_TO_THIS_AND_ADJACENT_UNITS:
                    ability = new DealDamageToThisAndAdjacentUnitsAbility(cardKind, abilityData);
                    abilityView = new DealDamageToThisAndAdjacentUnitsAbilityView((DealDamageToThisAndAdjacentUnitsAbility)ability);
                    break;
                case Enumerators.AbilityType.SWING:
                    ability = new SwingAbility(cardKind, abilityData);
                    abilityView = new SwingAbilityView((SwingAbility)ability);
                    break;
                case Enumerators.AbilityType.TAKE_DEFENSE_IF_OVERLORD_HAS_LESS_DEFENSE_THAN:
                    ability = new TakeDefenseIfOverlordHasLessDefenseThanAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.ADDITIONAL_DAMAGE_TO_HEAVY_IN_ATTACK:
                    ability = new AdditionalDamageToHeavyInAttackAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.GAIN_NUMBER_OF_LIFE_FOR_EACH_DAMAGE_THIS_DEALS:
                    ability = new GainNumberOfLifeForEachDamageThisDealsAbility(cardKind, abilityData);
                    abilityView = new GainNumberOfLifeForEachDamageThisDealsAbilityView((GainNumberOfLifeForEachDamageThisDealsAbility)ability);
                    break;
                case Enumerators.AbilityType.UNIT_WEAPON:
                    ability = new UnitWeaponAbility(cardKind, abilityData);
                    abilityView = new UnitWeaponAbilityView((UnitWeaponAbility)ability);
                    break;
                case Enumerators.AbilityType.TAKE_DAMAGE_AT_END_OF_TURN_TO_THIS:
                    ability = new TakeDamageAtEndOfTurnToThis(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.DELAYED_LOSE_HEAVY_GAIN_ATTACK:
                    ability = new DelayedLoseHeavyGainAttackAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.DELAYED_GAIN_ATTACK:
                    ability = new DelayedGainAttackAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.REANIMATE_UNIT:
                    ability = new ReanimateAbility(cardKind, abilityData);
                    abilityView = new ReanimateAbilityView((ReanimateAbility)ability);
                    break;
                case Enumerators.AbilityType.PRIORITY_ATTACK:
                    ability = new PriorityAttackAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.DESTROY_TARGET_UNIT_AFTER_ATTACK:
                    ability = new DestroyTargetUnitAfterAttackAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.COSTS_LESS_IF_CARD_TYPE_IN_HAND:
                    ability = new CostsLessIfCardTypeInHandAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.RETURN_UNITS_ON_BOARD_TO_OWNERS_HANDS:
                    ability = new ReturnUnitsOnBoardToOwnersHandsAbility(cardKind, abilityData);
                    abilityView = new ReturnUnitsOnBoardToOwnersHandsAbilityView((ReturnUnitsOnBoardToOwnersHandsAbility)ability);
                    break;
                case Enumerators.AbilityType.REPLACE_UNITS_WITH_TYPE_ON_STRONGER_ONES:
                    ability = new ReplaceUnitsWithTypeOnStrongerOnesAbility(cardKind, abilityData);
                    abilityView = new ReplaceUnitsWithTypeOnStrongerOnesAbilityView((ReplaceUnitsWithTypeOnStrongerOnesAbility)ability);
                    break;
                case Enumerators.AbilityType.RESTORE_DEF_RANDOMLY_SPLIT:
                    ability = new RestoreDefRandomlySplitAbility(cardKind, abilityData);
                    abilityView = new RestoreDefRandomlySplitAbilityView((RestoreDefRandomlySplitAbility)ability);
                    break;
                case Enumerators.AbilityType.ADJACENT_UNITS_GET_GUARD:
                    ability = new AdjacentUnitsGetGuardAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.DAMAGE_AND_DISTRACT_TARGET:
                    ability = new DamageAndDistractTargetAbility(cardKind, abilityData);
                    abilityView = new DamageAndDistractTargetAbilityView((DamageAndDistractTargetAbility)ability);
                    break;
                case Enumerators.AbilityType.DAMAGE_OVERLORD_ON_COUNT_ITEMS_PLAYED:
                    ability = new DamageTargetOnCountItemsPlayedAbility(cardKind, abilityData);
                    abilityView = new DamageTargetOnCountItemsPlayedAbilityView((DamageTargetOnCountItemsPlayedAbility)ability);
                    break;
                case Enumerators.AbilityType.DISTRACT:
                    ability = new DistractAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.ADJACENT_UNITS_GET_STAT:
                    ability = new AdjacentUnitsGetStatAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.DRAW_CARD_IF_DAMAGED_ZOMBIE_IN_PLAY:
                    ability = new DrawCardIfDamagedUnitInPlayAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.PUT_RANDOM_UNIT_FROM_DECK_ON_BOARD:
                    ability = new PutRandomUnitFromDeckOnBoardAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.DAMAGE_TARGET_FREEZE_IT_IF_SURVIVES:
                    ability = new DamageTargetFreezeItIfSurvivesAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.DESTROY_UNIT_BY_COST:
                    ability = new DestroyUnitByCostAbility(cardKind, abilityData);
                    abilityView = new DestroyUnitByCostAbilityView((DestroyUnitByCostAbility)ability);
                    break;
                case Enumerators.AbilityType.DELAYED_PLACE_COPIES_IN_PLAY_DESTROY_UNIT:
                    ability = new DelayedPlaceCopiesInPlayDestroyUnitAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.EXTRA_GOO_IF_UNIT_IN_PLAY:
                    ability = new ExtraGooIfUnitInPlayAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.SUMMON_UNIT_FROM_HAND:
                    ability = new SummonFromHandAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.TAKE_STAT_IF_OVERLORD_HAS_LESS_DEFENSE_THAN:
                    ability = new TakeStatIfOverlordHasLessDefenseThanAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.SHUFFLE_THIS_CARD_TO_DECK:
                    ability = new ShuffleCardToDeckAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.TAKE_DEFENSE_TO_OVERLORD_WITH_DEFENSE:
                    ability = new TakeDefenseToOverlordWithDefenseAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.TAKE_SWING_TO_UNITS:
                    ability = new TakeSwingToUnitsAbility(cardKind, abilityData);
                    abilityView = new TakeSwingToUnitsAbilityView((TakeSwingToUnitsAbility)ability);
                    break;
                case Enumerators.AbilityType.DESTROY_UNITS:
                    ability = new DestroyUnitsAbility(cardKind, abilityData);
                    abilityView = new DestroyUnitsAbilityView((DestroyUnitsAbility)ability);
                    break;
                case Enumerators.AbilityType.DEAL_DAMAGE_TO_UNIT_AND_SWING:
                    ability = new DealDamageToUnitAndSwing(cardKind, abilityData);
                    abilityView = new DealDamageToUnitAndSwingView((DealDamageToUnitAndSwing)ability);
                    break;
                case Enumerators.AbilityType.SET_ATTACK_AVAILABILITY:
                    ability = new SetAttackAvailabilityAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.CHOOSABLE_ABILITIES:
                    ability = new ChoosableAbilitiesAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.COSTS_LESS_IF_CARD_TYPE_IN_PLAY:
                    ability = new CostsLessIfCardTypeInPlayAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.GAIN_GOO:
                    ability = new GainGooAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.BLITZ:
                    ability = new BlitzAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.DRAW_CARD_BY_FACTION:
                    ability = new DrawCardByFactionAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.DESTROY_TARGET_UNIT:
                    ability = new DestroyTargetUnitAbility(cardKind, abilityData);
                    abilityView = new DestroyTargetUnitAbilityView((DestroyTargetUnitAbility)ability);
                    break;
                case Enumerators.AbilityType.AGILE:
                    ability = new AgileAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.CHANGE_STAT_OF_CARDS_IN_HAND:
                    ability = new ChangeStatsOfCardsInHandAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.GIVE_BUFFS_TO_UNIT:
                    ability = new GiveBuffsToUnitAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.DISCARD_CARD_FROM_HAND:
                    ability = new DiscardCardFromHandAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.GET_GOO_THIS_TURN:
                    ability = new GetGooThisTurnAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.COSTS_LESS:
                    ability = new CostsLessAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.FILL_BOARD_BY_UNITS:
                    ability = new FillBoardByUnitsAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.DEAL_DAMAGE_TO_TARGET_THAT_ATTACK_THIS:
                    ability = new DealDamageToTargetThatAttackThisAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.CHANGE_COST:
                    ability = new ChangeCostAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.GAIN_STATS_OF_ADJACENT_UNITS:
                    ability = new GainStatsOfAdjacentUnitsAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.TAKE_UNIT_TYPE_TO_TARGET_UNIT:
                    ability = new TakeUnitTypeToTargetUnitAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.DISTRACT_AND_CHANGE_STAT:
                    ability = new DistractAndChangeStatAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.DAMAGE_AND_DISTRACT:
                    ability = new DamageAndDistractAbility(cardKind, abilityData);
                    abilityView = new DamageAndDistractAbilityView((DamageAndDistractAbility)ability);
                    break;
                case Enumerators.AbilityType.PUT_UNITS_FROM_DISCARD_INTO_PLAY:
                    ability = new PutUnitsFromDiscardIntoPlayAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.PUT_UNITS_FRON_LIBRARY_INTO_PLAY:
                    ability = new PutUnitsFromLibraryIntoPlayAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.BLOCK_TAKE_DAMAGE:
                    ability = new BlockTakeDamageAbility(cardKind, abilityData);
                    abilityView = new BlockTakeDamageAbilityView((BlockTakeDamageAbility)ability);
                    break;
                case Enumerators.AbilityType.CHANGE_STAT_THIS_TURN:
                    ability = new ChangeStatThisTurnAbility(cardKind, abilityData);
                    break; 
                default:
                    throw new ArgumentOutOfRangeException(nameof(abilityData.Ability), abilityData.Ability, null);
            }
        }

        public void ResolveAllAbilitiesOnUnit(BoardObject boardObject, bool status = true, bool inputDragStatus = true)
        {
            if (boardObject is BoardUnitModel unit)
            {
                unit.IsAllAbilitiesResolvedAtStart = status;
            }

            _gameplayManager.CanDoDragActions = inputDragStatus;
        }

        private void CallPermanentAbilityAction(
            bool isPlayer,
            Action<BoardCardView> action,
            BoardCardView card,
            BoardObject target,
            ActiveAbility activeAbility,
            Enumerators.CardKind kind)
        {
            if (isPlayer)
            {
                _tutorialManager.ReportActivityAction(Enumerators.TutorialActivityAction.PlayerOverlordCardPlayed);

                GameClient.Get<IOverlordExperienceManager>().ReportExperienceAction(Common.Enumerators.ExperienceActionType.PlayCard, false);

                card.Model.Card.Owner.PlayerCardsController.RemoveCardFromHand(card.Model);

                if (card.Model.Card.Prototype.Kind == Enumerators.CardKind.CREATURE)
                {
                    card.Model.Card.Owner.PlayerCardsController.AddCardToBoard(card.Model, (ItemPosition)card.FuturePositionOnBoard);

                    InternalTools.DoActionDelayed(card.Dispose, 0.5f);

                    ProceedWithCardToGraveyard(card);
                }
                else
                {
                    card.Model.Card.Owner.PlayerCardsController.AddCardToGraveyard(card.Model);

                    card.GameObject.SetActive(true);

                    InternalTools.DoActionDelayed(() =>
                    {
                        _cardsController.RemoveCard(card);
                    }, 0.5f);

                    InternalTools.DoActionDelayed(() =>
                    {
                        ProceedWithCardToGraveyard(card);
                    }, 1.5f);
                }

                action?.Invoke(card);
            }
            else
            {
                _tutorialManager.ReportActivityAction(Enumerators.TutorialActivityAction.EnemyOverlordCardPlayed);

                if (activeAbility == null)
                    return;

                switch (target)
                {
                    case BoardUnitModel unit:
                        activeAbility.Ability.TargetUnit = unit;
                        break;
                    case Player player:
                        activeAbility.Ability.TargetPlayer = player;
                        break;
                    case null:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(target), target, null);
                }

                activeAbility.Ability.SelectedTargetAction(true);
            }

            _boardController.UpdateWholeBoard(null);
        }

        private void ProceedWithCardToGraveyard(BoardCardView card)
        {
            card.Model.Card.Owner.GraveyardCardsCount++;

            _actionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
            {
                ActionType = Enumerators.ActionType.PlayCardFromHand,
                Caller = card,
                TargetEffects = new List<PastActionsPopup.TargetEffectParam>(),
                CheckForCardOwner = false,
                Model = card.Model
            });
        }

        public static AbilityData GetAbilityDataByType(Enumerators.AbilityType ability)
        {
            AbilityData abilityData = null;

            // FIXME: why is this hardcoded? should probably be in a separate JSON file
            switch (ability)
            {
                case Enumerators.AbilityType.REANIMATE_UNIT:
                    abilityData = new AbilityData(
                        Enumerators.AbilityType.REANIMATE_UNIT,
                        Enumerators.AbilityActivity.PASSIVE,
                        Enumerators.AbilityTrigger.DEATH,
                        null,
                        default(Enumerators.Stat),
                        default(Enumerators.Faction),
                        default(Enumerators.AbilityEffect),
                        default(Enumerators.AttackRestriction),
                        default(Enumerators.CardType),
                        default(Enumerators.UnitSpecialStatus),
                        default(Enumerators.CardType),
                        0,
                        0,
                        0,
                        "",
                        0,
                        0,
                        0,
                        new List<AbilityData.VisualEffectInfo>()
                        {
                            new AbilityData.VisualEffectInfo(Enumerators.VisualEffectType.Impact, "Prefabs/VFX/ReanimateVFX")
                        },
                        Enumerators.GameMechanicDescription.Reanimate,
                        default(Enumerators.Faction),
                        default(Enumerators.AbilitySubTrigger),
                        null,
                        0,
                        0,
                        default(Enumerators.CardKind),
                        null
                        );
                    break;
                case Enumerators.AbilityType.DESTROY_TARGET_UNIT_AFTER_ATTACK:
                    abilityData = new AbilityData(
                        Enumerators.AbilityType.DESTROY_TARGET_UNIT_AFTER_ATTACK,
                        Enumerators.AbilityActivity.PASSIVE,
                        Enumerators.AbilityTrigger.ATTACK,
                        null,
                        default(Enumerators.Stat),
                        default(Enumerators.Faction),
                        default(Enumerators.AbilityEffect),
                        default(Enumerators.AttackRestriction),
                        default(Enumerators.CardType),
                        default(Enumerators.UnitSpecialStatus),
                        default(Enumerators.CardType),
                        0,
                        0,
                        0,
                        "",
                        0,
                        0,
                        0,
                        null,
                        Enumerators.GameMechanicDescription.Destroy,
                        default(Enumerators.Faction),
                        default(Enumerators.AbilitySubTrigger),
                        null,
                        0,
                        0,
                        default(Enumerators.CardKind),
                        null
                    );
                    break;
            }

            return abilityData;
        }

        public class ActiveAbility
        {
            public ulong Id;

            public AbilityBase Ability;

            public AbilityViewBase AbilityView;
        }
    }
}
