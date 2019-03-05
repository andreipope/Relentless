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
        private List<BoardUnitView> _boardUnits;
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

            _boardUnits = new List<BoardUnitView>();
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
                        _boardUnits.AddRange(GetOpponentOverlord().BoardCards.FindAll(unit => unit.Model.Card.CardPrototype.CardSetType == SetType));
                        break;
                    case Enumerators.AbilityTargetType.PLAYER_CARD:
                        _boardUnits.AddRange(PlayerCallerOfAbility.BoardCards.FindAll(unit => unit.Model.Card.CardPrototype.CardSetType == SetType));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(target), target, null);
                }
            }


            if (AbilityUnitOwner != null)
            {
                BoardUnitView view = BattlegroundController.GetBoardUnitViewByModel(AbilityUnitOwner);

                if (_boardUnits.Contains(view))
                {
                    _boardUnits.Remove(view);
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
                unit = CardsController.SpawnUnitOnBoard(unitInfo.OwnerPlayer, unitInfo.NewUnitCardTitle, itemPosition);
                if (unit != null)
                {
                    AddUnitToBoardCards(unitInfo.OwnerPlayer, itemPosition, unit);
                }
            }
        }

        private void ClearOldUnitsOnBoard()
        {
            if (PredefinedTargets != null && PredefinedTargets.Count > 0)
            {
                foreach (BoardUnitView unit in _boardUnits)
                {
                    unit.Model.OwnerPlayer.BoardCards.Remove(unit);
                    unit.Model.OwnerPlayer.RemoveCardFromBoard(unit.Model.Card);

                    unit.DisposeGameObject();
                }
            }
            else
            {
                foreach (ReplaceUnitInfo unitInfo in _replaceUnitInfos)
                {
                    unitInfo.OldUnitView.Model.OwnerPlayer.BoardCards.Remove(unitInfo.OldUnitView);
                    unitInfo.OldUnitView.Model.OwnerPlayer.RemoveCardFromBoard(unitInfo.OldUnitView.Model.Card);

                    unitInfo.OldUnitView.DisposeGameObject();
                }
            }
        }

        private void GetPossibleNewUnits()
        {
            if (PredefinedTargets != null)
            {
                BoardUnitView unit;
                foreach (ParametrizedAbilityBoardObject boardObject in PredefinedTargets)
                {
                    unit = BattlegroundController.GetBoardUnitViewByModel(boardObject.BoardObject as BoardUnitModel);

                    _replaceUnitInfos.Add(new ReplaceUnitInfo()
                    {
                        OldUnitView = unit,
                        NewUnitCardTitle = boardObject.Parameters.CardName,
                        OwnerPlayer = unit.Model.OwnerPlayer,
                        Position = unit.Model.OwnerPlayer.BoardCards.IndexOf(unit)
                    });
                }
            }
            else
            {
                ReplaceUnitInfo replaceUnitInfo = null;
	            foreach (BoardUnitView unit in _boardUnits)
	            {
	                replaceUnitInfo = new ReplaceUnitInfo()
	                {
	                    OldUnitCost = unit.Model.Card.InstanceCard.Cost,
	                    NewUnitPossibleCost = unit.Model.Card.InstanceCard.Cost + 1,
	                    OldUnitView = unit,
	                    OwnerPlayer = unit.Model.OwnerPlayer,
	                    Position = unit.Model.OwnerPlayer.BoardCards.IndexOf(unit),
                        NewUnitCardTitle = unit.Model.Card.CardPrototype.Name
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

        private void AddUnitToBoardCards(Player owner, ItemPosition position, BoardUnitView unit)
        {
            if (owner.IsLocalPlayer)
            {
                BattlegroundController.PlayerBoardCards.Insert(position, unit);
            }
            else
            {
                BattlegroundController.OpponentBoardCards.Insert(position, unit);
            }
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
