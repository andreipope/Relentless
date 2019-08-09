using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using DG.Tweening;
using Object = UnityEngine.Object;
using Card = Loom.ZombieBattleground.Data.Card;
using Deck = Loom.ZombieBattleground.Data.Deck;
using OverlordUserInstance = Loom.ZombieBattleground.Data.OverlordUserInstance;

namespace Loom.ZombieBattleground
{
    public class HordeEditingTab
    {
        private static readonly ILog Log = Logging.GetLog(nameof(HordeEditingTab));

        private IDataManager _dataManager;

        private ITutorialManager _tutorialManager;

        private IUIManager _uiManager;

        private HordeSelectionWithNavigationPage _myDeckPage;

        private GameObject CardCreaturePrefab;

        private CollectionData _collectionData;

        private Button _buttonAutoComplete;
        private Button _buttonBack;

        private Button _buttonLeftArrowScroll;
        private Button _buttonRightArrowScroll;

        private Scrollbar _cardCollectionScrollBar;

        private TextMeshProUGUI _textEditDeckName,
                                _textEditDeckCardsAmount;

        private List<Enumerators.Faction> _availableFaction;

        private HordeSelectionWithNavigationPage.Tab _nextTab;

        private Enumerators.Faction _againstFaction;

        private UICardCollections _uiCardCollections;
        private CustomDeckUI _customDeckUi;

        public bool VariantPopupIsActive;

        public void Init()
        {
            _dataManager = GameClient.Get<IDataManager>();
            _uiManager = GameClient.Get<IUIManager>();
            _tutorialManager = GameClient.Get<ITutorialManager>();

            _collectionData = new CollectionData();

            _myDeckPage = _uiManager.GetPage<HordeSelectionWithNavigationPage>();

            _uiCardCollections = new UICardCollections();
            _uiCardCollections.Init();

            _customDeckUi = new CustomDeckUI();
            _customDeckUi.Init();

            Log.Info("Editing init called");
        }

        public void Load(GameObject editingTabObj)
        {
            _buttonAutoComplete = editingTabObj.transform.Find("Panel_Frame/Upper_Items/Button_AutoComplete").GetComponent<Button>();
            _buttonAutoComplete.onClick.AddListener(ButtonAutoCompleteHandler);

            _buttonBack = editingTabObj.transform.Find("Panel_Frame/Upper_Items/Button_Back").GetComponent<Button>();
            _buttonBack.onClick.AddListener(ButtonBackHandler);

            _uiCardCollections.Show(editingTabObj, Enumerators.CardCollectionPageType.DeckEditing);
            _customDeckUi.Load(editingTabObj.transform.Find("Deck_Content").gameObject);

            _buttonLeftArrowScroll = editingTabObj.transform.Find("Panel_Frame/Panel_Content/Army/Element/Button_LeftArrow").GetComponent<Button>();
            _buttonLeftArrowScroll.onClick.AddListener(ButtonLeftArrowScrollHandler);

            _buttonRightArrowScroll = editingTabObj.transform.Find("Panel_Frame/Panel_Content/Army/Element/Button_RightArrow").GetComponent<Button>();
            _buttonRightArrowScroll.onClick.AddListener(ButtonRightArrowScrollHandler);

            _cardCollectionScrollBar = editingTabObj.transform.Find("Panel_Frame/Panel_Content/Army/Element/Scroll View").GetComponent<ScrollRect>().horizontalScrollbar;
        }

        public void Show(int deckId)
        {
            VariantPopupIsActive = true;

            FillCollectionData();

            if (deckId == -1)
            {
                _customDeckUi.ShowDeck(_myDeckPage.CurrentEditDeck);

                if(_tutorialManager.IsTutorial)
                    _uiCardCollections.UpdateCardsAmountDisplayTutorial();
                else
                    _uiCardCollections.UpdateCardsAmountDisplay();

            }
            else
            {
                _customDeckUi.ShowDeck(_myDeckPage.CurrentEditDeck);
                _uiCardCollections.UpdateCardsAmountDisplay(deckId);
                UpdateCollectionCards(deckId);
            }
        }

        private void UpdateCollectionCards(int deckId)
        {
            Deck selectedDeck = _dataManager.CachedDecksData.Decks.Find(deck => deck.Id.Id == deckId);
            if (selectedDeck == null)
                return;

            for (int i = 0; i < _collectionData.Cards.Count; i++)
            {
                CollectionCardData cardData = _collectionData.Cards[i];

                // get amount of card in collection data
                CollectionCardData cardInCollection = _dataManager.CachedCollectionData.Cards.Find(card => card.CardKey == cardData.CardKey);
                int totalCardAmount = cardInCollection.Amount;

                DeckCardData deckCardData = selectedDeck.Cards.Find(card => card.CardKey == cardData.CardKey);
                if (deckCardData != null)
                    cardData.Amount = totalCardAmount - deckCardData.Amount;
            }
        }

