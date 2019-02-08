using System;
using System.Collections.Generic;
using System.Linq;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using Random = UnityEngine.Random;

namespace Loom.ZombieBattleground
{
    public class TakeUnitTypeToAllyUnitAbility : AbilityBase
    {
        public Enumerators.CardType UnitType;
        public Enumerators.SetType SetType;

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

            if (AbilityCallType != Enumerators.AbilityCallType.ENTRY)
                return;

            Action();
        }

        protected override void UnitDiedHandler()
        {
            base.UnitDiedHandler();

            if (AbilityCallType != Enumerators.AbilityCallType.DEATH)
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

                        if (PredefinedTargets != null)
                        {
                            allies = PredefinedTargets.Select(x => x.BoardObject).Cast<BoardUnitModel>().ToList();

                            if (allies.Count > 0)
                            {
                                TakeTypeToUnit(allies[0]);

                                TargetEffects.Add(new PastActionsPopup.TargetEffectParam()
                                {
                                    ActionEffectType = effectType,
                                    Target = allies[0]
                                });


                                ThrowUseAbilityEvent(
                                    new List<ParametrizedAbilityBoardObject>
                                    {
                                        new ParametrizedAbilityBoardObject(allies[0])
                                    }
                                );
                            }
                        }
                        else
                        {
                            allies = PlayerCallerOfAbility.BoardCards.Select(x => x.Model)
                           .Where(unit => unit != AbilityUnitOwner && unit.InitialUnitType != UnitType)
                           .ToList();

                            if (allies.Count > 0)
                            {
                                int random = Random.Range(0, allies.Count);
                                TakeTypeToUnit(allies[random]);

                                TargetEffects.Add(new PastActionsPopup.TargetEffectParam()
                                {
                                    ActionEffectType = effectType,
                                    Target = allies[random]
                                });

                                ThrowUseAbilityEvent(
                                    new List<ParametrizedAbilityBoardObject>
                                    {
                                        new ParametrizedAbilityBoardObject(allies[random])
                                    }
                                );
                            }
                        }
                    }
                    break;
                case Enumerators.AbilitySubTrigger.OnlyThisUnitInPlay:
                    if (PlayerCallerOfAbility.BoardCards.Where(unit => unit.Model != AbilityUnitOwner).Count() == 0)
                    {
                        TargetEffects.Add(new PastActionsPopup.TargetEffectParam()
                        {
                            ActionEffectType = effectType,
                            Target = AbilityUnitOwner
                        });

                        TakeTypeToUnit(AbilityUnitOwner);
                        ThrowUseAbilityEvent();
                    }
                    break;
                case Enumerators.AbilitySubTrigger.AllOtherAllyUnitsInPlay:
                    {
                        List<BoardUnitModel> allies = PlayerCallerOfAbility.BoardCards.Select(x => x.Model)
                           .Where(unit => unit != AbilityUnitOwner &&
                                   unit.Card.LibraryCard.CardSetType == SetType &&
                                   unit.InitialUnitType != UnitType)
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

                        ThrowUseAbilityEvent(
                            allies
                                .Select(model => new ParametrizedAbilityBoardObject(model))
                                .ToList()
                        );
                    }
                    break;
                case Enumerators.AbilitySubTrigger.AllyUnitsByFactionThatCost:
                    {
                        List<BoardUnitModel> allies = PlayerCallerOfAbility.BoardCards.Select(x => x.Model)
                               .Where(unit => unit != AbilityUnitOwner && unit.Card.LibraryCard.CardSetType == SetType &&
                                      unit.Card.InstanceCard.Cost <= Cost && unit.InitialUnitType != UnitType)
                               .ToList();

                        foreach (BoardUnitModel unit in allies)
                        {
                            TakeTypeToUnit(unit);
                        }

                        ThrowUseAbilityEvent(
                            allies
                                .Select(model => new ParametrizedAbilityBoardObject(model))
                                .ToList()
                        );
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
