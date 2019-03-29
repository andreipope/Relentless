using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Helpers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class ReplaceUnitsWithTypeOnStrongerOnesAbilityView : AbilityViewBase<ReplaceUnitsWithTypeOnStrongerOnesAbility>
    {
        private BattlegroundController _battlegroundController;

        private List<BoardUnitView> _boardUnits;
        private List<BoardUnitView> _boardReplaceUnits;


        public ReplaceUnitsWithTypeOnStrongerOnesAbilityView(ReplaceUnitsWithTypeOnStrongerOnesAbility ability) : base(ability)
        {
            _battlegroundController = GameplayManager.GetController<BattlegroundController>();
        }

        protected override void OnAbilityAction(object info = null)
        {
            List<BoardUnitView>[] units = info as List<BoardUnitView>[];
            _boardUnits = units[0];
            _boardReplaceUnits = units[1];

            float delayAfter = 0;
            float delaySound = 0;
            float delayChangeState = 0;
            string soundName = string.Empty;
            float delayBeforeDestroy = 3f;

            if (Ability.AbilityData.HasVisualEffectType(Enumerators.VisualEffectType.Impact))
            {
                Vector3 targetPosition = Vector3.zero;

                VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>(Ability.AbilityData.GetVisualEffectByType(Enumerators.VisualEffectType.Impact).Path);

                AbilityEffectInfoView effectInfo = VfxObject.GetComponent<AbilityEffectInfoView>();
                if (effectInfo != null)
                {
                    delayBeforeDestroy = effectInfo.delayBeforeEffect;
                    delayAfter = effectInfo.delayAfterEffect;
                    soundName = effectInfo.soundName;
                    delaySound = effectInfo.delayForSound;
                    delayChangeState = effectInfo.delayForChangeState;
                    targetPosition += Ability.PlayerCallerOfAbility.IsLocalPlayer ? effectInfo.localOffset : effectInfo.offset;
                }

                CreateVfx(targetPosition, true, delayBeforeDestroy, true);

                Transform oldUnitsContainer = VfxObject.transform.Find("ZB_ANM_Vortex_Suck/AllCards_Start");
                for (int i = 0; i < _boardUnits.Count; i++)
                {
                    ChangeUnitLayer(_boardUnits[i]);
                    _boardUnits[i].Transform.SetParent(GetNearestContainer(_boardUnits[i], oldUnitsContainer), true);
                    _boardUnits[i].Transform.eulerAngles = Vector3.zero;
                    _boardUnits[i].UnitContentObject.SetActive(false);
                }


                Transform replaceUnitsContainer = VfxObject.transform.Find("AllCards_Spawn");
                for (int i = 0; i < _boardReplaceUnits.Count; i++)
                {
                    _boardReplaceUnits[i].Transform.SetParent(GetNearestContainer(_boardReplaceUnits[i], replaceUnitsContainer), true);
                    _boardReplaceUnits[i].Transform.eulerAngles = Vector3.zero;
                }
                replaceUnitsContainer.gameObject.SetActive(false);

                InternalTools.DoActionDelayed(() =>
                {
                    replaceUnitsContainer.gameObject.SetActive(true);
                    oldUnitsContainer.gameObject.SetActive(false);
                }, delayChangeState);
            }

            PlaySound(soundName, 0);

            InternalTools.DoActionDelayed(RemoveParent, delayBeforeDestroy - 0.1f);

            InternalTools.DoActionDelayed(Ability.InvokeVFXAnimationEnded, delayAfter);
        }

        protected override void CreateVfx(Vector3 pos, bool autoDestroy = false, float duration = 3, bool justPosition = false)
        {
            base.CreateVfx(pos, autoDestroy, duration, justPosition);
        }

        private Transform GetNearestContainer(BoardUnitView unit, Transform baseContainer)
        {
            float distance = float.MaxValue;
            Transform container = null;
            for (int i = 0; i < baseContainer.childCount; i++)
            {
                if (distance > Vector3.Distance(unit.Transform.position, baseContainer.GetChild(i).position))
                {
                    distance = Vector3.Distance(unit.Transform.position, baseContainer.GetChild(i).position);
                    container = baseContainer.GetChild(i);
                }
            }
            return container;
        }

        private void RemoveParent()
        {
            foreach (BoardUnitView unit in _boardReplaceUnits)
            {
                unit.Transform.parent = null;
            }
        }

        private void ChangeUnitLayer(BoardUnitView unit)
        {
            foreach (GameObject child in unit.GameObject.GetComponentsInChildren<Transform>().Select(x => x.gameObject))
            {
                child.layer = SRLayers.Default;
            }
        }
    }
}
