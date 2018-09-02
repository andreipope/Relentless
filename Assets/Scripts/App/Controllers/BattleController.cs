// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/

using System.Collections.Generic;
using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;

namespace LoomNetwork.CZB
{
    public class BattleController : IController
    {
        private IGameplayManager _gameplayManager;

        private ITutorialManager _tutorialManager;

        private ActionsQueueController _actionsQueueController;

        private AbilitiesController _abilitiesController;

        private VFXController _vfxController;

        private Dictionary<Enumerators.SetType, Enumerators.SetType> _strongerElemental, _weakerElemental;

        public void Dispose()
        {
        }

        public void Init()
        {
            _gameplayManager = GameClient.Get<IGameplayManager>();
            _tutorialManager = GameClient.Get<ITutorialManager>();

            _actionsQueueController = _gameplayManager.GetController<ActionsQueueController>();
            _abilitiesController = _gameplayManager.GetController<AbilitiesController>();
            _vfxController = _gameplayManager.GetController<VFXController>();

            FillStrongersAndWeakers();
        }

        public void Update()
        {
        }

        public void ResetAll()
        {
        }

        public void AttackPlayerByUnit(BoardUnit attackingUnit, Player attackedPlayer)
        {
            int damageAttacking = attackingUnit.CurrentDamage;

            if ((attackingUnit != null) && (attackedPlayer != null))
            {
                attackedPlayer.HP -= damageAttacking;
            }

            attackingUnit.ThrowOnAttackEvent(attackedPlayer, damageAttacking, true);

            _vfxController.SpawnGotDamageEffect(attackedPlayer, -damageAttacking);

            _tutorialManager.ReportAction(Enumerators.TutorialReportAction.ATTACK_CARD_HERO);

            _actionsQueueController.PostGameActionReport(_actionsQueueController.FormatGameActionReport(Enumerators.ActionType.ATTACK_PLAYER_BY_CREATURE, new object[] { attackingUnit, attackedPlayer }));
        }

        public void AttackUnitByUnit(BoardUnit attackingUnit, BoardUnit attackedUnit, int additionalDamage = 0)
        {
            int damageAttacked = 0;
            int damageAttacking = 0;

            if ((attackingUnit != null) && (attackedUnit != null))
            {
                int additionalDamageAttacker = _abilitiesController.GetStatModificatorByAbility(attackingUnit, attackedUnit, true);
                int additionalDamageAttacked = _abilitiesController.GetStatModificatorByAbility(attackedUnit, attackingUnit, false);

                // Removed for now
                // additionalDamageAttacker += GetStrongersAndWeakersModifier(attackingUnit.Card.libraryCard.cardSetType, attackedUnit.Card.libraryCard.cardSetType);
                // additionalDamageAttacked += GetStrongersAndWeakersModifier(attackedUnit.Card.libraryCard.cardSetType, attackingUnit.Card.libraryCard.cardSetType);
                damageAttacking = attackingUnit.CurrentDamage + additionalDamageAttacker + additionalDamage;

                if ((damageAttacking > 0) && attackedUnit.HasBuffShield)
                {
                    damageAttacking = 0;
                    attackedUnit.UseShieldFromBuff();
                }

                attackedUnit.CurrentHP -= damageAttacking;

                _vfxController.SpawnGotDamageEffect(attackedUnit, -damageAttacking);

                // if (damageAttacking > 0)
                attackedUnit.ThrowEventGotDamage(attackingUnit);
                attackingUnit.ThrowOnAttackEvent(attackedUnit, damageAttacking, true);

                if (((attackedUnit.CurrentHP > 0) && attackingUnit.AttackAsFirst) || !attackingUnit.AttackAsFirst)
                {
                    damageAttacked = attackedUnit.CurrentDamage + additionalDamageAttacked;

                    if ((damageAttacked > 0) && attackingUnit.HasBuffShield)
                    {
                        damageAttacked = 0;
                        attackingUnit.UseShieldFromBuff();
                    }

                    attackingUnit.CurrentHP -= damageAttacked;

                    _vfxController.SpawnGotDamageEffect(attackingUnit, -damageAttacked);

                    // if (damageAttacked > 0)
                    attackingUnit.ThrowEventGotDamage(attackedUnit);

                    attackedUnit.ThrowOnAttackEvent(attackingUnit, damageAttacked, false);
                }

                _actionsQueueController.PostGameActionReport(_actionsQueueController.FormatGameActionReport(Enumerators.ActionType.ATTACK_CREATURE_BY_CREATURE, new object[] { attackingUnit, damageAttacking, attackedUnit, damageAttacked }));

                _tutorialManager.ReportAction(Enumerators.TutorialReportAction.ATTACK_CARD_CARD);
            }
        }

