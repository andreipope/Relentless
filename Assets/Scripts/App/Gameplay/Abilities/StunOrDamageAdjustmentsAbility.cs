using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using System.Collections.Generic;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class StunOrDamageAdjustmentsAbility : AbilityBase
    {
        public Enumerators.Stat StatType { get; }

        public int Value { get; } = 1;

        public StunOrDamageAdjustmentsAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            StatType = ability.Stat;
            Value = ability.Value;
        }

        protected override void VFXAnimationEndedHandler()
        {
            base.VFXAnimationEndedHandler();

            BoardUnitModel creature = (BoardUnitModel)TargetUnit;

            CreateVfx(BattlegroundController.GetBoardUnitViewByModel(creature).Transform.position);

            BoardUnitView leftAdjustment = null, rightAdjastment = null;

            int targetIndex = -1;
            for (int i = 0; i < creature.OwnerPlayer.BoardCards.Count; i++)
            {
                if (creature.OwnerPlayer.BoardCards[i].Model == creature)
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
                if (leftAdjustment.Model.IsStun)
                {
                    BattleController.AttackUnitByAbility(AbilityUnitOwner, AbilityData, leftAdjustment.Model);
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
                    BattleController.AttackUnitByAbility(AbilityUnitOwner, AbilityData, rightAdjastment.Model);
                }
                else
                {
                    rightAdjastment.Model.Stun(Enumerators.StunType.FREEZE, 1);
                }
            }

            if (creature.IsStun)
            {
                BattleController.AttackUnitByAbility(AbilityUnitOwner, AbilityData, creature);
            }
            else
            {
                creature.Stun(Enumerators.StunType.FREEZE, 1);
            }

            InvokeUseAbilityEvent(
                new List<ParametrizedAbilityBoardObject>
                {
                    new ParametrizedAbilityBoardObject(TargetUnit)
                }
            );
        }

        protected override void InputEndedHandler()
        {
            base.InputEndedHandler();

            if (IsAbilityResolved)
            {
                InvokeActionTriggered();
            }
        }
    }
}
