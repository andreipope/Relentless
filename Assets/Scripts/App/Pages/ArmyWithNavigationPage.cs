using Loom.ZombieBattleground.Common;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public enum PageType { Army, DeckEditing }

namespace Loom.ZombieBattleground
{
    public class ArmyWithNavigationPage : IUIElement
    {
        private IUIManager _uiManager;
        private ILoadObjectsManager _loadObjectsManager;

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

            _uiCardCollections = new UICardCollections();
            _uiCardCollections.Init();
        }

        public void Show()
        {
            GameObject armyPage = Object.Instantiate(
                    _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Pages/MyCardsPage"),
                    _uiManager.Canvas.transform,
                    false);

            _selfPage = armyPage;

            _uiManager.DrawPopup<SideMenuPopup>(SideMenuPopup.MENU.MY_CARDS);
            _uiManager.DrawPopup<AreaBarPopup>();

            _uiCardCollections.Show(_selfPage, PageType.Army);
            _selfPage.transform.SetParent(_uiManager.Canvas.transform, false);

            _uiManager.DrawPopup<SideMenuPopup>(SideMenuPopup.MENU.MY_CARDS);
            _uiManager.DrawPopup<AreaBarPopup>();

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
    }
}