        public void AttackUnitBySkill(Player attackingPlayer, HeroSkill skill, BoardUnit attackedUnit, int modifier)
        {
            int damage = skill.value + modifier;

            if (attackedUnit != null)
            {
                if ((damage > 0) && attackedUnit.HasBuffShield)
                {
                    damage = 0;
                    attackedUnit.UseShieldFromBuff();
                }

                attackedUnit.CurrentHP -= damage;

                _vfxController.SpawnGotDamageEffect(attackedUnit, -damage);

                // if (damage > 0)
                // attackedUnit.ThrowEventGotDamage(attackingPlayer);
            }

            _actionsQueueController.PostGameActionReport(_actionsQueueController.FormatGameActionReport(Enumerators.ActionType.ATTACK_CREATURE_BY_SKILL, new object[] { attackingPlayer, skill, damage, attackedUnit }));
        }

        public void AttackPlayerBySkill(Player attackingPlayer, HeroSkill skill, Player attackedPlayer)
        {
            if (attackedPlayer != null)
            {
                int damage = skill.value;

                attackedPlayer.HP -= damage;

                _vfxController.SpawnGotDamageEffect(attackedPlayer, -damage);

                _actionsQueueController.PostGameActionReport(_actionsQueueController.FormatGameActionReport(Enumerators.ActionType.ATTACK_PLAYER_BY_SKILL, new object[] { attackingPlayer, skill, attackedPlayer }));
            }
        }

        public void HealPlayerBySkill(Player healingPlayer, HeroSkill skill, Player healedPlayer)
        {
            if (healingPlayer != null)
            {
                // if(healingPlayer.SelfHero.heroElement == Enumerators.SetType.EARTH)
                healedPlayer.HP += skill.value;

                if (skill.overlordSkill != Enumerators.OverlordSkill.HARDEN)
                {
                    if (healingPlayer.HP > Constants.DEFAULT_PLAYER_HP)
                    {
                        healingPlayer.HP = Constants.DEFAULT_PLAYER_HP;
                    }
                }
            }

            _actionsQueueController.PostGameActionReport(_actionsQueueController.FormatGameActionReport(Enumerators.ActionType.HEAL_PLAYER_BY_SKILL, new object[] { healedPlayer, skill, healedPlayer }));
        }

        public void HealUnitBySkill(Player healingPlayer, HeroSkill skill, BoardUnit healedCreature)
        {
            if (healedCreature != null)
            {
                healedCreature.CurrentHP += skill.value;
                if (healedCreature.CurrentHP > healedCreature.MaxCurrentHP)
                {
                    healedCreature.CurrentHP = healedCreature.MaxCurrentHP;
                }
            }

            _actionsQueueController.PostGameActionReport(_actionsQueueController.FormatGameActionReport(Enumerators.ActionType.HEAL_CREATURE_BY_SKILL, new object[] { healingPlayer, skill, healedCreature }));
        }

        public void AttackUnitByAbility(object attacker, AbilityData ability, BoardUnit attackedUnit, int damageOverride = -1)
        {
            int damage = ability.value;

            if (damageOverride > 0)
            {
                damage = damageOverride;
            }

            if (attackedUnit != null)
            {
                if ((damage > 0) && attackedUnit.HasBuffShield)
                {
                    damage = 0;
                    attackedUnit.UseShieldFromBuff();
                }

                attackedUnit.CurrentHP -= damage;

                // if (damage > 0)
                // attackedUnit.ThrowEventGotDamage(attacker);
                _vfxController.SpawnGotDamageEffect(attackedUnit, -damage);

                _actionsQueueController.PostGameActionReport(_actionsQueueController.FormatGameActionReport(Enumerators.ActionType.ATTACK_CREATURE_BY_ABILITY, new[] { attacker, ability, damage, attackedUnit }));
            }
        }

