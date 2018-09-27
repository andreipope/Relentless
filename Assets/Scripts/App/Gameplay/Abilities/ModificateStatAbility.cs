using System;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class ModificateStatAbility : AbilityBase
    {
        public Enumerators.SetType SetType;

        public Enumerators.StatType StatType;

        public int Value = 1;

        public ModificateStatAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            SetType = ability.AbilitySetType;
            StatType = ability.AbilityStatType;
            Value = ability.Value;
        }

        public override void Activate()
        {
            base.Activate();

            VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/GreenHealVFX");
        }

        public override void Action(object info = null)
        {
            base.Action(info);

            switch (AffectObjectType)
            {
                case Enumerators.AffectObjectType.CHARACTER:
                {
                    if (TargetUnitView.Model.Card.LibraryCard.CardSetType == SetType || SetType == Enumerators.SetType.NONE)
                    {
                        switch (StatType)
                        {
                            case Enumerators.StatType.DAMAGE:
                                TargetUnitView.Model.BuffedDamage += Value;
                                TargetUnitView.Model.CurrentDamage += Value;
                                break;
                            case Enumerators.StatType.HEALTH:
                                TargetUnitView.Model.BuffedHp += Value;
                                TargetUnitView.Model.CurrentHp += Value;
                                break;
                            default:
                                throw new ArgumentOutOfRangeException(nameof(StatType), StatType, null);
                        }

                        CreateVfx(TargetUnitView.Transform.position);
                    }
                }

                    break;
            }
        }

        protected override void InputEndedHandler()
        {
            base.InputEndedHandler();

            if (IsAbilityResolved)
            {
                Action();
            }
        }
    }
}
