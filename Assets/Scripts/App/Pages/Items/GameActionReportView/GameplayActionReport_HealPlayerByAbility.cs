using System;
using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;
using TMPro;
using UnityEngine;

namespace LoomNetwork.CZB
{
    public class GameplayActionReportHealPlayerByAbility : ReportViewBase
    {
        private object _abilityOwner;

        private AbilityData _usedAbility;

        private int _abilityValue;

        private Player _abilityUsedOnPlayer;

        private GameObject _healCreatureObj, _healedPlayerObj;

        public GameplayActionReportHealPlayerByAbility(GameObject prefab, Transform parent, GameActionReport gameAction)
            : base(prefab, parent, gameAction)
        {
        }

        public override void SetInfo()
        {
            base.SetInfo();
            _abilityOwner = GameAction.Parameters[0];
            _usedAbility = GameAction.Parameters[1] as AbilityData;
            _abilityValue = (int)GameAction.Parameters[2];
            _abilityUsedOnPlayer = GameAction.Parameters[3] as Player;

            switch (_abilityOwner)
            {
                case BoardUnit unit:
                    PreviewImage.sprite = unit.Sprite;
                    _healCreatureObj = CreateCardPreview(unit.Card, Vector3.zero);
                    break;
                case BoardSpell spell: {
                    string rarity = Enum.GetName(typeof(Enumerators.CardRank), spell.Card.LibraryCard.CardRank);
                    string cardSetName = CardsController.GetSetOfCard(spell.Card.LibraryCard);
                    PreviewImage.sprite = LoadObjectsManager.GetObjectByPath<Sprite>(string.Format("Images/Cards/Illustrations/{0}_{1}_{2}", cardSetName.ToLower(), rarity.ToLower(), spell.Card.LibraryCard.Picture.ToLower()));
                    _healCreatureObj = CreateCardPreview(spell.Card, Vector3.zero);
                    break;
                }
            }

            HealPictureObject.SetActive(true);

            _healedPlayerObj = CreatePlayerPreview(_abilityUsedOnPlayer, Vector3.right * 6);

            GameObject attackViewPlayer = _healedPlayerObj.transform.Find("AttackingHealth").gameObject;
            attackViewPlayer.SetActive(true);
            TextMeshPro damageText = attackViewPlayer.transform.Find("AttackText").GetComponent<TextMeshPro>();
            damageText.text = _abilityValue.ToString();
            attackViewPlayer.transform.localPosition = Vector3.up * -0.86f;
        }
    }
}
