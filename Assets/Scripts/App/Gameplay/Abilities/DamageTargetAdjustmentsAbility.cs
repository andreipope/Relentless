// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



using System;
using System.Collections.Generic;
using LoomNetwork.CZB.Common;
using UnityEngine;
using LoomNetwork.CZB.Data;
using DG.Tweening;
using LoomNetwork.Internal;

namespace LoomNetwork.CZB
{
    public class DamageTargetAdjustmentsAbility : AbilityBase
    {
        public int value = 1;

        public DamageTargetAdjustmentsAbility(Enumerators.CardKind cardKind, AbilityData ability) : base(cardKind, ability)
        {
            this.value = ability.value;
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

        public override void Update() { }

        public override void Dispose() { }

        protected override void OnInputEndEventHandler()
        {
            base.OnInputEndEventHandler();

            if (_isAbilityResolved)
            {
                switch (affectObjectType)
                {
                    case Enumerators.AffectObjectType.PLAYER:
                        //if (targetPlayer.playerInfo.netId == playerCallerOfAbility.netId)
                        //    CreateAndMoveParticle(() => playerCallerOfAbility.FightPlayerBySkill(value, false), targetPlayer.transform.position);
                        //else
                        //    CreateAndMoveParticle(() => playerCallerOfAbility.FightPlayerBySkill(value), targetPlayer.transform.position);
                        CreateAndMoveParticle(() => _battleController.AttackPlayerByAbility(abilityUnitOwner, abilityData, targetPlayer), targetPlayer.AvatarObject.transform.position);
                        break;
                    case Enumerators.AffectObjectType.CHARACTER:
                        Action(targetUnit);
                        CreateAndMoveParticle(() =>
                        {
                            _battleController.AttackCreatureByAbility(abilityUnitOwner, abilityData, targetUnit);

                        }, targetUnit.transform.position);
                
                        break;
                    default: break;
                }
            }
        }
        public override void Action(object info = null)
        {
            base.Action(info);

            Player opponent = _gameplayManager.OpponentPlayer;

            var creature = info as BoardUnit;

            BoardUnit leftAdjustment = null,
                    rightAdjastment = null;

            int targetIndex = -1;
            List<BoardUnit> list = null;
            for (int i = 0; i < opponent.BoardCards.Count; i++)
            {
                if (opponent.BoardCards[i] == creature)
                {
                    targetIndex = i;
                    list = opponent.BoardCards;
                    break;
                }
            }
            if (targetIndex == -1)
                for (int i = 0; i < playerCallerOfAbility.BoardCards.Count; i++)
                {
                    if (playerCallerOfAbility.BoardCards[i] == creature)
                    {
                        targetIndex = i;
                        list = playerCallerOfAbility.BoardCards;
                        break;
                    }
                }
            if (targetIndex > -1)
            {
                if (targetIndex - 1 > -1)
                    leftAdjustment = list[targetIndex - 1];
                if (targetIndex + 1 < list.Count)
                    rightAdjastment = list[targetIndex + 1];
            }

            if (leftAdjustment != null)
            {
                //CreateVFX(cardCaller.transform.position);
                //CreateAndMoveParticle(() => playerCallerOfAbility.FightCreatureBySkill(value, leftAdjustment.card), leftAdjustment.transform.position);
                CreateAndMoveParticle(() =>
                {
                    _battleController.AttackCreatureByAbility(abilityUnitOwner, abilityData, leftAdjustment);

                }, leftAdjustment.transform.position);
            }

            if (rightAdjastment != null)
            {
                //cardCaller.FightCreatureBySkill(value, rightAdjastment.card);
                //CreateAndMoveParticle(() => playerCallerOfAbility.FightCreatureBySkill(value, rightAdjastment.card), rightAdjastment.transform.position);
                CreateAndMoveParticle(() =>
                {
                    _battleController.AttackCreatureByAbility(abilityUnitOwner, abilityData, rightAdjastment);

                }, rightAdjastment.transform.position);
            }
        }

        private void CreateAndMoveParticle(Action callback, Vector3 targetPosition)
        {
            Vector3 startPosition = cardKind == Enumerators.CardKind.CREATURE ? abilityUnitOwner.transform.position : selectedPlayer.Transform.position;
            if (abilityCallType != Enumerators.AbilityCallType.AT_ATTACK)
            {
                //CreateVFX(cardCaller.transform.position);
                var particleMain = MonoBehaviour.Instantiate(_vfxObject);
                particleMain.transform.position = Utilites.CastVFXPosition(startPosition + Vector3.forward);
                particleMain.transform.DOMove(Utilites.CastVFXPosition(targetPosition), 0.5f).OnComplete(() => 
                {
                    callback();
                    if(abilityEffectType == Enumerators.AbilityEffectType.TARGET_ADJUSTMENTS_BOMB)
                    {
                        DestroyParticle(particleMain, true);
                        var prefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/NailBombVFX");
                        var particle  = MonoBehaviour.Instantiate(prefab);
                        particle.transform.position = Utilites.CastVFXPosition(targetPosition + Vector3.forward);
                        _particlesController.RegisterParticleSystem(particle, true);
                    }
                    else if(abilityEffectType == Enumerators.AbilityEffectType.TARGET_ADJUSTMENTS_AIR) //one particle
                    {
                        var main = _vfxObject.GetComponent<ParticleSystem>().main;
                        main.loop = false;
                    } 
                });
            }
            else
            {
                CreateVFX(Utilites.CastVFXPosition(targetUnit.transform.position));
                callback();
            }

            GameClient.Get<IGameplayManager>().RearrangeHands();
        }

        private void DestroyParticle(GameObject particleObj, bool isDirectly = false, float time = 3f)
        {
            if (isDirectly)
                DestroyParticle(new object[] { particleObj });
            else
                GameClient.Get<ITimerManager>().AddTimer(DestroyParticle, new object[] { particleObj }, time, false);
        }


        private void DestroyParticle(object[] param)
        {
            var particleObj = param[0] as GameObject;
            MonoBehaviour.Destroy(particleObj);
        }
    }
}