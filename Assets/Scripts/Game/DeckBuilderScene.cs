// Copyright (C) 2016-2017 David Pol. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.Collections.Generic;
using System.IO;

using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

using FullSerializer;
using TMPro;

using CCGKit;

public class DeckBuilderScene : BaseScene
{
    public List<Transform> cardPositions;

    [SerializeField]
    private GameObject creatureCardViewPrefab;

    [SerializeField]
    private GameObject spellCardViewPrefab;

    [SerializeField]
    private GameObject deckListContent;

    [SerializeField]
    private GameObject deckListItemPrefab;

    [SerializeField]
    private GameObject deckListAddItemPrefab;

    [SerializeField]
    private TextMeshProUGUI pageText;

    [SerializeField]
    private TMP_InputField deckNameInputField;

    [SerializeField]
    private GameObject cardListContent;

    [SerializeField]
    private GameObject cardListItemPrefab;

    [SerializeField]
    private TextMeshProUGUI numCardsText;

    private GameObject createDeckItem;

    private DeckButton currentDeckButton;

    private fsSerializer serializer = new fsSerializer();

    private int numPages;
    private int currentPage;

    private void Awake()
    {
        Assert.IsNotNull(creatureCardViewPrefab);
        Assert.IsNotNull(spellCardViewPrefab);
        Assert.IsNotNull(deckListContent);
        Assert.IsNotNull(deckListItemPrefab);
        Assert.IsNotNull(deckListAddItemPrefab);
        Assert.IsNotNull(pageText);
        Assert.IsNotNull(deckNameInputField);
        Assert.IsNotNull(cardListContent);
        Assert.IsNotNull(cardListItemPrefab);
        Assert.IsNotNull(numCardsText);
    }

    private void Start()
    {
        createDeckItem = Instantiate(deckListAddItemPrefab) as GameObject;
        createDeckItem.transform.SetParent(deckListContent.transform, false);
        createDeckItem.GetComponent<CreateDeckButton>().scene = this;

        LoadCards(0);
        numPages = Mathf.CeilToInt(GameManager.Instance.config.GetNumCards() / (float)cardPositions.Count);
        pageText.text = "Page " + (currentPage + 1) + "/" + numPages;

        foreach (var deck in GameManager.Instance.playerDecks)
        {
            var go = Instantiate(deckListItemPrefab) as GameObject;
            go.transform.SetParent(deckListContent.transform, false);
            createDeckItem.transform.SetAsLastSibling();
            go.GetComponent<DeckButton>().scene = this;
            go.GetComponent<DeckButton>().SetDeck(deck);
        }

        var firstDeckButton = deckListContent.GetComponentInChildren<DeckButton>();
        if (firstDeckButton != null)
        {
            SetActiveDeck(firstDeckButton);
        }
    }

    public void OnBackButtonPressed()
    {
        SceneManager.LoadScene("Home");
    }

    public void OnCreateDeckButtonPressed()
    {
        CreateNewDeck();
    }

    public void CreateNewDeck()
    {
        var go = Instantiate(deckListItemPrefab) as GameObject;
        go.transform.SetParent(deckListContent.transform, false);
        createDeckItem.transform.SetAsLastSibling();
        go.GetComponent<DeckButton>().scene = this;

        var deckButton = go.GetComponent<DeckButton>();
        var deck = new Deck();
        GameManager.Instance.playerDecks.Add(deck);
        deckButton.SetDeck(deck);
        SetActiveDeck(deckButton);
    }

    public void OnDeckNameInputFieldEndedEdit()
    {
        currentDeckButton.SetDeckName(deckNameInputField.text);
    }

    public void SetActiveDeck(DeckButton button)
    {
        if (currentDeckButton != null)
        {
            currentDeckButton.SetActive(false);
        }
        currentDeckButton = button;
        currentDeckButton.SetActive(true);

        deckNameInputField.text = currentDeckButton.deck.name;

        foreach (var item in cardListContent.GetComponentsInChildren<CardListItem>())
        {
            Destroy(item.gameObject);
        }

        foreach (var card in currentDeckButton.deck.cards)
        {
            var libraryCard = GameManager.Instance.config.GetCard(card.id);
            var go = Instantiate(cardListItemPrefab) as GameObject;
            go.transform.SetParent(cardListContent.transform, false);
//            go.GetComponent<CardListItem>().deckButton = currentDeckButton;
            go.GetComponent<CardListItem>().card = libraryCard;
            go.GetComponent<CardListItem>().cardNameText.text = libraryCard.name;
            go.GetComponent<CardListItem>().cardAmountText.text = "x" + card.amount.ToString();
            go.GetComponent<CardListItem>().count = card.amount;
            var cost = libraryCard.costs.Find(x => x is PayResourceCost);
            if (cost != null)
            {
                var payResourceCost = cost as PayResourceCost;
                var manaCost = payResourceCost.value;
                go.GetComponent<CardListItem>().cardCostText.text = manaCost.ToString();
            }
        }

        UpdateNumCardsText();
    }

    public void OnPrevPageButtonPressed()
    {
        --currentPage;
        if (currentPage < 0)
        {
            currentPage = 0;
        }
        pageText.text = "Page " + (currentPage + 1) + "/" + numPages;
        LoadCards(currentPage);
    }

    public void OnNextPageButtonPressed()
    {
        ++currentPage;
        if (currentPage == numPages)
        {
            currentPage = numPages - 1;
        }
        pageText.text = "Page " + (currentPage + 1) + "/" + numPages;
        LoadCards(currentPage);
    }

