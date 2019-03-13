using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Loom.ZombieBattleground;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using Opencoding.CommandHandlerSystem;
using UnityEngine;

static class BattleCommandsHandler
{
    private static readonly ILog Log = Logging.GetLog(nameof(BattleCommandsHandler));

    private static IGameplayManager _gameplayManager;
    private static SkillsController _skillController;
    private static BattlegroundController _battlegroundController;
    private static BoardController _boardController;
    private static CardsController _cardsController;
    private static AIController _aiController;
    private static IDataManager _dataManager;
    private static IOverlordExperienceManager _overlordManager;
    private static IUIManager _uiManager;

    public static void Initialize()
    {
        CommandHandlers.RegisterCommandHandlers(typeof(BattleCommandsHandler));

        _gameplayManager = GameClient.Get<IGameplayManager>();
        _dataManager = GameClient.Get<IDataManager>();
        _overlordManager = GameClient.Get<IOverlordExperienceManager>();
        _uiManager = GameClient.Get<IUIManager>();
        _skillController = _gameplayManager.GetController<SkillsController>();
        _battlegroundController = _gameplayManager.GetController<BattlegroundController>();
        _cardsController = _gameplayManager.GetController<CardsController>();
        _aiController = _gameplayManager.GetController<AIController>();
        _boardController = _gameplayManager.GetController<BoardController>();
    }

    [CommandHandler(Description = "Reduce the current def of the Player overlord")]
    private static void PlayerOverlordSetDef(int defenseValue)
    {
        _gameplayManager.CurrentPlayer.Defense = defenseValue;
    }

    [CommandHandler(Description = "Reduce the current def of the AI overlord")]
    private static void EnemyOverlordSetDef(int defenseValue)
    {
        _gameplayManager.OpponentPlayer.Defense = defenseValue;
    }

