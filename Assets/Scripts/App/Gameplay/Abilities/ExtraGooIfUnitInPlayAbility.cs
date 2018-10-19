using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using System.Collections.Generic;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class ExtraGooIfUnitInPlayAbility : AbilityBase
    {
        public int Value { get; }

        public ExtraGooIfUnitInPlayAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Value = ability.Value;
        }

        public override void Activate()
        {
            base.Activate();

            AbilitiesController.ThrowUseAbilityEvent(MainWorkingCard, new List<BoardObject>(), AbilityData.AbilityType, Protobuf.AffectObjectType.Player);

            if (AbilityCallType != Enumerators.AbilityCallType.ENTRY)
                return;

            Action();
        }

        public override void Action(object info = null)
        {
            base.Action(info);

            PlayerCallerOfAbility.ExtraGoo = Mathf.Clamp(PlayerCallerOfAbility.ExtraGoo + Value, 0, 9999);
        }

        protected override void UnitDiedHandler()
        {
            base.UnitDiedHandler();

            PlayerCallerOfAbility.ExtraGoo = Mathf.Clamp(PlayerCallerOfAbility.ExtraGoo - Value, 0, 9999);
        }
    }
}
