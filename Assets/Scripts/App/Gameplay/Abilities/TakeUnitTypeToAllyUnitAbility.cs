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

            switch (AbilityData.AbilitySubTrigger)
            {
                case Enumerators.AbilitySubTrigger.RandomUnit:
                    {
                        List<BoardUnitModel> allies;

                        if (PredefinedTargets != null)
                        {
                            allies = PredefinedTargets.Cast<BoardUnitModel>().ToList();

                            if (allies.Count > 0)
                            {
                                TakeTypeToUnit(allies[0]);

                                AbilitiesController.ThrowUseAbilityEvent(MainWorkingCard, new List<BoardObject>()
                                {
                                   allies[0]
                                }, AbilityData.AbilityType, Protobuf.AffectObjectType.Character);
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

                                AbilitiesController.ThrowUseAbilityEvent(MainWorkingCard, new List<BoardObject>()
                                {
                                   allies[random]
                                }, AbilityData.AbilityType, Protobuf.AffectObjectType.Character);
                            }
                        }
                    }
                    break;
                case Enumerators.AbilitySubTrigger.OnlyThisUnitInPlay:
                    if (PlayerCallerOfAbility.BoardCards.Where(unit => unit.Model != AbilityUnitOwner).Count() == 0)
                    {
                        TakeTypeToUnit(AbilityUnitOwner);
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
                        }

                        AbilitiesController.ThrowUseAbilityEvent(MainWorkingCard, allies.Cast<BoardObject>().ToList(),
                            AbilityData.AbilityType, Protobuf.AffectObjectType.Character);
                    }
                    break;
                case Enumerators.AbilitySubTrigger.AllyUnitsByFactionThatCost:
                    {
                        List<BoardUnitModel> allies = PlayerCallerOfAbility.BoardCards.Select(x => x.Model)
                               .Where(unit => unit != AbilityUnitOwner && unit.Card.LibraryCard.CardSetType == SetType &&
                                      unit.Card.RealCost <= Cost && unit.InitialUnitType != UnitType)
                               .ToList();

                        foreach (BoardUnitModel unit in allies)
                        {
                            TakeTypeToUnit(unit);
                        }

                        AbilitiesController.ThrowUseAbilityEvent(MainWorkingCard, allies.Cast<BoardObject>().ToList(),
                            AbilityData.AbilityType, Protobuf.AffectObjectType.Character);
                    }
                    break;
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
