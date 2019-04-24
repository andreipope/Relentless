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
            Model.UnitDamageChanged += InstanceCardOnStatChanged;
            Model.UnitDefenseChanged += InstanceCardOnStatChanged;
        }

        private void InstanceCardOnStatChanged(int oldValue, int newValue)
        {
            DrawStats();
        }

        private void DrawStats()
        {
            AttackText.text = Model.CurrentDamage.ToString();
            DefenseText.text = Model.CurrentDefense.ToString();

            FillColor(Model.CurrentDamage, Model.Card.Prototype.Damage, AttackText);
            FillColor(Model.CurrentDefense, Model.Card.Prototype.Defense, DefenseText);
        }

        public void DrawOriginalStats()
        {
            AttackText.text = Model.Card.Prototype.Damage.ToString();
            DefenseText.text = Model.Card.Prototype.Defense.ToString();

            FillColor(Model.Card.Prototype.Damage, Model.Card.Prototype.Damage, AttackText);
            FillColor(Model.Card.Prototype.Defense, Model.Card.Prototype.Defense, DefenseText);
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
