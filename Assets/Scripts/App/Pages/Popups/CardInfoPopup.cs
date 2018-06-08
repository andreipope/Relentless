using GrandDevs.CZB.Common;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using GrandDevs.CZB.Data;


namespace GrandDevs.CZB
{
    public class CardInfoPopup : IUIPopup
    {
		public GameObject Self
        {
            get { return _selfPage; }
        }

        private ILoadObjectsManager _loadObjectsManager;
        private IUIManager _uiManager;
        private GameObject _selfPage;

        private TextMeshProUGUI _description,
                                _amountAward;
        private MenuButtonNoGlow _backButton,
                                _desintegrateButton;
		private TextMeshProUGUI _buttonText;

        private Card _card;
        public Transform cardTransform;
        public CollectionCardData _cardData;

        public void Init()
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _uiManager = GameClient.Get<IUIManager>();

            _selfPage = MonoBehaviour.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Popups/CardInfoPopup"));
            _selfPage.transform.SetParent(_uiManager.Canvas2.transform, false);

			_desintegrateButton = _selfPage.transform.Find("DesintegrateArea/DesintegrateButton").GetComponent<MenuButtonNoGlow>();
			_backButton = _selfPage.transform.Find("BackButton").GetComponent<MenuButtonNoGlow>();


			_desintegrateButton.onClickEvent.AddListener(DesintegrateButtonHandler);
			_backButton.onClickEvent.AddListener(Hide);
			_selfPage.GetComponent<Button>().onClick.AddListener(Hide);


			_description = _selfPage.transform.Find("DesintegrateArea/Description").GetComponent<TextMeshProUGUI>();
			_amountAward = _selfPage.transform.Find("DesintegrateArea/GooAward/Value").GetComponent<TextMeshProUGUI>();

            Hide();
        }


		public void Dispose()
		{
		}

		public void Hide()
		{
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
            _card = data as Card;
            _description.text = _card.flavorText;

            _amountAward.text = (5 * ((int)_card.cardRarity + 1)).ToString();

            _cardData = GameClient.Get<IDataManager>().CachedCollectionData.GetCardData(_card.id);
            UpdateCardAmount();
            Show();
        }

        public void Update()
        {

        }

		public void UpdateCardAmount()
		{
			if (_cardData.amount == 0)
				_desintegrateButton.GetComponent<MenuButtonNoGlow>().interactable = false;
			else
				_desintegrateButton.GetComponent<MenuButtonNoGlow>().interactable = true;
		}

        private void DesintegrateButtonHandler()
        {
            GameClient.Get<ISoundManager>().PlaySound(Common.Enumerators.SoundType.CLICK, Constants.SFX_SOUND_VOLUME, false, false, true);
            int amount = _cardData.amount;
            if (amount == 0)
                _desintegrateButton.GetComponent<MenuButtonNoGlow>().interactable = false;
            //_uiManager.DrawPopup<WarningPopup>("Sorry you don't have cards to desintegrate");
            else
            {
                /*cardTransform.DOKill();
                cardTransform.DOScale(new Vector3(.3f, .3f, .3f), 0.2f);*/
                Hide();
                _uiManager.DrawPopup<DesintigrateCardPopup>(_cardData);
                (_uiManager.GetPopup<DesintigrateCardPopup>() as DesintigrateCardPopup).cardTransform = cardTransform;
            }   
		}
    }
}