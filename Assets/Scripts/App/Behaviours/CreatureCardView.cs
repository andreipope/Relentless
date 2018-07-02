// Copyright (C) 2016-2017 David Pol. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;
using UnityEngine.Assertions;

using TMPro;
using GrandDevs.CZB.Common;

namespace GrandDevs.CZB
{
    public class CreatureCardView : CardView
    {
        [SerializeField]
        protected TextMeshPro attackText;

        [SerializeField]
        protected SpriteRenderer typeSprite;

        [SerializeField]
        protected TextMeshPro defenseText;

        public int initialHealth,
                   initialDamage;

        public int health,
                   damage;

        protected override void Awake()
        {
            base.Awake();
            Assert.IsNotNull(attackText);
            Assert.IsNotNull(defenseText);
        }

        public override void PopulateWithInfo(WorkingCard card, string setName)
        {
            base.PopulateWithInfo(card, setName);

            damage = card.libraryCard.damage;
            initialDamage = card.libraryCard.damage;

            health = card.libraryCard.health;
            initialHealth = card.libraryCard.health;

            attackText.text = damage.ToString();
            defenseText.text = health.ToString();

            typeSprite.sprite = Resources.Load<Sprite>(string.Format("Images/{0}", (Enumerators.CardType)card.type + "_icon"));

         //   attackStat.onValueChanged += (oldValue, newValue) => { attackText.text = attackStat.effectiveValue.ToString(); };
         //   defenseStat.onValueChanged += (oldValue, newValue) => { defenseText.text = defenseStat.effectiveValue.ToString(); };
        }

        public override void PopulateWithLibraryInfo(Data.Card card, string setName = "", int amount = 0)
        {
            base.PopulateWithLibraryInfo(card, setName, amount);

            attackText.text = card.damage.ToString();
            defenseText.text = card.health.ToString();

            typeSprite.sprite = Resources.Load<Sprite>(string.Format("Images/{0}", card.type + "_icon"));
        }
    }
}