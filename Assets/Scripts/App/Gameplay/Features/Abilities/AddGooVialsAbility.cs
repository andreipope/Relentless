using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GrandDevs.CZB.Common;
using CCGKit;
using UnityEngine;

namespace GrandDevs.CZB
{
    public class AddGooVialsAbility : AbilityBase
    {
        public int value = 1;
        
        public AddGooVialsAbility(Enumerators.Ability abilityId, Enumerators.CardKind cardKind, Enumerators.AbilityType abilType, Enumerators.AbilityActivityType type, Enumerators.AbilityCallType abilityCallType, List<Enumerators.AbilityTargetType> targetTypes,
                                     int value = 1) : base(abilityId, cardKind, abilType, type, abilityCallType, targetTypes)
        {
            this.value = value;
        }

        public override void Activate()
        {
            base.Activate();

            _vfxObject = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/healVFX");

            cardCaller.manaStat.baseValue += value;
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
    }
}