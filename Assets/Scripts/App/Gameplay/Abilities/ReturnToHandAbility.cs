using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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

            if (AbilityTrigger != Enumerators.AbilityTrigger.ENTRY)
            {
                InvokeUseAbilityEvent();
                return;
            }
        }

        protected override void InputEndedHandler()
        {
            base.InputEndedHandler();

            if (IsAbilityResolved)
            {
                ReturnTargetToHand(TargetUnit);

                InvokeUseAbilityEvent(
                    new List<ParametrizedAbilityBoardObject>
                    {
                        new ParametrizedAbilityBoardObject(TargetUnit)
                    });
            }
        }

        protected override void UnitDiedHandler()
        {
            base.UnitDiedHandler();

            if (AbilityTrigger != Enumerators.AbilityTrigger.DEATH)
                return;

            if (AbilityTargets.Contains(Enumerators.Target.ITSELF))
            {
                ReturnTargetToHand(AbilityUnitOwner);
            }
        }

        private void ReturnTargetToHand(CardModel unit)
        {
            int currentCost = unit.CurrentCost;
            if (AbilityTrigger != Enumerators.AbilityTrigger.DEATH)
            {
                Vector3 unitPosition = BattlegroundController.GetCardViewByModel<BoardUnitView>(unit).Transform.position;

                CreateVfx(unitPosition, true, 3f, true);

                CardsController.ReturnCardToHand(unit);
            }
            else
            {
                ReturnDeadTargetToHand(unit);
            }

            if (AbilityData.SubTrigger == Enumerators.AbilitySubTrigger.HasChangesInParameters)
            {
                unit.AddToCurrentCostHistory((currentCost - unit.CurrentCost) + Cost, Enumerators.ReasonForValueChange.AbilityBuff);

                if (PlayerCallerOfAbility.IsLocalPlayer)
                {
                    BattlegroundController.GetCardViewByModel<BoardCardView>(CardModel).UpdateCardCost();
                }
            }

            ActionsReportController.PostGameActionReport(new PastActionsPopup.PastActionParam()
            {
                ActionType = Enumerators.ActionType.CardAffectingCard,
                Caller = AbilityUnitOwner,
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

        private void ReturnDeadTargetToHand(CardModel unit)
        {
            unit.ResetToInitial();

            unit.Owner.PlayerCardsController.ReturnToHandBoardUnit(unit, new Vector3());

            GameClient.Get<IGameplayManager>().RearrangeHands();
        }
    }
}
