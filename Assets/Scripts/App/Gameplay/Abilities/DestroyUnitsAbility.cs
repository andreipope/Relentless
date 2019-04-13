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

            _units = new List<CardModel>();

            foreach (Enumerators.Target target in AbilityTargets)
            {
                switch (target)
                {
                    case Enumerators.Target.OPPONENT_ALL_CARDS:
                        _units.AddRange(GetOpponentOverlord().CardsOnBoard);
                        break;
                    case Enumerators.Target.PLAYER_ALL_CARDS:
                        _units.AddRange(PlayerCallerOfAbility.PlayerCardsController.CardsOnBoard);

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

            if(AbilityData.SubTrigger == Enumerators.AbilitySubTrigger.RandomUnit)
            {
                _units = GetRandomUnits(_units, Count);
            }

            InvokeActionTriggered(_units);
        }

        public void DestroyUnit(BoardUnitView unit)
        {
            if(!unit.Model.HasBuffShield)
            {
                unit.ChangeModelVisibility(false);
            }
            BattlegroundController.DestroyBoardUnit(unit.Model, false);
        }

        protected override void VFXAnimationEndedHandler()
        {
            base.VFXAnimationEndedHandler();

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

                ActionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
                {
                    ActionType = actionType,
                    Caller = AbilityUnitOwner,
                    TargetEffects = targetEffects
                });
            }
        }
    }
}
