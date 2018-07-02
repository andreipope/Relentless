// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



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

        public void Dispose()
        {
        }

        public void Init()
        {
            _gameplayManager = GameClient.Get<IGameplayManager>();
            _tutorialManager = GameClient.Get<ITutorialManager>();

            _actionsQueueController = _gameplayManager.GetController<ActionsQueueController>();
            _abilitiesController = _gameplayManager.GetController<AbilitiesController>();
        }

        public void Update()
        {
        }

        public void AttackPlayerByCreature(BoardCreature attackingCreature, Player attackedPlayer)
        {
            if (attackingCreature != null && attackedPlayer != null)
            {
                attackedPlayer.HP -= attackingCreature.Damage;
            }

            _tutorialManager.ReportAction(Enumerators.TutorialReportAction.ATTACK_CARD_HERO);

            _actionsQueueController.PostGameActionReport(_actionsQueueController.FormatGameActionReport(Enumerators.ActionType.ATTACK_PLAYER_BY_CREATURE,
            new object[]
            {
                attackingCreature,
                attackedPlayer
            }));
        }

        public void AttackCreatureByCreature(BoardCreature attackingCreature, BoardCreature attackedCreature)
        {
            if (attackingCreature != null && attackedCreature != null)
            {
                int additionalDamageAttacker = _abilitiesController.GetStatModificatorByAbility(attackingCreature.Card, attackedCreature.Card);
                int additionalDamageAttacked = _abilitiesController.GetStatModificatorByAbility(attackedCreature.Card, attackingCreature.Card);

                attackedCreature.HP -= attackingCreature.Damage + additionalDamageAttacker;
                attackingCreature.HP -= attackedCreature.Damage + additionalDamageAttacked;
            }

            _tutorialManager.ReportAction(Enumerators.TutorialReportAction.ATTACK_CARD_CARD);

            _actionsQueueController.PostGameActionReport(_actionsQueueController.FormatGameActionReport(Enumerators.ActionType.ATTACK_CREATURE_BY_CREATURE,
            new object[]
            {
                attackingCreature,
                attackedCreature
            }));
        }

        public void AttackCreatureBySkill(Player attackingPlayer, BoardSkill skill, BoardCreature attackedCreature)
        {
            if (attackedCreature != null)
            {
                attackedCreature.HP -= skill.SkillPower;
            }

            _actionsQueueController.PostGameActionReport(_actionsQueueController.FormatGameActionReport(Enumerators.ActionType.ATTACK_CREATURE_BY_SKILL,
            new object[]
            {
                attackingPlayer,
                skill.SkillPower,
                attackedCreature
            }));
        }

        public void AttackPlayerBySkill(Player attackingPlayer, BoardSkill skill, Player attackedPlayer)
        {
            if (attackedPlayer != null)
            {
                attackedPlayer.HP -= skill.SkillPower;
            }

            _actionsQueueController.PostGameActionReport(_actionsQueueController.FormatGameActionReport(Enumerators.ActionType.ATTACK_PLAYER_BY_SKILL,
            new object[]
            {
                attackingPlayer,
                skill.SkillPower,
                attackedPlayer
            }));
        }

        public void HealPlayerBySkill(Player healingPlayer, BoardSkill skill, Player healedPlayer)
        {
            if (healingPlayer != null)
            {
                healedPlayer.HP += skill.SkillPower;
            }

            _actionsQueueController.PostGameActionReport(_actionsQueueController.FormatGameActionReport(Enumerators.ActionType.HEAL_PLAYER_BY_SKILL,
            new object[]
            {
                healedPlayer,
                skill.SkillPower,
                healedPlayer
            }));
        }

        public void HealCreatureBySkill(Player healingPlayer, BoardSkill skill, BoardCreature healedCreature)
        {
            if (healedCreature != null)
            {
                healedCreature.HP += skill.SkillPower;
            }

            _actionsQueueController.PostGameActionReport(_actionsQueueController.FormatGameActionReport(Enumerators.ActionType.HEAL_CREATURE_BY_SKILL,
            new object[]
            {
                healingPlayer,
                skill.SkillPower,
                healedCreature
            }));
        }
    
        public void AttackCreatureByAbility(Player attackingPlayer, AbilityData ability, BoardCreature attackedCreature)
        {
            if (attackedCreature != null)
            {
                attackedCreature.HP -= ability.value;
            }

            _actionsQueueController.PostGameActionReport(_actionsQueueController.FormatGameActionReport(Enumerators.ActionType.ATTACK_CREATURE_BY_ABILITY,
            new object[]
            {
                attackingPlayer,
                ability.value,
                attackedCreature
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
                ability.value,
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
                ability.value,
                healedPlayer
            }));
        }
    
        public void HealCreatureByAbility(Player healingPlayer, AbilityData ability, BoardCreature healedCreature)
        {
            if (healedCreature != null)
            {
                healedCreature.HP += ability.value;
            }

            _actionsQueueController.PostGameActionReport(_actionsQueueController.FormatGameActionReport(Enumerators.ActionType.HEAL_CREATURE_BY_SKILL,
            new object[]
            {
                healingPlayer,
                ability.value,
                healedCreature
            }));
        }

        public void AttackPlayerByWeapon(Player attackingPlayer, BoardWeapon weapon, Player attackedPlayer)
        {
            if (attackingPlayer != null && weapon != null && attackedPlayer != null)
            {
                attackedPlayer.HP -= weapon.Damage;
            }

            _actionsQueueController.PostGameActionReport(_actionsQueueController.FormatGameActionReport(Enumerators.ActionType.ATTACK_PLAYER_BY_WEAPON,
            new object[]
            {
                attackingPlayer,
                weapon,
                attackedPlayer
            }));
        }

        public void AttackCreatureByWeapon(Player attackingPlayer, BoardWeapon weapon, BoardCreature attackedCreature)
        {
            if (attackingPlayer != null && weapon != null && attackedCreature != null)
            {
                attackedCreature.HP -= weapon.Damage;
            }

            _actionsQueueController.PostGameActionReport(_actionsQueueController.FormatGameActionReport(Enumerators.ActionType.ATTACK_CREATURE_BY_WEAPON,
            new object[]
            {
                attackingPlayer,
                weapon,
                attackedCreature
            }));
        }
    }
}