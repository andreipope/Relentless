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

        private Dictionary<Enumerators.Faction, Enumerators.Faction> _strongerElemental, _weakerElemental;

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
                attackedPlayer.Defense -= damageAttacking;
            }

            attackingUnitModel.InvokeUnitAttacked(attackedPlayer, damageAttacking, true);

            _vfxController.SpawnGotDamageEffect(attackedPlayer, -damageAttacking);

            _tutorialManager.ReportActivityAction(Enumerators.TutorialActivityAction.BattleframeAttacked, attackingUnitModel.TutorialObjectId);

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

            if (attackingUnitModel.OwnerPlayer == _gameplayManager.CurrentPlayer)
            {
                _gameplayManager.PlayerMoves.AddPlayerMove(new PlayerMove(Enumerators.PlayerActionType.AttackOnOverlord,
                    new AttackOverlord(attackingUnitModel, attackedPlayer, damageAttacking)));
            }
        }

        public void AttackUnitByUnit(BoardUnitModel attackingUnitModel, BoardUnitModel attackedUnitModel, int additionalDamage = 0, bool hasCounterAttack = true)
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

                attackedUnitModel.LastAttackingSetType = attackingUnitModel.Card.Prototype.Faction;//LastAttackingUnit = attackingUnit;
                attackedUnitModel.CurrentDefense -= damageAttacking;

                CheckOnKillEnemyZombie(attackedUnitModel);

                if (attackedUnitModel.CurrentDefense <= 0)
                {
                    attackingUnitModel.InvokeKilledUnit(attackedUnitModel);
                }

                _vfxController.SpawnGotDamageEffect(_battlegroundController.GetBoardUnitViewByModel<BoardUnitView>(attackedUnitModel), -damageAttacking);

                attackedUnitModel.InvokeUnitDamaged(attackingUnitModel);
                attackingUnitModel.InvokeUnitAttacked(attackedUnitModel, damageAttacking, true);

                if (hasCounterAttack)
                {
                    if (attackedUnitModel.CurrentDefense > 0 && attackingUnitModel.AttackAsFirst || !attackingUnitModel.AttackAsFirst)
                    {
                        damageAttacked = attackedUnitModel.CurrentDamage + additionalDamageAttacked;

                        if (damageAttacked > 0 && attackingUnitModel.HasBuffShield)
                        {
                            damageAttacked = 0;
                            attackingUnitModel.HasUsedBuffShield = true;
                        }

                        attackingUnitModel.LastAttackingSetType = attackedUnitModel.Card.Prototype.Faction;
                        attackingUnitModel.CurrentDefense -= damageAttacked;

                        if (attackingUnitModel.CurrentDefense <= 0)
                        {
                            attackedUnitModel.InvokeKilledUnit(attackingUnitModel);
                        }

                        _vfxController.SpawnGotDamageEffect(_battlegroundController.GetBoardUnitViewByModel<BoardUnitView>(attackingUnitModel), -damageAttacked);

                        attackingUnitModel.InvokeUnitDamaged(attackedUnitModel);
                        attackedUnitModel.InvokeUnitAttacked(attackingUnitModel, damageAttacked, false);
                    }
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

                _tutorialManager.ReportActivityAction(Enumerators.TutorialActivityAction.BattleframeAttacked, attackingUnitModel.TutorialObjectId);

                if (attackingUnitModel.OwnerPlayer == _gameplayManager.CurrentPlayer)
                {
                    _gameplayManager.PlayerMoves.AddPlayerMove(
                        new PlayerMove(
                            Enumerators.PlayerActionType.AttackOnUnit,
                            new AttackUnit(attackingUnitModel, attackedUnitModel, damageAttacked, damageAttacking))
                        );
                }
            }
        }

        public void AttackUnitBySkill(Player attackingPlayer, BoardSkill skill, BoardUnitModel attackedUnitModel, int modifier, int damageOverride = -1)
        {
            if (attackedUnitModel != null)
            {
                int damage = damageOverride != -1 ? damageOverride : skill.Skill.Value + modifier;

                if (damage > 0 && attackedUnitModel.HasBuffShield)
                {
                    damage = 0;
                    attackedUnitModel.UseShieldFromBuff();
                }
                attackedUnitModel.LastAttackingSetType = attackingPlayer.SelfOverlord.Faction;
                attackedUnitModel.CurrentDefense -= damage;

                CheckOnKillEnemyZombie(attackedUnitModel);

                _vfxController.SpawnGotDamageEffect(_battlegroundController.GetBoardUnitViewByModel<BoardUnitView>(attackedUnitModel), -damage);
            }
        }

        public void AttackPlayerBySkill(Player attackingPlayer, BoardSkill skill, Player attackedPlayer, int damageOverride = -1)
        {
            if (attackedPlayer != null)
            {
                int damage = damageOverride != -1 ? damageOverride : skill.Skill.Value;

                attackedPlayer.Defense -= damage;

                _vfxController.SpawnGotDamageEffect(attackedPlayer, -damage);
            }
        }

        public void HealPlayerBySkill(Player healingPlayer, BoardSkill skill, Player healedPlayer)
        {
            if (healingPlayer != null)
            {
                healedPlayer.Defense += skill.Skill.Value;
                if (skill.Skill.Skill != Enumerators.Skill.HARDEN &&
                    skill.Skill.Skill != Enumerators.Skill.ICE_WALL)
                {
                    if (healingPlayer.Defense > Constants.DefaultPlayerHp)
                    {
                        healingPlayer.Defense = Constants.DefaultPlayerHp;
                    }
                }
            }
        }

        public void HealUnitBySkill(Player healingPlayer, BoardSkill skill, BoardUnitModel healedCreature)
        {
            if (healedCreature != null)
            {
                healedCreature.CurrentDefense += skill.Skill.Value;
                if (healedCreature.CurrentDefense > healedCreature.MaxCurrentDefense)
                {
                    healedCreature.CurrentDefense = healedCreature.MaxCurrentDefense;
                }
            }
        }

        public void AttackUnitByAbility(
            object attacker, AbilityData ability, BoardUnitModel attackedUnitModel, int damageOverride = -1)
        {
            int damage = damageOverride != -1 ? damageOverride : ability.Value;

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
                        attackedUnitModel.LastAttackingSetType = model.Card.Prototype.Faction;
                        break;
                    case BoardItem item:
                        attackedUnitModel.LastAttackingSetType = item.Model.Prototype.Faction;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(attacker), attacker, null);
                }

                attackedUnitModel.CurrentDefense -= damage;
                CheckOnKillEnemyZombie(attackedUnitModel);
            }
        }

        public void AttackPlayerByAbility(object attacker, AbilityData ability, Player attackedPlayer, int damageOverride = -1)
        {
            int damage = damageOverride != -1 ? damageOverride : ability.Value;

            AttackPlayer(attackedPlayer, damage);
        }

        public void AttackPlayer(Player attackedPlayer, int damage)
        {
            if (attackedPlayer != null)
            {
                attackedPlayer.Defense -= damage;

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
                healedPlayer.Defense += healValue;
                if (healedPlayer.Defense > Constants.DefaultPlayerHp)
                {
                    healedPlayer.Defense = Constants.DefaultPlayerHp;
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
                healedCreature.CurrentDefense += healValue;
                if (healedCreature.CurrentDefense > healedCreature.MaxCurrentDefense)
                {
                    healedCreature.CurrentDefense = healedCreature.MaxCurrentDefense;
                }
            }
        }

        public void CheckOnKillEnemyZombie(BoardUnitModel attackedUnit)
        {
            if (!attackedUnit.OwnerPlayer.IsLocalPlayer && attackedUnit.CurrentDefense == 0)
            {
                GameClient.Get<IOverlordExperienceManager>().ReportExperienceAction(_gameplayManager.CurrentPlayer.SelfOverlord, Common.Enumerators.ExperienceActionType.KillMinion);
            }
        }

        private void FillStrongersAndWeakers()
        {
            _strongerElemental = new Dictionary<Enumerators.Faction, Enumerators.Faction>
            {
                {
                    Enumerators.Faction.FIRE, Enumerators.Faction.TOXIC
                },
                {
                    Enumerators.Faction.TOXIC, Enumerators.Faction.LIFE
                },
                {
                    Enumerators.Faction.LIFE, Enumerators.Faction.EARTH
                },
                {
                    Enumerators.Faction.EARTH, Enumerators.Faction.AIR
                },
                {
                    Enumerators.Faction.AIR, Enumerators.Faction.WATER
                },
                {
                    Enumerators.Faction.WATER, Enumerators.Faction.FIRE
                }
            };

            _weakerElemental = new Dictionary<Enumerators.Faction, Enumerators.Faction>
            {
                {
                    Enumerators.Faction.FIRE, Enumerators.Faction.WATER
                },
                {
                    Enumerators.Faction.TOXIC, Enumerators.Faction.FIRE
                },
                {
                    Enumerators.Faction.LIFE, Enumerators.Faction.TOXIC
                },
                {
                    Enumerators.Faction.EARTH, Enumerators.Faction.LIFE
                },
                {
                    Enumerators.Faction.AIR, Enumerators.Faction.EARTH
                },
                {
                    Enumerators.Faction.WATER, Enumerators.Faction.AIR
                }
            };
        }
    }
}
