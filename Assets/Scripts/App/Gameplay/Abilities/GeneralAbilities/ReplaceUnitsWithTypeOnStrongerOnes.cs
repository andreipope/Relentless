using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using System.Collections.Generic;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class ReplaceUnitsWithTypeOnStrongerOnes : IAbility
    {
        private BoardObject _abilityUnitOwner;

        public NewAbilityData AbilityData { get; private set; }

        private List<BoardUnit> _boardUnits;
        private List<ReplaceUnitInfo> _replaceUnitInfos;

        private IGameplayManager _gameplayManager;
        private IDataManager _dataManager;
        private CardsController _cardsController;

        public void Init(NewAbilityData data, BoardObject owner)
        {
            AbilityData = data;
            _abilityUnitOwner = owner;

            _gameplayManager = GameClient.Get<IGameplayManager>();
            _dataManager = GameClient.Get<IDataManager>();
            _cardsController = _gameplayManager.GetController<CardsController>();
        }

        public void CallAction(object target)
        {
            GetInfosAboutUnitsOnBoard();
            GetPossibleNewUnits();
            ClearOldUnitsOnBoard();
            GenerateNewUnitsOnBoard();
        }


        private void GetInfosAboutUnitsOnBoard()
        {
            AbilityEnumerator.FactionType factionType = AbilityEnumerator.FactionType.NONE;

            foreach(var restriction in AbilityData.Restrictions)
            {
                if (restriction.Faction != AbilityEnumerator.FactionType.NONE)
                {
                    factionType = restriction.Faction;
                    break;
                }
            }

            switch (AbilityData.PossibleTargets)
            {
                case AbilityEnumerator.AbilityPossibleTargets.OPPONENT_CARD:
                    //_boardUnits.AddRange(_gameplayManager.OpponentPlayer.BoardCards.FindAll(x => x.Card.LibraryCard.CardSetType == factionType));
                    break;
                case AbilityEnumerator.AbilityPossibleTargets.ALLY_CARD:
                    //_boardUnits.AddRange(_gameplayManager.CurrentPlayer.BoardCards.FindAll(x => x.Card.LibraryCard.CardSetType == factionType));
                    break;
                default: break;
            }
        }

        private void GenerateNewUnitsOnBoard()
        {
            foreach (ReplaceUnitInfo unitInfo in _replaceUnitInfos)
            {
                _cardsController.SpawnUnitOnBoard(unitInfo.OwnerPlayer, unitInfo.NewUnitCardTitle);
            }
        }

        private void ClearOldUnitsOnBoard()
        {
            foreach (ReplaceUnitInfo unitInfo in _replaceUnitInfos)
            {
                unitInfo.OldUnit.OwnerPlayer.BoardCards.Remove(unitInfo.OldUnit);
                unitInfo.OldUnit.OwnerPlayer.RemoveCardFromBoard(unitInfo.OldUnit.Card);

                Object.Destroy(unitInfo.OldUnit.GameObject);
            }
        }

        private void GetPossibleNewUnits()
        {
            ReplaceUnitInfo replaceUnitInfo = null;

            foreach (BoardUnit unit in _boardUnits)
            {
                replaceUnitInfo = new ReplaceUnitInfo()
                {
                    OldUnitCost = unit.Card.RealCost,
                    NewUnitPossibleCost = unit.Card.RealCost + 1,
                    OldUnit = unit,
                    OwnerPlayer = unit.OwnerPlayer
                };

                _replaceUnitInfos.Add(replaceUnitInfo);
            }

            _replaceUnitInfos.ForEach(GetPossibleNewUnitByMinCost);
        }

        private void GetPossibleNewUnitByMinCost(ReplaceUnitInfo replaceUnitInfo)
        {
            List<Card> possibleUnits = _dataManager.CachedCardsLibraryData.Cards.FindAll(x => x.Cost >= replaceUnitInfo.NewUnitPossibleCost);

            if (possibleUnits.Count == 0)
            {
                possibleUnits = _dataManager.CachedCardsLibraryData.Cards.FindAll(x => x.Cost >= replaceUnitInfo.OldUnitCost);
            }

            replaceUnitInfo.NewUnitCardTitle = possibleUnits[Random.Range(0, possibleUnits.Count)].Name;
        }

        public class ReplaceUnitInfo
        {
            public int OldUnitCost;
            public int NewUnitPossibleCost;
            public string NewUnitCardTitle;
            public BoardUnit OldUnit;
            public BoardUnit NewUnit;
            public Player OwnerPlayer;
        }
    }
}
