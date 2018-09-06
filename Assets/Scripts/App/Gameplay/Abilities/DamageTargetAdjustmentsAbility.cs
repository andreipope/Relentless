using System;
using System.Collections.Generic;
using DG.Tweening;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Loom.ZombieBattleground
{
    public class DamageTargetAdjustmentsAbility : AbilityBase
    {
        public int Value { get; }

        public DamageTargetAdjustmentsAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Value = ability.Value;
        }

        public override void Activate()
        {
            base.Activate();

            switch (AbilityEffectType)
            {
                case Enumerators.AbilityEffectType.TARGET_ADJUSTMENTS_AIR:
                    VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/WhirlwindVFX");
                    break;
                default:
                    VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>(
                        "Prefabs/VFX/Spells/SpellTargetToxicAttack");
                    break;
            }
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

            BoardUnit unit = info as BoardUnit;

            Player playerOwner = unit.OwnerPlayer;

            BoardUnit leftAdjustment = null, rightAdjastment = null;

            int targetIndex = -1;
            List<BoardUnit> list = null;
            for (int i = 0; i < playerOwner.BoardCards.Count; i++)
            {
                if (playerOwner.BoardCards[i] == unit)
                {
                    targetIndex = i;
                    list = playerOwner.BoardCards;
                    break;
                }
            }

            object caller = AbilityUnitOwner != null ? AbilityUnitOwner : (object) BoardSpell;
            if (targetIndex > -1)
            {
                if (targetIndex - 1 > -1)
                {
                    leftAdjustment = list[targetIndex - 1];
                }

                if (targetIndex + 1 < list.Count)
                {
                    rightAdjastment = list[targetIndex + 1];
                }
            }

            if (leftAdjustment != null)
            {
                CreateAndMoveParticle(
                    () =>
                    {
                        BattleController.AttackUnitByAbility(caller, AbilityData, leftAdjustment);
                    },
                    leftAdjustment.Transform.position);
            }

            if (rightAdjastment != null)
            {
                CreateAndMoveParticle(
                    () =>
                    {
                        BattleController.AttackUnitByAbility(caller, AbilityData, rightAdjastment);
                    },
                    rightAdjastment.Transform.position);
            }
        }

        protected override void InputEndedHandler()
        {
            base.InputEndedHandler();

            object caller = AbilityUnitOwner != null ? AbilityUnitOwner : (object) BoardSpell;

            if (IsAbilityResolved)
            {
                switch (AffectObjectType)
                {
                    case Enumerators.AffectObjectType.CHARACTER:
                        Action(TargetUnit);
                        CreateAndMoveParticle(
                            () =>
                            {
                                BattleController.AttackUnitByAbility(caller, AbilityData, TargetUnit);
                            },
                            TargetUnit.Transform.position);

                        break;
                }
            }
        }

        protected override void UnitAttackedHandler(object info, int damage, bool isAttacker)
        {
            base.UnitAttackedHandler(info, damage, isAttacker);

            if (AbilityCallType != Enumerators.AbilityCallType.ATTACK || !isAttacker)
                return;

            Action(info);
        }

        private void CreateAndMoveParticle(Action callback, Vector3 targetPosition)
        {
            Vector3 startPosition = CardKind == Enumerators.CardKind.CREATURE ?
                AbilityUnitOwner.Transform.position :
                SelectedPlayer.Transform.position;
            if (AbilityCallType != Enumerators.AbilityCallType.ATTACK)
            {
                GameObject particleMain = Object.Instantiate(VfxObject);
                particleMain.transform.position = Utilites.CastVfxPosition(startPosition + Vector3.forward);
                particleMain.transform.DOMove(Utilites.CastVfxPosition(targetPosition), 0.5f).OnComplete(
                    () =>
                    {
                        callback();
                        if (AbilityEffectType == Enumerators.AbilityEffectType.TARGET_ADJUSTMENTS_BOMB)
                        {
                            DestroyParticle(particleMain, true);
                            GameObject prefab =
                                LoadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/toxicDamageVFX");
                            GameObject particle = Object.Instantiate(prefab);
                            particle.transform.position = Utilites.CastVfxPosition(targetPosition + Vector3.forward);
                            ParticlesController.RegisterParticleSystem(particle, true);

                            SoundManager.PlaySound(Enumerators.SoundType.SPELLS, "NailBomb",
                                Constants.SpellAbilitySoundVolume, Enumerators.CardSoundType.NONE);
                        }
                        else if (AbilityEffectType == Enumerators.AbilityEffectType.TARGET_ADJUSTMENTS_AIR)
                        {
                            // one particle
                            ParticleSystem.MainModule main = VfxObject.GetComponent<ParticleSystem>().main;
                            main.loop = false;
                        }
                    });
            }
            else
            {
                CreateVfx(Utilites.CastVfxPosition(TargetUnit.Transform.position));
                callback();
            }

            GameClient.Get<IGameplayManager>().RearrangeHands();
        }

        private void DestroyParticle(GameObject particleObj, bool isDirectly = false, float time = 3f)
        {
            if (isDirectly)
            {
                DestroyParticle(new object[]
                {
                    particleObj
                });
            }
            else
            {
                GameClient.Get<ITimerManager>().AddTimer(DestroyParticle, new object[]
                {
                    particleObj
                }, time);
            }
        }

        private void DestroyParticle(object[] param)
        {
            GameObject particleObj = param[0] as GameObject;
            Object.Destroy(particleObj);
        }
    }
}
