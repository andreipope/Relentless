using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using System.Collections.Generic;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class ExtraGooIfUnitInPlayAbility : AbilityBase
    {
        private const int MaxExtraGooValue = 9999;
        private const int MinExtraGooValue = 0;

        public int Value { get; }

        public ExtraGooIfUnitInPlayAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Value = ability.Value;
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

            PlayerCallerOfAbility.ExtraGoo = Mathf.Clamp(PlayerCallerOfAbility.ExtraGoo + Value, MinExtraGooValue, MaxExtraGooValue);
            PlayerCallerOfAbility.CurrentGoo += Value;
        }

        protected override void UnitDiedHandler()
        {
            base.UnitDiedHandler();

            PlayerCallerOfAbility.ExtraGoo = Mathf.Clamp(PlayerCallerOfAbility.ExtraGoo - Value, MinExtraGooValue, MaxExtraGooValue);
            PlayerCallerOfAbility.CurrentGoo -= Value;
        }
    }
}
