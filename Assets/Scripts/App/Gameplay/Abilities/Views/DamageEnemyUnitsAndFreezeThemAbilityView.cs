using DG.Tweening;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Helpers;
using System.Collections.Generic;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class DamageEnemyUnitsAndFreezeThemAbilityView : AbilityViewBase<DamageEnemyUnitsAndFreezeThemAbility>
    {
        private BattlegroundController _battlegroundController;

        private string _cardName;

        private List<IBoardObject> _targets;

        public DamageEnemyUnitsAndFreezeThemAbilityView(DamageEnemyUnitsAndFreezeThemAbility ability) : base(ability)
        {
            _battlegroundController = GameClient.Get<IGameplayManager>().GetController<BattlegroundController>();
        }

        protected override void OnAbilityAction(object info = null)
        {
            _targets = info as List<IBoardObject>;

            ActionCompleted();
        }

        private void ActionCompleted()
        {
            float delayAfter = 0;
            float delayBeforeDestroy = 5f;
            float delayChangeState = 0;
            Vector3 offset = Vector3.zero;
            string soundName = string.Empty;

            if (Ability.AbilityData.HasVisualEffectType(Enumerators.VisualEffectType.Impact))
            {
                Vector3 targetPosition = Vector3.zero;

                VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>(Ability.AbilityData.GetVisualEffectByType(Enumerators.VisualEffectType.Impact).Path);

                AbilityEffectInfoView effectInfo = VfxObject.GetComponent<AbilityEffectInfoView>();

                if (effectInfo != null)
                {
                    _cardName = effectInfo.cardName;
                    delayAfter = effectInfo.delayAfterEffect;
                    delayBeforeDestroy = effectInfo.delayBeforeEffect;
                    delayChangeState = effectInfo.delayForChangeState;
                    offset = effectInfo.offset;
                    soundName = effectInfo.soundName;
                }

                InternalTools.DoActionDelayed(() =>
                {
                    foreach (IBoardObject boardObject in _targets)
                    {
                        switch (boardObject)
                        {
                            case CardModel unit:
                                targetPosition = _battlegroundController.GetCardViewByModel<BoardUnitView>(unit).Transform.position;
                                break;
                            case Player player:
                                targetPosition = player.AvatarObject.transform.position;
                                break;
                        }

                        CreateVfx(targetPosition + offset, true, delayBeforeDestroy);
                    }
                }, delayChangeState);
            }
            InternalTools.DoActionDelayed(Ability.InvokeVFXAnimationEnded, delayAfter);
        }


        protected override void CreateVfx(Vector3 pos, bool autoDestroy = false, float duration = 3, bool justPosition = false)
        {
            base.CreateVfx(pos, autoDestroy, duration, justPosition);
        }
    }
}
