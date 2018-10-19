using System;
using System.Collections.Generic;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class ModificateStatAbility : AbilityBase
    {
        private bool _canBeReverted = false;

        public Enumerators.SetType SetType;

        public Enumerators.StatType StatType;

        public int Value = 1;

        public ModificateStatAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            SetType = ability.AbilitySetType;
            StatType = ability.AbilityStatType;
            Value = ability.Value;
        }

        public override void Activate()
        {
            base.Activate();

            VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/GreenHealVFX");

            if (AbilityCallType != Enumerators.AbilityCallType.ENTRY)
            {
                AbilitiesController.ThrowUseAbilityEvent(MainWorkingCard, new List<BoardObject>(), AbilityData.AbilityType, Protobuf.AffectObjectType.Character);
            }
            else if(AbilityCallType == Enumerators.AbilityCallType.ENTRY &&
                    AbilityActivityType == Enumerators.AbilityActivityType.PASSIVE)
            {
                if (AbilityData.AbilitySubTrigger == Enumerators.AbilitySubTrigger.AllAllyUhitsByFactionInPlay)
                {

                }
                else
                {
                    if (SetType != Enumerators.SetType.NONE)
                    {
                        if (PlayerCallerOfAbility.BoardCards.FindAll(x => x.Model.Card.LibraryCard.CardSetType == SetType).Count > 0)
                        {
                            ModificateStats(AbilityUnitOwner, GameplayManager.CurrentTurnPlayer.Equals(PlayerCallerOfAbility));
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
                        if (boardUnit.Card.LibraryCard.CardSetType == SetType || SetType == Enumerators.SetType.NONE)
                        {
                            switch (StatType)
                            {
                                case Enumerators.StatType.DAMAGE:
                                    if (revert)
                                    {
                                        boardUnit.BuffedDamage -= Value;
                                        boardUnit.CurrentDamage -= Value;
                                    }
                                    else
                                    {
                                        boardUnit.BuffedDamage += Value;
                                        boardUnit.CurrentDamage += Value;
                                    }
                                    break;
                                case Enumerators.StatType.HEALTH:
                                    if (revert)
                                    {
                                        boardUnit.BuffedHp -= Value;
                                        boardUnit.CurrentHp -= Value;
                                    }
                                    else
                                    {
                                        boardUnit.BuffedHp += Value;
                                        boardUnit.CurrentHp += Value;
                                    }
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException(nameof(StatType), StatType, null);
                            }

                            _canBeReverted = !revert;

                            CreateVfx(BattlegroundController.GetBoardUnitViewByModel(boardUnit).Transform.position);

                            if (AbilityCallType == Enumerators.AbilityCallType.ENTRY)
                            {
                                AbilitiesController.ThrowUseAbilityEvent(MainWorkingCard, new List<BoardObject>()
                                {
                                    boardUnit
                                }, AbilityData.AbilityType, Protobuf.AffectObjectType.Character);
                            }
                        }
                    }
                    break;
            }
        }
    }
}
