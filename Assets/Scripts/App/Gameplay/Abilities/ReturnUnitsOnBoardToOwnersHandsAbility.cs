using System.Collections.Generic;
using System.Linq;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;

namespace Loom.ZombieBattleground
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

            AbilitiesController.ThrowUseAbilityEvent(MainWorkingCard, new List<BoardObject>(), AbilityData.AbilityType, Protobuf.AffectObjectType.Character);

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
            units =
                units
                    .Where(x => x.Model != AbilityUnitOwner)
                    .ToList();

            foreach (BoardUnitView unit in units)
            {
                ReturnBoardUnitToHand(unit);
            }

            units.Clear();
        }

        private void ReturnBoardUnitToHand(BoardUnitView unit)
        {
            CreateVfx(unit.Transform.position, true, 3f, true);

            CardsController.ReturnCardToHand(unit);
        }
    }
}
