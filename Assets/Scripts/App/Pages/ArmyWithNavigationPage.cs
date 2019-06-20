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

        private Button _buttonBuyPacks,
                       _buttonMarketplace;

        private UICardCollections _uiCardCollections;

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
