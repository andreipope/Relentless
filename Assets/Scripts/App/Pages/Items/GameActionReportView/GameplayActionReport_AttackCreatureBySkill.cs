// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/

using LoomNetwork.CZB.Data;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace LoomNetwork.CZB
{
    public class GameplayActionReport_AttackCreatureBySkill : ReportViewBase
    {
        private Player _callerPlayer;

        private HeroSkill _usedSkill;

        private int _skillValue;

        private BoardUnit _skillUsedOnUnit;

        private GameObject _attackedCreatureObj, _attackingPlayerObj;

        public GameplayActionReport_AttackCreatureBySkill(GameObject prefab, Transform parent, GameActionReport gameAction)
            : base(prefab, parent, gameAction)
        {
        }

        public override void SetInfo()
        {
            base.SetInfo();

            _callerPlayer = gameAction.parameters[0] as Player;
            _usedSkill = gameAction.parameters[1] as HeroSkill;
            _skillValue = (int)gameAction.parameters[2];
            _skillUsedOnUnit = gameAction.parameters[3] as BoardUnit;

            previewImage.sprite = loadObjectsManager.GetObjectByPath<Sprite>("Images/Heroes/CZB_2D_Hero_Portrait_" + _callerPlayer.SelfHero.heroElement + "_EXP");

            attackingPictureObject.SetActive(true);

            _attackedCreatureObj = CreateCardPreview(_skillUsedOnUnit.Card, Vector3.right * 6);
            _attackingPlayerObj = CreatePlayerPreview(_callerPlayer, Vector3.zero);

            GameObject cardView = _attackedCreatureObj.transform.Find("AttackingHealth").gameObject;
            cardView.SetActive(true);
            TextMeshPro damageText = cardView.transform.Find("AttackText").GetComponent<TextMeshPro>();
            damageText.text = (-_skillValue).ToString();
            cardView.transform.localPosition = -Vector3.up * 3;
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
