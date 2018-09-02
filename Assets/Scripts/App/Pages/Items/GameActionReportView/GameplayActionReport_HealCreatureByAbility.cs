using System;
using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;
using TMPro;
using UnityEngine;

namespace LoomNetwork.CZB
{
    public class GameplayActionReportHealCreatureByAbility : ReportViewBase
    {
        private object _abilityOwner;

        private AbilityData _usedAbility;

        private int _abilityValue;

        private BoardUnit _abilityUsedOnUnit;

        private GameObject _healCreatureObj, _healedCreatureObj;

        public GameplayActionReportHealCreatureByAbility(GameObject prefab, Transform parent, GameActionReport gameAction)
            : base(prefab, parent, gameAction)
        {
        }

        public override void SetInfo()
        {
            base.SetInfo();

            _abilityOwner = GameAction.Parameters[0];
            _usedAbility = GameAction.Parameters[1] as AbilityData;
            _abilityValue = (int)GameAction.Parameters[2];
            _abilityUsedOnUnit = GameAction.Parameters[3] as BoardUnit;

            if (_abilityOwner is BoardUnit)
            {
                PreviewImage.sprite = (_abilityOwner as BoardUnit).Sprite;
                _healCreatureObj = CreateCardPreview((_abilityOwner as BoardUnit).Card, Vector3.zero);
            }
            else
            {
                string rarity = Enum.GetName(typeof(Enumerators.CardRank), (_abilityOwner as BoardSpell).Card.LibraryCard.CardRank);
                string cardSetName = CardsController.GetSetOfCard((_abilityOwner as BoardSpell).Card.LibraryCard);
                PreviewImage.sprite = LoadObjectsManager.GetObjectByPath<Sprite>(string.Format("Images/Cards/Illustrations/{0}_{1}_{2}", cardSetName.ToLower(), rarity.ToLower(), (_abilityOwner as BoardSpell).Card.LibraryCard.Picture.ToLower()));
                _healCreatureObj = CreateCardPreview((_abilityOwner as BoardSpell).Card, Vector3.zero);
            }

            HealPictureObject.SetActive(true);

            _healedCreatureObj = CreateCardPreview(_abilityUsedOnUnit.Card, Vector3.right * 6);

            GameObject attackViewPlayer = _healedCreatureObj.transform.Find("AttackingHealth").gameObject;
            attackViewPlayer.SetActive(true);
            TextMeshPro damageText = attackViewPlayer.transform.Find("AttackText").GetComponent<TextMeshPro>();
            damageText.text = _abilityValue.ToString();
            attackViewPlayer.transform.localPosition = -Vector3.up * 3;
        }
    }
}
