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
        private List<BoardUnitModel> _affectedUnits;

        public Enumerators.CardType UnitType;
        public Enumerators.Faction Faction;

        public int Cost { get; }

        public TakeUnitTypeToAllyUnitAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            UnitType = ability.TargetUnitType;
            Faction = ability.Faction;
            Cost = ability.Cost;

            _affectedUnits = new List<BoardUnitModel>();
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

        protected override void ChangeAuraStatusAction(bool status)
        {
            base.ChangeAuraStatusAction(status);

            if (AbilityTrigger != Enumerators.AbilityTrigger.AURA)
                return;

            if (status)
            {
                Action();
            }
            else
            {
                ResetAffectedUnits(_affectedUnits);
            }
        }

        protected override void BoardChangedHandler(int count)
        {
            base.BoardChangedHandler(count);

            if (AbilityUnitOwner.IsUnitActive && !AbilityUnitOwner.IsDead)
            {
                Action();
            }
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

            List<PastActionsPopup.TargetEffectParam> targetEffects = new List<PastActionsPopup.TargetEffectParam>();

            switch (AbilityData.SubTrigger)
            {
                case Enumerators.AbilitySubTrigger.RandomUnit:
                    {
                        List<BoardUnitModel> allies;

                        allies = PlayerCallerOfAbility.CardsOnBoard
                        .Where(unit => unit != AbilityUnitOwner && unit.InitialUnitType != UnitType && !unit.IsDead && unit.IsUnitActive)
                        .ToList();

                        if (allies.Count > 0)
                        {
                            int random = MTwister.IRandom(0, allies.Count - 1);

                            TakeTypeToUnit(allies[random]);

                            targetEffects.Add(new PastActionsPopup.TargetEffectParam()
                            {
                                ActionEffectType = effectType,
                                Target = allies[random]
                            });
                        }
                    }
                    break;
                case Enumerators.AbilitySubTrigger.OnlyThisUnitInPlay:
                    if (PlayerCallerOfAbility.CardsOnBoard.Where(
                            unit => unit != AbilityUnitOwner &&
                                !unit.IsDead &&
                                unit.CurrentDefense > 0)
                        .Count() == 0)
                    {
                        targetEffects.Add(new PastActionsPopup.TargetEffectParam()
                        {
                            ActionEffectType = effectType,
                            Target = AbilityUnitOwner
                        });

                        TakeTypeToUnit(AbilityUnitOwner);
                    }
                    break;
                case Enumerators.AbilitySubTrigger.AllOtherAllyUnitsInPlay:
                    {
                        List<BoardUnitModel> allies = PlayerCallerOfAbility.CardsOnBoard
                           .Where(unit => unit != AbilityUnitOwner &&
                                   (unit.Card.Prototype.Faction == Faction || Faction == Enumerators.Faction.Undefined) &&
                                   unit.InitialUnitType != UnitType && !unit.IsDead)
                           .ToList();

                        foreach (BoardUnitModel unit in allies)
                        {
                            if (TakeTypeToUnit(unit))
                            {
                                targetEffects.Add(new PastActionsPopup.TargetEffectParam()
                                {
                                    ActionEffectType = effectType,
                                    Target = unit
                                });
                            }
                        }
                    }
                    break;
                case Enumerators.AbilitySubTrigger.AllyUnitsByFactionThatCost:
                    {
                        List<BoardUnitModel> allies = PlayerCallerOfAbility.CardsOnBoard
                               .Where(unit => unit != AbilityUnitOwner && unit.Card.Prototype.Faction == Faction &&
                                      unit.Card.InstanceCard.Cost <= Cost && unit.InitialUnitType != UnitType && !unit.IsDead)
                               .ToList();

                        foreach (BoardUnitModel unit in allies)
                        {
                            if (TakeTypeToUnit(unit))
                            {
                                targetEffects.Add(new PastActionsPopup.TargetEffectParam()
                                {
                                    ActionEffectType = effectType,
                                    Target = unit
                                });
                            }
                        }
                    }
                    break;
                case Enumerators.AbilitySubTrigger.AllAllyUnitsInPlay:
                    {
                        List<BoardUnitModel> allies = PlayerCallerOfAbility.CardsOnBoard.Where(
                                       unit => unit != AbilityUnitOwner &&
                                           !unit.IsDead &&
                                           unit.CurrentDefense > 0 && unit.IsUnitActive).ToList();

                        if (AbilityTrigger == Enumerators.AbilityTrigger.AURA)
                        {
                            for (int i = 0; i < _affectedUnits.Count; i++)
                            {
                                if (!allies.Contains(_affectedUnits[i]))
                                {
                                    _affectedUnits.RemoveAt(i--);
                                }
                            }
                        }

                        foreach (BoardUnitModel unit in allies)
                        {
                            if (AbilityTrigger == Enumerators.AbilityTrigger.AURA &&
                                _affectedUnits.Contains(unit))
                                continue;

                            if (TakeTypeToUnit(unit))
                            {
                                targetEffects.Add(new PastActionsPopup.TargetEffectParam()
                                {
                                    ActionEffectType = effectType,
                                    Target = unit
                                });

                                if (AbilityTrigger == Enumerators.AbilityTrigger.AURA)
                                {
                                    _affectedUnits.Add(unit);
                                }
                            }
                        }
                    }
                    break;
                case Enumerators.AbilitySubTrigger.None:
                    if(TargetUnit != null)
                    {
                        TakeTypeToUnit(TargetUnit);
                        targetEffects.Add(new PastActionsPopup.TargetEffectParam()
                        {
                            ActionEffectType = effectType,
                            Target = TargetUnit
                        });
                    }
                    
                    break;
            }


            if (targetEffects.Count > 0)
            {
                Enumerators.ActionType actionType = Enumerators.ActionType.CardAffectingMultipleCards;

                if (targetEffects.Count == 1)
                {
                    actionType = Enumerators.ActionType.CardAffectingCard;
                }

                ActionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
                {
                    ActionType = actionType,
                    Caller = GetCaller(),
                    TargetEffects = targetEffects
                });
            }
        }

        protected override void InputEndedHandler()
        {
            base.InputEndedHandler();

            if (IsAbilityResolved)
            {
                Action();
            }
        }

        private bool TakeTypeToUnit(BoardUnitModel unit)
        {
            if (unit == null)
                return false;

            switch (UnitType)
            {
                case Enumerators.CardType.HEAVY:
                    unit.SetAsHeavyUnit();
                    return true;
                case Enumerators.CardType.FERAL:
                    unit.SetAsFeralUnit();
                    return true;
            }

            return false;
        }

        private void ResetAffectedUnits(List<BoardUnitModel> units)
        {
            foreach(BoardUnitModel unit in units)
            {
                switch(unit.Card.InstanceCard.CardType)
                {
                    case Enumerators.CardType.HEAVY:
                        unit.SetAsHeavyUnit();
                        break;
                    case Enumerators.CardType.WALKER:
                        unit.SetAsWalkerUnit();
                        break;
                    case Enumerators.CardType.FERAL:
                        unit.SetAsFeralUnit();
                        break;
                }
            }
            units.Clear();
        }
    }
}
