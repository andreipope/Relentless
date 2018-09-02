// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/

using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;
using UnityEngine;

namespace LoomNetwork.CZB
{
    public class TakeDamageAtEndOfTurnToThis : AbilityBase
    {
        public int value;

        public TakeDamageAtEndOfTurnToThis(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            value = ability.value;
        }

        public override void Activate()
        {
            base.Activate();

            _vfxObject = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/toxicDamageVFX");
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

            _battleController.AttackUnitByAbility(abilityUnitOwner, abilityData, abilityUnitOwner);
            CreateVFX(abilityUnitOwner.transform.position, true, 5f);
        }

        protected override void OnInputEndEventHandler()
        {
            base.OnInputEndEventHandler();
        }

        protected override void OnEndTurnEventHandler()
        {
            base.OnEndTurnEventHandler();

            if (!_gameplayManager.CurrentTurnPlayer.Equals(playerCallerOfAbility))
            
return;

            if (abilityCallType != Enumerators.AbilityCallType.END)
            
return;

            Action();
        }
    }
}
