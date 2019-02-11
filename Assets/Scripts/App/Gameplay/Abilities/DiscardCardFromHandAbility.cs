using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;

namespace Loom.ZombieBattleground
{
    public class DiscardCardFromHandAbility : AbilityBase
    {
        public DiscardCardFromHandAbility(Enumerators.CardKind cardKind, AbilityData ability) : base(cardKind, ability)
        {
        }

        public override void Activate()
        {
            base.Activate();

            if(AbilityCallType == Enumerators.AbilityCallType.ENTRY && AbilityActivityType == Enumerators.AbilityActivityType.PASSIVE)
            {

            }
        }
    }
}
