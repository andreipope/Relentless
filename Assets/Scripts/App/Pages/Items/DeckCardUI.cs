using System;
using System.Collections.Generic;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Loom.ZombieBattleground
{
    public class DeckCardUI
    {
        private GameObject _selfObject;

        private Image _creatureCardImage;
        private Image _frameEffectImage;

        private Image _setImage;

        private TextMeshProUGUI _gooAmountText;
        private TextMeshProUGUI _creatureNameText;
        private ILoadObjectsManager _loadObjectsManager;

        private Card _card;
        private DeckId _selectedDeckId;

        public void Init(GameObject obj)
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();

            _selfObject = obj;

            _creatureNameText = _selfObject.transform.Find("Frame/CreatureName").GetComponent<TextMeshProUGUI>();
            _gooAmountText = _selfObject.transform.Find("Frame/GooBottle/GooAmount").GetComponent<TextMeshProUGUI>();

            _creatureCardImage = _selfObject.transform.Find("Frame/Viewport/Picture").GetComponent<Image>();
            _frameEffectImage = _selfObject.transform.Find("Frame/Viewport/Frame_effect").GetComponent<Image>();

            _setImage = _selfObject.transform.Find("Frame/SetIconBg/SetIconImage").GetComponent<Image>();

            _card = null;
        }

        public void FillCard(Card card)
        {
            _card = card;

            _gooAmountText.text = _card.Cost.ToString();
            _creatureNameText.text = _card.Name;

            string imagePath = $"{Constants.PathToCardsIllustrations}{_card.Picture.ToLowerInvariant()}";
            _creatureCardImage.sprite = _loadObjectsManager.GetObjectByPath<Sprite>(imagePath);

            _frameEffectImage.color = GetFactionColor(_card.Faction);

            string setName = $"Images/IconsSet/set_icon_{_card.CardKey.Variant.ToString().ToLowerInvariant()}";
            _setImage.sprite = _loadObjectsManager.GetObjectByPath<Sprite>(setName);
        }

        public Card GetCard()
        {
            return _card;
        }

        public GameObject GetGameObject()
        {
            return _selfObject;
        }

        public IReadOnlyCard GetCardInterface()
        {
            return _card;
        }

        public void Dispose()
        {
            if (_selfObject != null)
            {
                Object.Destroy(_selfObject);
            }
        }

        private Color GetFactionColor(Enumerators.Faction faction)
        {
            Color color = Color.white;
            switch (faction)
            {
                case Enumerators.Faction.Undefined:
                    break;
                case Enumerators.Faction.FIRE:
                    return new Color(102f/255f, 21f/255f, 5f/255f, 255f/255f);
                case Enumerators.Faction.WATER:
                    return new Color(2f/255f, 86f/255f, 86f/255f, 255f/255f);
                case Enumerators.Faction.EARTH:
                    return new Color(55f/255f, 31f/255f, 12f/255f, 255f/255f);
                case Enumerators.Faction.AIR:
                    return new Color(35f/255f, 34f/255f, 60f/255f, 255f/255f);
                case Enumerators.Faction.LIFE:
                    return new Color(10f/255f, 68f/255f, 37f/255f, 255f/255f);
                case Enumerators.Faction.TOXIC:
                    return new Color(75f/255f, 109f/255f, 2f/255f, 255f/255f);
                case Enumerators.Faction.ITEM:
                    return new Color(71f/255f, 75f/255f, 67f/255f, 255f/255f);
                default:
                    throw new ArgumentOutOfRangeException(nameof(faction), faction, null);
            }

            return color;
        }
    }
}



