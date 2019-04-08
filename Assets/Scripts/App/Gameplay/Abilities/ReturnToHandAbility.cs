using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using System.Collections.Generic;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class ReturnToHandAbility : AbilityBase
    {
        public int Cost { get; }

        public ReturnToHandAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Cost = ability.Cost;
        }

        public override void Activate()
        {
            base.Activate();

            VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/PushVFX");
        }

        protected override void InputEndedHandler()
        {
            base.InputEndedHandler();

            if (IsAbilityResolved)
            {
                ReturnTargetToHand(TargetUnit);
            }
        }

        private void ReturnTargetToHand(BoardUnitModel unit)
        {
            Vector3 unitPosition = BattlegroundController.GetBoardUnitViewByModel<BoardUnitView>(TargetUnit).Transform.position;

            CreateVfx(unitPosition, true, 3f, true);

            CardsController.ReturnCardToHand(TargetUnit);

            if (AbilityData.SubTrigger == Enumerators.AbilitySubTrigger.HasChangesInParameters)
            {
                unit.Card.InstanceCard.Cost += Cost;
            }

            InvokeUseAbilityEvent(
                new List<ParametrizedAbilityBoardObject>
                {
                    new ParametrizedAbilityBoardObject(unit)
                }
            );

            ActionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
            {
                ActionType = Enumerators.ActionType.CardAffectingCard,
                Caller = GetCaller(),
                TargetEffects = new List<PastActionsPopup.TargetEffectParam>()
                {
                    new PastActionsPopup.TargetEffectParam()
                    {
                        ActionEffectType = Enumerators.ActionEffectType.Push,
                        Target = unit
                    }
                }
            });
        }
    }
}
