using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using System.Collections.Generic;

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

            if (AbilityCallType != Enumerators.AbilityCallType.ENTRY || AbilityActivityType != Enumerators.AbilityActivityType.PASSIVE)
                return;

            CheckSubTriggers();
        }

        protected override void InputEndedHandler()
        {
            base.InputEndedHandler();

            if (IsAbilityResolved)
            {
                GiveBuffsToUnit(TargetUnit);
            }
        }

        private void CheckSubTriggers()
        {
            if (AbilityData.AbilitySubTrigger == Enumerators.AbilitySubTrigger.OnlyThisUnitInPlay)
            {
                if (PlayerCallerOfAbility.BoardCards.FindAll(card => card.Model != AbilityUnitOwner).Count == 0)
                {
                    GiveBuffsToUnit(AbilityUnitOwner);
                }
            }
        }

        private void GiveBuffsToUnit(BoardUnitModel unit)
        {
            List<PastActionsPopup.TargetEffectParam> TargetEffects = new List<PastActionsPopup.TargetEffectParam>();
            Enumerators.ActionEffectType actionEffectType = Enumerators.ActionEffectType.None;
            foreach (Enumerators.GameMechanicDescriptionType type in AbilityData.TargetGameMechanicDescriptionTypes)
            {
                switch (type)
                {
                    case Enumerators.GameMechanicDescriptionType.Guard:
                        unit.AddBuffShield();
                        actionEffectType = Enumerators.ActionEffectType.Guard;
                        break;
                    case Enumerators.GameMechanicDescriptionType.Destroy:
                        unit.AddBuff(Enumerators.BuffType.DESTROY);
                        actionEffectType = Enumerators.ActionEffectType.DeathMark;
                        break;
                    case Enumerators.GameMechanicDescriptionType.Reanimate:
                        unit.AddBuff(Enumerators.BuffType.REANIMATE);
                        actionEffectType = Enumerators.ActionEffectType.Reanimate;
                        break;
                    case Enumerators.GameMechanicDescriptionType.Heavy:
                        unit.SetAsHeavyUnit();
                        actionEffectType = Enumerators.ActionEffectType.Heavy;
                        break;
                    case Enumerators.GameMechanicDescriptionType.Feral:
                        unit.SetAsFeralUnit();
                        actionEffectType = Enumerators.ActionEffectType.Feral;
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
                ActionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
                {
                    ActionType = Enumerators.ActionType.CardAffectingCard,
                    Caller = GetCaller(),
                    TargetEffects = TargetEffects
                });
            }
        }
    }
}
