using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using System.Collections.Generic;

namespace Loom.ZombieBattleground
{
    public class UseAllGooToIncreaseStatsAbility : AbilityBase
    {
        public int Value;

        public UseAllGooToIncreaseStatsAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Value = ability.Value;
        }

        public override void Activate()
        {
            base.Activate();

            if (AbilityTrigger != Enumerators.AbilityTrigger.ENTRY)
                return;

            Action();

            InvokeUseAbilityEvent();
        }

        public override void Action(object info = null)
        {
            base.Action(info);

            if (PlayerCallerOfAbility.CurrentGoo == 0)
                return;

            int increaseOn;

            increaseOn = PlayerCallerOfAbility.CurrentGoo * Value;
            AbilityUnitOwner.BuffedHp += increaseOn;
            AbilityUnitOwner.CurrentHp += increaseOn;
            AbilityUnitOwner.BuffedDamage += increaseOn;
            AbilityUnitOwner.CurrentDamage += increaseOn;

            PlayerCallerOfAbility.CurrentGoo = 0;
        }
    }
}
