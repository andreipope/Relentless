using TMPro;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class UnitBoardCardView : BoardCardView
    {
        protected TextMeshPro AttackText;

        protected SpriteRenderer TypeSprite;

        protected TextMeshPro DefenseText;

        public UnitBoardCardView(GameObject selfObject, CardModel cardModel)
            : base(selfObject, cardModel)
        {
            AttackText = selfObject.transform.Find("AttackText").GetComponent<TextMeshPro>();
            DefenseText = selfObject.transform.Find("DeffensText").GetComponent<TextMeshPro>();
            TypeSprite = selfObject.transform.Find("TypeIcon").GetComponent<SpriteRenderer>();

            DrawStats();

            TypeSprite.sprite =
                LoadObjectsManager.GetObjectByPath<Sprite>(string.Format("Images/IconsSmallUnitTypes/{0}", cardModel.Card.InstanceCard.CardType + "_icon"));

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
            AttackText.text = Model.Card.InstanceCard.Damage.ToString();
            DefenseText.text = Model.Card.InstanceCard.Defense.ToString();

            FillColor(Model.Card.InstanceCard.Damage, Model.Card.Prototype.Damage, AttackText);
            FillColor(Model.Card.InstanceCard.Defense, Model.Card.Prototype.Defense, DefenseText);
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
