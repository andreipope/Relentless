using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;
using UnityEngine;

namespace LoomNetwork.CZB
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
        }

        protected override void OnEndTurnEventHandler()
        {
            base.OnEndTurnEventHandler();

            if (AbilityCallType != Enumerators.AbilityCallType.END)
                return;

            CountDelay();
        }

        protected override void OnStartTurnEventHandler()
        {
            base.OnStartTurnEventHandler();

            if (AbilityCallType != Enumerators.AbilityCallType.TURN)
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
