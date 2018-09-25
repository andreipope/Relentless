using System;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using TMPro;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class GameplayActionReportAttackCreatureByAbility : ReportViewBase
    {
        private object _abilityOwner;

        private AbilityData _usedAbility;

        private int _abilityValue;

        private BoardUnitModel _abilityUsedOnUnitModel;

        private GameObject _attackingCreatureObj, _attackedCreatureObj;

        public GameplayActionReportAttackCreatureByAbility(
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
            _abilityUsedOnUnitModel = GameAction.Parameters[3] as BoardUnitModel;

            if (_abilityOwner is BoardUnitView)
            {
                PreviewImage.sprite = (_abilityOwner as BoardUnitView).Sprite;
                _attackingCreatureObj = CreateCardPreview((_abilityOwner as BoardUnitView).Model.Card, Vector3.zero);
            }
            else
            {
                string rarity = Enum.GetName(typeof(Enumerators.CardRank),
                    (_abilityOwner as BoardSpell).Card.LibraryCard.CardRank);
                string cardSetName = CardsController.GetSetOfCard((_abilityOwner as BoardSpell).Card.LibraryCard);
                PreviewImage.sprite = LoadObjectsManager.GetObjectByPath<Sprite>(
                    string.Format("Images/Cards/Illustrations/{0}_{1}_{2}", cardSetName.ToLower(), rarity.ToLower(),
                        (_abilityOwner as BoardSpell).Card.LibraryCard.Picture.ToLower()));
                _attackingCreatureObj = CreateCardPreview((_abilityOwner as BoardSpell).Card, Vector3.zero);
            }

            AttackingPictureObject.SetActive(true);

            _attackedCreatureObj = CreateCardPreview(_abilityUsedOnUnitModel.Card, Vector3.right * 6);

            GameObject attackViewPlayer = _attackedCreatureObj.transform.Find("AttackingHealth").gameObject;
            attackViewPlayer.SetActive(true);
            TextMeshPro damageText = attackViewPlayer.transform.Find("AttackText").GetComponent<TextMeshPro>();
            damageText.text = (-_abilityValue).ToString();
            attackViewPlayer.transform.localPosition = -Vector3.up * 3;
        }
    }
}
