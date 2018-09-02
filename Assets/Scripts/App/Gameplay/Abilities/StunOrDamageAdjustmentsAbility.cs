using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;
using UnityEngine;

namespace LoomNetwork.CZB
{
    public class StunOrDamageAdjustmentsAbility : AbilityBase
    {

        public Enumerators.StatType StatType { get; }

        public int Value { get; } = 1;

        public StunOrDamageAdjustmentsAbility(Enumerators.CardKind cardKind, AbilityData ability)
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
                case Enumerators.AbilityEffectType.STUN_OR_DAMAGE_FREEZES:
                    VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/FrozenVFX");
                    break;
                default:
                    VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/FrozenVFX");
                    break;
            }
        }

        public override void Action(object info = null)
        {
            base.Action(info);

            BoardUnit creature = info as BoardUnit;

            CreateVfx(creature.Transform.position);

            BoardUnit leftAdjustment = null, rightAdjastment = null;

            int targetIndex = -1;
            for (int i = 0; i < creature.OwnerPlayer.BoardCards.Count; i++)
            {
                if (creature.OwnerPlayer.BoardCards[i] == creature)
                {
                    targetIndex = i;
                }
            }

            if (targetIndex > -1)
            {
                if (targetIndex - 1 > -1)
                {
                    leftAdjustment = creature.OwnerPlayer.BoardCards[targetIndex - 1];
                }

                if (targetIndex + 1 < creature.OwnerPlayer.BoardCards.Count)
                {
                    rightAdjastment = creature.OwnerPlayer.BoardCards[targetIndex + 1];
                }
            }

            if (leftAdjustment != null)
            {
                if (leftAdjustment.IsStun)
                {
                    BattleController.AttackUnitByAbility(AbilityUnitOwner, AbilityData, leftAdjustment);
                }
                else
                {
                    leftAdjustment.Stun(Enumerators.StunType.FREEZE, 1);
                }

                // CreateVFX(leftAdjustment..transform.position);
            }

            if (rightAdjastment != null)
            {
                if (rightAdjastment.IsStun)
                {
                    BattleController.AttackUnitByAbility(AbilityUnitOwner, AbilityData, rightAdjastment);
                }
                else
                {
                    rightAdjastment.Stun(Enumerators.StunType.FREEZE, 1);
                }

                // CreateVFX(targetCreature.transform.position);
            }

            if (creature.IsStun)
            {
                BattleController.AttackUnitByAbility(AbilityUnitOwner, AbilityData, creature);
            }
            else
            {
                creature.Stun(Enumerators.StunType.FREEZE, 1);
            }
        }

        protected override void OnInputEndEventHandler()
        {
            base.OnInputEndEventHandler();

            if (IsAbilityResolved)
            {
                Action(TargetUnit);
            }
        }
    }
}
