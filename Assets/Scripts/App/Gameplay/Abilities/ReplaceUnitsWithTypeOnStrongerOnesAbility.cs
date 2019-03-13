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

            _boardUnits = new List<BoardUnitModel>();
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

            if (PvPManager.UseBackendGameLogic)
                return;

            if(PredefinedTargets != null && PredefinedTargets.Count == 0)
                return;

            GetInfosAboutUnitsOnBoard();
            GetPossibleNewUnits();
            ClearOldUnitsOnBoard();
            GenerateNewUnitsOnBoard();

            List<ParametrizedAbilityBoardObject> targets = new List<ParametrizedAbilityBoardObject>();

            foreach(ReplaceUnitInfo unitinfo in _replaceUnitInfos)
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

        private void GetInfosAboutUnitsOnBoard()
        {
            foreach (Enumerators.AbilityTargetType target in TargetTypes)
            {
                switch (target)
                {
                    case Enumerators.AbilityTargetType.OPPONENT_CARD:
                        _boardUnits.AddRange(GetOpponentOverlord().CardsOnBoard.FindAll(unit => unit.Card.Prototype.CardSetType == SetType));
                        break;
                    case Enumerators.AbilityTargetType.PLAYER_CARD:
                        _boardUnits.AddRange(PlayerCallerOfAbility.CardsOnBoard.FindAll(unit => unit.Card.Prototype.CardSetType == SetType));
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
                unit = unitInfo.OwnerPlayer.LocalCardsController.SpawnUnitOnBoard(unitInfo.NewUnitCardTitle, itemPosition);
            }
        }

        private void ClearOldUnitsOnBoard()
        {
            if (PredefinedTargets != null && PredefinedTargets.Count > 0)
            {
                foreach (BoardUnitModel unit in _boardUnits)
                {
                    unit.OwnerPlayer.BoardCards.Remove(unit);
                    unit.OwnerPlayer.RemoveCardFromBoard(unit);

                    BoardUnitView unitView = BattlegroundController.GetBoardUnitViewByModel<BoardUnitView>(unit);
                    unitView.DisposeGameObject();
                }
            }
            else
            {
                foreach (ReplaceUnitInfo unitInfo in _replaceUnitInfos)
                {
                    unitInfo.OldUnitView.Model.OwnerPlayer.BoardCards.Remove(unitInfo.OldUnitView);
                    unitInfo.OldUnitView.Model.OwnerPlayer.RemoveCardFromBoard(unitInfo.OldUnitView.Model);

                    unitInfo.OldUnitView.DisposeGameObject();
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
                .Where(x => x.Cost >= replaceUnitInfo.NewUnitPossibleCost && x.CardSetType == SetType)
                .ToList();

            if (possibleUnits.Count == 0)
            {
                possibleUnits = DataManager.CachedCardsLibraryData.Cards
                    .Where(x => x.Cost >= replaceUnitInfo.OldUnitCost && x.CardSetType == SetType)
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
