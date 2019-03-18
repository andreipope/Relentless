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

        public Enumerators.Faction Faction;

        public Enumerators.Stat StatType;

        public int Value { get; }

        public int Count { get; }

        public ModificateStatAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Faction = ability.Faction;
            StatType = ability.Stat;
            Value = ability.Value;
            Count = ability.Count;
        }

        public override void Activate()
        {
            base.Activate();

            VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/GreenHealVFX");

            if (AbilityTrigger != Enumerators.AbilityTrigger.ENTRY)
            {
                InvokeUseAbilityEvent();
            }
            else if(AbilityTrigger == Enumerators.AbilityTrigger.ENTRY &&
                    AbilityActivityType == Enumerators.AbilityActivity.PASSIVE)
            {
                if (AbilityData.SubTrigger == Enumerators.AbilitySubTrigger.AllAllyUnitsByFactionInPlay)
                {
                    IReadOnlyList<BoardUnitModel> units = PlayerCallerOfAbility.CardsOnBoard.FindAll(
                                    x => x.Card.Prototype.Faction == Faction && x != AbilityUnitOwner);
                    units = InternalTools.GetRandomElementsFromList(units, Count);

                    foreach (BoardUnitModel unit in units)
                    {
                        ModificateStats(unit);
                    }
                }
                else
                {
                    if (PlayerCallerOfAbility.CardsOnBoard.FindAll(x => x.Card.Prototype.Faction == Faction && x != AbilityUnitOwner).Count > 0)
                    {
                        ModificateStats(AbilityUnitOwner, GameplayManager.CurrentTurnPlayer == PlayerCallerOfAbility);
                    }
                }
            }
        }

        protected override void TurnStartedHandler()
        {
            base.TurnStartedHandler();

            if (AbilityTrigger != Enumerators.AbilityTrigger.TURN)
                return;

            ModificateStats(AbilityUnitOwner, GameplayManager.CurrentTurnPlayer.Equals(PlayerCallerOfAbility));
        }

        protected override void UnitDiedHandler()
        {
            base.UnitDiedHandler();

            if (AbilityTrigger != Enumerators.AbilityTrigger.DEATH)
                return;

            if (AbilityData.SubTrigger == Enumerators.AbilitySubTrigger.RandomUnit)
            {
                List<BoardUnitModel> targets = new List<BoardUnitModel>();

                foreach (Enumerators.Target targetType in AbilityTargetTypes)
                {
                    switch (targetType)
                    {
                        case Enumerators.Target.PLAYER_CARD:
                            targets.AddRange(PlayerCallerOfAbility.CardsOnBoard);
                            break;
                        case Enumerators.Target.OPPONENT_CARD:
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
                        if (Faction == 0 || Faction == boardUnit.Card.Prototype.Faction)
                        {
                            switch (StatType)
                            {
                                case Enumerators.Stat.DAMAGE:
                                    boardUnit.BuffedDamage += revert ? -Value : Value;
                                    boardUnit.CurrentDamage += revert ? -Value : Value;
                                    break;
                                case Enumerators.Stat.DEFENSE:
                                    boardUnit.BuffedDefense += revert ? -Value : Value;
                                    boardUnit.CurrentDefense += revert ? -Value : Value;
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException(nameof(StatType), StatType, null);
                            }

                            _canBeReverted = !revert;

                            CreateVfx(BattlegroundController.GetBoardUnitViewByModel<BoardUnitView>(boardUnit).Transform.position);

                            if (AbilityTrigger == Enumerators.AbilityTrigger.ENTRY)
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
