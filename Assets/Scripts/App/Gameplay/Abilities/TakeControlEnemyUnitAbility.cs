using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;

namespace Loom.ZombieBattleground
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
