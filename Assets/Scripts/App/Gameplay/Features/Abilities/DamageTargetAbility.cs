using System;
using System.Collections.Generic;
using GrandDevs.CZB.Common;
using CCGKit;
using UnityEngine;
using GrandDevs.CZB.Data;
using DG.Tweening;

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
        public override void Action(object info = null)
        {
            base.Action(info);

            CreateVFX(Vector3.zero);


        }

        protected override void CreateVFX(Vector3 pos, bool autoDestroy = false, float duration = 3f)
        {
            switch (abilityEffectType)
            {
                case Enumerators.AbilityEffectType.TARGET_ROCK:
                    _vfxObject = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Spells/SpellTargetRockAttack");
                    break;
                case Enumerators.AbilityEffectType.TARGET_FIRE:
                    _vfxObject = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Spells/SpellTargetFireAttack");
                    break;
                case Enumerators.AbilityEffectType.TARGET_LIFE:
                    _vfxObject = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Spells/SpellTargetLifeAttack");
                    break;
                case Enumerators.AbilityEffectType.TARGET_TOXIC:
                    _vfxObject = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Spells/SpellTargetToxicAttack");
                    break;
                default:
                    break;
            }
            //base.CreateVFX(pos);
            var targetPosition = affectObjectType == Enumerators.AffectObjectType.CHARACTER ? targetCreature.transform.position : targetPlayer.transform.position;

            _vfxObject = MonoBehaviour.Instantiate(_vfxObject);
            _vfxObject.transform.position = boardCreature.transform.position;
            _vfxObject.transform.DOMove(targetPosition, 0.5f).OnComplete(ActionCompleted);

            ulong id = _particlesController.RegisterParticleSystem(_vfxObject, autoDestroy, duration);

            if(!autoDestroy)
                _particleIds.Add(id);
        }

        private void ActionCompleted()
        {
            switch (affectObjectType)
            {
                case Enumerators.AffectObjectType.PLAYER:
                    if (targetPlayer.playerInfo.netId == playerCallerOfAbility.netId)
                        playerCallerOfAbility.FightPlayerBySkill(value, false);
                    else
                        playerCallerOfAbility.FightPlayerBySkill(value);
                    break;
                case Enumerators.AffectObjectType.CHARACTER:
                    playerCallerOfAbility.FightCreatureBySkill(value, targetCreature.card);
                    break;
                default: break;
            }

            var targetPosition = _vfxObject.transform.position;

            ClearParticles();

            switch (abilityEffectType)
            {
                case Enumerators.AbilityEffectType.TARGET_ROCK:
                    _vfxObject = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Spells/rockDamageVFX");
                    break;
                case Enumerators.AbilityEffectType.TARGET_FIRE:
                    _vfxObject = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/fireDamage2VFX");
                    break;
                case Enumerators.AbilityEffectType.TARGET_LIFE:
                    _vfxObject = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/ToxicAttackVFX");
                    break;
                case Enumerators.AbilityEffectType.TARGET_TOXIC:
                    _vfxObject = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/toxicDamageVFX");
                    break;
                default:
                    break;
            }
            _vfxObject = MonoBehaviour.Instantiate(_vfxObject);
            _vfxObject.transform.position = targetPosition;
            _particlesController.RegisterParticleSystem(_vfxObject, true);
        }
    }
}