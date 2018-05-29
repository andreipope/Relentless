using System;
using System.Collections.Generic;
using GrandDevs.CZB.Common;
using CCGKit;
using UnityEngine;
using GrandDevs.CZB.Data;
using DG.Tweening;

namespace GrandDevs.CZB
{
    public class HealTargetAbility : AbilityBase
    {
        public int value = 1;

        public HealTargetAbility(Enumerators.CardKind cardKind, AbilityData ability) : base(cardKind, ability)
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
        public override void Action(object info = null)
        {
            base.Action(info);

            switch (affectObjectType)
            {
                case Enumerators.AffectObjectType.PLAYER:
                    if (targetPlayer.playerInfo.netId == playerCallerOfAbility.netId)
                        CreateAndMoveParticle(() => { playerCallerOfAbility.HealPlayerBySkill(value, false); }, targetPlayer.transform.position);
                    else
                        CreateAndMoveParticle(() => { playerCallerOfAbility.HealPlayerBySkill(value); }, targetPlayer.transform.position);
                    break;
                case Enumerators.AffectObjectType.CHARACTER:

                    CreateAndMoveParticle(() => { playerCallerOfAbility.HealCreatureBySkill(value, targetCreature.card); }, targetCreature.transform.position);
                    break;
                default: break;
            }
        }

        private void CreateAndMoveParticle(Action callback, Vector3 target)
        {
           
            if (abilityEffectType == Enumerators.AbilityEffectType.HEAL)
            {
                Vector3 startPosition = cardKind == Enumerators.CardKind.CREATURE ? boardCreature.transform.position : selectedPlayer.transform.position;
                _vfxObject = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Spells/SpellTargetLifeAttack");
                CreateVFX(startPosition);
                _vfxObject.transform.DOMove(target, 0.5f).OnComplete(() => {
                    DestroyCurrentParticle(true);
                    _vfxObject = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/GreenHealVFX");
                    CreateVFX(target);
                    callback();
                    DestroyCurrentParticle();
                });
            }
            else if(abilityEffectType == Enumerators.AbilityEffectType.HEAL_DIRECTLY)
            {
                CreateVFX(target);
                callback();
                DestroyCurrentParticle();
            }
        }
    }
}