using Loom.ZombieBattleground;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using System.Collections.Generic;
using UnityEngine;

public class TestBroker
{
    private IGameplayManager _gameplayManager;
    private IUIManager _uiManager;
    private IDataManager _dataManager;

    private BattlegroundController _battlegroundController;
    private SkillsController _skillsController;
    private CardsController _cardsController;

    public TestBroker ()
    {
        _gameplayManager = GameClient.Get<IGameplayManager> ();
        _uiManager = GameClient.Get<IUIManager> ();
        _dataManager = GameClient.Get<IDataManager> ();

        _battlegroundController = _gameplayManager.GetController<BattlegroundController> ();
        _skillsController = _gameplayManager.GetController<SkillsController> ();
        _cardsController = _gameplayManager.GetController<CardsController> ();
    }

    public Player GetPlayer (Enumerators.MatchPlayer matchPlayer)
    {
        if (matchPlayer == Enumerators.MatchPlayer.CurrentPlayer)
            return _gameplayManager.CurrentPlayer;
        else
            return _gameplayManager.OpponentPlayer;
    }

    public List<BoardUnitView> GetBoardCards (Enumerators.MatchPlayer matchPlayer)
    {
        if (matchPlayer == Enumerators.MatchPlayer.CurrentPlayer)
            return _battlegroundController.PlayerBoardCards;
        else
            return _battlegroundController.OpponentBoardCards;
    }

    public GameObject GetPlayerBoardGameObject (Enumerators.MatchPlayer matchPlayer)
    {
        if (matchPlayer == Enumerators.MatchPlayer.CurrentPlayer)
            return GameObject.Find ("PlayerBoard");
        else
            return GameObject.Find ("OpponentBoard");
    }

    public GameObject GetSpellsPivotGameObject (Enumerators.MatchPlayer matchPlayer)
    {
        if (matchPlayer == Enumerators.MatchPlayer.CurrentPlayer)
            return GameObject.Find ("PlayerSpellsPivot");
        else
            return GameObject.Find ("OpponentSpellsPivot");
    }

    public BoardSkill GetPlayerPrimarySkill (Enumerators.MatchPlayer matchPlayer)
    {
        if (matchPlayer == Enumerators.MatchPlayer.CurrentPlayer)
            return _skillsController.PlayerPrimarySkill;
        else
            return _skillsController.OpponentPrimarySkill;
    }

    public BoardSkill GetPlayerSecondarySkill (Enumerators.MatchPlayer matchPlayer)
    {
        if (matchPlayer == Enumerators.MatchPlayer.CurrentPlayer)
            return _skillsController.PlayerSecondarySkill;
        else
            return _skillsController.OpponentSecondarySkill;
    }

    public string GetSRTags (Enumerators.MatchPlayer matchPlayer)
    {
        if (matchPlayer == Enumerators.MatchPlayer.CurrentPlayer)
            return SRTags.PlayerOwned;
        else
            return SRTags.OpponentOwned;
    }
}