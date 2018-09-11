using DG.Tweening;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Loom.ZombieBattleground
{
    public class PlayerOrderPopup : IUIPopup
    {
        private ILoadObjectsManager _loadObjectsManager;

        private IUIManager _uiManager;

        private IGameplayManager _gameplayManager;

        private ITimerManager _timerManager;

        private ISoundManager _soundManager;

        private Animator _selfAnimator;

        private TextMeshProUGUI _playerOverlordNameText, _opponentOverlordNameText;

        private Image _playerOverlordPicture, _opponentOverlordPicture;

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

        public GameObject Self { get; private set; }

        public void Init()
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _uiManager = GameClient.Get<IUIManager>();
            _gameplayManager = GameClient.Get<IGameplayManager>();
            _timerManager = GameClient.Get<ITimerManager>();
            _soundManager = GameClient.Get<ISoundManager>();
        }

        public void Dispose()
        {
        }

        public void Hide()
        {
            _selfAnimator.StopPlayback();

            _playerCardBackObject.SetActive(true);
            _playerCardFrontObject.SetActive(false);
            _playerFirstTurnObject.SetActive(false);
            _playerSecondTurnObject.SetActive(false);

            _opponentCardBackObject.SetActive(true);
            _opponentCardFrontObject.SetActive(false);
            _opponentFirstTurnObject.SetActive(false);
            _opponentSecondTurnObject.SetActive(false);

            if (Self == null)
                return;

            Self.SetActive(false);
            Object.Destroy(Self);
            Self = null;
        }

        public void SetMainPriority()
        {
        }

        public void Show()
        {
            Self = Object.Instantiate(
                _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Popups/PlayerOrderPopup"));
            Self.transform.SetParent(_uiManager.Canvas2.transform, false);

            _selfAnimator = Self.GetComponent<Animator>();

            _playerOverlordNameText = Self.transform.Find("Text_PlayerOverlordName").GetComponent<TextMeshProUGUI>();
            _opponentOverlordNameText =
                Self.transform.Find("Text_OpponentOverlordName").GetComponent<TextMeshProUGUI>();

            _playerOverlordPicture = Self.transform.Find("Image_PlayerOverlord").GetComponent<Image>();
            _opponentOverlordPicture = Self.transform.Find("OpponentMask/Image_OpponentOverlord").GetComponent<Image>();

            _opponentTurnRootObject = Self.transform.Find("Item_OpponentOverlordTurn").gameObject;
            _opponentFirstTurnObject = Self.transform.Find("Item_OpponentOverlordTurn/Image_FirstTurn").gameObject;
            _opponentSecondTurnObject = Self.transform.Find("Item_OpponentOverlordTurn/Image_SecondTurn").gameObject;
            _opponentCardBackObject = Self.transform.Find("Item_OpponentOverlordTurn/Image_BackCard").gameObject;
            _opponentCardFrontObject = Self.transform.Find("Item_OpponentOverlordTurn/Image_FrontCard").gameObject;

            _playerTurnRootObject = Self.transform.Find("Item_PlayerOverlordTurn").gameObject;
            _playerFirstTurnObject = Self.transform.Find("Item_PlayerOverlordTurn/Image_FirstTurn").gameObject;
            _playerSecondTurnObject = Self.transform.Find("Item_PlayerOverlordTurn/Image_SecondTurn").gameObject;
            _playerCardBackObject = Self.transform.Find("Item_PlayerOverlordTurn/Image_BackCard").gameObject;
            _playerCardFrontObject = Self.transform.Find("Item_PlayerOverlordTurn/Image_FrontCard").gameObject;

            Self.SetActive(true);
            _selfAnimator.Play(0);
        }

        public void Show(object data)
        {
            Show();

            object[] param = (object[]) data;

            ApplyInfoAboutHeroes((Hero) param[0], (Hero) param[1]);
        }

        public void Update()
        {
        }

        private void ApplyInfoAboutHeroes(Hero player, Hero opponent)
        {
            _playerOverlordNameText.text = player.Name.ToUpper();
            _opponentOverlordNameText.text = opponent.Name.ToUpper();

            _playerOverlordPicture.sprite =
                _loadObjectsManager.GetObjectByPath<Sprite>("Images/Heroes/hero_" +
                    player.Element.ToLower());
            _opponentOverlordPicture.sprite =
                _loadObjectsManager.GetObjectByPath<Sprite>("Images/Heroes/hero_" +
                    opponent.Element.ToLower());

            _playerOverlordPicture.SetNativeSize();
            _opponentOverlordPicture.SetNativeSize();

            DoAnimationOfWhoseTurn();

            _timerManager.AddTimer(
                x =>
                {
                    _selfAnimator.SetTrigger("Exit");

                    _timerManager.AddTimer(
                        y =>
                        {
                            _uiManager.HidePopup<PlayerOrderPopup>();

                            _gameplayManager.GetController<PlayerController>().SetHand();
                            _gameplayManager.GetController<CardsController>().StartCardDistribution();
                        },
                        null,
                        1.2f);
                },
                null,
                6f);
        }

        private void DoAnimationOfWhoseTurn()
        {
            const int turnsCount = 23;
            const float rotateTime = 0.125f;
            const float rotateAngle = 90f;

            _playerCardBackObject.SetActive(true);
            _playerCardFrontObject.SetActive(false);
            _playerFirstTurnObject.SetActive(false);
            _playerSecondTurnObject.SetActive(false);

            _opponentCardBackObject.SetActive(true);
            _opponentCardFrontObject.SetActive(false);
            _opponentFirstTurnObject.SetActive(false);
            _opponentSecondTurnObject.SetActive(false);

            bool isFrontViewOpponent = false;
            bool isFrontViewPlayer = false;

            bool isLatestSecondOpponent = !_gameplayManager.CurrentTurnPlayer.Equals(_gameplayManager.OpponentPlayer);
            bool isLatestSecondPlayer = !_gameplayManager.CurrentTurnPlayer.Equals(_gameplayManager.CurrentPlayer);

            bool startWithSecondOpponent = !isLatestSecondOpponent;
            bool startWithSecondPlayer = !isLatestSecondPlayer;

            // opponent
            Sequence sequenceOpponent = DOTween.Sequence();

            for (int i = 1; i < turnsCount; i++)
            {
                int index = i;
                sequenceOpponent.Append(
                    _opponentTurnRootObject.transform.DOLocalRotate(new Vector3(0, index * rotateAngle), rotateTime));
                sequenceOpponent.AppendCallback(
                    () =>
                    {
                        if (Mathf.Abs(_opponentTurnRootObject.transform.localEulerAngles.y) - 90f < 45 &&
                            Mathf.Abs(_opponentTurnRootObject.transform.localEulerAngles.y) - 90f > -45 ||
                            Mathf.Abs(_opponentTurnRootObject.transform.localEulerAngles.y) - 270f < 45 &&
                            Mathf.Abs(_opponentTurnRootObject.transform.localEulerAngles.y) - 270f > -45)
                        {
                            CheckOpponentObjects(ref isFrontViewOpponent, isLatestSecondOpponent, index,
                                ref startWithSecondOpponent);
                        }
                    });
            }

            sequenceOpponent.Play();

            Sequence sequence = DOTween.Sequence();

            for (int i = 1; i < turnsCount; i++)
            {
                int index = i;
                sequence.Append(
                    _playerTurnRootObject.transform.DOLocalRotate(new Vector3(0, index * rotateAngle), rotateTime));
                sequence.AppendCallback(
                    () =>
                    {
                        if (Mathf.Abs(_playerTurnRootObject.transform.localEulerAngles.y) - 90f < 45 &&
                            Mathf.Abs(_playerTurnRootObject.transform.localEulerAngles.y) - 90f > -45 ||
                            Mathf.Abs(_playerTurnRootObject.transform.localEulerAngles.y) - 270f < 45 &&
                            Mathf.Abs(_playerTurnRootObject.transform.localEulerAngles.y) - 270f > -45)
                        {
                            CheckPlayerObjects(ref isFrontViewPlayer, ref startWithSecondPlayer);
                        }

                        if (index == turnsCount - 1)
                        {
                            _soundManager.StopPlaying(Enumerators.SoundType.CARD_DECK_TO_HAND_SINGLE);
                        }
                    });
            }

            sequence.Play();

            _soundManager.PlaySound(Enumerators.SoundType.CARD_DECK_TO_HAND_SINGLE, Constants.SfxSoundVolume, true);
        }

        private void CheckPlayerObjects(ref bool isFrontView, ref bool startWithSecondPlayer)
        {
            isFrontView = !isFrontView;

            _playerCardFrontObject.SetActive(isFrontView);
            _playerCardBackObject.SetActive(!isFrontView);

            float finalRotate = _playerTurnRootObject.transform.localEulerAngles.y;

            if (Mathf.Abs(finalRotate) >= 180)
            {
                _playerFirstTurnObject.SetActive(false);
                _playerSecondTurnObject.SetActive(false);

                if (isFrontView)
                {
                    _playerCardFrontObject.transform.localScale = new Vector3(-1, 1, 1);
                    _playerCardBackObject.transform.localScale = Vector3.one;
                }
                else
                {
                    _playerCardFrontObject.transform.localScale = Vector3.one;
                    _playerCardBackObject.transform.localScale = new Vector3(-1, 1, 1);
                }
            }
            else if (Mathf.Abs(finalRotate) >= 0)
            {
                if (startWithSecondPlayer)
                {
                    _playerFirstTurnObject.SetActive(false);
                    _playerSecondTurnObject.SetActive(true);

                    startWithSecondPlayer = false;
                }
                else
                {
                    _playerFirstTurnObject.SetActive(true);
                    _playerSecondTurnObject.SetActive(false);

                    startWithSecondPlayer = true;
                }

                if (isFrontView)
                {
                    _playerFirstTurnObject.transform.localScale = new Vector3(-1, 1, 1);
                    _playerSecondTurnObject.transform.localScale = new Vector3(-1, 1, 1);

                    _playerCardFrontObject.transform.localScale = new Vector3(-1, 1, 1);
                    _playerCardBackObject.transform.localScale = Vector3.one;
                }
                else
                {
                    _playerFirstTurnObject.transform.localScale = Vector3.one;
                    _playerSecondTurnObject.transform.localScale = Vector3.one;

                    _playerCardFrontObject.transform.localScale = Vector3.one;
                    _playerCardBackObject.transform.localScale = new Vector3(-1, 1, 1);
                }
            }
        }

        private void CheckOpponentObjects(
            ref bool isFrontView, bool isLatestSecondOpponent, int index, ref bool startWithSecondOpponent)
        {
            isFrontView = !isFrontView;

            _opponentCardFrontObject.SetActive(isFrontView);
            _opponentCardBackObject.SetActive(!isFrontView);

            float finalRotate = _opponentTurnRootObject.transform.localEulerAngles.y;

            if (Mathf.Abs(finalRotate) >= 180)
            {
                _opponentFirstTurnObject.SetActive(false);
                _opponentSecondTurnObject.SetActive(false);

                if (isFrontView)
                {
                    _opponentCardFrontObject.transform.localScale = new Vector3(-1, 1, 1);
                    _opponentCardBackObject.transform.localScale = Vector3.one;
                }
                else
                {
                    _opponentCardFrontObject.transform.localScale = Vector3.one;
                    _opponentCardBackObject.transform.localScale = new Vector3(-1, 1, 1);
                }
            }
            else if (Mathf.Abs(finalRotate) >= 0)
            {
                if (startWithSecondOpponent)
                {
                    _opponentFirstTurnObject.SetActive(false);
                    _opponentSecondTurnObject.SetActive(true);

                    startWithSecondOpponent = false;
                }
                else
                {
                    _opponentFirstTurnObject.SetActive(true);
                    _opponentSecondTurnObject.SetActive(false);

                    startWithSecondOpponent = true;
                }

                if (isFrontView)
                {
                    _opponentFirstTurnObject.transform.localScale = new Vector3(-1, 1, 1);
                    _opponentSecondTurnObject.transform.localScale = new Vector3(-1, 1, 1);

                    _opponentCardFrontObject.transform.localScale = new Vector3(-1, 1, 1);
                    _opponentCardBackObject.transform.localScale = Vector3.one;
                }
                else
                {
                    _opponentFirstTurnObject.transform.localScale = Vector3.one;
                    _opponentSecondTurnObject.transform.localScale = Vector3.one;

                    _opponentCardFrontObject.transform.localScale = Vector3.one;
                    _opponentCardBackObject.transform.localScale = new Vector3(-1, 1, 1);
                }
            }
        }
    }
}
