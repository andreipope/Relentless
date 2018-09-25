using System.Collections.Generic;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;

namespace Loom.ZombieBattleground
{
    public class ReturnUnitsOnBoardToOwnersDecksAbility : AbilityBase
    {
        public ReturnUnitsOnBoardToOwnersDecksAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
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

            List<BoardUnitView> units = new List<BoardUnitView>();
            units.AddRange(GameplayManager.CurrentPlayer.BoardCards);
            units.AddRange(GameplayManager.OpponentPlayer.BoardCards);

            foreach (BoardUnitView unit in units)
            {
                ReturnBoardUnitToDeck(unit);
            }

            units.Clear();
        }

        private void ReturnBoardUnitToDeck(BoardUnitView unit)
        {
            if (AbilityUnitViewOwner != null && unit == AbilityUnitViewOwner || unit == null)
                return;

            // implement animation
            unit.Model.OwnerPlayer.AddCardToDeck(new WorkingCard(unit.Model.Card.LibraryCard.Clone(), unit.Model.OwnerPlayer));
            unit.Model.MoveUnitFromBoardToDeck();
        }
    }
}