        public void Update()
        {
            _uiCardCollections.Update();
            _customDeckUi.Update();
        }

        public void Dispose()
        {
            _uiCardCollections.Hide();
        }

        #region Button Handlers

        private void ButtonLeftArrowScrollHandler()
        {
            if (_tutorialManager.BlockAndReport(_buttonLeftArrowScroll.name))
                return;

            if (_cardCollectionScrollBar.value <= 0)
                return;

            _cardCollectionScrollBar.value -= _cardCollectionScrollBar.size;
            if (_cardCollectionScrollBar.value <= 0)
                _cardCollectionScrollBar.value = 0;

        }

        private void ButtonRightArrowScrollHandler()
        {
            if (_tutorialManager.BlockAndReport(_buttonLeftArrowScroll.name))
                return;

            if (_cardCollectionScrollBar.value >= 1)
                return;

            _cardCollectionScrollBar.value += _cardCollectionScrollBar.size;
            if (_cardCollectionScrollBar.value >= 1)
                _cardCollectionScrollBar.value = 1;
        }


        private void FinishAddDeck(bool success, Deck deck)
        {
            DeckGeneratorController deckGeneratorController = GameClient.Get<IGameplayManager>().GetController<DeckGeneratorController>();
            deckGeneratorController.FinishAddDeck -= FinishAddDeck;
            _myDeckPage.IsEditingNewDeck = false;

            if (GameClient.Get<IAppStateManager>().AppState != Enumerators.AppState.HordeSelection)
                return;

            if (_myDeckPage.CurrentEditDeck.Id.Id < 0)
                return;

            List<Deck> cacheDeckList = _myDeckPage.GetDeckList();
            _myDeckPage.SelectDeckIndex = cacheDeckList.IndexOf(_myDeckPage.CurrentEditDeck);
            _myDeckPage.SelectDeckIndex = Mathf.Min(_myDeckPage.SelectDeckIndex, cacheDeckList.Count-1);

            _myDeckPage.AssignCurrentDeck(_myDeckPage.SelectDeckIndex);
            _myDeckPage.ChangeTab(_nextTab);
        }

        private void FinishEditDeck(bool success, Deck deck)
        {
            GameClient.Get<IGameplayManager>().GetController<DeckGeneratorController>().FinishEditDeck -= FinishEditDeck;

            if (GameClient.Get<IAppStateManager>().AppState != Enumerators.AppState.HordeSelection)
                return;

            _myDeckPage.ChangeTab(_nextTab);
        }

        private void ButtonAutoCompleteHandler()
        {
            if (_tutorialManager.BlockAndReport(_buttonAutoComplete.name))
                return;

            DataUtilities.PlayClickSound();
            FillCollectionData();

            GameClient.Get<IGameplayManager>().GetController<DeckGeneratorController>().GenerateCardsToDeck
            (
                _myDeckPage.CurrentEditDeck,
                _collectionData
            );
            SubtractInitialDeckCardsAmountFromCollections(_myDeckPage.CurrentEditDeck);

            _customDeckUi.ShowDeck(_myDeckPage.CurrentEditDeck);
            _uiCardCollections.UpdateCardsAmountDisplay(_myDeckPage.CurrentEditDeck);
        }

        public void UpdateEditingTab(Deck deck, CollectionData collectionData)
        {
            _myDeckPage.CurrentEditDeck = deck;

            if(collectionData != null)
                _collectionData = collectionData;

            _customDeckUi.ShowDeck(_myDeckPage.CurrentEditDeck);
            _uiCardCollections.UpdateCardsAmountDisplay(_myDeckPage.CurrentEditDeck);
        }

        public CustomDeckUI GetCustomDeck()
        {
            return _customDeckUi;
        }

        private void ButtonBackHandler()
        {
            if (_tutorialManager.BlockAndReport(_buttonAutoComplete.name))
                return;

            DataUtilities.PlayClickSound();
            _uiManager.GetPopup<QuestionPopup>().ConfirmationReceived += ConfirmSaveDeckHandler;
            _uiManager.DrawPopup<QuestionPopup>("Would you like to save your progress?");
        }

        private void ConfirmSaveDeckHandler(bool status)
        {
            _uiManager.GetPopup<QuestionPopup>().ConfirmationReceived -= ConfirmSaveDeckHandler;

            if (status)
            {
                SaveDeck(HordeSelectionWithNavigationPage.Tab.SelectDeck);
            }
            else
            {
                _myDeckPage.ChangeTab(HordeSelectionWithNavigationPage.Tab.SelectDeck);
            }
        }

        #endregion

