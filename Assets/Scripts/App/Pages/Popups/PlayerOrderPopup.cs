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

        private ITutorialManager _tutorialManager;

        private Animator _playerAnimator, _opponentAnimator;

        private AnimationEventTriggering _animationEventTriggering;

        private TextMeshProUGUI _playerOverlordNameText, _opponentOverlordNameText;

        private Image _playerOverlordPicture, _opponentOverlordPicture;

        private const string _parameterName = "IsPlayerTurn";

        private const float _durationOfShow = 4f;

        public GameObject Self { get; private set; }

        public void Init()
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _uiManager = GameClient.Get<IUIManager>();
            _gameplayManager = GameClient.Get<IGameplayManager>();
            _soundManager = GameClient.Get<ISoundManager>();
            _tutorialManager = GameClient.Get<ITutorialManager>();
        }

        public void Dispose()
        {
        }

        public void Hide()
        {
            if (Self == null)
                return;

            _animationEventTriggering.AnimationEventTriggered -= AnimationEventTriggeredEventHandler;

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

            _playerAnimator = Self.transform.Find("Root/PlayerOrderCardFlip").GetComponent<Animator>();

            _opponentAnimator = Self.transform.Find("Root/OpponentOrderCardFlip").GetComponent<Animator>();


            _playerOverlordNameText = Self.transform.Find("UI_Root/UI_Over/Text_PlayerOverlordName").GetComponent<TextMeshProUGUI>();
            _opponentOverlordNameText =
                Self.transform.Find("UI_Root/UI_Over/Text_OpponentOverlordName").GetComponent<TextMeshProUGUI>();

            _playerOverlordPicture = Self.transform.Find("UI_Root/UI_Over/Image_PlayerOverlord").GetComponent<Image>();
            _opponentOverlordPicture = Self.transform.Find("UI_Root/UI/Image_Mask_OpponentOverlord/Image_OpponentOverlord").GetComponent<Image>();

            Self.SetActive(true);

            _animationEventTriggering = _playerAnimator.GetComponent<AnimationEventTriggering>();
            _animationEventTriggering.AnimationEventTriggered += AnimationEventTriggeredEventHandler;

            if (!_tutorialManager.IsTutorial ||
                (_tutorialManager.IsTutorial && !_tutorialManager.CurrentTutorialStep.ToGameplayStep().PlayerOrderScreenCloseManually))
            {
                InternalTools.DoActionDelayed(AnimationEnded, _durationOfShow);
            }
        }

        public void Show(object data)
        {
            Show();

            object[] param = (object[])data;

            ApplyInfoAboutHeroes((Hero)param[0], (Hero)param[1]);
        }

        public void Update()
        {
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
                    _tutorialManager.ReportActivityAction(Enumerators.TutorialActivityAction.EndCardFlipPlayerOrderPopup);
                    break;
                default:
                    break;
            }
        }

        public void AnimationEnded()
        {
            _uiManager.HidePopup<PlayerOrderPopup>();

            if (!_gameplayManager.IsTutorial)
            {
                if (_gameplayManager.CurrentPlayer != null)
                {
                    _gameplayManager.GetController<PlayerController>().SetHand();
                    _gameplayManager.GetController<CardsController>().StartCardDistribution();
                }
            }
            else
            {
                (_gameplayManager as GameplayManager).TutorialGameplayBeginAction?.Invoke();
            }
        }
    }
}
