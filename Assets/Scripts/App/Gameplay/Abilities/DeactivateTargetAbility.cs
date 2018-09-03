using System;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class DeactivateTargetAbility : AbilityBase
    {
        public int Value = 1;

        private int _turnsLength;

        public DeactivateTargetAbility(Enumerators.CardKind cardKind, AbilityData ability, int value = 1)
            : base(cardKind, ability)
        {
            Value = value;
        }

        public override void Activate()
        {
            base.Activate();

            VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/FrozenVFX");
            _turnsLength = Value;
        }

        public override void Update()
        {
        }

        public override void Dispose()
        {
        }

        public override void Action(object info = null)
        {
            base.Action(info);

            switch (AffectObjectType)
            {
                case Enumerators.AffectObjectType.CHARACTER:

                    TargetUnit.Card.IsPlayable = false;
                    TargetUnit.SetHighlightingEnabled(false);

                    CreateVfx(TargetUnit.Transform.position);
                    break;
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

        protected override void TurnEndedHandler()
        {
            base.TurnEndedHandler();

            _turnsLength--;

            if (_turnsLength > 0)
                return;

            switch (CardKind)
            {
                case Enumerators.CardKind.CREATURE:
                    UnitDiedHandler();
                    break;
                case Enumerators.CardKind.SPELL:
                    UsedHandler();

                    AbilitiesController.DeactivateAbility(ActivityId);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected override void TurnStartedHandler()
        {
            base.TurnStartedHandler();

            if (_turnsLength > 0)
            {
                if (TargetUnit != null)
                {
                    TargetUnit.Card.IsPlayable = false;
                    TargetUnit.SetHighlightingEnabled(false);
                }
                else
                {
                    if (CardKind == Enumerators.CardKind.CREATURE)
                    {
                        UnitDiedHandler();
                    }
                    else if (CardKind == Enumerators.CardKind.SPELL)
                    {
                        UsedHandler();
                        AbilitiesController.DeactivateAbility(ActivityId);
                    }
                }
            }
        }
    }
}
