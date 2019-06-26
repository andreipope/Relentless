using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using log4net;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Iap;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Purchasing;
using Object = UnityEngine.Object;
using OneOf;
using OneOf.Types;
using UnityEngine.Assertions;

namespace Loom.ZombieBattleground
{
    public class ShopWithNavigationPage : IUIElement
    {
        private static readonly ILog Log = Logging.GetLog(nameof(ShopWithNavigationPage));

        private const float IapInitializationTimeout = 12;

        private IUIManager _uiManager;

        private ILoadObjectsManager _loadObjectsManager;

        private GameObject _selfPage;

        private List<ShopItem> _items = new List<ShopItem>();

        private State _state, _unfinishedState;

        private FiatValidationData _fiatValidationData;

        private PlasmaChainBackendFacade _plasmaChainBackendFacade;

        private IapMediator _iapMediator;

        #region IUIElement

        public void Init()
        {
            _uiManager = GameClient.Get<IUIManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _plasmaChainBackendFacade = GameClient.Get<PlasmaChainBackendFacade>();

            _iapMediator = GameClient.Get<IapMediator>();
        }

        public void Update() { }

        public async void Show()
        {
            SubscribeIapEvents();

            _selfPage = Object.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Pages/MyShopPage"), _uiManager.Canvas.transform, false);

            UpdatePageScaleToMatchResolution();

            _uiManager.DrawPopup<SideMenuPopup>(SideMenuPopup.MENU.SHOP);
            _uiManager.DrawPopup<AreaBarPopup>();

            if (_iapMediator.InitializationState != IapInitializationState.Initialized)
            {
                bool initializationSuccess = await InitializeStore();
                if (!initializationSuccess)
                    return;
            }

            bool claimingSuccess = await ClaimPendingPurchases();
            if (!claimingSuccess)
                return;

            ChangeState(State.WaitForInput);
            CreateItems();
        }

        public void Hide()
        {
            Dispose();

            _uiManager.HidePopup<SideMenuPopup>();
            _uiManager.HidePopup<AreaBarPopup>();
            _uiManager.HidePopup<LoadingOverlayPopup>();

            if (_selfPage == null)
                return;

            _selfPage.SetActive(false);
            Object.Destroy(_selfPage);
            _selfPage = null;
        }

        public void Dispose()
        {
            UnsubscribeIapEvents();
            foreach (ShopItem item in _items)
            {
                item.Dispose();
            }

            _items.Clear();
        }

        #endregion

        #region UI Handler

        private void BuyButtonHandler(Product product)
        {
            Log.Debug($"Initiating purchase: {product.definition.storeSpecificId}");
#if UNITY_IOS || UNITY_ANDROID
            ChangeState(State.Purchasing);
            _uiManager.DrawPopup<LoadingOverlayPopup>("Activating Purchase...");

            async void OnIapMediatorOnPurchasingResultReceived(OneOf<PurchaseEventArgs, IapPlatformStorePurchaseError> oneOf)
            {
                // Wait until we get a result for the product we are buying
                bool matchingProduct = false;
                oneOf.Switch(
                    args => matchingProduct = args.purchasedProduct.Equals(product),
                    error => matchingProduct = error.Product.Equals(product)
                    );

                if (!matchingProduct)
                    return;

                _iapMediator.PurchasingResultReceived -= OnIapMediatorOnPurchasingResultReceived;
                await _iapMediator.ClaimStorePurchases();
            }

            _iapMediator.PurchasingResultReceived += OnIapMediatorOnPurchasingResultReceived;
            OneOf<Success, IapPlatformStorePurchaseError> buyProductResult = _iapMediator.InitiatePurchase(product);
#else
            _uiManager.GetPopup<QuestionPopup>().ConfirmationReceived += ConfirmRedirectMarketplaceLink;
            _uiManager.DrawPopup<QuestionPopup>("Would you like to visit the Marketplace website?");
#endif
        }

        private void ConfirmRedirectMarketplaceLink(bool status)
        {
            _uiManager.GetPopup<QuestionPopup>().ConfirmationReceived -= ConfirmRedirectMarketplaceLink;
            if (status)
            {
                Application.OpenURL(Constants.MarketPlaceLink);
            }
        }

        #endregion

