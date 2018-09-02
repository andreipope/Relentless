// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/

using System;
using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace LoomNetwork.CZB
{
    public class GameplayActionReport_AttackPlayerByAbility : ReportViewBase
    {
        private object _abilityOwner;

        private AbilityData _usedAbility;

        private int _abilityValue;

        private Player _abilityUsedOnPlayer;

        private GameObject _attackedPlayerObj, _attackingAbilityOwnerObj;

        public GameplayActionReport_AttackPlayerByAbility(GameObject prefab, Transform parent, GameActionReport gameAction)
            : base(prefab, parent, gameAction)
        {
        }

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
                _attackingAbilityOwnerObj = CreateCardPreview((_abilityOwner as BoardUnit).Card, Vector3.zero);
            } else
            {
                string rarity = Enum.GetName(typeof(Enumerators.CardRank), (_abilityOwner as BoardSpell).Card.libraryCard.cardRank);
                string cardSetName = cardsController.GetSetOfCard((_abilityOwner as BoardSpell).Card.libraryCard);
                previewImage.sprite = loadObjectsManager.GetObjectByPath<Sprite>(string.Format("Images/Cards/Illustrations/{0}_{1}_{2}", cardSetName.ToLower(), rarity.ToLower(), (_abilityOwner as BoardSpell).Card.libraryCard.picture.ToLower()));
                _attackingAbilityOwnerObj = CreateCardPreview((_abilityOwner as BoardSpell).Card, Vector3.zero);
            }

            attackingPictureObject.SetActive(true);

            _attackedPlayerObj = CreatePlayerPreview(_abilityUsedOnPlayer, Vector3.right * 6);

            GameObject cardView = _attackedPlayerObj.transform.Find("AttackingHealth").gameObject;
            cardView.SetActive(true);
            TextMeshPro damageText = cardView.transform.Find("AttackText").GetComponent<TextMeshPro>();
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
