// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



using UnityEngine;
using UnityEngine.UI;
using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Gameplay;
using TMPro;

namespace LoomNetwork.CZB
{
    public class MainMenuPage : IUIElement
    {
		private IUIManager _uiManager;
		private ILoadObjectsManager _loadObjectsManager;
		private ILocalizationManager _localizationManager;
		private IAppStateManager _stateManager;
		private ISoundManager _soundManager;
        private IPlayerManager _playerManager;
		private IDataManager _dataManager;

        private GameObject _selfPage;

        private MenuButtonNoGlow _buttonArmy;

        private Button _buttonPlay, _buttonDeck;

        private ButtonShiftingContent _buttonBuy, _buttonOpen,
                       _buttonCredits, _buttonTutorial, _buttonQuit;

        private MenuButtonToggle _buttonMusic,
                                 _buttonSFX;

        private TextMeshProUGUI _packsCount;

        private Animator _logoAnimator;

        private bool _logoShowed;

        public void Init()
        {
			_uiManager = GameClient.Get<IUIManager>();
			_loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
			_localizationManager = GameClient.Get<ILocalizationManager>();
            _stateManager = GameClient.Get<IAppStateManager>();
            _soundManager = GameClient.Get<ISoundManager>();
            _playerManager = GameClient.Get<IPlayerManager>();
			_dataManager = GameClient.Get<IDataManager> ();

            _selfPage = MonoBehaviour.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Pages/MainMenuPage"));
			_selfPage.transform.SetParent(_uiManager.Canvas.transform, false);

            _buttonPlay = _selfPage.transform.Find("Button_Play").GetComponent<Button>();
            _buttonDeck = _selfPage.transform.Find("Button_Deck").GetComponent<Button>();
            _buttonArmy = _selfPage.transform.Find("Button_Army").GetComponent<MenuButtonNoGlow>();
            _buttonCredits = _selfPage.transform.Find("Button_Credits").GetComponent<ButtonShiftingContent>();
            _buttonQuit = _selfPage.transform.Find("Button_Quit").GetComponent<ButtonShiftingContent>();
            _buttonTutorial = _selfPage.transform.Find("Button_Tutorial").GetComponent<ButtonShiftingContent>();
            _buttonBuy = _selfPage.transform.Find("Button_Shop").GetComponent<ButtonShiftingContent>();
            _buttonOpen = _selfPage.transform.Find("Button_OpenPacks").GetComponent<ButtonShiftingContent>();
            _packsCount = _selfPage.transform.Find("Button_OpenPacks/Count").GetComponent<TextMeshProUGUI>();
            _buttonMusic = _selfPage.transform.Find("Button_Music").GetComponent<MenuButtonToggle>();
            _buttonSFX = _selfPage.transform.Find("Button_SFX").GetComponent<MenuButtonToggle>();

            _logoAnimator = _selfPage.transform.Find("Logo").GetComponent<Animator>();

            _buttonPlay.onClick.AddListener(OnClickPlay);
            _buttonDeck.onClick.AddListener(DeckButtonOnClickHandler);
            _buttonArmy.onClickEvent.AddListener(OnClickCollection);
            _buttonBuy.onClick.AddListener(BuyButtonHandler);
            _buttonOpen.onClick.AddListener(OpenButtonHandler);
            _buttonCredits.onClick.AddListener(CreditsButtonOnClickHandler);
            _buttonQuit.onClick.AddListener(QuitButtonOnClickHandler);
            _buttonTutorial.onClick.AddListener(TutorialButtonOnClickHandler);
            
            _buttonMusic.onValueChangedEvent.AddListener(OnValueChangedEventMusic);
            _buttonSFX.onValueChangedEvent.AddListener(OnValueChangedEventSFX);

            Hide();
        }

        public void Update()
        {
            if(Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl) && Input.GetKey(KeyCode.T))
            {
                GameClient.Get<IDataManager>().CachedUserLocalData.tutorial = true;

                GameClient.Get<IGameplayManager>().IsTutorial = true;
            }

            /*  FOR TESTING
            if(Input.GetKeyUp(KeyCode.Z))
            {
                Debug.Log("BATTLEGROUND");
                _soundManager.CrossfaidSound(Enumerators.SoundType.BATTLEGROUND, null, true);
            }
            if (Input.GetKeyUp(KeyCode.X))
            {
                Debug.Log("BACKGROUND");
                _soundManager.CrossfaidSound(Enumerators.SoundType.BACKGROUND, null, true);
            }
            */
        }

