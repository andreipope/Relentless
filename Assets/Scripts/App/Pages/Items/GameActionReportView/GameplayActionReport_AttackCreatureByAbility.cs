using System;
using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace LoomNetwork.CZB
{
    public class GameplayActionReport_AttackCreatureByAbility : ReportViewBase
    {
        private object _abilityOwner;

        private AbilityData _usedAbility;

        private int _abilityValue;

        private BoardUnit _abilityUsedOnUnit;

        private GameObject _attackingCreatureObj, _attackedCreatureObj;

        public GameplayActionReport_AttackCreatureByAbility(GameObject prefab, Transform parent, GameActionReport gameAction)
            : base(prefab, parent, gameAction)
        {
        }

        public override void SetInfo()
        {
            base.SetInfo();

            _abilityOwner = gameAction.parameters[0];
            _usedAbility = gameAction.parameters[1] as AbilityData;
            _abilityValue = (int)gameAction.parameters[2];
            _abilityUsedOnUnit = gameAction.parameters[3] as BoardUnit;

            if (_abilityOwner is BoardUnit)
            {
                previewImage.sprite = (_abilityOwner as BoardUnit).sprite;
                _attackingCreatureObj = CreateCardPreview((_abilityOwner as BoardUnit).Card, Vector3.zero);
            } else
            {
                string rarity = Enum.GetName(typeof(Enumerators.CardRank), (_abilityOwner as BoardSpell).Card.libraryCard.cardRank);
                string cardSetName = cardsController.GetSetOfCard((_abilityOwner as BoardSpell).Card.libraryCard);
                previewImage.sprite = loadObjectsManager.GetObjectByPath<Sprite>(string.Format("Images/Cards/Illustrations/{0}_{1}_{2}", cardSetName.ToLower(), rarity.ToLower(), (_abilityOwner as BoardSpell).Card.libraryCard.picture.ToLower()));
                _attackingCreatureObj = CreateCardPreview((_abilityOwner as BoardSpell).Card, Vector3.zero);
            }

            attackingPictureObject.SetActive(true);

            _attackedCreatureObj = CreateCardPreview(_abilityUsedOnUnit.Card, Vector3.right * 6);

            GameObject attackViewPlayer = _attackedCreatureObj.transform.Find("AttackingHealth").gameObject;
            attackViewPlayer.SetActive(true);
            TextMeshPro damageText = attackViewPlayer.transform.Find("AttackText").GetComponent<TextMeshPro>();
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
