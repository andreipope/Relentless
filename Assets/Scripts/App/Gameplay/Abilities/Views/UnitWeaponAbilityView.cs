using DG.Tweening;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Helpers;
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

        private string _cardName;

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

            int count = unitAbilities.FindAll((x) => x.AbilityData.Ability == Ability.AbilityData.Ability).Count;

            if(count > 1)
            {
                Ability.InvokeVFXAnimationEnded();
                return;
            }

            float delayAfter = 0;
            string soundName = string.Empty;
            _cardName = "";
            float delayBeforeDestroy = 3f;
            float soundDelay = 0;

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

                Vector3 offset = Vector3.zero;

                VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>(Ability.AbilityData.GetVisualEffectByType(effectType).Path);

                VfxObject = Object.Instantiate(VfxObject, _battlegroundController.GetCardViewByModel<BoardUnitView>(Ability.TargetUnit).Transform, false);

                AbilityEffectInfoView effectInfo = VfxObject.GetComponent<AbilityEffectInfoView>();
                if (effectInfo != null)
                {
                    _cardName = effectInfo.cardName;
                    delayAfter = effectInfo.delayAfterEffect;
                    delayBeforeDestroy = effectInfo.delayBeforeEffect;
                    soundName = effectInfo.soundName;
                    offset = effectInfo.offset;
                    soundDelay = effectInfo.delayForSound;
                }

                VfxObject.transform.localPosition = offset;
                _id = ParticlesController.RegisterParticleSystem(VfxObject);
                ParticleIds.Add(_id);
            }

            switch (_cardName)
            {
                case "SuperSerum":
                    InternalTools.DoActionDelayed(() =>
                    {
                        ParticlesController.DestroyParticle(_id);

                    }, delayBeforeDestroy);
                    break;
                default:
                    break;
            }

            PlaySound(soundName, soundDelay);

            InternalTools.DoActionDelayed(Ability.InvokeVFXAnimationEnded, delayAfter);
        }

        private void TurnEndedEventHandler()
        {
            switch (_cardName)
            {
                case "Chainsaw":
                    ParticlesController.DestroyParticle(_id);
                    break;
                default:
                    break;
            }
            Ability.TurnEndedEvent -= TurnEndedEventHandler;
        }

        protected override void CreateVfx(Vector3 pos, bool autoDestroy = false, float duration = 3, bool justPosition = false)
        {
            base.CreateVfx(pos, autoDestroy, duration, justPosition);
        }
    }
}
