using System;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using TMPro;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class GameplayActionReportAttackPlayerByAbility : ReportViewBase
    {
        private object _abilityOwner;

        private AbilityData _usedAbility;

        private int _abilityValue;

        private Player _abilityUsedOnPlayer;

        private GameObject _attackedPlayerObj, _attackingAbilityOwnerObj;

        public GameplayActionReportAttackPlayerByAbility(
            GameObject prefab, Transform parent, GameActionReport gameAction)
            : base(prefab, parent, gameAction)
        {
        }

        public override void SetInfo()
        {
            base.SetInfo();

            _abilityOwner = GameAction.Parameters[0];
            _usedAbility = GameAction.Parameters[1] as AbilityData;
            _abilityValue = (int) GameAction.Parameters[2];
            _abilityUsedOnPlayer = GameAction.Parameters[3] as Player;

            if (_abilityOwner is BoardUnit)
            {
                PreviewImage.sprite = (_abilityOwner as BoardUnit).Sprite;
                _attackingAbilityOwnerObj = CreateCardPreview((_abilityOwner as BoardUnit).Card, Vector3.zero);
            }
            else
            {
                string rarity = Enum.GetName(typeof(Enumerators.CardRank),
                    (_abilityOwner as BoardSpell).Card.LibraryCard.CardRank);
                string cardSetName = CardsController.GetSetOfCard((_abilityOwner as BoardSpell).Card.LibraryCard);
                PreviewImage.sprite = LoadObjectsManager.GetObjectByPath<Sprite>(
                    string.Format("Images/Cards/Illustrations/{0}_{1}_{2}", cardSetName.ToLower(), rarity.ToLower(),
                        (_abilityOwner as BoardSpell).Card.LibraryCard.Picture.ToLower()));
                _attackingAbilityOwnerObj = CreateCardPreview((_abilityOwner as BoardSpell).Card, Vector3.zero);
            }

            AttackingPictureObject.SetActive(true);

            _attackedPlayerObj = CreatePlayerPreview(_abilityUsedOnPlayer, Vector3.right * 6);

            GameObject cardView = _attackedPlayerObj.transform.Find("AttackingHealth").gameObject;
            cardView.SetActive(true);
            TextMeshPro damageText = cardView.transform.Find("AttackText").GetComponent<TextMeshPro>();
            damageText.text = (-_abilityValue).ToString();
            cardView.transform.localPosition = -Vector3.up;
        }
    }
}
