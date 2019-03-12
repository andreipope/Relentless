using System;
using System.Collections.Generic;
using System.Linq;
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
        public delegate void AbilityUsedEventHandler(
            WorkingCard card,
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
                foreach (ActiveAbility item in _activeAbilities)
                {
                    item.Ability.Update();
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
            IReadOnlyCard cardOwner,
            WorkingCard workingCard)
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
                activeAbility.Ability.CardOwnerOfAbility = cardOwner;
                activeAbility.Ability.MainWorkingCard = workingCard;

                switch(boardObject)
                {
                    case BoardCardView card:
                        activeAbility.Ability.boardCardView = card;
                        break;
                    case BoardUnitModel model:
                        activeAbility.Ability.AbilityUnitOwner = model;
                        break;
                    case BoardSpell spell:
                        activeAbility.Ability.BoardSpell = spell;
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
            if (ability.AbilityTargetTypes.Count > 0)
            {
                return true;
            }

            return false;
        }

        public bool IsAbilityActive(AbilityData ability)
        {
            if (ability.ActivityType == Enumerators.AbilityActivityType.ACTIVE)
            {
                return true;
            }

            return false;
        }

        public bool IsAbilityCallsAtStart(AbilityData ability)
        {
            if (ability.CallType == Enumerators.AbilityCallType.ENTRY)
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
                foreach (Enumerators.AbilityTargetType item in ability.AbilityTargetTypes)
                {
                    switch (item)
                    {
                        case Enumerators.AbilityTargetType.OPPONENT_CARD:
                            if (opponent.BoardCards.Count > 0)
                            {
                                available = true;
                            }
                            break;
                        case Enumerators.AbilityTargetType.PLAYER_CARD:
                            if (localPlayer.BoardCards.Count > 1 || kind == Enumerators.CardKind.SPELL)
                            {
                                available = true;
                            }
                            break;
                        case Enumerators.AbilityTargetType.PLAYER:
                        case Enumerators.AbilityTargetType.OPPONENT:
                        case Enumerators.AbilityTargetType.ALL:
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
                    x.AbilityType == Enumerators.AbilityType.MODIFICATOR_STATS);

                for (int i = 0; i < abilities.Count; i++)
                {
                    if (attackedCard.CardSetType == abilities[i].AbilitySetType &&
                        abilities[i].CallType == Enumerators.AbilityCallType.PERMANENT)
                    {
                        value += abilities[i].Value;
                    }
                }
            }

            abilities = attackerCard.Abilities.FindAll(x =>
                x.AbilityType == Enumerators.AbilityType.ADDITIONAL_DAMAGE_TO_HEAVY_IN_ATTACK);

            for (int i = 0; i < abilities.Count; i++)
            {
                if (attacked.IsHeavyUnit)
                {
                    value += abilities[i].Value;
                }
            }

            return value;
        }

        public bool HasSpecialUnitOnBoard(WorkingCard workingCard, AbilityData ability)
        {
            if (ability.AbilityTargetTypes.Count == 0)
            {
                return false;
            }

            Player opponent = workingCard.Owner.Equals(_gameplayManager.CurrentPlayer) ?
                _gameplayManager.OpponentPlayer :
                _gameplayManager.CurrentPlayer;
            Player player = workingCard.Owner;

            foreach (Enumerators.AbilityTargetType target in ability.AbilityTargetTypes)
            {
                switch (target)
                {
                    case Enumerators.AbilityTargetType.PLAYER_CARD:
                    {
                        IReadOnlyList<BoardUnitView> units =
                            player.BoardCards.FindAll(x =>
                                x.Model.InitialUnitType == ability.TargetCardType &&
                                x.Model.UnitStatus == ability.TargetUnitStatusType);
                        if (units.Count > 0)
                            return true;

                        break;
                    }
                    case Enumerators.AbilityTargetType.OPPONENT_CARD:
                    {
                        IReadOnlyList<BoardUnitView> units =
                            opponent.BoardCards.FindAll(x =>
                                x.Model.InitialUnitType == ability.TargetCardType &&
                                x.Model.UnitStatus == ability.TargetUnitStatusType);
                        if (units.Count > 0)
                            return true;

                        break;
                    }
                    default:
                        throw new ArgumentOutOfRangeException(nameof(target), target, null);
                }
            }

            return false;
        }

        public bool HasSpecialUnitStatusOnBoard(WorkingCard workingCard, AbilityData ability)
        {
            if (ability.AbilityTargetTypes.Count == 0)
            {
                return false;
            }

            Player opponent = workingCard.Owner.Equals(_gameplayManager.CurrentPlayer) ?
                _gameplayManager.OpponentPlayer :
                _gameplayManager.CurrentPlayer;
            Player player = workingCard.Owner;

            foreach (Enumerators.AbilityTargetType target in ability.AbilityTargetTypes)
            {
                switch (target)
                {
                    case Enumerators.AbilityTargetType.PLAYER_CARD:
                    {
                        IReadOnlyList<BoardUnitView> units =
                            player.BoardCards.FindAll(x => x.Model.UnitStatus == ability.TargetUnitStatusType);

                        if (units.Count > 0)
                            return true;

                        break;
                    }
                    case Enumerators.AbilityTargetType.OPPONENT_CARD:
                    {
                        IReadOnlyList<BoardUnitView> units =
                            opponent.BoardCards.FindAll(x => x.Model.UnitStatus == ability.TargetUnitStatusType);

                        if (units.Count > 0)
                            return true;

                        break;
                    }
                    default:
                        throw new ArgumentOutOfRangeException(nameof(target), target, null);
                }
            }

            return false;
        }

        public bool HasSpecialUnitFactionOnMainBoard(WorkingCard workingCard, AbilityData ability)
        {
            if (workingCard.Owner.BoardCards.
                FindAll(x => x.Model.Card.Prototype.CardSetType == ability.TargetSetType && x.Model.Card != workingCard).Count > 0)
                return true;

            return false;
        }

        public bool CanTakeControlUnit(WorkingCard workingCard, AbilityData ability)
        {
            if (ability.AbilityType == Enumerators.AbilityType.TAKE_CONTROL_ENEMY_UNIT &&
                ability.CallType == Enumerators.AbilityCallType.ENTRY &&
                ability.ActivityType == Enumerators.AbilityActivityType.ACTIVE &&
                workingCard.Owner.BoardCards.Count >= workingCard.Owner.MaxCardsInPlay)
                return false;

            return true;
        }

        private ActiveAbility _activeAbility;
        public ActiveAbility CurrentActiveAbility
        {
            get { return _activeAbility; }
        }

        public void CallAbility(
            IReadOnlyCard prototype,
            BoardCardView card,
            WorkingCard workingCard,
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

            GameplayQueueAction<object> abilityHelperAction = null;

            actionInQueue.Action = (parameter, completeCallback) =>
               {
                   ResolveAllAbilitiesOnUnit(boardObject, false);

                   Action abilityEndAction = () =>
                   {
                       bool canUseAbility = false;
                       _activeAbility = null;
                       foreach (AbilityData item in prototype.Abilities)
                       {
                           _activeAbility = CreateActiveAbility(item, kind, boardObject, workingCard.Owner, prototype, workingCard);

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
                           AbilityData ability = prototype.Abilities.First(IsAbilityCanActivateTargetAtStart);

                           if (ability.TargetCardType != Enumerators.CardType.UNDEFINED &&
                               !HasSpecialUnitOnBoard(workingCard, ability) ||
                               ability.TargetUnitStatusType != Enumerators.UnitStatusType.NONE &&
                               !HasSpecialUnitStatusOnBoard(workingCard, ability) ||
                               (ability.AbilitySubTrigger == Enumerators.AbilitySubTrigger.IfHasUnitsWithFactionInPlay &&
                                ability.TargetSetType != Enumerators.SetType.NONE &&
                               !HasSpecialUnitFactionOnMainBoard(workingCard, ability)) ||
                               !CanTakeControlUnit(workingCard, ability))

                           {
                               CallPermanentAbilityAction(isPlayer, action, card, target, _activeAbility, kind);

                               onCompleteCallback?.Invoke(true);

                               ResolveAllAbilitiesOnUnit(boardObject);

                               abilityHelperAction?.ForceActionDone();
                               abilityHelperAction = null;

                               completeCallback?.Invoke();
                               return;
                           }

                           if (CheckActivateAvailability(kind, ability, workingCard.Owner))
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
                                           GameClient.Get<IOverlordExperienceManager>().ReportExperienceAction(card.BoardUnitModel.Card.Owner.SelfHero, Common.Enumerators.ExperienceActionType.PlayCard);
  
                                           workingCard.Owner.RemoveCardFromHand(workingCard, true);
                                           workingCard.Owner.AddCardToBoard(workingCard, (ItemPosition) card.FuturePositionOnBoard);

                                           if (card.BoardUnitModel.Card.Prototype.CardKind == Enumerators.CardKind.CREATURE)
                                           {
                                               InternalTools.DoActionDelayed(() =>
                                               {
                                                   Object.Destroy(card.GameObject);
                                               }, 0.5f);

                                               ProceedWithCardToGraveyard(card);
                                           }
                                           else
                                           {
                                               workingCard.Owner.AddCardToGraveyard(workingCard);

                                               handCard.GameObject.SetActive(true);

                                               InternalTools.DoActionDelayed(() =>
                                               {
                                                   _cardsController.RemoveCard(new object[] { card });
                                                   workingCard.Owner.RemoveCardFromBoard(workingCard);
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
                                           // HACK FIXME: why do we need to update library card instead of modifying a copy?
                                           ((ICard) prototype).ForceUpdateAbilities(prototype.InitialAbilities);

                                           card.BoardUnitModel.Card.Owner.CurrentGoo += card.BoardUnitModel.Card.InstanceCard.Cost;

                                           handCard.GameObject.SetActive(true);
                                           handCard.ResetToHandAnimation();
                                           handCard.CheckStatusOfHighlight();

                                           workingCard.Owner.CardsInHand.Insert(ItemPosition.End, card.BoardUnitModel.Card);
                                           _battlegroundController.PlayerHandCards.Insert(ItemPosition.End, card);
                                           workingCard.Owner.CardsOnBoard.Remove(card.BoardUnitModel.Card);
                                           BoardUnitView boardCardUnitView = workingCard.Owner.BoardCards.FirstOrDefault(boardCardView =>
                                               boardCardView.Model.Card.InstanceId == card.BoardUnitModel.Card.InstanceId);
                                           if (boardCardUnitView != null)
                                           {
                                               workingCard.Owner.BoardCards.Remove(boardCardUnitView);
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

                   AbilityData choosableAbility = prototype.Abilities.FirstOrDefault(x => x.HasChoosableAbilities());

                   if (choosableAbility != null && !(choosableAbility is default(AbilityData)))
                   {
                       if (HasPredefinedChoosableAbility)
                       {
                           prototype.Abilities[prototype.Abilities.IndexOf(choosableAbility)] =
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

                               callback = (x) =>
                               {
                                    prototype.Abilities[prototype.Abilities.IndexOf(choosableAbility)] = x.AbilityData;
                                    abilityEndAction.Invoke();
                                    _cardsController.CardForAbilityChoosed -= callback;
                               };


                               abilityHelperAction = _actionsQueueController.AddNewActionInToQueue(null,
                                                                                   Enumerators.QueueActionType.AbilityUsageBlocker,
                                                                                   blockQueue: true);


                              

                               _cardsController.CardForAbilityChoosed += callback;
                               _cardsController.CreateChoosableCardsForAbilities(choosableAbility.ChoosableAbilities, workingCard);
                           }
                           else
                           {
                               // TODO: improve functionality for the AI
                               prototype.Abilities[prototype.Abilities.IndexOf(choosableAbility)] = choosableAbility.ChoosableAbilities[0].AbilityData;
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

        public void InvokeUseAbilityEvent(
            WorkingCard card,
            Enumerators.AbilityType abilityType,
            List<ParametrizedAbilityBoardObject> targets)
        {
            if (!CanHandleAbiityUseEvent(card))
                return;

            AbilityUsed?.Invoke(card, abilityType, targets);
        }

        public void BuffUnitByAbility(Enumerators.AbilityType ability, object target, Enumerators.CardKind cardKind, IReadOnlyCard card, Player owner)
        {
            ActiveAbility activeAbility =
                CreateActiveAbility(GetAbilityDataByType(ability), cardKind, target, owner, card, null);
            activeAbility.Ability.Activate();
        }

        private bool CanHandleAbiityUseEvent(WorkingCard card)
        {
            if (!_gameplayManager.IsLocalPlayerTurn() || card == null || !card.Owner.IsLocalPlayer)
                return false;

            return true;
        }

        public void CallAbilitiesInHand(BoardCardView boardCardView, WorkingCard card)
        {
            List<AbilityData> handAbilities =
                card.Prototype.Abilities.FindAll(x => x.CallType.Equals(Enumerators.AbilityCallType.IN_HAND));
            foreach (AbilityData ability in handAbilities)
            {
                CreateActiveAbility(ability, card.Prototype.CardKind, boardCardView, card.Owner, card.Prototype, card)
                    .Ability
                    .Activate();
            }
        }

        private bool _PvPToggleFirstLastAbility = true;

        public void PlayAbilityFromEvent(Enumerators.AbilityType ability, BoardObject abilityCaller,
                                         List<ParametrizedAbilityBoardObject> targets, WorkingCard card, Player owner)
        {
            //FIXME Hard: This is an hack to fix Ghoul without changing the backend API.
            //We should absolutely change the backend API to support an index field.
            //That will tell us directly which one of multiple abilities with the same name we should use for a card.
            AbilityData abilityData;

            AbilityData subAbilitiesData = card.Prototype.Abilities.FirstOrDefault(x => x.ChoosableAbilities.Count > 0);

            if (subAbilitiesData != null)
            {
                abilityData = subAbilitiesData.ChoosableAbilities.Find(x => x.AbilityData.AbilityType == ability).AbilityData;
            }
            else
            {
                if (_PvPToggleFirstLastAbility)
                {
                    abilityData = card.Prototype.Abilities.First(x => x.AbilityType == ability);
                    _PvPToggleFirstLastAbility = false;
                }
                else
                {
                    abilityData = card.Prototype.Abilities.Last(x => x.AbilityType == ability);
                    _PvPToggleFirstLastAbility = true;
                }
            }

            ActiveAbility activeAbility = CreateActiveAbility(abilityData,
                                                               card.Prototype.CardKind, abilityCaller, owner, card.Prototype, card);

            activeAbility.Ability.PredefinedTargets = targets;
            activeAbility.Ability.IsPVPAbility = true;

            if (targets.Count > 0 && activeAbility.Ability.AbilityActivityType == Enumerators.AbilityActivityType.ACTIVE)
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
                    from = _battlegroundController.GetBoardUnitViewByModel(unitModel).Transform;
                }

                Action callback = () =>
                {
                    activeAbility.Ability.SelectedTargetAction(true);

                    _boardController.UpdateWholeBoard(null);
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

            activeAbility.Ability.Activate();
        }

        public void ActivateAbilitiesOnCard(BoardObject abilityCaller, WorkingCard card, Player owner)
        {
            foreach(AbilityData abilityData in card.Prototype.Abilities )
            {
                ActiveAbility activeAbility;
                if(abilityData.CallType != Enumerators.AbilityCallType.ENTRY)
                {
                    activeAbility = CreateActiveAbility(abilityData, card.Prototype.CardKind, abilityCaller, owner, card.Prototype, card);
                    activeAbility.Ability.Activate();
                }
            }
        }

        private void CreateAbilityByType(Enumerators.CardKind cardKind, AbilityData abilityData, out AbilityBase ability, out AbilityViewBase abilityView)
        {
            ability = null;
            abilityView = null;

            switch (abilityData.AbilityType)
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
                case Enumerators.AbilityType.WEAPON:
                    ability = new HeroWeaponAbility(cardKind, abilityData);
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
                case Enumerators.AbilityType.ADD_CARD_BY_NAME_TO_HAND:
                    ability = new AddCardByNameToHandAbility(cardKind, abilityData);
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
                    ability = new DamageOverlordOnCountItemsPlayedAbility(cardKind, abilityData);
                    abilityView = new DamageOverlordOnCountItemsPlayedAbilityView((DamageOverlordOnCountItemsPlayedAbility)ability);
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
                default:
                    throw new ArgumentOutOfRangeException(nameof(abilityData.AbilityType), abilityData.AbilityType, null);
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

                GameClient.Get<IOverlordExperienceManager>().ReportExperienceAction(card.BoardUnitModel.Card.Owner.SelfHero, Common.Enumerators.ExperienceActionType.PlayCard);

                card.BoardUnitModel.Card.Owner.RemoveCardFromHand(card.BoardUnitModel.Card);
                card.BoardUnitModel.Card.Owner.AddCardToBoard(card.BoardUnitModel.Card, (ItemPosition) card.FuturePositionOnBoard);

                if (card.BoardUnitModel.Card.Prototype.CardKind == Enumerators.CardKind.CREATURE)
                {
                    InternalTools.DoActionDelayed(() =>
                    {
                        Object.Destroy(card.GameObject);
                    }, 0.5f);

                    ProceedWithCardToGraveyard(card);
                }
                else
                {
                    card.BoardUnitModel.Card.Owner.AddCardToGraveyard(card.BoardUnitModel.Card);

                    card.GameObject.SetActive(true);

                    InternalTools.DoActionDelayed(() =>
                    {
                        _cardsController.RemoveCard(new object[] { card });
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
            card.BoardUnitModel.Card.Owner.GraveyardCardsCount++;

            _actionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
            {
                ActionType = Enumerators.ActionType.PlayCardFromHand,
                Caller = card,
                TargetEffects = new List<PastActionsPopup.TargetEffectParam>(),
                checkForCardOwner = false,
                workingCard = card.BoardUnitModel.Card
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
                        Enumerators.AbilityActivityType.PASSIVE,
                        Enumerators.AbilityCallType.DEATH,
                        null,
                        default(Enumerators.StatType),
                        default(Enumerators.SetType),
                        default(Enumerators.AbilityEffectType),
                        default(Enumerators.AttackRestriction),
                        default(Enumerators.CardType),
                        default(Enumerators.UnitStatusType),
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
                        Enumerators.GameMechanicDescriptionType.Reanimate,
                        default(Enumerators.SetType),
                        default(Enumerators.AbilitySubTrigger),
                        null,
                        0,
                        0
                        );
                    break;
                case Enumerators.AbilityType.DESTROY_TARGET_UNIT_AFTER_ATTACK:
                    abilityData = new AbilityData(
                        Enumerators.AbilityType.DESTROY_TARGET_UNIT_AFTER_ATTACK,
                        Enumerators.AbilityActivityType.PASSIVE,
                        Enumerators.AbilityCallType.ATTACK,
                        null,
                        default(Enumerators.StatType),
                        default(Enumerators.SetType),
                        default(Enumerators.AbilityEffectType),
                        default(Enumerators.AttackRestriction),
                        default(Enumerators.CardType),
                        default(Enumerators.UnitStatusType),
                        default(Enumerators.CardType),
                        0,
                        0,
                        0,
                        "",
                        0,
                        0,
                        0,
                        null,
                        Enumerators.GameMechanicDescriptionType.Destroy,
                        default(Enumerators.SetType),
                        default(Enumerators.AbilitySubTrigger),
                        null,
                        0,
                        0
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
