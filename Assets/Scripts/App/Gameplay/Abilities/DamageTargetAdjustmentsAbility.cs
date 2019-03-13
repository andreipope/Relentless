using System;
using System.Collections.Generic;
using DG.Tweening;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Loom.ZombieBattleground
{
    public class DamageTargetAdjustmentsAbility : AbilityBase
    {
        public int Value { get; }

        private List<BoardUnitView> _targetUnits;

        public DamageTargetAdjustmentsAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Value = ability.Value;
        }

        public override void Activate()
        {
            base.Activate();

            switch (AbilityEffectType)
            {
                case Enumerators.AbilityEffectType.TARGET_ADJUSTMENTS_AIR:
                    VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/WhirlwindVFX");
                    break;
                default:
                    VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Spells/SpellTargetToxicAttack");
                    break;
            }

            if(AbilityTrigger == Enumerators.AbilityTrigger.ATTACK)
            {
                InvokeUseAbilityEvent();
            }
        }

        public override void Update()
        {
        }

        public override void Dispose()
        {
        }

        public override void Action(object info = null)
        {
            AbilityProcessingAction = ActionsQueueController.AddNewActionInToQueue(null, Enumerators.QueueActionType.AbilityUsageBlocker);

            base.Action(info);

            _targetUnits = new List<BoardUnitView>();

            BoardUnitModel unit = (BoardUnitModel) info;

            Player playerOwner = unit.OwnerPlayer;

            BoardUnitView leftAdjustment = null, rightAdjastment = null;

            int targetIndex = -1;
            IReadOnlyList<BoardUnitView> list = null;
            for (int i = 0; i < playerOwner.BoardCards.Count; i++)
            {
                if (playerOwner.BoardCards[i].Model == unit)
                {
                    targetIndex = i;
                    list = playerOwner.BoardCards;
                    break;
                }
            }

            if (targetIndex > -1)
            {
                if (targetIndex - 1 > -1)
                {
                    leftAdjustment = list[targetIndex - 1];
                }

                if (targetIndex + 1 < list.Count)
                {
                    rightAdjastment = list[targetIndex + 1];
                }
            }

            _targetUnits.Add(BattlegroundController.GetBoardUnitViewByModel(unit));

            if (leftAdjustment != null)
            {
                _targetUnits.Add(leftAdjustment);
            }

            if (rightAdjastment != null)
            {
                _targetUnits.Add(rightAdjastment);
            }

            InvokeActionTriggered(_targetUnits);
        }

        protected override void InputEndedHandler()
        {
            base.InputEndedHandler();

            if (IsAbilityResolved)
            {
                switch (AffectObjectType)
                {
                    case Enumerators.AffectObjectType.Character:
                        Action(TargetUnit);
                        break;
                }
            }
        }

        protected override void UnitAttackedHandler(BoardObject info, int damage, bool isAttacker)
        {
            base.UnitAttackedHandler(info, damage, isAttacker);

            if (AbilityTrigger != Enumerators.AbilityTrigger.ATTACK || !isAttacker)
                return;

            Action(info);
        }


        protected override void VFXAnimationEndedHandler()
        {
            base.VFXAnimationEndedHandler();

            ActionCompleted();

            AbilityProcessingAction?.ForceActionDone();
        }

        private void ActionCompleted()
        {
            object caller = AbilityUnitOwner != null ? AbilityUnitOwner : (object)BoardItem;

            foreach (var unit in _targetUnits)
            {
                BattleController.AttackUnitByAbility(caller, AbilityData, unit.Model);
            }

            InvokeUseAbilityEvent(
                new List<ParametrizedAbilityBoardObject>
                {
                    new ParametrizedAbilityBoardObject(TargetUnit)
                }
            );
        }
    }
}
