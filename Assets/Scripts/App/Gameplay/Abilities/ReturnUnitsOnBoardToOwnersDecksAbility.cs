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

            List<BoardUnit> units = new List<BoardUnit>();
            units.AddRange(GameplayManager.CurrentPlayer.BoardCards);
            units.AddRange(GameplayManager.OpponentPlayer.BoardCards);

            foreach (BoardUnit unit in units)
            {
                ReturnBoardUnitToDeck(unit);
            }

            units.Clear();
        }

        private void ReturnBoardUnitToDeck(BoardUnit unit)
        {
            if (AbilityUnitOwner != null && unit == AbilityUnitOwner || unit == null)
                return;

            // implement animation
            unit.OwnerPlayer.AddCardToDeck(new WorkingCard(unit.Card.LibraryCard.Clone(), unit.OwnerPlayer));
            unit.MoveUnitFromBoardToDeck();
        }
    }
}
