using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;
using UnityEngine;

namespace LoomNetwork.CZB
{
    public class StunAbility : AbilityBase
    {
        public Enumerators.StatType StatType;

        public int Value = 1;

        public StunAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            StatType = ability.AbilityStatType;
            Value = ability.Value;
        }

        public override void Activate()
        {
            base.Activate();

            switch (AbilityEffectType)
            {
                case Enumerators.AbilityEffectType.STUN_FREEZES:
                    VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/FrozenVFX");
                    break;
                default:
                    VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/FrozenVFX");
                    break;
            }
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
            if (AbilityCallType != Enumerators.AbilityCallType.ATTACK || !isAttacker)
                return;

            if (info is BoardUnit)
            {
                BoardUnit creature = info as BoardUnit;
                creature.Stun(Enumerators.StunType.FREEZE, Value);
                CreateVfx(creature.Transform.position);

                ActionsQueueController.PostGameActionReport(ActionsQueueController.FormatGameActionReport(Enumerators.ActionType.STUN_CREATURE_BY_ABILITY, new object[] { AbilityUnitOwner, creature }));
            }
        }
    }
}