        private void FillCollectionData()
        {
            _collectionData = new CollectionData();
            _collectionData.Cards.Clear();

            List<CollectionCardData> data;
            if (_tutorialManager.IsTutorial)
            {
                data =
                    _tutorialManager.CurrentTutorial.TutorialContent.ToMenusContent().SpecificHordeInfo.CardsForArmy
                        .Select(card => card.ToCollectionCardData(_dataManager))
                        .ToList();
            }
            else
            {
                data = _dataManager.CachedCollectionData.Cards;
            }

            foreach (CollectionCardData card in data)
            {
                CollectionCardData cardData = new CollectionCardData(card.CardKey, card.Amount);
                _collectionData.Cards.Add(cardData);
            }
        }

        public CollectionData GetCollectionData()
        {
            return _collectionData;
        }

        private void SubtractInitialDeckCardsAmountFromCollections(Deck deck)
        {
            foreach(DeckCardData card in deck.Cards)
            {
                Card fetchedCard = _dataManager.CachedCardsLibraryData.GetCardByCardKey(card.CardKey);
                _collectionData.GetCardData(fetchedCard.CardKey).Amount -= card.Amount;
            }
        }

        public void AddCardToDeck(IReadOnlyCard card, bool animate = false, bool skipPopup = false,  object cardKey = null, bool forcePopup = false)
        {   
            List<CollectionCardData> otherVariants = _collectionData.Cards.FindAll(x => x.CardKey.MouldId == card.CardKey.MouldId && x.Amount > 0).ToList();
            
            if (otherVariants.Count > 1 && ((VariantPopupIsActive && !skipPopup) || forcePopup))
            {
                _uiManager.GetPopup<SelectSkinPopup>().Show(card);
                return;
            }

            if (_myDeckPage.CurrentEditDeck == null)
            {
                Debug.LogError("current edit deck is null");
                return;
            }

            OverlordUserInstance overlordData = _dataManager.CachedOverlordData.GetOverlordById(_myDeckPage.CurrentEditDeck.OverlordId);
            if (Constants.FactionAgainstDictionary[overlordData.Prototype.Faction] == card.Faction)
            {
                OpenAlertDialog(
                    "Cannot add from the faction your Champion is weak against.");
                return;
            }

            CollectionCardData collectionCardData = _collectionData.GetCardData(card.CardKey);
            if (cardKey is CardKey)
            {
                collectionCardData = _collectionData.GetCardData((CardKey) cardKey);
            }
            
            if (collectionCardData.Amount <= 0)
            {
                otherVariants = _collectionData.Cards.FindAll(x => x.CardKey.MouldId == card.CardKey.MouldId).ToList();
                for (int i = 0; i < otherVariants.Count; i++)
                {
                    if (otherVariants[i].Amount > 0) 
                    {
                        collectionCardData = otherVariants[i];
                        break;
                    }
                }
                
                if (collectionCardData.Amount <= 0)
                {
                    OpenAlertDialog(
                        "You don't have enough of this card.\nBuy or earn packs to get more cards.");
                    return;
                }
            }

            List<DeckCardData> existingCard = _myDeckPage.CurrentEditDeck.Cards.FindAll(x => x.CardKey.MouldId == card.CardKey.MouldId).ToList();
            int totalExistingAmount = 0;

            for (int i = 0; i < existingCard.Count; i++)
            {
                totalExistingAmount += existingCard[i].Amount;
            }

            uint maxCopies = GetMaxCopiesValue(card);
            if ((existingCard != null || existingCard.Count > 0) && totalExistingAmount >= maxCopies)
            {
                OpenAlertDialog("Cannot have more than " + maxCopies + " copies of an " +
                    card.Rank.ToString().ToLowerInvariant() + " card in one deck.");
                return;
            }

            if (_myDeckPage.CurrentEditDeck.GetNumCards() == Constants.DeckMaxSize)
            {
                OpenAlertDialog("Cannot have more than " + Constants.DeckMaxSize + " cards in one deck.");
                return;
            }

            _myDeckPage.CurrentEditDeck.AddCard(collectionCardData.CardKey);

            // update count in card collection list left panel
            collectionCardData.Amount--;
            _uiCardCollections.UpdateCardAmountDisplay(card, collectionCardData.Amount);

            _customDeckUi.AddCard((Card)_dataManager.CachedCardsLibraryData.Cards.FirstOrDefault(x => x.CardKey == collectionCardData.CardKey));

            _customDeckUi.UpdateCardsInDeckCountDisplay();

            if (_tutorialManager.IsTutorial && _myDeckPage.CurrentEditDeck.GetNumCards() >= _tutorialManager.CurrentTutorial.TutorialContent.ToMenusContent().SpecificHordeInfo.MaximumCardsCount)
            {
                GameClient.Get<ITutorialManager>().ReportActivityAction(Enumerators.TutorialActivityAction.HordeFilled);
            }
        }

