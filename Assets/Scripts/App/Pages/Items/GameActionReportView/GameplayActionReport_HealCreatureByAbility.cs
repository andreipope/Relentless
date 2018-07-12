// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using LoomNetwork.CZB.Data;

namespace LoomNetwork.CZB
{
    public class GameplayActionReport_HealCreatureByAbility : ReportViewBase
    {
        private BoardUnit _abilityUnitOwner;
        private AbilityData _usedAbility;
        private int _abilityValue;
        private BoardUnit _abilityUsedOnUnit;

        private GameObject _healCreatureObj,
                           _healedCreatureObj;

        public GameplayActionReport_HealCreatureByAbility(GameObject prefab, Transform parent, GameActionReport gameAction) : base(prefab, parent, gameAction) { }

        public override void SetInfo()
        {
            base.SetInfo();

            _abilityUnitOwner = gameAction.parameters[0] as BoardUnit;
            _usedAbility = gameAction.parameters[1] as AbilityData;
            _abilityValue = (int)gameAction.parameters[2];
            _abilityUsedOnUnit = gameAction.parameters[3] as BoardUnit;

            previewImage.sprite = _abilityUnitOwner.sprite;

            healPictureObject.SetActive(true);

            _healCreatureObj = CreateCardPreview(_abilityUnitOwner.Card, Vector3.zero);
            _healedCreatureObj = CreateCardPreview(_abilityUsedOnUnit.Card, Vector3.right * 6);

            GameObject attackViewPlayer = _healedCreatureObj.transform.Find("AttackingHealth").gameObject;
            attackViewPlayer.SetActive(true);
            var damageText = attackViewPlayer.transform.Find("AttackText").GetComponent<TextMeshPro>();
            damageText.text = _abilityValue.ToString();
            attackViewPlayer.transform.localPosition = -Vector3.up * 3;
        }

        public override void OnPointerEnterEventHandler(PointerEventData obj)
        {
            base.OnPointerEnterEventHandler(obj);
        }

        public override void OnPointerExitEventHandler(PointerEventData obj)
        {
            base.OnPointerExitEventHandler(obj);
        }

        public override void Dispose()
        {
            base.Dispose();
        }

    }
}
