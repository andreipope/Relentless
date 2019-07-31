using System;
using System.Collections.Generic;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;

namespace Loom.ZombieBattleground
{
    public class MassiveDamageAbility : AbilityBase
    {
        private int Damage;

        public event Action OnUpdateEvent;

        private bool _targetsAreReady;

        private List<IBoardObject> _targets;

        public MassiveDamageAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Damage = ability.Value;
        }

        public override void Activate()
        {
            base.Activate();

            InvokeUseAbilityEvent();
            if (AbilityTrigger != Enumerators.AbilityTrigger.ENTRY)
                return;

            TakeDamage(true);
        }

        public override void Update()
        {
            base.Update();
            OnUpdateEvent?.Invoke();
        }

        protected override void UnitDiedHandler()
        {
            if (AbilityTrigger != Enumerators.AbilityTrigger.DEATH) {
                base.UnitDiedHandler();
                return;
            }

            TakeDamage();
        }

        protected override void TurnEndedHandler()
        {
            base.TurnEndedHandler();

            if (AbilityTrigger != Enumerators.AbilityTrigger.END ||
                     !GameplayManager.CurrentTurnPlayer.Equals(PlayerCallerOfAbility))
                return;

            TakeDamage();
        }

        protected override void UnitAttackedHandler(IBoardObject info, int damage, bool isAttacker)
        {
            base.UnitAttackedHandler(info, damage, isAttacker);
            if (AbilityTrigger != Enumerators.AbilityTrigger.ATTACK || !isAttacker)
                return;

            _targetsAreReady = false;
            TakeDamage();
        }

        protected override void VFXAnimationEndedHandler()
        {
            base.VFXAnimationEndedHandler();

            AbilityProcessingAction?.TriggerActionExternally();

            if (_targets.Count > 0)
            {
                List<PastActionsPopup.TargetEffectParam> targetEffects = new List<PastActionsPopup.TargetEffectParam>();

                foreach (IBoardObject unit in _targets)
                {
                    targetEffects.Add(new PastActionsPopup.TargetEffectParam
                    {
                        ActionEffectType = Enumerators.ActionEffectType.Damage,
                        Target = unit
                    });
                }

                Enumerators.ActionType actionType = Enumerators.ActionType.CardAffectingCard;

                if (_targets.Count > 1)
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

            for (int i = _targets.Count-1; i >= 0; i--)
            {
                OneActionCompleted(_targets[i]);
            }

            if (TutorialManager.IsTutorial && AbilityTrigger == Enumerators.AbilityTrigger.DEATH)
            {
                TutorialManager.ReportActivityAction(Enumerators.TutorialActivityAction.DeathAbilityCompleted);
            }
        }

        private void PrepareTargets(bool exceptCaller = false)
        {
            _targets = new List<IBoardObject>();

            foreach (Enumerators.Target target in AbilityTargets)
            {
                switch (target)
                {
                    case Enumerators.Target.OPPONENT_ALL_CARDS:
                    case Enumerators.Target.OPPONENT_CARD:
                        _targets.AddRange(GetAliveUnits(GetOpponentOverlord().PlayerCardsController.CardsOnBoard));
                        break;
                    case Enumerators.Target.PLAYER_ALL_CARDS:
                    case Enumerators.Target.PLAYER_CARD:
                        _targets.AddRange(GetAliveUnits(PlayerCallerOfAbility.PlayerCardsController.CardsOnBoard));

                        if (exceptCaller && _targets.Contains(AbilityUnitOwner))
                        {
                            _targets.Remove(AbilityUnitOwner);
                        }
                        break;
                    case Enumerators.Target.OPPONENT:
                        _targets.Add(GetOpponentOverlord());
                        break;
                    case Enumerators.Target.PLAYER:
                        _targets.Add(PlayerCallerOfAbility);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(target), target, null);
                }
            }

            if(AbilityData.SubTrigger == Enumerators.AbilitySubTrigger.EqualToUnitAttack)
            {
                Damage = CardModel.CurrentDamage;
            }

            _targetsAreReady = true;
        }

        private void TakeDamage(bool exceptCaller = false)
        {
            _targets = new List<IBoardObject>();

            if (!_targetsAreReady)
            {
                PrepareTargets(exceptCaller);
            }

            foreach(IBoardObject boardObject in _targets)
            {
                if (boardObject is CardModel boardUnit)
                {
                    boardUnit.HandleDefenseBuffer(Damage);
                }
            }

            AbilityProcessingAction?.TriggerActionExternally();
            AbilityProcessingAction = ActionsQueueController.EnqueueAction(null, Enumerators.QueueActionType.AbilityUsageBlocker, blockQueue:true);

            InvokeActionTriggered(_targets);
        }

        private void PrepareTargetsBeforeDeath()
        {
            PrepareTargets();
        }

        protected override void PrepairingToDieHandler(IBoardObject from)
        {
            base.PrepairingToDieHandler(from);

            PrepareTargetsBeforeDeath();
        }

        protected override void UnitHpChangedHandler(int oldValue, int newValue)
        {
            base.UnitHpChangedHandler(oldValue, newValue);

            if (AbilityTrigger != Enumerators.AbilityTrigger.DEATH) 
                return;

            if (AbilityUnitOwner.CurrentDefense <= 0)
            {
                PrepareTargetsBeforeDeath();
            }
        }

        public void OneActionCompleted(IBoardObject boardObject)
        {
            switch (boardObject)
            {
                case Player player:
                    BattleController.AttackPlayerByAbility(AbilityUnitOwner, AbilityData, player, Damage);
                    break;
                case CardModel unit:
                    BattleController.AttackUnitByAbility(AbilityUnitOwner, AbilityData, unit, Damage);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(boardObject), boardObject, null);
            }
            _targets.Remove(boardObject);
        }
    }
}
