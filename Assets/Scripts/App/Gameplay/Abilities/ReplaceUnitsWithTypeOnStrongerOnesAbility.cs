using System;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using System.Collections.Generic;
using System.Linq;
using Object = UnityEngine.Object;

namespace Loom.ZombieBattleground
{
    public class ReplaceUnitsWithTypeOnStrongerOnesAbility : AbilityBase
    {
        private List<BoardUnitModel> _boardUnits;
        private List<BoardUnitView> _boardUnitsViews;
        private List<BoardUnitView> _replaceBoardUnitsViews;
        private List<ReplaceUnitInfo> _replaceUnitInfos;

        public int Value;
        public Enumerators.Faction Faction;
        public List<Enumerators.Target> TargetTypes;

        public ReplaceUnitsWithTypeOnStrongerOnesAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Value = ability.Value;
            Faction = ability.Faction;
            TargetTypes = ability.Targets;

            _boardUnits = new List<BoardUnitModel>();
            _replaceUnitInfos = new List<ReplaceUnitInfo>();
            _replaceBoardUnitsViews = new List<BoardUnitView>();
            _boardUnitsViews = new List<BoardUnitView>();
        }

        public override void Activate()
        {
            base.Activate();

            if (AbilityTrigger != Enumerators.AbilityTrigger.ENTRY)
                return;

            Action();
        }


        public override void Action(object info = null)
        {
            base.Action(info);

            if (PvPManager.UseBackendGameLogic)
                return;

            if(PredefinedTargets != null && PredefinedTargets.Count == 0)
                return;

            GetInfosAboutUnitsOnBoard();
            GetPossibleNewUnits();
            ClearOldUnitsOnBoard();
            GenerateNewUnitsOnBoard();

            InvokeActionTriggered(new List<BoardUnitView>[] { _boardUnitsViews, _replaceBoardUnitsViews });

            List<ParametrizedAbilityBoardObject> targets = new List<ParametrizedAbilityBoardObject>();

            foreach (ReplaceUnitInfo unitinfo in _replaceUnitInfos)
            {
                targets.Add(new ParametrizedAbilityBoardObject(
                    unitinfo.OldUnitView.Model,
                    new ParametrizedAbilityParameters
                    {
                        CardName = unitinfo.NewUnitCardTitle
                    }
                ));
            }

            InvokeUseAbilityEvent(targets);
        }

        protected override void VFXAnimationEndedHandler()
        {
            base.VFXAnimationEndedHandler();
        }

        private void GetInfosAboutUnitsOnBoard()
        {
            foreach (Enumerators.Target target in TargetTypes)
            {
                switch (target)
                {
                    case Enumerators.Target.OPPONENT_CARD:
                        _boardUnits.AddRange(GetOpponentOverlord().CardsOnBoard.FindAll(unit => unit.Card.Prototype.Faction == Faction));
                        break;
                    case Enumerators.Target.PLAYER_CARD:
                        _boardUnits.AddRange(PlayerCallerOfAbility.CardsOnBoard.FindAll(unit => unit.Card.Prototype.Faction == Faction));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(target), target, null);
                }
            }


            if (AbilityUnitOwner != null)
            {
                if (_boardUnits.Contains(AbilityUnitOwner))
                {
                    _boardUnits.Remove(AbilityUnitOwner);
                }
            }
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
            if (PredefinedTargets != null && PredefinedTargets.Count > 0)
            {
                foreach (BoardUnitModel unit in _boardUnits)
                {
                    BoardUnitView unitView = BattlegroundController.GetBoardUnitViewByModel<BoardUnitView>(unit);
                    unit.OwnerPlayer.PlayerCardsController.RemoveCardFromBoard(unit);
                    
                    BattlegroundController.DeactivateAllAbilitiesOnUnit(unit);
                }
            }
            else
            {
                foreach (ReplaceUnitInfo unitInfo in _replaceUnitInfos)
                {
                    unitInfo.OldUnitView.Model.OwnerPlayer.PlayerCardsController.RemoveCardFromBoard(unitInfo.OldUnitView.Model);

                    BattlegroundController.DeactivateAllAbilitiesOnUnit(unitInfo.OldUnitView.Model);
                }
            }
        }

        private void GetPossibleNewUnits()
        {
            if (PredefinedTargets != null)
            {
                foreach (ParametrizedAbilityBoardObject boardObject in PredefinedTargets)
                {
                    BoardUnitModel unit = boardObject.BoardObject as BoardUnitModel;
                    BoardUnitView unitView = BattlegroundController.GetBoardUnitViewByModel<BoardUnitView>(unit);
                    _boardUnitsViews.Add(unitView);
                    _replaceUnitInfos.Add(new ReplaceUnitInfo()
                    {
                        OldUnitView = unitView,
                        NewUnitCardTitle = boardObject.Parameters.CardName,
                        OwnerPlayer = unit.OwnerPlayer,
                        Position = unit.OwnerPlayer.CardsOnBoard.IndexOf(unit)
                    });
                }
            }
            else
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
        }

        private void GetPossibleNewUnitByMinCost(ReplaceUnitInfo replaceUnitInfo)
        {
            List<Card> possibleUnits = DataManager.CachedCardsLibraryData.Cards
                .Where(x => x.Cost >= replaceUnitInfo.NewUnitPossibleCost && x.Faction == Faction)
                .ToList();

            if (possibleUnits.Count == 0)
            {
                possibleUnits = DataManager.CachedCardsLibraryData.Cards
                    .Where(x => x.Cost >= replaceUnitInfo.OldUnitCost && x.Faction == Faction)
                    .ToList();
            }

            replaceUnitInfo.NewUnitCardTitle = possibleUnits[UnityEngine.Random.Range(0, possibleUnits.Count)].Name;
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
