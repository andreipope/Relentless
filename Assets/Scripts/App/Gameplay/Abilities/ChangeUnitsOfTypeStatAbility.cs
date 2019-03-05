using System;
using System.Collections.Generic;
using System.Linq;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class ChangeUnitsOfTypeStatAbility : AbilityBase
    {
        public Enumerators.SetType SetType;

        public Enumerators.StatType StatType;

        public int Value = 1;

        public ChangeUnitsOfTypeStatAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            StatType = ability.AbilityStatType;
            SetType = ability.AbilitySetType;
            Value = ability.Value;
        }

        public override void Activate()
        {
            base.Activate();

            switch (StatType)
            {
                case Enumerators.StatType.HEALTH:
                case Enumerators.StatType.DAMAGE:
                default:
                    VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/GreenHealVFX");
                    break;
            }

            InvokeUseAbilityEvent();

            if (AbilityCallType != Enumerators.AbilityCallType.PERMANENT)
                return;

            Action();
        }

        private void Action()
        {
            UniquePositionedList<BoardUnitView> unitsOnBoard =
                PlayerCallerOfAbility.BoardCards.FindAll(x => x.Model.Card.CardPrototype.CardSetType.Equals(SetType));

            foreach (BoardUnitView unit in unitsOnBoard)
            {
                if (unit.Model == AbilityUnitOwner)
                {
                    continue;
                }

                switch (StatType)
                {
                    case Enumerators.StatType.DAMAGE:
                        unit.Model.BuffedDamage += Value;
                        unit.Model.CurrentDamage += Value;
                        break;
                    case Enumerators.StatType.HEALTH:
                        unit.Model.BuffedHp += Value;
                        unit.Model.CurrentHp += Value;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(StatType), StatType, null);
                }

                CreateVfx(unit.Transform.position, true);
            }
        }
    }
}
