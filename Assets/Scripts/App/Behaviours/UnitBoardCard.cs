// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/


using LoomNetwork.CZB.Common;
using System;
using TMPro;
using UnityEngine;

namespace LoomNetwork.CZB
{
    public class UnitBoardCard : BoardCard
    {
        public event Action<int, int> HealthChangedEvent;
        public event Action<int, int> DamageChangedEvent;

        protected TextMeshPro attackText;
        protected SpriteRenderer typeSprite;
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

        public UnitBoardCard(GameObject selfObject) : base(selfObject)
        {
            attackText = selfObject.transform.Find("AttackText").GetComponent<TextMeshPro>();
            defenseText = selfObject.transform.Find("DeffensText").GetComponent<TextMeshPro>();
            typeSprite = selfObject.transform.Find("TypeIcon").GetComponent<SpriteRenderer>();
        }

        public override void Init(WorkingCard card, string setName)
        {
            base.Init(card, setName);

            Damage = card.libraryCard.damage;
            initialDamage = card.libraryCard.damage;

            Health = card.libraryCard.health;
            initialHealth = card.libraryCard.health;

            attackText.text = Damage.ToString();
            defenseText.text = Health.ToString();

            typeSprite.sprite = _loadObjectsManager.GetObjectByPath<Sprite>(string.Format("Images/{0}", (Enumerators.CardType)card.type + "_icon"));

            DamageChangedEvent += (oldValue, newValue) => { attackText.text = newValue.ToString(); };
            HealthChangedEvent += (oldValue, newValue) => { defenseText.text = newValue.ToString(); };
        }

        public override void Init(Data.Card card, string setName = "", int amount = 0)
        {
            base.Init(card, setName, amount);

            attackText.text = card.damage.ToString();
            defenseText.text = card.health.ToString();

            typeSprite.sprite = _loadObjectsManager.GetObjectByPath<Sprite>(string.Format("Images/{0}", card.type + "_icon"));
        }
    }
}