using System;
using Loom.ZombieBattleground.Data;
using TMPro;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class UnitBoardCard : BoardCardView
    {
        protected TextMeshPro AttackText;

        protected SpriteRenderer TypeSprite;

        protected TextMeshPro DefenseText;

        public UnitBoardCard(GameObject selfObject, BoardUnitModel boardUnitModel)
            : base(selfObject, boardUnitModel)
        {
            AttackText = selfObject.transform.Find("AttackText").GetComponent<TextMeshPro>();
            DefenseText = selfObject.transform.Find("DeffensText").GetComponent<TextMeshPro>();
            TypeSprite = selfObject.transform.Find("TypeIcon").GetComponent<SpriteRenderer>();

            DrawStats();

            TypeSprite.sprite =
                LoadObjectsManager.GetObjectByPath<Sprite>(string.Format("Images/IconsSmallUnitTypes/{0}", boardUnitModel.Card.InstanceCard.CardType + "_icon"));

            // TODO: refactor-state: unsubscribe
            BoardUnitModel.UnitDamageChanged += InstanceCardOnStatChanged;
            BoardUnitModel.UnitHpChanged += InstanceCardOnStatChanged;
        }

        private void InstanceCardOnStatChanged(int oldValue, int newValue)
        {
            DrawStats();
        }

        private void DrawStats()
        {
            AttackText.text = BoardUnitModel.Card.InstanceCard.Attack.ToString();
            DefenseText.text = BoardUnitModel.Card.InstanceCard.Defense.ToString();

            FillColor(BoardUnitModel.Card.InstanceCard.Attack, BoardUnitModel.Card.Prototype.Damage, AttackText);
            FillColor(BoardUnitModel.Card.InstanceCard.Defense, BoardUnitModel.Card.Prototype.Health, DefenseText);
        }

        private void FillColor(int stat, int initialStat, TextMeshPro text)
        {
            if (stat > initialStat)
            {
                text.color = Color.green;
            }
            else if (stat < initialStat)
            {
                text.color = Color.red;
            }
            else
            {
                text.color = Color.white;
            }
        }
    }
}