        private void ChangeState(State newState)
        {
            Assert.IsFalse(_selfPage == null);

            if (_state == newState)
                return;

            Log.Info($"ChangeState: prev:{_state.ToString()} next:{newState.ToString()}");

            _state = newState;
            switch (_state)
            {
                case State.Undefined:
                    break;
                case State.InitializingStore:
                    _unfinishedState = _state;
                    _uiManager.DrawPopup<LoadingOverlayPopup>("Initializing store...");
                    break;
                case State.ClaimingPendingPurchases:
                    _unfinishedState = _state;
                    _uiManager.DrawPopup<LoadingOverlayPopup>("Checking for purchases...");
                    break;
                case State.WaitForInput:
                    _uiManager.HidePopup<LoadingOverlayPopup>();
                    break;
                case State.InitiatedPurchase:
                    _unfinishedState = _state;
                    _uiManager.DrawPopup<LoadingOverlayPopup>("Activating Purchase...");
                    break;
                case State.Purchasing:
                    _unfinishedState = _state;
                    _uiManager.DrawPopup<LoadingOverlayPopup>("Processing Purchase...");
                    break;
                case State.RequestFiatValidation:
                    _unfinishedState = _state;
                    _uiManager.DrawPopup<LoadingOverlayPopup>("Processing payment...");
                    break;
                case State.RequestFiatTransaction:
                    _unfinishedState = _state;
                    _uiManager.DrawPopup<LoadingOverlayPopup>("Fetching your packs");
                    break;
                case State.RequestPack:
                    _unfinishedState = _state;
                    _uiManager.DrawPopup<LoadingOverlayPopup>("Fetching your packs.");
                    break;
                case State.WaitForRequestPackResponse:
                    _unfinishedState = _state;
                    _uiManager.DrawPopup<LoadingOverlayPopup>("Fetching your packs..");
                    break;
                case State.RequestFiatClaim:
                    _unfinishedState = _state;
                    _uiManager.DrawPopup<LoadingOverlayPopup>("Fetching your packs...");
                    break;
                case State.TransitionToPackOpener:
                    _unfinishedState = State.Undefined;
                    OnFinishRequestPack();
                    break;
                default:
                    throw new InvalidEnumArgumentException(nameof(_state), (int) _state, typeof(State));
            }
        }

        private void UpdatePageScaleToMatchResolution()
        {
            float screenRatio = (float) Screen.width / Screen.height;
            if (screenRatio < 1.76f)
            {
                _selfPage.transform.localScale = Vector3.one * 0.93f;
            }
        }

        private void CreateItems()
        {
            Transform root = _selfPage.transform.Find("Panel_Content/Mask/Group_Packs");
            foreach (Product product in _iapMediator.Products)
            {
                ShopItem shopItem = new ShopItem(root, _loadObjectsManager, _iapMediator, product);
                shopItem.Button.onClick.AddListener(() =>
                {
                    PlayClickSound();
                    BuyButtonHandler(product);
                });

                _items.Add(shopItem);
            }
        }

        #region Purchasing Logic

