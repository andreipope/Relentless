using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Purchasing;
using log4net;
using Loom.Org.BouncyCastle.Utilities;
using OneOf;
using OneOf.Types;
using UnityEngine;
using UnityEngine.Purchasing.Extension;

namespace Loom.ZombieBattleground.Iap
{
    /// <summary>
    /// Provides a more convenient interface for Unity IAP.
    /// </summary>
    public class IapPlatformStoreFacade : IIapPlatformStoreFacade, IStoreListener, IService
    {
        private static readonly ILog Log = Logging.GetLog(nameof(IapPlatformStoreFacade));

        private IStoreController _storeController;
        private IExtensionProvider _storeExtensionProvider;
        private IapInitializationState _initializationState;

        #region IInAppPurchaseManager

        public event PurchaseFailedHandler PurchaseFailed;

        public event ProcessPurchaseHandler ProcessingPurchase;

        public event Action Initialized;

        public event Action<InitializationFailureReason> InitializationFailed;

        public IapInitializationState InitializationState => _initializationState;

        public IStoreController StoreController => _storeController;

        public void BeginInitialization(List<IapMarketplaceProduct> products)
        {
            if (_initializationState == IapInitializationState.Initialized)
                throw new InvalidOperationException("Already initialized");

            if (_initializationState == IapInitializationState.Initializing)
                throw new InvalidOperationException("Initialization already in progress");

            Log.Debug($"{nameof(BeginInitialization)} (products = {Utilites.FormatCallLogList(products.Select(product => product.Definition.storeSpecificId))})");
            _initializationState = IapInitializationState.Initializing;
#if (UNITY_ANDROID || UNITY_IOS) && !USE_WEB_MARKETPLACE && !UNITY_EDITOR
            IPurchasingModule purchasingModule = StandardPurchasingModule.Instance();
#else
            IPurchasingModule purchasingModule = new LoomWebMarketplacePurchasingModule(products);
#endif

            ConfigurationBuilder builder = ConfigurationBuilder.Instance(purchasingModule);
            builder.AddProducts(products.Select(product => product.Definition));

            UnityPurchasing.Initialize(this, builder);
        }

        public OneOf<Success, IapPlatformStorePurchaseError> InitiatePurchase(string productId)
        {
            if (_initializationState == IapInitializationState.Initializing)
                throw new InvalidOperationException("Initialization in progress");

            if (_initializationState != IapInitializationState.Initialized)
                throw new InvalidOperationException("Not initialized");

            Log.Debug($"{nameof(InitiatePurchase)} (string productId = {productId})");
            Product product = _storeController.products.WithStoreSpecificID(productId);
            if (product == null)
                throw new InvalidOperationException($"Unknown product {productId}");

            if (!product.availableToPurchase)
                return new IapPlatformStorePurchaseError(product, PurchaseFailureReason.ProductUnavailable);

            _storeController.InitiatePurchase(product);
            return new Success();
        }

        #endregion

        #region IStoreListener

        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            Log.Debug(nameof(OnInitialized));

            _storeController = controller;
            _storeExtensionProvider = extensions;
            _initializationState = IapInitializationState.Initialized;
            Initialized?.Invoke();
        }

        public void OnInitializeFailed(InitializationFailureReason error)
        {
            Log.Debug($"{nameof(OnInitializeFailed)}: {error}");
            _initializationState = IapInitializationState.Failed;
            InitializationFailed?.Invoke(error);
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            Log.Debug($"{nameof(OnPurchaseFailed)}(Product product = {product.definition.storeSpecificId}, PurchaseFailureReason failureReason = {failureReason})");
            PurchaseFailed?.Invoke(product, failureReason);
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
        {
            Log.Debug($"{nameof(ProcessPurchase)}(PurchaseEventArgs args = {args.purchasedProduct.definition.storeSpecificId})");
            PurchaseProcessingResult result = PurchaseProcessingResult.Pending;
            ProcessingPurchase?.Invoke(args, processingResult => result = processingResult);
            return result;
        }

        #endregion

        #region IService

        void IService.Init()
        {
        }

        void IService.Update() { }

        void IService.Dispose() { }

        #endregion
    }
}
