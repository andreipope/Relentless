using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using System.Collections.Generic;

namespace Loom.ZombieBattleground
{
    public class AllyUnitsOfTypeInPlayGetStatsAbility : AbilityBase
    {
        public int Health { get; }

        public int Damage { get; }

        public Enumerators.SetType SetType { get; }

        public AllyUnitsOfTypeInPlayGetStatsAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Health = ability.Health;
            Damage = ability.Damage;
            SetType = ability.AbilitySetType;
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

            foreach (BoardUnitModel unit in PlayerCallerOfAbility.CardsOnBoard)
            {
                if (unit.Card.Prototype.CardSetType.Equals(SetType) && unit != AbilityUnitOwner)
                {
                    unit.BuffedDamage += Damage;
                    unit.CurrentDamage += Damage;

                    unit.BuffedHp += Health;
                    unit.CurrentHp += Health;
                }
            }
        }
    }
}
