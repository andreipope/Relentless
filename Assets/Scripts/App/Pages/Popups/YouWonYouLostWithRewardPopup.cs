using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using DG.Tweening;

namespace Loom.ZombieBattleground
{
    public class YouWonYouLostWithRewardPopup : IUIPopup
    {
        public GameObject Self { get; private set; }

        private IUIManager _uiManager;

        private ILoadObjectsManager _loadObjectsManager;

        private IAppStateManager _stateManager;

        private ISoundManager _soundManager;

        private ITutorialManager _tutorialManager;

        private BackendDataControlMediator _backendDataControlMediator;

        private IMatchManager _matchManager;

        private IDataManager _dataManager;

        private Button _buttonPlayAgain,
                       _buttonContinue,
                       _buttonOpenPack;

        private GameObject _groupYouWin,
                           _groupYouLost,
                           _rewardAnimation;

        private Transform _movingPanel;

        private Image _imageOverlordPortrait;

        private TextMeshProUGUI _textDeckName,
                                _textPlayerName,
                                _textLevel;

        private TextMeshPro _textPackAmount,
                            _textPackName;

        private bool _isWin;

        private int _packAmount;

        private string _packName;

        #region IUIPopup

        public void Init()
        {
            _uiManager = GameClient.Get<IUIManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _stateManager = GameClient.Get<IAppStateManager>();
            _soundManager = GameClient.Get<ISoundManager>();
            _tutorialManager = GameClient.Get<ITutorialManager>();
            _matchManager = GameClient.Get<IMatchManager>();
            _dataManager = GameClient.Get<IDataManager>();
            _backendDataControlMediator = GameClient.Get<BackendDataControlMediator>();
            _isWin = true;
        }

        public void Dispose()
        {
        }

        public void Hide()
        {
            if (Self == null)
                return;

            Self.SetActive(false);
            Object.Destroy(Self);
            Object.Destroy(_buttonOpenPack.gameObject);
            Object.Destroy(_rewardAnimation);
            Self = null;
        }

        public void SetMainPriority()
        {
        }

