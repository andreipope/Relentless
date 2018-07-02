// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/


using UnityEngine;
using UnityEngine.Assertions;

using TMPro;
using LoomNetwork.CZB.Common;

namespace LoomNetwork.CZB
{
    public class UnitBoardCard : BoardCard
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

        public UnitBoardCard(GameObject selfObject) : base(selfObject)
        {
        }

        public override void Init(WorkingCard card, string setName)
        {
            base.Init(card, setName);

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

        public override void Init(Data.Card card, string setName = "", int amount = 0)
        {
            base.Init(card, setName, amount);

            attackText.text = card.damage.ToString();
            defenseText.text = card.health.ToString();

            typeSprite.sprite = Resources.Load<Sprite>(string.Format("Images/{0}", card.type + "_icon"));
        }
    }
}