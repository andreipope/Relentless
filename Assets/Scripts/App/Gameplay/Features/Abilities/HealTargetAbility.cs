using System;
using System.Collections.Generic;
using GrandDevs.CZB.Common;
using CCGKit;
using UnityEngine;

namespace GrandDevs.CZB
{
    public class HealTargetAbility : AbilityBase
    {
        public int value = 1;

        public HealTargetAbility(Enumerators.Ability abilityId, Enumerators.CardKind cardKind, Enumerators.AbilityType abilType, Enumerators.AbilityActivityType type, Enumerators.AbilityCallType abilityCallType, List<Enumerators.AbilityTargetType> targetTypes,
                                     int value = 1) : base(abilityId, cardKind, abilType, type, abilityCallType, targetTypes)
        {
            this.value = value;
        }

        public override void Activate()
        {
            base.Activate();

            _vfxObject = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/healVFX");
        }

        public override void Update() { }

        public override void Dispose() { }

        protected override void OnInputEndEventHandler()
        {
            base.OnInputEndEventHandler();

            if (_isAbilityResolved)
            {
                switch(affectObjectType)
                {
                    case Enumerators.AffectObjectType.PLAYER:
                        cardCaller.HealPlayerBySkill(value);
                        CreateVFX(targetPlayer.transform.position);
                        break;
                    case Enumerators.AffectObjectType.CHARACTER:
                        cardCaller.HealCreatureBySkill(value, targetCreature.card);
                        CreateVFX(targetCreature.transform.position);
                        break;
                    default: break;
                }
            }
        }
    }
}