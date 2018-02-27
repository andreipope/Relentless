using GrandDevs.CZB.Common;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CCGKit;
using DG.Tweening;


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

        private TextMeshProUGUI _description;
        private Text _amount;
        private MenuButtonNoGlow _backButton,
                                _desintegrateButton;
		private TextMeshProUGUI _buttonText;

        private Card _card;
        public Transform cardTransform;

        public void Init()
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _uiManager = GameClient.Get<IUIManager>();

            _selfPage = MonoBehaviour.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Popups/CardInfoPopup"));
            _selfPage.transform.SetParent(_uiManager.Canvas.transform, false);

			_desintegrateButton = _selfPage.transform.Find("DesintegrateArea/DesintegrateButton").GetComponent<MenuButtonNoGlow>();
			_backButton = _selfPage.transform.Find("BackButton").GetComponent<MenuButtonNoGlow>();

			_desintegrateButton.onClickEvent.AddListener(DesintegrateButtonHandler);
			_backButton.onClickEvent.AddListener(Hide);

			_description = _selfPage.transform.Find("DesintegrateArea/Description").GetComponent<TextMeshProUGUI>();
			_amount = _selfPage.transform.Find("DesintegrateArea/Amount/Value").GetComponent<Text>();

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
            _description.text = _card.GetStringProperty("Text");
            _amount.text = _card.GetIntProperty("MaxCopies").ToString();

            Show();
        }

        public void Update()
        {

        }

        private void DesintegrateButtonHandler()
        {
            cardTransform.DOKill();
			cardTransform.DOScale(new Vector3(1.8f, 1.8f, 1.8f), 0.2f);
            _uiManager.DrawPopup<DesintigrateCardPopup>(_card);
			(_uiManager.GetPopup<DesintigrateCardPopup>() as DesintigrateCardPopup).cardTransform = cardTransform;

		}

    }
}