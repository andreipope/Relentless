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

        private int _hp, _damage;

        private int _initialHp, _initialDamage;

        public UnitBoardCard(GameObject selfObject)
            : base(selfObject)
        {
            AttackText = selfObject.transform.Find("AttackText").GetComponent<TextMeshPro>();
            DefenseText = selfObject.transform.Find("DeffensText").GetComponent<TextMeshPro>();
            TypeSprite = selfObject.transform.Find("TypeIcon").GetComponent<SpriteRenderer>();
        }

        public event Action<int, int> HealthChangedEvent;

        public event Action<int, int> DamageChangedEvent;

        public int Health
        {
            get => _hp;
            set
            {
                int oldHp = _hp;
                _hp = Mathf.Clamp(value, 0, int.MaxValue);
                HealthChangedEvent?.Invoke(oldHp, _hp);
            }
        }

        public int Damage
        {
            get => _damage;
            set
            {
                int oldDamage = _damage;
                _damage = Mathf.Clamp(value, 0, int.MaxValue);
                DamageChangedEvent?.Invoke(oldDamage, _damage);
            }
        }

        public override void Init(BoardUnitModel boardUnitModel)
        {
            base.Init(boardUnitModel);

            Damage = boardUnitModel.Card.InstanceCard.Damage;
            Health = boardUnitModel.Card.InstanceCard.Health;

            _initialDamage = Damage;
            _initialHp = Health;

            DrawStats();

            TypeSprite.sprite =
                LoadObjectsManager.GetObjectByPath<Sprite>(string.Format("Images/IconsSmallUnitTypes/{0}", boardUnitModel.Card.InstanceCard.CardType + "_icon"));

            DamageChangedEvent += (oldValue, newValue) =>
            {
                DrawStats();
            };
            HealthChangedEvent += (oldValue, newValue) =>
            {
                DrawStats();
            };
        }

        public override void Init(IReadOnlyCard card, int amount = 0)
        {
            base.Init(card, amount);

            Damage = card.Damage;
            Health = card.Health;

            _initialDamage = Damage;
            _initialHp = Health;

            DrawStats();

            TypeSprite.sprite =
                LoadObjectsManager.GetObjectByPath<Sprite>(string.Format("Images/{0}", card.CardType + "_icon"));
        }

        private void DrawStats()
        {
            AttackText.text = Damage.ToString();
            DefenseText.text = Health.ToString();

            FillColor(Damage, _initialDamage, AttackText);
            FillColor(Health, _initialHp, DefenseText);
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
