
using System.Collections.Generic;
using log4net;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Loom.ZombieBattleground
{
    public class ViewDeckPage : IUIElement
    {
        private static readonly ILog Log = Logging.GetLog(nameof(ShopWithNavigationPage));

        private const float BoardCardScale = 0.35f;

        private GameObject _selfPage;
        private GameObject _cardCreaturePrefab;

        private ILoadObjectsManager _loadObjectsManager;
        private IUIManager _uiManager;
        private IDataManager _dataManager;

        private RectTransform _allCardsContent;
        private Button _backButton;
        private TextMeshProUGUI _cardsCountText;

        private List<UnitCardUI> _cardInDeckUIList = new List<UnitCardUI>();

        private DeckInfoUI _deckInfoUi;
        private AbilitiesInfoUI _abilitiesInfoUi;
        private CardGooInfoUI _cardGooInfoUi;

        public void Init()
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _uiManager = GameClient.Get<IUIManager>();
            _dataManager = GameClient.Get<IDataManager>();

            _cardCreaturePrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Cards/CreatureCard_UI");
        }

        public void Show()
        {

        }

        public void Show(Deck deck)
        {
            _selfPage = Object.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Pages/ViewDeckPage"),
                _uiManager.Canvas.transform, false);

            _cardsCountText = _selfPage.transform.Find("ViewDeck/Top_Panel/Left_Panel/Image_CardCounter/Text_CardsAmount").GetComponent<TextMeshProUGUI>();

            _backButton = _selfPage.transform.Find("ViewDeck/Top_Panel/Left_Panel/Button_Back").GetComponent<Button>();
            _backButton.onClick.AddListener(BackButtonHandler);

            _allCardsContent = _selfPage.transform.Find("ViewDeck/Panel_Content/Deck/Element/Scroll View")
                .GetComponent<ScrollRect>().content;

            UpdatePageScaleToMatchResolution();

            // load deck cards
            LoadAllCardsInDeck(deck);

            //deck info
            _deckInfoUi = new DeckInfoUI();
            _deckInfoUi.Load(_selfPage.transform.Find("ViewDeck/Top_Panel/Middle_Panel/Deck_Info").gameObject);
            _deckInfoUi.Show(deck);
            _deckInfoUi.OnPressedAutoComplete += OnPressAutoCompleteDeckHandler;

            // abilities info
            _abilitiesInfoUi = new AbilitiesInfoUI();
            _abilitiesInfoUi.Load(_selfPage.transform.Find("ViewDeck/Top_Panel/Middle_Panel/Abilities_Info").gameObject);
            _abilitiesInfoUi.ShowAbilities(deck.OverlordId, deck.PrimarySkill, deck.SecondarySkill);

            // Goo info
            _cardGooInfoUi = new CardGooInfoUI();
            _cardGooInfoUi.Load(_selfPage.transform.Find("ViewDeck/Top_Panel/Middle_Panel/Goo_Info").gameObject);
            _cardGooInfoUi.SetGooMeter(deck.Cards);

            // cards count
            UpdateCardsInDeckCountDisplay(deck);
        }

        private void OnPressAutoCompleteDeckHandler(Deck deck, CollectionData collectionData)
        {
            LoadAllCardsInDeck(deck);

            _cardGooInfoUi.SetGooMeter(deck.Cards);

            UpdateCardsInDeckCountDisplay(deck);
        }

        private void UpdateCardsInDeckCountDisplay(Deck deck)
        {
            int cardsInDeckCount = 0;
            if (deck != null)
            {
                for (int i = 0; i < deck.Cards.Count; i++)
                {
                    DeckCardData deckCardData = deck.Cards[i];
                    cardsInDeckCount += deckCardData.Amount;
                }
            }

            _cardsCountText.text = cardsInDeckCount + "/" + Constants.MaxDeckSize;
        }

        private void BackButtonHandler()
        {
            DataUtilities.PlayClickSound();
            _uiManager.GetPopup<QuestionPopup>().ConfirmationReceived += ConfirmSaveDeckHandler;
            _uiManager.DrawPopup<QuestionPopup>("Would you like to save your progress?");

        }

        private void ConfirmSaveDeckHandler(bool status)
        {
            _uiManager.GetPopup<QuestionPopup>().ConfirmationReceived -= ConfirmSaveDeckHandler;

            if (status)
            {
                // TODO : save deck
                // change ui of editor tab
                // save deck
                Dispose();
            }
            else
            {
                Dispose();
            }
        }

        private void UpdatePageScaleToMatchResolution()
        {
            float screenRatio = (float) Screen.width / Screen.height;
            if (screenRatio < 1.76f)
            {
                _selfPage.transform.localScale = Vector3.one * 0.93f;
            }
        }

        public void Hide()
        {

        }

        public void Update()
        {

        }

        public void Dispose()
        {
            if (_selfPage == null)
                return;

            Object.Destroy(_selfPage);
        }

        private void LoadAllCardsInDeck(Deck deck)
        {
            for (int i = _cardInDeckUIList.Count-1; i >= 0; i--)
            {
                if(_cardInDeckUIList[i].GetGameObject() != null)
                    Object.Destroy(_cardInDeckUIList[i].GetGameObject());
            }
            _cardInDeckUIList = new List<UnitCardUI>();

            for (int i = 0; i < deck.Cards.Count; i++)
            {
                DeckCardData deckCardData = deck.Cards[i];
                int cardIndex = _dataManager.CachedCardsLibraryData.Cards.FindIndex(cachedCard => cachedCard.MouldId == deckCardData.MouldId);
                if (cardIndex == -1)
                {
                    Log.Error($"Card with MouldId {deckCardData.MouldId} not found.");
                    return;
                }

                Card card = _dataManager.CachedCardsLibraryData.Cards[cardIndex];
                InstantiateCard(card, deckCardData.Amount);
            }
        }

        private void InstantiateCard(Card card, int cardAmount)
        {
            GameObject go = Object.Instantiate(_cardCreaturePrefab, _allCardsContent, false);
            go.transform.localScale = Vector3.one * BoardCardScale;

            UnitCardUI unitCard = new UnitCardUI();
            unitCard.Init(go);
            unitCard.FillCardData(card, cardAmount);

            _cardInDeckUIList.Add(unitCard);
        }


    }
}


