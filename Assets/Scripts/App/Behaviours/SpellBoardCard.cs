// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/


using UnityEngine;
using UnityEngine.Assertions;

using TMPro;

namespace LoomNetwork.CZB
{

    public class SpellBoardCard : BoardCard
    {

        [SerializeField]
        protected TextMeshPro attackText;

        [SerializeField]
        protected TextMeshPro defenseText;

        public int initialHealth,
                   initialDamage;

        public int health,
                   damage;

        public SpellBoardCard(GameObject selfObject) : base(selfObject)
        {
        }

        public override void Init(WorkingCard card, string setName)
        {
            base.Init(card, setName);

            if (card.libraryCard.damage == 0)
                attackText.gameObject.SetActive(false);
            else
            {
                damage = card.libraryCard.damage;
                initialDamage = card.libraryCard.damage;

                attackText.text = damage.ToString();

            //    attackStat.onValueChanged += (oldValue, newValue) => { attackText.text = attackStat.effectiveValue.ToString(); };
            }

            if (card.libraryCard.health == 0)
                defenseText.gameObject.SetActive(false);
            else
            {
                health = card.libraryCard.health;
                initialHealth = card.libraryCard.health;

                defenseText.text = health.ToString();

           //     defenseStat.onValueChanged += (oldValue, newValue) => { defenseText.text = defenseStat.effectiveValue.ToString(); };
            }
        }

        public override void Init(Data.Card card, string setName = "", int amount = 0)
        {
            base.Init(card, setName, amount);

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