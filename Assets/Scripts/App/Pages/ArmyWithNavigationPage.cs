using System;
using log4net;
using Loom.ZombieBattleground.Common;
using OneOf;
using OneOf.Types;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Loom.ZombieBattleground
{
    public class ArmyWithNavigationPage : IUIElement
    {
        private static readonly ILog Log = Logging.GetLog(nameof(ArmyWithNavigationPage));

        private IUIManager _uiManager;
        private ILoadObjectsManager _loadObjectsManager;
        private BackendDataSyncService _backendDataSyncService;

        private Button _buttonBuyPacks;
        private Button _buttonMarketplace;
        private Button _buttonLeftArrowScroll;
        private Button _buttonRightArrowScroll;

        private Scrollbar _cardCollectionScrollBar;

        private UICardCollections _uiCardCollections;

        private FadeoutBars _fadeoutBars;

        private GameObject _selfPage;

        #region IUIElement

        public void Init()
        {
            _uiManager = GameClient.Get<IUIManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _backendDataSyncService = GameClient.Get<BackendDataSyncService>();

            _uiCardCollections = new UICardCollections();
            _uiCardCollections.Init();
        }

        public async void Show()
        {
            GameObject armyPage = Object.Instantiate(
                    _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Pages/MyCardsPage"),
                    _uiManager.Canvas.transform,
                    false);

            _selfPage = armyPage;

            _uiManager.DrawPopup<SideMenuPopup>(SideMenuPopup.MENU.MY_CARDS);
            _uiManager.HidePopup<AreaBarPopup>();

            if (_backendDataSyncService.IsCollectionDataDirty)
            {
                OneOf<Success, Exception> result = await _backendDataSyncService.UpdateCardCollectionWithUi(false);
                if (result.IsT1)
                {
                    Log.Warn(result.AsT1);

                    FailAndGoToMainMenu("Failed to update card collection. Please try again.");
                    return;
                }
            }

            _uiCardCollections.Show(_selfPage, Enumerators.CardCollectionPageType.Army);
            _selfPage.transform.SetParent(_uiManager.Canvas.transform, false);

            _uiManager.DrawPopup<SideMenuPopup>(SideMenuPopup.MENU.MY_CARDS);

            _buttonMarketplace = _selfPage.transform.Find("Panel_Frame/Upper_Items/Button_MarketPlace").GetComponent<Button>();
            _buttonMarketplace.onClick.AddListener(ButtonMarketplace);

            _buttonBuyPacks = _selfPage.transform.Find("Panel_Frame/Lower_Items/Button_BuyMorePacks").GetComponent<Button>();
            _buttonBuyPacks.onClick.AddListener(ButtonBuyPacksHandler);

            _buttonLeftArrowScroll = _selfPage.transform.Find("Panel_Frame/Panel_Content/Army/Element/Button_LeftArrow").GetComponent<Button>();
            _buttonLeftArrowScroll.onClick.AddListener(ButtonLeftArrowScrollHandler);

            _buttonRightArrowScroll = _selfPage.transform.Find("Panel_Frame/Panel_Content/Army/Element/Button_RightArrow").GetComponent<Button>();
            _buttonRightArrowScroll.onClick.AddListener(ButtonRightArrowScrollHandler);

            _cardCollectionScrollBar = _selfPage.transform.Find("Panel_Frame/Panel_Content/Army/Element/Scroll View").GetComponent<ScrollRect>().horizontalScrollbar;

            GameObject leftFadeGameObject = _selfPage.transform.Find("Panel_Frame/Panel_Content/Army/Element/Fade_Left").gameObject;
            GameObject rightFadeGameObject = _selfPage.transform.Find("Panel_Frame/Panel_Content/Army/Element/Fade_Right").gameObject;

            _fadeoutBars = new FadeoutBars();
            _fadeoutBars.Init(_cardCollectionScrollBar, leftFadeGameObject, rightFadeGameObject);

            UpdatePageScaleToMatchResolution();
        }

        public void Hide()
        {
            Dispose();

            if (_selfPage == null)
                return;

            _uiCardCollections.Hide();

            _selfPage.SetActive(false);
            Object.Destroy(_selfPage);
            _selfPage = null;
            _uiManager.HidePopup<SideMenuPopup>();
            _uiManager.HidePopup<AreaBarPopup>();
        }

        public void Update()
        {
            _uiCardCollections.Update();
            _fadeoutBars?.Update();
        }

        public void Dispose()
        {

        }

        #endregion

        private void UpdatePageScaleToMatchResolution()
        {
            float screenRatio = (float)Screen.width/Screen.height;
            if(screenRatio < 1.76f)
            {
                _selfPage.transform.localScale = Vector3.one * 0.93f;
            }
        }

        #region UI Handlers

        private void ButtonBuyPacksHandler()
        {
            PlayClickSound();
            GameClient.Get<IAppStateManager>().ChangeAppState(Enumerators.AppState.SHOP);
        }

        private void ButtonLeftArrowScrollHandler()
        {
            if (_cardCollectionScrollBar.value <= 0)
                return;

            _cardCollectionScrollBar.value -= _cardCollectionScrollBar.size;
            
            if (_cardCollectionScrollBar.value <= 0)
            {
                _cardCollectionScrollBar.value = 0;
            }

        }

        private void ButtonRightArrowScrollHandler()
        {
            if (_cardCollectionScrollBar.value >= 1)
                return;

            _cardCollectionScrollBar.value += _cardCollectionScrollBar.size;

            if (_cardCollectionScrollBar.value >= 1)
            {
                _cardCollectionScrollBar.value = 1;
            }
        }


        private void ButtonMarketplace()
        {
            PlayClickSound();
            _uiManager.GetPopup<QuestionPopup>().ConfirmationReceived += ConfirmRedirectMarketplaceLink;
            _uiManager.DrawPopup<QuestionPopup>("Would you like to visit the Marketplace website?");
        }

        private void ConfirmRedirectMarketplaceLink(bool status)
        {
            _uiManager.GetPopup<QuestionPopup>().ConfirmationReceived -= ConfirmRedirectMarketplaceLink;
            if(status)
            {
                Application.OpenURL(Constants.MarketPlaceLink);
            }
        }

        #endregion

        #region Util

        private void PlayClickSound()
        {
            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
        }
        #endregion

        private void FailAndGoToMainMenu(string customMessage = null)
        {
            _uiManager.HidePopup<LoadingOverlayPopup>();
            _uiManager.DrawPopup<WarningPopup>(customMessage ?? "Something went wrong.\n Please try again.");
            GameClient.Get<IAppStateManager>().ChangeAppState(Enumerators.AppState.MAIN_MENU, true);
        }
    }
}
