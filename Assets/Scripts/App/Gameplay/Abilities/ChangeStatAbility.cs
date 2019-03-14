using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using System.Collections.Generic;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class ChangeStatAbility : AbilityBase
    {
        public Enumerators.Stat StatType { get; }

        public int Value { get; }

        public ChangeStatAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            StatType = ability.Stat;
            Value = ability.Value;
        }

        public override void Activate()
        {
            base.Activate();

            VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/GreenHealVFX");

            InvokeUseAbilityEvent();
        }

        protected override void UnitAttackedHandler(BoardObject info, int damage, bool isAttacker)
        {
            base.UnitAttackedHandler(info, damage, isAttacker);
            if (AbilityTrigger != Enumerators.AbilityTrigger.ATTACK || !isAttacker)
                return;

            Action();
        }

        protected override void UnitDiedHandler()
        {
            base.UnitDiedHandler();

            if (AbilityTrigger != Enumerators.AbilityTrigger.DEATH)
                return;

            Action();
        }


        public override void Action(object info = null)
        {
            base.Action(info);

            if (!PvPManager.UseBackendGameLogic)
            {
                switch (StatType)
                {
                    case Enumerators.Stat.DEFENSE:
                        AbilityUnitOwner.BuffedDefense += Value;
                        AbilityUnitOwner.CurrentDefense += Value;
                        break;
                    case Enumerators.Stat.DAMAGE:
                        AbilityUnitOwner.BuffedDamage += Value;
                        AbilityUnitOwner.CurrentDamage += Value;
                        break;
                }
            }
        }
    }
}
