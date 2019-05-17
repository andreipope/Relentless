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
                Vector3 targetPosition = _battlegroundController.GetCardViewByModel<BoardUnitView>(Ability.TargetUnit).Transform.position;

                VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>(Ability.AbilityData.GetVisualEffectByType(Enumerators.VisualEffectType.Moving).Path);

                VfxObject = Object.Instantiate(VfxObject);
                VfxObject.transform.position = _battlegroundController.GetCardViewByModel<BoardUnitView>(Ability.AbilityUnitOwner).Transform.position;
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
            Enumerators.AbilityEffectInfoPositionType positionType = Enumerators.AbilityEffectInfoPositionType.Target;


            if (Ability.AbilityData.HasVisualEffectType(Enumerators.VisualEffectType.Impact))
            {
                Vector3 targetPosition = Ability.PlayerCallerOfAbility.AvatarObject.transform.position;

                VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>(Ability.AbilityData.GetVisualEffectByType(Enumerators.VisualEffectType.Impact).Path);

                AbilityEffectInfoView effectInfo = VfxObject.GetComponent<AbilityEffectInfoView>();
                if (effectInfo != null)
                {
                    _cardName = effectInfo.cardName;
                    delayAfter = effectInfo.delayAfterEffect;
                    delayBeforeDestroy = effectInfo.delayBeforeEffect;
                    soundName = effectInfo.soundName;
                    delaySound = effectInfo.delayForSound;
                    positionType = effectInfo.positionInfo.type;
                }

                CreateVfx(targetPosition, true, delayBeforeDestroy, true);


                if(positionType == Enumerators.AbilityEffectInfoPositionType.Overlord && !Ability.PlayerCallerOfAbility.IsLocalPlayer)
                {
                    VfxObject.transform.eulerAngles = new Vector3(180, VfxObject.transform.eulerAngles.y, VfxObject.transform.eulerAngles.z);
                }
            }

            PlaySound(soundName, delaySound);

            InternalTools.DoActionDelayed(Ability.InvokeVFXAnimationEnded, delayAfter);
        }


        protected override void CreateVfx(Vector3 pos, bool autoDestroy = false, float duration = 3, bool justPosition = false)
        {
            base.CreateVfx(pos, autoDestroy, duration, justPosition);
        }
    }
}
