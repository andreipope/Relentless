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
        private readonly WaitForSeconds _experienceFillWait = new WaitForSeconds(1);

        private ILoadObjectsManager _loadObjectsManager;

        private IUIManager _uiManager;

        IOverlordManager _overlordManager;

        private Button _buttonOk;

        private TextMeshProUGUI _message;

        private SpriteRenderer _selectHeroSpriteRenderer;

        private Image _experienceBar;

        private TextMeshProUGUI _currentLevel;

        private TextMeshProUGUI _nextLevel;

        public GameObject Self { get; private set; }

        private Hero _heroInStartGame;

        private Hero _currentPlayerHero;

        private bool _isLevelUp;

        public void Init()
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _uiManager = GameClient.Get<IUIManager>();
            _overlordManager = GameClient.Get<IOverlordManager>();
        }

        public void Dispose()
        {
        }

        public void Hide()
        {
            GameClient.Get<ICameraManager>().FadeOut(null, 1);

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
            if (Self != null)
                return;

            Self = Object.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Popups/YouWonPopup"));
            Self.transform.SetParent(_uiManager.Canvas3.transform, false);

            _selectHeroSpriteRenderer = Self.transform.Find("Pivot/YouWonPopup/YouWonPanel/SelectHero")
                .GetComponent<SpriteRenderer>();
            _message = Self.transform.Find("Pivot/YouWonPopup/YouWonPanel/UI/Message").GetComponent<TextMeshProUGUI>();

            _buttonOk = Self.transform.Find("Pivot/YouWonPopup/YouWonPanel/UI/Button_Continue").GetComponent<Button>();
            _buttonOk.onClick.AddListener(OnClickOkButtonEventHandler);
            _buttonOk.gameObject.SetActive(false);
            _experienceBar = Self.transform.Find("Pivot/YouWonPopup/YouWonPanel/UI/ExperienceBar")
                .GetComponent<Image>();
            _currentLevel = Self.transform.Find("Pivot/YouWonPopup/YouWonPanel/UI/CurrentLevel")
                .GetComponent<TextMeshProUGUI>();
            _nextLevel = Self.transform.Find("Pivot/YouWonPopup/YouWonPanel/UI/NextLevel")
                .GetComponent<TextMeshProUGUI>();

            _message.text = "Rewards have been disabled for ver " + BuildMetaInfo.Instance.DisplayVersionName;

            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.WON_POPUP, Constants.SfxSoundVolume, false,
                false, true);
            GameClient.Get<ICameraManager>().FadeIn(0.8f, 1);
            Self.SetActive(true);

            int playerDeckId = GameClient.Get<IGameplayManager>().PlayerDeckId;
            IDataManager dataManager = GameClient.Get<IDataManager>();

            int heroId = GameClient.Get<IGameplayManager>().IsTutorial
                ? GameClient.Get<ITutorialManager>().CurrentTutorial.SpecificBattlegroundInfo.PlayerInfo.HeroId
                : dataManager.CachedDecksData.Decks.First(d => d.Id == playerDeckId).HeroId;

            _currentPlayerHero = dataManager.CachedHeroesData.Heroes[heroId];
            string heroName = _currentPlayerHero.HeroElement.ToString().ToLowerInvariant();

            _selectHeroSpriteRenderer.sprite =
                _loadObjectsManager.GetObjectByPath<Sprite>("Images/Heroes/hero_" + heroName.ToLowerInvariant());

            _currentLevel.text = _currentPlayerHero.Level.ToString();
            _nextLevel.text = (_currentPlayerHero.Level + 1).ToString();

            _heroInStartGame = _overlordManager.HeroInStartGame;

            _isLevelUp = false;

            float currentExperiencePercentage = (float)_heroInStartGame.Experience / _overlordManager.GetRequiredExperienceForNewLevel(_heroInStartGame);
            _experienceBar.fillAmount = currentExperiencePercentage;

            FillingExperinceBar();

            int index = dataManager.CachedHeroesData.Heroes.FindIndex(hero => hero.HeroId == heroId);
            if (index != -1)
            {
                dataManager.CachedHeroesData.Heroes[index] = _currentPlayerHero;
            }
        }

        private void FillingExperinceBar()
        {
            if (_currentPlayerHero.Level > _heroInStartGame.Level)
            {
                MainApp.Instance.StartCoroutine(FillExperinceBarWithLevelUp(_currentPlayerHero.Level));
            }
            else if (_currentPlayerHero.Experience > _heroInStartGame.Experience)
            {
                float updatedExperiencePercetage = (float)_currentPlayerHero.Experience
                    / _overlordManager.GetRequiredExperienceForNewLevel(_currentPlayerHero);

                MainApp.Instance.StartCoroutine(FillExperinceBar(updatedExperiencePercetage));
            }
        }

        public void Show(object data)
        {
            Show();
        }

        public void Update()
        {
        }

        private IEnumerator FillExperinceBar(float xpPercentage)
        {
            yield return _experienceFillWait;
            _experienceBar.DOFillAmount(xpPercentage, 1f);

            yield return _experienceFillWait;
            _buttonOk.gameObject.SetActive(true);

            if(_isLevelUp)
            {
                _uiManager.DrawPopup<LevelUpPopup>();
            }
        }

        private IEnumerator FillExperinceBarWithLevelUp(int currentLevel)
        {
            yield return _experienceFillWait;
            _experienceBar.DOFillAmount(1, 1f);
            
            yield return _experienceFillWait;

            _experienceBar.fillAmount = 0f;
            _currentLevel.text = currentLevel.ToString();
            _nextLevel.text = (currentLevel + 1).ToString();

            _heroInStartGame.Level++;

            _isLevelUp = true;

            FillingExperinceBar();
        }

        private void OnClickOkButtonEventHandler()
        {
            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);

            _uiManager.HidePopup<YouWonPopup>();

            if (GameClient.Get<IGameplayManager>().IsTutorial)
            {
                GameClient.Get<IMatchManager>().FinishMatch(Enumerators.AppState.PlaySelection);
            }
            else
            {
                GameClient.Get<IMatchManager>().FinishMatch(Enumerators.AppState.HordeSelection);
            }
        }
    }
}
