using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class ChangeStat : IAbility
    {
        private BoardObject _abilityUnitOwner;

        private GameObject _vfxObject;

        public AbilityEnumerator.StatType StatType { get; }

        public NewAbilityData AbilityData { get; private set; }

        public void Init(NewAbilityData data, BoardObject owner)
        {
            AbilityData = data;
            _abilityUnitOwner = owner;
        }

        public void CallAction(object target)
        {
            //_vfxObject = LoadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/GreenHealVFX");
            switch (AbilityData.Stat)
            {
                case AbilityEnumerator.StatType.DEFENCE:
                    {
                        if (target is Player player)
                        {
                            player.Health += AbilityData.StatValue;
                        }
                        else if (target is BoardUnit unit)
                        {
                            unit.BuffedHp += AbilityData.StatValue;
                            unit.CurrentHp += AbilityData.StatValue;
                        }
                    }
                    break;
                case AbilityEnumerator.StatType.ATTACK:
                    {
                        if (target is BoardUnit unit)
                        {
                            unit.BuffedDamage += AbilityData.StatValue;
                            unit.CurrentDamage += AbilityData.StatValue;
                        }
                    }
                    break;
                default: break;
            }
        }
    }
}
