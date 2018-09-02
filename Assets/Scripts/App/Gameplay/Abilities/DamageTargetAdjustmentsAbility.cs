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
        public int Value = 1;

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
                case Enumerators.AbilityEffectType.TargetAdjustmentsAir:
                    VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/WhirlwindVFX");
                    break;
                default:
                    VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Spells/SpellTargetToxicAttack");
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

            object caller = AbilityUnitOwner != null?AbilityUnitOwner:(object)BoardSpell;

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
                        BattleController.AttackUnitByAbility(caller, AbilityData, leftAdjustment);
                    },
                    leftAdjustment.Transform.position);
            }

            if (rightAdjastment != null)
            {
                // cardCaller.FightCreatureBySkill(value, rightAdjastment.card);
                // CreateAndMoveParticle(() => playerCallerOfAbility.FightCreatureBySkill(value, rightAdjastment.card), rightAdjastment.transform.position);
                CreateAndMoveParticle(
                    () =>
                    {
                        BattleController.AttackUnitByAbility(caller, AbilityData, rightAdjastment);
                    },
                    rightAdjastment.Transform.position);
            }
        }

        protected override void OnInputEndEventHandler()
        {
            base.OnInputEndEventHandler();

            object caller = AbilityUnitOwner != null?AbilityUnitOwner:(object)BoardSpell;

            if (IsAbilityResolved)
            {
                switch (AffectObjectType)
                {
                    /*      case Enumerators.AffectObjectType.PLAYER:
                              //if (targetPlayer.playerInfo.netId == playerCallerOfAbility.netId)
                              //    CreateAndMoveParticle(() => playerCallerOfAbility.FightPlayerBySkill(value, false), targetPlayer.transform.position);
                              //else
                              //    CreateAndMoveParticle(() => playerCallerOfAbility.FightPlayerBySkill(value), targetPlayer.transform.position);
                              CreateAndMoveParticle(() => _battleController.AttackPlayerByAbility(caller, abilityData, targetPlayer), targetPlayer.AvatarObject.transform.position);
                              break; */
                    case Enumerators.AffectObjectType.Character:
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

        protected override void UnitOnAttackEventHandler(object info, int damage, bool isAttacker)
        {
            base.UnitOnAttackEventHandler(info, damage, isAttacker);

            if ((AbilityCallType != Enumerators.AbilityCallType.Attack) || !isAttacker)

                return;

            Action(info);
        }

        private void CreateAndMoveParticle(Action callback, Vector3 targetPosition)
        {
            Vector3 startPosition = CardKind == Enumerators.CardKind.Creature?AbilityUnitOwner.Transform.position:SelectedPlayer.Transform.position;
            if (AbilityCallType != Enumerators.AbilityCallType.Attack)
            {
                // CreateVFX(cardCaller.transform.position);
                GameObject particleMain = Object.Instantiate(VfxObject);
                particleMain.transform.position = Utilites.CastVfxPosition(startPosition + Vector3.forward);
                particleMain.transform.DOMove(Utilites.CastVfxPosition(targetPosition), 0.5f).OnComplete(
                    () =>
                    {
                        callback();
                        if (AbilityEffectType == Enumerators.AbilityEffectType.TargetAdjustmentsBomb)
                        {
                            DestroyParticle(particleMain, true);
                            GameObject prefab = LoadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/toxicDamageVFX");
                            GameObject particle = Object.Instantiate(prefab);
                            particle.transform.position = Utilites.CastVfxPosition(targetPosition + Vector3.forward);
                            ParticlesController.RegisterParticleSystem(particle, true);

                            SoundManager.PlaySound(Enumerators.SoundType.Spells, "NailBomb", Constants.SpellAbilitySoundVolume, Enumerators.CardSoundType.None);
                        }
                        else if (AbilityEffectType == Enumerators.AbilityEffectType.TargetAdjustmentsAir)
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
                DestroyParticle(new object[] { particleObj });
            }
            else
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
