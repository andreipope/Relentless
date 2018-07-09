// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;
using System.Collections.Generic;

namespace LoomNetwork.CZB
{
    public class BattleController : IController
    {
        private IGameplayManager _gameplayManager;
        private ITutorialManager _tutorialManager;

        private ActionsQueueController _actionsQueueController;
        private AbilitiesController _abilitiesController;

        private Dictionary<Enumerators.SetType, Enumerators.SetType> _strongerElemental,
                                                                     _weakerElemental;

        public void Dispose()
        {
        }

        public void Init()
        {
            _gameplayManager = GameClient.Get<IGameplayManager>();
            _tutorialManager = GameClient.Get<ITutorialManager>();

            _actionsQueueController = _gameplayManager.GetController<ActionsQueueController>();
            _abilitiesController = _gameplayManager.GetController<AbilitiesController>();

            FillStrongersAndWeakers();
        }

        public void Update()
        {
        }


        private void FillStrongersAndWeakers()
        {
            _strongerElemental = new Dictionary<Enumerators.SetType, Enumerators.SetType>()
            {
                { Enumerators.SetType.FIRE, Enumerators.SetType.TOXIC },
                { Enumerators.SetType.TOXIC, Enumerators.SetType.LIFE },
                { Enumerators.SetType.LIFE, Enumerators.SetType.EARTH },
                { Enumerators.SetType.EARTH, Enumerators.SetType.AIR },
                { Enumerators.SetType.AIR, Enumerators.SetType.WATER },
                { Enumerators.SetType.WATER, Enumerators.SetType.FIRE },
            };

            _weakerElemental = new Dictionary<Enumerators.SetType, Enumerators.SetType>()
            {
                { Enumerators.SetType.FIRE, Enumerators.SetType.WATER },
                { Enumerators.SetType.TOXIC, Enumerators.SetType.FIRE },
                { Enumerators.SetType.LIFE, Enumerators.SetType.TOXIC },
                { Enumerators.SetType.EARTH, Enumerators.SetType.LIFE },
                { Enumerators.SetType.AIR, Enumerators.SetType.EARTH },
                { Enumerators.SetType.WATER, Enumerators.SetType.AIR },
            };
        }

        private int GetStrongersAndWeakersModifier(Enumerators.SetType attackerElement, Enumerators.SetType defenderElement)
        {
            int modifier = 0;

            if (_strongerElemental[attackerElement].Equals(defenderElement))
                modifier++;
            else if(_weakerElemental[attackerElement].Equals(defenderElement))
                modifier--;

            return modifier;
        }

        public void AttackPlayerByCreature(BoardUnit attackingUnit, Player attackedPlayer)
        {
            if (attackingUnit != null && attackedPlayer != null)
            {
                attackedPlayer.HP -= attackingUnit.Damage;
            }

            _tutorialManager.ReportAction(Enumerators.TutorialReportAction.ATTACK_CARD_HERO);

            _actionsQueueController.PostGameActionReport(_actionsQueueController.FormatGameActionReport(Enumerators.ActionType.ATTACK_PLAYER_BY_CREATURE,
            new object[]
            {
                attackingUnit,
                attackedPlayer
            }));
        }

        public void AttackCreatureByCreature(BoardUnit attackingUnit, BoardUnit attackedUnit)
        {
            if (attackingUnit != null && attackedUnit != null)
            {
                int additionalDamageAttacker = _abilitiesController.GetStatModificatorByAbility(attackingUnit.Card, attackedUnit.Card);
                int additionalDamageAttacked = _abilitiesController.GetStatModificatorByAbility(attackedUnit.Card, attackingUnit.Card);

                additionalDamageAttacker += GetStrongersAndWeakersModifier(attackingUnit.Card.libraryCard.cardSetType, attackedUnit.Card.libraryCard.cardSetType);
                additionalDamageAttacked += GetStrongersAndWeakersModifier(attackedUnit.Card.libraryCard.cardSetType, attackingUnit.Card.libraryCard.cardSetType);

                int damage = attackingUnit.Damage + additionalDamageAttacker;

                if (attackedUnit.HasBuffShield)
                {
                    damage = 0;
                    attackedUnit.UseShieldFromBuff();
                }

                attackedUnit.HP -= damage;

                damage = attackedUnit.Damage + additionalDamageAttacked;

                if (attackedUnit.HasBuffShield)
                {
                    damage = 0;
                    attackedUnit.UseShieldFromBuff();
                }

                attackingUnit.HP -= damage;
            }

            _tutorialManager.ReportAction(Enumerators.TutorialReportAction.ATTACK_CARD_CARD);

            _actionsQueueController.PostGameActionReport(_actionsQueueController.FormatGameActionReport(Enumerators.ActionType.ATTACK_CREATURE_BY_CREATURE,
            new object[]
            {
                attackingUnit,
                attackedUnit
            }));
        }

