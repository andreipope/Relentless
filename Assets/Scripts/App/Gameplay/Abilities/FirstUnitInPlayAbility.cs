using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;

namespace LoomNetwork.CZB
{
    public class FirstUnitInPlayAbility : AbilityBase
    {
        public int Value;

        public FirstUnitInPlayAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Value = ability.Value;
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

            if ((PlayerCallerOfAbility.BoardCards.Count == 0) || ((PlayerCallerOfAbility.BoardCards.Count == 1) && PlayerCallerOfAbility.BoardCards[0].Equals(AbilityUnitOwner)))
            {
                AbilityUnitOwner.BuffedHp += Value;
                AbilityUnitOwner.CurrentHp += Value;

                AbilityUnitOwner.BuffedDamage += Value;
                AbilityUnitOwner.CurrentDamage += Value;
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
