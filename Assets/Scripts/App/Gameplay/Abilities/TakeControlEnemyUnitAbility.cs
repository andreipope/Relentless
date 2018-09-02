using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;

namespace LoomNetwork.CZB
{
    public class TakeControlEnemyUnitAbility : AbilityBase
    {
        public TakeControlEnemyUnitAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
        }

        protected override void InputEndedHandler()
        {
            base.InputEndedHandler();

            if (IsAbilityResolved)
            {
                BattlegroundController.TakeControlUnit(PlayerCallerOfAbility, TargetUnit);
            }
        }
    }
}
