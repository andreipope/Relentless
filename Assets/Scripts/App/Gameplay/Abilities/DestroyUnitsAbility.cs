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

        private List<BoardUnitModel> _units;

        public DestroyUnitsAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
        }

        public override void Activate()
        {
            base.Activate();

            InvokeUseAbilityEvent();

            if (AbilityCallType != Enumerators.AbilityCallType.ENTRY)
                return;

            Action();
        }

        public override void Update()
        {
            base.Update();

            OnUpdateEvent?.Invoke();
        }

        public override void Action(object info = null)
        {
            base.Action(info);

            _units = new List<BoardUnitModel>();

            foreach (Enumerators.AbilityTargetType target in AbilityTargetTypes)
            {
                switch (target)
                {
                    case Enumerators.AbilityTargetType.OPPONENT_ALL_CARDS:
                        _units.AddRange(GetOpponentOverlord().CardsOnBoard);
                        break;
                    case Enumerators.AbilityTargetType.PLAYER_ALL_CARDS:
                        _units.AddRange(PlayerCallerOfAbility.CardsOnBoard);
                        break;
                }
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

                foreach (BoardUnitModel unit in _units)
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
                    Caller = GetCaller(),
                    TargetEffects = targetEffects
                });
            }
        }
    }
}
