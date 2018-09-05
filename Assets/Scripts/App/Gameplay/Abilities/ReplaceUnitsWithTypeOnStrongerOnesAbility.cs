using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using System.Collections.Generic;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class ReplaceUnitsWithTypeOnStrongerOnesAbility : AbilityBase
    {
        private List<BoardUnit> _boardUnits;
        private List<ReplaceUnitInfo> _replaceUnitInfos;

        public int Value;
        public Enumerators.SetType SetType;
        public List<Enumerators.AbilityTargetType> TargetTypes;

        public ReplaceUnitsWithTypeOnStrongerOnesAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Value = ability.Value;
            SetType = ability.AbilitySetType;
            TargetTypes = ability.AbilityTargetTypes;

            _boardUnits = new List<BoardUnit>();
            _replaceUnitInfos = new List<ReplaceUnitInfo>();
        }

        public override void Activate()
        {
            base.Activate();

            if (AbilityCallType != Enumerators.AbilityCallType.ENTRY)
                return;

            Action();
        }


        public override void Action(object info = null)
        {
            base.Action(info);

            GetInfosAboutUnitsOnBoard();
            GetPossibleNewUnits();
            ClearOldUnitsOnBoard();
            GenerateNewUnitsOnBoard();
        }

        private void GetInfosAboutUnitsOnBoard()
        {
            foreach (var target in TargetTypes)
            {
                switch (target)
                {
                    case Enumerators.AbilityTargetType.OPPONENT_CARD:
                        _boardUnits.AddRange(GameplayManager.OpponentPlayer.BoardCards.FindAll(x => CardsController.GetSetOfCard(x.Card.LibraryCard) == SetType.ToString()));
                        break;
                    case Enumerators.AbilityTargetType.PLAYER_CARD:
                        _boardUnits.AddRange(GameplayManager.CurrentPlayer.BoardCards.FindAll(x => CardsController.GetSetOfCard(x.Card.LibraryCard) == SetType.ToString()));
                        break;
                    default: break;
                }
            }
        }

        private void GenerateNewUnitsOnBoard()
        {
            foreach (var unitInfo in _replaceUnitInfos)
            {
                CardsController.SpawnUnitOnBoard(unitInfo.OwnerPlayer, unitInfo.NewUnitCardTitle);
            }
        }

        private void ClearOldUnitsOnBoard()
        {
            foreach(var unitInfo in _replaceUnitInfos)
            {
                unitInfo.OldUnit.OwnerPlayer.BoardCards.Remove(unitInfo.OldUnit);
                unitInfo.OldUnit.OwnerPlayer.RemoveCardFromBoard(unitInfo.OldUnit.Card);
               
                Object.Destroy(unitInfo.OldUnit.GameObject);
            }
        }

        private void GetPossibleNewUnits()
        {
            ReplaceUnitInfo replaceUnitInfo = null;
            foreach (var unit in _boardUnits)
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
            var possibleUnits = DataManager.CachedCardsLibraryData.Cards.FindAll(x => x.Cost >= replaceUnitInfo.NewUnitPossibleCost);

            if (possibleUnits.Count == 0)
            {
                possibleUnits = DataManager.CachedCardsLibraryData.Cards.FindAll(x => x.Cost >= replaceUnitInfo.OldUnitCost);
            }

            replaceUnitInfo.NewUnitCardTitle = possibleUnits[UnityEngine.Random.Range(0, possibleUnits.Count)].Name;
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
