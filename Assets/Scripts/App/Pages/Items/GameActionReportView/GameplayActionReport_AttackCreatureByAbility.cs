// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using LoomNetwork.CZB.Data;

namespace LoomNetwork.CZB
{
    public class GameplayActionReport_AttackCreatureByAbility : ReportViewBase
    {
        private AbilityData _usedAbility;
        private int _abilityValue;
        private BoardUnit _abilityUsedOnUnit;
        private BoardUnit _abilityUnitOwner;

        private GameObject _attackingCreatureObj,
                           _attackedCreatureObj;

        public GameplayActionReport_AttackCreatureByAbility(GameObject prefab, Transform parent, GameActionReport gameAction) : base(prefab, parent, gameAction) { }

        public override void SetInfo()
        {
            base.SetInfo();

            _abilityUnitOwner = gameAction.parameters[0] as BoardUnit;
            _usedAbility = gameAction.parameters[1] as AbilityData;
            _abilityValue = (int)gameAction.parameters[2];
            _abilityUsedOnUnit = gameAction.parameters[3] as BoardUnit;

            previewImage.sprite = _abilityUnitOwner.sprite;

            attackingPictureObject.SetActive(true);

            _attackingCreatureObj = CreateCardPreview(_abilityUsedOnUnit.Card, Vector3.zero);
            _attackedCreatureObj = CreateCardPreview(_abilityUnitOwner.Card, Vector3.right * 6);

            GameObject attackViewPlayer = _attackedCreatureObj.transform.Find("AttackingHealth").gameObject;
            attackViewPlayer.SetActive(true);
            var damageText = attackViewPlayer.transform.Find("AttackText").GetComponent<TextMeshPro>();
            damageText.text = (-_abilityValue).ToString();
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
