using Loom.ZombieBattleground;
using Loom.ZombieBattleground.Common;
using Opencoding.CommandHandlerSystem;
using UnityEngine;

static class BattleCommandsHandler
{
    private static IGameplayManager _gameplayManager;
    private static SkillsController _skillController;
    private static BattlegroundController _battlegroundController;
    private static CardsController _cardsController;

    public static void Initialize()
    {
        CommandHandlers.RegisterCommandHandlers(typeof(BattleCommandsHandler));

        _gameplayManager = GameClient.Get<IGameplayManager>();
        _skillController = _gameplayManager.GetController<SkillsController>();
        _battlegroundController = _gameplayManager.GetController<BattlegroundController>();
        _cardsController = _gameplayManager.GetController<CardsController>();
    }

    [CommandHandler(Description = "Reduce the current def of the Player overlord")]
    private static void PlayerOverlordSetDef(int defenseValue)
    {
        _gameplayManager.CurrentPlayer.Health = defenseValue;
    }

    [CommandHandler(Description = "Reduce the current def of the AI overlord")]
    private static void EnemyOverlordSetDef(int defenseValue)
    {
        _gameplayManager.OpponentPlayer.Health = defenseValue;
    }

    [CommandHandler(Description = "Player Overlord Ability Slot (0 or 1) and Cool Down Timer")]
    private static void PlayerOverlordSetAbilityTurn(int abilitySlot, int coolDownTimer)
    {
        if (abilitySlot == 0)
        {
            _skillController.PlayerPrimarySkill.SetCoolDown(coolDownTimer);
        }
        else if (abilitySlot == 1)
        {
            _skillController.PlayerSecondarySkill.SetCoolDown(coolDownTimer);
        }
    }

    [CommandHandler(Description = "AI Overlord Ability Slot (0 or 1) and Cool Down Timer")]
    private static void EnemyOverlordSetAbilityTurn(int abilitySlot, int coolDownTimer)
    {
        if (abilitySlot == 0)
        {
            _skillController.OpponentPrimarySkill.SetCoolDown(coolDownTimer);
        }
        else if (abilitySlot == 1)
        {
            _skillController.OpponentSecondarySkill.SetCoolDown(coolDownTimer);
        }
    }


    [CommandHandler(Description = "Enemy Mode - DoNothing / Normal / DontAttack")]
    private static void EnemyMode(Enumerators.AiBrainType aiBrainType)
    {
        _gameplayManager.GetController<AIController>().SetAiBrainType(aiBrainType);
    }

    [CommandHandler(Description = "Player Draw Next - Draw next Card with Card Name")]
    private static void PlayerDrawNext(string cardName)
    {
        Player player = _gameplayManager.CurrentPlayer;

        if (!_gameplayManager.CurrentTurnPlayer.Equals(player))
        {
            Debug.LogError("Please Wait For Your Turn");
            return;
        }

        WorkingCard workingCard = player.CardsInHand.Find(x => x.LibraryCard.Name == cardName);
        if (workingCard != null)
        {
            BoardCard card = _battlegroundController.PlayerHandCards.Find(x => x.WorkingCard == workingCard);
            _cardsController.PlayPlayerCard(player, card, card.HandBoardCard);
        }
        else
        {
            workingCard = player.CardsInDeck.Find(x => x.LibraryCard.Name == cardName);
            if (workingCard != null)
            {
                _cardsController.AddCardToHand(player, workingCard);
                workingCard = player.CardsInHand.Find(x => x.LibraryCard.Name == cardName);
                BoardCard card = _battlegroundController.PlayerHandCards.Find(x => x.WorkingCard == workingCard);
                _cardsController.PlayPlayerCard(player, card, card.HandBoardCard);
            }
            else
            {
                Debug.LogError(cardName + " not Found.");
            }
        }

    }
}
