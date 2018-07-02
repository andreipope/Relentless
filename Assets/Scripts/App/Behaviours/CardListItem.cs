// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



using UnityEngine;
using TMPro;
using LoomNetwork.CZB.Data;
using System;
using System.Collections.Generic;
using UnityEngine.UI;

namespace LoomNetwork.CZB
{
    public class CardListItem
    {
        private ILoadObjectsManager _loadObjectsManager;

        private GameObject _selfObject;

        public Action<CardListItem, int> OnDeleteCard;

        public Deck deckButton;
        public Card card;
        public TextMeshProUGUI cardNameText;
        public TextMeshProUGUI cardCostText;
        public TextMeshProUGUI cardAmountText;

        public int count = 1;

        private Transform panelCardCount;

        private Image logoImage;

        private Button _deleteCardButton;

        private uint _maxCount;

        private List<CardInDeckAmountItem> _cardAmount;


        public CardListItem(GameObject selfObject)
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();

            _selfObject = selfObject;

            logoImage = _selfObject.transform.Find("Image_Logo").GetComponent<Image>();
            _deleteCardButton = _selfObject.transform.Find("DeleteButton").GetComponent<Button>();

            _deleteCardButton.onClick.AddListener(OnDeleteButtonPressed);
        }

        public void Init(Deck deck, Card card, int count, uint maxCount)
        {
            logoImage.sprite = _loadObjectsManager.GetObjectByPath<Sprite>("Images/CardsDeckEditingIcons/" + card.picture.ToLower());
           
            deckButton = deck;
            this.card = card;
            this.count = count;
            cardNameText.text = card.name;
            cardAmountText.text = "x" + count.ToString();
            _maxCount = maxCount;

            FillCardAmount();
        }

        public void AddCard()
        {
            UpdateCardsCount(1);
        }

        public void OnDeleteButtonPressed()
        {
            UpdateCardsCount(-1);
            OnDeleteCard?.Invoke(this, card.id);

            if (count > 0)
            {
                foreach (var deckEntry in deckButton.cards)
                {
                    if (deckEntry.cardId == card.id)
                    {
                        deckEntry.amount--;
                        break;
                    }
                }
            }
            else
            {
                deckButton.RemoveCard(card.id);
            }
        }

        public void UpdateCardsCount(int amount)
        {
            count += amount;
            cardAmountText.text = "x" + count.ToString();
            bool state = amount > 0 ? true : false;
            int index = amount > 0 ? count - 1 : count;
            _cardAmount[index].ChangeActivate(state);
        }

        public void Dispose()
        {
            MonoBehaviour.Destroy(_selfObject);
        }

        private void FillCardAmount()
        {
            panelCardCount = _selfObject.transform.Find("Panel_CardCount");

            _cardAmount = new List<CardInDeckAmountItem>();

            CardInDeckAmountItem item = null;
            for (int i = 0; i < _maxCount; i++)
            {
                item = new CardInDeckAmountItem(panelCardCount);
                if (i >= count)
                    item.ChangeActivate(false);
                _cardAmount.Add(item);
            }
        }
    }

    public class CardInDeckAmountItem
    {
        private ILoadObjectsManager _loadObjectsManager;

        private GameObject _selfObject;

        private GameObject _activateObject;

        public CardInDeckAmountItem(Transform parent)
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();

            _selfObject = MonoBehaviour.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Elements/CardInDeckCountPrefab"), parent, false);

            _activateObject = _selfObject.transform.Find("Image_Active").gameObject;
        }

        public void ChangeActivate(bool state)
        {
            _activateObject.SetActive(state);
        }

    }
}