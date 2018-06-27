// Copyright (C) 2016-2017 David Pol. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;
using UnityEngine.Assertions;

using TMPro;

namespace GrandDevs.CZB
{

    public class SpellCardView : CardView
    {

        [SerializeField]
        protected TextMeshPro attackText;

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

            if (card.libraryCard.damage == 0)
                attackText.gameObject.SetActive(false);
            else
            {
                damage = card.libraryCard.damage;
                initialDamage = card.libraryCard.damage;

                attackText.text = damage.ToString();

                attackStat.onValueChanged += (oldValue, newValue) => { attackText.text = attackStat.effectiveValue.ToString(); };
            }

            if (card.libraryCard.health == 0)
                defenseText.gameObject.SetActive(false);
            else
            {
                health = card.libraryCard.health;
                initialHealth = card.libraryCard.health;

                defenseText.text = health.ToString();

                defenseStat.onValueChanged += (oldValue, newValue) => { defenseText.text = defenseStat.effectiveValue.ToString(); };
            }
        }

        public override void PopulateWithLibraryInfo(Data.Card card, string setName = "", int amount = 0)
        {
            base.PopulateWithLibraryInfo(card, setName, amount);

            if (card.damage == 0)
                attackText.gameObject.SetActive(false);
            else
                attackText.text = card.damage.ToString();

            if (card.health == 0)
                defenseText.gameObject.SetActive(false);
            else
                defenseText.text = card.health.ToString();
        }
    }
}