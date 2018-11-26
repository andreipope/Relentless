using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using System.Collections.Generic;

namespace Loom.ZombieBattleground
{
    public class TakeControlEnemyUnitAbility : AbilityBase
    {
        public TakeControlEnemyUnitAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
        }

        public override void Activate()
        {
            base.Activate();
        }

        protected override void InputEndedHandler()
        {
            base.InputEndedHandler();

            if (IsAbilityResolved)
            {
                InvokeActionTriggered();
            }
        }

        public override void Action(object info = null)
        {
            base.Action(info);

            BattlegroundController.TakeControlUnit(PlayerCallerOfAbility, TargetUnit);

            AbilitiesController.ThrowUseAbilityEvent(MainWorkingCard, new List<BoardObject>()
            {
              TargetUnit
            }, AbilityData.AbilityType, Protobuf.AffectObjectType.Types.Enum.Character);
        }

        protected override void VFXAnimationEndedHandler()
        {
            base.VFXAnimationEndedHandler();

            Action();
        }
    }
}
