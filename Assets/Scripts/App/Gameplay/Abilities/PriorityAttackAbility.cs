using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;

namespace LoomNetwork.CZB
{
    public class PriorityAttackAbility : AbilityBase
    {
        public PriorityAttackAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
        }

        public override void Activate()
        {
            base.Activate();

            if (AbilityCallType != Enumerators.AbilityCallType.Entry)
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
            AbilityUnitOwner.AttackAsFirst = true;
        }

        protected override void UnitOnDieEventHandler()
        {
            base.UnitOnDieEventHandler();
        }
    }
}
