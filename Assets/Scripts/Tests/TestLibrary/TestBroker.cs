using System.Collections.Generic;
using Loom.ZombieBattleground.Common;
using UnityEngine;

namespace Loom.ZombieBattleground.Test
{
    public class TestBroker
    {
        private readonly IGameplayManager _gameplayManager;
        private readonly IUIManager _uiManager;
        private readonly IDataManager _dataManager;
        private readonly BattlegroundController _battlegroundController;
        private readonly SkillsController _skillsController;
        private readonly CardsController _cardsController;

        public TestBroker()
        {
            _gameplayManager = GameClient.Get<IGameplayManager>();
            _uiManager = GameClient.Get<IUIManager>();
            _dataManager = GameClient.Get<IDataManager>();

            _battlegroundController = _gameplayManager.GetController<BattlegroundController>();
            _skillsController = _gameplayManager.GetController<SkillsController>();
            _cardsController = _gameplayManager.GetController<CardsController>();
        }

        public Player GetPlayer(Enumerators.MatchPlayer matchPlayer)
        {
            return matchPlayer == Enumerators.MatchPlayer.CurrentPlayer ? _gameplayManager.CurrentPlayer : _gameplayManager.OpponentPlayer;
        }

        public IReadOnlyList<CardModel> GetBoardCards(Enumerators.MatchPlayer matchPlayer)
        {
            return matchPlayer == Enumerators.MatchPlayer.CurrentPlayer ? _gameplayManager.CurrentPlayer.CardsOnBoard : _gameplayManager.OpponentPlayer.CardsOnBoard;
        }

        public GameObject GetPlayerBoardGameObject(Enumerators.MatchPlayer matchPlayer)
        {
            return GameObject.Find(matchPlayer == Enumerators.MatchPlayer.CurrentPlayer ? "PlayerBoard" : "OpponentBoard");
        }

        public GameObject GetSpellsPivotGameObject(Enumerators.MatchPlayer matchPlayer)
        {
            return GameObject.Find(matchPlayer == Enumerators.MatchPlayer.CurrentPlayer ? "PlayerSpellsPivot" : "OpponentSpellsPivot");
        }

        public BoardSkill GetPlayerPrimarySkill(Enumerators.MatchPlayer matchPlayer)
        {
            return matchPlayer == Enumerators.MatchPlayer.CurrentPlayer ? _skillsController.PlayerPrimarySkill : _skillsController.OpponentPrimarySkill;
        }

        public BoardSkill GetPlayerSecondarySkill(Enumerators.MatchPlayer matchPlayer)
        {
            return matchPlayer == Enumerators.MatchPlayer.CurrentPlayer ? _skillsController.PlayerSecondarySkill : _skillsController.OpponentSecondarySkill;
        }

        public string GetSRTags(Enumerators.MatchPlayer matchPlayer)
        {
            return matchPlayer == Enumerators.MatchPlayer.CurrentPlayer ? "PlayerOwned" : "OpponentOwned";
        }
    }
}
