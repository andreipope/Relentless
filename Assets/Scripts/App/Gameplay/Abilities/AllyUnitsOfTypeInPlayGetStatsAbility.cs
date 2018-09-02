using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;

namespace LoomNetwork.CZB
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

            if (AbilityCallType != Enumerators.AbilityCallType.ENTRY)
                return;

            Action();
        }

        public override void Action(object info = null)
        {
            base.Action(info);

            foreach (BoardUnit unit in PlayerCallerOfAbility.BoardCards)
            {
                if (unit.Card.LibraryCard.CardSetType.Equals(SetType) && unit != AbilityUnitOwner)
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
