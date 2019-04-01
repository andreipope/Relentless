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

            if (AbilityTrigger != Enumerators.AbilityTrigger.ENTRY)
                return;

            Action();
        }

        public override void Action(object info = null)
        {
            base.Action(info);

            List<CardModel> units = new List<CardModel>();
            units.AddRange(GameplayManager.CurrentPlayer.CardsOnBoard);
            units.AddRange(GameplayManager.OpponentPlayer.CardsOnBoard);

            foreach (CardModel unit in units)
            {
                ReturnBoardUnitToDeck(unit);
            }

            units.Clear();
        }

        private void ReturnBoardUnitToDeck(CardModel unit)
        {
            if (AbilityUnitOwner != null && unit == AbilityUnitOwner || unit == null)
                return;

            // implement animation
            unit.Card = new WorkingCard(unit.Card.Prototype, unit.Card.Prototype, unit.OwnerPlayer, unit.InstanceId);
            unit.OwnerPlayer.PlayerCardsController.AddCardToDeck(unit);
            unit.MoveUnitFromBoardToDeck();
        }
    }
}
