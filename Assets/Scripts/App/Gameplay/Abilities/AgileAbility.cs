using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using System.Collections.Generic;

namespace Loom.ZombieBattleground
{
    public class AgileAbility : AbilityBase
    {
        public AgileAbility(Enumerators.CardKind cardKind, AbilityData ability) : base(cardKind, ability)
        {
        }

        public override void Activate()
        {
            base.Activate();

            if(AbilityCallType == Enumerators.AbilityCallType.ENTRY && AbilityActivityType == Enumerators.AbilityActivityType.PASSIVE)
            {
                if(AbilityTargetTypes.Contains(Enumerators.AbilityTargetType.ITSELF))
                {
                    EnableAgileOnUnit(AbilityUnitOwner);
                }
            }
        }

        protected override void InputEndedHandler()
        {
            base.InputEndedHandler();

            if (IsAbilityResolved)
            {
                EnableAgileOnUnit(TargetUnit);
            }
        }

        private void EnableAgileOnUnit(BoardUnitModel boardUnit)
        {
            boardUnit.SetAgileStatus(true);

            AbilitiesController.ThrowUseAbilityEvent(MainWorkingCard, new List<BoardObject>() { boardUnit }, AbilityData.AbilityType, Enumerators.AffectObjectType.Character);
        }
    }
}
