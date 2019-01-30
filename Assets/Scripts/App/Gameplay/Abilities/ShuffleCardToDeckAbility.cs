using System.Collections.Generic;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;

namespace Loom.ZombieBattleground
{
    public class ShuffleCardToDeckAbility : AbilityBase
    {
        public List<Enumerators.AbilityTargetType> TargetTypes { get; }

        public ShuffleCardToDeckAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            TargetTypes = ability.AbilityTargetTypes;
        }

        public override void Activate()
        {
            base.Activate();

            AbilitiesController.ThrowUseAbilityEvent(MainWorkingCard, new List<BoardObject>(), AbilityData.AbilityType, Enumerators.AffectObjectType.Player);
        }

        protected override void UnitDiedHandler()
        {
            base.UnitDiedHandler();

            if (AbilityCallType != Enumerators.AbilityCallType.DEATH)
                return;

            Action();
        }

        public override void Action(object param = null)
        {
            base.Action(param);

            if (TargetTypes.Contains(Enumerators.AbilityTargetType.PLAYER))
            {
                // FIXME: doesn't this cause de-sync?
                PlayerCallerOfAbility.AddCardToDeck(MainWorkingCard, true);
            }
        }
    }
}
