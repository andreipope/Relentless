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
        public int Damage { get; }

        public int Count { get; }

        public Enumerators.SetType SetType;

        public TakeDamageRandomEnemyAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Damage = ability.Damage;
            Count = ability.Count;
            SetType = ability.AbilitySetType;
        }

        public override void Activate()
        {
            base.Activate();

            if (AbilityCallType != Enumerators.AbilityCallType.ENTRY)
                return;

            Action();
        }

        protected override void TurnEndedHandler()
        {
            base.TurnEndedHandler();

            if (AbilityCallType != Enumerators.AbilityCallType.END)
                return;

            Action();
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

                foreach (Enumerators.AbilityTargetType abilityTarget in AbilityData.AbilityTargetTypes)
                {
                    switch (abilityTarget)
                    {
                        case Enumerators.AbilityTargetType.OPPONENT_ALL_CARDS:
                            _targets.AddRange(GetOpponentOverlord().BoardCards.Select(x => x.Model));
                            break;
                        case Enumerators.AbilityTargetType.PLAYER_ALL_CARDS:
                            _targets.AddRange(PlayerCallerOfAbility.BoardCards.Select(x => x.Model));
                            break;
                        case Enumerators.AbilityTargetType.PLAYER:
                            _targets.Add(PlayerCallerOfAbility);
                            break;
                        case Enumerators.AbilityTargetType.OPPONENT:
                            _targets.Add(GetOpponentOverlord());
                            break;
                    }
                }

                _targets = InternalTools.GetRandomElementsFromList(_targets, Count);
            }

            VfxObject = null;

            if (AbilityData.HasVisualEffectType(Enumerators.VisualEffectType.Moving))
            {
                VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>(AbilityData.GetVisualEffectByType(Enumerators.VisualEffectType.Moving).Path);
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
                        targetPosition = BattlegroundController.GetBoardUnitViewByModel(unit).Transform.position;
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

            if (AbilityData.HasVisualEffectType(Enumerators.VisualEffectType.Impact))
            {
                VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>(AbilityData.GetVisualEffectByType(Enumerators.VisualEffectType.Impact).Path);

                vfxObject = Object.Instantiate(vfxObject);
                vfxObject.transform.position = targetPosition;
                ParticlesController.RegisterParticleSystem(vfxObject, true);
            }

            int damageOverride = Damage;

            if (AbilityData.AbilitySubTrigger == Enumerators.AbilitySubTrigger.ForEachFactionOfUnitInHand)
            {
                damageOverride = PlayerCallerOfAbility.CardsInHand.FindAll(x => x.LibraryCard.CardSetType == SetType).Count;
            }

            switch (target)
            {
                case Player allyPlayer:
                    BattleController.AttackPlayerByAbility(GetCaller(), AbilityData, allyPlayer, damageOverride);
                    break;
                case BoardUnitModel allyUnit:
                    BattleController.AttackUnitByAbility(GetCaller(), AbilityData, allyUnit, damageOverride);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(target), target, null);
            }
        }
    }
}
