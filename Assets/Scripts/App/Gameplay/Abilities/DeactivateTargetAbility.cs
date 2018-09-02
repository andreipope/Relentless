using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;
using UnityEngine;

namespace LoomNetwork.CZB
{
    public class DeactivateTargetAbility : AbilityBase
    {
        public int value = 1;

        private int _turnsLength;

        public DeactivateTargetAbility(Enumerators.CardKind cardKind, AbilityData ability, int value = 1)
            : base(cardKind, ability)
        {
            this.value = value;
        }

        public override void Activate()
        {
            base.Activate();

            _vfxObject = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/FrozenVFX");
            _turnsLength = value;
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

            switch (affectObjectType)
            {
                case Enumerators.AffectObjectType.CHARACTER:

                    targetUnit.Card.IsPlayable = false;
                    targetUnit.SetHighlightingEnabled(false);

                    CreateVFX(targetUnit.transform.position);
                    break;
            }
        }

        protected override void OnInputEndEventHandler()
        {
            base.OnInputEndEventHandler();

            if (_isAbilityResolved)
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
                if (cardKind == Enumerators.CardKind.CREATURE)
                {
                    // targetCreature.Card.DisconnectAbility((uint)abilityType);
                    UnitOnDieEventHandler();
                } else if (cardKind == Enumerators.CardKind.SPELL)
                {
                    SpellOnUsedEventHandler();

                    _abilitiesController.DeactivateAbility(activityId);
                }
            }
        }

        protected override void OnStartTurnEventHandler()
        {
            base.OnStartTurnEventHandler();

            if (_turnsLength > 0)
            {
                if (targetUnit != null)
                {
                    targetUnit.Card.IsPlayable = false;
                    targetUnit.SetHighlightingEnabled(false);
                } else
                {
                    if (cardKind == Enumerators.CardKind.CREATURE)
                    {
                        UnitOnDieEventHandler();
                    } else if (cardKind == Enumerators.CardKind.SPELL)
                    {
                        SpellOnUsedEventHandler();
                        _abilitiesController.DeactivateAbility(activityId);
                    }
                }
            }
        }
    }
}
