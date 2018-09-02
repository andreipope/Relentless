using DG.Tweening;
using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;
using LoomNetwork.Internal;
using UnityEngine;

namespace LoomNetwork.CZB
{
    public class DamageTargetAbility : AbilityBase
    {
        public int Value = 1;

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

        protected override void OnInputEndEventHandler()
        {
            base.OnInputEndEventHandler();

            if (IsAbilityResolved)
            {
                Action();
            }
        }

        protected override void CreateVfx(Vector3 pos, bool autoDestroy = false, float duration = 3f, bool justPosition = false)
        {
            switch (AbilityEffectType)
            {
                case Enumerators.AbilityEffectType.TargetRock:
                    VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Spells/SpellTargetFireAttack");
                    break;
                case Enumerators.AbilityEffectType.TargetFire:
                    VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Spells/SpellTargetFireAttack");
                    break;
                case Enumerators.AbilityEffectType.TargetLife:
                    VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Spells/SpellTargetLifeAttack");
                    break;
                case Enumerators.AbilityEffectType.TargetToxic:
                    VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Spells/SpellTargetToxicAttack");
                    break;
            }

            // base.CreateVFX(pos);
            Vector3 targetPosition = AffectObjectType == Enumerators.AffectObjectType.Character?TargetUnit.Transform.position:TargetPlayer.AvatarObject.transform.position;

            VfxObject = Object.Instantiate(VfxObject);
            VfxObject.transform.position = Utilites.CastVfxPosition(AbilityUnitOwner.Transform.position);
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
            object caller = AbilityUnitOwner != null?AbilityUnitOwner:(object)BoardSpell;

            switch (AffectObjectType)
            {
                case Enumerators.AffectObjectType.Player:

                    // if (targetPlayer.id == playerCallerOfAbility.id)
                    BattleController.AttackPlayerByAbility(caller, AbilityData, TargetPlayer);
                    break;
                case Enumerators.AffectObjectType.Character:

                    // playerCallerOfAbility.FightCreatureBySkill(value, targetCreature.card);
                    BattleController.AttackUnitByAbility(caller, AbilityData, TargetUnit);
                    break;
            }

            Vector3 targetPosition = VfxObject.transform.position;

            ClearParticles();

            switch (AbilityEffectType)
            {
                case Enumerators.AbilityEffectType.TargetRock:
                    VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/toxicDamageVFX");
                    break;
                case Enumerators.AbilityEffectType.TargetFire:
                    VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/toxicDamageVFX");
                    break;
                case Enumerators.AbilityEffectType.TargetLife:
                    VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/toxicDamageVFX");
                    break;
                case Enumerators.AbilityEffectType.TargetToxic:
                    VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/toxicDamageVFX");
                    break;
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
