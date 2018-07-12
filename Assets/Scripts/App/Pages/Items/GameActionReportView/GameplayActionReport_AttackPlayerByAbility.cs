// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using LoomNetwork.CZB.Data;

namespace LoomNetwork.CZB
{
    public class GameplayActionReport_AttackPlayerByAbility : ReportViewBase
    {
        private BoardUnit _abilityUnitOwner;
        private AbilityData _usedAbility;
        private int _abilityValue;
        private Player _abilityUsedOnPlayer;

        private GameObject _attackedPlayerObj,
                           _attackingAbilityOwnerObj;

        public GameplayActionReport_AttackPlayerByAbility(GameObject prefab, Transform parent, GameActionReport gameAction) : base(prefab, parent, gameAction) { }

        public override void SetInfo()
        {
            base.SetInfo();

            _abilityUnitOwner = gameAction.parameters[0] as BoardUnit;
            _usedAbility = gameAction.parameters[1] as AbilityData;
            _abilityValue = (int)gameAction.parameters[2];
            _abilityUsedOnPlayer = gameAction.parameters[3] as Player;

            previewImage.sprite = _abilityUnitOwner.sprite;

            attackingPictureObject.SetActive(true);

            _attackingAbilityOwnerObj = CreateCardPreview(_abilityUnitOwner.Card, Vector3.zero);
            _attackedPlayerObj = CreatePlayerPreview(_abilityUsedOnPlayer, Vector3.right * 6);

            GameObject cardView = _attackedPlayerObj.transform.Find("AttackingHealth").gameObject;
            cardView.SetActive(true);
            var damageText = cardView.transform.Find("AttackText").GetComponent<TextMeshPro>();
            damageText.text = (-_abilityValue).ToString();
            cardView.transform.localPosition = -Vector3.up;
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