        public void Show()
        {
            _selfPage.SetActive(true);
            _buttonArmy.interactable = true;

            _packsCount.text = _playerManager.LocalUser.packsCount <= 99 ? _playerManager.LocalUser.packsCount.ToString() : "99";

            _logoAnimator.SetBool("LogoShow", true);

            _buttonMusic.SetStatus(!_soundManager.MusicMuted);
            _buttonSFX.SetStatus(!_soundManager.SfxMuted);

			if (!_dataManager.CachedUserLocalData.agreedTerms) {
				_uiManager.DrawPopup<TermsPopup> ();
			}

            /*if (_logoShowed && !_logoAnimator.GetBool("LogoShow"))
                _logoAnimator.SetBool("LogoShow", true);

            if (!_logoShowed)
            {
                GameClient.Get<ISoundManager>().PlaySound(Common.Enumerators.SoundType.LOGO_APPEAR);
                _logoShowed = true;
            } */
        }

        public void Hide()
        {
            _selfPage.SetActive(false);
        }

        public void Dispose()
        {
            
        }

        #region Buttons Handlers
        private void DeckButtonOnClickHandler()
        {
            _soundManager.PlaySound(Common.Enumerators.SoundType.CLICK, Constants.SFX_SOUND_VOLUME, false, false, true);

            _stateManager.ChangeAppState(Common.Enumerators.AppState.DECK_SELECTION);
        }

        private void OnClickPlay()
        {
            _soundManager.PlaySound(Common.Enumerators.SoundType.CLICK, Constants.SFX_SOUND_VOLUME, false, false, true);
            if (GameClient.Get<IDataManager>().CachedUserLocalData.tutorial)
            {
                (_uiManager.GetPage<GameplayPage>() as GameplayPage).CurrentDeckId = 0;

                GameClient.Get<IMatchManager>().FindMatch(Enumerators.MatchType.LOCAL);
            }
            else
                _stateManager.ChangeAppState(Common.Enumerators.AppState.DECK_SELECTION);
        }

        private void TutorialButtonOnClickHandler()
        {
            _soundManager.PlaySound(Common.Enumerators.SoundType.CLICK, Constants.SFX_SOUND_VOLUME, false, false, true);
            GameClient.Get<IDataManager>().CachedUserLocalData.tutorial = true;
            GameClient.Get<IGameplayManager>().IsTutorial = true;
            (_uiManager.GetPage<GameplayPage>() as GameplayPage).CurrentDeckId = 0;
            GameClient.Get<IMatchManager>().FindMatch(Enumerators.MatchType.LOCAL);
        }

		private void OnClickCollection()
		{
            _soundManager.PlaySound(Common.Enumerators.SoundType.CLICK, Constants.SFX_SOUND_VOLUME, false, false, true);
            _stateManager.ChangeAppState(Common.Enumerators.AppState.COLLECTION);
		}

        private void BuyButtonHandler()
        {
            _soundManager.PlaySound(Common.Enumerators.SoundType.CLICK, Constants.SFX_SOUND_VOLUME, false, false, true);
			_stateManager.ChangeAppState (Common.Enumerators.AppState.SHOP);
        }

        private void CreditsButtonOnClickHandler()
        {
            _soundManager.PlaySound(Common.Enumerators.SoundType.CLICK, Constants.SFX_SOUND_VOLUME, false, false, true);
            _stateManager.ChangeAppState(Common.Enumerators.AppState.CREDITS);
        }

        private void QuitButtonOnClickHandler()
        {
            Application.Quit();
        }

        private void OpenButtonHandler()
        {
            _soundManager.PlaySound(Common.Enumerators.SoundType.CLICK, Constants.SFX_SOUND_VOLUME, false, false, true);
			_stateManager.ChangeAppState (Common.Enumerators.AppState.PACK_OPENER);
        }

        private void OnValueChangedEventMusic(bool value)
		{
            _soundManager.PlaySound(Common.Enumerators.SoundType.CLICK, Constants.SFX_SOUND_VOLUME, false, false, true);
           // _soundManager.SetMusicVolume(value ? Constants.BACKGROUND_SOUND_VOLUME : 0);
            _soundManager.SetMusicMuted(!value);
        }

        private void OnValueChangedEventSFX(bool value)
        {
            _soundManager.PlaySound(Common.Enumerators.SoundType.CLICK, Constants.SFX_SOUND_VOLUME, false, false, true);
          //  _soundManager.SetSoundVolume(value ? Constants.SFX_SOUND_VOLUME : 0);
            _soundManager.SetSoundMuted(!value);
        }
        #endregion

        private void OpenAlertDialog(string msg)
		{
			_uiManager.DrawPopup<WarningPopup>(msg);
		}
    }
}