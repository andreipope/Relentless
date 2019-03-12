using System;
using System.Collections.Generic;
using System.Linq;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using Random = UnityEngine.Random;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class TakeUnitTypeToAllyUnitAbility : AbilityBase
    {
        public Enumerators.CardType UnitType;
        public Enumerators.Faction SetType;

        public int Cost { get; }

        public TakeUnitTypeToAllyUnitAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            UnitType = ability.TargetUnitType;
            SetType = ability.AbilitySetType;
            Cost = ability.Cost;
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

        public override void Action(object info = null)
        {
            base.Action(info);

            Enumerators.ActionEffectType effectType = Enumerators.ActionEffectType.None;

            if (UnitType == Enumerators.CardType.FERAL)
            {
                effectType = Enumerators.ActionEffectType.Feral;
            }
            else if (UnitType == Enumerators.CardType.HEAVY)
            {
                effectType = Enumerators.ActionEffectType.Heavy;
            }

            List<PastActionsPopup.TargetEffectParam> TargetEffects = new List<PastActionsPopup.TargetEffectParam>();

            switch (AbilityData.AbilitySubTrigger)
            {
                case Enumerators.AbilitySubTrigger.RandomUnit:
                    {
                        List<BoardUnitModel> allies;

                        allies = PlayerCallerOfAbility.BoardCards.Select(x => x.Model)
                        .Where(unit => unit != AbilityUnitOwner && unit.InitialUnitType != UnitType && !unit.IsDead)
                        .ToList();

                        if (allies.Count > 0)
                        {
                            int random = MTwister.IRandom(0, allies.Count);

                            TakeTypeToUnit(allies[random]);

                            TargetEffects.Add(new PastActionsPopup.TargetEffectParam()
                            {
                                ActionEffectType = effectType,
                                Target = allies[random]
                            });
                        }
                    }
                    break;
                case Enumerators.AbilitySubTrigger.OnlyThisUnitInPlay:
                    if (PlayerCallerOfAbility.BoardCards.Where(unit => unit.Model != AbilityUnitOwner &&
                                                               !unit.Model.IsDead &&
                                                               unit.Model.CurrentHp > 0).Count() == 0)
                    {
                        TargetEffects.Add(new PastActionsPopup.TargetEffectParam()
                        {
                            ActionEffectType = effectType,
                            Target = AbilityUnitOwner
                        });

                        TakeTypeToUnit(AbilityUnitOwner);
                    }
                    break;
                case Enumerators.AbilitySubTrigger.AllOtherAllyUnitsInPlay:
                    {
                        List<BoardUnitModel> allies = PlayerCallerOfAbility.BoardCards.Select(x => x.Model)
                           .Where(unit => unit != AbilityUnitOwner &&
                                   unit.Card.Prototype.Faction == SetType &&
                                   unit.InitialUnitType != UnitType && !unit.IsDead)
                           .ToList();

                        foreach(BoardUnitModel unit in allies)
                        {
                            TakeTypeToUnit(unit);

                            TargetEffects.Add(new PastActionsPopup.TargetEffectParam()
                            {
                                ActionEffectType = effectType,
                                Target = unit
                            });
                        }
                    }
                    break;
                case Enumerators.AbilitySubTrigger.AllyUnitsByFactionThatCost:
                    {
                        List<BoardUnitModel> allies = PlayerCallerOfAbility.BoardCards.Select(x => x.Model)
                               .Where(unit => unit != AbilityUnitOwner && unit.Card.Prototype.Faction == SetType &&
                                      unit.Card.InstanceCard.Cost <= Cost && unit.InitialUnitType != UnitType && !unit.IsDead)
                               .ToList();

                        foreach (BoardUnitModel unit in allies)
                        {
                            TakeTypeToUnit(unit);
                        }
                    }
                    break;
            }


            if (TargetEffects.Count > 0)
            {
                Enumerators.ActionType actionType = Enumerators.ActionType.CardAffectingMultipleCards;

                if (TargetEffects.Count == 1)
                {
                    actionType = Enumerators.ActionType.CardAffectingCard;
                }

                ActionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
                {
                    ActionType = actionType,
                    Caller = GetCaller(),
                    TargetEffects = TargetEffects
                });
            }
        }

        private void TakeTypeToUnit(BoardUnitModel unit)
        {
            if (unit == null)
                return;

            switch (UnitType)
            {
                case Enumerators.CardType.HEAVY:
                    unit.SetAsHeavyUnit();
                    break;
                case Enumerators.CardType.FERAL:
                    unit.SetAsFeralUnit();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(UnitType), UnitType, null);
            }
        }
    }
}