        public void Show()
        {
            if (Self != null)
                return;

            Self = Object.Instantiate(
                _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Popups/YouWonYouLostWithRewardPopup"));
            Self.transform.SetParent(_uiManager.Canvas2.transform, false);

            _movingPanel = Self.transform.Find("Moving_Panel");

            _groupYouWin = Self.transform.Find("Moving_Panel/Image_Panel_Win").gameObject;
            _groupYouLost = Self.transform.Find("Moving_Panel/Image_Panel_Lose").gameObject;

            _buttonPlayAgain = Self.transform.Find("Moving_Panel/Button_PlayAgain").GetComponent<Button>();
            _buttonPlayAgain.onClick.AddListener(ButtonPlayAgainHandler);

            _buttonContinue = Self.transform.Find("Moving_Panel/Button_Continue").GetComponent<Button>();
            _buttonContinue.onClick.AddListener(ButtonContinueHandler);

            _buttonOpenPack = Self.transform.Find("Moving_Panel/Button_OpenPack").GetComponent<Button>();
            _buttonOpenPack.onClick.AddListener(ButtonOpenPackHandler);
            _buttonOpenPack.transform.SetParent(_uiManager.Canvas3.transform, false);
            _buttonOpenPack.gameObject.SetActive(false);

            _groupYouWin.SetActive(_isWin);
            _groupYouLost.SetActive(!_isWin);

            Enumerators.SoundType soundType = _isWin ? Enumerators.SoundType.WON_POPUP : Enumerators.SoundType.LOST_POPUP;
            _soundManager.PlaySound(soundType, Constants.SfxSoundVolume, false, false, true);

            Deck deck = _uiManager.GetPopup<DeckSelectionPopup>().GetLastSelectedDeckFromCache();

            OverlordUserInstance overlord = _dataManager.CachedOverlordData.GetOverlordById(deck.OverlordId);

            _imageOverlordPortrait = Self.transform.Find("Moving_Panel/Image_OverlordPortrait").GetComponent<Image>();
            _imageOverlordPortrait.sprite = GetOverlordPortraitSprite
            (
                overlord.Prototype.Faction
            );

            _textDeckName = Self.transform.Find("Moving_Panel/Text_DeckName").GetComponent<TextMeshProUGUI>();
            _textPlayerName = Self.transform.Find("Moving_Panel/Group_PlayerInfo/Text_PlayerName").GetComponent<TextMeshProUGUI>();
            _textLevel = Self.transform.Find("Moving_Panel/Group_PlayerInfo/Image_Circle/Text_LevelNumber").GetComponent<TextMeshProUGUI>();

            _textPlayerName.text = _backendDataControlMediator.UserDataModel.UserId;
            _textDeckName.text = deck.Name;
            _textLevel.text = "1";

            _rewardAnimation = Object.Instantiate(
                _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Elements/Reward/PackReward"));

            _textPackAmount = _rewardAnimation.transform.Find("rewards_panel_top/tray/Text").GetComponent<TextMeshPro>();
            _textPackName = _rewardAnimation.transform.Find("rewards_panel_bottom/tray/Text").GetComponent<TextMeshPro>();

            if(_isWin)
            {
                _textPackAmount.text = "x " + _packAmount;
                _textPackName.text = _packName;
            }

            Vector3 endPosition = _rewardAnimation.transform.position;
            _rewardAnimation.transform.position = Vector3.up * endPosition.y;

            Sequence animationSequence = DOTween.Sequence();
            animationSequence.AppendInterval(2f);
            animationSequence.Append
            (
                _rewardAnimation.transform.DOMove(endPosition, 1f)
            );

            float moveOffset = _uiManager.Canvas2.GetComponent<CanvasScaler>().referenceResolution.x * 0.241f;
            _movingPanel.localPosition = Vector3.right * moveOffset;
            animationSequence.Append
            (
                _movingPanel.DOLocalMove
                (
                    -Vector3.right * moveOffset,
                    2f
                )
                .SetEase(Ease.InOutCubic)
            );

            animationSequence.OnComplete(() =>
            {
                _buttonOpenPack.transform.position = _rewardAnimation.transform.Find("buttons/Locator").position;
                _buttonOpenPack.gameObject.SetActive(true);
            });
        }

        public void Show(object data)
        {
            if (data is object[] param)
            {
                _isWin = (bool)param[0];
                if (_isWin)
                {
                    _packAmount = (int)param[1];
                    _packName = (string)param[2];
                }
            }
            Show();
        }

        public void Update()
        {
        }

        #endregion

        #region Buttons Handlers

        private void ButtonPlayAgainHandler()
        {
            PlayClickSound();
            //TODO Play Again
            if (_isWin)
            {
                ContinueOnWin();
            }
            else
            {
                ContinueOnLost();
            }
        }

        private void ButtonContinueHandler()
        {
            PlayClickSound();
            if (_isWin)
            {
                ContinueOnWin();
            }
            else
            {
                ContinueOnLost();
            }
        }

        private void ButtonOpenPackHandler()
        {
        }

        #endregion

        private void ContinueOnWin()
        {
            _uiManager.HidePopup<YouWonYouLostPopup>();
            _soundManager.StopPlaying(Enumerators.SoundType.WON_POPUP);

            if (_tutorialManager.IsTutorial)
            {
                _tutorialManager.ReportActivityAction(Enumerators.TutorialActivityAction.YouWonPopupClosed);

                _uiManager.GetPopup<TutorialProgressInfoPopup>().PopupHiding += () =>
                {
                    _matchManager.FinishMatch(Enumerators.AppState.MAIN_MENU);
                    _tutorialManager.ReportActivityAction(Enumerators.TutorialActivityAction.TutorialProgressInfoPopupClosed);
                    GameClient.Get<ITutorialManager>().StopTutorial();
                };
                _uiManager.DrawPopup<TutorialProgressInfoPopup>();
            }
            else
            {
                _matchManager.FinishMatch(Enumerators.AppState.MAIN_MENU);
            }
        }

        private void ContinueOnLost()
        {
            if(_tutorialManager.IsTutorial)
            {
                _tutorialManager.ReportActivityAction(Enumerators.TutorialActivityAction.YouLosePopupClosed);
            }

            GameClient.Get<IMatchManager>().FinishMatch(Enumerators.AppState.MAIN_MENU);

            _uiManager.HidePopup<YouWonYouLostPopup>();
            _soundManager.StopPlaying(Enumerators.SoundType.LOST_POPUP);
        }

        public Sprite GetOverlordPortraitSprite(Enumerators.Faction heroElement)
        {
            string path = "Images/UI/WinLose/OverlordPortrait/results_overlord_"+heroElement.ToString().ToLowerInvariant();
            return _loadObjectsManager.GetObjectByPath<Sprite>(path);
        }

        public void PlayClickSound()
        {
            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
        }
    }
}
