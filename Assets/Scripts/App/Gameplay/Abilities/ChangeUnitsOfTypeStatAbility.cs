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
            IReadOnlyList<BoardUnitModel> unitsOnBoard =
                PlayerCallerOfAbility.CardsOnBoard.FindAll(x => x.Card.Prototype.CardSetType == SetType);

            foreach (BoardUnitModel unit in unitsOnBoard)
            {
                if (unit == AbilityUnitOwner)
                    continue;

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
                    default:
                        throw new ArgumentOutOfRangeException(nameof(StatType), StatType, null);
                }

                BoardUnitView unitView = BattlegroundController.GetBoardUnitViewByModel<BoardUnitView>(unit);
                CreateVfx(unitView.Transform.position, true);
            }
        }
    }
}
