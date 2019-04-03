using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using System.Collections.Generic;
using System.Linq;

namespace Loom.ZombieBattleground
{
    internal class ReplaceOnStrongerOnesAbility : CardAbility
    {
        private List<BoardUnitModel> _boardUnits;
        private List<BoardUnitView> _boardUnitsViews;
        private List<BoardUnitView> _replaceBoardUnitsViews;
        private List<ReplaceUnitInfo> _replaceUnitInfos;

        private Enumerators.Faction faction;

        public override void DoAction(IReadOnlyList<GenericParameter> genericParameters)
        {
            _boardUnits = new List<BoardUnitModel>();
            _replaceUnitInfos = new List<ReplaceUnitInfo>();
            _replaceBoardUnitsViews = new List<BoardUnitView>();
            _boardUnitsViews = new List<BoardUnitView>();

            _boardUnits = Targets.Select(target => target as BoardUnitModel).ToList();

            if (AbilitiesController.HasParameter(GenericParameters, Common.Enumerators.AbilityParameter.TargetFaction))
            {
                faction = AbilitiesController.GetParameterValue<Enumerators.Faction>(GenericParameters,
                                                                    Enumerators.AbilityParameter.TargetFaction);
            }

            GetPossibleNewUnits();
            ClearOldUnitsOnBoard();
            GenerateNewUnitsOnBoard();
        }

        private void VFXAnimationEndedHandler()
        {
        }

        private void GenerateNewUnitsOnBoard()
        {
            BoardUnitView unit;
            ItemPosition itemPosition;
            foreach (ReplaceUnitInfo unitInfo in _replaceUnitInfos)
            {
                itemPosition = new ItemPosition(unitInfo.Position);
                unit = unitInfo.OwnerPlayer.PlayerCardsController.SpawnUnitOnBoard(unitInfo.NewUnitCardTitle, itemPosition);
                _replaceBoardUnitsViews.Add(unit);
                RanksController.AddUnitForIgnoreRankBuff(unit.Model);
            }
        }

        private void ClearOldUnitsOnBoard()
        {
            foreach (ReplaceUnitInfo unitInfo in _replaceUnitInfos)
            {
                unitInfo.OldUnitView.Model.OwnerPlayer.PlayerCardsController.RemoveCardFromBoard(unitInfo.OldUnitView.Model);

                BattlegroundController.DeactivateAllAbilitiesOnUnit(unitInfo.OldUnitView.Model);

                unitInfo.OldUnitView.DisposeGameObject();
            }
        }

        private void GetPossibleNewUnits()
        {
            foreach (BoardUnitModel unit in _boardUnits)
            {
                BoardUnitView unitView = BattlegroundController.GetBoardUnitViewByModel<BoardUnitView>(unit);
                _boardUnitsViews.Add(unitView);
                ReplaceUnitInfo replaceUnitInfo = new ReplaceUnitInfo()
                {
                    OldUnitCost = unit.Card.InstanceCard.Cost,
                    NewUnitPossibleCost = unit.Card.InstanceCard.Cost + 1,
                    OldUnitView = unitView,
                    OwnerPlayer = unit.OwnerPlayer,
                    Position = unit.OwnerPlayer.CardsOnBoard.IndexOf(unit),
                    NewUnitCardTitle = unit.Card.Prototype.Name
                };

                _replaceUnitInfos.Add(replaceUnitInfo);
            }

            _replaceUnitInfos.ForEach(GetPossibleNewUnitByMinCost);
        }

        private void GetPossibleNewUnitByMinCost(ReplaceUnitInfo replaceUnitInfo)
        {
            List<Card> possibleUnits = DataManager.CachedCardsLibraryData.Cards
                .Where(x => x.Cost >= replaceUnitInfo.NewUnitPossibleCost && x.Faction == faction)
                .ToList();

            if (possibleUnits.Count == 0)
            {
                possibleUnits = DataManager.CachedCardsLibraryData.Cards
                    .Where(x => x.Cost >= replaceUnitInfo.OldUnitCost && x.Faction == faction)
                    .ToList();
            }
            replaceUnitInfo.NewUnitCardTitle = possibleUnits[MTwister.IRandom(0, possibleUnits.Count)].Name;
        }

        public class ReplaceUnitInfo
        {
            public int OldUnitCost;
            public int NewUnitPossibleCost;
            public string NewUnitCardTitle;
            public BoardUnitView OldUnitView;
            public BoardUnitView NewUnitView;
            public Player OwnerPlayer;
            public int Position;
        }
    }
}