        private async Task<bool> InitializeStore()
        {
            ChangeState(State.InitializingStore);

            Action onFail = () =>
            {
                _iapMediator.Initialized -= IapMediatorOnInitialized;
                _iapMediator.InitializationFailed -= IapMediatorOnInitializationFailed;
                FailAndGoToMainMenu();
            };

            _iapMediator.Initialized += IapMediatorOnInitialized;
            _iapMediator.InitializationFailed += IapMediatorOnInitializationFailed;

            // If initialization did start successfully, we need to wait for an event to get the results of initialization.
            // Initialization is asynchronous and will only fail due to misconfiguration.
            // This means that in case of, for example, missing internet connectivity, no event will fire,
            // and the initialization will remain in "Initializing state".
            // We handle this by giving the IAP platform a timeout to initialize, and if timeout ends,
            // propose the user to try again later (when IAP might already initialize itself by then).

            bool gotInitializationResult = false;
            OneOf<InitializationFailureReason, IapException> failure = default;

            void IapMediatorOnInitialized()
            {
                Log.Debug("IapMediatorOnInitialized");
                gotInitializationResult = true;
                _iapMediator.Initialized -= IapMediatorOnInitialized;
                _iapMediator.InitializationFailed -= IapMediatorOnInitializationFailed;
            }

            void IapMediatorOnInitializationFailed(OneOf<InitializationFailureReason, IapException> innerFailure)
            {
                gotInitializationResult = true;
                _iapMediator.Initialized -= IapMediatorOnInitialized;
                _iapMediator.InitializationFailed -= IapMediatorOnInitializationFailed;
                failure = innerFailure;
            }

            if (_iapMediator.InitializationState == IapInitializationState.NotInitialized ||
                _iapMediator.InitializationState == IapInitializationState.Failed)
            {
                // Start initialization and check if it fails on the first step
                OneOf<Success, IapException> iapInitializeResult = await _iapMediator.BeginInitialization();

                if (!iapInitializeResult.IsT0)
                {
                    Log.Warn("Failed to initialize store: " + iapInitializeResult.Value);
                    onFail();
                    return false;
                }
            }

            double timestamp = Utilites.GetTimestamp();
            bool isInitializationTimeout = false;
            await new WaitUntil(() =>
            {
                isInitializationTimeout = Utilites.GetTimestamp() - timestamp > IapInitializationTimeout;
                return isInitializationTimeout || gotInitializationResult;
            });

            if (isInitializationTimeout)
            {
                Log.Warn("Store initialization timed out");
                onFail();
                return false;
            }

            if (_iapMediator.InitializationState == IapInitializationState.Failed)
            {
                Log.Warn("Store initialization failed: " + failure);
                onFail();
                return false;
            }

            return true;
        }

        private async Task<bool> ClaimPendingPurchases()
        {
            ChangeState(State.ClaimingPendingPurchases);
            const string error = "Failed to claim pending purchases. Please try again.";

            OneOf<Success, IapPurchaseProcessingError, IapException> claimStorePurchases = await _iapMediator.ClaimStorePurchases();
            if (!claimStorePurchases.IsT0)
            {
                Log.Warn(claimStorePurchases);
                FailAndGoToMainMenu(error);
                return false;
            }

            ChangeState(State.ClaimingPendingPurchases);
            OneOf<Success, IapPurchaseProcessingError, IapException> claimMarketplacePurchases = await _iapMediator.ClaimMarketplacePurchases();
            if (!claimMarketplacePurchases.IsT0)
            {
                Log.Warn(claimMarketplacePurchases);
                FailAndGoToMainMenu(error);
                return false;
            }

            return true;
        }

        private void FailAndGoToMainMenu(string customMessage = null)
        {
            _uiManager.HidePopup<LoadingOverlayPopup>();
            _uiManager.DrawPopup<WarningPopup>(customMessage ?? "Failed to initialize store.\n Please try again");
            ChangeState(State.Undefined);
            GameClient.Get<IAppStateManager>().ChangeAppState(Enumerators.AppState.MAIN_MENU);
        }

        private void SubscribeIapEvents()
        {
            _iapMediator.PurchaseStateChanged += IapMediatorOnPurchaseStateChanged;
        }

        private void UnsubscribeIapEvents()
        {
            _iapMediator.PurchaseStateChanged -= IapMediatorOnPurchaseStateChanged;
        }

        private async void OnFinishRequestPack()
        {
            Log.Debug("SUCCESSFULLY REQUEST for packs");
            _uiManager.DrawPopup<LoadingOverlayPopup>($"Successfully request for pack(s).");
            await Task.Delay(TimeSpan.FromSeconds(1f));
            _uiManager.HidePopup<LoadingOverlayPopup>();
            GameClient.Get<IAppStateManager>().ChangeAppState(Enumerators.AppState.PACK_OPENER);
        }

