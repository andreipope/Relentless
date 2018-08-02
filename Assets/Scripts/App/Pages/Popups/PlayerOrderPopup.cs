// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/


using UnityEngine;
using UnityEngine.UI;
using TMPro;
using LoomNetwork.CZB.Data;
using DG.Tweening;

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
        private IGameplayManager _gameplayManager;
        private ITimerManager _timerManager;

        private GameObject _selfPage;

        private Animator _selfAnimator;


        private TextMeshProUGUI _playerOverlordNameText,
                                _opponentOverlordNameText;

        private Image _playerOverlordPicture,
                      _opponentOverlordPicture;

        private GameObject _opponentTurnRootObject,
                           _opponentFirstTurnObject,
                           _opponentSecondTurnObject,
                           _opponentCardBackObject,
                           _opponentCardFrontObject;

        private GameObject _playerTurnRootObject,
                           _playerFirstTurnObject,
                           _playerSecondTurnObject,
                           _playerCardBackObject,
                           _playerCardFrontObject;


        public void Init()
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _uiManager = GameClient.Get<IUIManager>();
            _gameplayManager = GameClient.Get<IGameplayManager>();
            _timerManager = GameClient.Get<ITimerManager>();

            _selfPage = MonoBehaviour.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Popups/PlayerOrderPopup"));
            _selfPage.transform.SetParent(_uiManager.Canvas2.transform, false);

            _selfAnimator = _selfPage.GetComponent<Animator>();

            _playerOverlordNameText = _selfPage.transform.Find("Text_PlayerOverlordName").GetComponent<TextMeshProUGUI>();
            _opponentOverlordNameText = _selfPage.transform.Find("Text_OpponentOverlordName").GetComponent<TextMeshProUGUI>();

            _playerOverlordPicture = _selfPage.transform.Find("Image_PlayerOverlord").GetComponent<Image>();
            _opponentOverlordPicture = _selfPage.transform.Find("Image_OpponentOverlord").GetComponent<Image>();

            _opponentTurnRootObject = _selfPage.transform.Find("Item_OpponentOverlordTurn").gameObject;
            _opponentFirstTurnObject = _selfPage.transform.Find("Item_OpponentOverlordTurn/Image_FirstTurn").gameObject;
            _opponentSecondTurnObject = _selfPage.transform.Find("Item_OpponentOverlordTurn/Image_SecondTurn").gameObject;
            _opponentCardBackObject = _selfPage.transform.Find("Item_OpponentOverlordTurn/Image_BackCard").gameObject;
            _opponentCardFrontObject = _selfPage.transform.Find("Item_OpponentOverlordTurn/Image_FrontCard").gameObject;

            _playerTurnRootObject = _selfPage.transform.Find("Item_PlayerOverlordTurn").gameObject;
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
            _playerOverlordNameText.text = player.name.Split(',')[0];
            _opponentOverlordNameText.text = opponent.name.Split(',')[0];

            _playerOverlordPicture.sprite = _loadObjectsManager.GetObjectByPath<Sprite>("Images/Overlords/abilityselect_hero_" + player.element.ToLower());
            _opponentOverlordPicture.sprite = _loadObjectsManager.GetObjectByPath<Sprite>("Images/Overlords/abilityselect_hero_" + opponent.element.ToLower());

            _playerOverlordPicture.SetNativeSize();
            _opponentOverlordPicture.SetNativeSize();

            // _timerManager.AddTimer((t) =>
            //   {
            DoAnimationOfWhoseTurn();

            _timerManager.AddTimer((x) =>
            {
                _selfAnimator.SetTrigger("Exit");

                _timerManager.AddTimer((y) =>
                {
                    _uiManager.HidePopup<PlayerOrderPopup>();

                    _gameplayManager.GetController<PlayerController>().SetHand();
                    _gameplayManager.GetController<CardsController>().StartCardDistribution();
                }, null, 1.2f);

            }, null, 6f);
            //}, null, 0.9f);
        }

        private void DoAnimationOfWhoseTurn()
        {
            int turnsCount = 23;
            float rotateTime = 0.125f;
            float rotateAngle = 90f;
            RotateMode mode = RotateMode.Fast;


            float offsetPlayer = 0;
            float offsetOpponent = 0;

            if (_gameplayManager.CurrentTurnPlayer.Equals(_gameplayManager.CurrentPlayer))
                offsetOpponent = 180;
            else
                offsetPlayer = 180;

            _opponentTurnRootObject.transform.localEulerAngles = new Vector3(0, offsetOpponent, 0);
            _playerTurnRootObject.transform.localEulerAngles = new Vector3(0, offsetPlayer, 0);

            bool activeSecondOpponent = offsetOpponent > 0 ? true : false;
            bool activeSecond = offsetPlayer > 0 ? true : false;

            CheckPlayerObjects(ref activeSecond);
            CheckOpponentObjects(ref activeSecondOpponent);

            // opponent

            Sequence sequenceOpponent = DOTween.Sequence();

            for (int i = 1; i < turnsCount; i++)
            {
                int index = i;
                sequenceOpponent.Append(_opponentTurnRootObject.transform.DOLocalRotate(new Vector3(0, offsetOpponent + index * rotateAngle), rotateTime, mode));
                sequenceOpponent.AppendCallback(() =>
                {
                    if (Mathf.Abs(_opponentTurnRootObject.transform.localEulerAngles.y) - 90f == 0 ||
                        Mathf.Abs(_opponentTurnRootObject.transform.localEulerAngles.y) - 270f == 0)
                    {
                        CheckOpponentObjects(ref activeSecondOpponent);
                    }
                });
            }

            sequenceOpponent.Play();

            Sequence sequence = DOTween.Sequence();

            for (int i = 1; i < turnsCount; i++)
            {
                int index = i;
                sequence.Append(_playerTurnRootObject.transform.DOLocalRotate(new Vector3(0, offsetPlayer + index * rotateAngle), rotateTime, mode));
                sequence.AppendCallback(() =>
                {
                    if (Mathf.Abs(_playerTurnRootObject.transform.localEulerAngles.y) - 90f == 0 ||
                        Mathf.Abs(_playerTurnRootObject.transform.localEulerAngles.y) - 270f == 0)
                    {
                        CheckPlayerObjects(ref activeSecond);
                    }
                });
            }

            sequence.Play();
        }

        private void CheckPlayerObjects(ref bool activeSecond, bool ignore = false)
        {
            if (!ignore)
                activeSecond = !activeSecond;

            _playerFirstTurnObject.SetActive(!activeSecond);
            _playerCardFrontObject.SetActive(!activeSecond);
            _playerSecondTurnObject.SetActive(activeSecond);
            _playerCardBackObject.SetActive(activeSecond);


            float finalRotate = _playerTurnRootObject.transform.localEulerAngles.y;

            if (Mathf.Abs(finalRotate) >= 180)
            {
                if (activeSecond)
                {
                    _playerFirstTurnObject.transform.localScale = new Vector3(-1, 1, 1);
                    _playerCardFrontObject.transform.localScale = new Vector3(-1, 1, 1);
                    _playerSecondTurnObject.transform.localScale = Vector3.one;
                    _playerCardBackObject.transform.localScale = Vector3.one;
                }
                else
                {
                    _playerFirstTurnObject.transform.localScale = Vector3.one;
                    _playerCardFrontObject.transform.localScale = Vector3.one;
                    _playerSecondTurnObject.transform.localScale = new Vector3(-1, 1, 1);
                    _playerCardBackObject.transform.localScale = new Vector3(-1, 1, 1);
                }
            }
            else if (Mathf.Abs(finalRotate) >= 0)
            {
                if (!activeSecond)
                {
                    _playerFirstTurnObject.transform.localScale = new Vector3(-1, 1, 1);
                    _playerCardFrontObject.transform.localScale = new Vector3(-1, 1, 1);
                    _playerSecondTurnObject.transform.localScale = Vector3.one;
                    _playerCardBackObject.transform.localScale = Vector3.one;
                }
                else
                {
                    _playerFirstTurnObject.transform.localScale = Vector3.one;
                    _playerCardFrontObject.transform.localScale = Vector3.one;
                    _playerSecondTurnObject.transform.localScale = new Vector3(-1, 1, 1);
                    _playerCardBackObject.transform.localScale = new Vector3(-1, 1, 1);
                }
            }
        }

        private void CheckOpponentObjects(ref bool activeSecondOpponent, bool ignore = false)
        {
            if (!ignore)
                activeSecondOpponent = !activeSecondOpponent;

            _opponentFirstTurnObject.SetActive(!activeSecondOpponent);
            _opponentCardFrontObject.SetActive(!activeSecondOpponent);
            _opponentSecondTurnObject.SetActive(activeSecondOpponent);
            _opponentCardBackObject.SetActive(activeSecondOpponent);

            float finalRotate = _opponentTurnRootObject.transform.localEulerAngles.y;

            if (Mathf.Abs(finalRotate) >= 180)
            {
                if (activeSecondOpponent)
                {
                    _opponentFirstTurnObject.transform.localScale = new Vector3(-1, 1, 1);
                    _opponentCardFrontObject.transform.localScale = new Vector3(-1, 1, 1);
                    _opponentSecondTurnObject.transform.localScale = Vector3.one;
                    _opponentCardBackObject.transform.localScale = Vector3.one;
                }
                else
                {
                    _opponentFirstTurnObject.transform.localScale = Vector3.one;
                    _opponentCardFrontObject.transform.localScale = Vector3.one;
                    _opponentSecondTurnObject.transform.localScale = new Vector3(-1, 1, 1);
                    _opponentCardBackObject.transform.localScale = new Vector3(-1, 1, 1);
                }
            }
            else if (Mathf.Abs(finalRotate) >= 0)
            {
                if (!activeSecondOpponent)
                {
                    _opponentFirstTurnObject.transform.localScale = new Vector3(-1, 1, 1);
                    _opponentCardFrontObject.transform.localScale = new Vector3(-1, 1, 1);
                    _opponentSecondTurnObject.transform.localScale = Vector3.one;
                    _opponentCardBackObject.transform.localScale = Vector3.one;
                }
                else
                {
                    _opponentFirstTurnObject.transform.localScale = Vector3.one;
                    _opponentCardFrontObject.transform.localScale = Vector3.one;
                    _opponentSecondTurnObject.transform.localScale = new Vector3(-1, 1, 1);
                    _opponentCardBackObject.transform.localScale = new Vector3(-1, 1, 1);
                }
            }
        }
    }
}