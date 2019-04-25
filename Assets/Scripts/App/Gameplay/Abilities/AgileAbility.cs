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

            if(AbilityTrigger == Enumerators.AbilityTrigger.ENTRY && AbilityActivity == Enumerators.AbilityActivity.PASSIVE)
            {
                if(AbilityTargets.Contains(Enumerators.Target.ITSELF))
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

            InvokeUseAbilityEvent(new List<ParametrizedAbilityBoardObject>()
            {
                new ParametrizedAbilityBoardObject(boardUnit)
            });
        }
    }
}
