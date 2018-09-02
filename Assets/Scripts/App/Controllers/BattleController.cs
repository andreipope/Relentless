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

            if ((attackingUnit != null) && (attackedPlayer != null))
            {
                attackedPlayer.Hp -= damageAttacking;
            }

            attackingUnit.ThrowOnAttackEvent(attackedPlayer, damageAttacking, true);

            _vfxController.SpawnGotDamageEffect(attackedPlayer, -damageAttacking);

            _tutorialManager.ReportAction(Enumerators.TutorialReportAction.AttackCardHero);

            _actionsQueueController.PostGameActionReport(_actionsQueueController.FormatGameActionReport(Enumerators.ActionType.AttackPlayerByCreature, new object[] { attackingUnit, attackedPlayer }));
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

                attackedUnit.CurrentHp -= damageAttacking;

                _vfxController.SpawnGotDamageEffect(attackedUnit, -damageAttacking);

                // if (damageAttacking > 0)
                attackedUnit.ThrowEventGotDamage(attackingUnit);
                attackingUnit.ThrowOnAttackEvent(attackedUnit, damageAttacking, true);

                if (((attackedUnit.CurrentHp > 0) && attackingUnit.AttackAsFirst) || !attackingUnit.AttackAsFirst)
                {
                    damageAttacked = attackedUnit.CurrentDamage + additionalDamageAttacked;

                    if ((damageAttacked > 0) && attackingUnit.HasBuffShield)
                    {
                        damageAttacked = 0;
                        attackingUnit.UseShieldFromBuff();
                    }

                    attackingUnit.CurrentHp -= damageAttacked;

                    _vfxController.SpawnGotDamageEffect(attackingUnit, -damageAttacked);

                    // if (damageAttacked > 0)
                    attackingUnit.ThrowEventGotDamage(attackedUnit);

                    attackedUnit.ThrowOnAttackEvent(attackingUnit, damageAttacked, false);
                }

                _actionsQueueController.PostGameActionReport(_actionsQueueController.FormatGameActionReport(Enumerators.ActionType.AttackCreatureByCreature, new object[] { attackingUnit, damageAttacking, attackedUnit, damageAttacked }));

                _tutorialManager.ReportAction(Enumerators.TutorialReportAction.AttackCardCard);
            }
        }

        public void AttackUnitBySkill(Player attackingPlayer, HeroSkill skill, BoardUnit attackedUnit, int modifier)
        {
            int damage = skill.Value + modifier;

            if (attackedUnit != null)
            {
                if ((damage > 0) && attackedUnit.HasBuffShield)
                {
                    damage = 0;
                    attackedUnit.UseShieldFromBuff();
                }

                attackedUnit.CurrentHp -= damage;

                _vfxController.SpawnGotDamageEffect(attackedUnit, -damage);

                // if (damage > 0)
                // attackedUnit.ThrowEventGotDamage(attackingPlayer);
            }

            _actionsQueueController.PostGameActionReport(_actionsQueueController.FormatGameActionReport(Enumerators.ActionType.AttackCreatureBySkill, new object[] { attackingPlayer, skill, damage, attackedUnit }));
        }

        public void AttackPlayerBySkill(Player attackingPlayer, HeroSkill skill, Player attackedPlayer)
        {
            if (attackedPlayer != null)
            {
                int damage = skill.Value;

                attackedPlayer.Hp -= damage;

                _vfxController.SpawnGotDamageEffect(attackedPlayer, -damage);

                _actionsQueueController.PostGameActionReport(_actionsQueueController.FormatGameActionReport(Enumerators.ActionType.AttackPlayerBySkill, new object[] { attackingPlayer, skill, attackedPlayer }));
            }
        }

        public void HealPlayerBySkill(Player healingPlayer, HeroSkill skill, Player healedPlayer)
        {
            if (healingPlayer != null)
            {
                // if(healingPlayer.SelfHero.heroElement == Enumerators.SetType.EARTH)
                healedPlayer.Hp += skill.Value;

                if (skill.OverlordSkill != Enumerators.OverlordSkill.Harden)
                {
                    if (healingPlayer.Hp > Constants.DefaultPlayerHp)
                    {
                        healingPlayer.Hp = Constants.DefaultPlayerHp;
                    }
                }
            }

            _actionsQueueController.PostGameActionReport(_actionsQueueController.FormatGameActionReport(Enumerators.ActionType.HealPlayerBySkill, new object[] { healedPlayer, skill, healedPlayer }));
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

            _actionsQueueController.PostGameActionReport(_actionsQueueController.FormatGameActionReport(Enumerators.ActionType.HealCreatureBySkill, new object[] { healingPlayer, skill, healedCreature }));
        }

        public void AttackUnitByAbility(object attacker, AbilityData ability, BoardUnit attackedUnit, int damageOverride = -1)
        {
            int damage = ability.Value;

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

                attackedUnit.CurrentHp -= damage;

                // if (damage > 0)
                // attackedUnit.ThrowEventGotDamage(attacker);
                _vfxController.SpawnGotDamageEffect(attackedUnit, -damage);

                _actionsQueueController.PostGameActionReport(_actionsQueueController.FormatGameActionReport(Enumerators.ActionType.AttackCreatureByAbility, new[] { attacker, ability, damage, attackedUnit }));
            }
        }

        public void AttackPlayerByAbility(object attacker, AbilityData ability, Player attackedPlayer)
        {
            if (attackedPlayer != null)
            {
                int damage = ability.Value;

                attackedPlayer.Hp -= damage;

                _vfxController.SpawnGotDamageEffect(attackedPlayer, -damage);

                _actionsQueueController.PostGameActionReport(_actionsQueueController.FormatGameActionReport(Enumerators.ActionType.AttackPlayerByAbility, new[] { attacker, ability, ability.Value, attackedPlayer }));
            }
        }

        public void HealPlayerByAbility(object healler, AbilityData ability, Player healedPlayer)
        {
            if (healedPlayer != null)
            {
                healedPlayer.Hp += ability.Value;
                if (healedPlayer.Hp > Constants.DefaultPlayerHp)
                {
                    healedPlayer.Hp = Constants.DefaultPlayerHp;
                }
            }

            _actionsQueueController.PostGameActionReport(_actionsQueueController.FormatGameActionReport(Enumerators.ActionType.HealPlayerByAbility, new[] { healler, ability, ability.Value, healedPlayer }));
        }

        public void HealUnitByAbility(object healler, AbilityData ability, BoardUnit healedCreature)
        {
            if (healedCreature != null)
            {
                healedCreature.CurrentHp += ability.Value;
                if (healedCreature.CurrentHp > healedCreature.MaxCurrentHp)
                {
                    healedCreature.CurrentHp = healedCreature.MaxCurrentHp;
                }
            }

            _actionsQueueController.PostGameActionReport(_actionsQueueController.FormatGameActionReport(Enumerators.ActionType.HealCreatureByAbility, new[] { healler, ability, ability.Value, healedCreature }));
        }

        private void FillStrongersAndWeakers()
        {
            _strongerElemental = new Dictionary<Enumerators.SetType, Enumerators.SetType>
            {
                { Enumerators.SetType.Fire, Enumerators.SetType.Toxic },
                { Enumerators.SetType.Toxic, Enumerators.SetType.Life },
                { Enumerators.SetType.Life, Enumerators.SetType.Earth },
                { Enumerators.SetType.Earth, Enumerators.SetType.Air },
                { Enumerators.SetType.Air, Enumerators.SetType.Water },
                { Enumerators.SetType.Water, Enumerators.SetType.Fire }
            };

            _weakerElemental = new Dictionary<Enumerators.SetType, Enumerators.SetType>
            {
                { Enumerators.SetType.Fire, Enumerators.SetType.Water },
                { Enumerators.SetType.Toxic, Enumerators.SetType.Fire },
                { Enumerators.SetType.Life, Enumerators.SetType.Toxic },
                { Enumerators.SetType.Earth, Enumerators.SetType.Life },
                { Enumerators.SetType.Air, Enumerators.SetType.Earth },
                { Enumerators.SetType.Water, Enumerators.SetType.Air }
            };
        }

        private int GetStrongersAndWeakersModifier(Enumerators.SetType attackerElement, Enumerators.SetType defenderElement)
        {
            int modifier = 0;

            if (_strongerElemental.ContainsKey(attackerElement) && _strongerElemental[attackerElement].Equals(defenderElement))
            {
                modifier++;
            }
            else if (_weakerElemental.ContainsKey(attackerElement) && _weakerElemental[attackerElement].Equals(defenderElement))
            {
                modifier--;
            }

            return modifier;
        }
    }
}
