using System;
using Loom.ZombieBattleground;
using Loom.ZombieBattleground.Common;
using Opencoding.CommandHandlerSystem;
using UnityEngine;

static class BattleCommandsHandler
{
    public static void Initialize()
    {
        CommandHandlers.RegisterCommandHandlers(typeof(BattleCommandsHandler));
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


    [CommandHandler(Description = "Enemy Mode - DoNothing / Normal / DontAttack")]
    private static void EnemyMode(Enumerators.AiBrainType aiBrainType)
    {
        IGameplayManager gameplayManager = GameClient.Get<IGameplayManager>();
        gameplayManager.GetController<AIController>().SetAiBrainType(aiBrainType);
    }


    [CommandHandler(Description = "Player Draw Next - Draw next Card with Card Name")]
    private static void PlayerDrawNext(string cardName)
    {
        IGameplayManager gameplayManager = GameClient.Get<IGameplayManager>();
        BattlegroundController battlegroundController = gameplayManager.GetController<BattlegroundController>();
        CardsController cardsController = gameplayManager.GetController<CardsController>();
        Player player = gameplayManager.CurrentPlayer;

        if (!gameplayManager.CurrentTurnPlayer.Equals(player))
        {
            Debug.LogError("Please Wait For Your Turn");
            return;
        }

        WorkingCard workingCard = player.CardsInHand.Find(x => x.LibraryCard.Name == cardName);
        if (workingCard != null)
        {
            BoardCard card = battlegroundController.PlayerHandCards.Find(x => x.WorkingCard == workingCard);
            cardsController.PlayPlayerCard(player, card, card.HandBoardCard, PlayPlayerCardOnBoard);
        }
        else
        {
            workingCard = player.CardsInDeck.Find(x => x.LibraryCard.Name == cardName);
            if (workingCard != null)
            {
                cardsController.AddCardToHand(player, workingCard);
                workingCard = player.CardsInHand.Find(x => x.LibraryCard.Name == cardName);
                BoardCard card = battlegroundController.PlayerHandCards.Find(x => x.WorkingCard == workingCard);
                cardsController.PlayPlayerCard(player, card, card.HandBoardCard, PlayPlayerCardOnBoard);
            }
            else
            {
                Debug.LogError(cardName + " not Found.");
            }
        }
    }

    private static void PlayPlayerCardOnBoard(PlayCardOnBoard playCardOnBoard)
    {
        IGameplayManager gameplayManager = GameClient.Get<IGameplayManager>();
        Player player = gameplayManager.CurrentPlayer;

        if (player == gameplayManager.CurrentPlayer)
        {
            int gooDiff = playCardOnBoard.GooCost;
            if (player.Goo < playCardOnBoard.GooCost)
            {
                gooDiff = player.Goo > 0 ? player.Goo : 0;
            }

            playCardOnBoard.GooCost = gooDiff;

            PlayerMove playerMove = new PlayerMove(Enumerators.PlayerActionType.PlayCardOnBoard, playCardOnBoard);
            gameplayManager.PlayerMoves.AddPlayerMove(playerMove);
        }
    }


    [CommandHandler(Description = "Undoes the Previous Action")]
    private static void Undo()
    {
        IGameplayManager gameplayManager = GameClient.Get<IGameplayManager>();
        if (gameplayManager.CurrentTurnPlayer != gameplayManager.CurrentPlayer)
        {
            Debug.LogError("Please Wait for Your turn");
            return;
        }

        PlayerMove playerMove = gameplayManager.PlayerMoves.GetPlayerMove();
        if (playerMove == null)
        {
            Debug.LogError(" No Moves Exists");
            return;
        }

        switch (playerMove.PlayerActionType)
        {
            case Enumerators.PlayerActionType.PlayCardOnBoard:
                RevertPlayCardOnBoard(playerMove.Move);
                break;
            case Enumerators.PlayerActionType.AttackOnUnit:
                RevertAttackOnUnit(playerMove.Move);
                break;
            case Enumerators.PlayerActionType.AttackOnOverlord:
                RevertAttackOnOverlord(playerMove.Move);
                break;
            case Enumerators.PlayerActionType.PlayOverlordSkill:
                RevertOverlordSkill(playerMove.Move);
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private static void RevertPlayCardOnBoard(IMove move)
    {
        IGameplayManager gameplayManager = GameClient.Get<IGameplayManager>();
        CardsController cardsController = gameplayManager.GetController<CardsController>();

        PlayCardOnBoard obj = (PlayCardOnBoard)move;

        if (obj.Unit.GameObject == null)
        {
            Debug.LogError("Object is already Destoryed");
        }
        else
        {
            cardsController.ReturnCardToHand(obj.Unit);
            gameplayManager.CurrentPlayer.Goo += obj.GooCost;
        }
    }


    private static void RevertAttackOnUnit(IMove move)
    {
        AttackUnit obj = (AttackUnit) move;

        obj.AttackingUnitModel.NumTurnsOnBoard--;
        obj.AttackingUnitModel.OnStartTurn();

        obj.AttackingUnitModel.CurrentHp += obj.DamageOnAttackingUnit;
        obj.AttackedUnitModel.CurrentHp += obj.DamageOnAttackedUnit;

        // TODO : code if unit is destroyed
    }


    private static void RevertAttackOnOverlord(IMove move)
    {
        AttackOverlord obj = (AttackOverlord)move;

        obj.UnitModel.NumTurnsOnBoard--;
        obj.UnitModel.OnStartTurn();

        obj.AttackedPlayer.Health += obj.Damage;
    }

    private static void RevertOverlordSkill(IMove move)
    {
        PlayOverlordSkill obj = (PlayOverlordSkill) move;
        switch (obj.Skill.Skill.OverlordSkill)
        {
            case Enumerators.OverlordSkill.NONE:
                break;
            case Enumerators.OverlordSkill.PUSH:
                RevertPush(obj);
                break;
            case Enumerators.OverlordSkill.DRAW:
                break;
            case Enumerators.OverlordSkill.WIND_SHIELD:
                break;
            case Enumerators.OverlordSkill.LEVITATE:
                break;
            case Enumerators.OverlordSkill.RETREAT:
                break;
            case Enumerators.OverlordSkill.HARDEN:
                break;
            case Enumerators.OverlordSkill.STONE_SKIN:
                break;
            case Enumerators.OverlordSkill.FORTIFY:
                break;
            case Enumerators.OverlordSkill.PHALANX:
                break;
            case Enumerators.OverlordSkill.FORTRESS:
                break;
            case Enumerators.OverlordSkill.FIRE_BOLT:
                break;
            case Enumerators.OverlordSkill.RABIES:
                break;
            case Enumerators.OverlordSkill.FIREBALL:
                break;
            case Enumerators.OverlordSkill.MASS_RABIES:
                break;
            case Enumerators.OverlordSkill.METEOR_SHOWER:
                break;
            case Enumerators.OverlordSkill.HEALING_TOUCH:
                break;
            case Enumerators.OverlordSkill.MEND:
                break;
            case Enumerators.OverlordSkill.RESSURECT:
                break;
            case Enumerators.OverlordSkill.ENHANCE:
                break;
            case Enumerators.OverlordSkill.REANIMATE:
                break;
            case Enumerators.OverlordSkill.POISON_DART:
                RevertPosionDartAttack(obj);
                break;
            case Enumerators.OverlordSkill.TOXIC_POWER:
                RevertToxicPowerAttack(obj);
                break;
            case Enumerators.OverlordSkill.BREAKOUT:
                break;
            case Enumerators.OverlordSkill.INFECT:
                break;
            case Enumerators.OverlordSkill.EPIDEMIC:
                break;
            case Enumerators.OverlordSkill.FREEZE:
                break;
            case Enumerators.OverlordSkill.ICE_BOLT:
                break;
            case Enumerators.OverlordSkill.ICE_WALL:
                break;
            case Enumerators.OverlordSkill.SHATTER:
                break;
            case Enumerators.OverlordSkill.BLIZZARD:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private static void RevertPush(PlayOverlordSkill playOverlordSkill)
    {
        IGameplayManager gameplayManager = GameClient.Get<IGameplayManager>();
        CardsController cardsController = gameplayManager.GetController<CardsController>();


        /*WorkingCard workingCard = player.CardsInHand.Find(x => x.LibraryCard.Name == cardName);
        if (workingCard != null)
        {
            BoardCard card = battlegroundController.PlayerHandCards.Find(x => x.WorkingCard == workingCard);
            cardsController.PlayPlayerCard(player, card, card.HandBoardCard, PlayPlayerCardOnBoard);
            cardsController.
        }*/
    }

    private static void RevertToxicPowerAttack(PlayOverlordSkill playOverlordSkill)
    {
        if (playOverlordSkill.Target is BoardUnitModel unit)
        {
            RevertAttackOnUnitBySkill(unit, playOverlordSkill.Skill);

            unit.BuffedDamage -= playOverlordSkill.Skill.Skill.Attack;
            unit.CurrentDamage -= playOverlordSkill.Skill.Skill.Attack;

            playOverlordSkill.Skill.SetCoolDown(0);
        }
    }

    private static void RevertPosionDartAttack(PlayOverlordSkill playOverlordSkill)
    {
        if (playOverlordSkill.Target is Player player)
        {
            RevertAttackOnOverlordBySkill(player, playOverlordSkill.Skill);
        }
        else if(playOverlordSkill.Target is BoardUnitModel unit)
        {
            RevertAttackOnUnitBySkill(unit, playOverlordSkill.Skill);
        }

        playOverlordSkill.Skill.SetCoolDown(0);
    }

    private static void RevertAttackOnOverlordBySkill(Player player, BoardSkill boardSkill)
    {
        player.Health += boardSkill.Skill.Value;
    }

    private static void RevertAttackOnUnitBySkill(BoardUnitModel unitModel, BoardSkill boardSkill)
    {
        BoardUnitModel creature = unitModel;
        creature.CurrentHp += boardSkill.Skill.Value;
    }
}
