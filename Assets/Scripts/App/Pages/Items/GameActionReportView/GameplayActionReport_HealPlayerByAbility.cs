// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using LoomNetwork.CZB.Data;
using System;
using LoomNetwork.CZB.Common;

namespace LoomNetwork.CZB
{
    public class GameplayActionReport_HealPlayerByAbility : ReportViewBase
    {
        private object _abilityOwner;
        private AbilityData _usedAbility;
        private int _abilityValue;
        private Player _abilityUsedOnPlayer;

        private GameObject _healCreatureObj,
                           _healedPlayerObj;

        public GameplayActionReport_HealPlayerByAbility(GameObject prefab, Transform parent, GameActionReport gameAction) : base(prefab, parent, gameAction) { }

        public override void SetInfo()
        {
            base.SetInfo();
            _abilityOwner = gameAction.parameters[0];
            _usedAbility = gameAction.parameters[1] as AbilityData;
            _abilityValue = (int)gameAction.parameters[2];
            _abilityUsedOnPlayer = gameAction.parameters[3] as Player;


            if (_abilityOwner is BoardUnit)
            {
                previewImage.sprite = (_abilityOwner as BoardUnit).sprite;
                _healCreatureObj = CreateCardPreview((_abilityOwner as BoardUnit).Card, Vector3.zero);
            }
            else if (_abilityOwner is BoardSpell)
            {
                var rarity = Enum.GetName(typeof(Enumerators.CardRank), (_abilityOwner as BoardSpell).Card.libraryCard.cardRank);
                string cardSetName = cardsController.GetSetOfCard((_abilityOwner as BoardSpell).Card.libraryCard);
                previewImage.sprite = loadObjectsManager.GetObjectByPath<Sprite>(string.Format("Images/Cards/Illustrations/{0}_{1}_{2}", cardSetName.ToLower(), rarity.ToLower(), (_abilityOwner as BoardSpell).Card.libraryCard.picture.ToLower()));
                _healCreatureObj = CreateCardPreview((_abilityOwner as BoardSpell).Card, Vector3.zero);
            }

            healPictureObject.SetActive(true);

            _healedPlayerObj = CreatePlayerPreview(_abilityUsedOnPlayer, Vector3.right * 6);

            GameObject attackViewPlayer = _healedPlayerObj.transform.Find("AttackingHealth").gameObject;
            attackViewPlayer.SetActive(true);
            var damageText = attackViewPlayer.transform.Find("AttackText").GetComponent<TextMeshPro>();
            damageText.text = _abilityValue.ToString();
            attackViewPlayer.transform.localPosition = Vector3.up * -0.86f;
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
