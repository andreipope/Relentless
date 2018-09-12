using System.Collections.Generic;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;

namespace Loom.ZombieBattleground
{
    public class BattleController : IController
    {
        private IGameplayManager _gameplayManager;

        private ITutorialManager _tutorialManager;

        private ActionsQueueController _actionsQueueController;

        private AbilitiesController _abilitiesController;

        private VfxController _vfxController;

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
            _vfxController = _gameplayManager.GetController<VfxController>();

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

            if (attackingUnit != null && attackedPlayer != null)
            {
                attackedPlayer.Health -= damageAttacking;
            }

            attackingUnit.InvokeUnitAttacked(attackedPlayer, damageAttacking, true);

            _vfxController.SpawnGotDamageEffect(attackedPlayer, -damageAttacking);

            _tutorialManager.ReportAction(Enumerators.TutorialReportAction.ATTACK_CARD_HERO);

            _actionsQueueController.PostGameActionReport(_actionsQueueController.FormatGameActionReport(
                Enumerators.ActionType.ATTACK_PLAYER_BY_CREATURE, new object[]
                {
                    attackingUnit, attackedPlayer
                }));
        }

        public void AttackUnitByUnit(BoardUnit attackingUnit, BoardUnit attackedUnit, int additionalDamage = 0)
        {
            int damageAttacked = 0;
            int damageAttacking;

            if (attackingUnit != null && attackedUnit != null)
            {
                int additionalDamageAttacker =
                    _abilitiesController.GetStatModificatorByAbility(attackingUnit, attackedUnit, true);
                int additionalDamageAttacked =
                    _abilitiesController.GetStatModificatorByAbility(attackedUnit, attackingUnit, false);

                damageAttacking = attackingUnit.CurrentDamage + additionalDamageAttacker + additionalDamage;

                if (damageAttacking > 0 && attackedUnit.HasBuffShield)
                {
                    damageAttacking = 0;
                    attackedUnit.HasUsedBuffShield = true;
                }

                attackedUnit.CurrentHp -= damageAttacking;

                _vfxController.SpawnGotDamageEffect(attackedUnit, -damageAttacking);

                attackedUnit.InvokeUnitDamaged(attackingUnit);
                attackingUnit.InvokeUnitAttacked(attackedUnit, damageAttacking, true);

                if (attackedUnit.CurrentHp > 0 && attackingUnit.AttackAsFirst || !attackingUnit.AttackAsFirst)
                {
                    damageAttacked = attackedUnit.CurrentDamage + additionalDamageAttacked;

                    if (damageAttacked > 0 && attackingUnit.HasBuffShield)
                    {
                        damageAttacked = 0;
                        attackingUnit.HasUsedBuffShield = true;
                    }

                    attackingUnit.CurrentHp -= damageAttacked;

                    _vfxController.SpawnGotDamageEffect(attackingUnit, -damageAttacked);

                    attackingUnit.InvokeUnitDamaged(attackedUnit);
                    attackedUnit.InvokeUnitAttacked(attackingUnit, damageAttacked, false);
                }

                _actionsQueueController.PostGameActionReport(_actionsQueueController.FormatGameActionReport(
                    Enumerators.ActionType.ATTACK_CREATURE_BY_CREATURE, new object[]
                    {
                        attackingUnit, damageAttacking, attackedUnit, damageAttacked
                    }));

                _tutorialManager.ReportAction(Enumerators.TutorialReportAction.ATTACK_CARD_CARD);
            }
        }

        public void AttackUnitBySkill(Player attackingPlayer, HeroSkill skill, BoardUnit attackedUnit, int modifier)
        {
            int damage = skill.Value + modifier;

            if (attackedUnit != null)
            {
                if (damage > 0 && attackedUnit.HasBuffShield)
                {
                    damage = 0;
                    attackedUnit.UseShieldFromBuff();
                }

                attackedUnit.CurrentHp -= damage;

                _vfxController.SpawnGotDamageEffect(attackedUnit, -damage);
            }

            _actionsQueueController.PostGameActionReport(_actionsQueueController.FormatGameActionReport(
                Enumerators.ActionType.ATTACK_CREATURE_BY_SKILL, new object[]
                {
                    attackingPlayer, skill, damage, attackedUnit
                }));
        }

        public void AttackPlayerBySkill(Player attackingPlayer, HeroSkill skill, Player attackedPlayer)
        {
            if (attackedPlayer != null)
            {
                int damage = skill.Value;

                attackedPlayer.Health -= damage;

                _vfxController.SpawnGotDamageEffect(attackedPlayer, -damage);

                _actionsQueueController.PostGameActionReport(_actionsQueueController.FormatGameActionReport(
                    Enumerators.ActionType.ATTACK_PLAYER_BY_SKILL, new object[]
                    {
                        attackingPlayer, skill, attackedPlayer
                    }));
            }
        }

        public void HealPlayerBySkill(Player healingPlayer, HeroSkill skill, Player healedPlayer)
        {
            if (healingPlayer != null)
            {
                healedPlayer.Health += skill.Value;

                if (skill.OverlordSkill != Enumerators.OverlordSkill.HARDEN)
                {
                    if (healingPlayer.Health > Constants.DefaultPlayerHp)
                    {
                        healingPlayer.Health = Constants.DefaultPlayerHp;
                    }
                }
            }

            _actionsQueueController.PostGameActionReport(_actionsQueueController.FormatGameActionReport(
                Enumerators.ActionType.HEAL_PLAYER_BY_SKILL, new object[]
                {
                    healedPlayer, skill, healedPlayer
                }));
        }

