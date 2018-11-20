using DG.Tweening;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Helpers;
using System.Collections.Generic;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class DevourZombiesAndCombineStatsAbilityView : AbilityViewBase<DevourZombiesAndCombineStatsAbility>
    {
        private BattlegroundController _battlegroundController;

        private List<BoardUnitModel> _units;

        private string _cardName;

        public DevourZombiesAndCombineStatsAbilityView(DevourZombiesAndCombineStatsAbility ability) : base(ability)
        {
            _battlegroundController = GameplayManager.GetController<BattlegroundController>();

            _units = new List<BoardUnitModel>();
        }

        protected override void OnAbilityAction(object info = null)
        {
            float delayAfter = 0;
            string soundName = string.Empty;
            _cardName = "";
            float delayBeforeDestroy = 3f;

            if (Ability.AbilityData.HasVisualEffectType(Enumerators.VisualEffectType.Impact))
            {
                float delayBeforeScaling = 0f;

                _units = info as List<BoardUnitModel>;
               
                Transform container = LoadObjectsManager.GetObjectByPath<GameObject>(Ability.AbilityData.GetVisualEffectByType(Enumerators.VisualEffectType.Impact).Path).transform;

                AbilityEffectInfoView effectInfo = container.GetComponent<AbilityEffectInfoView>();
                if (effectInfo != null)
                {
                    _cardName = effectInfo.cardName;
                    delayAfter = effectInfo.delayAfterEffect;
                    delayBeforeDestroy = effectInfo.delayBeforeEffect;
                    delayBeforeScaling = effectInfo.delayChangeState;
                    soundName = effectInfo.soundName;
                }

                GameObject[] prefabs = new GameObject[container.childCount];
                for (int i = 0; i < prefabs.Length; i++)
                {
                    prefabs[i] = container.GetChild(i).gameObject;
                }

                int random;
                Transform unitTransform = null;

                foreach (var unit in _units)
                {
                    unitTransform = _battlegroundController.GetBoardUnitViewByModel(unit).Transform;

                    random = UnityEngine.Random.Range(0, prefabs.Length);
                    VfxObject = Object.Instantiate(VfxObject, unitTransform, false);
                    VfxObject.transform.localPosition = Vector3.zero;
                    InternalTools.DoActionDelayed(() =>
                    {
                        unitTransform.DOScale(Vector3.zero, 1.5f).OnComplete(() =>
                        {
                            unitTransform.gameObject.SetActive(false);
                        });
                    }, delayAfter);
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
