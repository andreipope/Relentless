using DG.Tweening;
using Loom.ZombieBattleground.Common;
using System.Collections.Generic;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class UnitWeaponAbilityView : AbilityViewBase<UnitWeaponAbility>
    {
        private BattlegroundController _battlegroundController;
        private AbilitiesController _abilitiesController;

        private UnitWeaponAbility _unitWeaponAbility;

        private ulong _id;
        private ISoundManager _soundManager;

        public UnitWeaponAbilityView(UnitWeaponAbility ability) : base(ability)
        {
            _soundManager = GameClient.Get<ISoundManager>();
            _battlegroundController = GameplayManager.GetController<BattlegroundController>();
            _abilitiesController = GameplayManager.GetController<AbilitiesController>();
        }

        protected override void OnAbilityAction(object info = null)
        {
            ActionCompleted();

            Ability.TurnEndedEvent += TurnEndedEventHandler;
        }

        private void ActionCompleted()
        {
            ClearParticles();

            List<AbilityBase> unitAbilities = _abilitiesController.GetAbilitiesConnectedToUnit(Ability.TargetUnit);

            int count = unitAbilities.FindAll((x) => x.AbilityData.AbilityType == Ability.AbilityData.AbilityType).Count;

            if(count > 1)
            {
                Ability.InvokeVFXAnimationEnded();
                return;
            }

            if (Ability.AbilityData.HasVisualEffectType(Enumerators.VisualEffectType.Impact))
            {
                Enumerators.VisualEffectType effectType = Enumerators.VisualEffectType.Impact;

                switch (Ability.TargetUnit.InitialUnitType)
                {
                    case Enumerators.CardType.FERAL:
                        effectType = Enumerators.VisualEffectType.Impact_Feral;
                        break;
                    case Enumerators.CardType.HEAVY:
                        effectType = Enumerators.VisualEffectType.Impact_Heavy;
                        break;
                    default:
                        effectType = Enumerators.VisualEffectType.Impact;
                        break;
                }

                if (!Ability.AbilityData.HasVisualEffectType(effectType))
                {
                    effectType = Enumerators.VisualEffectType.Impact;
                }
                
                VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>(Ability.AbilityData.GetVisualEffectByType(effectType).Path);
                VfxObject = Object.Instantiate(VfxObject, _battlegroundController.GetBoardUnitViewByModel(Ability.TargetUnit).Transform, false);
                VfxObject.transform.localPosition = Vector3.forward * 3f;
                _id = ParticlesController.RegisterParticleSystem(VfxObject);
                ParticleIds.Add(_id);

                string soundName = VfxObject.GetComponent<AbilityImpactEffectInfo>().soundName;
                Enumerators.SoundType soundType = Enumerators.SoundType.CARDS;
                if (Ability.GetCaller() is BoardSpell)
                {
                    soundType = Enumerators.SoundType.SPELLS;
                }
                
                _soundManager.PlaySound(
                    soundType,
                    soundName,
                    Constants.ZombiesSoundVolume / 2f,
                    Enumerators.CardSoundType.NONE);
            }

            Ability.InvokeVFXAnimationEnded();
        }

        private void TurnEndedEventHandler()
        {
            ParticlesController.DestroyParticle(_id);

            Ability.TurnEndedEvent -= TurnEndedEventHandler;
        }


        protected override void CreateVfx(Vector3 pos, bool autoDestroy = false, float duration = 3, bool justPosition = false)
        {
            base.CreateVfx(pos, autoDestroy, duration, justPosition);
        }
    }
}
