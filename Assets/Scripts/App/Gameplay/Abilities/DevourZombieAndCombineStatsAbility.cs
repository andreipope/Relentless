using System.Collections.Generic;
using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;
using UnityEngine;

namespace LoomNetwork.CZB
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

            if (AbilityCallType != Enumerators.AbilityCallType.Entry)
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

        protected override void OnInputEndEventHandler()
        {
            base.OnInputEndEventHandler();

            if (IsAbilityResolved && (Value > 0))
            {
                DevourTargetZombie(TargetUnit);
            }
        }

        private void DevourAllAllyZombies()
        {
            List<BoardUnit> units = PlayerCallerOfAbility.BoardCards;

            foreach (BoardUnit unit in units)
            {
                DevourTargetZombie(unit);
            }
        }

        private void DevourTargetZombie(BoardUnit unit)
        {
            if (unit.Equals(AbilityUnitOwner))
                return;

            int health = unit.InitialHp;
            int damage = unit.InitialDamage;

            BattlegroundController.DestroyBoardUnit(unit);

            AbilityUnitOwner.BuffedHp += health;
            AbilityUnitOwner.CurrentHp += health;

            AbilityUnitOwner.BuffedDamage += damage;
            AbilityUnitOwner.CurrentDamage += damage;

            CreateVfx(unit.Transform.position, true, 5f);
        }
    }
}
