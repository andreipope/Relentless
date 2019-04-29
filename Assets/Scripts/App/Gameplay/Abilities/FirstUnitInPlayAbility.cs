using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using System.Collections.Generic;

namespace Loom.ZombieBattleground
{
    public class FirstUnitInPlayAbility : AbilityBase
    {
        public int Value { get; }

        public FirstUnitInPlayAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Value = ability.Value;
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

            if (PlayerCallerOfAbility.CardsOnBoard.Count == 0 ||
                PlayerCallerOfAbility.CardsOnBoard.Count == 1 &&
                PlayerCallerOfAbility.CardsOnBoard[0] == AbilityUnitOwner)
            {
                AbilityUnitOwner.BuffedDefense += Value;
                AbilityUnitOwner.AddToCurrentDefenseHistory(Value, Enumerators.ReasonForValueChange.AbilityBuff);

                AbilityUnitOwner.BuffedDamage += Value;
                AbilityUnitOwner.AddToCurrentDamageHistory(Value, Enumerators.ReasonForValueChange.AbilityBuff);
            }
        }
    }
}
