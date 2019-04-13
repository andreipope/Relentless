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

        private List<CardModel> _targetUnits;

        public DamageTargetAdjustmentsAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Value = ability.Value;
        }

        public override void Activate()
        {
            base.Activate();

            switch (AbilityEffect)
            {
                case Enumerators.AbilityEffect.TARGET_ADJUSTMENTS_AIR:
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

            _targetUnits = new List<CardModel>();

            CardModel unit = (CardModel) info;

            Player playerOwner = unit.OwnerPlayer;

            CardModel leftAdjustment = null, rightAdjustment = null;

            int targetIndex = -1;
            IReadOnlyList<CardModel> list = null;
            for (int i = 0; i < playerOwner.CardsOnBoard.Count; i++)
            {
                if (playerOwner.CardsOnBoard[i] == unit)
                {
                    targetIndex = i;
                    list = playerOwner.CardsOnBoard;
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
                    rightAdjustment = list[targetIndex + 1];
                }
            }

            _targetUnits.Add(unit);

            if (leftAdjustment != null)
            {
                _targetUnits.Add(leftAdjustment);
            }

            if (rightAdjustment != null)
            {
                _targetUnits.Add(rightAdjustment);
            }

            foreach(CardModel target in _targetUnits)
            {
                target.HandleDefenseBuffer(Value);
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

        protected override void UnitAttackedHandler(IBoardObject info, int damage, bool isAttacker)
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
            foreach (CardModel unit in _targetUnits)
            {
                BattleController.AttackUnitByAbility(AbilityUnitOwner, AbilityData, unit);
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
