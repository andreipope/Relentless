using DG.Tweening;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Helpers;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class GainNumberOfLifeForEachDamageThisDealsAbilityView : AbilityViewBase<GainNumberOfLifeForEachDamageThisDealsAbility>
    {
        private BattlegroundController _battlegroundController;

        private string _cardName;

        public GainNumberOfLifeForEachDamageThisDealsAbilityView(GainNumberOfLifeForEachDamageThisDealsAbility ability) : base(ability)
        {
            _battlegroundController = GameClient.Get<IGameplayManager>().GetController<BattlegroundController>();
        }

        protected override void OnAbilityAction(object info = null)
        {
            ActionCompleted();
        }

        private void ActionCompleted()
        {
            _cardName = "";
            float delayAfter = 0;
            float delayBeforeDestroy = 5f;
            Vector3 offset = Vector3.zero;
            string soundName = string.Empty;

            ClearParticles();
            if (Ability.AbilityData.HasVisualEffectType(Enumerators.VisualEffectType.Impact))
            {
                Vector3 targetPosition = _battlegroundController.GetBoardUnitViewByModel(Ability.AbilityUnitOwner).Transform.position;

                VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>(Ability.AbilityData.GetVisualEffectByType(Enumerators.VisualEffectType.Impact).Path);

                AbilityEffectInfoView effectInfo = VfxObject.GetComponent<AbilityEffectInfoView>();
                if (effectInfo != null)
                {
                    _cardName = effectInfo.cardName;
                    delayAfter = effectInfo.delayAfterEffect;
                    delayBeforeDestroy = effectInfo.delayBeforeEffect;
                    offset = effectInfo.offset;
                    soundName = effectInfo.soundName;
                }

                CreateVfx(targetPosition + offset, true, delayBeforeDestroy);

                GameObject frameMaskObject = null;
                switch (Ability.AbilityUnitOwner.InitialUnitType)
                {
                    case Enumerators.CardType.WALKER:
                        frameMaskObject = VfxObject.transform.Find("WalkerMask").gameObject;
                        break;
                    case Enumerators.CardType.FERAL:
                        frameMaskObject = VfxObject.transform.Find("FeralMask").gameObject;
                        break;
                    case Enumerators.CardType.HEAVY:
                        frameMaskObject = VfxObject.transform.Find("HeavyMask").gameObject;
                        break;
                    default:
                        break;
                }

                if (frameMaskObject != null)
                {
                    frameMaskObject.SetActive(true);
                }
            }

            InternalTools.DoActionDelayed(Ability.InvokeVFXAnimationEnded, delayAfter);
        }


        protected override void CreateVfx(Vector3 pos, bool autoDestroy = false, float duration = 3, bool justPosition = false)
        {
            base.CreateVfx(pos, autoDestroy, duration, justPosition);
        }
    }
}
