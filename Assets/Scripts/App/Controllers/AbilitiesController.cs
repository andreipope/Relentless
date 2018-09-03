using System;
using System.Collections.Generic;
using System.Linq;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class AbilitiesController : IController
    {
        private readonly object _lock = new object();

        private IGameplayManager _gameplayManager;

        private ITutorialManager _tutorialManager;

        private CardsController _cardsController;

        private PlayerController _playerController;

        private BattlegroundController _battlegroundController;

        private ActionsQueueController _actionsQueueController;

        private ulong _castedAbilitiesIds;

        private List<ActiveAbility> _activeAbilities;

        public void Init()
        {
            _activeAbilities = new List<ActiveAbility>();

            _gameplayManager = GameClient.Get<IGameplayManager>();
            _tutorialManager = GameClient.Get<ITutorialManager>();

            _cardsController = _gameplayManager.GetController<CardsController>();
            _playerController = _gameplayManager.GetController<PlayerController>();
            _battlegroundController = _gameplayManager.GetController<BattlegroundController>();
            _actionsQueueController = _gameplayManager.GetController<ActionsQueueController>();
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

        public List<AbilityBase> GetAbilitiesConnectedToUnit(BoardUnit unit)
        {
            return _activeAbilities.FindAll(x => x.Ability.TargetUnit == unit).Select(y => y.Ability).ToList();
        }

        public ActiveAbility CreateActiveAbility(
            AbilityData ability,
            Enumerators.CardKind kind,
            object boardObject,
            Player caller,
            Card cardOwner,
            WorkingCard workingCard)
        {
            lock (_lock)
            {
                ActiveAbility activeAbility = new ActiveAbility
                {
                    Id = _castedAbilitiesIds++,
                    Ability = CreateAbilityByType(kind, ability)
                };

                activeAbility.Ability.ActivityId = activeAbility.Id;
                activeAbility.Ability.PlayerCallerOfAbility = caller;
                activeAbility.Ability.CardOwnerOfAbility = cardOwner;
                activeAbility.Ability.MainWorkingCard = workingCard;

                if (boardObject != null)
                {
                    if (boardObject is BoardCard)
                    {
                        activeAbility.Ability.BoardCard = boardObject as BoardCard;
                    }
                    else
                    {
                        if (kind == Enumerators.CardKind.CREATURE)
                        {
                            activeAbility.Ability.AbilityUnitOwner = boardObject as BoardUnit;
                        }
                        else
                        {
                            activeAbility.Ability.BoardSpell = boardObject as BoardSpell;
                        }
                    }
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
            if (ability.AbilityActivityType == Enumerators.AbilityActivityType.ACTIVE)
            {
                return true;
            }

            return false;
        }

        public bool IsAbilityCallsAtStart(AbilityData ability)
        {
            if (ability.AbilityCallType == Enumerators.AbilityCallType.ENTRY)
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
                        {
                            if (opponent.BoardCards.Count > 0)
                            {
                                available = true;
                            }
                        }

                            break;
                        case Enumerators.AbilityTargetType.PLAYER_CARD:
                        {
                            if (localPlayer.BoardCards.Count > 1 || kind == Enumerators.CardKind.SPELL)
                            {
                                available = true;
                            }
                        }

                            break;
                        case Enumerators.AbilityTargetType.PLAYER:
                        case Enumerators.AbilityTargetType.OPPONENT:
                        case Enumerators.AbilityTargetType.ALL:
                            available = true;
                            break;
                    }
                }
            }

            return available;
        }

        public int GetStatModificatorByAbility(BoardUnit attacker, BoardUnit attacked, bool isAttackking)
        {
            int value = 0;

            Card attackedCard = attacked.Card.LibraryCard;
            Card attackerCard = attacker.Card.LibraryCard;

            List<AbilityData> abilities;

            if (isAttackking)
            {
                abilities = attackerCard.Abilities.FindAll(x =>
                    x.AbilityType == Enumerators.AbilityType.MODIFICATOR_STATS);

                for (int i = 0; i < abilities.Count; i++)
                {
                    if (attackedCard.CardSetType == abilities[i].AbilitySetType)
                    {
                        value += abilities[i].Value;
                    }
                }
            }

            abilities = attackerCard.Abilities.FindAll(x =>
                x.AbilityType == Enumerators.AbilityType.ADDITIONAL_DAMAGE_TO_HEAVY_IN_ATTACK);

            for (int i = 0; i < abilities.Count; i++)
            {
                if (attacked.IsHeavyUnit())
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
                        List<BoardUnit> units =
                            player.BoardCards.FindAll(x =>
                                x.InitialUnitType == ability.TargetCardType &&
                                x.UnitStatus == ability.TargetUnitStatusType);
                        if (units.Count > 0)
                            return true;

                        break;
                    }
                    case Enumerators.AbilityTargetType.OPPONENT_CARD:
                    {
                        List<BoardUnit> units =
                            opponent.BoardCards.FindAll(x =>
                                x.InitialUnitType == ability.TargetCardType &&
                                x.UnitStatus == ability.TargetUnitStatusType);
                        if (units.Count > 0)
                            return true;

                        break;
                    }
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
                        List<BoardUnit> units =
                            player.BoardCards.FindAll(x => x.UnitStatus == ability.TargetUnitStatusType);

                        if (units.Count > 0)
                            return true;

                        break;
                    }
                    case Enumerators.AbilityTargetType.OPPONENT_CARD:
                    {
                        List<BoardUnit> units =
                            opponent.BoardCards.FindAll(x => x.UnitStatus == ability.TargetUnitStatusType);

                        if (units.Count > 0)
                            return true;

                        break;
                    }
                }
            }

            return false;
        }

        public void CallAbility(
            Card libraryCard,
            BoardCard card,
            WorkingCard workingCard,
            Enumerators.CardKind kind,
            object boardObject,
            Action<BoardCard> action,
            bool isPlayer,
            Action onCompleteCallback,
            object target = null,
            HandBoardCard handCard = null)
        {
            ResolveAllAbilitiesOnUnit(boardObject, false);

            bool canUseAbility = false;
            ActiveAbility activeAbility = null;
            foreach (AbilityData item in libraryCard.Abilities)
            {
                // todo improve it bcoz can have queue of abilities with targets
                activeAbility =
                    CreateActiveAbility(item, kind, boardObject, workingCard.Owner, libraryCard, workingCard);

                if (IsAbilityCanActivateTargetAtStart(item))
                {
                    canUseAbility = true;
                }
                else
                {
                    activeAbility.Ability.Activate();
                }
            }

            if (kind == Enumerators.CardKind.SPELL)
            {
            }
            else
            {
                workingCard.Owner.RemoveCardFromHand(workingCard);
                workingCard.Owner.AddCardToBoard(workingCard);
            }

            if (kind == Enumerators.CardKind.SPELL)
            {
                if (handCard != null && isPlayer)
                {
                    handCard.GameObject.SetActive(false);
                }
            }

            if (canUseAbility)
            {
                AbilityData ability = libraryCard.Abilities.Find(x => IsAbilityCanActivateTargetAtStart(x));

                if (ability.TargetCardType != Enumerators.CardType.NONE &&
                    !HasSpecialUnitOnBoard(workingCard, ability) ||
                    ability.TargetUnitStatusType != Enumerators.UnitStatusType.NONE &&
                    !HasSpecialUnitStatusOnBoard(workingCard, ability))
                {
                    CallPermanentAbilityAction(isPlayer, action, card, target, activeAbility, kind);

                    onCompleteCallback?.Invoke();

                    ResolveAllAbilitiesOnUnit(boardObject);
                    return;
                }

                if (CheckActivateAvailability(kind, ability, workingCard.Owner))
                {
                    activeAbility.Ability.Activate();

                    if (isPlayer)
                    {
                        activeAbility.Ability.ActivateSelectTarget(
                            callback: () =>
                            {
                                if (kind == Enumerators.CardKind.SPELL && isPlayer)
                                {
                                    card.WorkingCard.Owner.Goo -= card.ManaCost;
                                    _tutorialManager.ReportAction(Enumerators.TutorialReportAction.MOVE_CARD);

                                    handCard.GameObject.SetActive(true);
                                    card.RemoveCardParticle.Play(); // move it when card should call hide action

                                    workingCard.Owner.RemoveCardFromHand(workingCard, true);
                                    workingCard.Owner.AddCardToBoard(workingCard);

                                    GameClient.Get<ITimerManager>().AddTimer(_cardsController.RemoveCard, new object[]
                                    {
                                        card
                                    }, 0.5f);

                                    GameClient.Get<ITimerManager>().AddTimer(
                                        creat =>
                                        {
                                            workingCard.Owner.GraveyardCardsCount++;

                                            _actionsQueueController.PostGameActionReport(
                                                _actionsQueueController.FormatGameActionReport(
                                                    Enumerators.ActionType.PLAY_SPELL_CARD, new object[]
                                                    {
                                                        workingCard.Owner, card
                                                    }));
                                        },
                                        null,
                                        1.5f);
                                }

                                action?.Invoke(card);

                                onCompleteCallback?.Invoke();

                                ResolveAllAbilitiesOnUnit(boardObject);
                            },
                            failedCallback: () =>
                            {
                                if (kind == Enumerators.CardKind.SPELL && isPlayer)
                                {
                                    handCard.GameObject.SetActive(true);
                                    handCard.ResetToHandAnimation();
                                    handCard.CheckStatusOfHighlight();

                                    workingCard.Owner.CardsInHand.Add(card.WorkingCard);
                                    _battlegroundController.PlayerHandCards.Add(card);
                                    _battlegroundController.UpdatePositionOfCardsInPlayerHand();

                                    _playerController.IsCardSelected = false;
                                }
                                else
                                {
                                    Debug.Log("RETURN CARD TO HAND MAYBE.. SHOULD BE CASE !!!!!");
                                    action?.Invoke(card);
                                }

                                onCompleteCallback?.Invoke();

                                ResolveAllAbilitiesOnUnit(boardObject);
                            });
                    }
                    else
                    {
                        switch (target)
                        {
                            case BoardUnit unit:
                                activeAbility.Ability.TargetUnit = unit;
                                break;
                            case Player player:
                                activeAbility.Ability.TargetPlayer = player;
                                break;
                        }

                        activeAbility.Ability.SelectedTargetAction(true);

                        _battlegroundController.UpdatePositionOfBoardUnitsOfPlayer(_gameplayManager.CurrentPlayer
                            .BoardCards);
                        _battlegroundController.UpdatePositionOfBoardUnitsOfOpponent();

                        onCompleteCallback?.Invoke();

                        ResolveAllAbilitiesOnUnit(boardObject);
                    }
                }
                else
                {
                    CallPermanentAbilityAction(isPlayer, action, card, target, activeAbility, kind);
                    onCompleteCallback?.Invoke();

                    ResolveAllAbilitiesOnUnit(boardObject);
                }
            }
            else
            {
                CallPermanentAbilityAction(isPlayer, action, card, target, activeAbility, kind);
                onCompleteCallback?.Invoke();

                ResolveAllAbilitiesOnUnit(boardObject);
            }
        }

        public void BuffUnitByAbility(Enumerators.AbilityType ability, object target, Card card, Player owner)
        {
            ActiveAbility activeAbility =
                CreateActiveAbility(GetAbilityDataByType(ability), card.CardKind, target, owner, card, null);
            activeAbility.Ability.Activate();
        }

        public void CallAbilitiesInHand(BoardCard boardCard, WorkingCard card)
        {
            List<AbilityData> handAbilities =
                card.LibraryCard.Abilities.FindAll(x => x.AbilityCallType.Equals(Enumerators.AbilityCallType.IN_HAND));
            foreach (AbilityData ability in handAbilities)
            {
                CreateActiveAbility(ability, card.LibraryCard.CardKind, boardCard, card.Owner, card.LibraryCard, card)
                    .Ability.Activate();
            }
        }

        private AbilityBase CreateAbilityByType(Enumerators.CardKind cardKind, AbilityData abilityData)
        {
            AbilityBase ability = null;
            switch (abilityData.AbilityType)
            {
                case Enumerators.AbilityType.HEAL:
                    ability = new HealTargetAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.DAMAGE_TARGET:
                    ability = new DamageTargetAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.DAMAGE_TARGET_ADJUSTMENTS:
                    ability = new DamageTargetAdjustmentsAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.ADD_GOO_VIAL:
                    ability = new AddGooVialsAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.MODIFICATOR_STATS:
                    ability = new ModificateStatAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.MASSIVE_DAMAGE:
                    ability = new MassiveDamageAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.CHANGE_STAT:
                    ability = new ChangeStatAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.STUN:
                    ability = new StunAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.STUN_OR_DAMAGE_ADJUSTMENTS:
                    ability = new StunOrDamageAdjustmentsAbility(cardKind, abilityData);
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
                    break;
                case Enumerators.AbilityType.DESTROY_UNIT_BY_TYPE:
                    ability = new DestroyUnitByTypeAbility(cardKind, abilityData);
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
                    break;
                case Enumerators.AbilityType.FREEZE_UNITS:
                    ability = new FreezeUnitsAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.TAKE_DAMAGE_RANDOM_ENEMY:
                    ability = new TakeDamageRandomEnemyAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.TAKE_CONTROL_ENEMY_UNIT:
                    ability = new TakeControlEnemyUnitAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.GUARD:
                    ability = new ShieldAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.DESTROY_FROZEN_UNIT:
                    ability = new DestroyFrozenZombieAbility(cardKind, abilityData);
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
                    break;
                case Enumerators.AbilityType.ATTACK_OVERLORD:
                    ability = new AttackOverlordAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.ADJACENT_UNITS_GET_HEAVY:
                    ability = new AdjacentUnitsGetHeavyAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.FREEZE_NUMBER_OF_RANDOM_ALLY:
                    ability = new FreezeNumberOfRandomAllyAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.ADD_CARD_BY_NAME_TO_HAND:
                    ability = new AddCardByNameToHandAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.DEAL_DAMAGE_TO_THIS_AND_ADJACENT_UNITS:
                    ability = new DealDamageToThisAndAdjacentUnitsAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.SWING:
                    ability = new SwingAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.TAKE_DEFENSE_IF_OVERLORD_HAS_LESS_DEFENSE_THAN:
                    ability = new TakeDefenseIfOverlordHasLessDefenseThanAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.ADDITIONAL_DAMAGE_TO_HEAVY_IN_ATTACK:
                    ability = new AdditionalDamageToHeavyInAttackAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.GAIN_NUMBER_OF_LIFE_FOR_EACH_DAMAGE_THIS_DEALS:
                    ability = new GainNumberOfLifeForEachDamageThisDealsAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.UNIT_WEAPON:
                    ability = new UnitWeaponAbility(cardKind, abilityData);
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
                    break;
            }

            return ability;
        }

        private void ResolveAllAbilitiesOnUnit(object boardObject, bool status = true)
        {
            if (boardObject is BoardUnit unit)
            {
                unit.IsAllAbilitiesResolvedAtStart = status;
            }
        }

        private void CallPermanentAbilityAction(
            bool isPlayer,
            Action<BoardCard> action,
            BoardCard card,
            object target,
            ActiveAbility activeAbility,
            Enumerators.CardKind kind)
        {
            if (isPlayer)
            {
                if (kind == Enumerators.CardKind.SPELL)
                {
                    card.WorkingCard.Owner.Goo -= card.ManaCost;
                    _tutorialManager.ReportAction(Enumerators.TutorialReportAction.MOVE_CARD);

                    card.GameObject.SetActive(true);
                    card.RemoveCardParticle.Play(); // move it when card should call hide action

                    card.WorkingCard.Owner.RemoveCardFromHand(card.WorkingCard);
                    card.WorkingCard.Owner.AddCardToBoard(card.WorkingCard);

                    GameClient.Get<ITimerManager>().AddTimer(_cardsController.RemoveCard, new object[]
                    {
                        card
                    }, 0.5f);

                    GameClient.Get<ITimerManager>().AddTimer(
                        create =>
                        {
                            card.WorkingCard.Owner.GraveyardCardsCount++;

                            _actionsQueueController.PostGameActionReport(_actionsQueueController.FormatGameActionReport(
                                Enumerators.ActionType.PLAY_SPELL_CARD, new object[]
                                {
                                    card.WorkingCard.Owner, card
                                }));
                        },
                        null,
                        1.5f);
                }

                action?.Invoke(card);
            }
            else
            {
                if (activeAbility == null)
                    return;

                switch (target)
                {
                    case BoardUnit unit:
                        activeAbility.Ability.TargetUnit = unit;
                        break;
                    case Player player:
                        activeAbility.Ability.TargetPlayer = player;
                        break;
                }

                activeAbility.Ability.SelectedTargetAction(true);
            }

            _battlegroundController.UpdatePositionOfBoardUnitsOfPlayer(_gameplayManager.CurrentPlayer.BoardCards);
            _battlegroundController.UpdatePositionOfBoardUnitsOfOpponent();
        }

        private AbilityData GetAbilityDataByType(Enumerators.AbilityType ability)
        {
            AbilityData abilityData = null;

            switch (ability)
            {
                case Enumerators.AbilityType.REANIMATE_UNIT:
                    abilityData = new AbilityData();
                    abilityData.BuffType = "REANIMATE";
                    abilityData.Type = "REANIMATE_UNIT";
                    abilityData.ActivityType = "PASSIVE";
                    abilityData.CallType = "DEATH";
                    abilityData.ParseData();
                    break;
                case Enumerators.AbilityType.DESTROY_TARGET_UNIT_AFTER_ATTACK:
                    abilityData = new AbilityData();
                    abilityData.BuffType = "DESTROY";
                    abilityData.Type = "DESTROY_TARGET_UNIT_AFTER_ATTACK";
                    abilityData.ActivityType = "PASSIVE";
                    abilityData.CallType = "ATTACK";
                    abilityData.ParseData();
                    break;
            }

            return abilityData;
        }

        public class ActiveAbility
        {
            public ulong Id;

            public AbilityBase Ability;
        }
    }
}
