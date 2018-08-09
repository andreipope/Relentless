// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



using System;
using System.Collections.Generic;
using LoomNetwork.CZB.Common;
using UnityEngine;
using LoomNetwork.CZB.Data;

namespace LoomNetwork.CZB
{
    public class DeactivateTargetAbility : AbilityBase
    {
        private int _turnsLength;

        public int value = 1;

        public DeactivateTargetAbility(Enumerators.CardKind cardKind, AbilityData ability, int value = 1) : base(cardKind, ability)
        {
            this.value = value;
        }

        public override void Activate()
        {
            base.Activate();

            _vfxObject = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/FrozenVFX");
            _turnsLength = (int)(value);
        }

        public override void Update() { }

        public override void Dispose() { }

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
                if (this.cardKind == Enumerators.CardKind.CREATURE)
                {
                    // targetCreature.Card.DisconnectAbility((uint)abilityType);
                    UnitOnDieEventHandler();
                }
                else if (this.cardKind == Enumerators.CardKind.SPELL)
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
                }
                else
                {
                    if (this.cardKind == Enumerators.CardKind.CREATURE)
                    {
                        UnitOnDieEventHandler();
                    }
                    else if (this.cardKind == Enumerators.CardKind.SPELL)
                    {

                        SpellOnUsedEventHandler();
                        _abilitiesController.DeactivateAbility(activityId);
                    }
                }
            }
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
                default: break;
            }
        }
    }
}