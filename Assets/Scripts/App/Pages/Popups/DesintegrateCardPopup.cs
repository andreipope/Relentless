using GrandDevs.CZB.Common;
using GrandDevs.CZB.Data;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;


namespace GrandDevs.CZB
{
    public class DesintigrateCardPopup : IUIPopup
    {
		public GameObject Self
        {
            get { return _selfPage; }
        }

        private ILoadObjectsManager _loadObjectsManager;
        private IUIManager _uiManager;
        private GameObject _selfPage;

        //private TextMeshProUGUI _description;
        private Text _amount;
        private MenuButtonNoGlow _yesButton,
                                _noButton;
		private TextMeshProUGUI _buttonText;

		public Transform cardTransform;

        private CollectionCardData _cardData;

		public void Init()
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _uiManager = GameClient.Get<IUIManager>();

            _selfPage = MonoBehaviour.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Popups/DesintegrateCardPopup"));
            _selfPage.transform.SetParent(_uiManager.Canvas.transform, false);

            _yesButton = _selfPage.transform.Find("QuestionArea/YesButton").GetComponent<MenuButtonNoGlow>();
            _noButton = _selfPage.transform.Find("QuestionArea/NoButton").GetComponent<MenuButtonNoGlow>();

			_yesButton.onClickEvent.AddListener(DesintegrateButtonHandler);
			_noButton.onClickEvent.AddListener(CloseDesintegratePopup);

			//_description = _selfPage.transform.Find("DesintegrateArea/Description").GetComponent<TextMeshProUGUI>();
			_amount = _selfPage.transform.Find("QuestionArea/Amount/Value").GetComponent<Text>();

            Hide();
        }


		public void Dispose()
		{
		}

        private void CloseDesintegratePopup()
        {
            (_uiManager.GetPopup<CardInfoPopup>() as CardInfoPopup).UpdateCardAmount();
            Hide();
        }

		public void Hide()
		{
            if (cardTransform != null)
            {
                cardTransform.DOKill();
                cardTransform.DOScale(new Vector3(2.5f, 2.5f, 2.5f), 0.2f);
            }
            _selfPage.SetActive(false);
		}

        public void SetMainPriority()
        {
        }

        public void Show()
        {
            _selfPage.SetActive(true);
        }

        public void Show(object data)
        {
            _cardData =  data as CollectionCardData;
            //_description.text = _card.description;
            _amount.text = _cardData.amount.ToString();
            if (_cardData.amount == 0)
                _yesButton.GetComponent<MenuButtonNoGlow>().interactable = false;
            else
                _yesButton.GetComponent<MenuButtonNoGlow>().interactable = true;

                  
            Show();
        }

        public void Update()
        {

        }

        private void DesintegrateButtonHandler()
        {
            _cardData.amount--;
            if (_cardData.amount == 0)
                _yesButton.GetComponent<MenuButtonNoGlow>().interactable = false;
            _amount.text = _cardData.amount.ToString();
            GameObject.Find("CardPreview").GetComponent<CardView>().UpdateAmount(_cardData.amount);

            var libraryCard = GameClient.Get<IDataManager>().CachedCardsLibraryData.Cards.Find(card => card.id == _cardData.cardId);
            GameClient.Get<IPlayerManager>().LocalUser.gooValue += 25 * ((int)libraryCard.cardRarity + 1);

			(_uiManager.GetPage<CollectionPage>() as CollectionPage).UpdateGooValue();
		}
    }
}