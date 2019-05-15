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
                Vector3 targetPosition = _battlegroundController.GetCardViewByModel<BoardUnitView>(Ability.AbilityUnitOwner).Transform.position;

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

                string objectName = "WalkerMask";
                switch (Ability.AbilityUnitOwner.InitialUnitType)
                {
                    case Enumerators.CardType.FERAL:
                        objectName = "FeralMask";
                        break;
                    case Enumerators.CardType.HEAVY:
                        objectName = "HeavyMask";
                        break;
                }
                VfxObject.transform.Find(objectName).gameObject.SetActive(true);
            }

            InternalTools.DoActionDelayed(Ability.InvokeVFXAnimationEnded, delayAfter);
        }


        protected override void CreateVfx(Vector3 pos, bool autoDestroy = false, float duration = 3, bool justPosition = false)
        {
            base.CreateVfx(pos, autoDestroy, duration, justPosition);
        }
    }
}
