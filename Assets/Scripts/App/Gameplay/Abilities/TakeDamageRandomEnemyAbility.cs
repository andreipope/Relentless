using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.Helpers;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Loom.ZombieBattleground
{
    public class TakeDamageRandomEnemyAbility : AbilityBase
    {
        public int Value { get; } = 1;

        public TakeDamageRandomEnemyAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Value = ability.Value;
        }

        public override void Activate()
        {
            base.Activate();

            if (AbilityCallType != Enumerators.AbilityCallType.ENTRY)
                return;

            Action(null);
        }

        public override void Action(object info = null)
        {
            base.Action(info);

            List<BoardObject> _targets;

            if (PredefinedTargets != null)
            {
                _targets = PredefinedTargets;
            }
            else
            {
                _targets = new List<BoardObject>();
                _targets.AddRange(GetOpponentOverlord().BoardCards.Select(x => x.Model));
                _targets.Add(GetOpponentOverlord());

                _targets = InternalTools.GetRandomElementsFromList(_targets, Value);
            }

            VfxObject = null;

            if (AbilityData.HasVFXType(Enumerators.VFXType.Moving))
            {
                VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>(AbilityData.GetVFXByType(Enumerators.VFXType.Moving).Path);
            }

            foreach (object target in _targets)
            {
                object targetObject = target;
                Vector3 targetPosition = Vector3.zero;

                switch (target)
                {
                    case Player player:
                        targetPosition = player.AvatarObject.transform.position;
                        break;
                    case BoardUnitModel unit:
                        targetPosition =  BattlegroundController.GetBoardUnitViewByModel(unit).Transform.position;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(target), target, null);
                }

                if (VfxObject != null)
                {
                    VfxObject = Object.Instantiate(VfxObject);
                    VfxObject.transform.position = Utilites.CastVfxPosition(BattlegroundController.GetBoardUnitViewByModel(AbilityUnitOwner).Transform.position);
                    targetPosition = Utilites.CastVfxPosition(targetPosition);
                    VfxObject.transform.DOMove(targetPosition, 0.5f).OnComplete(() => { ActionCompleted(targetObject, targetPosition); });
                    ParticleIds.Add(ParticlesController.RegisterParticleSystem(VfxObject));
                }
                else
                {
                    ActionCompleted(targetObject, targetPosition);
                }
            }

            AbilitiesController.ThrowUseAbilityEvent(MainWorkingCard, _targets, AbilityData.AbilityType, Protobuf.AffectObjectType.Character);
        }


    private void ActionCompleted(object target, Vector3 targetPosition)
        {
            ClearParticles();

            GameObject vfxObject = null;

            if (AbilityData.HasVFXType(Enumerators.VFXType.Impact))
            {
                VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>(AbilityData.GetVFXByType(Enumerators.VFXType.Impact).Path);

                vfxObject = Object.Instantiate(vfxObject);
                vfxObject.transform.position = targetPosition;
                ParticlesController.RegisterParticleSystem(vfxObject, true);
            }

            switch (target)
            {
                case Player allyPlayer:
                    BattleController.AttackPlayerByAbility(GetCaller(), AbilityData, allyPlayer);
                    break;
                case BoardUnitModel allyUnit:
                    BattleController.AttackUnitByAbility(GetCaller(), AbilityData, allyUnit);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(target), target, null);
            }
        }
    }
}