    public void LoadCards(int page)
    {
        var gameConfig = GameManager.Instance.config;
        var startIndex = page * cardPositions.Count;
        var endIndex = Mathf.Min(startIndex + cardPositions.Count, gameConfig.GetNumCards());

        foreach (var card in FindObjectsOfType<CardView>())
        {
            Destroy(card.gameObject);
        }

        for (var i = startIndex; i < endIndex; i++)
        {
            var card = gameConfig.cards[i];
            var cardType = gameConfig.cardTypes.Find(x => x.id == card.cardTypeId);
            GameObject go = null;
            if (cardType.name == "Creature")
            {
                go = Instantiate(creatureCardViewPrefab as GameObject);
            }
            else if (cardType.name == "Spell")
            {
                go = Instantiate(spellCardViewPrefab as GameObject);
            }
            var cardView = go.GetComponent<CardView>();
            cardView.PopulateWithLibraryInfo(card);
            cardView.SetHighlightingEnabled(false);
            cardView.transform.position = cardPositions[i % cardPositions.Count].position;
            cardView.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
            cardView.GetComponent<SpriteRenderer>().sortingLayerName = "Default";
            cardView.GetComponent<SpriteRenderer>().sortingOrder = 1;
            cardView.GetComponent<SortingGroup>().sortingLayerName = "Default";
            cardView.GetComponent<SortingGroup>().sortingOrder = 1;

            var deckBuilderCard = go.AddComponent<DeckBuilderCard>();
         //   deckBuilderCard.scene = this;
            deckBuilderCard.card = card;
        }
    }

    public void AddCardToDeck(Card card)
    {
        if (currentDeckButton == null)
        {
            return;
        }

        var existingCards = currentDeckButton.deck.cards.Find(x => x.id == card.id);
        var maxCopies = card.GetIntProperty("MaxCopies");
        if (existingCards != null && existingCards.amount == maxCopies)
        {
            OpenPopup<PopupOneButton>("PopupOneButton", popup =>
            {
                popup.text.text = "You cannot have more than " + maxCopies + " copies of this card in your deck.";
                popup.buttonText.text = "OK";
                popup.button.onClickEvent.AddListener(() => { popup.Close(); });
            });
            return;
        }

        var itemFound = false;
        foreach (var item in cardListContent.GetComponentsInChildren<CardListItem>())
        {
            if (item.card == card)
            {
                itemFound = true;
                item.AddCard();
                break;
            }
        }

        if (!itemFound)
        {
            var go = Instantiate(cardListItemPrefab) as GameObject;
            go.transform.SetParent(cardListContent.transform, false);
       //     go.GetComponent<CardListItem>().deckButton = currentDeckButton;
            go.GetComponent<CardListItem>().card = card;
            go.GetComponent<CardListItem>().cardNameText.text = card.name;
            var cost = card.costs.Find(x => x is PayResourceCost);
            if (cost != null)
            {
                var payResourceCost = cost as PayResourceCost;
                var manaCost = payResourceCost.value;
                go.GetComponent<CardListItem>().cardCostText.text = manaCost.ToString();
            }
        }

        currentDeckButton.deck.AddCard(card);
        currentDeckButton.UpdateDeckInfo();
    }

    public void OnClearAllButtonPressed()
    {
        currentDeckButton.deck.cards.Clear();
        foreach (var item in cardListContent.GetComponentsInChildren<CardListItem>())
        {
            Destroy(item.gameObject);
        }
        currentDeckButton.UpdateDeckInfo();
    }

    public void UpdateNumCardsText()
    {
        if (currentDeckButton != null)
        {
            numCardsText.text = currentDeckButton.deck.GetNumCards().ToString();
        }
    }

    public void RemoveDeck(Deck deck)
    {
        GameManager.Instance.playerDecks.Remove(deck);
        foreach (var item in cardListContent.GetComponentsInChildren<CardListItem>())
        {
            Destroy(item.gameObject);
        }
    }

    public void OnDoneButtonPressed()
    {
        var config = GameManager.Instance.config;
        var minDeckSize = config.properties.minDeckSize;
        var maxDeckSize = config.properties.maxDeckSize;
        foreach (var deck in GameManager.Instance.playerDecks)
        {
            var numCards = deck.GetNumCards();
            if (numCards < minDeckSize)
            {
                OpenPopup<PopupOneButton>("PopupOneButton", popup =>
                {
                    popup.text.text = "Your '" + deck.name + "' deck has less than " + minDeckSize + " cards.";
                    popup.buttonText.text = "OK";
                    popup.button.onClickEvent.AddListener(() => { popup.Close(); });
                });
                return;
            }
            else if (numCards > maxDeckSize)
            {
                OpenPopup<PopupOneButton>("PopupOneButton", popup =>
                {
                    popup.text.text = "Your '" + deck.name + "' deck has more than " + maxDeckSize + " cards.";
                    popup.buttonText.text = "OK";
                    popup.button.onClickEvent.AddListener(() => { popup.Close(); });
                });
                return;
            }
        }

        var decksPath = Application.persistentDataPath + "/decks.json";
        Debug.Log(decksPath);
        fsData serializedData;
        serializer.TrySerialize(GameManager.Instance.playerDecks, out serializedData).AssertSuccessWithoutWarnings();
        var file = new StreamWriter(decksPath);
        var json = fsJsonPrinter.PrettyJson(serializedData);
        file.WriteLine(json);
        file.Close();
    }
}
