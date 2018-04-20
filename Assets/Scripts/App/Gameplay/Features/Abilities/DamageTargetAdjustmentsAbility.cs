using System;
using System.Collections.Generic;
using GrandDevs.CZB.Common;
using CCGKit;
using UnityEngine;
using GrandDevs.CZB.Data;
using DG.Tweening;

namespace GrandDevs.CZB
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
                        if (targetPlayer.playerInfo.netId == cardCaller.netId)
                            CreateAndMoveParticle(() => cardCaller.FightPlayerBySkill(value, false), targetPlayer.transform.position);
                        else
                            CreateAndMoveParticle(() => cardCaller.FightPlayerBySkill(value), targetPlayer.transform.position);
                        break;
                    case Enumerators.AffectObjectType.CHARACTER:
                        Action(targetCreature);

                        //cardCaller.FightCreatureBySkill(value, targetCreature.card);
                        //CreateVFX(cardCaller.transform.position);
                        //CreateAndMoveParticle(targetCreature);
                        CreateAndMoveParticle(() => cardCaller.FightCreatureBySkill(value, targetCreature.card), targetCreature.transform.position);
                        break;
                    default: break;
                }

                
            }
        }
        public override void Action(object info = null)
        {
            base.Action(info);

            var creature = info as BoardCreature;

            BoardCreature leftAdjustment = null,
                    rightAdjastment = null;

            int targetIndex = -1;
            List<BoardCreature> list = null;
            for (int i = 0; i < cardCaller.opponentBoardCardsList.Count; i++)
            {
                if (cardCaller.opponentBoardCardsList[i] == creature)
                {
                    targetIndex = i;
                    list = cardCaller.opponentBoardCardsList;
                    break;
                }
            }
            if(targetIndex == -1)
            for (int i = 0; i < cardCaller.playerBoardCardsList.Count; i++)
            {
                if (cardCaller.playerBoardCardsList[i] == creature)
                {
                    targetIndex = i;
                    list = cardCaller.playerBoardCardsList;
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
                CreateAndMoveParticle(() => cardCaller.FightCreatureBySkill(value, leftAdjustment.card), leftAdjustment.transform.position);
            }

            if (rightAdjastment != null)
            {
                //cardCaller.FightCreatureBySkill(value, rightAdjastment.card);
                CreateAndMoveParticle(() => cardCaller.FightCreatureBySkill(value, rightAdjastment.card), rightAdjastment.transform.position);
            }
        }

        private void CreateAndMoveParticle(Action callback, Vector3 targetPosition)
        {
            Vector3 startPosition = cardKind == Enumerators.CardKind.CREATURE ? boardCreature.transform.position : selectedPlayer.transform.position;
            if (abilityCallType != Enumerators.AbilityCallType.AT_ATTACK)
            {
                //CreateVFX(cardCaller.transform.position);
                var particleMain = MonoBehaviour.Instantiate(_vfxObject);
                particleMain.transform.position = startPosition + Vector3.forward;
                particleMain.transform.DOMove(targetPosition, 0.5f).OnComplete(() => 
                {
                    callback();
                    if(abilityEffectType == Enumerators.AbilityEffectType.TARGET_ADJUSTMENTS_BOMB)
                    {
                        DestroyParticle(particleMain, true);
                        var prefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/toxicDamageVFX");
                        var particle  = MonoBehaviour.Instantiate(prefab);
                        particle.transform.position = targetPosition + Vector3.forward;
                        DestroyParticle(particle);
                    }
                });
            }
            else
            {
                CreateVFX(targetCreature.transform.position);
                callback();
            }
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