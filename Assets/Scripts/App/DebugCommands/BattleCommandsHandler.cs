using System;
using Loom.ZombieBattleground;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
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
            _cardsController.PlayPlayerCard(player, card, card.HandBoardCard, PlayPlayerCardOnBoard);
        }
        else
        {
            workingCard = player.CardsInDeck.Find(x => x.LibraryCard.Name == cardName);
            if (workingCard != null)
            {
                _cardsController.AddCardToHand(player, workingCard);
                workingCard = player.CardsInHand.Find(x => x.LibraryCard.Name == cardName);
                BoardCard card = _battlegroundController.PlayerHandCards.Find(x => x.WorkingCard == workingCard);
                _cardsController.PlayPlayerCard(player, card, card.HandBoardCard, PlayPlayerCardOnBoard);
            }
            else
            {
                Debug.LogError(cardName + " not Found.");
            }
        }
    }

    private static void PlayPlayerCardOnBoard(PlayCardOnBoard playCardOnBoard)
    {
        Player player = _gameplayManager.CurrentPlayer;

        int gooDiff = playCardOnBoard.GooCost;
        if (player.Goo < playCardOnBoard.GooCost)
        {
            gooDiff = player.Goo > 0 ? player.Goo : 0;
        }

        playCardOnBoard.GooCost = gooDiff;

        PlayerMove playerMove = new PlayerMove(Enumerators.PlayerActionType.PlayCardOnBoard, playCardOnBoard);
        _gameplayManager.PlayerMoves.AddPlayerMove(playerMove);
    }


    [CommandHandler(Description = "Undoes the Previous Action")]
    private static void Undo()
    {
        if (_gameplayManager.CurrentTurnPlayer != _gameplayManager.CurrentPlayer)
        {
            Debug.LogError("Please Wait for Your turn");
            return;
        }

        PlayerMove playerMove = _gameplayManager.PlayerMoves.GetPlayerMove();
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
        PlayCardOnBoard obj = (PlayCardOnBoard)move;

        if (obj.Unit.GameObject == null)
        {
            GetCardFromGraveyard(obj.Unit, _gameplayManager.CurrentPlayer);
        }
        else
        {
            _cardsController.ReturnCardToHand(obj.Unit);
            _gameplayManager.CurrentPlayer.Goo += obj.GooCost;
        }
    }

    private static void GetCardFromGraveyard(BoardUnitView unit, Player player)
    {
        Card libraryCard = unit.Model.Card.LibraryCard.Clone();
        WorkingCard workingCard = new WorkingCard(libraryCard, player);
        BoardUnitView newUnit = _battlegroundController.CreateBoardUnit(player, workingCard);

        player.RemoveCardFromGraveyard(unit.Model.Card);
        player.AddCardToBoard(workingCard);
        player.BoardCards.Add(newUnit);

        _battlegroundController.PlayerBoardCards.Add(newUnit);
        _battlegroundController.UpdatePositionOfBoardUnitsOfPlayer(player.BoardCards);
    }


    private static void RevertAttackOnUnit(IMove move)
    {
        AttackUnit obj = (AttackUnit) move;

        BoardUnitView attackingUnitView = _battlegroundController.GetBoardUnitViewByModel(obj.AttackingUnitModel);
        if (attackingUnitView.GameObject == null)
        {
            GetCardFromGraveyard(attackingUnitView, _gameplayManager.CurrentPlayer);
        }
        else
        {
            obj.AttackingUnitModel.NumTurnsOnBoard--;
            obj.AttackingUnitModel.OnStartTurn();
            obj.AttackingUnitModel.CurrentHp += obj.DamageOnAttackingUnit;
        }

         obj.AttackedUnitModel.CurrentHp += obj.DamageOnAttackedUnit;
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
                RevertDraw(obj);
                break;
            case Enumerators.OverlordSkill.HARDEN:
                RevertHarden(obj);
                break;
            case Enumerators.OverlordSkill.STONE_SKIN:
                RevertStoneSkin(obj);
                break;
            case Enumerators.OverlordSkill.FIRE_BOLT:
                RevertFireBolt(obj);
                break;
            case Enumerators.OverlordSkill.RABIES:
                RevertRabies(obj);
                break;
            case Enumerators.OverlordSkill.FIREBALL:
                RevertFireball(obj);
                break;
            case Enumerators.OverlordSkill.HEALING_TOUCH:
                RevertHealingTouch(obj);
                break;
            case Enumerators.OverlordSkill.MEND:
                RevertMend(obj);
                break;
            case Enumerators.OverlordSkill.POISON_DART:
                RevertPosionDartAttack(obj);
                break;
            case Enumerators.OverlordSkill.TOXIC_POWER:
                RevertToxicPowerAttack(obj);
                break;
            case Enumerators.OverlordSkill.FREEZE:
                RevertFreeze(obj);
                break;
            case Enumerators.OverlordSkill.ICE_BOLT:
                RevertIceBolt(obj);
                break;
            case Enumerators.OverlordSkill.ICE_WALL:
                RevertIceWall(obj);
                break;
            case Enumerators.OverlordSkill.BLIZZARD:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private static void RevertIceWall(PlayOverlordSkill playOverlordSkill)
    {
        if (playOverlordSkill.Target is Player player)
        {
            RevertHealPlayerBySkill(player, playOverlordSkill.Skill);
        }
        else if(playOverlordSkill.Target is BoardUnitModel unit)
        {
            unit.BuffedHp -= playOverlordSkill.Skill.Skill.Value;
            unit.CurrentHp -= playOverlordSkill.Skill.Skill.Value;
        }

        playOverlordSkill.Skill.SetCoolDown(0);
    }

    private static void RevertFireball(PlayOverlordSkill playOverlordSkill)
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

    private static void RevertMend(PlayOverlordSkill playOverlordSkill)
    {
        if (playOverlordSkill.Target is Player player)
        {
            RevertAttackOnOverlordBySkill(player, playOverlordSkill.Skill);
            playOverlordSkill.Skill.SetCoolDown(0);
        }
    }

    private static void RevertHealingTouch(PlayOverlordSkill playOverlordSkill)
    {
        if (playOverlordSkill.Target is Player player)
        {
            RevertHealPlayerBySkill(player, playOverlordSkill.Skill);
        }
        else if(playOverlordSkill.Target is BoardUnitModel unit)
        {
            RevertHealUnityBySkill(unit, playOverlordSkill.Skill);
        }

        playOverlordSkill.Skill.SetCoolDown(0);
    }

    private static void RevertIceBolt(PlayOverlordSkill playOverlordSkill)
    {
        if (playOverlordSkill.Target is BoardUnitModel unit)
        {
            RevertAttackOnUnitBySkill(unit, playOverlordSkill.Skill);
            unit.RevertStun();
            playOverlordSkill.Skill.SetCoolDown(0);
        }
    }

    private static void RevertFreeze(PlayOverlordSkill playOverlordSkill)
    {
        if (playOverlordSkill.Target is Player player)
        {
            player.RevertStun();
        }
        else if(playOverlordSkill.Target is BoardUnitModel unit)
        {
            unit.RevertStun();
        }

        playOverlordSkill.Skill.SetCoolDown(0);
    }

    private static void RevertRabies(PlayOverlordSkill playOverlordSkill)
    {
        if (playOverlordSkill.Target is BoardUnitModel unit)
        {
            unit.SetInitialUnitType();
            playOverlordSkill.Skill.SetCoolDown(0);
        }
    }

    private static void RevertFireBolt(PlayOverlordSkill playOverlordSkill)
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

    private static void RevertStoneSkin(PlayOverlordSkill playOverlordSkill)
    {
        if (playOverlordSkill.Target is BoardUnitModel unit)
        {
            unit.BuffedHp -= playOverlordSkill.Skill.Skill.Value;
            unit.CurrentHp -= playOverlordSkill.Skill.Skill.Value;
            playOverlordSkill.Skill.SetCoolDown(0);
        }
    }

    private static void RevertHarden(PlayOverlordSkill playOverlordSkill)
    {
        if (playOverlordSkill.Target is Player player)
        {
            RevertHealPlayerBySkill(player, playOverlordSkill.Skill);
            playOverlordSkill.Skill.SetCoolDown(0);
        }
    }

    private static void RevertPush(PlayOverlordSkill playOverlordSkill)
    {
        Player player = _gameplayManager.CurrentPlayer;
        BoardUnitModel targetUnit = (BoardUnitModel)playOverlordSkill.Target;
        WorkingCard workingCard = targetUnit.Card;

        BoardCard card = _battlegroundController.PlayerHandCards.Find(x => x.WorkingCard == workingCard);
        _cardsController.PlayPlayerCard(player, card, card.HandBoardCard, null);

        playOverlordSkill.Skill.SetCoolDown(0);
    }

    private static void RevertDraw(PlayOverlordSkill playOverlordSkill)
    {
        // TODO : no information which card added...
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

    private static void RevertHealPlayerBySkill(Player player, BoardSkill boardSkill)
    {
        if (player == null)
            return;

        player.Health -= boardSkill.Skill.Value;
    }

    private static void RevertHealUnityBySkill(BoardUnitModel unitModel, BoardSkill boardSkill)
    {
        if (unitModel == null)
            return;

        unitModel.CurrentHp -= boardSkill.Skill.Value;
    }
}
