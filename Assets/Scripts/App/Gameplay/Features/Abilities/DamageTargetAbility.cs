using System;
using System.Collections.Generic;
using GrandDevs.CZB.Common;
using CCGKit;
using UnityEngine;
using GrandDevs.CZB.Data;

namespace GrandDevs.CZB
{
    public class DamageTargetAbility : AbilityBase
    {
        public int value = 1;

        public DamageTargetAbility(Enumerators.CardKind cardKind, AbilityData ability) : base(cardKind, ability)
        {
            this.value = ability.value;
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
                Action();
            }
        }
        public override void Action(RuntimeCard attacked = null)
        {
            base.Action(attacked);

            switch (affectObjectType)
            {
                case Enumerators.AffectObjectType.PLAYER:
                    if (targetPlayer.playerInfo.netId == cardCaller.netId)
                        cardCaller.FightPlayerBySkill(value, false);
                    else
                        cardCaller.FightPlayerBySkill(value);
                    CreateVFX(targetPlayer.transform.position);
                    break;
                case Enumerators.AffectObjectType.CHARACTER:
                    cardCaller.FightCreatureBySkill(value, targetCreature.card);
                    CreateVFX(targetCreature.transform.position);
                    break;
                default: break;
            }
        }
    }
}