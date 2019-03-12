using System;
using System.Collections.Generic;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.Helpers;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class ModificateStatAbility : AbilityBase
    {
        private bool _canBeReverted = false;

        public Enumerators.SetType SetType;

        public Enumerators.StatType StatType;

        public int Value { get; }

        public int Count { get; }

        public ModificateStatAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            SetType = ability.AbilitySetType;
            StatType = ability.AbilityStatType;
            Value = ability.Value;
            Count = ability.Count;
        }

        public override void Activate()
        {
            base.Activate();

            VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/GreenHealVFX");

            if (AbilityCallType != Enumerators.AbilityCallType.ENTRY)
            {
                InvokeUseAbilityEvent();
            }
            else if(AbilityCallType == Enumerators.AbilityCallType.ENTRY &&
                    AbilityActivityType == Enumerators.AbilityActivityType.PASSIVE)
            {
                if (AbilityData.AbilitySubTrigger == Enumerators.AbilitySubTrigger.AllAllyUnitsByFactionInPlay)
                {
                    IReadOnlyList<BoardUnitModel> units = PlayerCallerOfAbility.CardsOnBoard.FindAll(
                                    x => x.Card.Prototype.CardSetType == SetType && x != AbilityUnitOwner);
                    units = InternalTools.GetRandomElementsFromList(units, Count);

                    foreach (BoardUnitModel unit in units)
                    {
                        ModificateStats(unit);
                    }
                }
                else
                {
                    if (SetType != Enumerators.SetType.NONE)
                    {
                        if (PlayerCallerOfAbility.CardsOnBoard.FindAll(x => x.Card.Prototype.CardSetType == SetType && x != AbilityUnitOwner).Count > 0)
                        {
                            ModificateStats(AbilityUnitOwner, !GameplayManager.CurrentTurnPlayer.Equals(PlayerCallerOfAbility));
                        }
                    }
                }
            }
        }

        protected override void TurnStartedHandler()
        {
            base.TurnStartedHandler();

            if (AbilityCallType != Enumerators.AbilityCallType.TURN)
                return;

            ModificateStats(AbilityUnitOwner, GameplayManager.CurrentTurnPlayer.Equals(PlayerCallerOfAbility));
        }

        protected override void UnitDiedHandler()
        {
            base.UnitDiedHandler();

            if (AbilityCallType != Enumerators.AbilityCallType.DEATH)
                return;

            if (AbilityData.AbilitySubTrigger == Enumerators.AbilitySubTrigger.RandomUnit)
            {
                List<BoardUnitModel> targets = new List<BoardUnitModel>();

                foreach (Enumerators.AbilityTargetType targetType in AbilityTargetTypes)
                {
                    switch (targetType)
                    {
                        case Enumerators.AbilityTargetType.PLAYER_CARD:
                            targets.AddRange(PlayerCallerOfAbility.CardsOnBoard);
                            break;
                        case Enumerators.AbilityTargetType.OPPONENT_CARD:
                            targets.AddRange(GetOpponentOverlord().CardsOnBoard);
                            break;
                    }
                }

                targets = targets.FindAll(x => x!= AbilityUnitOwner);

                List<BoardUnitModel> finalTargets = new List<BoardUnitModel>();
                int count = Mathf.Max(1, Count);
                while (count > 0 && targets.Count > 0)
                {   
                    int chosenIndex = MTwister.IRandom(0, targets.Count-1);
                    finalTargets.Add(targets[chosenIndex]);
                    targets.RemoveAt(chosenIndex);
                    count--;
                }

                foreach (BoardUnitModel target in finalTargets)
                {
                    ModificateStats(target, false);
                }
            }
        }

        public override void Action(object info = null)
        {
            base.Action(info);

            ModificateStats(TargetUnit);
        }

        protected override void InputEndedHandler()
        {
            base.InputEndedHandler();

            if (IsAbilityResolved)
            {
                Action();
            }
        }

        private void ModificateStats(BoardObject boardObject, bool revert = false)
        {
            if (revert && !_canBeReverted)
                return;

            switch (boardObject)
            {
                case BoardUnitModel boardUnit:
                    {
                        if (boardUnit.Card.Prototype.CardSetType == SetType || SetType == Enumerators.SetType.NONE)
                        {
                            switch (StatType)
                            {
                                case Enumerators.StatType.DAMAGE:
                                    boardUnit.BuffedDamage += revert ? -Value : Value;
                                    boardUnit.CurrentDamage += revert ? -Value : Value;
                                    break;
                                case Enumerators.StatType.HEALTH:
                                    boardUnit.BuffedHp += revert ? -Value : Value;
                                    boardUnit.CurrentHp += revert ? -Value : Value;
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException(nameof(StatType), StatType, null);
                            }

                            _canBeReverted = !revert;

                            CreateVfx(BattlegroundController.GetBoardUnitViewByModel<BoardUnitView>(boardUnit).Transform.position);

                            if (AbilityCallType == Enumerators.AbilityCallType.ENTRY)
                            {
                                InvokeUseAbilityEvent(
                                    new List<ParametrizedAbilityBoardObject>
                                    {
                                        new ParametrizedAbilityBoardObject(boardUnit)
                                    }
                                );
                            }
                        }
                    }
                    break;
            }
        }
    }
}
