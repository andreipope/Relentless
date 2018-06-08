﻿using UnityEngine;
using UnityEngine.UI;
using GrandDevs.CZB.Common;
using GrandDevs.CZB.Gameplay;
using CCGKit;
using TMPro;

namespace GrandDevs.CZB
{
    public class MainMenuPage : IUIElement
    {
		private IUIManager _uiManager;
		private ILoadObjectsManager _loadObjectsManager;
		private ILocalizationManager _localizationManager;
		private IAppStateManager _stateManager;
		private ISoundManager _soundManager;
        private IPlayerManager _playerManager;

        private GameObject _selfPage;

        private MenuButtonNoGlow //_buttonPlay,
                            _buttonCollection;
        //_buttonCredits;
        //_buttonBuy,
        //_buttonOpen;

        private Button _buttonPlay, _buttonBuy, _buttonOpen,
                       _buttonCredits;

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

            _selfPage = MonoBehaviour.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Pages/MainMenuPage"));
			_selfPage.transform.SetParent(_uiManager.Canvas.transform, false);

            _buttonPlay = _selfPage.transform.Find("Button_Play").GetComponent<Button>();
            _buttonCollection = _selfPage.transform.Find("Button_Collection").GetComponent<MenuButtonNoGlow>();
            //_buttonCredits = _selfPage.transform.Find("Button_Credits").GetComponent<MenuButtonNoGlow>();
            _buttonBuy = _selfPage.transform.Find("BuyButton").GetComponent<Button>();
            _buttonOpen = _selfPage.transform.Find("OpenButton").GetComponent<Button>();

            _packsCount = _selfPage.transform.Find("OpenButton/Count").GetComponent<TextMeshProUGUI>();

            _buttonCredits = _selfPage.transform.Find("Button_Credits").GetComponent<Button>();

            //Debug.Log(_buttonCredits);
            _buttonMusic = _selfPage.transform.Find("Button_Music").GetComponent<MenuButtonToggle>();
            _buttonSFX = _selfPage.transform.Find("Button_SFX").GetComponent<MenuButtonToggle>();

            _logoAnimator = _selfPage.transform.Find("Logo").GetComponent<Animator>();

            _buttonPlay.onClick.AddListener(OnClickPlay);
            _buttonCollection.onClickEvent.AddListener(OnClickCollection);
            //_buttonCredits.onClickEvent.AddListener(OnClickCredits);
            _buttonBuy.onClick.AddListener(BuyButtonHandler);
            _buttonOpen.onClick.AddListener(OpenButtonHandler);
            _buttonCredits.onClick.AddListener(CreditsButtonOnClickHandler);

            _buttonMusic.onValueChangedEvent.AddListener(OnValueChangedEventMusic);
            _buttonSFX.onValueChangedEvent.AddListener(OnValueChangedEventSFX);

            Hide();
        }

        public void Update()
        {
            if(Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl) && Input.GetKey(KeyCode.T))
            {
                GameClient.Get<IDataManager>().CachedUserLocalData.tutorial = true;
                GameManager.Instance.tutorial = true;
            }
        }

        public void Show()
        {
            _selfPage.SetActive(true);
            _buttonCollection.interactable = true;

            _packsCount.text = _playerManager.LocalUser.packsCount <= 99 ? _playerManager.LocalUser.packsCount.ToString() : "99";

            _logoAnimator.SetBool("LogoShow", true);

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
        public void OnClickPlay()
        {
            _soundManager.PlaySound(Common.Enumerators.SoundType.CLICK, Constants.SFX_SOUND_VOLUME, false, false, true);
            if (GameClient.Get<IDataManager>().CachedUserLocalData.tutorial)
            {
                _uiManager.HideAllPages();
                _uiManager.DrawPopup<PreparingForBattlePopup>();

                (_uiManager.GetPage<GameplayPage>() as GameplayPage).CurrentDeckId = 0;

                // small hack untill we will optimize the game because app stuck on this state.
                GameClient.Get<ITimerManager>().AddTimer((x) =>
                {
                    _stateManager.ChangeAppState(Common.Enumerators.AppState.GAMEPLAY);
                }, null, Time.deltaTime, false);
            }
            else
                _stateManager.ChangeAppState(Common.Enumerators.AppState.DECK_SELECTION);
        }
		private void OnClickCollection()
		{
            _soundManager.PlaySound(Common.Enumerators.SoundType.CLICK, Constants.SFX_SOUND_VOLUME, false, false, true);
            _stateManager.ChangeAppState(Common.Enumerators.AppState.COLLECTION);
		}
		private void OnClickCredits()
		{
            _soundManager.PlaySound(Common.Enumerators.SoundType.CLICK, Constants.SFX_SOUND_VOLUME, false, false, true);
            OpenAlertDialog("Coming Soon");
		}

        private void BuyButtonHandler()
        {
            _soundManager.PlaySound(Common.Enumerators.SoundType.CLICK, Constants.SFX_SOUND_VOLUME, false, false, true);
            _stateManager.ChangeAppState(Common.Enumerators.AppState.SHOP);
        }

        private void CreditsButtonOnClickHandler()
        {
            _soundManager.PlaySound(Common.Enumerators.SoundType.CLICK, Constants.SFX_SOUND_VOLUME, false, false, true);
            _stateManager.ChangeAppState(Common.Enumerators.AppState.CREDITS);
        }

        private void OpenButtonHandler()
        {
            _soundManager.PlaySound(Common.Enumerators.SoundType.CLICK, Constants.SFX_SOUND_VOLUME, false, false, true);
            _stateManager.ChangeAppState(Common.Enumerators.AppState.PACK_OPENER);
        }

        private void OnValueChangedEventMusic(bool value)
		{
            _soundManager.PlaySound(Common.Enumerators.SoundType.CLICK, Constants.SFX_SOUND_VOLUME, false, false, true);
            _soundManager.SetMusicVolume(value ? Constants.BACKGROUND_SOUND_VOLUME : 0);


        }
        private void OnValueChangedEventSFX(bool value)
        {
            _soundManager.PlaySound(Common.Enumerators.SoundType.CLICK, Constants.SFX_SOUND_VOLUME, false, false, true);
            _soundManager.SetSoundVolume(value ? Constants.SFX_SOUND_VOLUME : 0);
        }
        #endregion

        private void OpenAlertDialog(string msg)
		{
			_uiManager.DrawPopup<WarningPopup>(msg);
		}
    }
}