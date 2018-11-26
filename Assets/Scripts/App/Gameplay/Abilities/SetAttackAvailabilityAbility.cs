using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using System.Collections.Generic;

namespace Loom.ZombieBattleground
{
    public class SetAttackAvailabilityAbility : AbilityBase
    {
        public SetAttackAvailabilityAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
        }

        public override void Activate()
        {
            base.Activate();

            AbilitiesController.ThrowUseAbilityEvent(MainWorkingCard, new List<BoardObject>(), AbilityData.AbilityType, Protobuf.AffectObjectType.Types.Enum.Character);

            if (AbilityCallType != Enumerators.AbilityCallType.ENTRY)
                return;

            SetAttackAvailability(AbilityUnitOwner);
        }

        private void SetAttackAvailability(BoardUnitModel boardUnit)
        {
            boardUnit.CanAttackByDefault = false;
        }
    }
}
