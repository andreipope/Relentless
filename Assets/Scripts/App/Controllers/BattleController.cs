using System;
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

        private BattlegroundController _battlegroundController;

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
            _battlegroundController = _gameplayManager.GetController<BattlegroundController>();

            FillStrongersAndWeakers();
        }

        public void Update()
        {
        }

        public void ResetAll()
        {
        }

        public void AttackPlayerByUnit(BoardUnitModel attackingUnitModel, Player attackedPlayer)
        {
            int damageAttacking = attackingUnitModel.CurrentDamage;

            if (attackingUnitModel != null && attackedPlayer != null)
            {
                attackedPlayer.Health -= damageAttacking;
            }

            attackingUnitModel.InvokeUnitAttacked(attackedPlayer, damageAttacking, true);

            _vfxController.SpawnGotDamageEffect(attackedPlayer, -damageAttacking);

            _tutorialManager.ReportAction(Enumerators.TutorialReportAction.ATTACK_CARD_HERO);

            _actionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
            {
                ActionType = Enumerators.ActionType.CardAttackOverlord,
                Caller = attackingUnitModel,
                TargetEffects = new List<PastActionsPopup.TargetEffectParam>()
                {
                    new PastActionsPopup.TargetEffectParam()
                    {
                        ActionEffectType = Enumerators.ActionEffectType.ShieldDebuff,
                        Target = attackedPlayer,
                        HasValue = true,
                        Value = -damageAttacking
                    }
                }
            });
        }

        public void AttackUnitByUnit(BoardUnitModel attackingUnitModel, BoardUnitModel attackedUnitModel, int additionalDamage = 0)
        {
            int damageAttacked = 0;
            int damageAttacking;

            if (attackingUnitModel != null && attackedUnitModel != null)
            {
                int additionalDamageAttacker =
                    _abilitiesController.GetStatModificatorByAbility(attackingUnitModel, attackedUnitModel, true);
                int additionalDamageAttacked =
                    _abilitiesController.GetStatModificatorByAbility(attackedUnitModel, attackingUnitModel, false);

                damageAttacking = attackingUnitModel.CurrentDamage + additionalDamageAttacker + additionalDamage;

                if (damageAttacking > 0 && attackedUnitModel.HasBuffShield)
                {
                    damageAttacking = 0;
                    attackedUnitModel.HasUsedBuffShield = true;
                }

                attackedUnitModel.LastAttackingSetType = attackingUnitModel.Card.LibraryCard.CardSetType;//LastAttackingUnit = attackingUnit;
                attackedUnitModel.CurrentHp -= damageAttacking;

                CheckOnKillEnemyZombie(attackedUnitModel);

                _vfxController.SpawnGotDamageEffect(_battlegroundController.GetBoardUnitViewByModel(attackedUnitModel), -damageAttacking);

                attackedUnitModel.InvokeUnitDamaged(attackingUnitModel);
                attackingUnitModel.InvokeUnitAttacked(attackedUnitModel, damageAttacking, true);

                if (attackedUnitModel.CurrentHp > 0 && attackingUnitModel.AttackAsFirst || !attackingUnitModel.AttackAsFirst)
                {
                    damageAttacked = attackedUnitModel.CurrentDamage + additionalDamageAttacked;

                    if (damageAttacked > 0 && attackingUnitModel.HasBuffShield)
                    {
                        damageAttacked = 0;
                        attackingUnitModel.HasUsedBuffShield = true;
                    }

                    attackingUnitModel.LastAttackingSetType = attackedUnitModel.Card.LibraryCard.CardSetType;
                    attackingUnitModel.CurrentHp -= damageAttacked;

                    _vfxController.SpawnGotDamageEffect(_battlegroundController.GetBoardUnitViewByModel(attackingUnitModel), -damageAttacked);

                    attackingUnitModel.InvokeUnitDamaged(attackedUnitModel);
                    attackedUnitModel.InvokeUnitAttacked(attackingUnitModel, damageAttacked, false);
                }

                _actionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
                    {
                    ActionType = Enumerators.ActionType.CardAttackCard,
                    Caller = attackingUnitModel,
                    TargetEffects = new List<PastActionsPopup.TargetEffectParam>()
                    {
                        new PastActionsPopup.TargetEffectParam()
                        {
                            ActionEffectType = Enumerators.ActionEffectType.ShieldDebuff,
                            Target = attackedUnitModel,
                            HasValue = true,
                            Value = -damageAttacking
                        }
                    }
                });

                _tutorialManager.ReportAction(Enumerators.TutorialReportAction.ATTACK_CARD_CARD);
            }
        }

        public void AttackUnitBySkill(Player attackingPlayer, BoardSkill skill, BoardUnitModel attackedUnitModel, int modifier)
        {
            int damage = skill.Skill.Value + modifier;

            if (attackedUnitModel != null)
            {
                if (damage > 0 && attackedUnitModel.HasBuffShield)
                {
                    damage = 0;
                    attackedUnitModel.UseShieldFromBuff();
                }
                attackedUnitModel.LastAttackingSetType = attackingPlayer.SelfHero.HeroElement;
                attackedUnitModel.CurrentHp -= damage;

                CheckOnKillEnemyZombie(attackedUnitModel);

                _vfxController.SpawnGotDamageEffect(_battlegroundController.GetBoardUnitViewByModel(attackedUnitModel), -damage);
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

        public void HealUnitBySkill(Player healingPlayer, BoardSkill skill, BoardUnitModel healedCreature)
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
            object attacker, AbilityData ability, BoardUnitModel attackedUnitModel, int damageOverride = -1)
        {
            int damage = ability.Value;

            if (damageOverride > 0)
            {
                damage = damageOverride;
            }

            if (attackedUnitModel != null)
            {
                if (damage > 0 && attackedUnitModel.HasBuffShield)
                {
                    damage = 0;
                    attackedUnitModel.UseShieldFromBuff();
                }

                switch (attacker)
                {
                    case BoardUnitModel model:
                        attackedUnitModel.LastAttackingSetType = model.Card.LibraryCard.CardSetType;
                        break;
                    case BoardSpell spell:
                        attackedUnitModel.LastAttackingSetType = spell.Card.LibraryCard.CardSetType;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(attacker), attacker, null);
                }

                attackedUnitModel.CurrentHp -= damage;
                CheckOnKillEnemyZombie(attackedUnitModel);
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

        public void HealUnitByAbility(object healler, AbilityData ability, BoardUnitModel healedCreature, int value = -1)
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

        public void CheckOnKillEnemyZombie(BoardUnitModel attackedUnit)
        {
            if (!attackedUnit.OwnerPlayer.IsLocalPlayer && attackedUnit.CurrentHp == 0)
            {
                GameClient.Get<IOverlordManager>().ReportExperienceAction(_gameplayManager.CurrentPlayer.SelfHero, Common.Enumerators.ExperienceActionType.KillMinion);
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
