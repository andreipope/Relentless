using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;
using UnityEngine;

namespace LoomNetwork.CZB
{
    public class ChangeStatAbility : AbilityBase
    {
        public Enumerators.SetType setType;

        public Enumerators.StatType statType;

        public int value = 1;

        public ChangeStatAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            statType = ability.abilityStatType;
            value = ability.value;
        }

        public override void Activate()
        {
            base.Activate();

            _vfxObject = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/GreenHealVFX");
        }

        public override void Update()
        {
            base.Update();
        }

        public override void Dispose()
        {
            base.Dispose();
        }

        protected override void OnInputEndEventHandler()
        {
            base.OnInputEndEventHandler();
        }

        protected override void UnitOnAttackEventHandler(object info, int damage, bool isAttacker)
        {
            base.UnitOnAttackEventHandler(info, damage, isAttacker);
            if ((abilityCallType != Enumerators.AbilityCallType.ATTACK) || !isAttacker)

                return;

            switch (statType)
            {
                case Enumerators.StatType.HEALTH:
                    abilityUnitOwner.BuffedHP += value;
                    abilityUnitOwner.CurrentHP += value;
                    break;
                case Enumerators.StatType.DAMAGE:
                    abilityUnitOwner.BuffedDamage += value;
                    abilityUnitOwner.CurrentDamage += value;
                    break;
            }

            // _ranksController.UpdateRanksBuffs();
        }
    }
}
