using DG.Tweening;
using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;
using LoomNetwork.Internal;
using UnityEngine;

namespace LoomNetwork.CZB
{
    public class DamageTargetAbility : AbilityBase
    {
        public int value = 1;

        public DamageTargetAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            value = ability.value;
        }

        public override void Activate()
        {
            base.Activate();

            _vfxObject = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/toxicDamageVFX");
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

            CreateVFX(Vector3.zero);
        }

        protected override void OnInputEndEventHandler()
        {
            base.OnInputEndEventHandler();

            if (_isAbilityResolved)
            {
                Action();
            }
        }

        protected override void CreateVFX(Vector3 pos, bool autoDestroy = false, float duration = 3f, bool justPosition = false)
        {
            switch (abilityEffectType)
            {
                case Enumerators.AbilityEffectType.TARGET_ROCK:
                    _vfxObject = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Spells/SpellTargetFireAttack");
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
            }

            // base.CreateVFX(pos);
            Vector3 targetPosition = affectObjectType == Enumerators.AffectObjectType.CHARACTER?targetUnit.transform.position:targetPlayer.AvatarObject.transform.position;

            _vfxObject = Object.Instantiate(_vfxObject);
            _vfxObject.transform.position = Utilites.CastVFXPosition(abilityUnitOwner.transform.position);
            targetPosition = Utilites.CastVFXPosition(targetPosition);
            _vfxObject.transform.DOMove(targetPosition, 0.5f).OnComplete(ActionCompleted);
            ulong id = _particlesController.RegisterParticleSystem(_vfxObject, autoDestroy, duration);

            if (!autoDestroy)
            {
                _particleIds.Add(id);
            }
        }

        private void ActionCompleted()
        {
            object caller = abilityUnitOwner != null?abilityUnitOwner:(object)boardSpell;

            switch (affectObjectType)
            {
                case Enumerators.AffectObjectType.PLAYER:

                    // if (targetPlayer.id == playerCallerOfAbility.id)
                    _battleController.AttackPlayerByAbility(caller, abilityData, targetPlayer);
                    break;
                case Enumerators.AffectObjectType.CHARACTER:

                    // playerCallerOfAbility.FightCreatureBySkill(value, targetCreature.card);
                    _battleController.AttackUnitByAbility(caller, abilityData, targetUnit);
                    break;
            }

            Vector3 targetPosition = _vfxObject.transform.position;

            ClearParticles();

            switch (abilityEffectType)
            {
                case Enumerators.AbilityEffectType.TARGET_ROCK:
                    _vfxObject = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/toxicDamageVFX");
                    break;
                case Enumerators.AbilityEffectType.TARGET_FIRE:
                    _vfxObject = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/toxicDamageVFX");
                    break;
                case Enumerators.AbilityEffectType.TARGET_LIFE:
                    _vfxObject = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/toxicDamageVFX");
                    break;
                case Enumerators.AbilityEffectType.TARGET_TOXIC:
                    _vfxObject = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/toxicDamageVFX");
                    break;
            }

            if (_vfxObject != null)
            {
                _vfxObject = Object.Instantiate(_vfxObject);
                _vfxObject.transform.position = targetPosition;
                _particlesController.RegisterParticleSystem(_vfxObject, true);
            }
        }
    }
}
