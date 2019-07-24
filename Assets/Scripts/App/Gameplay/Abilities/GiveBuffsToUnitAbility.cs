using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using System.Collections.Generic;
using System.Linq;

namespace Loom.ZombieBattleground
{
    public class GiveBuffsToUnitAbility : AbilityBase
    {
        public GiveBuffsToUnitAbility(Enumerators.CardKind cardKind, AbilityData ability) : base(cardKind, ability)
        {
        }

        public override void Activate()
        {
            base.Activate();

            if (AbilityTrigger != Enumerators.AbilityTrigger.ENTRY || AbilityActivity != Enumerators.AbilityActivity.PASSIVE)
                return;

            InvokeUseAbilityEvent();

            CheckSubTriggers();
        }

        protected override void InputEndedHandler()
        {
            base.InputEndedHandler();

            if (IsAbilityResolved)
            {
                GiveBuffsToUnit(TargetUnit);

                InvokeUseAbilityEvent(new List<ParametrizedAbilityBoardObject>()
                {
                    new ParametrizedAbilityBoardObject(TargetUnit)
                });
            }
        }

        private void CheckSubTriggers()
        {
            if (AbilityData.SubTrigger == Enumerators.AbilitySubTrigger.OnlyThisUnitInPlay)
            {
                if (GetAliveUnits(PlayerCallerOfAbility.PlayerCardsController.CardsOnBoard).Where(card => card != AbilityUnitOwner).Count() == 0)
                {
                    GiveBuffsToUnit(AbilityUnitOwner);
                }
            }
        }

        private void GiveBuffsToUnit(CardModel unit)
        {
            List<PastActionsPopup.TargetEffectParam> TargetEffects = new List<PastActionsPopup.TargetEffectParam>();
            Enumerators.ActionEffectType actionEffectType = Enumerators.ActionEffectType.None;
            foreach (Enumerators.GameMechanicDescription type in AbilityData.TargetGameMechanicDescriptions)
            {
                switch (type)
                {
                    case Enumerators.GameMechanicDescription.Guard:
                        unit.AddBuffShield();
                        actionEffectType = Enumerators.ActionEffectType.Guard;
                        break;
                    case Enumerators.GameMechanicDescription.Destroy:
                        unit.ApplyBuff(Enumerators.BuffType.DESTROY);
                        actionEffectType = Enumerators.ActionEffectType.DeathMark;
                        break;
                    case Enumerators.GameMechanicDescription.Reanimate:
                        unit.ApplyBuff(Enumerators.BuffType.REANIMATE);
                        actionEffectType = Enumerators.ActionEffectType.Reanimate;
                        break;
                    case Enumerators.GameMechanicDescription.Heavy:
                        unit.SetAsHeavyUnit();
                        actionEffectType = Enumerators.ActionEffectType.Heavy;
                        break;
                    case Enumerators.GameMechanicDescription.Feral:
                        unit.SetAsFeralUnit();
                        actionEffectType = Enumerators.ActionEffectType.Feral;
                        break;
                    case Enumerators.GameMechanicDescription.SwingX:
                        unit.AddBuffSwing();
                        actionEffectType = Enumerators.ActionEffectType.Swing;
                        break;
                }

                TargetEffects.Add(new PastActionsPopup.TargetEffectParam()
                {
                    ActionEffectType = actionEffectType,
                    Target = unit,
                });
            }

            if (TargetEffects.Count > 0)
            {
                ActionsReportController.PostGameActionReport(new PastActionsPopup.PastActionParam()
                {
                    ActionType = Enumerators.ActionType.CardAffectingCard,
                    Caller = AbilityUnitOwner,
                    TargetEffects = TargetEffects
                });
            }
        }
    }
}
