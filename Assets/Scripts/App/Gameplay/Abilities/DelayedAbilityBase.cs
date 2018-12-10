using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using System.Collections.Generic;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class DelayedAbilityBase : AbilityBase
    {
        public int Delay { get; }

        private int _delayedTurnsLeft;

        public DelayedAbilityBase(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Delay = ability.Delay;
            _delayedTurnsLeft = Delay;
        }

        public override void Activate()
        {
            base.Activate();

            VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/GreenHealVFX");

            AbilitiesController.ThrowUseAbilityEvent(MainWorkingCard, new List<BoardObject>(), AbilityData.AbilityType, Enumerators.AffectObjectType.Character);
        }

        protected override void TurnEndedHandler()
        {
            base.TurnEndedHandler();

            if (GetCaller() != null)
            {
                CountDelay();
            }
            else
            {
                AbilitiesController.DeactivateAbility(ActivityId);
            }
        }

        protected override void TurnStartedHandler()
        {
            base.TurnStartedHandler();

            if (GetCaller() != null)
            {
                CountDelay();
            }
            else
            {
                AbilitiesController.DeactivateAbility(ActivityId);
            }
        }

        private void CountDelay()
        {
            if (_delayedTurnsLeft == 0)
            {
                Action();

                AbilitiesController.DeactivateAbility(ActivityId);
            }

            _delayedTurnsLeft--;
        }
    }
}
