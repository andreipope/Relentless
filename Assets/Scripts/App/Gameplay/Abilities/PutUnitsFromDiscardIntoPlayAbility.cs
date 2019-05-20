using System.Collections.Generic;
using System.Linq;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class PutUnitsFromDiscardIntoPlayAbility : AbilityBase
    {
        public int Count { get; }

        public PutUnitsFromDiscardIntoPlayAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Count = ability.Count;
        }

        public override void Activate()
        {
            base.Activate();

            InvokeUseAbilityEvent();

            if (AbilityTrigger != Enumerators.AbilityTrigger.ENTRY)
                return;

            Action();
        }

        protected override void UnitDiedHandler()
        {
            base.UnitDiedHandler();

            if (AbilityTrigger != Enumerators.AbilityTrigger.DEATH)
                return;

            Action();
        }

        protected override void UnitHpChangedHandler(int oldValue, int newValue)
        {
            base.UnitHpChangedHandler(oldValue, newValue);

            if (AbilityUnitOwner.CurrentDefense <= 0)
            {
                AbilityProcessingAction = ActionsQueueController.EnqueueAction(null, Enumerators.QueueActionType.AbilityUsageBlocker, blockQueue: true);
            }
        }

        public override void Action(object info = null)
        {
            base.Action(info);

            List<CardModel> targets = new List<CardModel>();
            List<PastActionsPopup.TargetEffectParam> targetEffects = new List<PastActionsPopup.TargetEffectParam>();

            Player playerOwner = null;
            
            foreach (Enumerators.Target targetType in AbilityData.Targets)
            {
                switch (targetType)
                {
                    case Enumerators.Target.PLAYER:
                        playerOwner = PlayerCallerOfAbility;
                        break;
                    case Enumerators.Target.OPPONENT:
                        playerOwner = GetOpponentOverlord();
                        break;
                }

                List<CardModel> elements =
                    playerOwner.PlayerCardsController.CardsInGraveyard
                        .FindAll(card => card.Card.Prototype.Kind == Enumerators.CardKind.CREATURE && card != AbilityUnitOwner);

                elements = elements.OrderByDescending(x => x.InstanceId.Id).ToList();
                
                if (AbilityData.SubTrigger == Enumerators.AbilitySubTrigger.RandomUnit)
                {
                    elements = GetRandomElements(elements, Count);
                }

                if (HasEmptySpaceOnBoard(playerOwner, out int emptyFields) && elements.Count > 0)
                {
                    for (int i = 0; i < emptyFields; i++)
                    {
                        if (i >= elements.Count)
                            break;

                        targets.Add(elements[i]);
                    }
                }
            }

            if (targets.Count > 0)
            {
                foreach (CardModel target in targets)
                {
                    PutCardOnBoard(target.OwnerPlayer, target, ref targetEffects);
                }

                ActionsReportController.PostGameActionReport(new PastActionsPopup.PastActionParam()
                {
                    ActionType = Enumerators.ActionType.CardAffectingMultipleCards,
                    Caller = AbilityUnitOwner,
                    TargetEffects = targetEffects
                });
            }

            AbilityProcessingAction?.TriggerActionExternally();
        }

        private void PutCardOnBoard(Player owner, CardModel cardModel, ref List<PastActionsPopup.TargetEffectParam> targetEffects)
        {
            owner.PlayerCardsController.RemoveCardFromGraveyard(cardModel);
            cardModel.ResetToInitial();
            owner.PlayerCardsController.SpawnUnitOnBoard(cardModel, ItemPosition.End, IsPVPAbility);

            targetEffects.Add(new PastActionsPopup.TargetEffectParam()
            {
                ActionEffectType = Enumerators.ActionEffectType.SpawnOnBoard,
                Target = cardModel
            });
        }
    }
}
