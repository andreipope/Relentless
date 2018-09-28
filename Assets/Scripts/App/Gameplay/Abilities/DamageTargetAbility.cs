using System;
using DG.Tweening;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Loom.ZombieBattleground
{
    public class DamageTargetAbility : AbilityBase
    {
        public int Value { get; }

        public DamageTargetAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Value = ability.Value;
        }

        public override void Activate()
        {
            base.Activate();

            VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/toxicDamageVFX");
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

            CreateVfx(Vector3.zero);
        }

        protected override void InputEndedHandler()
        {
            base.InputEndedHandler();

            if (IsAbilityResolved)
            {
                Action();
            }
        }

        protected override void CreateVfx(
            Vector3 pos, bool autoDestroy = false, float duration = 3f, bool justPosition = false)
        {
            switch (AbilityEffectType)
            {
                case Enumerators.AbilityEffectType.TARGET_ROCK:
                    VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>(
                        "Prefabs/VFX/Spells/SpellTargetFireAttack");
                    break;
                case Enumerators.AbilityEffectType.TARGET_FIRE:
                    VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>(
                        "Prefabs/VFX/Spells/SpellTargetFireAttack");
                    break;
                case Enumerators.AbilityEffectType.TARGET_LIFE:
                    VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>(
                        "Prefabs/VFX/Spells/SpellTargetLifeAttack");
                    break;
                case Enumerators.AbilityEffectType.TARGET_TOXIC:
                    VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>(
                        "Prefabs/VFX/Spells/SpellTargetToxicAttack");
                    break;
            }

            Vector3 targetPosition =
                AffectObjectType == Enumerators.AffectObjectType.CHARACTER ?
                BattlegroundController.GetBoardUnitViewByModel(TargetUnit).Transform.position :
                TargetPlayer.AvatarObject.transform.position;

            VfxObject = Object.Instantiate(VfxObject);
            VfxObject.transform.position = Utilites.CastVfxPosition(GetAbilityUnitOwnerView().Transform.position);
            targetPosition = Utilites.CastVfxPosition(targetPosition);
            VfxObject.transform.DOMove(targetPosition, 0.5f).OnComplete(ActionCompleted);
            ulong id = ParticlesController.RegisterParticleSystem(VfxObject, autoDestroy, duration);

            if (!autoDestroy)
            {
                ParticleIds.Add(id);
            }
        }

        private void ActionCompleted()
        {
            object caller = AbilityUnitOwner != null ? AbilityUnitOwner : (object) BoardSpell;

            switch (AffectObjectType)
            {
                case Enumerators.AffectObjectType.PLAYER:
                    BattleController.AttackPlayerByAbility(caller, AbilityData, TargetPlayer);
                    break;
                case Enumerators.AffectObjectType.CHARACTER:
                    BattleController.AttackUnitByAbility(caller, AbilityData, TargetUnit);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(AffectObjectType), AffectObjectType, null);
            }

            Vector3 targetPosition = VfxObject.transform.position;

            ClearParticles();

            switch (AbilityEffectType)
            {
                case Enumerators.AbilityEffectType.TARGET_ROCK:
                    VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/toxicDamageVFX");
                    break;
                case Enumerators.AbilityEffectType.TARGET_FIRE:
                    VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/toxicDamageVFX");
                    break;
                case Enumerators.AbilityEffectType.TARGET_LIFE:
                    VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/toxicDamageVFX");
                    break;
                case Enumerators.AbilityEffectType.TARGET_TOXIC:
                    VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/toxicDamageVFX");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(AbilityEffectType), AbilityEffectType, null);
            }

            if (VfxObject != null)
            {
                VfxObject = Object.Instantiate(VfxObject);
                VfxObject.transform.position = targetPosition;
                ParticlesController.RegisterParticleSystem(VfxObject, true);
            }
        }
    }
}
