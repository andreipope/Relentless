using System;
using System.Collections.Generic;
using GrandDevs.CZB.Common;
using CCGKit;
using UnityEngine;

namespace GrandDevs.CZB
{
    public class DeactivateTargetAbility : AbilityBase
    {
        private int _turnsLength;

        public double value = 2;

        public DeactivateTargetAbility(Enumerators.Ability abilityId, Enumerators.CardKind cardKind, Enumerators.AbilityType abilType, Enumerators.AbilityActivityType type, Enumerators.AbilityCallType abilityCallType, List<Enumerators.AbilityTargetType> targetTypes,
                                     double value = 2) : base(abilityId, cardKind, abilType, type, abilityCallType, targetTypes)
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
                switch (affectObjectType)
                {
                    case Enumerators.AffectObjectType.CHARACTER:

                        targetCreature.isPlayable = false;
                        targetCreature.SetHighlightingEnabled(false);

                        CreateVFX(targetCreature.transform.position);
                        break;
                    default: break;
                }
            }
        }
        protected override void OnEndTurnEventHandler()
        {
            base.OnEndTurnEventHandler();

            _turnsLength--;

            if (_turnsLength <= 0)
            {
                if (selfCardKind == Enumerators.CardKind.CREATURE)
                {
                    targetCreature.card.DisconnectAbility((uint)abilityType);
                    CreatureOnDieEventHandler();
                }
                else if (selfCardKind == Enumerators.CardKind.SPELL)
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
                    targetCreature.isPlayable = false;
                    targetCreature.SetHighlightingEnabled(false);
                }
                else
                {
                    if (selfCardKind == Enumerators.CardKind.CREATURE)
                    {
                        CreatureOnDieEventHandler();
                    }
                    else if (selfCardKind == Enumerators.CardKind.SPELL)
                        SpellOnUsedEventHandler();
                }
            }
        }
    }
}