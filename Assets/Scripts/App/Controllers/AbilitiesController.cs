using CCGKit;
using GrandDevs.CZB.Common;
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
        private List<AbilityBase> _abilities;

        public AbilitiesController()
        {
            _activeAbilities = new List<ActiveAbility>();

            FillAbilities();
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

        public ActiveAbility ActivateAbility(Enumerators.Ability ability, BoardCreature creature, DemoHumanPlayer caller)
        {
            lock (_lock)
            {
                ActiveAbility activeAbility = new ActiveAbility()
                {
                    id = _castedAbilitiesIds++,
                    ability = _abilities.Find(x => x.ability == ability).Clone()
                };

                activeAbility.ability.cardCaller = caller;
                activeAbility.ability.boardCreature = creature;

                _activeAbilities.Add(activeAbility);

                return activeAbility;
            }
        }

        public ActiveAbility ActivateAbility(Enumerators.Ability ability, BoardSpell spell, DemoHumanPlayer caller)
        {
            lock (_lock)
            {
                ActiveAbility activeAbility = new ActiveAbility()
                {
                    id = _castedAbilitiesIds++,
                    ability = _abilities.Find(x => x.ability == ability).Clone()
                };

                activeAbility.ability.cardCaller = caller;
                activeAbility.ability.boardSpell = spell;

                _activeAbilities.Add(activeAbility);

                return activeAbility;
            }
        }

        public bool HasTargets(Enumerators.Ability ability)
        {
            if (_abilities.Find(x => x.ability == ability).abilityTargetTypes.Count > 0)
                return true;
            return false;
        }

        public bool IsAbilityActive(Enumerators.Ability ability)
        {
            if (_abilities.Find(x => x.ability == ability).abilityActivityType == Enumerators.AbilityActivityType.ACTIVE)
                return true;
            return false;
        }

        public bool IsAbilityCallsAtStart(Enumerators.Ability ability)
        {
            if (_abilities.Find(x => x.ability == ability).abilityCallType == Enumerators.AbilityCallType.AT_START)
                return true;
            return false;
        }

        public bool IsAbilityCanActivateTargetAtStart(Enumerators.Ability ability)
        {
            if (HasTargets(ability) && IsAbilityCallsAtStart(ability) && IsAbilityActive(ability))
                return true;
            return false;
        }

        public bool IsAbilityCanActivateWithoutTargetAtStart(Enumerators.Ability ability)
        {
            if (HasTargets(ability) && IsAbilityCallsAtStart(ability) && !IsAbilityActive(ability))
                return true;
            return false;
        }

        public bool CheckActivateAvailability(Enumerators.CardKind kind, Enumerators.Ability abilityType, DemoHumanPlayer localPlayer)
        {
            bool available = false;

            lock (_lock)
            {
                var ability = _abilities.Find(x => x.ability == abilityType);

                foreach (var item in ability.abilityTargetTypes)
                {
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
                }
            }

            return available;
        }


        public ActiveAbility GetAbilityByTypeCardOwner(Enumerators.Ability ability, BoardCreature creature, DemoHumanPlayer caller)
        {
            lock (_lock)
            {
                return _activeAbilities.Find(x => x.ability.boardCreature == creature && x.ability.cardCaller.Equals(caller) && x.ability.ability.Equals(ability));
            }
        }

        public ActiveAbility GetAbilityByTypeCardOwner(Enumerators.Ability ability, BoardSpell spell, DemoHumanPlayer caller)
        {
            lock (_lock)
            {
                return _activeAbilities.Find(x => x.ability.boardSpell == spell && x.ability.cardCaller.Equals(caller) && x.ability.ability.Equals(ability));
            }
        }

        public AbilityBase GetAbilityInfoByType(Enumerators.Ability ability)
        {
            return _abilities.Find(x => x.ability == ability);
        }

        public int GetStatModificatorByAbility(RuntimeCard attacker, RuntimeCard attacked)
        {
            int value = 0;
            var attackedCard = GameClient.Get<IDataManager>().CachedCardsLibraryData.GetCard(attacked.cardId);
            var abilities = attacker.abilities.FindAll(x =>
            x == Enumerators.Ability.ONE_ADDITIONAL_DAMAGE_VERSUS_LIFE ||
            x == Enumerators.Ability.ONE_ADDITIONAL_DAMAGE_VERSUS_WATER //todo improve
            );

            ModificateStatVersusAbility ability;
            for (int i = 0; i < abilities.Count; i++)
            {
                ability = (GetAbilityInfoByType(abilities[i]) as ModificateStatVersusAbility);
                if (attackedCard.cardSetType == ability.setType)
                    value += ability.value;
            }

            return value;
        }

        public static uint[] AbilityTypeToUintArray(List<Enumerators.Ability> abilities)
        {
            uint[] abils = new uint[abilities.Count];
            for (int i = 0; i < abilities.Count; i++)
                abils[i] = (uint)abilities[i];

            return abils;
        }

        public static List<Enumerators.Ability> AbilityUintArrayTypeToList(uint[] abilities)
        {
            List<Enumerators.Ability> abils = new List<Enumerators.Ability>();
            for (int i = 0; i < abilities.Length; i++)
                abils.Add((Enumerators.Ability)abilities[i]);

            return abils;
        }

        #region fill abilities
        private void FillAbilities()
        {
            _abilities = new List<AbilityBase>();
            _abilities.Add(new ModificateStatAbility(Enumerators.Ability.EXTRA_DAMAGE_TO_FIRE,
                                         Enumerators.CardKind.CREATURE,
                                         Enumerators.AbilityType.MODIFICATOR_STATIC_DAMAGE,
                                                     Enumerators.AbilityActivityType.ACTIVE,
                                                     Enumerators.AbilityCallType.AT_START,
                                                     new List<Enumerators.AbilityTargetType>()
                                                            { Enumerators.AbilityTargetType.OPPONENT_CARD,
                                                              Enumerators.AbilityTargetType.PLAYER_CARD    },
                                                     Enumerators.StatType.DAMAGE,
                                                     Enumerators.SetType.FIRE,
                                                     1));

            _abilities.Add(new HealTargetAbility(Enumerators.Ability.HEALS_ZOMBIE_4_HP,
                                         Enumerators.CardKind.SPELL,
                                         Enumerators.AbilityType.HEAL,
                                         Enumerators.AbilityActivityType.ACTIVE,
                                         Enumerators.AbilityCallType.AT_START,
                                         new List<Enumerators.AbilityTargetType>()
                                                { Enumerators.AbilityTargetType.PLAYER_CARD,
                                                  Enumerators.AbilityTargetType.OPPONENT_CARD },
                                         4));

            // not working properly
            _abilities.Add(new DeactivateTargetAbility(Enumerators.Ability.STUN_TARGET_UNTILL_NEXT_TURN,
                                         Enumerators.CardKind.CREATURE,
                                         Enumerators.AbilityType.STUN,
                                         Enumerators.AbilityActivityType.ACTIVE,
                                         Enumerators.AbilityCallType.AT_START,
                                         new List<Enumerators.AbilityTargetType>()
                                                { Enumerators.AbilityTargetType.OPPONENT_CARD,
                                                  Enumerators.AbilityTargetType.PLAYER_CARD },
                                         1));


            _abilities.Add(new ModificateStatVersusAbility(Enumerators.Ability.ONE_ADDITIONAL_DAMAGE_VERSUS_LIFE,
                                         Enumerators.CardKind.CREATURE,
                                          Enumerators.AbilityType.MODIFICATOR_STAT_VERSUS,
                                         Enumerators.AbilityActivityType.PASSIVE,
                                         Enumerators.AbilityCallType.PERMANENT,
                                         new List<Enumerators.AbilityTargetType>()
                                                { Enumerators.AbilityTargetType.NONE },
                                         Enumerators.StatType.DAMAGE,
                                         Enumerators.SetType.LIFE,
                                         1));

            _abilities.Add(new ModificateStatVersusAbility(Enumerators.Ability.ONE_ADDITIONAL_DAMAGE_VERSUS_WATER,
                                         Enumerators.CardKind.CREATURE,
                                          Enumerators.AbilityType.MODIFICATOR_STAT_VERSUS,
                             Enumerators.AbilityActivityType.PASSIVE,
                             Enumerators.AbilityCallType.PERMANENT,
                             new List<Enumerators.AbilityTargetType>()
                                    { Enumerators.AbilityTargetType.NONE },
                             Enumerators.StatType.DAMAGE,
                             Enumerators.SetType.WATER,
                             1));

            _abilities.Add(new DeactivateTargetAbility(Enumerators.Ability.FREEZE_ENEMY_1_TURN,
                                         Enumerators.CardKind.CREATURE,
                                         Enumerators.AbilityType.STUN,
                                         Enumerators.AbilityActivityType.PASSIVE,
                                         Enumerators.AbilityCallType.AT_ATTACK,
                                         new List<Enumerators.AbilityTargetType>()
                                                { Enumerators.AbilityTargetType.OPPONENT_CARD },
                                         1));
            // not working properly
            _abilities.Add(new DeactivateTargetAbility(Enumerators.Ability.FREEZE_TARGET_1_TURN,
                                         Enumerators.CardKind.CREATURE,
                                          Enumerators.AbilityType.STUN,
                             Enumerators.AbilityActivityType.ACTIVE,
                             Enumerators.AbilityCallType.AT_START,
                             new List<Enumerators.AbilityTargetType>()
                                    { Enumerators.AbilityTargetType.OPPONENT_CARD,
                                      Enumerators.AbilityTargetType.PLAYER_CARD },
                             1));
            // not working properly
            _abilities.Add(new DeactivateTargetAbility(Enumerators.Ability.ENTANGLES_TARGET_DISABLE_UNTILL_REST_OF_TURN,
                                         Enumerators.CardKind.CREATURE,
                                          Enumerators.AbilityType.STUN,
                             Enumerators.AbilityActivityType.ACTIVE,
                             Enumerators.AbilityCallType.AT_START,
                             new List<Enumerators.AbilityTargetType>()
                                    { Enumerators.AbilityTargetType.OPPONENT_CARD,
                                      Enumerators.AbilityTargetType.PLAYER_CARD },
                             1));


            _abilities.Add(new AddGooVialsAbility(Enumerators.Ability.ADDS_2_FULL_GOO_VIALS,
                              Enumerators.CardKind.SPELL,
                              Enumerators.AbilityType.ADD_GOO_VIAL,
                             Enumerators.AbilityActivityType.PASSIVE,
                             Enumerators.AbilityCallType.AT_START,
                             new List<Enumerators.AbilityTargetType>()
                                    { Enumerators.AbilityTargetType.PLAYER },
                             2));
        }
        #endregion fill abilities
    }


    public class ActiveAbility
    {
        public ulong id;
        public AbilityBase ability;
    }
}