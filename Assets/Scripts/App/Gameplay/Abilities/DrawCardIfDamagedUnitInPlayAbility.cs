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

            if(PlayerCallerOfAbility.BoardCards.FindAll(x => x.Model.CurrentDefense < x.Model.MaxCurrentDefense).Count > 0)
            {
                CardsController.AddCardToHand(PlayerCallerOfAbility);
            }
        }
    }
}
