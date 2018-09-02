using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;
using UnityEngine;

namespace LoomNetwork.CZB
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

        protected override void OnInputEndEventHandler()
        {
            base.OnInputEndEventHandler();

            if (IsAbilityResolved)
            {
                Action();
            }
        }

        protected override void OnEndTurnEventHandler()
        {
            base.OnEndTurnEventHandler();

            _turnsLength--;

            if (_turnsLength <= 0)
            {
                if (CardKind == Enumerators.CardKind.CREATURE)
                {
                    // targetCreature.Card.DisconnectAbility((uint)abilityType);
                    UnitOnDieEventHandler();
                }
                else if (CardKind == Enumerators.CardKind.SPELL)
                {
                    SpellOnUsedEventHandler();

                    AbilitiesController.DeactivateAbility(ActivityId);
                }
            }
        }

        protected override void OnStartTurnEventHandler()
        {
            base.OnStartTurnEventHandler();

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
                        UnitOnDieEventHandler();
                    }
                    else if (CardKind == Enumerators.CardKind.SPELL)
                    {
                        SpellOnUsedEventHandler();
                        AbilitiesController.DeactivateAbility(ActivityId);
                    }
                }
            }
        }
    }
}
