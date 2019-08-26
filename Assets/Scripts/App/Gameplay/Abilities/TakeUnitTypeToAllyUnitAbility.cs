using System.Collections.Generic;
using System.Linq;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;

namespace Loom.ZombieBattleground
{
    public class TakeUnitTypeToAllyUnitAbility : AbilityBase
    {
        private List<CardModel> _affectedUnits;

        private static Dictionary<CardModel, AuraState> _affectedAura = new Dictionary<CardModel, AuraState>();        

        public class AuraState
        {
            public Player Player;
            public MouldId TypeById;
        }

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

            UpdateAura(status);
        }

        protected override void BoardChangedHandler(int count)
        {
            base.BoardChangedHandler(count);

            if (AbilityUnitOwner.IsUnitActive && !AbilityUnitOwner.IsDead && AbilityTrigger == Enumerators.AbilityTrigger.AURA && LastAuraState)
            {
                RemoveAffectedUnitsIfNoAura();
                UpdateAura(true);
            }
        }

        protected override void PlayerOwnerHasChanged(Player oldPlayer, Player newPlayer)
        {
            RemoveAuraFromPlayer(oldPlayer);            
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
                        //Aura status has already updated from ChangeAuraStatusAction method
                        if (AbilityTrigger != Enumerators.AbilityTrigger.AURA)
                        {            
                            List<CardModel> allies = AbilityUnitOwner.OwnerPlayer.CardsOnBoard.Where
                            (
                                unit => !unit.IsDead &&
                                unit.CurrentDefense > 0 &&
                                unit.IsUnitActive
                            ).ToList();
            
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
        
        private void UpdateAura(bool status)
        {
            if (AbilityTrigger != Enumerators.AbilityTrigger.AURA)
                return;                
            
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
            
            if(status)
            {
                Player affectedPlayer = AbilityUnitOwner.OwnerPlayer;
                
                if (!_affectedAura.ContainsKey(AbilityUnitOwner))
                {
                    _affectedAura.Add
                    (
                        AbilityUnitOwner, 
                        new AuraState()
                        {
                            Player = affectedPlayer,
                            TypeById = AbilityUnitOwner.Prototype.CardKey.MouldId
                        }
                    );
                }
                
                List<CardModel> unitsToAffect = affectedPlayer.CardsOnBoard.Where
                (
                    unit => !unit.IsDead &&
                    unit.CurrentDefense > 0 &&
                    unit.IsUnitActive
                ).ToList();

                foreach (CardModel unit in unitsToAffect)
                {
                    if (_affectedUnits.Contains(unit))
                        continue;
                        
                    if (TakeTypeToUnit(unit))
                    {
                        targetEffects.Add(new PastActionsPopup.TargetEffectParam()
                        {
                            ActionEffectType = effectType,
                            Target = unit
                        });
                    }
                    
                    _affectedUnits.Add(unit);
                }
            }
            else
            {
                if (!_affectedAura.ContainsKey(AbilityUnitOwner))
                    return;

                Player affectedPlayer = _affectedAura[AbilityUnitOwner].Player;
                
                 _affectedAura.Remove(AbilityUnitOwner);

                bool otherAuraExist = _affectedAura.Any
                (
                    kvp => 
                        kvp.Value.Player.IsLocalPlayer == affectedPlayer.IsLocalPlayer &&
                        kvp.Value.TypeById == AbilityUnitOwner.Prototype.CardKey.MouldId
                );

                if(!otherAuraExist)
                {
                    List<CardModel> unitsToAffect = affectedPlayer.CardsOnBoard.Where
                    (
                        unit => !unit.IsDead &&
                        unit.CurrentDefense > 0 &&
                        unit.IsUnitActive
                    ).ToList();
                    foreach (CardModel unit in unitsToAffect)
                    {
                        ResetAffectedUnit(unit);
                        if(_affectedUnits.Contains(unit))
                        {
                            _affectedUnits.Remove(unit);
                        }
                    }
                }
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
        
        private void RemoveAuraFromPlayer(Player affectedPlayer)
        {
            if (AbilityTrigger != Enumerators.AbilityTrigger.AURA)
                return;
                
            if (_affectedAura.ContainsKey(AbilityUnitOwner))
            {
                if(_affectedAura[AbilityUnitOwner].Player == affectedPlayer)
                {
                    _affectedAura.Remove(AbilityUnitOwner);
                }
            }

            bool otherAuraExist = _affectedAura.Any
            (
                kvp => 
                    kvp.Value.Player.IsLocalPlayer == affectedPlayer.IsLocalPlayer &&
                    kvp.Value.TypeById == AbilityUnitOwner.Prototype.CardKey.MouldId
            );

            if(!otherAuraExist)
            {
                List<CardModel> unitsToAffect = affectedPlayer.CardsOnBoard.Where
                (
                    unit => !unit.IsDead &&
                    unit.CurrentDefense > 0 &&
                    unit.IsUnitActive
                ).ToList();
                foreach (CardModel unit in unitsToAffect)
                {
                    ResetAffectedUnit(unit);
                    if(_affectedUnits.Contains(unit))
                    {
                        _affectedUnits.Remove(unit);
                    }
                }
            }
        }
        
        private bool CheckAllyUnitWithAuraExist(Player player)
        {
            bool otherUnitWithAuraExist = false;
            foreach(CardModel card in player.PlayerCardsController.CardsOnBoard)
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
            return otherUnitWithAuraExist;
        }

        private bool TakeTypeToUnit(CardModel unit)
        {
            if (unit == null)
                return false;

            switch (UnitType)
            {
                case Enumerators.CardType.HEAVY:
                    unit.AddHeavyTypeToUnit();
                    return true;
                case Enumerators.CardType.FERAL:
                    unit.AddFeralTypeToUnit();
                    return true;
            }

            return false;
        }
        
        private void RemoveAffectedUnitsIfNoAura()
        {
            for(int i = 0; i < _affectedUnits.Count; ++i)
            {
                CardModel unit = _affectedUnits[i];

                bool hasAura = _affectedAura.Any
                (
                    kvp => kvp.Value.Player.IsLocalPlayer == unit.OwnerPlayer.IsLocalPlayer && 
                    kvp.Value.TypeById == AbilityUnitOwner.Prototype.CardKey.MouldId
                );
                if(!hasAura)
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
            switch (UnitType)
            {
                case Enumerators.CardType.HEAVY:
                    unit.RemoveHeavyTypeFromUnit();
                    break;
                case Enumerators.CardType.FERAL:
                    unit.RemoveFeralTypeFromUnit();
                    break;
            }
        }
    }
}
