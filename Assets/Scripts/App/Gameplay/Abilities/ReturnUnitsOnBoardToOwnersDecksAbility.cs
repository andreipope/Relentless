using System.Collections.Generic;
using System.Linq;
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

            InvokeUseAbilityEvent();

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
                ReturnBoardUnitToDeck(unit.Model);
            }

            units.Clear();
        }

        private void ReturnBoardUnitToDeck(BoardUnitModel unit)
        {
            if (AbilityUnitOwner != null && unit == AbilityUnitOwner || unit == null)
                return;

            // implement animation
            unit.OwnerPlayer.AddCardToDeck(new WorkingCard(unit.Card.LibraryCard, unit.Card.LibraryCard, unit.OwnerPlayer));
            unit.MoveUnitFromBoardToDeck();
        }
    }
}
