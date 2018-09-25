using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class GainNumberOfLifeForEachDamageThisDealsAbility : AbilityBase
    {
        public int Value { get; }

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

        public override void Action(object info = null)
        {
            base.Action(info);

            int damageDeal = (int) info;

            AbilityUnitViewOwner.Model.BuffedHp += Value * damageDeal;
            AbilityUnitViewOwner.Model.CurrentHp += Value * damageDeal;

            CreateVfx(AbilityUnitViewOwner.Transform.position, true);
        }

        protected override void UnitAttackedHandler(object info, int damage, bool isAttacker)
        {
            base.UnitAttackedHandler(info, damage, isAttacker);

            if (AbilityCallType != Enumerators.AbilityCallType.ATTACK || !isAttacker)
                return;

            Action(damage);
        }
    }
}
