// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LoomNetwork.CZB.Common;
using UnityEngine;
using LoomNetwork.CZB.Data;

namespace LoomNetwork.CZB
{
    public class DevourZombieAndCombineStatsAbility : AbilityBase
    {
        public DevourZombieAndCombineStatsAbility(Enumerators.CardKind cardKind, AbilityData ability) : base(cardKind, ability)
        {
          
        }

        public override void Activate()
        {
            base.Activate();

            switch (abilityEffectType)
            {
                default:
                    _vfxObject = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/GreenHealVFX");
                    break;
            }
            
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

            Action();
        }

        public override void Action(object info = null)
        {
            base.Action(info);

          
        }
    }
}