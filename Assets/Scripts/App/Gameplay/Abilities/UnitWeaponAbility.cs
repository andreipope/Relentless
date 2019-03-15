using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class UnitWeaponAbility : AbilityBase
    {
        public int Value;

        public int Defense;

        public int Damage;

        public event Action TurnEndedEvent;

        private Enumerators.GameMechanicDescription _gameMechanicType;

        public UnitWeaponAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Value = ability.Value;
            Damage = ability.Damage;
            Defense = ability.Defense;
        }

        public override void Activate()
        {
            base.Activate();
        }

        public override void Action(object info = null)
        {
            base.Action(info);

            TargetUnit.CurrentDamage += Value;
            TargetUnit.BuffedDamage += Value;

            TargetUnit.CurrentDefense += Defense;
            TargetUnit.BuffedDefense += Defense;

            _gameMechanicType = Enumerators.GameMechanicDescription.Chainsaw;

            switch (BoardUnitModel.Card.Prototype.MouldId)
            {
                case 41:
                    _gameMechanicType = Enumerators.GameMechanicDescription.SuperSerum;
                    break;
                case 18:
                    _gameMechanicType = Enumerators.GameMechanicDescription.Chainsaw;
                    break;
                default:
                    break;
            }

            TargetUnit.AddGameMechanicDescriptionOnUnit(_gameMechanicType);
        }

        protected override void InputEndedHandler()
        {
            base.InputEndedHandler();

            if (IsAbilityResolved)
            {
                if (TargetUnit != null)
                {
                    InvokeActionTriggered();

                    TargetUnit.UnitDied += TargetUnitDiedHandler;

                    InvokeUseAbilityEvent(
                        new List<ParametrizedAbilityBoardObject>
                        {
                            new ParametrizedAbilityBoardObject(TargetUnit)
                        }
                    );
                }
            }
        }

        protected override void VFXAnimationEndedHandler()
        {
            base.VFXAnimationEndedHandler();

            Action();
        }

        protected override void TurnEndedHandler()
        {
            base.TurnEndedHandler();

            if (!GameplayManager.CurrentTurnPlayer.Equals(PlayerCallerOfAbility))
                return;

            TurnEndedEvent?.Invoke();

            ActionEnd();
        }

        private void ActionEnd()
        {
            if (TargetUnit != null)
            {
                BattleController.AttackUnitByAbility(TargetUnit, AbilityData, TargetUnit, Damage);

                CreateVfx(BattlegroundController.GetBoardUnitViewByModel<BoardUnitView>(TargetUnit).Transform.position, true, 5f);

                TargetUnit.RemoveGameMechanicDescriptionFromUnit(_gameMechanicType);
            }
        }

        private void TargetUnitDiedHandler()
        {
            if (TargetUnit != null)
            {
                TargetUnit.UnitDied -= TargetUnitDiedHandler;
            }

            Deactivate();
        }
    }
}
