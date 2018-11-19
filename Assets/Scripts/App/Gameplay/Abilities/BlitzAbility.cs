using System;
using System.Collections.Generic;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class BlitzAbility : AbilityBase
    {
        public BlitzAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
        }

        public override void Activate()
        {
            base.Activate();

            AbilityUnitOwner.AddEffectOnUnit(Enumerators.EffectOnUnitType.Blitz);
        }
    }
}
