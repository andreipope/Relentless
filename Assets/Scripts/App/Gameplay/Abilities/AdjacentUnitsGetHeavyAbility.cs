using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using System.Collections.Generic;

namespace Loom.ZombieBattleground
{
    public class AdjacentUnitsGetHeavyAbility : AbilityBase
    {
        public AdjacentUnitsGetHeavyAbility(Enumerators.CardKind cardKind, AbilityData ability)
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

            int targetIndex = -1;
            for (int i = 0; i < PlayerCallerOfAbility.CardsOnBoard.Count; i++)
            {
                if (PlayerCallerOfAbility.CardsOnBoard[i] == AbilityUnitOwner)
                {
                    targetIndex = i;
                    break;
                }
            }

            if (targetIndex > -1)
            {
                if (targetIndex - 1 > -1)
                {
                    TakeHeavyToUnit(PlayerCallerOfAbility.CardsOnBoard[targetIndex - 1]);
                }

                if (targetIndex + 1 < PlayerCallerOfAbility.CardsOnBoard.Count)
                {
                    TakeHeavyToUnit(PlayerCallerOfAbility.CardsOnBoard[targetIndex + 1]);
                }
            }
        }

        private static void TakeHeavyToUnit(CardModel unit)
        {
            unit?.SetAsHeavyUnit();
        }
    }
}
