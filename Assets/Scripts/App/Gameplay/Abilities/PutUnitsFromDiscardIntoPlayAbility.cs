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

        private List<CardModel> _targets;

        private List<PastActionsPopup.TargetEffectParam> _targetEffects;

        private bool _targetsAreReady;

        public PutUnitsFromDiscardIntoPlayAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Count = ability.Count;

            _targets = new List<CardModel>();
            _targetEffects = new List<PastActionsPopup.TargetEffectParam>();
            _targetsAreReady = false;
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
                PrepareTargets();
                AbilityProcessingAction?.TriggerActionExternally();
                AbilityProcessingAction = ActionsQueueController.EnqueueAction(null, Enumerators.QueueActionType.AbilityUsageBlocker, blockQueue: true);
            }
        }

        public override void Action(object info = null)
        {
            base.Action(info);

            if (!_targetsAreReady)
            {
                PrepareTargets();
            }

            if (_targets.Count > 0)
            {
                foreach (CardModel target in _targets)
                {
                    PutCardOnBoard(target.OwnerPlayer, target, ref _targetEffects);
                }

                ActionsReportController.PostGameActionReport(new PastActionsPopup.PastActionParam()
                {
                    ActionType = Enumerators.ActionType.CardAffectingMultipleCards,
                    Caller = AbilityUnitOwner,
                    TargetEffects = _targetEffects
                });
            }

            AbilityProcessingAction?.TriggerActionExternally();
        }

        private void PutCardOnBoard(Player owner, CardModel cardModel, ref List<PastActionsPopup.TargetEffectParam> targetEffects)
        {
            owner.PlayerCardsController.RemoveCardFromGraveyard(cardModel);
            cardModel.ResetToInitial();

            Card prototype = new Card(DataManager.CachedCardsLibraryData.GetCardByName(cardModel.Card.Prototype.Name));
            InstanceId updatedId = new InstanceId(cardModel.InstanceId.Id, Enumerators.ReasonForInstanceIdChange.BackFromGraveyard);
            WorkingCard card = new WorkingCard(prototype, prototype, cardModel.OwnerPlayer, updatedId);
            CardModel resurrectedUnitModel = new CardModel(card);

            owner.PlayerCardsController.SpawnUnitOnBoard(resurrectedUnitModel, ItemPosition.End, IsPVPAbility);

            targetEffects.Add(new PastActionsPopup.TargetEffectParam()
            {
                ActionEffectType = Enumerators.ActionEffectType.SpawnOnBoard,
                Target = cardModel
            });
        }

        private void PrepareTargets() 
        {
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

            _targets = targets;
            _targetEffects = targetEffects;
            _targetsAreReady = true;
        }
    }
}
