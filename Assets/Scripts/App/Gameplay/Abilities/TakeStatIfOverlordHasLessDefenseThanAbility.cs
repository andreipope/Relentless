using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using System.Collections.Generic;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class TakeStatIfOverlordHasLessDefenseThanAbility : AbilityBase
    {
        public int Value;

        public int Health;

        public int Damage;

        public Enumerators.StatType StatType;

        public TakeStatIfOverlordHasLessDefenseThanAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Value = ability.Value;
            Health = ability.Health;
            Damage = ability.Damage;
            StatType = ability.AbilityStatType;
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

            if (PlayerCallerOfAbility.Defense <= Value)
            {
                if (StatType == Enumerators.StatType.HEALTH)
                {
                    AbilityUnitOwner.BuffedHp += Health;
                    AbilityUnitOwner.CurrentHp += Health;
                }
                else if (StatType == Enumerators.StatType.DAMAGE)
                {
                    AbilityUnitOwner.BuffedDamage += Damage;
                    AbilityUnitOwner.CurrentDamage += Damage;
                }
            }
        }
    }
}
