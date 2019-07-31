using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Loom.ZombieBattleground
{
    public class DestroyUnitsAbility : AbilityBase
    {
        public event Action OnUpdateEvent;

        private List<CardModel> _units;

        private bool _targetsAreReady;

        private int Count { get; }

        public DestroyUnitsAbility(Enumerators.CardKind cardKind, AbilityData ability)
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

        protected override void InputEndedHandler()
        {
            base.InputEndedHandler();

            if (IsAbilityResolved)
            {
                _units = new List<CardModel>();
                _units.Add(TargetUnit);

                AbilityProcessingAction?.TriggerActionExternally();
                AbilityProcessingAction = AbilityProcessingAction = ActionsQueueController.EnqueueAction(null, Enumerators.QueueActionType.AbilityUsageBlocker, blockQueue:true);

                InvokeActionTriggered(_units);
            }
        }

        public override void Update()
        {
            base.Update();

            OnUpdateEvent?.Invoke();
        }

        public override void Action(object info = null)
        {
            base.Action(info);

            if (!_targetsAreReady)
            {
                PrepareTargets();
            }

            _units = _units.OrderByDescending(x => x.InstanceId.Id).ToList();

            if(AbilityData.SubTrigger == Enumerators.AbilitySubTrigger.RandomUnit)
            {
                _units = GetRandomUnits(_units, Count);
            }

            foreach (CardModel target in _units)
            {
                target.HandleDefenseBuffer(target.CurrentDefense);
                target.SetUnitActiveStatus(false);
                target.InvokeAboutToDie();
            }

            AbilityProcessingAction?.TriggerActionExternally();
            AbilityProcessingAction = ActionsQueueController.EnqueueAction(null, Enumerators.QueueActionType.AbilityUsageBlocker, blockQueue:true);

            InvokeActionTriggered(_units);
        }

        protected override void UnitIsPreparingToDie()
        {
            base.UnitIsPreparingToDie();

            PrepareTargetsBeforeDeath();
        }

        public void DestroyUnit(CardModel unit)
        {
            bool withEffect = true;

            if (AbilityData.VisualEffectsToPlay != null && AbilityData.VisualEffectsToPlay.Count > 0)
            {
                withEffect = false;
            }

            BattlegroundController.DestroyBoardUnit(unit, withEffect);
            _units.Remove(unit);
        }

        protected override void VFXAnimationEndedHandler()
        {
            base.VFXAnimationEndedHandler();
            
            AbilityProcessingAction?.TriggerActionExternally();

            OnUpdateEvent = null;

            if (_units.Count > 0)
            {
                List<PastActionsPopup.TargetEffectParam> targetEffects = new List<PastActionsPopup.TargetEffectParam>();

                foreach (CardModel unit in _units)
                {
                    targetEffects = new List<PastActionsPopup.TargetEffectParam>()
                    {
                        new PastActionsPopup.TargetEffectParam()
                        {
                            ActionEffectType = Enumerators.ActionEffectType.DeathMark,
                            Target = unit
                        }
                    };
                }

                Enumerators.ActionType actionType = Enumerators.ActionType.CardAffectingCard;

                if (_units.Count > 1)
                {
                    actionType = Enumerators.ActionType.CardAffectingMultipleCards;
                }

                ActionsReportController.PostGameActionReport(new PastActionsPopup.PastActionParam()
                {
                    ActionType = actionType,
                    Caller = AbilityUnitOwner,
                    TargetEffects = targetEffects
                });
            }

            for (int i = _units.Count -1; i >= 0; i--)
            {
                DestroyUnit(_units[i]);
            }
        }

        private void PrepareTargets() 
        {
            _units = new List<CardModel>();

            foreach (Enumerators.Target target in AbilityTargets)
            {
                switch (target)
                {
                    case Enumerators.Target.OPPONENT_ALL_CARDS:
                        IReadOnlyList<CardModel> boardCardsOpponent = GetOpponentOverlord().CardsOnBoard;
                        for (int i = 0; i < boardCardsOpponent.Count; i++)
                        {
                            if (!boardCardsOpponent[i].IsDead && boardCardsOpponent[i].CurrentDefense > 0)
                            {
                                _units.Add(boardCardsOpponent[i]);
                            }
                        }
                        break;
                    case Enumerators.Target.PLAYER_ALL_CARDS:
                        IReadOnlyList<CardModel> boardCardsPlayers = PlayerCallerOfAbility.PlayerCardsController.CardsOnBoard;
                        for (int i = 0; i < boardCardsPlayers.Count; i++)
                        {
                            if (!boardCardsPlayers[i].IsDead && boardCardsPlayers[i].CurrentDefense > 0)
                            {
                                _units.Add(boardCardsPlayers[i]);
                            }
                        }

                        if (AbilityUnitOwner != null)
                        {
                            if (_units.Contains(AbilityUnitOwner))
                            {
                                _units.Remove(AbilityUnitOwner);
                            }
                        }
                        break;
                }
            }

            _targetsAreReady = true;
        }

        private void PrepareTargetsBeforeDeath()
        {
            PrepareTargets();
        }
    }
}
