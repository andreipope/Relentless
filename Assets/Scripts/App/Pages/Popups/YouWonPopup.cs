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

        private Button _buttonOk;

        private TextMeshProUGUI _message;

        private SpriteRenderer _selectHeroSpriteRenderer;

        private Image _experienceBar;

        private TextMeshProUGUI _currentLevel;

        private TextMeshProUGUI _nextLevel;

        public GameObject Self { get; private set; }

        public void Init()
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _uiManager = GameClient.Get<IUIManager>();
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

            Hero currentPlayerHero = dataManager.CachedHeroesData.Heroes[heroId];
            string heroName = currentPlayerHero.HeroElement.ToString().ToLowerInvariant();
            _selectHeroSpriteRenderer.sprite =
                _loadObjectsManager.GetObjectByPath<Sprite>("Images/Heroes/hero_" + heroName.ToLowerInvariant());

            // TODO : instead of 1000, should be a value accordint to Level
            // TODO : instead of 400, should be how much player experinece on wining game
            _currentLevel.text = currentPlayerHero.Level.ToString();
            _nextLevel.text = (currentPlayerHero.Level + 1).ToString();
            float currentExperiencePercentage = (float) currentPlayerHero.Experience / 1000;
            _experienceBar.fillAmount = currentExperiencePercentage;
            GameClient.Get<IOverlordManager>().ChangeExperience(currentPlayerHero, 400);
            float updatedExperiencePercetage = (float) currentPlayerHero.Experience / 1000;

            if (updatedExperiencePercetage < currentExperiencePercentage)
            {
                MainApp.Instance.StartCoroutine(FillExperinceBarWithLevelUp(updatedExperiencePercetage,
                    currentPlayerHero.Level));
            }
            else
            {
                MainApp.Instance.StartCoroutine(FillExperinceBar(updatedExperiencePercetage));
            }

            // save to data manager cached hero list
            int index = dataManager.CachedHeroesData.Heroes.FindIndex(hero => hero.HeroId == heroId);
            if (index != -1)
            {
                dataManager.CachedHeroesData.Heroes[index] = currentPlayerHero;
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
        }

        private IEnumerator FillExperinceBarWithLevelUp(float xpPercentage, int currentLevel)
        {
            yield return _experienceFillWait;
            _experienceBar.DOFillAmount(1, 1f);

            // show level up pop up or something
            yield return _experienceFillWait;

            _experienceBar.fillAmount = 0f;
            _currentLevel.text = currentLevel.ToString();
            _nextLevel.text = (currentLevel + 1).ToString();

            yield return _experienceFillWait;
            _experienceBar.DOFillAmount(xpPercentage, 1f);

            yield return _experienceFillWait;
            _buttonOk.gameObject.SetActive(true);
        }

        private void OnClickOkButtonEventHandler()
        {
            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);

            _uiManager.HidePopup<YouWonPopup>();

            GameClient.Get<IMatchManager>().FinishMatch(Enumerators.AppState.HordeSelection);
        }
    }
}
