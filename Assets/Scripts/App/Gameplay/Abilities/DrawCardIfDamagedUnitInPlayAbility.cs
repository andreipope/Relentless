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

            if (AbilityCallType != Enumerators.AbilityCallType.ENTRY)
                return;

            Action();
        }

        public override void Action(object info = null)
        {
            base.Action(info);

            if(PlayerCallerOfAbility.CardsOnBoard.FindAll(x => x.CurrentHp < x.MaxCurrentHp).Count > 0)
            {
                PlayerCallerOfAbility.LocalCardsController.AddCardToHand3();
            }
        }
    }
}
