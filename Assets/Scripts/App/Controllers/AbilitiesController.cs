using CCGKit;
using GrandDevs.CZB.Common;
using GrandDevs.CZB.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GrandDevs.CZB
{
    public class AbilitiesController : IController
    {
        private object _lock = new object();

        private ulong _castedAbilitiesIds = 0;
        private List<ActiveAbility> _activeAbilities;

        public AbilitiesController()
        {
            _activeAbilities = new List<ActiveAbility>();
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

        public ActiveAbility CreateActiveAbility(AbilityData ability, Enumerators.CardKind kind, object boardObject, DemoHumanPlayer caller)
        {
            lock (_lock)
            {
                ActiveAbility activeAbility = new ActiveAbility()
                {
                    id = _castedAbilitiesIds++,
                    ability = CreateAbilityByType(kind, ability)
                };

                activeAbility.ability.cardCaller = caller;
                if(kind == Enumerators.CardKind.CREATURE)
                    activeAbility.ability.boardCreature = boardObject as BoardCreature;
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
                case Enumerators.AbilityType.MODIFICATOR_STATS:
                    ability = new ModificateStatAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.MASSIVE_DAMAGE:
                    ability = new MassiveDamageAbility(cardKind, abilityData);
                    break;
                case Enumerators.AbilityType.CHANGE_STAT:
                    ability = new ChangeStatAbility(cardKind, abilityData);
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

        public bool CheckActivateAvailability(Enumerators.CardKind kind, AbilityData ability, DemoHumanPlayer localPlayer)
        {
            bool available = false;

            lock (_lock)
            {
                Debug.Log("ability - " + ability);

                foreach (var item in ability.abilityTargetTypes)
                {
                    Debug.Log("item - " + item);

                    switch (item)
                    {
                        case Enumerators.AbilityTargetType.OPPONENT_CARD:
                            {
                                if (localPlayer.opponentBoardZone.cards.Count > 0)
                                    available = true;
                            }
                            break;
                        case Enumerators.AbilityTargetType.PLAYER_CARD:
                            {
                                Debug.Log("localPlayer.boardZone.cards.Count - " + localPlayer.boardZone.cards.Count);
                                Debug.Log("kind - " + kind);

                                if (localPlayer.boardZone.cards.Count > 1 || kind == Enumerators.CardKind.SPELL)
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
                    Debug.Log("available - " + available);

                }
            }

            return available;
        }

        public int GetStatModificatorByAbility(RuntimeCard attacker, RuntimeCard attacked)
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

        public void UpdateAttackAbilities(RuntimeCard attacker)
        {
            Debug.Log("@");

            var attackerCard = GameClient.Get<IDataManager>().CachedCardsLibraryData.GetCard(attacker.cardId);

            var abilities = _activeAbilities.FindAll(x => x.ability.abilityCallType == Enumerators.AbilityCallType.AT_ATTACK);
            foreach(var ability in abilities)
            {
                if(ability.ability.boardCreature.card == attacker)
                {
                    Debug.Log("@@");

                    if(ability.ability.abilityType == Enumerators.AbilityType.CHANGE_STAT)
                        (ability.ability as ChangeStatAbility).Action();
                }
            }
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
    }


    public class ActiveAbility
    {
        public ulong id;
        public AbilityBase ability;
    }
}