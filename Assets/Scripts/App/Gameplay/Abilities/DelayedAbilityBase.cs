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

            AbilitiesController.ThrowUseAbilityEvent(MainWorkingCard, new List<BoardObject>(), AbilityData.AbilityType, Protobuf.AffectObjectType.Character);
        }

        protected override void TurnEndedHandler()
        {
            base.TurnEndedHandler();

            if (AbilityCallType != Enumerators.AbilityCallType.END ||
         !GameplayManager.CurrentTurnPlayer.Equals(PlayerCallerOfAbility))
                return;

            CountDelay();
        }

        protected override void TurnStartedHandler()
        {
            base.TurnStartedHandler();

            if (AbilityCallType != Enumerators.AbilityCallType.TURN ||
            !GameplayManager.CurrentTurnPlayer.Equals(PlayerCallerOfAbility))
                return;

            CountDelay();
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
