// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/


using UnityEngine;
using UnityEngine.UI;
using TMPro;
using LoomNetwork.CZB.Data;

namespace LoomNetwork.CZB
{
    public class PlayerOrderPopup : IUIPopup
    {
		public GameObject Self
        {
            get { return _selfPage; }
        }

        private ILoadObjectsManager _loadObjectsManager;
        private IUIManager _uiManager;

        private GameObject _selfPage;

        private Animator _selfAnimator;


        private TextMeshProUGUI _playerOverlordNameText,
                                _opponentOverlordNameText;

        private Image _playerOverlordPicture,
                      _opponentOverlordPicture;

        private GameObject _opponentFirstTurnObject,
                           _opponentSecondTurnObject,
                           _opponentCardBackObject,
                           _opponentCardFrontObject;

        private GameObject _playerFirstTurnObject,
                           _playerSecondTurnObject,
                           _playerCardBackObject,
                           _playerCardFrontObject;


        public void Init()
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _uiManager = GameClient.Get<IUIManager>();

            _selfPage = MonoBehaviour.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Popups/PlayerOrderPopup"));
            _selfPage.transform.SetParent(_uiManager.Canvas2.transform, false);

            _selfAnimator = _selfPage.GetComponent<Animator>();

            _playerOverlordNameText = _selfPage.transform.Find("Text_PlayerOverlordName").GetComponent<TextMeshProUGUI>();
            _opponentOverlordNameText = _selfPage.transform.Find("Text_OpponentOverlordName").GetComponent<TextMeshProUGUI>();

            _playerOverlordPicture = _selfPage.transform.Find("Image_PlayerOverlord").GetComponent<Image>();
            _opponentOverlordPicture = _selfPage.transform.Find("Image_OpponentOverlord").GetComponent<Image>();

            _opponentFirstTurnObject = _selfPage.transform.Find("Item_OpponentOverlordTurn/Image_FirstTurn").gameObject;
            _opponentSecondTurnObject = _selfPage.transform.Find("Item_OpponentOverlordTurn/Image_SecondTurn").gameObject;
            _opponentCardBackObject = _selfPage.transform.Find("Item_OpponentOverlordTurn/Image_BackCard").gameObject;
            _opponentCardFrontObject = _selfPage.transform.Find("Item_OpponentOverlordTurn/Image_FrontCard").gameObject;

            _playerFirstTurnObject = _selfPage.transform.Find("Item_PlayerOverlordTurn/Image_FirstTurn").gameObject;
            _playerSecondTurnObject = _selfPage.transform.Find("Item_PlayerOverlordTurn/Image_SecondTurn").gameObject;
            _playerCardBackObject = _selfPage.transform.Find("Item_PlayerOverlordTurn/Image_BackCard").gameObject;
            _playerCardFrontObject = _selfPage.transform.Find("Item_PlayerOverlordTurn/Image_FrontCard").gameObject;

            Hide();
        }


		public void Dispose()
		{
		}

		public void Hide()
		{
            _selfAnimator.StopPlayback();

              _selfPage.SetActive(false);
		}

        public void SetMainPriority()
        {
        }

        public void Show()
        {
            _selfPage.SetActive(true);
            _selfAnimator.Play(0);

            GameClient.Get<ITimerManager>().AddTimer((x) =>
            {
                _uiManager.HidePopup<PlayerOrderPopup>();
            }, null, 5f);
        }

        public void Show(object data)
        {
            object[] param = (object[])data;

            ApplyInfoAboutHeroes((Hero)param[0], (Hero)param[1]);

            Show();
        }

        public void Update()
        {

        }

        private void ApplyInfoAboutHeroes(Hero player, Hero opponent)
        {
            _playerOverlordNameText.name = player.name;
            _opponentOverlordNameText.name = opponent.name;

            _playerOverlordPicture.sprite = _loadObjectsManager.GetObjectByPath<Sprite>("Images/Overlords/abilityselect_hero_" + player.element.ToLower());
           _opponentOverlordPicture.sprite = _loadObjectsManager.GetObjectByPath<Sprite>("Images/Overlords/abilityselect_hero_" + opponent.element.ToLower());

            _playerOverlordPicture.SetNativeSize();
            _opponentOverlordPicture.SetNativeSize();

            //_opponentFirstTurnObject 
            //_opponentSecondTurnObject
            //_opponentCardBackObject 
            //_opponentCardFrontObject

            //_playerFirstTurnObject 
            //_playerSecondTurnObject
            //_playerCardBackObject 
            //_playerCardFrontObject 
        }
    }
}