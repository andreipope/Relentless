using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;
using UnityEngine;

namespace LoomNetwork.CZB
{
    public class DelayedAbilityBase : AbilityBase
    {
        public int Delay;

        protected int DelayedTurnsLeft;

        public DelayedAbilityBase(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Delay = ability.Delay;
            DelayedTurnsLeft = Delay;
        }

        public override void Activate()
        {
            base.Activate();

            VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/GreenHealVFX");
        }

        public override void Update()
        {
            base.Update();
        }

        public override void Dispose()
        {
            base.Dispose();
        }

        public override void Action(object info = null)
        {
            base.Action(info);
        }

        protected override void OnInputEndEventHandler()
        {
            base.OnInputEndEventHandler();
        }

        protected override void OnEndTurnEventHandler()
        {
            base.OnEndTurnEventHandler();

            if (AbilityCallType != Enumerators.AbilityCallType.End)
                return;

            // if (!_gameplayManager.CurrentTurnPlayer.Equals(playerCallerOfAbility))
            // return;
            CountDelay();
        }

        protected override void OnStartTurnEventHandler()
        {
            base.OnStartTurnEventHandler();

            if (AbilityCallType != Enumerators.AbilityCallType.Turn)
                return;

            CountDelay();
        }

        private void CountDelay()
        {
            if (DelayedTurnsLeft == 0)
            {
                Action();

                AbilitiesController.DeactivateAbility(ActivityId);
            }

            DelayedTurnsLeft--;
        }
    }
}
