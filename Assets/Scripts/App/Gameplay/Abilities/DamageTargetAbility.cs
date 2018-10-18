using System;
using System.Collections.Generic;
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

            // lets improve this when it will be possible ofr the VFX that it can be used more accurate for different cards!
            if (AbilityUnitOwner != null && AbilityUnitOwner.Card.LibraryCard.Name == "Jetter")
            {
                VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/IceBoltVFX");
            }
            else
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
            }

            Vector3 targetPosition =
                AffectObjectType == Enumerators.AffectObjectType.Character ?
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

            object target = null;

            switch (AffectObjectType)
            {
                case Enumerators.AffectObjectType.Player:
                    BattleController.AttackPlayerByAbility(caller, AbilityData, TargetPlayer);
                    AbilitiesController.ThrowUseAbilityEvent(MainWorkingCard, new List<BoardObject>()
                    {
                        TargetPlayer
                    },  AbilityData.AbilityType, Protobuf.AffectObjectType.Player);

                    target = TargetPlayer;
                    break;
                case Enumerators.AffectObjectType.Character:
                    BattleController.AttackUnitByAbility(caller, AbilityData, TargetUnit);
                    AbilitiesController.ThrowUseAbilityEvent(MainWorkingCard, new List<BoardObject>()
                    {
                        TargetUnit
                    }, AbilityData.AbilityType, Protobuf.AffectObjectType.Character);

                    target = TargetUnit;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(AffectObjectType), AffectObjectType, null);
            }

            ActionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
            {
                ActionType = Enumerators.ActionType.CardAffectingCard,
                Caller = GetCaller(),
                TargetEffects = new List<PastActionsPopup.TargetEffectParam>()
                {
                    new PastActionsPopup.TargetEffectParam()
                    {
                        ActionEffectType = Enumerators.ActionEffectType.ShieldDebuff,
                        Target = target,
                        HasValue = true,
                        Value = -AbilityData.Value
                    }
                }
            });

            Vector3 targetPosition = VfxObject.transform.position;

            ClearParticles();

            // lets improve this when it will be possible ofr the VFX that it can be used more accurate for different cards!
            if (AbilityUnitOwner != null && AbilityUnitOwner.Card.LibraryCard.Name == "Jetter")
            {
                VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/IceBolt_Impact");
            }
            else
            {
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
                        break;
                }
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
