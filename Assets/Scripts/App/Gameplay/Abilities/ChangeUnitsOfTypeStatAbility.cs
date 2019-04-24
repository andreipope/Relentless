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
        public Enumerators.Faction Faction;

        public Enumerators.Stat StatType;

        public int Value = 1;

        public ChangeUnitsOfTypeStatAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            StatType = ability.Stat;
            Faction = ability.Faction;
            Value = ability.Value;
        }

        public override void Activate()
        {
            base.Activate();

            switch (StatType)
            {
                case Enumerators.Stat.DEFENSE:
                case Enumerators.Stat.DAMAGE:
                default:
                    VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/GreenHealVFX");
                    break;
            }

            InvokeUseAbilityEvent();

            if (AbilityTrigger != Enumerators.AbilityTrigger.PERMANENT)
                return;

            Action();
        }

        private void Action()
        {
            IReadOnlyList<BoardUnitModel> unitsOnBoard =
                PlayerCallerOfAbility.CardsOnBoard.FindAll(x => x.Card.Prototype.Faction == Faction);

            foreach (BoardUnitModel unit in unitsOnBoard)
            {
                if (unit == AbilityUnitOwner)
                    continue;

                switch (StatType)
                {
                    case Enumerators.Stat.DAMAGE:
                        unit.BuffedDamage += Value;
                        unit.AddToCurrentDamageHistory(Value, Enumerators.ReasonForValueChange.AbilityBuff);
                        break;
                    case Enumerators.Stat.DEFENSE:
                        unit.BuffedDefense += Value;
                        unit.AddToCurrentDefenseHistory(Value, Enumerators.ReasonForValueChange.AbilityBuff);
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
