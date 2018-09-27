using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using UnityEngine;

namespace Loom.ZombieBattleground
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

            BoardUnitView creature = (BoardUnitView) info;

            CreateVfx(creature.Transform.position);

            BoardUnitView leftAdjustment = null, rightAdjastment = null;

            int targetIndex = -1;
            for (int i = 0; i < creature.Model.OwnerPlayer.BoardCards.Count; i++)
            {
                if (creature.Model.OwnerPlayer.BoardCards[i] == creature)
                {
                    targetIndex = i;
                }
            }

            if (targetIndex > -1)
            {
                if (targetIndex - 1 > -1)
                {
                    leftAdjustment = creature.Model.OwnerPlayer.BoardCards[targetIndex - 1];
                }

                if (targetIndex + 1 < creature.Model.OwnerPlayer.BoardCards.Count)
                {
                    rightAdjastment = creature.Model.OwnerPlayer.BoardCards[targetIndex + 1];
                }
            }

            if (leftAdjustment != null)
            {
                if (leftAdjustment.Model.IsStun)
                {
                    BattleController.AttackUnitByAbility(AbilityUnitViewOwner, AbilityData, leftAdjustment.Model);
                }
                else
                {
                    leftAdjustment.Model.Stun(Enumerators.StunType.FREEZE, 1);
                }
            }

            if (rightAdjastment != null)
            {
                if (rightAdjastment.Model.IsStun)
                {
                    BattleController.AttackUnitByAbility(AbilityUnitViewOwner, AbilityData, rightAdjastment.Model);
                }
                else
                {
                    rightAdjastment.Model.Stun(Enumerators.StunType.FREEZE, 1);
                }
            }

            if (creature.Model.IsStun)
            {
                BattleController.AttackUnitByAbility(AbilityUnitViewOwner, AbilityData, creature.Model);
            }
            else
            {
                creature.Model.Stun(Enumerators.StunType.FREEZE, 1);
            }
        }

        protected override void InputEndedHandler()
        {
            base.InputEndedHandler();

            if (IsAbilityResolved)
            {
                Action(TargetUnitView);
            }
        }
    }
}
