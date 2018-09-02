using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;
using UnityEngine;

namespace LoomNetwork.CZB
{
    public class StunOrDamageAdjustmentsAbility : AbilityBase
    {
        public Enumerators.StatType statType;

        public int value = 1;

        public StunOrDamageAdjustmentsAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            statType = ability.abilityStatType;
            value = ability.value;
        }

        public override void Activate()
        {
            base.Activate();

            switch (abilityEffectType)
            {
                case Enumerators.AbilityEffectType.STUN_OR_DAMAGE_FREEZES:
                    _vfxObject = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/FrozenVFX");
                    break;
                default:
                    _vfxObject = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/FrozenVFX");
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

        public override void Action(object info = null)
        {
            base.Action(info);

            BoardUnit creature = info as BoardUnit;

            CreateVFX(creature.transform.position);

            BoardUnit leftAdjustment = null, rightAdjastment = null;

            int targetIndex = -1;
            for (int i = 0; i < creature.ownerPlayer.BoardCards.Count; i++)
            {
                if (creature.ownerPlayer.BoardCards[i] == creature)
                {
                    targetIndex = i;
                }
            }

            if (targetIndex > -1)
            {
                if (targetIndex - 1 > -1)
                {
                    leftAdjustment = creature.ownerPlayer.BoardCards[targetIndex - 1];
                }

                if (targetIndex + 1 < creature.ownerPlayer.BoardCards.Count)
                {
                    rightAdjastment = creature.ownerPlayer.BoardCards[targetIndex + 1];
                }
            }

            if (leftAdjustment != null)
            {
                if (leftAdjustment.IsStun)
                {
                    _battleController.AttackUnitByAbility(abilityUnitOwner, abilityData, leftAdjustment);
                } else
                {
                    leftAdjustment.Stun(Enumerators.StunType.FREEZE, 1);
                }

                // CreateVFX(leftAdjustment..transform.position);
            }

            if (rightAdjastment != null)
            {
                if (rightAdjastment.IsStun)
                {
                    _battleController.AttackUnitByAbility(abilityUnitOwner, abilityData, rightAdjastment);
                } else
                {
                    rightAdjastment.Stun(Enumerators.StunType.FREEZE, 1);
                }

                // CreateVFX(targetCreature.transform.position);
            }

            if (creature.IsStun)
            {
                _battleController.AttackUnitByAbility(abilityUnitOwner, abilityData, creature);
            } else
            {
                creature.Stun(Enumerators.StunType.FREEZE, 1);
            }
        }

        protected override void OnInputEndEventHandler()
        {
            base.OnInputEndEventHandler();

            if (_isAbilityResolved)
            {
                Action(targetUnit);
            }
        }

        protected override void UnitOnAttackEventHandler(object info, int damage, bool isAttacker)
        {
            base.UnitOnAttackEventHandler(info, damage, isAttacker);
        }
    }
}
