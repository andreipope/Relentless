using System;
using System.Collections.Generic;
using Loom.ZombieBattleground.Common;
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

        private TextMeshProUGUI _gooAmountText;
        private TextMeshProUGUI _creatureNameText;

        private CardCountIndicator _cardCountIndicator;

        private ILoadObjectsManager _loadObjectsManager;

        public void Init(GameObject obj)
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();

            _selfObject = obj;

            _creatureNameText = _selfObject.transform.Find("Frame/CreatureName").GetComponent<TextMeshProUGUI>();
            _gooAmountText = _selfObject.transform.Find("Frame/GooBottle/GooAmount").GetComponent<TextMeshProUGUI>();

            _creatureCardImage = _selfObject.transform.Find("Frame/Viewport/Picture").GetComponent<Image>();
            _frameEffectImage = _selfObject.transform.Find("Frame/Viewport/Frame_effect").GetComponent<Image>();

            _cardCountIndicator = new CardCountIndicator();
            _cardCountIndicator.Init(_selfObject);
        }

        public void FillCard(DeckCardInfo deckCardInfo)
        {
            _gooAmountText.text = deckCardInfo.GooAmount.ToString();
            _creatureNameText.text = deckCardInfo.CreatureName;

            string imagePath = $"{Constants.PathToCardsIllustrations}{deckCardInfo.PicturePath.ToLowerInvariant()}";
            _creatureCardImage.sprite = _loadObjectsManager.GetObjectByPath<Sprite>(imagePath);

            _frameEffectImage.color = GetFactionColor(deckCardInfo.Faction);

            _cardCountIndicator.EnableIndicator(deckCardInfo.CardAmount);
        }

        public GameObject GetGameObject()
        {
            return _selfObject;
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


    public class CardCountIndicator
    {
        private ILoadObjectsManager _loadObjectsManager;

        private GameObject _indicatorPrefab;

        private const int MaxIndicatorCount = 4;

        private List<Button> _indicators;

        public void Init(GameObject obj)
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();

            _indicatorPrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Cards/Indicator");

            var meterTransform = obj.transform.Find("CardCount/Meter").transform;

            _indicators = new List<Button>();
            for (int i = 0; i < MaxIndicatorCount; i++)
            {
                var indicatorObj = Object.Instantiate(_indicatorPrefab, meterTransform);
                _indicators.Add(indicatorObj.GetComponent<Button>());
            }

            EnableIndicator(0);
        }

        public void EnableIndicator(int count)
        {
            for (int i = 0; i < MaxIndicatorCount; i++)
            {
                _indicators[i].interactable = i < count;
            }
        }
    }

    public class DeckCardInfo
    {
        public int GooAmount;
        public string CreatureName;
        public string PicturePath;
        public int CardAmount;
        public Enumerators.Faction Faction;
    }
}



