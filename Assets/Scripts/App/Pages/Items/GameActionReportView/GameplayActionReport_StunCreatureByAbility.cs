// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using LoomNetwork.CZB.Data;

namespace LoomNetwork.CZB
{
    public class GameplayActionReport_StunCreatureByAbility : ReportViewBase
    {
        private BoardUnit _abilityUnitOwner;
        private BoardUnit _abilityUsedOnUnit;

        private GameObject _stunnedUnitPreviewObject,
                           _abilityOwnerObject;

        public GameplayActionReport_StunCreatureByAbility(GameObject prefab, Transform parent, GameActionReport gameAction) : base(prefab, parent, gameAction) { }

        public override void SetInfo()
        {
            base.SetInfo();

            _abilityUnitOwner = gameAction.parameters[0] as BoardUnit;
            _abilityUsedOnUnit = gameAction.parameters[1] as BoardUnit;

            previewImage.sprite = _abilityUnitOwner.sprite;

            _abilityOwnerObject = CreateCardPreview(_abilityUnitOwner.Card, Vector3.zero);
            _stunnedUnitPreviewObject = CreateCardPreview(_abilityUsedOnUnit.Card, Vector3.zero);
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