        public void AttackPlayerByAbility(object attacker, AbilityData ability, Player attackedPlayer)
        {
            if (attackedPlayer != null)
            {
                int damage = ability.value;

                attackedPlayer.HP -= damage;

                _vfxController.SpawnGotDamageEffect(attackedPlayer, -damage);

                _actionsQueueController.PostGameActionReport(_actionsQueueController.FormatGameActionReport(Enumerators.ActionType.ATTACK_PLAYER_BY_ABILITY, new[] { attacker, ability, ability.value, attackedPlayer }));
            }
        }

        public void HealPlayerByAbility(object healler, AbilityData ability, Player healedPlayer)
        {
            if (healedPlayer != null)
            {
                healedPlayer.HP += ability.value;
                if (healedPlayer.HP > Constants.DEFAULT_PLAYER_HP)
                {
                    healedPlayer.HP = Constants.DEFAULT_PLAYER_HP;
                }
            }

            _actionsQueueController.PostGameActionReport(_actionsQueueController.FormatGameActionReport(Enumerators.ActionType.HEAL_PLAYER_BY_ABILITY, new[] { healler, ability, ability.value, healedPlayer }));
        }

        public void HealUnitByAbility(object healler, AbilityData ability, BoardUnit healedCreature)
        {
            if (healedCreature != null)
            {
                healedCreature.CurrentHP += ability.value;
                if (healedCreature.CurrentHP > healedCreature.MaxCurrentHP)
                {
                    healedCreature.CurrentHP = healedCreature.MaxCurrentHP;
                }
            }

            _actionsQueueController.PostGameActionReport(_actionsQueueController.FormatGameActionReport(Enumerators.ActionType.HEAL_CREATURE_BY_ABILITY, new[] { healler, ability, ability.value, healedCreature }));
        }

        private void FillStrongersAndWeakers()
        {
            _strongerElemental = new Dictionary<Enumerators.SetType, Enumerators.SetType>
                                 {
                                     { Enumerators.SetType.FIRE, Enumerators.SetType.TOXIC },
                                     { Enumerators.SetType.TOXIC, Enumerators.SetType.LIFE },
                                     { Enumerators.SetType.LIFE, Enumerators.SetType.EARTH },
                                     { Enumerators.SetType.EARTH, Enumerators.SetType.AIR },
                                     { Enumerators.SetType.AIR, Enumerators.SetType.WATER },
                                     { Enumerators.SetType.WATER, Enumerators.SetType.FIRE }
                                 };

            _weakerElemental = new Dictionary<Enumerators.SetType, Enumerators.SetType>
                               {
                                   { Enumerators.SetType.FIRE, Enumerators.SetType.WATER },
                                   { Enumerators.SetType.TOXIC, Enumerators.SetType.FIRE },
                                   { Enumerators.SetType.LIFE, Enumerators.SetType.TOXIC },
                                   { Enumerators.SetType.EARTH, Enumerators.SetType.LIFE },
                                   { Enumerators.SetType.AIR, Enumerators.SetType.EARTH },
                                   { Enumerators.SetType.WATER, Enumerators.SetType.AIR }
                               };
        }

        private int GetStrongersAndWeakersModifier(Enumerators.SetType attackerElement, Enumerators.SetType defenderElement)
        {
            int modifier = 0;

            if (_strongerElemental.ContainsKey(attackerElement) && _strongerElemental[attackerElement].Equals(defenderElement))
            {
                modifier++;
            } else if (_weakerElemental.ContainsKey(attackerElement) && _weakerElemental[attackerElement].Equals(defenderElement))
            {
                modifier--;
            }

            return modifier;
        }
    }
}