    [CommandHandler(Description = "Player Overlord Ability Slot (0 or 1) and Cool Down Timer")]
    private static void PlayerOverlordSetAbilityTurn(int abilitySlot, int coolDownTimer)
    {
        if (abilitySlot == 0)
        {
            if (_skillController.PlayerPrimarySkill == null)
            {
                Log.Error("Primary Skill is Not set");
            }
            else
            {
                _skillController.PlayerPrimarySkill.SetCoolDown(coolDownTimer);
            }
        }
        else if (abilitySlot == 1)
        {
            if (_skillController.PlayerSecondarySkill == null)
            {
                Log.Error("Secondary Skill is Not set");
            }
            else
            {
                _skillController.PlayerSecondarySkill.SetCoolDown(coolDownTimer);
            }
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

    [CommandHandler(Description = "Allow Player to Play Cards without costing any goo")]
    private static void PlayerInfiniteGoo(bool useInfiniteGoo)
    {
        _gameplayManager.AvoidGooCost = useInfiniteGoo;
    }


    [CommandHandler(Description = "Enemy Mode - DoNothing / Normal / DontAttack")]
    private static void EnemyMode(Enumerators.AiBrainType aiBrainType)
    {
        _gameplayManager.GetController<AIController>().SetAiBrainType(aiBrainType);
    }

    [CommandHandler(Description = "Player Draw Next - Draw next Card with Card Name")]
    private static void PlayerDrawNext([Autocomplete(typeof(BattleCommandsHandler), "CardsInDeck")] string cardName)
    {
        Player player = _gameplayManager.CurrentPlayer;

        if (!_gameplayManager.CurrentTurnPlayer.Equals(player))
        {
            Log.Error("Please Wait For Your Turn");
            return;
        }

        BoardUnitModel boardUnitModel = player.CardsInDeck.FirstOrDefault(x => x.Prototype.Name == cardName);
        if (boardUnitModel != null)
        {
            _cardsController.AddCardToHand(player, boardUnitModel);
        }
        else
        {
            Log.Error(cardName + " not Found.");
        }
    }

    public static IEnumerable<string> CardsInDeck()
    {
        Player player = _gameplayManager.CurrentPlayer;
        string[] deckNames = new string[player.CardsInDeck.Count];
        for (var i = 0; i < player.CardsInDeck.Count; i++)
        {
            deckNames[i] = player.CardsInDeck[i].Prototype.Name;
        }
        return deckNames;
    }

    [CommandHandler(Description = "Will list down the cards that started in the Enemy Overlord's deck.")]
    private static void EnemyShowDeck()
    {
        Player player = _gameplayManager.OpponentPlayer;
        string cardsInDeck = "Cards In Deck = ";
        string cardsInHand = "Cards In Hand = ";
        for (var i = 0; i < player.CardsInDeck.Count; i++)
        {
            cardsInDeck += player.CardsInDeck[i].Prototype.Name + ",";
        }

        for (var i = 0; i < player.CardsInHand.Count; i++)
        {
            cardsInHand += player.CardsInHand[i].Prototype.Name + ",";
        }

        cardsInDeck = cardsInDeck.TrimEnd(',');
        cardsInHand = cardsInHand.TrimEnd(',');
        Log.Info(cardsInDeck);
        Log.Info(cardsInHand);
    }

    [CommandHandler(Description = "Sets the number of goo vials / bottles for the player where x is the number of goo vials")]
    private static void PlayerSetGooVial(int gooVials)
    {
        Player player = _gameplayManager.CurrentPlayer;
        if (!_gameplayManager.CurrentTurnPlayer.Equals(player))
        {
            Log.Warn("Please Wait For Your Turn");
            return;
        }

        if (gooVials <= 0 || gooVials > player.MaxGooVials)
        {
            Log.Error("Vials should not be less than zero or more than " + player.MaxGooVials);
            return;
        }

        player.GooVials = gooVials;
    }

    [CommandHandler(Description = "Sets the number of goo (max will be determined by current number of vials of course) for the player")]
    private static void PlayerSetGooAmount(int gooAmount)
    {
        Player player = _gameplayManager.CurrentPlayer;
        if (!_gameplayManager.CurrentTurnPlayer.Equals(player))
        {
            Log.Error("Please Wait For Your Turn");
            return;
        }

        if (gooAmount < 0)
        {
            Log.Error("Goo Amount should not be less than zero");
            return;
        }

        if (gooAmount > player.GooVials)
        {
            gooAmount = player.GooVials;
        }

        player.CurrentGoo = gooAmount;
    }

    [CommandHandler(Description = "Adds xp to an overlord. ")]
    private static void AddXP([Autocomplete(typeof(BattleCommandsHandler), "OverlordsNames")] string overlordName, int xpAmount)
    {
        Hero hero = _dataManager.CachedHeroesData.Heroes
            .Find(x => x.Name == overlordName);

        if (hero == null)
        {
            Log.Error(" Hero not found");
            return;
        }

        if (xpAmount <= 0)
        {
            Log.Error("Xp Amount should be higher than zero");
            return;
        }

        _overlordManager.InitializeExperienceInfoInMatch(hero);

        _overlordManager.ApplyExperience(hero, xpAmount);
        if (hero.Level > _overlordManager.MatchExperienceInfo.LevelAtBegin)
        {
            _uiManager.DrawPopup<LevelUpPopup>();
        }
    }

    [CommandHandler(Description = "Adds xp to an overlord. ")]
    private static void SetOverlordLevel([Autocomplete(typeof(BattleCommandsHandler), "OverlordsNames")] string overlordName, int level)
    {
        Hero hero = _dataManager.CachedHeroesData.Heroes
            .Find(x => x.Name == overlordName);

        if (hero == null)
        {
            Log.Error(" Hero not found");
            return;
        }

        if (level <= 0 || level > 20)
        {
            Log.Error("Level cant be set less than 1 nor max than 20");
            return;
        }

        hero.Level = level;

        _dataManager.SaveCache(Enumerators.CacheDataType.HEROES_DATA);
    }

    public static IEnumerable<string> OverlordsNames()
    {
        string[] overlordNames = new string[_dataManager.CachedHeroesData.Heroes.Count];
        for (var i = 0; i < _dataManager.CachedHeroesData.Heroes.Count; i++)
        {
            overlordNames[i] = _dataManager.CachedHeroesData.Heroes[i].Name;
        }
        return overlordNames;
    }


    [CommandHandler(Description = "Player Draw - Draw Card from Library with Card Name")]
    private static void PlayerDraw(string cardName)
    {
        Player player = _gameplayManager.CurrentPlayer;
        if (!_gameplayManager.CurrentTurnPlayer.Equals(player))
        {
            Log.Error("Please Wait For Your Turn");
            return;
        }
        _cardsController.CreateNewCardByNameAndAddToHand(player, cardName);
    }

    [CommandHandler(Description = "Sets the cooldown of the player's Overlord abilities to 0")]
    private static void PlayerInfiniteAbility(bool useInfiniteAbility)
    {
        Player player = _gameplayManager.CurrentPlayer;
        if (!_gameplayManager.CurrentTurnPlayer.Equals(player))
        {
            Log.Error("Please Wait For Your Turn");
            return;
        }

        if (_skillController.PlayerPrimarySkill == null)
        {
            Log.Error("Primary Skill is Not set");
        }
        else
        {
            _gameplayManager.UseInifiniteAbility = useInfiniteAbility;
            if(useInfiniteAbility)
                _skillController.PlayerPrimarySkill.SetCoolDown(0);
        }

        if (_skillController.PlayerSecondarySkill == null)
        {
            Log.Error("Secondary Skill is Not set");
        }
        else
        {
            _gameplayManager.UseInifiniteAbility = useInfiniteAbility;
            if(useInfiniteAbility)
                _skillController.PlayerSecondarySkill.SetCoolDown(0);
        }
    }


    [CommandHandler(Description = "Enemy Draw - Puts a card into play for the side of the AI/enemy from the library")]
    private static void EnemyOverlordPlayAnyCard(string cardName)
    {
        Player opponentPlayer = _gameplayManager.OpponentPlayer;
        if (!_gameplayManager.CurrentTurnPlayer.Equals(opponentPlayer))
        {
            Log.Error("Please Wait For Opponent Turn");
            return;
        }
        BoardUnitModel boardUnitModel = _cardsController.CreateNewCardByNameAndAddToHand(opponentPlayer, cardName);
        _aiController.PlayCardOnBoard(boardUnitModel, true);
    }

    [CommandHandler(Description = "Force the AI to draw and IMMEDIATELY play a card.")]
    private static void EnemyOverlordPlayCard(string cardName)
    {
        Player opponentPlayer = _gameplayManager.OpponentPlayer;
        if (!_gameplayManager.CurrentTurnPlayer.Equals(opponentPlayer))
        {
            Log.Error("Please Wait For Opponent Turn");
            return;
        }

        BoardUnitModel boardUnitModel = opponentPlayer.CardsInDeck.FirstOrDefault(x => x.Prototype.Name == cardName);
        if (boardUnitModel != null)
        {
            _cardsController.AddCardToHand(opponentPlayer, boardUnitModel);
            boardUnitModel = opponentPlayer.CardsInHand.FirstOrDefault(x => x.Prototype.Name == cardName);
            _aiController.PlayCardOnBoard(boardUnitModel, true);
        }
        else
        {
            Log.Error(cardName + " not Found.");
        }
    }


    private static void PlayPlayerCardOnBoard(PlayCardOnBoard playCardOnBoard)
    {
        Player player = _gameplayManager.CurrentPlayer;

        int gooDiff = playCardOnBoard.GooCost;
        if (player.CurrentGoo < playCardOnBoard.GooCost)
        {
            gooDiff = player.CurrentGoo > 0 ? player.CurrentGoo : 0;
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
            Log.Error("Please Wait for Your turn");
            return;
        }

        PlayerMove playerMove = _gameplayManager.PlayerMoves.GetPlayerMove();
        if (playerMove == null)
        {
            Log.Error(" No Moves Exists");
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
            _gameplayManager.CurrentPlayer.CurrentGoo += obj.GooCost;
        }
    }

    private static void GetCardFromGraveyard(BoardUnitView unit, Player player)
    {
        Card prototype = new Card(unit.Model.Card.Prototype);
        WorkingCard workingCard = new WorkingCard(prototype, prototype, player);
        BoardUnitModel boardUnitModel = new BoardUnitModel(workingCard);
        BoardUnitView newUnit = _battlegroundController.CreateBoardUnit(player, boardUnitModel);

        player.RemoveCardFromGraveyard(unit.Model);
        player.AddCardToBoard(boardUnitModel, ItemPosition.End);
        player.BoardCards.Insert(ItemPosition.End, newUnit);
        _battlegroundController.PlayerBoardCards.Insert(ItemPosition.End, newUnit);

        _boardController.UpdateBoard(player.BoardCards, true, null);
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

        obj.AttackedPlayer.Defense += obj.Damage;
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
        if (playOverlordSkill.Targets[0].BoardObject is Player player)
        {
            RevertHealPlayerBySkill(player, playOverlordSkill.Skill);
        }
        else if(playOverlordSkill.Targets[0].BoardObject is BoardUnitModel unit)
        {
            unit.BuffedHp -= playOverlordSkill.Skill.Skill.Value;
            unit.CurrentHp -= playOverlordSkill.Skill.Skill.Value;
        }

        playOverlordSkill.Skill.SetCoolDown(0);
    }

    private static void RevertFireball(PlayOverlordSkill playOverlordSkill)
    {
        if (playOverlordSkill.Targets[0].BoardObject is Player player)
        {
            RevertAttackOnOverlordBySkill(player, playOverlordSkill.Skill);
        }
        else if(playOverlordSkill.Targets[0].BoardObject is BoardUnitModel unit)
        {
            RevertAttackOnUnitBySkill(unit, playOverlordSkill.Skill);
        }

        playOverlordSkill.Skill.SetCoolDown(0);
    }

    private static void RevertMend(PlayOverlordSkill playOverlordSkill)
    {
        if (playOverlordSkill.Targets[0].BoardObject is Player player)
        {
            RevertAttackOnOverlordBySkill(player, playOverlordSkill.Skill);
            playOverlordSkill.Skill.SetCoolDown(0);
        }
    }

    private static void RevertHealingTouch(PlayOverlordSkill playOverlordSkill)
    {
        if (playOverlordSkill.Targets[0].BoardObject is Player player)
        {
            RevertHealPlayerBySkill(player, playOverlordSkill.Skill);
        }
        else if(playOverlordSkill.Targets[0].BoardObject is BoardUnitModel unit)
        {
            RevertHealUnityBySkill(unit, playOverlordSkill.Skill);
        }

        playOverlordSkill.Skill.SetCoolDown(0);
    }

    private static void RevertIceBolt(PlayOverlordSkill playOverlordSkill)
    {
        if (playOverlordSkill.Targets[0].BoardObject is BoardUnitModel unit)
        {
            RevertAttackOnUnitBySkill(unit, playOverlordSkill.Skill);
            unit.RevertStun();
            playOverlordSkill.Skill.SetCoolDown(0);
        }
    }

    private static void RevertFreeze(PlayOverlordSkill playOverlordSkill)
    {
        if (playOverlordSkill.Targets[0].BoardObject is Player player)
        {
            player.RevertStun();
        }
        else if(playOverlordSkill.Targets[0].BoardObject is BoardUnitModel unit)
        {
            unit.RevertStun();
        }

        playOverlordSkill.Skill.SetCoolDown(0);
    }

    private static void RevertRabies(PlayOverlordSkill playOverlordSkill)
    {
        if (playOverlordSkill.Targets[0].BoardObject is BoardUnitModel unit)
        {
            unit.SetInitialUnitType();
            playOverlordSkill.Skill.SetCoolDown(0);
        }
    }

    private static void RevertFireBolt(PlayOverlordSkill playOverlordSkill)
    {
        if (playOverlordSkill.Targets[0].BoardObject is Player player)
        {
            RevertAttackOnOverlordBySkill(player, playOverlordSkill.Skill);
        }
        else if(playOverlordSkill.Targets[0].BoardObject is BoardUnitModel unit)
        {
            RevertAttackOnUnitBySkill(unit, playOverlordSkill.Skill);
        }

        playOverlordSkill.Skill.SetCoolDown(0);
    }

    private static void RevertStoneSkin(PlayOverlordSkill playOverlordSkill)
    {
        if (playOverlordSkill.Targets[0].BoardObject is BoardUnitModel unit)
        {
            unit.BuffedHp -= playOverlordSkill.Skill.Skill.Value;
            unit.CurrentHp -= playOverlordSkill.Skill.Skill.Value;
            playOverlordSkill.Skill.SetCoolDown(0);
        }
    }

    private static void RevertHarden(PlayOverlordSkill playOverlordSkill)
    {
        if (playOverlordSkill.Targets[0].BoardObject is Player player)
        {
            RevertHealPlayerBySkill(player, playOverlordSkill.Skill);
            playOverlordSkill.Skill.SetCoolDown(0);
        }
    }

    private static void RevertPush(PlayOverlordSkill playOverlordSkill)
    {
        Player player = _gameplayManager.CurrentPlayer;
        BoardUnitModel targetUnit = (BoardUnitModel)playOverlordSkill.Targets[0].BoardObject;
        WorkingCard workingCard = targetUnit.Card;

        BoardCardView card = _battlegroundController.PlayerHandCards.First(x => x.BoardUnitModel.Card == workingCard);
        _cardsController.PlayPlayerCard(player, card, card.HandBoardCard, null);

        playOverlordSkill.Skill.SetCoolDown(0);
    }

    private static void RevertDraw(PlayOverlordSkill playOverlordSkill)
    {
        // TODO : no information which card added...
    }

    private static void RevertToxicPowerAttack(PlayOverlordSkill playOverlordSkill)
    {
        if (playOverlordSkill.Targets[0].BoardObject is BoardUnitModel unit)
        {
            RevertAttackOnUnitBySkill(unit, playOverlordSkill.Skill);

            unit.BuffedDamage -= playOverlordSkill.Skill.Skill.Attack;
            unit.CurrentDamage -= playOverlordSkill.Skill.Skill.Attack;

            playOverlordSkill.Skill.SetCoolDown(0);
        }
    }

    private static void RevertPosionDartAttack(PlayOverlordSkill playOverlordSkill)
    {
        if (playOverlordSkill.Targets[0].BoardObject is Player player)
        {
            RevertAttackOnOverlordBySkill(player, playOverlordSkill.Skill);
        }
        else if(playOverlordSkill.Targets[0].BoardObject is BoardUnitModel unit)
        {
            RevertAttackOnUnitBySkill(unit, playOverlordSkill.Skill);
        }

        playOverlordSkill.Skill.SetCoolDown(0);
    }

    private static void RevertAttackOnOverlordBySkill(Player player, BoardSkill boardSkill)
    {
        player.Defense += boardSkill.Skill.Value;
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

        player.Defense -= boardSkill.Skill.Value;
    }

    private static void RevertHealUnityBySkill(BoardUnitModel unitModel, BoardSkill boardSkill)
    {
        if (unitModel == null)
            return;

        unitModel.CurrentHp -= boardSkill.Skill.Value;
    }

    [CommandHandler(Description = "Unlocks current overlord abilities")]
    private static void UnlockAllCurrentOverlordAbilities()
    {
        foreach (var skill in _gameplayManager.CurrentPlayer.SelfHero.Skills)
        {
            skill.Unlocked = true;
        }

        GameClient.Get<IDataManager>().SaveCache(Enumerators.CacheDataType.HEROES_DATA);
    }
}