        public void RemoveCardFromDeck(IReadOnlyCard card, bool animate)
        {
            CollectionCardData collectionCardData = _collectionData.GetCardData(card.CardKey);
            collectionCardData.Amount++;

            DeckCardData existingCard = _myDeckPage.CurrentEditDeck.Cards.Find(x => x.CardKey == card.CardKey);
            int existingCardAmount = existingCard?.Amount ?? 0;

            _myDeckPage.CurrentEditDeck.RemoveCard(card.CardKey);

            _customDeckUi.RemoveCard((Card)card);

            // update left panel.. change the card amount in card
            _uiCardCollections.UpdateCardAmountDisplay(card, collectionCardData.Amount);

            // update card count
            _customDeckUi.UpdateCardsInDeckCountDisplay();

            if (_tutorialManager.IsTutorial)
            {
                GameClient.Get<ITutorialManager>().ReportActivityAction(Enumerators.TutorialActivityAction.CardRemoved);
            }
        }

        private void CreateExchangeAnimationCard
        (
            BoardCardView animatedCard,
            BoardCardView targetCard,
            bool targetCardWasAlreadyPresent
        )
        {
            if(targetCard == null)
            {
                Object.Destroy(animatedCard.GameObject);
                return;
            }

            animatedCard.GameObject.GetComponent<SortingGroup>().sortingOrder++;

            Vector3 endPosition = targetCard.GameObject.transform.position;

            if (!targetCardWasAlreadyPresent)
            {
                targetCard.GameObject.SetActive(false);
            }

            Sequence animatedSequence = DOTween.Sequence();
            animatedSequence.Append(animatedCard.Transform.DOMove(endPosition, .3f));
            animatedSequence.AppendCallback
            (() =>
            {
                Object.Destroy(animatedCard.GameObject);
                if (!targetCardWasAlreadyPresent)
                {
                    targetCard.GameObject.SetActive(true);
                }
            });
        }

        #region Boardcard Handler

        #endregion

        private uint GetMaxCopiesValue(IReadOnlyCard card)
        {
            Enumerators.CardRank rank = card.Rank;
            uint maxCopies;

            if (card.Faction == Enumerators.Faction.ITEM)
            {
                maxCopies = Constants.CardItemMaxCopies;
                return maxCopies;
            }

            switch (rank)
            {
                case Enumerators.CardRank.MINION:
                    maxCopies = Constants.CardMinionMaxCopies;
                    break;
                case Enumerators.CardRank.OFFICER:
                    maxCopies = Constants.CardOfficerMaxCopies;
                    break;
                case Enumerators.CardRank.COMMANDER:
                    maxCopies = Constants.CardCommanderMaxCopies;
                    break;
                case Enumerators.CardRank.GENERAL:
                    maxCopies = Constants.CardGeneralMaxCopies;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(rank), rank, null);
            }

            return maxCopies;
        }

        public void SaveDeck(HordeSelectionWithNavigationPage.Tab nextTab)
        {
            _nextTab = nextTab;

            DeckGeneratorController deckGeneratorController = GameClient.Get<IGameplayManager>().GetController<DeckGeneratorController>();
            if(_myDeckPage.IsEditingNewDeck)
            {
                deckGeneratorController.FinishAddDeck += FinishAddDeck;
                //_myDeckPage.CurrentEditDeck.OverlordId = _myDeckPage.CurrentEditOverlord.Prototype.Id;
                //_myDeckPage.CurrentEditDeck.PrimarySkill = _myDeckPage.SelectOverlordSkillTab.SelectedPrimarySkill;
                //_myDeckPage.CurrentEditDeck.SecondarySkill = _myDeckPage.SelectOverlordSkillTab.SelectedSecondarySkill;
                deckGeneratorController.ProcessAddDeck(_myDeckPage.CurrentEditDeck);

            }
            else
            {
                deckGeneratorController.FinishEditDeck += FinishEditDeck;
                deckGeneratorController.ProcessEditDeck(_myDeckPage.CurrentEditDeck);
            }
        }

        private void PlayAddCardSound()
        {
            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.DECKEDITING_ADD_CARD,
                Constants.SfxSoundVolume, false, false, true);
        }

        private void PlayRemoveCardSound()
        {
             GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.DECKEDITING_REMOVE_CARD,
                Constants.SfxSoundVolume, false, false, true);
        }

        private void OpenAlertDialog(string msg)
        {
            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.CHANGE_SCREEN, Constants.SfxSoundVolume,
                false, false, true);
            _uiManager.DrawPopup<WarningPopup>(msg);
        }
    }
}
