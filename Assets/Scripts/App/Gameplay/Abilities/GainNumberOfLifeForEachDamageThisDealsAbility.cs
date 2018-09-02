using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;
using UnityEngine;

namespace LoomNetwork.CZB
{
    public class GainNumberOfLifeForEachDamageThisDealsAbility : AbilityBase
    {
        public int Value;

        public GainNumberOfLifeForEachDamageThisDealsAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
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

        public override void Action(object info = null)
        {
            base.Action(info);

            int damageDeal = (int)info;

            AbilityUnitOwner.BuffedHp += Value * damageDeal;
            AbilityUnitOwner.CurrentHp += Value * damageDeal;

            CreateVfx(AbilityUnitOwner.Transform.position, true);
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

            Action(damage);
        }
    }
}
