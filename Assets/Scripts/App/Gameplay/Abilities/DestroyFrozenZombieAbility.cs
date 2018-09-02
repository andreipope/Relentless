using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;

namespace LoomNetwork.CZB
{
    public class DestroyFrozenZombieAbility : AbilityBase
    {
        public DestroyFrozenZombieAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            TargetUnitStatusType = ability.TargetUnitStatusType;
        }

        protected override void OnInputEndEventHandler()
        {
            base.OnInputEndEventHandler();

            if (IsAbilityResolved)
            {
                TargetUnit.Die();
            }
        }
    }
}
