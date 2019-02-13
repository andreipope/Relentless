using System.Collections;
using System.Linq;
using DG.Tweening;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Loom.ZombieBattleground
{
    public class YouWonPopup : IUIPopup
    {
        private static Color ColorDisabledContinueButtonForTutorial = new Color32(159, 159, 159, 225);

        private readonly WaitForSeconds _experienceFillWait = new WaitForSeconds(1);

        private ILoadObjectsManager _loadObjectsManager;

        private IUIManager _uiManager;

        private IOverlordExperienceManager _overlordManager;

        private IDataManager _dataManager;

        private ISoundManager _soundManager;

        private IGameplayManager _gameplayManager;

        private ITutorialManager _tutorialManager;

        private IMatchManager _matchManager;

        private ICameraManager _cameraManager;

        private Button _buttonOk;

        private Button _packOpenButton;

        private Image _openPacksImage;

        private TextMeshProUGUI _message;

        private SpriteRenderer _selectHeroSpriteRenderer;

        private Image _experienceBar;

        private TextMeshProUGUI _currentLevel;

        private TextMeshProUGUI _nextLevel;

        private TextMeshProUGUI _continueText;

        public GameObject Self { get; private set; }

        private Hero _currentPlayerHero;

        private bool _isLevelUp;
        private Coroutine _fillExperienceBarCoroutine;

        public void Init()
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _uiManager = GameClient.Get<IUIManager>();
            _overlordManager = GameClient.Get<IOverlordExperienceManager>();
            _dataManager = GameClient.Get<IDataManager>();
            _soundManager = GameClient.Get<ISoundManager>();
            _gameplayManager = GameClient.Get<IGameplayManager>();
            _tutorialManager = GameClient.Get<ITutorialManager>();
            _matchManager = GameClient.Get<IMatchManager>();
            _cameraManager = GameClient.Get<ICameraManager>();
        }

        public void Dispose()
        {
        }

        public void Hide()
        {
            _cameraManager.FadeOut(null, 1);

            if (Self == null)
                return;

            if (_fillExperienceBarCoroutine != null)
            {
                MainApp.Instance.StopCoroutine(_fillExperienceBarCoroutine);
            }

            Self.SetActive(false);
            Object.Destroy(Self);
            Self = null;
        }

        public void SetMainPriority()
        {
        }

        public void Show()
        {
            if (Self != null)
                return;

            Self = Object.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Popups/YouWonPopup"));
            Self.transform.SetParent(_uiManager.Canvas3.transform, false);

            _selectHeroSpriteRenderer = Self.transform.Find("Pivot/YouWonPopup/YouWonPanel/SelectHero")
                .GetComponent<SpriteRenderer>();
            _message = Self.transform.Find("Pivot/YouWonPopup/YouWonPanel/UI/Message").GetComponent<TextMeshProUGUI>();

            _buttonOk = Self.transform.Find("Pivot/YouWonPopup/YouWonPanel/UI/Panel_Buttons/Button_Continue").GetComponent<Button>();
            _buttonOk.onClick.AddListener(OnClickOkButtonEventHandler);
            _buttonOk.gameObject.SetActive(false);

            _continueText = _buttonOk.transform.Find("Shifted/Text").GetComponent<TextMeshProUGUI>();

            _packOpenButton = Self.transform.Find("Pivot/YouWonPopup/YouWonPanel/UI/Panel_Buttons/Button_OpenPacks").GetComponent<Button>();
            _packOpenButton.onClick.AddListener(OpenPackButtonOnClickHandler);

            _openPacksImage = Self.transform.Find("Pivot/YouWonPopup/YouWonPanel/UI/Image_OpenPacks").GetComponent<Image>();

            _experienceBar = Self.transform.Find("Pivot/YouWonPopup/YouWonPanel/UI/ExperienceBar")
                .GetComponent<Image>();
            _currentLevel = Self.transform.Find("Pivot/YouWonPopup/YouWonPanel/UI/CurrentLevel")
                .GetComponent<TextMeshProUGUI>();
            _nextLevel = Self.transform.Find("Pivot/YouWonPopup/YouWonPanel/UI/NextLevel")
                .GetComponent<TextMeshProUGUI>();

            _message.text = "Rewards have been disabled for ver " + BuildMetaInfo.Instance.DisplayVersionName;

            _soundManager.PlaySound(Enumerators.SoundType.WON_POPUP, Constants.SfxSoundVolume, false, false, true);

            _cameraManager.FadeIn(0.8f, 1);

            Self.SetActive(true);

            int heroId = _gameplayManager.IsTutorial
                ? _tutorialManager.CurrentTutorial.TutorialContent.ToGameplayContent().SpecificBattlegroundInfo.PlayerInfo.OverlordId :
                  _gameplayManager.CurrentPlayerDeck.HeroId;

            _currentPlayerHero = _dataManager.CachedHeroesData.Heroes[heroId];
            string heroName = _currentPlayerHero.HeroElement.ToString().ToLowerInvariant();

            _selectHeroSpriteRenderer.sprite =
                _loadObjectsManager.GetObjectByPath<Sprite>("Images/Heroes/hero_" + heroName.ToLowerInvariant());

            _overlordManager.ApplyExperienceFromMatch(_currentPlayerHero);

            _currentLevel.text = (_overlordManager.MatchExperienceInfo.LevelAtBegin).ToString();
            _nextLevel.text = (_overlordManager.MatchExperienceInfo.LevelAtBegin + 1).ToString();

            _isLevelUp = false;

            float currentExperiencePercentage = (float)_overlordManager.MatchExperienceInfo.ExperienceAtBegin /
                                                _overlordManager.GetRequiredExperienceForNewLevel(_currentPlayerHero);
            _experienceBar.fillAmount = currentExperiencePercentage;

            FillingExperienceBar();

            if(_tutorialManager.IsTutorial)
            {
                _tutorialManager.ReportActivityAction(Enumerators.TutorialActivityAction.YouWonPopupOpened);
                if(_tutorialManager.CurrentTutorial.Id == 0)
                {
                    _message.gameObject.SetActive(false);
                }
                else if( _tutorialManager.CurrentTutorial.Id == Constants.LastTutorialId)
                {
                    _message.text = "Congratulations!\nThe reward will be\nautomatically claimed..";
                }
            }
        }

        private void FillingExperienceBar()
        {
            if (_currentPlayerHero.Level > _overlordManager.MatchExperienceInfo.LevelAtBegin)
            {
                _fillExperienceBarCoroutine = MainApp.Instance.StartCoroutine(FillExperienceBarWithLevelUp(_currentPlayerHero.Level));
            }
            else if (_currentPlayerHero.Experience > _overlordManager.MatchExperienceInfo.ExperienceAtBegin)
            {
                float updatedExperiencePercetage = (float)_currentPlayerHero.Experience
                    / _overlordManager.GetRequiredExperienceForNewLevel(_currentPlayerHero);

                _fillExperienceBarCoroutine = MainApp.Instance.StartCoroutine(FillExperienceBar(updatedExperiencePercetage));
            }
            else
            {
                _buttonOk.gameObject.SetActive(true);
            }
        }

        private void EnablePackOpenerPart()
        {
            _packOpenButton.gameObject.SetActive(true);
            _openPacksImage.gameObject.SetActive(true);
            _buttonOk.interactable = false;
            _buttonOk.targetGraphic.color = ColorDisabledContinueButtonForTutorial;
            _continueText.color = ColorDisabledContinueButtonForTutorial;
        }

        public void Show(object data)
        {
            Show();
        }

        public void Update()
        {
        }

        private IEnumerator FillExperienceBar(float xpPercentage)
        {
            yield return _experienceFillWait;
            _experienceBar.DOFillAmount(xpPercentage, 1f);

            yield return _experienceFillWait;
            _buttonOk.gameObject.SetActive(true);

            if (_isLevelUp)
            {
                _uiManager.DrawPopup<LevelUpPopup>();
            }
        }

        private IEnumerator FillExperienceBarWithLevelUp(int currentLevel)
        {
            yield return _experienceFillWait;
            _experienceBar.DOFillAmount(1, 1f);

            yield return _experienceFillWait;

            _overlordManager.MatchExperienceInfo.LevelAtBegin++;

            _experienceBar.fillAmount = 0f;
            _currentLevel.text = _overlordManager.MatchExperienceInfo.LevelAtBegin.ToString();
            _nextLevel.text = (_overlordManager.MatchExperienceInfo.LevelAtBegin + 1).ToString();

            _isLevelUp = true;

            FillingExperienceBar();
        }

        private void OnClickOkButtonEventHandler()
        {
            _soundManager.PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);

            _uiManager.HidePopup<YouWonPopup>();

            if (_tutorialManager.IsTutorial)
            {
                _tutorialManager.ReportActivityAction(Enumerators.TutorialActivityAction.YouWonPopupClosed);

                _uiManager.GetPopup<TutorialProgressInfoPopup>().PopupHiding += () =>
                {
                    _matchManager.FinishMatch(Enumerators.AppState.MAIN_MENU);
                    _tutorialManager.ReportActivityAction(Enumerators.TutorialActivityAction.TutorialProgressInfoPopupClosed);
                    GameClient.Get<ITutorialManager>().StopTutorial();
                    if (_tutorialManager.CurrentTutorial.Id == Constants.LastTutorialId && !_dataManager.CachedUserLocalData.TutorialRewardClaimed)
                    {
                        GameClient.Get<TutorialRewardManager>().CallRewardTutorialFlow();
                    } 
                };
                _uiManager.DrawPopup<TutorialProgressInfoPopup>();
            }
            else
            {
                _matchManager.FinishMatch(Enumerators.AppState.HordeSelection);
            }
        }

        private void OpenPackButtonOnClickHandler()
        {
            _soundManager.PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
            _uiManager.HidePopup<YouWonPopup>();

            if (_tutorialManager.IsTutorial)
            {
                _tutorialManager.ReportActivityAction(Enumerators.TutorialActivityAction.YouWonPopupClosed);

                _uiManager.GetPopup<TutorialProgressInfoPopup>().PopupHiding += () =>
                {
                    _matchManager.FinishMatch(Enumerators.AppState.PlaySelection);
                    _tutorialManager.ReportActivityAction(Enumerators.TutorialActivityAction.TutorialProgressInfoPopupClosed);
                };
                _uiManager.DrawPopup<TutorialProgressInfoPopup>();
            }
        }
    }
}
