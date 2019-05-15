using DG.Tweening;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Helpers;
using System.Collections.Generic;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class DamageTargetOnCountItemsPlayedAbilityView : AbilityViewBase<DamageTargetOnCountItemsPlayedAbility>
    {
        private BattlegroundController _battlegroundController;

        private string _cardName;

        private List<IBoardObject> _targets;

        public DamageTargetOnCountItemsPlayedAbilityView(DamageTargetOnCountItemsPlayedAbility ability) : base(ability)
        {
            _battlegroundController = GameClient.Get<IGameplayManager>().GetController<BattlegroundController>();
        }

        protected override void OnAbilityAction(object info = null)
        {
            _targets = info as List<IBoardObject>;

            if (Ability.AbilityData.HasVisualEffectType(Enumerators.VisualEffectType.Moving))
            {
                Vector3 targetPosition = Vector3.zero;

                for(int i = 0; i < _targets.Count; i++)
                {
                    VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>(Ability.AbilityData.GetVisualEffectByType(Enumerators.VisualEffectType.Moving).Path);

                    switch (_targets[i])
                    {
                        case Player player:
                            targetPosition = player.AvatarObject.transform.position;
                            break;
                        case CardModel cardModel:
                            targetPosition = _battlegroundController.GetCardViewByModel<BoardUnitView>(cardModel).Transform.position;
                            break;
                    }

                    VfxObject = Object.Instantiate(VfxObject);
                    VfxObject.transform.position = _battlegroundController.GetCardViewByModel<BoardUnitView>(Ability.AbilityUnitOwner).Transform.position;
                    VfxObject.transform.DOMove(targetPosition, 0.5f).OnComplete(() =>
                    {
                        ActionCompleted(_targets[i], i == _targets.Count-1);
                    });
                    ParticleIds.Add(ParticlesController.RegisterParticleSystem(VfxObject));
                }
            }
            else
            {
                InternalTools.DoActionDelayed(Ability.InvokeVFXAnimationEnded);
            }
        }

        private void ActionCompleted(IBoardObject target, bool isFinal)
        {
            ClearParticles();

            soundClipTitle = string.Empty;
            float delayBefore = 0f;
            float delayAfter = 0;

            if (Ability.AbilityData.HasVisualEffectType(Enumerators.VisualEffectType.Impact))
            {
                Vector3 targetPosition = Vector3.zero;

                switch (target)
                {
                    case Player player:
                        targetPosition = player.AvatarObject.transform.position;
                        break;
                    case CardModel cardModel:
                        targetPosition = _battlegroundController.GetCardViewByModel<BoardUnitView>(cardModel).Transform.position;
                        break;
                }

                VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>(Ability.AbilityData.GetVisualEffectByType(Enumerators.VisualEffectType.Impact).Path);

                AbilityEffectInfoView effectInfo = VfxObject.GetComponent<AbilityEffectInfoView>();
                if (effectInfo != null)
                {
                    delayAfter = effectInfo.delayAfterEffect;
                    delayBefore = effectInfo.delayBeforeEffect;
                    soundClipTitle = effectInfo.soundName;
                    delayBeforeSound = effectInfo.delayForSound;
                }

                CreateVfx(targetPosition, true, delayBefore, true);

                PlaySound(soundClipTitle, delayBeforeSound);
            }

            if (isFinal)
            {
                InternalTools.DoActionDelayed(Ability.InvokeVFXAnimationEnded, delayAfter);
            }
        }


        protected override void CreateVfx(Vector3 pos, bool autoDestroy = false, float duration = 3, bool justPosition = false)
        {
            base.CreateVfx(pos, autoDestroy, duration, justPosition);
        }
    }
}
