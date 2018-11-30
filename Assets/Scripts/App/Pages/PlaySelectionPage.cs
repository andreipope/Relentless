using System;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Loom.ZombieBattleground
{
    public class PlaySelectionPage : IUIElement
    {
        private IUIManager _uiManager;

        private ILoadObjectsManager _loadObjectsManager;

        private IAppStateManager _stateManager;

        private ISoundManager _soundManager;

        private IDataManager _dataManager;

        private GameObject _selfPage;

        private Button _backButton,
                       _buttonSoloMode,
                       _buttonPvPMode;

        private ButtonShiftingContent _buttonTutorial;

        private ILocalizationManager _localizationManager;

        private TextMeshProUGUI _soloTextMesh;
        private TextMeshProUGUI _vsTextMesh;
        private TextMeshProUGUI _tutorialTextMesh;

        public void Init()
        {
            _uiManager = GameClient.Get<IUIManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _stateManager = GameClient.Get<IAppStateManager>();
            _soundManager = GameClient.Get<ISoundManager>();
            _dataManager = GameClient.Get<IDataManager>();
            _localizationManager = GameClient.Get<ILocalizationManager>();
            _localizationManager.LanguageWasChangedEvent += LanguageWasChangedEventHandler;
        }

        public void Update()
        {
        }

        public void Show()
        {
            _selfPage = Object.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Pages/PlaySelectionPage"));
            _selfPage.transform.SetParent(_uiManager.Canvas.transform, false);

            _buttonTutorial = _selfPage.transform.Find("Button_Tutorial").GetComponent<ButtonShiftingContent>();
            _buttonSoloMode = _selfPage.transform.Find("Button_SoloMode").GetComponent<Button>();
            _buttonPvPMode = _selfPage.transform.Find("Button_PvPMode").GetComponent<Button>();
            _backButton = _selfPage.transform.Find("Button_Back").GetComponent<Button>();

            _soloTextMesh = _buttonSoloMode.transform.Find("Text").GetComponent<TextMeshProUGUI>();
            _vsTextMesh = _buttonPvPMode.transform.Find("Text").GetComponent<TextMeshProUGUI>();
            _tutorialTextMesh = _buttonTutorial.transform.Find("Text").GetComponent<TextMeshProUGUI>();

            _buttonTutorial.onClick.AddListener(TutorialButtonOnClickHandler);
            _buttonSoloMode.onClick.AddListener(SoloModeButtonOnClickHandler);
            _buttonPvPMode.onClick.AddListener(PvPModeButtonOnClickHandler);
            _backButton.onClick.AddListener(BackButtonOnClickHandler);

            _buttonPvPMode.interactable = _dataManager.ConfigData.EnablePvP;
            _buttonPvPMode.interactable = true;
#if DISABLE_PVP
            _buttonPvPMode.interactable = false;
#endif

            UpdateLocalization();
        }

        public void Hide()
        {
            if (_selfPage == null)
                return;

            _selfPage.SetActive(false);
            Object.Destroy(_selfPage);
            _selfPage = null;
        }

        public void Dispose()
        {
        }

        private void LanguageWasChangedEventHandler(Enumerators.Language obj)
        {
            UpdateLocalization();
        }

        private void UpdateLocalization()
        {
            if (_selfPage == null)
                return;

            _soloTextMesh.text = _localizationManager.GetUITranslation(LocalizationKeys.SoloText.ToString());
            _vsTextMesh.text = _localizationManager.GetUITranslation(LocalizationKeys.VsText.ToString());
            _tutorialTextMesh.text = _localizationManager.GetUITranslation(LocalizationKeys.TutorialText.ToString());
        }

        #region Buttons Handlers

        private void TutorialButtonOnClickHandler()
        {
            _soundManager.PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);

            _dataManager.CachedUserLocalData.CurrentTutorialId = 0;
            _dataManager.CachedUserLocalData.Tutorial = true;
            GameClient.Get<IGameplayManager>().IsTutorial = true;
            _uiManager.GetPage<GameplayPage>().CurrentDeckId = 0;
            GameClient.Get<IMatchManager>().FindMatch(Enumerators.MatchType.LOCAL);
        }

        private void SoloModeButtonOnClickHandler()
        {
            _soundManager.PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
            GameClient.Get<IMatchManager>().MatchType = Enumerators.MatchType.LOCAL;
            _stateManager.ChangeAppState(Enumerators.AppState.HordeSelection);
        }

        private void PvPModeButtonOnClickHandler()
        {
            _soundManager.PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
            _stateManager.ChangeAppState(Enumerators.AppState.PvPSelection);
        }

        private void BackButtonOnClickHandler()
        {
            _soundManager.PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
            _stateManager.ChangeAppState(Enumerators.AppState.MAIN_MENU);
        }

        #endregion

    }
}
