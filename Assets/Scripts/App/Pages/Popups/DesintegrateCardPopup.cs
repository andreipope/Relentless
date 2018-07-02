// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;


namespace LoomNetwork.CZB
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
        private MenuButtonNoGlow _yesButton,
                                _noButton,
                                _backButton;

		private TextMeshProUGUI _buttonText;

		public Transform cardTransform;

        private CollectionCardData _cardData;

		public void Init()
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _uiManager = GameClient.Get<IUIManager>();

            _selfPage = MonoBehaviour.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Popups/DesintegrateCardPopup"));
            _selfPage.transform.SetParent(_uiManager.Canvas2.transform, false);

            _yesButton = _selfPage.transform.Find("QuestionArea/YesButton").GetComponent<MenuButtonNoGlow>();
            _noButton = _selfPage.transform.Find("QuestionArea/NoButton").GetComponent<MenuButtonNoGlow>();
            _backButton = _selfPage.transform.Find("Button_Back").GetComponent<MenuButtonNoGlow>(); 

            _yesButton.onClickEvent.AddListener(DesintegrateButtonHandler);
			_noButton.onClickEvent.AddListener(CloseDesintegratePopup);
            _backButton.onClickEvent.AddListener(CloseDesintegratePopup);

            //_description = _selfPage.transform.Find("DesintegrateArea/Description").GetComponent<TextMeshProUGUI>();

            Hide();
        }


		public void Dispose()
		{
		}

        private void CloseDesintegratePopup()
        {
            GameClient.Get<ISoundManager>().PlaySound(Common.Enumerators.SoundType.CLICK, Constants.SFX_SOUND_VOLUME, false, false, true);
            var libraryCard = GameClient.Get<IDataManager>().CachedCardsLibraryData.Cards.Find(card => card.id == _cardData.cardId);
			_uiManager.DrawPopup<CardInfoPopup>(libraryCard);

			//(_uiManager.GetPopup<CardInfoPopup>() as CardInfoPopup).UpdateCardAmount();
            Hide();
        }

		public void Hide()
		{
            /*if (cardTransform != null)
            {
                cardTransform.DOKill();
                cardTransform.DOScale(new Vector3(1f, 1f, 1f), 0.2f);
            }*/
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
            GameClient.Get<ISoundManager>().PlaySound(Common.Enumerators.SoundType.CLICK, Constants.SFX_SOUND_VOLUME, false, false, true);
            _cardData.amount--;
            if (_cardData.amount == 0)
                _yesButton.GetComponent<MenuButtonNoGlow>().interactable = false;
            GameObject.Find("CardPreview").GetComponent<BoardCard>().UpdateAmount(_cardData.amount);

            var libraryCard = GameClient.Get<IDataManager>().CachedCardsLibraryData.Cards.Find(card => card.id == _cardData.cardId);
            GameClient.Get<IPlayerManager>().LocalUser.gooValue += 5 * ((int)libraryCard.cardRarity + 1);

			(_uiManager.GetPage<CollectionPage>() as CollectionPage).UpdateGooValue();
		}
    }
}