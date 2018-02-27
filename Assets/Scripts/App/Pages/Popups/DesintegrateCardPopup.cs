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

		public void Init()
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _uiManager = GameClient.Get<IUIManager>();

            _selfPage = MonoBehaviour.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Popups/DesintegrateCardPopup"));
            _selfPage.transform.SetParent(_uiManager.Canvas.transform, false);

            _yesButton = _selfPage.transform.Find("QuestionArea/YesButton").GetComponent<MenuButtonNoGlow>();
            _noButton = _selfPage.transform.Find("QuestionArea/NoButton").GetComponent<MenuButtonNoGlow>();

			_yesButton.onClickEvent.AddListener(DesintegrateButtonHandler);
			_noButton.onClickEvent.AddListener(Hide);

			//_description = _selfPage.transform.Find("DesintegrateArea/Description").GetComponent<TextMeshProUGUI>();
			_amount = _selfPage.transform.Find("QuestionArea/Amount/Value").GetComponent<Text>();

            Hide();
        }


		public void Dispose()
		{
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
            Card card = data as Card;
            //_description.text = card.GetStringProperty("Text");
            _amount.text = card.GetIntProperty("MaxCopies").ToString();

            Show();
        }

        public void Update()
        {

        }

        private void DesintegrateButtonHandler()
        {
            //_uiManager.DrawPopup<WarningPopup>("Sorry you can't do that right now");
        }

    }
}