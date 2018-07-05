// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



using UnityEngine;
using UnityEngine.UI;
using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using System;
using System.Linq;
using UnityEngine.Rendering;
using UnityEngine.EventSystems;

namespace LoomNetwork.CZB
{
    public class ReportViewBaseAttackCreatureByCreature : ReportViewBase
    {
        private BoardUnit _attackingCreature,
                              _attackedCreature;

        private GameObject _attackingCreatureObj,
                           _attackedCreatureObj;

        public ReportViewBaseAttackCreatureByCreature(GameObject prefab, Transform parent, GameActionReport gameAction) : base(prefab, parent, gameAction) { }

        public override void SetInfo()
        {
            base.SetInfo();

            _attackingCreature = gameAction.parameters[0] as BoardUnit;
            _attackedCreature = gameAction.parameters[1] as BoardUnit;
            previewImage.sprite = _attackingCreature.sprite;
            _attackingCreatureObj = CreateCardPreview(_attackingCreature.Card, Vector3.zero, false);
            _attackedCreatureObj = CreateCardPreview(_attackedCreature.Card, Vector3.right * 6, false);


            GameObject attackViewPlayer = _attackedCreatureObj.transform.Find("AttackingHealth").gameObject;
            attackViewPlayer.SetActive(true);
            var damageText = attackViewPlayer.transform.Find("AttackText").GetComponent<TextMeshPro>();
            damageText.text = (-_attackingCreature.Damage).ToString();
            attackViewPlayer.transform.localPosition = -Vector3.up * 3;

            if (_attackedCreature.Damage > 0)
            {
                GameObject attackViewCreature = _attackingCreatureObj.transform.Find("AttackingHealth").gameObject;
                attackViewCreature.SetActive(true);
                var damageTextCreature = attackViewCreature.transform.Find("AttackText").GetComponent<TextMeshPro>();
                damageTextCreature.text = (-_attackedCreature.Damage).ToString();
                attackViewCreature.transform.localPosition = -Vector3.up * 3;

            }
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
