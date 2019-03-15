using System;
using System.Collections.Generic;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;

namespace Loom.ZombieBattleground
{
    public class DrawCardIfDamagedUnitInPlayAbility : AbilityBase
    {
        public DrawCardIfDamagedUnitInPlayAbility(Enumerators.CardKind cardKind, AbilityData ability)
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

            if(PlayerCallerOfAbility.CardsOnBoard.FindAll(x => x.CurrentDefense < x.MaxCurrentDefense).Count > 0)
            {
                PlayerCallerOfAbility.PlayerCardsController.AddCardFromDeckToHand();
            }
        }
    }
}