        private void IapMediatorOnPurchaseStateChanged(
            IapPurchaseState state,
            OneOf<IapPlatformStorePurchaseError, IapPurchaseProcessingError, IapException>? failure)
        {
            switch (state)
            {
                case IapPurchaseState.Undefined:
                    break;
                case IapPurchaseState.Failed:
                    ChangeState(State.WaitForInput);
                    Assert.IsTrue(failure != null);

                    Log.Warn("Error while processing purchase: " + failure.Value);

                    if (failure.Value.IsT0)
                    {
                        switch (failure.Value.AsT0.FailureReason)
                        {
                            case PurchaseFailureReason.Unknown:
                                // Happens at least when user enters incorrect password on iOS, should be safe to ignore?
                                return;
                            case PurchaseFailureReason.UserCancelled:
                                // Don't show error on user cancel
                                return;
                            case PurchaseFailureReason.ExistingPurchasePending:
                                OpenAlertDialog("Purchase for this product is already in progress. Please try again.");
                                return;
                            case PurchaseFailureReason.PaymentDeclined:
                                OpenAlertDialog("Payment was declined.");
                                return;
                            case PurchaseFailureReason.PurchasingUnavailable:
                                OpenAlertDialog("Purchasing is not available.\n\nCheck if you are using a valid account and purchasing is allowed on your device.");
                                return;
                            case PurchaseFailureReason.ProductUnavailable:
                            case PurchaseFailureReason.SignatureInvalid:
                            case PurchaseFailureReason.DuplicateTransaction:
                                // Those cases don't happen normally, so fallthrough to the next error handler
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }

                    string failureString = "";
                    failure.Value.Switch(
                        error => failureString = error.FailureReason.ToString(),
                        error => failureString = error.ToString(),
                        exception => failureString = exception.Message
                    );

                    OpenAlertDialog("Error while processing purchase: " + failureString);
                    break;
                case IapPurchaseState.StorePurchaseInitiated:
                    ChangeState(State.InitiatedPurchase);
                    break;
                case IapPurchaseState.StorePurchaseProcessing:
                    ChangeState(State.Purchasing);
                    break;
                case IapPurchaseState.RequestingFiatValidation:
                    ChangeState(State.RequestFiatValidation);
                    break;
                case IapPurchaseState.RequestingFiatTransaction:
                    ChangeState(State.RequestFiatTransaction);
                    break;
                case IapPurchaseState.RequestingPack:
                    ChangeState(State.RequestPack);
                    break;
                case IapPurchaseState.Finished:
                    ChangeState(State.WaitForInput);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }

        #endregion

        #region Util

        public void PlayClickSound()
        {
            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
        }

        public void OpenAlertDialog(string msg)
        {
            GameClient.Get<ISoundManager>()
                .PlaySound(Enumerators.SoundType.CHANGE_SCREEN,
                    Constants.SfxSoundVolume,
                    false,
                    false,
                    true);
            _uiManager.DrawPopup<WarningPopup>(msg);
        }

        #endregion

        private enum State
        {
            Undefined,
            InitializingStore,
            ClaimingPendingPurchases,
            WaitForInput,
            InitiatedPurchase,
            Purchasing,
            RequestFiatValidation,
            RequestFiatTransaction,
            RequestPack,
            WaitForRequestPackResponse,
            RequestFiatClaim,
            TransitionToPackOpener
        }

        private class ShopItem
        {
            public GameObject GameObject { get; }

            public Button Button { get; }

            public TextMeshProUGUI NameText { get; }

            public TextMeshProUGUI PriceText { get; }

            public ShopItem(Transform parent, ILoadObjectsManager loadObjectsManager, IapMediator iapMediator, Product product)
            {
                GameObject = Object.Instantiate(loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Elements/Shop_Item_Pack"), parent);
                NameText = GameObject.transform.Find("Text_PackName").GetComponent<TextMeshProUGUI>();
                PriceText = GameObject.transform.Find("Text_Price").GetComponent<TextMeshProUGUI>();
                Button = GameObject.transform.Find("Button").GetComponent<Button>();

                NameText.text = iapMediator.ProcessProductTitle(product.metadata.localizedTitle);

                // Pretty-format the price
                CultureInfo currencyCulture = CurrencyUtility.GetCultureFromIsoCurrencyCode(product.metadata.isoCurrencyCode);
                string priceText = null;
                if (currencyCulture != null)
                {
                    // Other symbols are not supported by the current font
                    string[] allowedCurrencySymbols =
                    {
                        "$", "€", "£"
                    };

                    if (allowedCurrencySymbols.Contains(currencyCulture.NumberFormat.CurrencySymbol))
                    {
                        priceText = String.Format(currencyCulture, "{0:C}", product.metadata.localizedPrice);
                    }
                }

                if (priceText == null)
                {
                    priceText = $"{product.metadata.localizedPrice:0.00} {product.metadata.isoCurrencyCode}";
                }

                PriceText.text = priceText;
            }

            public void Dispose()
            {
                Object.Destroy(GameObject);
            }
        }
    }
}
