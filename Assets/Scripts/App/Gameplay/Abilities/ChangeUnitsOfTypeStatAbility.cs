// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/


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


        public ChangeUnitsOfTypeStatAbility(Enumerators.CardKind cardKind, AbilityData ability) : base(cardKind, ability)
        {
            this.statType = ability.abilityStatType;
            this.setType = Utilites.CastStringTuEnum<Enumerators.SetType>(ability.setType);
            this.value = ability.value;
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
            var unitsOnBoard = playerCallerOfAbility.BoardCards.FindAll(x => x.Card.libraryCard.cardSetType.Equals(setType));

            foreach (var unit in unitsOnBoard)
            {
                if (unit.Equals(abilityUnitOwner))
                    continue;

                switch (statType)
                {
                    case Enumerators.StatType.DAMAGE:
                        unit.Damage += value;
                        break;
                    case Enumerators.StatType.HEALTH:
                        unit.HP += value;
                        break;
                    default: break;
                }

                CreateVFX(unit.transform.position, true);
            }
        }
    }
}