        public void AttackCreatureBySkill(Player attackingPlayer, HeroSkill skill, BoardUnit attackedUnit, int modifier)
        {
            if (attackedUnit != null)
            {
                int damage = (skill.value + modifier);

                if (attackedUnit.HasBuffShield)
                {
                    damage = 0;
                    attackedUnit.UseShieldFromBuff();
                }

                attackedUnit.HP -= damage;
            }

            _actionsQueueController.PostGameActionReport(_actionsQueueController.FormatGameActionReport(Enumerators.ActionType.ATTACK_CREATURE_BY_SKILL,
            new object[]
            {
                attackingPlayer,
                skill,
                (skill.value + modifier),
                attackedUnit
            }));
        }

        public void AttackPlayerBySkill(Player attackingPlayer, HeroSkill skill, Player attackedPlayer)
        {
            if (attackedPlayer != null)
            {
                attackedPlayer.HP -= skill.value;
            }

            _actionsQueueController.PostGameActionReport(_actionsQueueController.FormatGameActionReport(Enumerators.ActionType.ATTACK_PLAYER_BY_SKILL,
            new object[]
            {
                attackingPlayer,
                skill,
                attackedPlayer
            }));
        }

        public void HealPlayerBySkill(Player healingPlayer, HeroSkill skill, Player healedPlayer)
        {
            if (healingPlayer != null)
            {
                //if(healingPlayer.SelfHero.heroElement == Enumerators.SetType.EARTH)
                healedPlayer.HP += skill.value;
            }

            _actionsQueueController.PostGameActionReport(_actionsQueueController.FormatGameActionReport(Enumerators.ActionType.HEAL_PLAYER_BY_SKILL,
            new object[]
            {
                healedPlayer,
                skill,
                healedPlayer
            }));
        }

        public void HealCreatureBySkill(Player healingPlayer, HeroSkill skill, BoardUnit healedCreature)
        {
            if (healedCreature != null)
            {
                healedCreature.HP += skill.value;
            }

            _actionsQueueController.PostGameActionReport(_actionsQueueController.FormatGameActionReport(Enumerators.ActionType.HEAL_CREATURE_BY_SKILL,
            new object[]
            {
                healingPlayer,
                skill,
                healedCreature
            }));
        }
    
        public void AttackCreatureByAbility(Player attackingPlayer, AbilityData ability, BoardUnit attackedUnit)
        {
            if (attackedUnit != null)
            {
                int damage = ability.value;

                if (attackedUnit.HasBuffShield)
                {
                    damage = 0;
                    attackedUnit.UseShieldFromBuff();
                }

                attackedUnit.HP -= damage;
            }

            _actionsQueueController.PostGameActionReport(_actionsQueueController.FormatGameActionReport(Enumerators.ActionType.ATTACK_CREATURE_BY_ABILITY,
            new object[]
            {
                attackingPlayer,
                ability,
                attackedUnit
            }));
        }

        public void AttackPlayerByAbility(Player attackingPlayer, AbilityData ability, Player attackedPlayer)
        {
            if (attackedPlayer != null)
            {
                attackedPlayer.HP -= ability.value;
            }

            _actionsQueueController.PostGameActionReport(_actionsQueueController.FormatGameActionReport(Enumerators.ActionType.ATTACK_PLAYER_BY_ABILITY,
            new object[]
            {
                attackingPlayer,
                ability,
                attackedPlayer
            }));
        }

        public void HealPlayerByAbility(Player healingPlayer, AbilityData ability, Player healedPlayer)
        {
            if (healingPlayer != null)
            {
                healingPlayer.HP += ability.value;
            }

            _actionsQueueController.PostGameActionReport(_actionsQueueController.FormatGameActionReport(Enumerators.ActionType.HEAL_PLAYER_BY_ABILITY,
            new object[]
            {
                healingPlayer,
                ability,
                healedPlayer
            }));
        }

        public void HealCreatureByAbility(Player healingPlayer, AbilityData ability, BoardUnit healedCreature)
        {
            if (healedCreature != null)
            {
                healedCreature.HP += ability.value;
            }

            _actionsQueueController.PostGameActionReport(_actionsQueueController.FormatGameActionReport(Enumerators.ActionType.HEAL_CREATURE_BY_SKILL,
            new object[]
            {
                healingPlayer,
                ability,
                healedCreature
            }));
        }
    }
}