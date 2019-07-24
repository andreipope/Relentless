using System.Collections.Generic;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class DistractAndChangeStatAbility : AbilityBase
    {
        private int Damage { get; }

        private int Defense { get; }

        public DistractAndChangeStatAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Damage = ability.Damage;
            Defense = ability.Defense;
        }

        public override void Activate()
        {
            base.Activate();
        }

        protected override void InputEndedHandler()
        {
            base.InputEndedHandler();

            if (IsAbilityResolved)
            {
                InvokeUseAbilityEvent(new List<ParametrizedAbilityBoardObject>()
                {
                    new ParametrizedAbilityBoardObject(TargetUnit)
                });

                Debug.LogWarning("Invoke action triggered");
                InvokeActionTriggered(TargetUnit);
            }
        }

        protected override void VFXAnimationEndedHandler()
        {
            base.VFXAnimationEndedHandler();
            DistractAndChangeStat(new List<CardModel>() { TargetUnit }, Defense, Damage);
        }


        private void DistractAndChangeStat(List<CardModel> units, int defense, int attack)
        {
            List<PastActionsPopup.TargetEffectParam> TargetEffects = new List<PastActionsPopup.TargetEffectParam>();

            foreach (CardModel boardUnit in units)
            {
                BattlegroundController.DistractUnit(boardUnit);

                if (defense != 0)
                {
                    boardUnit.BuffedDefense += defense;
                    boardUnit.AddToCurrentDefenseHistory(defense, Enumerators.ReasonForValueChange.AbilityBuff);

                    TargetEffects.Add(new PastActionsPopup.TargetEffectParam()
                    {
                        ActionEffectType = defense > 0 ? Enumerators.ActionEffectType.ShieldBuff : Enumerators.ActionEffectType.ShieldDebuff,
                        Target = boardUnit,
                        HasValue = true,
                        Value = defense
                    });
                }

                if (attack != 0)
                {
                    boardUnit.BuffedDamage += attack;
                    boardUnit.AddToCurrentDamageHistory(attack, Enumerators.ReasonForValueChange.AbilityBuff);

                    TargetEffects.Add(new PastActionsPopup.TargetEffectParam()
                    {
                        ActionEffectType = attack > 0 ? Enumerators.ActionEffectType.AttackBuff : Enumerators.ActionEffectType.AttackDebuff,
                        Target = boardUnit,
                        HasValue = true,
                        Value = attack
                    });
                }

                TargetEffects.Add(new PastActionsPopup.TargetEffectParam()
                {
                    ActionEffectType = Enumerators.ActionEffectType.Distract,
                    Target = boardUnit,
                });
            }

            if (TargetEffects.Count > 0)
            {
                ActionsReportController.PostGameActionReport(new PastActionsPopup.PastActionParam()
                {
                    ActionType = Enumerators.ActionType.CardAffectingMultipleCards,
                    Caller = AbilityUnitOwner,
                    TargetEffects = TargetEffects
                });
            }
        }
    }
}
