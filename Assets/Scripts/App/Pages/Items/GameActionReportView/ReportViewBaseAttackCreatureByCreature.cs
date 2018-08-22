// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

namespace LoomNetwork.CZB
{
    public class ReportViewBaseAttackCreatureByCreature : ReportViewBase
    {
        private BoardUnit _attackingCreature,
                              _attackedCreature;

        private int _attackingDamage,
                    _attackedDamage;

        private GameObject _attackingCreatureObj,
                           _attackedCreatureObj;

        public ReportViewBaseAttackCreatureByCreature(GameObject prefab, Transform parent, GameActionReport gameAction) : base(prefab, parent, gameAction) { }

        public override void SetInfo()
        {
            base.SetInfo();

            _attackingCreature = gameAction.parameters[0] as BoardUnit;
            _attackingDamage = (int)gameAction.parameters[1];
            _attackedCreature = gameAction.parameters[2] as BoardUnit;
            _attackedDamage = (int)gameAction.parameters[3];


            previewImage.sprite = _attackingCreature.sprite;
            _attackingCreatureObj = CreateCardPreview(_attackingCreature.Card, Vector3.zero);
            _attackedCreatureObj = CreateCardPreview(_attackedCreature.Card, Vector3.right * 6);

            attackingPictureObject.SetActive(true);

            GameObject attackViewPlayer = _attackedCreatureObj.transform.Find("AttackingHealth").gameObject;
            attackViewPlayer.SetActive(true);
            var damageText = attackViewPlayer.transform.Find("AttackText").GetComponent<TextMeshPro>();
            damageText.text = (-_attackingDamage).ToString();
            attackViewPlayer.transform.localPosition = -Vector3.up * 3;

            if (_attackedDamage > 0)
            {
                GameObject attackViewCreature = _attackingCreatureObj.transform.Find("AttackingHealth").gameObject;
                attackViewCreature.SetActive(true);
                var damageTextCreature = attackViewCreature.transform.Find("AttackText").GetComponent<TextMeshPro>();
                damageTextCreature.text = (-_attackedDamage).ToString();
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
