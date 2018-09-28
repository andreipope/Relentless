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
                    if (TargetUnit.Card.LibraryCard.CardSetType == SetType || SetType == Enumerators.SetType.NONE)
                    {
                        switch (StatType)
                        {
                            case Enumerators.StatType.DAMAGE:
                                TargetUnit.BuffedDamage += Value;
                                TargetUnit.CurrentDamage += Value;
                                break;
                            case Enumerators.StatType.HEALTH:
                                TargetUnit.BuffedHp += Value;
                                TargetUnit.CurrentHp += Value;
                                break;
                            default:
                                throw new ArgumentOutOfRangeException(nameof(StatType), StatType, null);
                        }

                        CreateVfx(BattlegroundController.GetBoardUnitViewByModel(TargetUnit).Transform.position);
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
