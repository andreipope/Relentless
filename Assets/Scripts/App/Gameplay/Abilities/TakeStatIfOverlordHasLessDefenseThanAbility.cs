using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using System.Collections.Generic;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class TakeStatIfOverlordHasLessDefenseThanAbility : AbilityBase
    {
        public int Value;

        public int Defense;

        public int Damage;

        public Enumerators.Stat StatType;

        public TakeStatIfOverlordHasLessDefenseThanAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Value = ability.Value;
            Defense = ability.Defense;
            Damage = ability.Damage;
            StatType = ability.Stat;
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

            if (PlayerCallerOfAbility.Defense <= Value)
            {
                if (StatType == Enumerators.Stat.DEFENSE)
                {
                    AbilityUnitOwner.BuffedDefense += Defense;
                    AbilityUnitOwner.CurrentDefense += Defense;
                }
                else if (StatType == Enumerators.Stat.DAMAGE)
                {
                    AbilityUnitOwner.BuffedDamage += Damage;
                    AbilityUnitOwner.AddToCurrentDamageHistory(Damage, Enumerators.ReasonForValueChange.AbilityBuff);
                }
            }
        }
    }
}
