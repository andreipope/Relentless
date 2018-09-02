using System;
using DG.Tweening;
using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;
using LoomNetwork.Internal;
using UnityEngine;

namespace LoomNetwork.CZB
{
    public class HealTargetAbility : AbilityBase
    {
        public int value = 1;

        public HealTargetAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            value = ability.value;
        }

        public override void Activate()
        {
            base.Activate();

            _vfxObject = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/GreenHealVFX");
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

            object caller = abilityUnitOwner != null?abilityUnitOwner:(object)boardSpell;

            switch (affectObjectType)
            {
                case Enumerators.AffectObjectType.PLAYER:
                    _battleController.HealPlayerByAbility(caller, abilityData, targetPlayer);
                    break;
                case Enumerators.AffectObjectType.CHARACTER:
                    _battleController.HealUnitByAbility(caller, abilityData, targetUnit);
                    break;
            }
        }

        protected override void OnInputEndEventHandler()
        {
            base.OnInputEndEventHandler();

            if (_isAbilityResolved)
            {
                Action();
            }
        }

        private void CreateAndMoveParticle(Action callback, Vector3 target)
        {
            target = Utilites.CastVFXPosition(target);
            if (abilityEffectType == Enumerators.AbilityEffectType.HEAL)
            {
                Vector3 startPosition = cardKind == Enumerators.CardKind.CREATURE?abilityUnitOwner.transform.position:selectedPlayer.Transform.position;
                _vfxObject = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Spells/SpellTargetLifeAttack");

                CreateVFX(startPosition);
                _vfxObject.transform.DOMove(target, 0.5f).OnComplete(
                    () =>
                    {
                        ClearParticles();
                        _vfxObject = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/GreenHealVFX");

                        CreateVFX(target, true);
                        callback();
                    });
            } else if (abilityEffectType == Enumerators.AbilityEffectType.HEAL_DIRECTLY)
            {
                CreateVFX(target, true);
                callback();
            }
        }
    }
}
