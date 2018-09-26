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

            _actionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
            {
                ActionType = Enumerators.ActionType.CardAttackOverlord,
                Caller = attackingUnit,
                TargetEffects = new List<PastActionsPopup.TargetEffectParam>()
                {
                    new PastActionsPopup.TargetEffectParam()
                    {
                        ActionEffectType = Enumerators.ActionEffectType.ShieldDebuff,
                        Target = attackedPlayer
                    }
                }
            });
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

                attackedUnit.LastAttackingSetType = attackingUnit.Card.LibraryCard.CardSetType;//LastAttackingUnit = attackingUnit;
                attackedUnit.CurrentHp -= damageAttacking;

                CheckOnKillEnemyZombie(attackedUnit);

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

                    attackingUnit.LastAttackingSetType = attackedUnit.Card.LibraryCard.CardSetType;
                    attackingUnit.CurrentHp -= damageAttacked;

                    _vfxController.SpawnGotDamageEffect(attackingUnit, -damageAttacked);

                    attackingUnit.InvokeUnitDamaged(attackedUnit);
                    attackedUnit.InvokeUnitAttacked(attackingUnit, damageAttacked, false);
                }

                _actionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
                {
                    ActionType = Enumerators.ActionType.CardAttackCard,
                    Caller = attackingUnit,
                    TargetEffects = new List<PastActionsPopup.TargetEffectParam>()
                    {
                        new PastActionsPopup.TargetEffectParam()
                        {
                            ActionEffectType = Enumerators.ActionEffectType.ShieldDebuff,
                            Target = attackedUnit
                        }
                    }
                });

                _tutorialManager.ReportAction(Enumerators.TutorialReportAction.ATTACK_CARD_CARD);
            }
        }

        public void AttackUnitBySkill(Player attackingPlayer, BoardSkill skill, BoardUnit attackedUnit, int modifier)
        {
            int damage = skill.Skill.Value + modifier;

            if (attackedUnit != null)
            {
                if (damage > 0 && attackedUnit.HasBuffShield)
                {
                    damage = 0;
                    attackedUnit.UseShieldFromBuff();
                }
                attackedUnit.LastAttackingSetType = attackingPlayer.SelfHero.HeroElement;
                attackedUnit.CurrentHp -= damage;

                CheckOnKillEnemyZombie(attackedUnit);

                _vfxController.SpawnGotDamageEffect(attackedUnit, -damage);
            }
        }

        public void AttackPlayerBySkill(Player attackingPlayer, BoardSkill skill, Player attackedPlayer)
        {
            if (attackedPlayer != null)
            {
                int damage = skill.Skill.Value;

                attackedPlayer.Health -= damage;

                _vfxController.SpawnGotDamageEffect(attackedPlayer, -damage);
            }
        }

        public void HealPlayerBySkill(Player healingPlayer, BoardSkill skill, Player healedPlayer)
        {
            if (healingPlayer != null)
            {
                healedPlayer.Health += skill.Skill.Value;

                if (skill.Skill.OverlordSkill != Enumerators.OverlordSkill.HARDEN ||
                    skill.Skill.OverlordSkill != Enumerators.OverlordSkill.ICE_WALL)
                {
                    if (healingPlayer.Health > Constants.DefaultPlayerHp)
                    {
                        healingPlayer.Health = Constants.DefaultPlayerHp;
                    }
                }
            }
        }

        public void HealUnitBySkill(Player healingPlayer, BoardSkill skill, BoardUnit healedCreature)
        {
            if (healedCreature != null)
            {
                healedCreature.CurrentHp += skill.Skill.Value;
                if (healedCreature.CurrentHp > healedCreature.MaxCurrentHp)
                {
                    healedCreature.CurrentHp = healedCreature.MaxCurrentHp;
                }
            }
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
                if (attacker is BoardUnit)
                    attackedUnit.LastAttackingSetType = (attacker as BoardUnit).Card.LibraryCard.CardSetType;
                else if(attacker is BoardSpell)
                    attackedUnit.LastAttackingSetType = (attacker as BoardSpell).Card.LibraryCard.CardSetType;

                attackedUnit.CurrentHp -= damage;

                 CheckOnKillEnemyZombie(attackedUnit);
            }
        }

        public void AttackPlayerByAbility(object attacker, AbilityData ability, Player attackedPlayer)
        {
            if (attackedPlayer != null)
            {
                int damage = ability.Value;

                attackedPlayer.Health -= damage;

                _vfxController.SpawnGotDamageEffect(attackedPlayer, -damage);
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
        }

        public void CheckOnKillEnemyZombie(BoardUnit attackedUnit)
        {
            if (!attackedUnit.OwnerPlayer.IsLocalPlayer && attackedUnit.CurrentHp == 0)
            {
                GameClient.Get<IOverlordManager>().ReportXPAction(_gameplayManager.CurrentPlayer.SelfHero, Common.Enumerators.XPActionType.KillMinion);
            }
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
