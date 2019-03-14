using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using System.Collections.Generic;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class AddGooVialsAbility : AbilityBase
    {
        public int Value = 1;

        public int Count;

        public AddGooVialsAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Value = ability.Value;
            Count = ability.Count;
        }

        public override void Activate()
        {
            base.Activate();

            InvokeUseAbilityEvent();
            if (AbilityTrigger != Enumerators.AbilityTrigger.ENTRY)
                return;

            InvokeActionTriggered();
        }

        protected override void UnitDiedHandler()
        {
            if (AbilityTrigger == Enumerators.AbilityTrigger.DEATH)
            {
                InvokeActionTriggered();
            }

            base.UnitDiedHandler();
        }

        public override void Action(object info = null)
        {
            base.Action(info);

            if (PlayerCallerOfAbility.GooVials == PlayerCallerOfAbility.MaxGooVials)
            {
                for (int i = 0; i < Count; i++)
                {
                    CardsController.AddCardToHand(PlayerCallerOfAbility);
                }
            }
            else if (PlayerCallerOfAbility.GooVials == PlayerCallerOfAbility.MaxGooVials - 1)
            {
                for (int i = 0; i < Count - 1; i++)
                {
                    CardsController.AddCardToHand(PlayerCallerOfAbility);
                }
            }

            PlayerCallerOfAbility.GooVials += Value;
        }

        protected override void VFXAnimationEndedHandler()
        {
            base.VFXAnimationEndedHandler();

            Action();
        }
    }
}
