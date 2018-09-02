// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/

using System;
using System.Collections.Generic;
using DG.Tweening;
using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;
using LoomNetwork.Internal;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LoomNetwork.CZB
{
    public class DamageTargetAdjustmentsAbility : AbilityBase
    {
        public int value = 1;

        public DamageTargetAdjustmentsAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            value = ability.value;
        }

        public override void Activate()
        {
            base.Activate();

            switch (abilityEffectType)
            {
                case Enumerators.AbilityEffectType.TARGET_ADJUSTMENTS_AIR:
                    _vfxObject = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/WhirlwindVFX");
                    break;
                default:
                    _vfxObject = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Spells/SpellTargetToxicAttack");
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

            Player playerOwner = unit.ownerPlayer;

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

            object caller = abilityUnitOwner != null?abilityUnitOwner:(object)boardSpell;

            /*if (targetIndex == -1)
                for (int i = 0; i < playerCallerOfAbility.BoardCards.Count; i++)
                {
                    if (playerCallerOfAbility.BoardCards[i] == creature)
                    {
                        targetIndex = i;
                        list = playerCallerOfAbility.BoardCards;
                        break;
                    }
                }
                */
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
                // CreateVFX(cardCaller.transform.position);
                // CreateAndMoveParticle(() => playerCallerOfAbility.FightCreatureBySkill(value, leftAdjustment.card), leftAdjustment.transform.position);
                CreateAndMoveParticle(
                    () =>
                    {
                        _battleController.AttackUnitByAbility(caller, abilityData, leftAdjustment);
                    },
                    leftAdjustment.transform.position);
            }

            if (rightAdjastment != null)
            {
                // cardCaller.FightCreatureBySkill(value, rightAdjastment.card);
                // CreateAndMoveParticle(() => playerCallerOfAbility.FightCreatureBySkill(value, rightAdjastment.card), rightAdjastment.transform.position);
                CreateAndMoveParticle(
                    () =>
                    {
                        _battleController.AttackUnitByAbility(caller, abilityData, rightAdjastment);
                    },
                    rightAdjastment.transform.position);
            }
        }

        protected override void OnInputEndEventHandler()
        {
            base.OnInputEndEventHandler();

            object caller = abilityUnitOwner != null?abilityUnitOwner:(object)boardSpell;

            if (_isAbilityResolved)
            {
                switch (affectObjectType)
                {
                    /*      case Enumerators.AffectObjectType.PLAYER:
                              //if (targetPlayer.playerInfo.netId == playerCallerOfAbility.netId)
                              //    CreateAndMoveParticle(() => playerCallerOfAbility.FightPlayerBySkill(value, false), targetPlayer.transform.position);
                              //else
                              //    CreateAndMoveParticle(() => playerCallerOfAbility.FightPlayerBySkill(value), targetPlayer.transform.position);
                              CreateAndMoveParticle(() => _battleController.AttackPlayerByAbility(caller, abilityData, targetPlayer), targetPlayer.AvatarObject.transform.position);
                              break; */
                    case Enumerators.AffectObjectType.CHARACTER:
                        Action(targetUnit);
                        CreateAndMoveParticle(
                            () =>
                            {
                                _battleController.AttackUnitByAbility(caller, abilityData, targetUnit);
                            },
                            targetUnit.transform.position);

                        break;
                }
            }
        }

        protected override void UnitOnAttackEventHandler(object info, int damage, bool isAttacker)
        {
            base.UnitOnAttackEventHandler(info, damage, isAttacker);

            if ((abilityCallType != Enumerators.AbilityCallType.ATTACK) || !isAttacker)

                return;

            Action(info);
        }

        private void CreateAndMoveParticle(Action callback, Vector3 targetPosition)
        {
            Vector3 startPosition = cardKind == Enumerators.CardKind.CREATURE?abilityUnitOwner.transform.position:selectedPlayer.Transform.position;
            if (abilityCallType != Enumerators.AbilityCallType.ATTACK)
            {
                // CreateVFX(cardCaller.transform.position);
                GameObject particleMain = Object.Instantiate(_vfxObject);
                particleMain.transform.position = Utilites.CastVFXPosition(startPosition + Vector3.forward);
                particleMain.transform.DOMove(Utilites.CastVFXPosition(targetPosition), 0.5f).OnComplete(
                    () =>
                    {
                        callback();
                        if (abilityEffectType == Enumerators.AbilityEffectType.TARGET_ADJUSTMENTS_BOMB)
                        {
                            DestroyParticle(particleMain, true);
                            GameObject prefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/toxicDamageVFX");
                            GameObject particle = Object.Instantiate(prefab);
                            particle.transform.position = Utilites.CastVFXPosition(targetPosition + Vector3.forward);
                            _particlesController.RegisterParticleSystem(particle, true);

                            _soundManager.PlaySound(Enumerators.SoundType.SPELLS, "NailBomb", Constants.SPELL_ABILITY_SOUND_VOLUME, Enumerators.CardSoundType.NONE);
                        } else if (abilityEffectType == Enumerators.AbilityEffectType.TARGET_ADJUSTMENTS_AIR)
                        {
                            // one particle
                            ParticleSystem.MainModule main = _vfxObject.GetComponent<ParticleSystem>().main;
                            main.loop = false;
                        }
                    });
            } else
            {
                CreateVFX(Utilites.CastVFXPosition(targetUnit.transform.position));
                callback();
            }

            GameClient.Get<IGameplayManager>().RearrangeHands();
        }

        private void DestroyParticle(GameObject particleObj, bool isDirectly = false, float time = 3f)
        {
            if (isDirectly)
            {
                DestroyParticle(new object[] { particleObj });
            } else
            {
                GameClient.Get<ITimerManager>().AddTimer(DestroyParticle, new object[] { particleObj }, time, false);
            }
        }

        private void DestroyParticle(object[] param)
        {
            GameObject particleObj = param[0] as GameObject;
            Object.Destroy(particleObj);
        }
    }
}
