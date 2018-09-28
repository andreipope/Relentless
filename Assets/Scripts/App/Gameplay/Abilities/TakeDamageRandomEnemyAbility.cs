using System.Collections.Generic;
using DG.Tweening;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.Helpers;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class TakeDamageRandomEnemyAbility : AbilityBase
    {
        public int Value { get; }

        private List<object> _targets = new List<object>();

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

            _targets.AddRange(GetOpponentOverlord().BoardCards);
            _targets.Add(GetOpponentOverlord());

            _targets = InternalTools.GetRandomElementsFromList(_targets, 1);

            // lets improve this when it will be possible ofr the VFX that it can be used more accurate for different cards!
            if (AbilityUnitViewOwner != null && AbilityUnitViewOwner.Model.Card.LibraryCard.Name == "Zpitter")
            {
                VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/PoisonDartVFX");
            }
            else
            {
                VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/PoisonDartVFX");
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
                    case BoardUnitView unit:
                        targetPosition = unit.Transform.position;
                        break;
                }

                VfxObject = Object.Instantiate(VfxObject);
                VfxObject.transform.position = Utilites.CastVfxPosition(AbilityUnitViewOwner.Transform.position);
                targetPosition = Utilites.CastVfxPosition(targetPosition);
                VfxObject.transform.DOMove(targetPosition, 0.5f).OnComplete(() => { ActionCompleted(targetObject, targetPosition); });
                ParticleIds.Add(ParticlesController.RegisterParticleSystem(VfxObject));
            }
        }


        private void ActionCompleted(object target, Vector3 targetPosition)
        {
            ClearParticles();

            GameObject vfxObject = null;

            // lets improve this when it will be possible ofr the VFX that it can be used more accurate for different cards!
            if (AbilityUnitViewOwner != null && AbilityUnitViewOwner.Model.Card.LibraryCard.Name == "Zpitter")
            {
                vfxObject = LoadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Skills/PoisonDart_ImpactVFX");
            }
            else
            {
                vfxObject = null;
            }

            if (vfxObject != null)
            {
                vfxObject = Object.Instantiate(vfxObject);
                vfxObject.transform.position = targetPosition;
                ParticlesController.RegisterParticleSystem(vfxObject, true);
            }

            switch (target)
            {
                case Player allyPlayer:
                    BattleController.AttackPlayerByAbility(GetCaller(), AbilityData, allyPlayer);
                    break;
                case BoardUnitView allyUnit:
                    BattleController.AttackUnitByAbility(GetCaller(), AbilityData, allyUnit.Model);
                    break;
            }
        }
    }
}
