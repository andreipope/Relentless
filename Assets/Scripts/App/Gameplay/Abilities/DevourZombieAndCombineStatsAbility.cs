using System.Collections.Generic;
using System.Linq;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class DevourZombiesAndCombineStatsAbility : AbilityBase
    {
        public int Value;

        public DevourZombiesAndCombineStatsAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Value = ability.Value;
        }

        public override void Activate()
        {
            base.Activate();

            if (AbilityCallType != Enumerators.AbilityCallType.ENTRY)
                return;

            VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/GreenHealVFX");

            if (Value == -1)
            {
                DevourAllAllyZombies();
            }
        }

        public override void Update()
        {
        }

        public override void Dispose()
        {
        }

        protected override void InputEndedHandler()
        {
            base.InputEndedHandler();

            if (IsAbilityResolved && Value > 0)
            {
                DevourTargetZombie(TargetUnit);

                AbilitiesController.ThrowUseAbilityEvent(MainWorkingCard, new List<BoardObject>()
                {
                    TargetUnit
                }, AbilityData.AbilityType, Protobuf.AffectObjectType.Character);
            }
        }

        private void DevourAllAllyZombies()
        {
            List<BoardUnitModel> units;

            if (PredefinedTargets != null)
            {
                units = PredefinedTargets.Cast<BoardUnitModel>().ToList();
            }
            else
            {
                units = PlayerCallerOfAbility.BoardCards.Select(x => x.Model).ToList();
            }

            foreach (BoardUnitModel unit in units)
            {
                DevourTargetZombie(unit);
            }

            AbilitiesController.ThrowUseAbilityEvent(MainWorkingCard, units.Cast<BoardObject>().ToList(), AbilityData.AbilityType, Protobuf.AffectObjectType.Character);
        }

        private void DevourTargetZombie(BoardUnitModel unit)
        {
            if (unit == AbilityUnitOwner)
                return;

            int health = unit.InitialHp;
            int damage = unit.InitialDamage;

            BattlegroundController.DestroyBoardUnit(unit);

            AbilityUnitOwner.BuffedHp += health;
            AbilityUnitOwner.CurrentHp += health;

            AbilityUnitOwner.BuffedDamage += damage;
            AbilityUnitOwner.CurrentDamage += damage;

            CreateVfx(BattlegroundController.GetBoardUnitViewByModel(unit).Transform.position, true, 5f);
        }
    }
}
