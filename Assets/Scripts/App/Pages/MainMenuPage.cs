using UnityEngine;
using UnityEngine.UI;
using GrandDevs.CZB.Common;
using GrandDevs.CZB.Gameplay;
using CCGKit;

namespace GrandDevs.CZB
{
    public class MainMenuPage : IUIElement
    {
		private IUIManager _uiManager;
		private ILoadObjectsManager _loadObjectsManager;
		private ILocalizationManager _localizationManager;
		private IAppStateManager _stateManager;
		private ISoundManager _soundManager;

        private GameObject _selfPage;

        private MenuButtonNoGlow _buttonPlay,
                            _buttonCollection,
                            _buttonCredits,
                            _buttonBuy,
                            _buttonOpen;

        private MenuButtonToggle _buttonMusic,
                                 _buttonSFX;


        public void Init()
        {
			_uiManager = GameClient.Get<IUIManager>();
			_loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
			_localizationManager = GameClient.Get<ILocalizationManager>();
            _stateManager = GameClient.Get<IAppStateManager>();
            _soundManager = GameClient.Get<ISoundManager>();


            _selfPage = MonoBehaviour.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Pages/MainMenuPage"));
			_selfPage.transform.SetParent(_uiManager.Canvas.transform, false);


            _buttonPlay = _selfPage.transform.Find("Button_Play").GetComponent<MenuButtonNoGlow>();
            _buttonCollection = _selfPage.transform.Find("Button_Collection").GetComponent<MenuButtonNoGlow>();
            _buttonCredits = _selfPage.transform.Find("Button_Credits").GetComponent<MenuButtonNoGlow>();
            _buttonBuy = _selfPage.transform.Find("BuyButton").GetComponent<MenuButtonNoGlow>();
            _buttonOpen = _selfPage.transform.Find("OpenButton").GetComponent<MenuButtonNoGlow>();
            

            _buttonMusic = _selfPage.transform.Find("Button_Music").GetComponent<MenuButtonToggle>();
            _buttonSFX = _selfPage.transform.Find("Button_SFX").GetComponent<MenuButtonToggle>();

            _buttonPlay.onClickEvent.AddListener(OnClickPlay);
            _buttonCollection.onClickEvent.AddListener(OnClickCollection);
            _buttonCredits.onClickEvent.AddListener(OnClickCredits);
            _buttonBuy.onClickEvent.AddListener(BuyButtonHandler);
            _buttonOpen.onClickEvent.AddListener(OpenButtonHandler);

            _buttonMusic.onValueChangedEvent.AddListener(OnValueChangedEventMusic);
            _buttonSFX.onValueChangedEvent.AddListener(OnValueChangedEventSFX);

            Hide();
        }

        public void Update()
        {
        }

        public void Show()
        {
            _selfPage.SetActive(true);
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
            _soundManager.PlaySound(Common.Enumerators.SoundType.CLICK, dropOldBackgroundMusic: false);
            if (GameClient.Get<IDataManager>().CachedUserLocalData.tutorial)
            {
                (_uiManager.GetPage<GameplayPage>() as GameplayPage).CurrentDeckId = 0;
                _stateManager.ChangeAppState(Common.Enumerators.AppState.GAMEPLAY);
            }
            else
                _stateManager.ChangeAppState(Common.Enumerators.AppState.DECK_SELECTION);
        }
		private void OnClickCollection()
		{
            _soundManager.PlaySound(Common.Enumerators.SoundType.CLICK, dropOldBackgroundMusic: false);
            _stateManager.ChangeAppState(Common.Enumerators.AppState.COLLECTION);
		}
		private void OnClickCredits()
		{
            _soundManager.PlaySound(Common.Enumerators.SoundType.CLICK, dropOldBackgroundMusic: false);
            OpenAlertDialog("Coming Soon");
		}

        private void BuyButtonHandler()
        {
            _soundManager.PlaySound(Common.Enumerators.SoundType.CLICK, dropOldBackgroundMusic: false);
            _stateManager.ChangeAppState(Common.Enumerators.AppState.SHOP);
        }

        private void OpenButtonHandler()
        {
            _soundManager.PlaySound(Common.Enumerators.SoundType.CLICK, dropOldBackgroundMusic: false);
            _stateManager.ChangeAppState(Common.Enumerators.AppState.PACK_OPENER);
        }

        private void OnValueChangedEventMusic(bool value)
		{
            _soundManager.PlaySound(Common.Enumerators.SoundType.CLICK, dropOldBackgroundMusic: false);
            _soundManager.SetMusicVolume(value ? .5f : 0);


        }
        private void OnValueChangedEventSFX(bool value)
        {
            _soundManager.PlaySound(Common.Enumerators.SoundType.CLICK, dropOldBackgroundMusic: false);
            _soundManager.SetSoundVolume(value ? 1 : 0);
        }
        #endregion

        private void OpenAlertDialog(string msg)
		{
			_uiManager.DrawPopup<WarningPopup>(msg);
		}
    }
}