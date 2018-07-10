// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LoomNetwork.CZB.Common;
using UnityEngine;
using LoomNetwork.CZB.Data;

namespace LoomNetwork.CZB
{
    public class ChangeStatAbility : AbilityBase
    {
        public Enumerators.SetType setType;
        public Enumerators.StatType statType;
        public int value = 1;


        public ChangeStatAbility(Enumerators.CardKind cardKind, AbilityData ability) : base(cardKind, ability)
        {
            this.statType = ability.abilityStatType;
            this.value = ability.value;
        }

        public override void Activate()
        {
            base.Activate();

            _vfxObject = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/fireDamageVFX");
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

        protected override void UnitOnAttackEventHandler(object info)
        {
            base.UnitOnAttackEventHandler(info);
            if (abilityCallType != Enumerators.AbilityCallType.AT_ATTACK)
                return;
            
			string statName = statType == Enumerators.StatType.HEALTH ? "HP" : "DMG";

            switch (statType)
            {
                case Enumerators.StatType.HEALTH:
                    abilityUnitOwner.HP = ChangeValue(abilityUnitOwner.HP, value);
                    break;
                case Enumerators.StatType.DAMAGE:
                    abilityUnitOwner.Damage = ChangeValue(abilityUnitOwner.Damage, value);
                    break;
                default:
                    break;
            }
        }

        private int ChangeValue(int param, int valueChange)
        {
            param = param + valueChange;
            if (param < 0)
                param = 0;
            return param;
        }
    }
}