using System.Collections.Generic;
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
                DevourTargetZombie(TargetUnitView);
            }
        }

        private void DevourAllAllyZombies()
        {
            List<BoardUnitView> units = PlayerCallerOfAbility.BoardCards;

            foreach (BoardUnitView unit in units)
            {
                DevourTargetZombie(unit);
            }
        }

        private void DevourTargetZombie(BoardUnitView unit)
        {
            if (unit.Equals(AbilityUnitViewOwner))
                return;

            int health = unit.Model.InitialHp;
            int damage = unit.Model.InitialDamage;

            BattlegroundController.DestroyBoardUnit(unit);

            AbilityUnitViewOwner.Model.BuffedHp += health;
            AbilityUnitViewOwner.Model.CurrentHp += health;

            AbilityUnitViewOwner.Model.BuffedDamage += damage;
            AbilityUnitViewOwner.Model.CurrentDamage += damage;

            CreateVfx(unit.Transform.position, true, 5f);
        }
    }
}
