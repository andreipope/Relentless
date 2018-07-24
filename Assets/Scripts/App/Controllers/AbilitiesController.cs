// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace LoomNetwork.CZB
{
    public class AbilitiesController : IController
    {
        private IGameplayManager _gameplayManager;
        private CardsController _cardsController;
        private PlayerController _playerController;
        private BattlegroundController _battlegroundController;
        private ActionsQueueController _actionsQueueController;

        private object _lock = new object();

        private ulong _castedAbilitiesIds = 0;
        private List<ActiveAbility> _activeAbilities;

        public void Init()
        {
            _activeAbilities = new List<ActiveAbility>();


            _gameplayManager = GameClient.Get<IGameplayManager>();
            _cardsController = _gameplayManager.GetController<CardsController>();
            _playerController = _gameplayManager.GetController<PlayerController>();
            _battlegroundController = _gameplayManager.GetController<BattlegroundController>();
            _actionsQueueController = _gameplayManager.GetController<ActionsQueueController>();
        }

        public void Reset()
        {
            lock (_lock)
            {
                foreach (var item in _activeAbilities)
                    item.ability.Dispose();
                _activeAbilities.Clear();
            }

            _castedAbilitiesIds = 0;
        }

        public void Update()
        {
            lock (_lock)
            {
                foreach (var item in _activeAbilities)
                    item.ability.Update();
            }
        }

        public void Dispose()
        {
            Reset();
        }

        public void DeactivateAbility(ulong id)
        {
            lock (_lock)
            {
                var item = _activeAbilities.Find(x => x.id == id);
                if (_activeAbilities.Contains(item))
                    _activeAbilities.Remove(item);

                if (item != null && item.ability != null)
                    item.ability.Dispose();
            }
        }

        public ActiveAbility CreateActiveAbility(AbilityData ability, Enumerators.CardKind kind, object boardObject, Player caller, Data.Card cardOwner)
        {
            lock (_lock)
            {
                ActiveAbility activeAbility = new ActiveAbility()
                {
                    id = _castedAbilitiesIds++,
                    ability = CreateAbilityByType(kind, ability)
                };

                activeAbility.ability.playerCallerOfAbility = caller;
                activeAbility.ability.cardOwnerOfAbility = cardOwner;

                if (kind == Enumerators.CardKind.CREATURE)
                    activeAbility.ability.abilityUnitOwner = boardObject as BoardUnit;
                else
                    activeAbility.ability.boardSpell = boardObject as BoardSpell;

                _activeAbilities.Add(activeAbility);

                return activeAbility;
            }
        }

        private AbilityBase CreateAbilityByType(Enumerators.CardKind cardKind, AbilityData abilityData)
        {
            AbilityBase ability = null;
            switch (abilityData.abilityType)
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
                case Enumerators.AbilityType.ADD_GOO_CARRIER:
                    ability = new AddGooByCarrierAbility(cardKind, abilityData);
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
                case Enumerators.AbilityType.DEVOUR_ZOMBIE_AND_COMBINE_STATS:
                    ability = new DevourZombieAndCombineStatsAbility(cardKind, abilityData);
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
                case Enumerators.AbilityType.RAGE:
                    ability = new RageAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.FREEZE_UNITS:
                    ability = new FreezeUnitsAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.TAKE_DAMAGE_RANDOM_UNIT:
                    ability = new TakeDamageRandomUnitAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.TAKE_CONTROL_ENEMY_UNIT:
                    ability = new TakeControlEnemyUnitAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.SHILED:
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
                default:
                    break;
            }
            return ability;
        }

        public bool HasTargets(AbilityData ability)
        {
            if(ability.abilityTargetTypes.Count > 0)
                return true;
            return false;
        }

        public bool IsAbilityActive(AbilityData ability)
        {
            if (ability.abilityActivityType == Enumerators.AbilityActivityType.ACTIVE)
                return true;
            return false;
        }

        public bool IsAbilityCallsAtStart(AbilityData ability)
        {
            if (ability.abilityCallType == Enumerators.AbilityCallType.AT_START)
                return true;
            return false;
        }

        public bool IsAbilityCanActivateTargetAtStart(AbilityData ability)
        {
            if (HasTargets(ability) && IsAbilityCallsAtStart(ability) && IsAbilityActive(ability))
                return true;
            return false;
        }

        public bool IsAbilityCanActivateWithoutTargetAtStart(AbilityData ability)
        {
            if (HasTargets(ability) && IsAbilityCallsAtStart(ability) && !IsAbilityActive(ability))
                return true;
            return false;
        }

        public bool CheckActivateAvailability(Enumerators.CardKind kind, AbilityData ability, Player localPlayer)
        {
            bool available = false;

            var opponent = localPlayer.Equals(_gameplayManager.CurrentPlayer) ? _gameplayManager.OpponentPlayer : _gameplayManager.CurrentPlayer;

            lock (_lock)
            {
                foreach (var item in ability.abilityTargetTypes)
                {
                    switch (item)
                    {
                        case Enumerators.AbilityTargetType.OPPONENT_CARD:
                            {
                                if (opponent.BoardCards.Count > 0)
                                    available = true;
                            }
                            break;
                        case Enumerators.AbilityTargetType.PLAYER_CARD:
                            {
                                if (localPlayer.BoardCards.Count > 1 || kind == Enumerators.CardKind.SPELL)
                                    available = true;
                            }
                            break;
                        case Enumerators.AbilityTargetType.PLAYER:
                        case Enumerators.AbilityTargetType.OPPONENT:
                        case Enumerators.AbilityTargetType.ALL:
                            available = true;
                            break;
                        default: break;
                    }
                }
            }

            return available;
        }

        public int GetStatModificatorByAbility(WorkingCard attacker, WorkingCard attacked)
        {
            int value = 0;

            var attackedCard = GameClient.Get<IDataManager>().CachedCardsLibraryData.GetCard(attacked.cardId);
            var attackerCard = GameClient.Get<IDataManager>().CachedCardsLibraryData.GetCard(attacker.cardId);

            var abilities = attackerCard.abilities.FindAll(x =>
            x.abilityType == Enumerators.AbilityType.MODIFICATOR_STATS);

            for (int i = 0; i < abilities.Count; i++)
            {
                if (attackedCard.cardSetType == abilities[i].abilitySetType)
                    value += abilities[i].value;
            }
            return value;
        }

        public static uint[] AbilityTypeToUintArray(List<Enumerators.AbilityType> abilities)
        {
            uint[] abils = new uint[abilities.Count];
            for (int i = 0; i < abilities.Count; i++)
                abils[i] = (uint)abilities[i];

            return abils;
        }

        public static List<Enumerators.AbilityType> AbilityTypeToUintArray(uint[] abilities)
        {
            List<Enumerators.AbilityType> abils = new List<Enumerators.AbilityType>();
            for (int i = 0; i < abilities.Length; i++)
                abils[i] = (Enumerators.AbilityType)abilities[i];

            return abils;
        }

        public bool HasUnitTypeOnBoard(Player player, Enumerators.CardType type)
        {
            return player.BoardCards.FindAll(x => x.Card.type == type).Count > 0;
        }

        public void CallAbility(Card libraryCard, BoardCard card, WorkingCard workingCard, Enumerators.CardKind kind, object boardObject, Action<BoardCard> action, bool isPlayer, Action onCompleteCallback, object target = null, HandBoardCard handCard = null)
        {
            Vector3 postionOfCardView = Vector3.zero;

            if (card != null && card.gameObject != null)
                postionOfCardView = card.transform.position;

            bool canUseAbility = false;
            ActiveAbility activeAbility = null;
            foreach (var item in libraryCard.abilities) //todo improve it bcoz can have queue of abilities with targets
            {
                activeAbility = CreateActiveAbility(item, kind, boardObject, workingCard.owner, libraryCard);
                //Debug.Log(_abilitiesController.IsAbilityCanActivateTargetAtStart(item));
                if (IsAbilityCanActivateTargetAtStart(item))
                    canUseAbility = true;
                else //if (_abilitiesController.IsAbilityCanActivateWithoutTargetAtStart(item))
                    activeAbility.ability.Activate();
            }

            if (kind == Enumerators.CardKind.SPELL)
            {
                //if (isPlayer)
                //    currentSpellCard = card;
            }
            else
            {
                workingCard.owner.RemoveCardFromHand(workingCard);
                workingCard.owner.AddCardToBoard(workingCard);
            }

            if(kind == Enumerators.CardKind.SPELL)
            {
                if (handCard != null && isPlayer)
                {
                    handCard.gameObject.SetActive(false);
                }
            }

            if (canUseAbility)
            {
                var ability = libraryCard.abilities.Find(x => IsAbilityCanActivateTargetAtStart(x));

                if (ability.targetCardType != Enumerators.CardType.NONE)
                {
                    if(!HasUnitTypeOnBoard(workingCard.owner.Equals(_gameplayManager.CurrentPlayer) ? _gameplayManager.OpponentPlayer : _gameplayManager.CurrentPlayer, ability.targetCardType))
                    {
                        CallPermanentAbilityAction(isPlayer, action, card, target, activeAbility, kind);
                        onCompleteCallback?.Invoke();

                        return;
                    }
                }

                if (CheckActivateAvailability(kind, ability, workingCard.owner))
                {
                    activeAbility.ability.Activate();

                    if (isPlayer)
                    {
                        activeAbility.ability.ActivateSelectTarget(callback: () =>
                        {
                            if (kind == Enumerators.CardKind.SPELL && isPlayer)
                            {
                                handCard.gameObject.SetActive(true);
                                card.removeCardParticle.Play(); // move it when card should call hide action

                                workingCard.owner.RemoveCardFromHand(workingCard, true);
                                workingCard.owner.AddCardToBoard(workingCard);

                                GameClient.Get<ITimerManager>().AddTimer(_cardsController.RemoveCard, new object[] { card }, 0.5f, false);

                                GameClient.Get<ITimerManager>().AddTimer((creat) =>
                                {
                                    workingCard.owner.GraveyardCardsCount++;

                                    _actionsQueueController.PostGameActionReport(_actionsQueueController.FormatGameActionReport(Enumerators.ActionType.PLAY_SPELL_CARD, new object[]
                                    {
                                        workingCard.owner,
                                        card
                                    }));
                                }, null, 1.5f);
                            }

                            action?.Invoke(card);

                            onCompleteCallback?.Invoke();
                        },
                        failedCallback: () =>
                        {
                            if (kind == Enumerators.CardKind.SPELL && isPlayer)
                            {
                                handCard.gameObject.SetActive(true);
                                handCard.ResetToHandAnimation();
                                handCard.CheckStatusOfHighlight();

                                workingCard.owner.CardsInHand.Add(card.WorkingCard);
                                _battlegroundController.playerHandCards.Add(card);
                                _battlegroundController.UpdatePositionOfCardsInPlayerHand();

                                _playerController.IsCardSelected = false;
                                //  currentSpellCard = null;

                                // GameClient.Get<IUIManager>().GetPage<GameplayPage>().SetEndTurnButtonStatus(true);
                            }
                            else
                            {
                                Debug.Log("RETURN CARD TO HAND MAYBE.. SHOULD BE CASE !!!!!");
                                action?.Invoke(card);
                            }

                            onCompleteCallback?.Invoke();
                        });
                    }
                    else
                    {
                        if (target is BoardUnit)
                            activeAbility.ability.targetUnit = target as BoardUnit;
                        else if (target is Player)
                            activeAbility.ability.targetPlayer = target as Player;

                        activeAbility.ability.SelectedTargetAction(true);

                        _battlegroundController.UpdatePositionOfBoardUnitsOfPlayer();
                        _battlegroundController.UpdatePositionOfBoardUnitsOfOpponent();
                        //  Debug.LogError(activeAbility.ability.abilityType.ToString() + " ABIITY WAS ACTIVATED!!!! on " + (target == null ? target : target.GetType()));

                        onCompleteCallback?.Invoke();
                    }
                }
                else
                {
                    CallPermanentAbilityAction(isPlayer, action, card, target, activeAbility, kind);
                    onCompleteCallback?.Invoke();
                }
            }
            else
            {
                CallPermanentAbilityAction(isPlayer, action, card, target, activeAbility, kind);
                onCompleteCallback?.Invoke();
            }
        }

        private void CallPermanentAbilityAction(bool isPlayer, Action<BoardCard> action, BoardCard card, object target, ActiveAbility activeAbility, Enumerators.CardKind kind)
        {
            if (isPlayer)
            {
                if (kind == Enumerators.CardKind.SPELL)
                {
                    card.gameObject.SetActive(true);
                    card.removeCardParticle.Play(); // move it when card should call hide action

                    card.WorkingCard.owner.RemoveCardFromHand(card.WorkingCard);
                    card.WorkingCard.owner.AddCardToBoard(card.WorkingCard);

                    GameClient.Get<ITimerManager>().AddTimer(_cardsController.RemoveCard, new object[] { card }, 0.5f, false);

                    GameClient.Get<ITimerManager>().AddTimer((creat) =>
                    {
                        card.WorkingCard.owner.GraveyardCardsCount++;

                        _actionsQueueController.PostGameActionReport(_actionsQueueController.FormatGameActionReport(Enumerators.ActionType.PLAY_SPELL_CARD, new object[]
                        {
                            card.WorkingCard.owner,
                            card
                        }));
                    }, null, 1.5f);
                }

                action?.Invoke(card);
            }
            else
            {
                if (activeAbility == null)
                    return;
                if (target is BoardUnit)
                    activeAbility.ability.targetUnit = target as BoardUnit;
                else if (target is Player)
                    activeAbility.ability.targetPlayer = target as Player;

                activeAbility.ability.SelectedTargetAction(true);
            }

            _battlegroundController.UpdatePositionOfBoardUnitsOfPlayer();
            _battlegroundController.UpdatePositionOfBoardUnitsOfOpponent();
        }

        public Player GetOpponentPlayer(AbilityBase ability)
        {
            return ability.playerCallerOfAbility.Equals(_gameplayManager.CurrentPlayer) ? _gameplayManager.OpponentPlayer : _gameplayManager.CurrentPlayer;
        }

        public class ActiveAbility
        {
            public ulong id;
            public AbilityBase ability;
        }
    }
}