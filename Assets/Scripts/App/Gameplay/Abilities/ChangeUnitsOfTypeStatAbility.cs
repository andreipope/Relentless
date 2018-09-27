using System;
using System.Collections.Generic;
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
            SetType = Utilites.CastStringTuEnum<Enumerators.SetType>(ability.SetType);
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

            if (AbilityCallType != Enumerators.AbilityCallType.PERMANENT)
                return;

            Action();
        }

        private void Action()
        {
            List<BoardUnitView> unitsOnBoard =
                PlayerCallerOfAbility.BoardCards.FindAll(x => x.Model.Card.LibraryCard.CardSetType.Equals(SetType));

            foreach (BoardUnitView unit in unitsOnBoard)
            {
                if (unit.Equals(AbilityUnitViewOwner))
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
