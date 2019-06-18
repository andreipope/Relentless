using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using log4net;
using Loom.ZombieBattleground;
using Loom.ZombieBattleground.BackendCommunication;
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
    private static IOverlordExperienceManager _overlordExperienceManager;
    private static IUIManager _uiManager;
    private static BackendFacade _backendFacade;
    private static BackendDataControlMediator _backendDataControlMediator;

    public static void Initialize()
    {
        CommandHandlers.RegisterCommandHandlers(typeof(BattleCommandsHandler));

        _gameplayManager = GameClient.Get<IGameplayManager>();
        _dataManager = GameClient.Get<IDataManager>();
        _overlordExperienceManager = GameClient.Get<IOverlordExperienceManager>();
        _uiManager = GameClient.Get<IUIManager>();
        _backendFacade = GameClient.Get<BackendFacade>();
        _backendDataControlMediator = GameClient.Get<BackendDataControlMediator>();
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

        CardModel cardModel = player.CardsInDeck.FirstOrDefault(x => x.Prototype.Name == cardName);
        if (cardModel != null)
        {
            player.PlayerCardsController.AddCardFromDeckToHand(cardModel);
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

    public static IEnumerable<string> OverlordsNames()
    {
        string[] overlordNames = new string[_dataManager.CachedOverlordData.Overlords.Count];
        for (var i = 0; i < _dataManager.CachedOverlordData.Overlords.Count; i++)
        {
            overlordNames[i] = _dataManager.CachedOverlordData.Overlords[i].Prototype.Name;
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
        Card card = new Card(_dataManager.CachedCardsLibraryData.GetCardByName(cardName));
        player.PlayerCardsController.CreateNewCardAndAddToHand(card);
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

        Card card = new Card(_dataManager.CachedCardsLibraryData.GetCardByName(cardName));
        CardModel cardModel = opponentPlayer.PlayerCardsController.CreateNewCardAndAddToHand(card);
        _aiController.PlayCardOnBoard(cardModel, true);
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

        CardModel cardModel = opponentPlayer.CardsInDeck.FirstOrDefault(x => x.Prototype.Name == cardName);
        if (cardModel != null)
        {
            opponentPlayer.PlayerCardsController.AddCardFromDeckToHand(cardModel);
            cardModel = opponentPlayer.CardsInHand.FirstOrDefault(x => x.Prototype.Name == cardName);
            _aiController.PlayCardOnBoard(cardModel, true);
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
            _cardsController.ReturnCardToHand(obj.Unit.Model);
            _gameplayManager.CurrentPlayer.CurrentGoo += obj.GooCost;
        }
    }

    private static void GetCardFromGraveyard(BoardUnitView unit, Player player)
    {
        Card prototype = new Card(unit.Model.Card.Prototype);
        InstanceId updatedId = new InstanceId(unit.Model.InstanceId.Id, Enumerators.ReasonForInstanceIdChange.BackFromGraveyard);
        WorkingCard workingCard = new WorkingCard(prototype, prototype, player, id:updatedId);
        CardModel cardModel = new CardModel(workingCard);
        BoardUnitView newUnit = _battlegroundController.CreateBoardUnit(player, cardModel);

        player.PlayerCardsController.RemoveCardFromGraveyard(unit.Model);
        player.PlayerCardsController.AddCardToBoard(cardModel, ItemPosition.End);
        _battlegroundController.RegisterCardView(newUnit, player);

        _boardController.UpdateBoard(_battlegroundController.GetCardViewsByModels<BoardUnitView>(player.CardsOnBoard), true, null);
    }

    private static void RevertAttackOnUnit(IMove move)
    {
        AttackUnit obj = (AttackUnit) move;

        BoardUnitView attackingUnitView = _battlegroundController.GetCardViewByModel<BoardUnitView>(obj.AttackingUnitModel);
        if (attackingUnitView.GameObject == null)
        {
            GetCardFromGraveyard(attackingUnitView, _gameplayManager.CurrentPlayer);
        }
        else
        {
            obj.AttackingUnitModel.NumTurnsOnBoard--;
            obj.AttackingUnitModel.OnStartTurn();
            obj.AttackingUnitModel.AddToCurrentDefenseHistory(obj.DamageOnAttackingUnit, Enumerators.ReasonForValueChange.AbilityBuff);
        }

         obj.AttackedUnitModel.AddToCurrentDefenseHistory(obj.DamageOnAttackedUnit, Enumerators.ReasonForValueChange.AbilityBuff);
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
        switch (obj.Skill.Skill.Skill)
        {
            case Enumerators.Skill.NONE:
                break;
            case Enumerators.Skill.PUSH:
                RevertPush(obj);
                break;
            case Enumerators.Skill.DRAW:
                RevertDraw(obj);
                break;
            case Enumerators.Skill.HARDEN:
                RevertHarden(obj);
                break;
            case Enumerators.Skill.STONE_SKIN:
                RevertStoneSkin(obj);
                break;
            case Enumerators.Skill.FIRE_BOLT:
                RevertFireBolt(obj);
                break;
            case Enumerators.Skill.RABIES:
                RevertRabies(obj);
                break;
            case Enumerators.Skill.FIREBALL:
                RevertFireball(obj);
                break;
            case Enumerators.Skill.HEALING_TOUCH:
                RevertHealingTouch(obj);
                break;
            case Enumerators.Skill.MEND:
                RevertMend(obj);
                break;
            case Enumerators.Skill.POISON_DART:
                RevertPosionDartAttack(obj);
                break;
            case Enumerators.Skill.TOXIC_POWER:
                RevertToxicPowerAttack(obj);
                break;
            case Enumerators.Skill.FREEZE:
                RevertFreeze(obj);
                break;
            case Enumerators.Skill.ICE_BOLT:
                RevertIceBolt(obj);
                break;
            case Enumerators.Skill.ICE_WALL:
                RevertIceWall(obj);
                break;
            case Enumerators.Skill.BLIZZARD:
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
        else if(playOverlordSkill.Targets[0].BoardObject is CardModel unit)
        {
            unit.BuffedDefense -= playOverlordSkill.Skill.Skill.Value;
            unit.AddToCurrentDefenseHistory(-playOverlordSkill.Skill.Skill.Value, Enumerators.ReasonForValueChange.AbilityBuff);
        }

        playOverlordSkill.Skill.SetCoolDown(0);
    }

    private static void RevertFireball(PlayOverlordSkill playOverlordSkill)
    {
        if (playOverlordSkill.Targets[0].BoardObject is Player player)
        {
            RevertAttackOnOverlordBySkill(player, playOverlordSkill.Skill);
        }
        else if(playOverlordSkill.Targets[0].BoardObject is CardModel unit)
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
        else if(playOverlordSkill.Targets[0].BoardObject is CardModel unit)
        {
            RevertHealUnityBySkill(unit, playOverlordSkill.Skill);
        }

        playOverlordSkill.Skill.SetCoolDown(0);
    }

    private static void RevertIceBolt(PlayOverlordSkill playOverlordSkill)
    {
        if (playOverlordSkill.Targets[0].BoardObject is CardModel unit)
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
        else if(playOverlordSkill.Targets[0].BoardObject is CardModel unit)
        {
            unit.RevertStun();
        }

        playOverlordSkill.Skill.SetCoolDown(0);
    }

    private static void RevertRabies(PlayOverlordSkill playOverlordSkill)
    {
        if (playOverlordSkill.Targets[0].BoardObject is CardModel unit)
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
        else if(playOverlordSkill.Targets[0].BoardObject is CardModel unit)
        {
            RevertAttackOnUnitBySkill(unit, playOverlordSkill.Skill);
        }

        playOverlordSkill.Skill.SetCoolDown(0);
    }

    private static void RevertStoneSkin(PlayOverlordSkill playOverlordSkill)
    {
        if (playOverlordSkill.Targets[0].BoardObject is CardModel unit)
        {
            unit.BuffedDefense -= playOverlordSkill.Skill.Skill.Value;
            unit.AddToCurrentDefenseHistory(-playOverlordSkill.Skill.Skill.Value, Enumerators.ReasonForValueChange.AbilityBuff);
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
        CardModel targetUnit = (CardModel)playOverlordSkill.Targets[0].BoardObject;

        BoardCardView card =
            _battlegroundController.GetCardViewByModel<BoardCardView>(
                _gameplayManager.CurrentPlayer.CardsInHand.First(x => x == targetUnit));
        _cardsController.PlayPlayerCard(player, card, card.HandBoardCard, null);

        playOverlordSkill.Skill.SetCoolDown(0);
    }

    private static void RevertDraw(PlayOverlordSkill playOverlordSkill)
    {
        // TODO : no information which card added...
    }

    private static void RevertToxicPowerAttack(PlayOverlordSkill playOverlordSkill)
    {
        if (playOverlordSkill.Targets[0].BoardObject is CardModel unit)
        {
            RevertAttackOnUnitBySkill(unit, playOverlordSkill.Skill);

            unit.BuffedDamage -= playOverlordSkill.Skill.Skill.Damage;
            unit.AddToCurrentDamageHistory(-playOverlordSkill.Skill.Skill.Damage, Enumerators.ReasonForValueChange.AbilityBuff);
            playOverlordSkill.Skill.SetCoolDown(0);
        }
    }

    private static void RevertPosionDartAttack(PlayOverlordSkill playOverlordSkill)
    {
        if (playOverlordSkill.Targets[0].BoardObject is Player player)
        {
            RevertAttackOnOverlordBySkill(player, playOverlordSkill.Skill);
        }
        else if(playOverlordSkill.Targets[0].BoardObject is CardModel unit)
        {
            RevertAttackOnUnitBySkill(unit, playOverlordSkill.Skill);
        }

        playOverlordSkill.Skill.SetCoolDown(0);
    }

    private static void RevertAttackOnOverlordBySkill(Player player, BoardSkill boardSkill)
    {
        player.Defense += boardSkill.Skill.Value;
    }

    private static void RevertAttackOnUnitBySkill(CardModel unitModel, BoardSkill boardSkill)
    {
        CardModel creature = unitModel;
        creature.AddToCurrentDefenseHistory(boardSkill.Skill.Value, Enumerators.ReasonForValueChange.AbilityBuff);
    }

    private static void RevertHealPlayerBySkill(Player player, BoardSkill boardSkill)
    {
        if (player == null)
            return;

        player.Defense -= boardSkill.Skill.Value;
    }

    private static void RevertHealUnityBySkill(CardModel unitModel, BoardSkill boardSkill)
    {
        if (unitModel == null)
            return;

        unitModel.AddToCurrentDefenseHistory(-boardSkill.Skill.Value, Enumerators.ReasonForValueChange.AbilityBuff);
    }

    [CommandHandler(Description = "Unlocks current overlord abilities")]
    private static void UnlockAllCurrentOverlordAbilities()
    {
        foreach (var skill in _gameplayManager.CurrentPlayer.SelfOverlord.Skills)
        {
            skill.UserData.IsUnlocked = true;
        }
    }

    [CommandHandler(Description = "Show Player and Opponent XP")]
    private static void ShowPlayerAndOpponentXP()
    {
        Debug.Log("Player Experience = " + _overlordExperienceManager.PlayerMatchMatchExperienceInfo.ExperienceReceived);
        Debug.Log("Opponent Experience = " + _overlordExperienceManager.OpponentMatchMatchExperienceInfo.ExperienceReceived);
    }

    [CommandHandler(Description = "Set Player and Opponent XP")]
    private static void SetPlayerAndOpponentXP(int playerExperience, int opponentExperience)
    {
        _overlordExperienceManager.PlayerMatchMatchExperienceInfo.ExperienceReceived = playerExperience;
        _overlordExperienceManager.OpponentMatchMatchExperienceInfo.ExperienceReceived = opponentExperience;
    }
}
