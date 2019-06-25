

using System;
using System.Collections.Generic;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Loom.ZombieBattleground
{
    public class DeckInfoUI
    {
        private TextMeshProUGUI _deckNameText;

        private Button _buttonRename;
        private Button _buttonAutoComplete;

        private Image _overlordImage;

        private Deck _currentEditDeck;

        public Action<Deck, CollectionData> OnPressedAutoComplete;

        public void Load(GameObject obj)
        {
            _deckNameText = obj.transform.Find("Deck_Name").GetComponent<TextMeshProUGUI>();

            _overlordImage = obj.transform.Find("Overlord_Frame/Overlord_Image/Image").GetComponent<Image>();

            _buttonRename = obj.transform.Find("Button_Rename").GetComponent<Button>();
            _buttonRename.onClick.AddListener(ButtonRenameHandler);

            _buttonAutoComplete = obj.transform.Find("Button_AutoComplete").GetComponent<Button>();
            _buttonAutoComplete.onClick.AddListener(ButtonAutoCompleteHandler);
        }

        public void Show(Deck deck)
        {
            _currentEditDeck = deck.Clone();
            _deckNameText.text = deck.Name;

            Enumerators.Faction faction = DataUtilities.GetFaction(deck.OverlordId);
            _overlordImage.sprite = DataUtilities.GetOverlordImage(deck.OverlordId);
            RectTransform rectTransform = _overlordImage.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = DataUtilities.GetOverlordImagePositionInViewDeck(faction);
            rectTransform.localScale = DataUtilities.GetOverlordImageScaleInViewDeck(faction);

            RenamePopup.OnSaveNewDeckName += OnSaveNewDeckName;
        }

        public void Dispose()
        {
            RenamePopup.OnSaveNewDeckName -= OnSaveNewDeckName;
        }

        private void OnSaveNewDeckName(string name)
        {
            _deckNameText.text = name;
        }

        private void ButtonAutoCompleteHandler()
        {
            DataUtilities.PlayClickSound();

            CollectionData collectionData = new CollectionData();
            collectionData.Cards.Clear();

            List<CollectionCardData> data = GameClient.Get<IDataManager>().CachedCollectionData.Cards;
            foreach (CollectionCardData card in data)
            {
                CollectionCardData cardData = new CollectionCardData(card.CardKey, card.Amount);
                collectionData.Cards.Add(cardData);
            }

            GameClient.Get<IGameplayManager>().GetController<DeckGeneratorController>().GenerateCardsToDeck
            (
                _currentEditDeck,
                collectionData
            );

            foreach(DeckCardData card in _currentEditDeck.Cards)
            {
                Card fetchedCard = GameClient.Get<IDataManager>().CachedCardsLibraryData.GetCardByCardKey(card.CardKey);
                collectionData.GetCardData(fetchedCard.CardKey).Amount -= card.Amount;
            }

            OnPressedAutoComplete?.Invoke(_currentEditDeck, collectionData);
        }

        private void ButtonRenameHandler()
        {
            IUIManager uiManager = GameClient.Get<IUIManager>();
            uiManager.DrawPopup<RenamePopup>(_currentEditDeck);

        }
    }
}
