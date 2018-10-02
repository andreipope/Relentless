using System;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Loom.ZombieBattleground
{
    public class MainMenuPage : IUIElement
    {
        private IUIManager _uiManager;

        private ILoadObjectsManager _loadObjectsManager;

        private IAppStateManager _stateManager;

        private ISoundManager _soundManager;

        private IPlayerManager _playerManager;

        private IDataManager _dataManager;

        private GameObject _selfPage;

        private MenuButtonNoGlow _buttonArmy;

        private Button _buttonPlay, _buttonDeck;

        private ButtonShiftingContent _buttonBuy, _buttonOpen, _buttonCredits, _buttonTutorial, _buttonQuit;

        private MenuButtonToggle _buttonMusic, _buttonSfx;

        private TextMeshProUGUI _packsCount;

        private Animator _logoAnimator;

        public void Init()
        {
            _uiManager = GameClient.Get<IUIManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _stateManager = GameClient.Get<IAppStateManager>();
            _soundManager = GameClient.Get<ISoundManager>();
            _playerManager = GameClient.Get<IPlayerManager>();
            _dataManager = GameClient.Get<IDataManager>();
        }

        public void Update()
        {
        }

        public void Show()
        {
            _selfPage = Object.Instantiate(
                _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Pages/MainMenuPage"));
            _selfPage.transform.SetParent(_uiManager.Canvas.transform, false);

            _buttonPlay = _selfPage.transform.Find("Button_Play").GetComponent<Button>();
            _buttonDeck = _selfPage.transform.Find("Button_Deck").GetComponent<Button>();
            _buttonArmy = _selfPage.transform.Find("Button_Army").GetComponent<MenuButtonNoGlow>();
            _buttonCredits = _selfPage.transform.Find("BackMetalLeft2/Button_Credits")
                .GetComponent<ButtonShiftingContent>();
            _buttonQuit = _selfPage.transform.Find("BackMetalLeft/Button_Quit").GetComponent<ButtonShiftingContent>();
            _buttonTutorial = _selfPage.transform.Find("Button_Tutorial").GetComponent<ButtonShiftingContent>();
            _buttonBuy = _selfPage.transform.Find("Button_Shop").GetComponent<ButtonShiftingContent>();
            _buttonOpen = _selfPage.transform.Find("Button_OpenPacks").GetComponent<ButtonShiftingContent>();
            _packsCount = _selfPage.transform.Find("Button_OpenPacks/Count").GetComponent<TextMeshProUGUI>();
            _buttonMusic = _selfPage.transform.Find("Button_Music").GetComponent<MenuButtonToggle>();
            _buttonSfx = _selfPage.transform.Find("Button_SFX").GetComponent<MenuButtonToggle>();

            _logoAnimator = _selfPage.transform.Find("Logo").GetComponent<Animator>();

            _buttonPlay.onClick.AddListener(OnClickPlay);
            _buttonDeck.onClick.AddListener(OnClickPlay);
            _buttonArmy.Clicked.AddListener(OnClickCollection);
            _buttonBuy.onClick.AddListener(BuyButtonHandler);
            _buttonOpen.onClick.AddListener(OpenButtonHandler);
            _buttonCredits.onClick.AddListener(CreditsButtonOnClickHandler);
            _buttonQuit.onClick.AddListener(QuitButtonOnClickHandler);
            _buttonTutorial.onClick.AddListener(TutorialButtonOnClickHandler);
            _buttonMusic.ValueChanged.AddListener(OnValueChangedEventMusic);
            _buttonSfx.ValueChanged.AddListener(OnValueChangedEventSfx);

            _buttonArmy.Interactable = true;

            _packsCount.text = _playerManager.LocalUser.PacksCount <= 99 ?
                _playerManager.LocalUser.PacksCount.ToString() :
                "99";

            if (_logoAnimator.gameObject.activeInHierarchy)
            {
                _logoAnimator.SetBool("LogoShow", true);
            }

            _buttonMusic.SetStatus(!_soundManager.MusicMuted);
            _buttonSfx.SetStatus(!_soundManager.SfxMuted);

            if (!_dataManager.CachedUserLocalData.AgreedTerms)
            {
                _uiManager.DrawPopup<TermsPopup>();
            }
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

        private void OpenAlertDialog(string msg)
        {
            _uiManager.DrawPopup<WarningPopup>(msg);
        }

        #region Buttons Handlers

        private void OnClickPlay()
        {
            _soundManager.PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
            if (GameClient.Get<IDataManager>().CachedUserLocalData.Tutorial)
            {
                _uiManager.GetPage<GameplayPage>().CurrentDeckId = 0;

                GameClient.Get<IMatchManager>().FindMatch(Enumerators.MatchType.LOCAL);
            }
            else
            {
                _stateManager.ChangeAppState(Enumerators.AppState.PlaySelection);
            }
        }

        private void TutorialButtonOnClickHandler()
        {
            _soundManager.PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);

            _dataManager.CachedUserLocalData.CurrentTutorialId = 0;
            _dataManager.CachedUserLocalData.Tutorial = true;
            GameClient.Get<IGameplayManager>().IsTutorial = true;
            _uiManager.GetPage<GameplayPage>().CurrentDeckId = 0;
            GameClient.Get<IMatchManager>().FindMatch(Enumerators.MatchType.LOCAL);
        }

        private void OnClickCollection()
        {
            _soundManager.PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
            _stateManager.ChangeAppState(Enumerators.AppState.ARMY);
        }

        private void BuyButtonHandler()
        {
            _soundManager.PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
            _stateManager.ChangeAppState(Enumerators.AppState.SHOP);
        }

        private void CreditsButtonOnClickHandler()
        {
            _soundManager.PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
            _stateManager.ChangeAppState(Enumerators.AppState.CREDITS);
        }

        private void QuitButtonOnClickHandler()
        {
            Action[] actions = new Action[2];
            actions[0] = () =>
            {
                Application.Quit();
            };
            actions[1] = () => { };

            _uiManager.DrawPopup<ConfirmationPopup>(actions);
        }

        private void OpenButtonHandler()
        {
            _soundManager.PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
            _stateManager.ChangeAppState(Enumerators.AppState.PACK_OPENER);
        }

        private void OnValueChangedEventMusic(bool value)
        {
            _soundManager.PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
            _soundManager.SetMusicMuted(!value);
        }

        private void OnValueChangedEventSfx(bool value)
        {
            _soundManager.PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
            _soundManager.SetSoundMuted(!value);
        }

        #endregion

    }
}
