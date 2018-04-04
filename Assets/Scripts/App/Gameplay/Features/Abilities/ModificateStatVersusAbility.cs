using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GrandDevs.CZB.Common;
using CCGKit;
using UnityEngine;

namespace GrandDevs.CZB
{
    public class ModificateStatVersusAbility : ModificateStatAbility
    {
        public ModificateStatVersusAbility(Enumerators.Ability abilityId, Enumerators.CardKind cardKind, Enumerators.AbilityType abilType, Enumerators.AbilityActivityType type, Enumerators.AbilityCallType abilityCallType, List<Enumerators.AbilityTargetType> targetTypes,
                                     Enumerators.StatType statType, Enumerators.SetType setType, int value = 1) : base(abilityId, cardKind, abilType, type, abilityCallType, targetTypes, statType, setType, value)
        {
        }

        public override void Activate()
        {
            base.Activate();
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
            if (_isAbilityResolved)
            {
                //todo smth
            }
        }
    }
}