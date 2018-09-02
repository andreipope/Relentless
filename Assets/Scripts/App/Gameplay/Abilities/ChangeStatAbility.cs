using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;
using UnityEngine;

namespace LoomNetwork.CZB
{
    public class ChangeStatAbility : AbilityBase
    {
        public Enumerators.SetType SetType;

        public Enumerators.StatType StatType;

        public int Value = 1;

        public ChangeStatAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            StatType = ability.AbilityStatType;
            Value = ability.Value;
        }

        public override void Activate()
        {
            base.Activate();

            VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/GreenHealVFX");
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
            if ((AbilityCallType != Enumerators.AbilityCallType.Attack) || !isAttacker)
                return;

            switch (StatType)
            {
                case Enumerators.StatType.Health:
                    AbilityUnitOwner.BuffedHp += Value;
                    AbilityUnitOwner.CurrentHp += Value;
                    break;
                case Enumerators.StatType.Damage:
                    AbilityUnitOwner.BuffedDamage += Value;
                    AbilityUnitOwner.CurrentDamage += Value;
                    break;
            }

            // _ranksController.UpdateRanksBuffs();
        }
    }
}
