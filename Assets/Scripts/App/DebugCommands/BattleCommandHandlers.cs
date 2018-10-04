using Loom.ZombieBattleground;
using Opencoding.CommandHandlerSystem;

public enum AiType { DoNothing, Normal, DontAttack }

static class BattleCommandHandlers
{
    public static void Initialize()
    {
        CommandHandlers.RegisterCommandHandlers(typeof(BattleCommandHandlers));
    }

    [CommandHandler(Description = "Reduce the current def of the Player overlord")]
    private static void PlayerOverlordSetDef(int defenseValue)
    {
        IGameplayManager gameplayManager = GameClient.Get<IGameplayManager>();
        gameplayManager.CurrentPlayer.Health = defenseValue;
    }

    [CommandHandler(Description = "Reduce the current def of the AI overlord")]
    private static void EnemyOverlordSetDef(int defenseValue)
    {
        IGameplayManager gameplayManager = GameClient.Get<IGameplayManager>();
        gameplayManager.OpponentPlayer.Health = defenseValue;
    }

    [CommandHandler(Description = "Player Overlord Ability Slot (0 or 1) and Cool Down Timer")]
    private static void PlayerOverlordSetAbilityTurn(int abilitySlot, int coolDownTimer)
    {
        IGameplayManager gameplayManager = GameClient.Get<IGameplayManager>();
        SkillsController skillController = gameplayManager.GetController<SkillsController>();

        if (abilitySlot == 0)
        {
            skillController.PlayerPrimarySkill.SetCoolDown(coolDownTimer);
        }
        else if (abilitySlot == 1)
        {
            skillController.PlayerSecondarySkill.SetCoolDown(coolDownTimer);
        }
    }

    [CommandHandler(Description = "AI Overlord Ability Slot (0 or 1) and Cool Down Timer")]
    private static void EnemyOverlordSetAbilityTurn(int abilitySlot, int coolDownTimer)
    {
        IGameplayManager gameplayManager = GameClient.Get<IGameplayManager>();
        SkillsController skillController = gameplayManager.GetController<SkillsController>();

        if (abilitySlot == 0)
        {
            skillController.OpponentPrimarySkill.SetCoolDown(coolDownTimer);
        }
        else if (abilitySlot == 1)
        {
            skillController.OpponentSecondarySkill.SetCoolDown(coolDownTimer);
        }
    }
}
