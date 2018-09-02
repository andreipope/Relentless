using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;

namespace LoomNetwork.CZB
{
    public class AllyUnitsOfTypeInPlayGetStatsAbility : AbilityBase
    {
        public int Health;

        public int Damage;

        public Enumerators.SetType SetType;

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

        public override void Update()
        {
            base.Update();
        }

        public override void Dispose()
        {
            base.Dispose();
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

        protected override void OnInputEndEventHandler()
        {
            base.OnInputEndEventHandler();
        }

        protected override void UnitOnAttackEventHandler(object info, int damage, bool isAttacker)
        {
            base.UnitOnAttackEventHandler(info, damage, isAttacker);
        }
    }
}
