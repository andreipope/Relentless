// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/


using LoomNetwork.CZB.Common;
using UnityEngine;
using LoomNetwork.CZB.Data;

namespace LoomNetwork.CZB
{
    public class AddGooVialsAbility : AbilityBase
    {
        public int value = 1;

        public AddGooVialsAbility(Enumerators.CardKind cardKind, AbilityData ability) : base(cardKind, ability)
        {
            this.value = ability.value;
        }

        public override void Activate()
        {
            base.Activate();

            Debug.Log("Activate");
            _vfxObject = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/GreenHealVFX");
            Action();
        }

        public override void Update()
        {
            base.Update();
        }

        public override void Dispose()
        {
            base.Dispose();
        }

        protected override void OnInputEndEventHandler()
        {
            base.OnInputEndEventHandler();

            if (_isAbilityResolved)
            {

            }
        }

        public override void Action(object info = null)
        {
            base.Action(info);

            playerCallerOfAbility.GooOnCurrentTurn += value;
            //playerCallerOfAbility.Goo = playerCallerOfAbility.GooOnCurrentTurn;
        }
    }
}