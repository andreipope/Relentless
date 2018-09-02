using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;

namespace LoomNetwork.CZB
{
    public class AdjacentUnitsGetHeavyAbility : AbilityBase
    {
        public AdjacentUnitsGetHeavyAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
        }

        public override void Activate()
        {
            base.Activate();

            if (abilityCallType != Enumerators.AbilityCallType.ENTRY)

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

            int targetIndex = -1;
            for (int i = 0; i < playerCallerOfAbility.BoardCards.Count; i++)
            {
                if (playerCallerOfAbility.BoardCards[i] == abilityUnitOwner)
                {
                    targetIndex = i;
                    break;
                }
            }

            if (targetIndex > -1)
            {
                if (targetIndex - 1 > -1)
                {
                    TakeHeavyToUnit(playerCallerOfAbility.BoardCards[targetIndex - 1]);
                }

                if (targetIndex + 1 < playerCallerOfAbility.BoardCards.Count)
                {
                    TakeHeavyToUnit(playerCallerOfAbility.BoardCards[targetIndex + 1]);
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

        private void TakeHeavyToUnit(BoardUnit unit)
        {
            if (unit == null)

                return;

            unit.SetAsHeavyUnit();
        }
    }
}
