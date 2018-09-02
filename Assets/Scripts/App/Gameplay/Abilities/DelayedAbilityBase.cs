// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/

using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;
using UnityEngine;

namespace LoomNetwork.CZB
{
    public class DelayedAbilityBase : AbilityBase
    {
        public int delay;

        protected int _delayedTurnsLeft;

        public DelayedAbilityBase(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            delay = ability.delay;
            _delayedTurnsLeft = delay;
        }

        public override void Activate()
        {
            base.Activate();

            _vfxObject = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/GreenHealVFX");
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

            if (abilityCallType != Enumerators.AbilityCallType.END)

                return;

            // if (!_gameplayManager.CurrentTurnPlayer.Equals(playerCallerOfAbility))
            // return;
            CountDelay();
        }

        protected override void OnStartTurnEventHandler()
        {
            base.OnStartTurnEventHandler();

            if (abilityCallType != Enumerators.AbilityCallType.TURN)

                return;

            CountDelay();
        }

        private void CountDelay()
        {
            if (_delayedTurnsLeft == 0)
            {
                Action();

                _abilitiesController.DeactivateAbility(activityId);
            }

            _delayedTurnsLeft--;
        }
    }
}
