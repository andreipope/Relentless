using System;
using System.Collections.Generic;
using GrandDevs.CZB.Common;
using CCGKit;
using UnityEngine;
using GrandDevs.CZB.Data;

namespace GrandDevs.CZB
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

            _vfxObject = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/fireDamageVFX");
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
                    targetCreature.card.DisconnectAbility((uint)abilityType);
                    CreatureOnDieEventHandler();
                }
                else if (this.cardKind == Enumerators.CardKind.SPELL)
                    SpellOnUsedEventHandler();
            }
        }


        protected override void OnStartTurnEventHandler()
        {
            base.OnStartTurnEventHandler();

            if (_turnsLength > 0)
            {
                if (targetCreature != null)
                {
                    targetCreature.card.isPlayable = false;
                    targetCreature.SetHighlightingEnabled(false);
                }
                else
                {
                    if (this.cardKind == Enumerators.CardKind.CREATURE)
                    {
                        CreatureOnDieEventHandler();
                    }
                    else if (this.cardKind == Enumerators.CardKind.SPELL)
                        SpellOnUsedEventHandler();
                }
            }
        }
        public override void Action(object info = null)
        {
            base.Action(info);

            switch (affectObjectType)
            {
                case Enumerators.AffectObjectType.CHARACTER:

                    targetCreature.card.isPlayable = false;
                    targetCreature.SetHighlightingEnabled(false);

                    CreateVFX(targetCreature.transform.position);
                    break;
                default: break;
            }
        }
    }
}