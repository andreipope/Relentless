// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/


using UnityEngine;
using TMPro;
using System;

namespace LoomNetwork.CZB
{
    public class SpellBoardCard : BoardCard
    {
        public event Action<int, int> HealthChangedEvent;
        public event Action<int, int> DamageChangedEvent;

        protected TextMeshPro attackText;
        protected TextMeshPro defenseText;

        private int _hp,
                    _damage;

        public int initialHealth,
                   initialDamage;

        public int Health
        {
            get
            {
                return _hp;
            }
            set
            {
                int oldHP = _hp;
                _hp = Mathf.Clamp(value, 0, int.MaxValue);
                HealthChangedEvent?.Invoke(oldHP, _hp);
            }
        }

        public int Damage
        {
            get
            {
                return _damage;
            }
            set
            {
                int _oldDamage = _damage;
                _damage = Mathf.Clamp(value, 0, int.MaxValue);
                DamageChangedEvent?.Invoke(_oldDamage, _damage);
            }
        }

        public SpellBoardCard(GameObject selfObject) : base(selfObject)
        {
            attackText = selfObject.transform.Find("AttackText").GetComponent<TextMeshPro>();
            defenseText = selfObject.transform.Find("DeffensText").GetComponent<TextMeshPro>();
        }

        public override void Init(WorkingCard card)
        {
            base.Init(card);

            if (card.libraryCard.damage == 0)
                attackText.gameObject.SetActive(false);
            else
            {
                Damage = card.libraryCard.damage;
                initialDamage = card.libraryCard.damage;

                attackText.text = Damage.ToString();

                DamageChangedEvent += (oldValue, newValue) => { attackText.text = newValue.ToString(); };
            }

            if (card.libraryCard.health == 0)
                defenseText.gameObject.SetActive(false);
            else
            {
                Health = card.libraryCard.health;
                initialHealth = card.libraryCard.health;

                defenseText.text = Health.ToString();

                HealthChangedEvent += (oldValue, newValue) => { defenseText.text = newValue.ToString(); };
            }
        }

        public override void Init(Data.Card card, int amount = 0)
        {
            base.Init(card, amount);

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