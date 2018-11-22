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

            _selfAnimator.StopPlayback();

            _playerCardBackObject.SetActive(true);
            _playerCardFrontObject.SetActive(false);
            _playerFirstTurnObject.SetActive(false);
            _playerSecondTurnObject.SetActive(false);

            _opponentCardBackObject.SetActive(true);
            _opponentCardFrontObject.SetActive(false);
            _opponentFirstTurnObject.SetActive(false);
            _opponentSecondTurnObject.SetActive(false);

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

            _selfAnimator = Self.transform.Find("Root").GetComponent<Animator>();

            _playerOverlordNameText = Self.transform.Find("Root/Text_PlayerOverlordName").GetComponent<TextMeshProUGUI>();
            _opponentOverlordNameText =
                Self.transform.Find("Root/Text_OpponentOverlordName").GetComponent<TextMeshProUGUI>();

            _playerOverlordPicture = Self.transform.Find("Root/Image_PlayerOverlord").GetComponent<Image>();
            _opponentOverlordPicture = Self.transform.Find("Root/Image_Mask_OpponentOverlord/Image_OpponentOverlord").GetComponent<Image>();

            _opponentTurnRootObject = Self.transform.Find("Root/Item_OpponentOverlordTurn").gameObject;
            _opponentFirstTurnObject = Self.transform.Find("Root/Item_OpponentOverlordTurn/Image_FirstTurn").gameObject;
            _opponentSecondTurnObject = Self.transform.Find("Root/Item_OpponentOverlordTurn/Image_SecondTurn").gameObject;
            _opponentCardBackObject = Self.transform.Find("Root/Item_OpponentOverlordTurn/Image_BackCard").gameObject;
            _opponentCardFrontObject = Self.transform.Find("Root/Item_OpponentOverlordTurn/Image_FrontCard").gameObject;

            _playerTurnRootObject = Self.transform.Find("Root/Item_PlayerOverlordTurn").gameObject;
            _playerFirstTurnObject = Self.transform.Find("Root/Item_PlayerOverlordTurn/Image_FirstTurn").gameObject;
            _playerSecondTurnObject = Self.transform.Find("Root/Item_PlayerOverlordTurn/Image_SecondTurn").gameObject;
            _playerCardBackObject = Self.transform.Find("Root/Item_PlayerOverlordTurn/Image_BackCard").gameObject;
            _playerCardFrontObject = Self.transform.Find("Root/Item_PlayerOverlordTurn/Image_FrontCard").gameObject;

            Self.SetActive(true);
            _selfAnimator.Play(0);

            EnableBackCard(false);
            EnableBackCard(true);
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
                    EnableFrontCard(true, _isPlayerHasStartingTurn);
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
                        EnableFrontCard(true, _lastPlayerTurnValue);
                        _lastPlayerTurnValue = !_lastPlayerTurnValue;
                    }
                }
            }
            else if (_playerTurnRootObject.transform.localEulerAngles.y >= 270f)
            {
                EnableBackCard(true);
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
                    EnableFrontCard(false, !_isPlayerHasStartingTurn);
                    if (_opponentTurnRootObject.transform.localEulerAngles.y >= 180f)
                        _areCardsRotating = false;
                }
                else
                {
                    if (!_opponentCardFrontObject.activeSelf)
                    {
                        EnableFrontCard(false, _lastOpponentTurnValue);
                        _lastOpponentTurnValue = !_lastOpponentTurnValue;
                    }
                }
            }
            else if (_opponentTurnRootObject.transform.localEulerAngles.y >= 270f)
            {
                EnableBackCard(false);
            }
        }

        private void EnableFrontCard(bool isPlayer, bool isFirstTurn)
        {
            if(isPlayer)
            {
                _playerCardBackObject.SetActive(false);

                _playerCardFrontObject.transform.localScale = new Vector3(-1, 1, 1);
                _playerFirstTurnObject.transform.localScale = new Vector3(-1, 1, 1);
                _playerSecondTurnObject.transform.localScale = new Vector3(-1, 1, 1);

                _playerCardFrontObject.SetActive(true);
                _playerFirstTurnObject.SetActive(isFirstTurn);
                _playerSecondTurnObject.SetActive(!isFirstTurn);
            }
            else
            {
                _opponentCardBackObject.SetActive(false);
                _opponentCardFrontObject.SetActive(true);

                _opponentCardFrontObject.transform.localScale = new Vector3(-1, 1, 1);
                _opponentFirstTurnObject.transform.localScale = new Vector3(-1, 1, 1);
                _opponentSecondTurnObject.transform.localScale = new Vector3(-1, 1, 1);

                _opponentFirstTurnObject.SetActive(isFirstTurn);
                _opponentSecondTurnObject.SetActive(!isFirstTurn);
            }
        }

        private void EnableBackCard(bool isPlayer)
        {
            if(isPlayer)
            {
                _playerCardBackObject.SetActive(true);
                _playerCardFrontObject.SetActive(false);
                _playerFirstTurnObject.SetActive(false);
                _playerSecondTurnObject.SetActive(false);
            }
            else
            {
                _opponentCardBackObject.SetActive(true);
                _opponentCardFrontObject.SetActive(false);
                _opponentFirstTurnObject.SetActive(false);
                _opponentSecondTurnObject.SetActive(false);
            }
        }
    }
}
