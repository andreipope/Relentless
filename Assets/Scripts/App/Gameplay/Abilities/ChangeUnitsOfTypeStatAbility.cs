using System.Collections.Generic;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using LoomNetwork.Internal;
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
            List<BoardUnit> unitsOnBoard =
                PlayerCallerOfAbility.BoardCards.FindAll(x => x.Card.LibraryCard.CardSetType.Equals(SetType));

            foreach (BoardUnit unit in unitsOnBoard)
            {
                if (unit.Equals(AbilityUnitOwner))
                {
                    continue;
                }

                switch (StatType)
                {
                    case Enumerators.StatType.DAMAGE:
                        unit.BuffedDamage += Value;
                        unit.CurrentDamage += Value;
                        break;
                    case Enumerators.StatType.HEALTH:
                        unit.BuffedHp += Value;
                        unit.CurrentHp += Value;
                        break;
                }

                CreateVfx(unit.Transform.position, true);
            }
        }
    }
}