        public void HealUnitBySkill(Player healingPlayer, HeroSkill skill, BoardUnit healedCreature)
        {
            if (healedCreature != null)
            {
                healedCreature.CurrentHp += skill.Value;
                if (healedCreature.CurrentHp > healedCreature.MaxCurrentHp)
                {
                    healedCreature.CurrentHp = healedCreature.MaxCurrentHp;
                }
            }

            _actionsQueueController.PostGameActionReport(_actionsQueueController.FormatGameActionReport(
                Enumerators.ActionType.HEAL_CREATURE_BY_SKILL, new object[]
                {
                    healingPlayer, skill, healedCreature
                }));
        }

        public void AttackUnitByAbility(
            object attacker, AbilityData ability, BoardUnit attackedUnit, int damageOverride = -1)
        {
            int damage = ability.Value;

            if (damageOverride > 0)
            {
                damage = damageOverride;
            }

            if (attackedUnit != null)
            {
                if (damage > 0 && attackedUnit.HasBuffShield)
                {
                    damage = 0;
                    attackedUnit.UseShieldFromBuff();
                }

                attackedUnit.CurrentHp -= damage;

                _vfxController.SpawnGotDamageEffect(attackedUnit, -damage);
                _actionsQueueController.PostGameActionReport(_actionsQueueController.FormatGameActionReport(
                    Enumerators.ActionType.ATTACK_CREATURE_BY_ABILITY, new[]
                    {
                        attacker, ability, damage, attackedUnit
                    }));
            }
        }

        public void AttackPlayerByAbility(object attacker, AbilityData ability, Player attackedPlayer)
        {
            if (attackedPlayer != null)
            {
                int damage = ability.Value;

                attackedPlayer.Health -= damage;

                _vfxController.SpawnGotDamageEffect(attackedPlayer, -damage);

                _actionsQueueController.PostGameActionReport(_actionsQueueController.FormatGameActionReport(
                    Enumerators.ActionType.ATTACK_PLAYER_BY_ABILITY, new[]
                    {
                        attacker, ability, ability.Value, attackedPlayer
                    }));
            }
        }

        public void HealPlayerByAbility(object healler, AbilityData ability, Player healedPlayer, int value = -1)
        {
            int healValue = ability.Value;

            if (value > 0)
                healValue = value;

            if (healedPlayer != null)
            {
                healedPlayer.Health += healValue;
                if (healedPlayer.Health > Constants.DefaultPlayerHp)
                {
                    healedPlayer.Health = Constants.DefaultPlayerHp;
                }
            }

            _actionsQueueController.PostGameActionReport(_actionsQueueController.FormatGameActionReport(
                Enumerators.ActionType.HEAL_PLAYER_BY_ABILITY, new[]
                {
                    healler, ability, healValue, healedPlayer
                }));
        }

        public void HealUnitByAbility(object healler, AbilityData ability, BoardUnit healedCreature, int value = -1)
        {
            int healValue = ability.Value;

            if (value > 0)
                healValue = value;

            if (healedCreature != null)
            {
                healedCreature.CurrentHp += healValue;
                if (healedCreature.CurrentHp > healedCreature.MaxCurrentHp)
                {
                    healedCreature.CurrentHp = healedCreature.MaxCurrentHp;
                }
            }

            _actionsQueueController.PostGameActionReport(_actionsQueueController.FormatGameActionReport(
                Enumerators.ActionType.HEAL_CREATURE_BY_ABILITY, new[]
                {
                    healler, ability, healValue, healedCreature
                }));
        }

        private void FillStrongersAndWeakers()
        {
            _strongerElemental = new Dictionary<Enumerators.SetType, Enumerators.SetType>
            {
                {
                    Enumerators.SetType.FIRE, Enumerators.SetType.TOXIC
                },
                {
                    Enumerators.SetType.TOXIC, Enumerators.SetType.LIFE
                },
                {
                    Enumerators.SetType.LIFE, Enumerators.SetType.EARTH
                },
                {
                    Enumerators.SetType.EARTH, Enumerators.SetType.AIR
                },
                {
                    Enumerators.SetType.AIR, Enumerators.SetType.WATER
                },
                {
                    Enumerators.SetType.WATER, Enumerators.SetType.FIRE
                }
            };

            _weakerElemental = new Dictionary<Enumerators.SetType, Enumerators.SetType>
            {
                {
                    Enumerators.SetType.FIRE, Enumerators.SetType.WATER
                },
                {
                    Enumerators.SetType.TOXIC, Enumerators.SetType.FIRE
                },
                {
                    Enumerators.SetType.LIFE, Enumerators.SetType.TOXIC
                },
                {
                    Enumerators.SetType.EARTH, Enumerators.SetType.LIFE
                },
                {
                    Enumerators.SetType.AIR, Enumerators.SetType.EARTH
                },
                {
                    Enumerators.SetType.WATER, Enumerators.SetType.AIR
                }
            };
        }
    }
}
