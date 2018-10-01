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

            BoardUnitModel unit = (BoardUnitModel) info;

            Player playerOwner = unit.OwnerPlayer;

            BoardUnitView leftAdjustment = null, rightAdjastment = null;

            int targetIndex = -1;
            List<BoardUnitView> list = null;
            for (int i = 0; i < playerOwner.BoardCards.Count; i++)
            {
                if (playerOwner.BoardCards[i].Model == unit)
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
                        BattleController.AttackUnitByAbility(caller, AbilityData, leftAdjustment.Model);
                    },
                    leftAdjustment.Transform.position);
            }

            if (rightAdjastment != null)
            {
                CreateAndMoveParticle(
                    () =>
                    {
                        BattleController.AttackUnitByAbility(caller, AbilityData, rightAdjastment.Model);
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
                            BattlegroundController.GetBoardUnitViewByModel(TargetUnit).Transform.position);

                        break;
                }
            }
        }

        protected override void UnitAttackedHandler(BoardObject info, int damage, bool isAttacker)
        {
            base.UnitAttackedHandler(info, damage, isAttacker);

            if (AbilityCallType != Enumerators.AbilityCallType.ATTACK || !isAttacker)
                return;

            Action(info);
        }

        private void CreateAndMoveParticle(Action callback, Vector3 targetPosition)
        {
            Vector3 startPosition = CardKind == Enumerators.CardKind.CREATURE ?
                GetAbilityUnitOwnerView().Transform.position :
                SelectedPlayer.Transform.position;

            if (AbilityCallType != Enumerators.AbilityCallType.ATTACK)
            {
                if (AbilityEffectType == Enumerators.AbilityEffectType.NONE ||
                    AbilityEffectType == Enumerators.AbilityEffectType.TARGET_ADJUSTMENTS_BOMB)
                {
                    #warning DELETE this line in future when on backend will be updated card library
                    callback();
                }
                else
                {
                    GameObject particleMain = Object.Instantiate(VfxObject);
                    particleMain.transform.position = Utilites.CastVfxPosition(startPosition + Vector3.forward);
                    particleMain.transform.DOMove(Utilites.CastVfxPosition(targetPosition), 0.5f).OnComplete(
                        () =>
                        {
                            callback();
                            if (AbilityEffectType == Enumerators.AbilityEffectType.TARGET_ADJUSTMENTS_AIR)
                            {
                                // one particle
                                ParticleSystem.MainModule main = particleMain.GetComponent<ParticleSystem>().main;
                                main.loop = false;
                            }

                            Object.Destroy(particleMain);
                        });
                }
            }
            else
            {
                 CreateVfx(Utilites.CastVfxPosition(BattlegroundController.GetBoardUnitViewByModel(TargetUnit).Transform.position));
                callback();
            }

            GameClient.Get<IGameplayManager>().RearrangeHands();
        }
    }
}
