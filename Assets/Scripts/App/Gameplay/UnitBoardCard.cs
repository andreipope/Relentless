using System;
using Loom.ZombieBattleground.Data;
using TMPro;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class UnitBoardCard : BoardCard
    {
        protected TextMeshPro AttackText;

        protected SpriteRenderer TypeSprite;

        protected TextMeshPro DefenseText;

        private int _hp, _damage;

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

        public override void Init(WorkingCard card)
        {
            base.Init(card);

            Damage = card.InstanceCard.Damage;
            Health = card.InstanceCard.Health;

            AttackText.text = Damage.ToString();
            DefenseText.text = Health.ToString();

            TypeSprite.sprite =
                LoadObjectsManager.GetObjectByPath<Sprite>(string.Format("Images/IconsSmallUnitTypes/{0}", card.InstanceCard.CardType + "_icon"));

            DamageChangedEvent += (oldValue, newValue) =>
            {
                AttackText.text = newValue.ToString();
            };
            HealthChangedEvent += (oldValue, newValue) =>
            {
                DefenseText.text = newValue.ToString();
            };
        }

        public override void Init(IReadOnlyCard card, int amount = 0)
        {
            base.Init(card, amount);

            AttackText.text = card.Damage.ToString();
            DefenseText.text = card.Health.ToString();

            TypeSprite.sprite =
                LoadObjectsManager.GetObjectByPath<Sprite>(string.Format("Images/{0}", card.CardType + "_icon"));
        }
    }
}
