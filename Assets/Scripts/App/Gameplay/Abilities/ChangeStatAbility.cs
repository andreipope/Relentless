using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using System.Collections.Generic;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class ChangeStatAbility : AbilityBase
    {
        public Enumerators.StatType StatType { get; }

        public int Value { get; }

        public ChangeStatAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            StatType = ability.AbilityStatType;
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
            if (AbilityCallType != Enumerators.AbilityCallType.ATTACK || !isAttacker)
                return;

            Action();
        }

        protected override void UnitDiedHandler()
        {
            base.UnitDiedHandler();

            if (AbilityCallType != Enumerators.AbilityCallType.DEATH)
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
                    case Enumerators.StatType.HEALTH:
                        AbilityUnitOwner.BuffedHp += Value;
                        AbilityUnitOwner.CurrentHp += Value;
                        break;
                    case Enumerators.StatType.DAMAGE:
                        AbilityUnitOwner.BuffedDamage += Value;
                        AbilityUnitOwner.CurrentDamage += Value;
                        break;
                }
            }
        }
    }
}
