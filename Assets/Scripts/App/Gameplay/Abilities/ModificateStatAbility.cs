using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;
using UnityEngine;

namespace LoomNetwork.CZB
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

        public override void Update()
        {
            base.Update();
        }

        public override void Dispose()
        {
            base.Dispose();
        }

        public override void Action(object info = null)
        {
            base.Action(info);

            switch (AffectObjectType)
            {
                case Enumerators.AffectObjectType.Character:
                {
                    if ((TargetUnit.Card.LibraryCard.CardSetType == SetType) || (SetType == Enumerators.SetType.None))
                    {
                        if (StatType == Enumerators.StatType.Damage)
                        {
                            TargetUnit.BuffedDamage += Value;
                            TargetUnit.CurrentDamage += Value;
                        } else if (StatType == Enumerators.StatType.Health)
                        {
                            TargetUnit.BuffedHp += Value;
                            TargetUnit.CurrentHp += Value;
                        }

                        CreateVfx(TargetUnit.Transform.position);
                    }
                }

                    break;
            }
        }

        protected override void OnInputEndEventHandler()
        {
            base.OnInputEndEventHandler();

            if (IsAbilityResolved)
            {
                Action();
            }
        }
    }
}
