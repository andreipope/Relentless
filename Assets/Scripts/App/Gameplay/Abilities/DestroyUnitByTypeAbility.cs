using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using System.Collections.Generic;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class DestroyUnitByTypeAbility : AbilityBase
    {
        public DestroyUnitByTypeAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            TargetCardType = ability.TargetCardType;
        }

        public override void Activate()
        {
            base.Activate();
        }

        public override void Action(object info = null)
        {
            base.Action(info);

            ThrowUseAbilityEvent(
                new List<ParametrizedAbilityBoardObject> {
                    new ParametrizedAbilityBoardObject(TargetUnit)
                }
            );
            BattlegroundController.DestroyBoardUnit(TargetUnit);

            ActionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
            {
                ActionType = Enumerators.ActionType.CardAffectingCard,
                Caller = GetCaller(),
                TargetEffects = new List<PastActionsPopup.TargetEffectParam>()
                    {
                        new PastActionsPopup.TargetEffectParam()
                        {
                            ActionEffectType = Enumerators.ActionEffectType.DeathMark,
                            Target = TargetUnit
                        }
                    }
            });
        }

        protected override void InputEndedHandler()
        {
            base.InputEndedHandler();

            if (IsAbilityResolved)
            {
                InvokeActionTriggered();
            }
        }

        protected override void VFXAnimationEndedHandler()
        {
            base.VFXAnimationEndedHandler();

            Action();
        }
    }
}
