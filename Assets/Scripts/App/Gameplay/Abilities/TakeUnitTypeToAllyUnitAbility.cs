using System.Collections.Generic;
using System.Linq;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;

namespace Loom.ZombieBattleground
{
    public class TakeUnitTypeToAllyUnitAbility : AbilityBase
    {
        private List<CardModel> _affectedUnits;

        public Enumerators.CardType UnitType;
        public Enumerators.Faction Faction;

        public int Cost { get; }

        public TakeUnitTypeToAllyUnitAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            UnitType = ability.TargetUnitType;
            Faction = ability.Faction;
            Cost = ability.Cost;

            _affectedUnits = new List<CardModel>();
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
                bool otherUnitWithAuraExist = false;
                foreach(CardModel card in AbilityUnitOwner.OwnerPlayer.PlayerCardsController.CardsOnBoard)
                {
                    if
                    (
                        card != AbilityUnitOwner &&
                        card.Prototype.CardKey.MouldId == AbilityUnitOwner.Prototype.CardKey.MouldId
                    )
                    {
                        otherUnitWithAuraExist = true;
                    }
                }
                
                if(!otherUnitWithAuraExist)
                {
                    ResetAffectedUnits(_affectedUnits);
                }
            }
        }

        protected override void BoardChangedHandler(int count)
        {
            base.BoardChangedHandler(count);

            if (AbilityUnitOwner.IsUnitActive && !AbilityUnitOwner.IsDead && AbilityTrigger == Enumerators.AbilityTrigger.AURA && LastAuraState)
            {                
                RemoveAffectedUnitsNotCurrentlyOwn();
                Action();
            }
        }

        protected override void PlayerOwnerHasChanged(Player oldPlayer, Player newPlayer)
        {
            ResetAffectedUnits(_affectedUnits);
            BoardChangedHandler(newPlayer.CardsOnBoard.Count);
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
                        List<CardModel> allies;

                        allies = AbilityUnitOwner.OwnerPlayer.CardsOnBoard
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
                    if (GetAliveUnits(AbilityUnitOwner.OwnerPlayer.CardsOnBoard).Count() == 1)
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
                        List<CardModel> allies = AbilityUnitOwner.OwnerPlayer.CardsOnBoard
                           .Where(unit => unit != AbilityUnitOwner &&
                                   (unit.Card.Prototype.Faction == Faction || Faction == Enumerators.Faction.Undefined) &&
                                   unit.InitialUnitType != UnitType && !unit.IsDead)
                           .ToList();

                        foreach (CardModel unit in allies)
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
                        List<CardModel> allies = AbilityUnitOwner.OwnerPlayer.CardsOnBoard
                               .Where(unit => unit != AbilityUnitOwner && unit.Card.Prototype.Faction == Faction &&
                                      unit.CurrentCost <= Cost && unit.InitialUnitType != UnitType && !unit.IsDead)
                               .ToList();

                        foreach (CardModel unit in allies)
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
                        List<CardModel> allies = AbilityUnitOwner.OwnerPlayer.CardsOnBoard.Where(
                                       unit => !unit.IsDead &&
                                           unit.CurrentDefense > 0 && unit.IsUnitActive).ToList();

                        foreach (CardModel unit in allies)
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
            }


            if (targetEffects.Count > 0)
            {
                Enumerators.ActionType actionType = Enumerators.ActionType.CardAffectingMultipleCards;

                if (targetEffects.Count == 1)
                {
                    actionType = Enumerators.ActionType.CardAffectingCard;
                }

                ActionsReportController.PostGameActionReport(new PastActionsPopup.PastActionParam()
                {
                    ActionType = actionType,
                    Caller = AbilityUnitOwner,
                    TargetEffects = targetEffects
                });
            }
        }

        private bool TakeTypeToUnit(CardModel unit)
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

        private void RemoveAffectedUnitsNotCurrentlyOwn()
        {
            for(int i = 0; i < _affectedUnits.Count; ++i)
            {
                CardModel unit = _affectedUnits[i];
                if(unit.OwnerPlayer != AbilityUnitOwner.OwnerPlayer)
                {
                    ResetAffectedUnit(unit);
                    _affectedUnits.Remove(unit);
                    --i;
                }
            }
        }

        private void ResetAffectedUnits(List<CardModel> units)
        {
            foreach(CardModel unit in units)
            {
                ResetAffectedUnit(unit);
            }
            units.Clear();
        }
        
        private void ResetAffectedUnit(CardModel unit)
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
    }
}
