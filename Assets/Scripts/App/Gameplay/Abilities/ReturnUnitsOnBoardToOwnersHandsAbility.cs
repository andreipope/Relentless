using System.Collections.Generic;
using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;

namespace LoomNetwork.CZB
{
    public class ReturnUnitsOnBoardToOwnersHandsAbility : AbilityBase
    {
        public ReturnUnitsOnBoardToOwnersHandsAbility(Enumerators.CardKind cardKind, AbilityData ability)
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
                ReturnBoardUnitToHand(unit);
            }

            units.Clear();
        }

        private void ReturnBoardUnitToHand(BoardUnit unit)
        {
            CreateVfx(unit.Transform.position, true, 3f, true);

            CardsController.ReturnCardToHand(PlayerCallerOfAbility, unit);

            ActionsQueueController.PostGameActionReport(ActionsQueueController.FormatGameActionReport(Enumerators.ActionType.RETURN_TO_HAND_CARD_ABILITY, new object[] { PlayerCallerOfAbility, AbilityData, unit }));
        }
    }
}
