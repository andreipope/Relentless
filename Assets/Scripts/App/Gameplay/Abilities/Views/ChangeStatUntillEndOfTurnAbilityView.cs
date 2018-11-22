using DG.Tweening;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Helpers;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class ChangeStatUntillEndOfTurnAbilityView : AbilityViewBase<ChangeStatUntillEndOfTurnAbility>
    {
        private BattlegroundController _battlegroundController;

        private string _cardName;

        public ChangeStatUntillEndOfTurnAbilityView(ChangeStatUntillEndOfTurnAbility ability) : base(ability)
        {
            _battlegroundController = GameClient.Get<IGameplayManager>().GetController<BattlegroundController>();
        }

        protected override void OnAbilityAction(object info = null)
        {
            if (Ability.AbilityData.HasVisualEffectType(Enumerators.VisualEffectType.Moving))
            {
                Vector3 targetPosition = _battlegroundController.GetBoardUnitViewByModel(Ability.TargetUnit).Transform.position;

                VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>(Ability.AbilityData.GetVisualEffectByType(Enumerators.VisualEffectType.Moving).Path);

                VfxObject = Object.Instantiate(VfxObject);
                VfxObject.transform.position = _battlegroundController.GetBoardUnitViewByModel(Ability.AbilityUnitOwner).Transform.position;
                VfxObject.transform.DOMove(targetPosition, 0.5f).OnComplete(ActionCompleted);
                ParticleIds.Add(ParticlesController.RegisterParticleSystem(VfxObject));
            }
            else
            {
                ActionCompleted();
            }
        }

        private void ActionCompleted()
        {
            ClearParticles();
            _cardName = "";
            float delayAfter = 0;
            float delayBeforeDestroy = 3f;
            float delaySound = 0;
            string soundName = string.Empty;

            Debug.LogError(1111);

            //if (Ability.AbilityData.HasVisualEffectType(Enumerators.VisualEffectType.Impact))
            {
                bool isOnlyForLocalPlayer = false;
                Vector3 rotation = Vector3.zero;

                Vector3 targetPosition = Ability.PlayerCallerOfAbility.AvatarObject.transform.position;

                VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/FreshMeatVFX");//(Ability.AbilityData.GetVisualEffectByType(Enumerators.VisualEffectType.Impact).Path);

                AbilityEffectInfoView effectInfo = VfxObject.GetComponent<AbilityEffectInfoView>();
                if (effectInfo != null)
                {
                    _cardName = effectInfo.cardName;
                    delayAfter = effectInfo.delayAfterEffect;
                    delayBeforeDestroy = effectInfo.delayBeforeEffect;
                    soundName = effectInfo.soundName;
                    delaySound = effectInfo.delayForSound;

                    isOnlyForLocalPlayer = effectInfo.rotationParameters.isOnlyForLocalPlayer;
                    rotation = effectInfo.rotationParameters.rotation;
                    Debug.LogError(2222);

                }

                CreateVfx(targetPosition, true, delayBeforeDestroy, true);
                Debug.LogError(VfxObject.name);


                if (isOnlyForLocalPlayer == Ability.PlayerCallerOfAbility.IsLocalPlayer)
                {
                    VfxObject.transform.eulerAngles = rotation;
                    Debug.LogError(3333);
                }
            }

            if (!string.IsNullOrEmpty(soundName))
            {
                InternalTools.DoActionDelayed(() =>
                {
                    SoundManager.PlaySound(Enumerators.SoundType.SPELLS, soundName, Constants.SfxSoundVolume, Enumerators.CardSoundType.NONE);
                }, delaySound);
            }

            InternalTools.DoActionDelayed(Ability.InvokeVFXAnimationEnded, delayAfter);
        }


        protected override void CreateVfx(Vector3 pos, bool autoDestroy = false, float duration = 3, bool justPosition = false)
        {
            base.CreateVfx(pos, autoDestroy, duration, justPosition);
        }
    }
}
