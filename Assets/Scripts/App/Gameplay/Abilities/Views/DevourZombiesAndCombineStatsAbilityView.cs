using DG.Tweening;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Helpers;
using System.Collections.Generic;
using System.Linq;
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
            _cardName = "";
            float delayAfter = 0;
            float delayBeforeDestroy = 3f;
            string soundName = string.Empty;

            if (Ability.AbilityData.HasVisualEffectType(Enumerators.VisualEffectType.Impact))
            {
                _units = info as List<BoardUnitModel>;

                Transform container = LoadObjectsManager.GetObjectByPath<GameObject>(Ability.AbilityData.GetVisualEffectByType(Enumerators.VisualEffectType.Impact).Path).transform;

                AbilityEffectInfoView effectInfo = container.GetComponent<AbilityEffectInfoView>();
                if (effectInfo != null)
                {
                    _cardName = effectInfo.cardName;
                    delayAfter = effectInfo.delayAfterEffect;
                    delayBeforeDestroy = effectInfo.delayBeforeEffect;
                    soundName = effectInfo.soundName;
                }
                GameObject[] prefabs = new GameObject[container.childCount];
                for (int i = 0; i < prefabs.Length; i++)
                {
                    prefabs[i] = container.GetChild(i).gameObject;
                }

                int random;
                Transform unitTransform = null;

                foreach (BoardUnitModel unit in _units)
                {
                    if (unit == Ability.AbilityUnitOwner)
                        continue;

                    unitTransform = _battlegroundController.GetBoardUnitViewByModel<BoardUnitView>(unit).Transform;

                    random = UnityEngine.Random.Range(0, prefabs.Length);
                    VfxObject = Object.Instantiate(prefabs[random]);
                    VfxObject.transform.position = unitTransform.position;
                    unitTransform.SetParent(VfxObject.transform.Find("Container"), false);
                    unitTransform.localPosition = Vector3.zero;
                    ParticlesController.RegisterParticleSystem(VfxObject, true, delayBeforeDestroy);

                    List<GameObject> allUnitObj = unitTransform.GetComponentsInChildren<Transform>().Select(x => x.gameObject).ToList();
                    foreach (GameObject child in allUnitObj)
                    {
                        child.layer = 0;
                    }
                }
            }

            InternalTools.DoActionDelayed(Ability.InvokeVFXAnimationEnded, delayAfter);
        }

        private void ChangeUnitState(Transform unit, float delay, float duration)
        {
            InternalTools.DoActionDelayed(() =>
            {
                unit.DOScale(Vector3.zero, duration).SetEase(Ease.InSine).OnComplete(() =>
                {
                    unit.gameObject.SetActive(false);
                });
            }, delay);
        }

        protected override void CreateVfx(Vector3 pos, bool autoDestroy = false, float duration = 3, bool justPosition = false)
        {
            base.CreateVfx(pos, autoDestroy, duration, justPosition);
        }
    }
}
