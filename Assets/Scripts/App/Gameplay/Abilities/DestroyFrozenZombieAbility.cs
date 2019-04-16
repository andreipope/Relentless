using DG.Tweening;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class DestroyFrozenZombieAbility : AbilityBase
    {
        public DestroyFrozenZombieAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            TargetUnitSpecialStatus = ability.TargetUnitSpecialStatus;
        }

        public override void Activate()
        {
            base.Activate();

            if (AbilityTrigger != Enumerators.AbilityTrigger.ENTRY)
                return;

            if (AbilityActivity != Enumerators.AbilityActivity.PASSIVE)
                return;

            HandleSubTriggers();
        }

        protected override void InputEndedHandler()
        {
            base.InputEndedHandler();

            if (IsAbilityResolved)
            {
                InvokeActionTriggered();
            }
        }

        private void HandleSubTriggers()
        {
            List<CardModel> targets = new List<CardModel>();

            foreach(Enumerators.Target target in AbilityTargets)
            {
                switch(target)
                {
                    case Enumerators.Target.PLAYER_ALL_CARDS:
                    case Enumerators.Target.PLAYER_CARD:
                        targets.AddRange(PlayerCallerOfAbility.PlayerCardsController.CardsOnBoard);
                        break;
                    case Enumerators.Target.OPPONENT_ALL_CARDS:
                    case Enumerators.Target.OPPONENT_CARD:
                        targets.AddRange(GetOpponentOverlord().PlayerCardsController.CardsOnBoard);
                        break;
                }
            }

            if(TargetUnitSpecialStatus != Enumerators.UnitSpecialStatus.NONE)
            {
                targets = targets.FindAll(card => card.UnitSpecialStatus == TargetUnitSpecialStatus);
            }

            DestroyFrozenUnits(targets);
        }

        protected override void VFXAnimationEndedHandler()
        {
            base.VFXAnimationEndedHandler();

            if (TargetUnit != null)
            {
                DestroyFrozenUnits(new List<CardModel>() { TargetUnit });
            }
        }

        private void DestroyFrozenUnits(List<CardModel> boardUnits)
        {
            if (boardUnits == null || boardUnits.Count == 0)
                return;

            List<PastActionsPopup.TargetEffectParam> targetEffects = new List<PastActionsPopup.TargetEffectParam>();

            foreach (CardModel unit in boardUnits)
            {
                BattlegroundController.DestroyBoardUnit(TargetUnit, false);

                targetEffects.Add(new PastActionsPopup.TargetEffectParam()
                {
                    ActionEffectType = Enumerators.ActionEffectType.DeathMark,
                    Target = TargetUnit
                });
            }

            ActionsReportController.PostGameActionReport(new PastActionsPopup.PastActionParam()
            {
                ActionType = Enumerators.ActionType.CardAffectingMultipleCards,
                Caller = AbilityUnitOwner,
                TargetEffects = targetEffects
            });


            InvokeUseAbilityEvent(boardUnits.Select(card => new ParametrizedAbilityBoardObject(card)).ToList());
        }
    }
}
