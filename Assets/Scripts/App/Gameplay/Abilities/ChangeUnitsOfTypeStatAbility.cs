using System.Collections.Generic;
using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;
using LoomNetwork.Internal;
using UnityEngine;

namespace LoomNetwork.CZB
{
    public class ChangeUnitsOfTypeStatAbility : AbilityBase
    {
        public Enumerators.SetType setType;

        public Enumerators.StatType statType;

        public int value = 1;

        public ChangeUnitsOfTypeStatAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            statType = ability.abilityStatType;
            setType = Utilites.CastStringTuEnum<Enumerators.SetType>(ability.setType);
            value = ability.value;
        }

        public override void Activate()
        {
            base.Activate();

            switch (statType)
            {
                case Enumerators.StatType.HEALTH:
                case Enumerators.StatType.DAMAGE:
                default:
                    _vfxObject = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/GreenHealVFX");
                    break;
            }

            if (abilityCallType != Enumerators.AbilityCallType.PERMANENT)

                return;

            Action();
        }

        public override void Update()
        {
            base.Update();
        }

        public override void Dispose()
        {
            base.Dispose();
        }

        private void Action()
        {
            List<BoardUnit> unitsOnBoard = playerCallerOfAbility.BoardCards.FindAll(x => x.Card.libraryCard.cardSetType.Equals(setType));

            foreach (BoardUnit unit in unitsOnBoard)
            {
                if (unit.Equals(abilityUnitOwner))
                {
                    continue;
                }

                switch (statType)
                {
                    case Enumerators.StatType.DAMAGE:
                        unit.BuffedDamage += value;
                        unit.CurrentDamage += value;
                        break;
                    case Enumerators.StatType.HEALTH:
                        unit.BuffedHP += value;
                        unit.CurrentHP += value;
                        break;
                }

                CreateVFX(unit.transform.position, true);
            }
        }
    }
}
