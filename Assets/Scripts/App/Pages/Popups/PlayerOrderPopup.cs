using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.Helpers;
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

        private ISoundManager _soundManager;

        private Animator _playerAnimator, _opponentAnimator;

        private AnimationEventTriggering _animationEventTriggering;

        private TextMeshProUGUI _playerOverlordNameText, _opponentOverlordNameText;

        private Image _playerOverlordPicture, _opponentOverlordPicture;

        private const string _parameterName = "IsPlayerTurn";

        private const float _durationOfShow = 4f;

        public GameObject Self { get; private set; }

        private float _rotationElapsedTime;
        private bool _isPlayerHasStartingTurn;
        private bool _areCardsRotating;
        private bool _lastPlayerTurnValue;
        private bool _lastOpponentTurnValue;

        private const float RotationSpeed = 400f;
        private const float StopRotatingCardsTime = 4f;
        private const float FinishOrderPopupTime = 7f;

        public void Init()
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _uiManager = GameClient.Get<IUIManager>();
            _gameplayManager = GameClient.Get<IGameplayManager>();
            _soundManager = GameClient.Get<ISoundManager>();
        }

        public void Dispose()
        {
        }

        public void Hide()
        {
            if (Self == null)
                return;

<<<<<<< HEAD
            _selfAnimator.StopPlayback();

            _playerCardBackObject.SetActive(true);
            _playerCardFrontObject.SetActive(false);
            _playerFirstTurnObject.SetActive(false);
            _playerSecondTurnObject.SetActive(false);

            _opponentCardBackObject.SetActive(true);
            _opponentCardFrontObject.SetActive(false);
            _opponentFirstTurnObject.SetActive(false);
            _opponentSecondTurnObject.SetActive(false);
=======
            _animationEventTriggering.AnimationEventTriggered -= AnimationEventTriggeredEventHandler;
>>>>>>> content-development

            Self.SetActive(false);
            Object.Destroy(Self);
            Self = null;
            _rotationElapsedTime = 0f;
            _areCardsRotating = false;
        }

        public void SetMainPriority()
        {
        }

        public void Show()
        {
            Self = Object.Instantiate(
                _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Popups/PlayerOrderPopup"));
            Self.transform.SetParent(_uiManager.Canvas2.transform, false);

            _playerAnimator = Self.transform.Find("Root/PlayerOrderCardFlip").GetComponent<Animator>();

            _opponentAnimator = Self.transform.Find("Root/OpponentOrderCardFlip").GetComponent<Animator>();


            _playerOverlordNameText = Self.transform.Find("UI_Root/UI_Over/Text_PlayerOverlordName").GetComponent<TextMeshProUGUI>();
            _opponentOverlordNameText =
                Self.transform.Find("UI_Root/UI_Over/Text_OpponentOverlordName").GetComponent<TextMeshProUGUI>();

            _playerOverlordPicture = Self.transform.Find("UI_Root/UI_Over/Image_PlayerOverlord").GetComponent<Image>();
            _opponentOverlordPicture = Self.transform.Find("UI_Root/UI/Image_Mask_OpponentOverlord/Image_OpponentOverlord").GetComponent<Image>();

            Self.SetActive(true);
<<<<<<< HEAD
            _selfAnimator.Play(0);

            EnableBackCard(_opponentCardBackObject, _opponentCardFrontObject, _opponentFirstTurnObject, _opponentSecondTurnObject);
            EnableBackCard(_playerCardBackObject, _playerCardFrontObject, _playerFirstTurnObject, _playerSecondTurnObject);
=======

            _animationEventTriggering = _playerAnimator.GetComponent<AnimationEventTriggering>();
            _animationEventTriggering.AnimationEventTriggered += AnimationEventTriggeredEventHandler;

            InternalTools.DoActionDelayed(AnimationEnded, _durationOfShow);
>>>>>>> content-development
        }

        public void Show(object data)
        {
            Show();

            object[] param = (object[]) data;

            ApplyInfoAboutHeroes((Hero) param[0], (Hero) param[1]);
        }

        public void Update()
        {
            DoAnimation();
        }

        private void DoAnimation()
        {
            if (Self == null)
                return;

            _rotationElapsedTime += Time.deltaTime;
            if (_areCardsRotating)
            {
                CheckPlayerObjects();
                CheckOpponentObjects();
            }
            else
            {
                if (_rotationElapsedTime >= FinishOrderPopupTime)
                {
                    _uiManager.HidePopup<PlayerOrderPopup>();

                    if (_gameplayManager.CurrentPlayer != null)
                    {
                        _gameplayManager.GetController<PlayerController>().SetHand();
                        _gameplayManager.GetController<CardsController>().StartCardDistribution();
                    }
                }
            }
        }

        private void ApplyInfoAboutHeroes(Hero player, Hero opponent)
        {
            _playerOverlordNameText.text = player.Name.ToUpperInvariant();
            _opponentOverlordNameText.text = opponent.Name.ToUpperInvariant();

            _playerOverlordPicture.sprite =
                _loadObjectsManager.GetObjectByPath<Sprite>("Images/Heroes/hero_" +
                    player.HeroElement.ToString().ToLowerInvariant());
            _opponentOverlordPicture.sprite =
                _loadObjectsManager.GetObjectByPath<Sprite>("Images/Heroes/hero_" +
                    opponent.HeroElement.ToString().ToLowerInvariant());

            _playerOverlordPicture.SetNativeSize();
            _opponentOverlordPicture.SetNativeSize();

<<<<<<< HEAD
            _rotationElapsedTime = 0f;
            _isPlayerHasStartingTurn = _gameplayManager.CurrentTurnPlayer.Equals(_gameplayManager.CurrentPlayer);
            _areCardsRotating = true;
            _lastPlayerTurnValue = _isPlayerHasStartingTurn;
            _lastOpponentTurnValue = !_isPlayerHasStartingTurn;
            _soundManager.PlaySound(Enumerators.SoundType.CARD_DECK_TO_HAND_SINGLE, Constants.SfxSoundVolume, true);
        }

        private void CheckPlayerObjects()
        {
            _playerTurnRootObject.transform.Rotate(Vector3.up * Time.deltaTime * RotationSpeed);


            if (_playerTurnRootObject.transform.localEulerAngles.y >= 90f &&
                _playerTurnRootObject.transform.localEulerAngles.y < 270f)
            {
                if(_rotationElapsedTime >= StopRotatingCardsTime  && _playerTurnRootObject.transform.localEulerAngles.y >= 90f)
                {
                    EnableFrontCard(_playerCardBackObject, _playerCardFrontObject, _playerFirstTurnObject, _playerSecondTurnObject, _isPlayerHasStartingTurn);
                    if (_playerTurnRootObject.transform.localEulerAngles.y >= 180f)
                    {
                        _soundManager.StopPlaying(Enumerators.SoundType.CARD_DECK_TO_HAND_SINGLE);
                        _areCardsRotating = false;
                    }
                }
                else
                {
                    if (!_playerCardFrontObject.activeSelf)
                    {
                        EnableFrontCard(_playerCardBackObject, _playerCardFrontObject, _playerFirstTurnObject, _playerSecondTurnObject, _lastPlayerTurnValue);
                        _lastPlayerTurnValue = !_lastPlayerTurnValue;
                    }
                }
            }
            else if (_playerTurnRootObject.transform.localEulerAngles.y >= 270f)
            {
                EnableBackCard(_playerCardBackObject, _playerCardFrontObject, _playerFirstTurnObject, _playerSecondTurnObject);
            }
        }

        private void CheckOpponentObjects()
        {
            _opponentTurnRootObject.transform.Rotate(Vector3.up * Time.deltaTime * RotationSpeed);
            if (_opponentTurnRootObject.transform.localEulerAngles.y >= 90f &&
                _opponentTurnRootObject.transform.localEulerAngles.y < 270f)
            {
                if(_rotationElapsedTime >= StopRotatingCardsTime && _opponentTurnRootObject.transform.localEulerAngles.y >= 90f)
                {
                    EnableFrontCard(_opponentCardBackObject, _opponentCardFrontObject, _opponentFirstTurnObject, _opponentSecondTurnObject, !_isPlayerHasStartingTurn);
                    if (_opponentTurnRootObject.transform.localEulerAngles.y >= 180f)
                        _areCardsRotating = false;
                }
                else
                {
                    if (!_opponentCardFrontObject.activeSelf)
                    {
                        EnableFrontCard(_opponentCardBackObject, _opponentCardFrontObject, _opponentFirstTurnObject, _opponentSecondTurnObject, _lastOpponentTurnValue);
                        _lastOpponentTurnValue = !_lastOpponentTurnValue;
                    }
                }
            }
            else if (_opponentTurnRootObject.transform.localEulerAngles.y >= 270f)
            {
                EnableBackCard(_opponentCardBackObject, _opponentCardFrontObject, _opponentFirstTurnObject, _opponentSecondTurnObject);
            }
        }

        private void EnableFrontCard(GameObject cardBackObj, GameObject cardFrontObj, GameObject firstTurnObj, GameObject secondTurnObj, bool isFirstTurn)
        {
            cardBackObj.SetActive(false);

            cardFrontObj.transform.localScale = new Vector3(-1, 1, 1);
            firstTurnObj.transform.localScale = new Vector3(-1, 1, 1);
            secondTurnObj.transform.localScale = new Vector3(-1, 1, 1);

            cardFrontObj.SetActive(true);
            firstTurnObj.SetActive(isFirstTurn);
            secondTurnObj.SetActive(!isFirstTurn);
        }

        private void EnableBackCard(GameObject cardBackObj, GameObject cardFrontObj, GameObject firstTurnObj, GameObject secondTurnObj)
        {
            cardBackObj.SetActive(true);
            cardFrontObj.SetActive(false);
            firstTurnObj.SetActive(false);
            secondTurnObj.SetActive(false);
=======
            bool isPlayerFirstTurn = _gameplayManager.CurrentTurnPlayer.Equals(_gameplayManager.CurrentPlayer);

            _playerAnimator.SetBool(_parameterName, isPlayerFirstTurn);
            _opponentAnimator.SetBool(_parameterName, !isPlayerFirstTurn);
        }

        private void AnimationEventTriggeredEventHandler(string animationName)
        {
            switch (animationName)
            {
                case "StartRotate":
                    _soundManager.PlaySound(Enumerators.SoundType.CARD_DECK_TO_HAND_SINGLE, Constants.SfxSoundVolume, true);
                    break;
                case "EndRotate":
                    _soundManager.StopPlaying(Enumerators.SoundType.CARD_DECK_TO_HAND_SINGLE);
                    break;
                default:
                    break;
            }
        }

        private void AnimationEnded()
        {
            _uiManager.HidePopup<PlayerOrderPopup>();

            if (_gameplayManager.CurrentPlayer != null)
            {
                _gameplayManager.GetController<PlayerController>().SetHand();
                _gameplayManager.GetController<CardsController>().StartCardDistribution();
            }
>>>>>>> content-development
        }
    }
}
