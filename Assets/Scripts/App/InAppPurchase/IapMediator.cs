using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using log4net;
using Loom.Client;
using OneOf;
using OneOf.Types;
using UnityEngine;
using UnityEngine.Purchasing;

namespace Loom.ZombieBattleground.Iap
{
    public class IapMediator : IService
    {
        private static readonly ILog Log = Logging.GetLog(nameof(IapMediator));

        private AuthFiatApiFacade _authFiatApiFacade;

        private PlasmaChainBackendFacade _plasmaChainBackendFacade;

        private IIapPlatformStoreFacade _iapPlatformStoreFacade;

        private AuthFiatApiFacade.StoreData _dataData;

        private UniqueList<Product> _storePendingPurchases = new UniqueList<Product>();

        private IapInitializationState _initializationState;

        private IapPurchaseState _lastPurchaseState, _currentPurchaseState;

        public event Action Initialized;

        public event Action<OneOf<InitializationFailureReason, IapException>> InitializationFailed;

        public event PurchaseStateChangedHandler PurchaseStateChanged;

        public event Action<OneOf<PurchaseEventArgs, IapPlatformStorePurchaseError>> PurchasingResultReceived;

        public IapInitializationState InitializationState => _initializationState;

        public IReadOnlyList<Product> Products { get; private set; }

        public IReadOnlyList<string> StringsRemovedFromProductTitles { get; private set; }

        /// <summary>
        /// Begins the IAP initialization process. The initialization can fail immediately.
        /// If it doesn't, listen for the <see cref="Initialized"/> and <see cref="InitializationFailed"/> events
        /// to get the result.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task<OneOf<Success, IapException>> BeginInitialization()
        {
            if (_initializationState == IapInitializationState.Initialized)
                throw new InvalidOperationException("Already initialized");

            if (_initializationState == IapInitializationState.Initializing)
                throw new InvalidOperationException("Initialization already in progress");

            Log.Debug(nameof(BeginInitialization));
            _initializationState = IapInitializationState.Initializing;
            List<IapMarketplaceProduct> productDefinitions;
            try
            {
                // Get the products IDs from Marketplace and use them for IAP
                IReadOnlyList<AuthFiatApiFacade.StoreData> stores = await _authFiatApiFacade.GetProducts();
                AuthFiatApiFacade.StoreData storeData = stores.Single(store => store.Store == GetAuthPlatformName());
                productDefinitions =
                    storeData.Packs
                        .Select(pack => ProductDataToMarketplaceProduct(storeData, pack))
                        .ToList();
            }
            catch (Exception e)
            {
                _initializationState = IapInitializationState.Failed;
                return new IapException("Loading store data failed", e);
            }

            _iapPlatformStoreFacade.BeginInitialization(productDefinitions);
            return new Success();
        }

        /// <summary>
        /// Initiates asynchronous purchase flow. Changes can be monitored using the <see cref="PurchaseStateChanged"/> event.
        /// </summary>
        /// <param name="product"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public OneOf<Success, IapPlatformStorePurchaseError> InitiatePurchase(Product product)
        {
            if (_initializationState == IapInitializationState.Initializing)
                throw new InvalidOperationException("Initialization in progress");

            if (_initializationState != IapInitializationState.Initialized)
                throw new InvalidOperationException("Not initialized");

            Log.Debug($"{nameof(InitiatePurchase)} ({product.definition.storeSpecificId})");
            SetState(IapPurchaseState.StorePurchaseInitiated, null);
            OneOf<Success, IapPlatformStorePurchaseError> initiatePurchaseResult =
                _iapPlatformStoreFacade.InitiatePurchase(product.definition.id);
            OneOf<IapPlatformStorePurchaseError, IapPurchaseProcessingError, IapException>? stateFailure = null;
            OneOf<Success, IapPlatformStorePurchaseError> result = new Success();
            initiatePurchaseResult.Switch(
                success => { },
                error =>
                {
                    stateFailure = error;
                    result = error;
                }
            );
            if (stateFailure != null)
            {
                SetState(IapPurchaseState.Failed, stateFailure);
            }

            return result;
        }

        /// <summary>
        /// Executes post-purchase processing the purchase receipt.
        /// </summary>
        /// <param name="receiptJson">Purchase receipt JSON</param>
        /// <param name="product">Product to process the purchase for. If set, any pending purchase for this product will be confirmed. Optional.</param>
        /// <returns></returns>
        public async Task<OneOf<Success, IapPurchaseProcessingError, IapException>> ExecutePostStorePurchaseProcessing(
            string receiptJson,
            Product product)
        {
            DAppChainClient plasmaChainClient;
            try
            {
                plasmaChainClient = await _plasmaChainBackendFacade.GetConnectedClient();
            }
            catch (Exception e)
            {
                IapException iapException = new IapException("Failed to connect to PlasmaChain", e);
                return iapException;
            }

            using (plasmaChainClient)
            {
                return await ExecutePostPurchaseProcessingInternal(plasmaChainClient, receiptJson, product, true);
            }
        }

        /// <summary>
        /// Gets the list of all registered transactions on Marketplace and attempts to claim them.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="IapException"></exception>
        public async Task<OneOf<Success, IapPurchaseProcessingError, IapException>> ClaimMarketplacePurchases()
        {
            Log.Debug(nameof(ClaimMarketplacePurchases));
            List<AuthFiatApiFacade.TransactionReceipt> transactions;
            try
            {
                transactions = await _authFiatApiFacade.ListPendingTransactions();
                Log.Debug("Pending transaction TxIDs: " + Utilites.FormatCallLogList(transactions.Select(tx => tx.TxId)));
            }
            catch (Exception e)
            {
                return new IapException("Failed to list pending transactions", e);
            }

            if (transactions.Count != 0)
            {
                DAppChainClient plasmaChainClient;
                try
                {
                    plasmaChainClient = await _plasmaChainBackendFacade.GetConnectedClient();
                }
                catch (Exception e)
                {
                    IapException iapException = new IapException("Failed to connect to PlasmaChain", e);
                    return iapException;
                }

                using (plasmaChainClient)
                {
                    foreach (AuthFiatApiFacade.TransactionReceipt transaction in transactions)
                    {
                        Log.Debug("Claiming transaction with TxId " + transaction.TxId);
                        IapPurchaseProcessor iapPurchaseProcessor =
                            new IapPurchaseProcessor(_authFiatApiFacade, _plasmaChainBackendFacade, plasmaChainClient, SetState);
                        OneOf<Success, IapPurchaseProcessingError, IapException> requestFiatTransactionResult =
                            await iapPurchaseProcessor.RequestFiatTransaction(transaction.TxId);
                        Log.Debug($"{nameof(iapPurchaseProcessor.RequestFiatTransaction)} result: " + requestFiatTransactionResult);

                        bool isFailed = false;
                        requestFiatTransactionResult.Switch(
                            success => { },
                            error =>
                            {
                                isFailed = true;
                                SetState(IapPurchaseState.Failed, error);
                            },
                            exception =>
                            {
                                isFailed = true;
                                SetState(IapPurchaseState.Failed, exception);
                            }
                        );

                        if (isFailed)
                            return requestFiatTransactionResult;
                    }
                }
            }

            SetState(IapPurchaseState.Finished, default);
            return new Success();
        }

        /// <summary>
        /// Attempts to claim all pending store purchases.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="IapException"></exception>
        public async Task<OneOf<Success, IapPurchaseProcessingError, IapException>> ClaimStorePurchases(bool setFailFinishStates = true)
        {
            Log.Debug($"{nameof(ClaimStorePurchases)} Pending product purchases: " + Utilites.FormatCallLogList(_storePendingPurchases.Select(p => p.definition.storeSpecificId)));

            if (_storePendingPurchases.Count != 0)
            {
                DAppChainClient plasmaChainClient;
                try
                {
                    plasmaChainClient = await _plasmaChainBackendFacade.GetConnectedClient();
                }
                catch (Exception e)
                {
                    return new IapException("Failed to connect to PlasmaChain", e);
                }

                using (plasmaChainClient)
                {
                    while (_storePendingPurchases.Count > 0)
                    {
                        Product pendingPurchase = _storePendingPurchases[0];
                        Log.Debug("Claiming product purchase: " + pendingPurchase.definition.storeSpecificId);
                        OneOf<Success, IapPurchaseProcessingError, IapException> processPurchaseResult =
                            await ExecutePostPurchaseProcessingInternal(plasmaChainClient, pendingPurchase, false);
                        Log.Debug($"Claiming product {pendingPurchase.definition.storeSpecificId} purchase, result: " + processPurchaseResult);

                        bool isFailed = false;
                        processPurchaseResult.Switch(
                            success => { },
                            error =>
                            {
                                isFailed = true;
                                if (setFailFinishStates)
                                {
                                    SetState(IapPurchaseState.Failed, error);
                                }
                            },
                            exception =>
                            {
                                isFailed = true;
                                if (setFailFinishStates)
                                {
                                    SetState(IapPurchaseState.Failed, exception);
                                }
                            }
                        );

                        if (isFailed)
                            return processPurchaseResult;
                    }
                }
            }

            if (setFailFinishStates)
            {
                SetState(IapPurchaseState.Finished, default);
            }

            return new Success();
        }

        public string ProcessProductTitle(string title)
        {
            if (title == null)
                throw new ArgumentNullException(nameof(title));

            foreach (string removedString in StringsRemovedFromProductTitles)
            {
                title = title.Replace(removedString, "");
            }

            return title.Trim();
        }

        private async Task<OneOf<Success, IapPurchaseProcessingError, IapException>> ExecutePostPurchaseProcessingInternal(
            DAppChainClient plasmaChainClient,
            string receiptJson,
            Product product,
            bool setFailFinishStates)
        {
            Log.Info("[Purchase Receipt]:\n" + receiptJson);

#if !UNITY_EDITOR && IAP_PRINT_RECEIPT_AND_FAIL
            throw new IapException("printing receipt and failing");
#endif

            IapPurchaseProcessor iapPurchaseProcessor = new IapPurchaseProcessor(_authFiatApiFacade, _plasmaChainBackendFacade, plasmaChainClient, SetState);
            OneOf<Success, IapPurchaseProcessingError, IapException> processPurchaseResult =
                await iapPurchaseProcessor.ProcessPurchase(receiptJson, product);
            Log.Debug("ProcessPurchase result: " + processPurchaseResult);

            bool isFailed = false;
            processPurchaseResult.Switch(
                success => { },
                error =>
                {
                    // Possible if purchasing and claiming went fine, but confirming the pending transaction failed.
                    // We can safely ignore it.
                    if (error == IapPurchaseProcessingError.TxNotRegistered)
                        return;

                    isFailed = true;
                    if (setFailFinishStates)
                    {
                        SetState(IapPurchaseState.Failed, error);
                    }
                },
                exception =>
                {
                    isFailed = true;
                    if (setFailFinishStates)
                    {
                        SetState(IapPurchaseState.Failed, exception);
                    }
                }
            );
            if (isFailed)
                return processPurchaseResult;

            if (product != null)
            {
                Log.Debug($"Confirming pending purchase of product {product.definition.storeSpecificId}");
                _iapPlatformStoreFacade.StoreController.ConfirmPendingPurchase(product);
            }
            else
            {
                Log.Warn("No product, not confirming pending purchase");
            }

            if (setFailFinishStates)
            {
                SetState(IapPurchaseState.Finished, default);
            }

            return new Success();
        }

        private async Task<OneOf<Success, IapPurchaseProcessingError, IapException>> ExecutePostPurchaseProcessingInternal(
            DAppChainClient plasmaChainClient,
            Product product,
            bool setFailFinishStates)
        {
            string receipt = product.receipt;
            OneOf<Success, IapPurchaseProcessingError, IapException> result =
                await ExecutePostPurchaseProcessingInternal(plasmaChainClient, receipt, product, setFailFinishStates);
            if (result.IsT0)
            {
                Log.Debug($"{nameof(ExecutePostPurchaseProcessingInternal)} success, removing product {product.definition.storeSpecificId} from pending purchases");
                _storePendingPurchases.Remove(product);
            }

            return result;
        }

        #region Event Handlers

        private void IapPlatformStoreFacadeOnInitializationFailed(InitializationFailureReason reason)
        {
            _initializationState = IapInitializationState.Failed;
            InitializationFailed?.Invoke(reason);
        }

        private void IapPlatformStoreFacadeOnInitialized()
        {
            _initializationState = IapInitializationState.Initialized;
            Products = _iapPlatformStoreFacade.StoreController.products.all.ToList();
            Initialized?.Invoke();
        }

        private void IapPlatformStoreFacadeOnPurchaseFailedOrCanceled(Product product, PurchaseFailureReason failureReason)
        {
            Log.Debug($"{nameof(IapPlatformStoreFacadeOnPurchaseFailedOrCanceled)} " +
                $"(Product product = {product.definition.storeSpecificId}, PurchaseFailureReason failureReason = {failureReason}");

            IapPlatformStorePurchaseError error = new IapPlatformStorePurchaseError(product, failureReason);
            PurchasingResultReceived?.Invoke(error);
            SetState(IapPurchaseState.Failed, error);
        }

        private void IapPlatformStoreFacadeOnProcessPurchase(PurchaseEventArgs args, Action<PurchaseProcessingResult> setPurchaseProcessingResult)
        {
            Log.Debug($"{nameof(IapPlatformStoreFacadeOnProcessPurchase)} (PurchaseEventArgs args = " +
                $"[id: {args.purchasedProduct.definition.storeSpecificId}, transactionID: {args.purchasedProduct.transactionID}])");

            setPurchaseProcessingResult(PurchaseProcessingResult.Pending);
            _storePendingPurchases.Add(args.purchasedProduct);

            PurchasingResultReceived?.Invoke(args);
        }

        #endregion

        private void SetState(IapPurchaseState state, OneOf<IapPlatformStorePurchaseError, IapPurchaseProcessingError, IapException>? failure)
        {
            _lastPurchaseState = _currentPurchaseState;
            _currentPurchaseState = state;
            Log.Info($"{nameof(SetState)}(state = {state}, failure = {failure?.ToString() ?? "None"})");
            PurchaseStateChanged?.Invoke(_currentPurchaseState, failure);
        }

        private static IapMarketplaceProduct ProductDataToMarketplaceProduct(
            AuthFiatApiFacade.StoreData storeData,
            AuthFiatApiFacade.ProductData productData)
        {
            return new IapMarketplaceProduct(
                new ProductDefinition(productData.StoreId, productData.StoreId, ProductType.Consumable),
                new ProductMetadata(
                    null,
                    productData.DisplayName,
                    productData.Description,
                    storeData.Currency,
                    productData.Price / (decimal) storeData.UnitPercent
                )
            );
        }

        private static string GetAuthPlatformName()
        {
#if UNITY_ANDROID
            return "PlayStore";
#elif UNITY_IOS
            return "AppStore";
#else
            return "MarketPlace";
#endif
        }

        #region IService

        void IService.Init()
        {
            _authFiatApiFacade = GameClient.Get<AuthFiatApiFacade>();
            _plasmaChainBackendFacade = GameClient.Get<PlasmaChainBackendFacade>();

            ILoadObjectsManager loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            string removedStringsText = loadObjectsManager.GetObjectByPath<TextAsset>("Data/iap_titles_strings_to_remove").text;
            StringsRemovedFromProductTitles = removedStringsText.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

            _iapPlatformStoreFacade = GameClient.Get<IIapPlatformStoreFacade>();
            _iapPlatformStoreFacade.ProcessingPurchase += IapPlatformStoreFacadeOnProcessPurchase;
            _iapPlatformStoreFacade.PurchaseFailed += IapPlatformStoreFacadeOnPurchaseFailedOrCanceled;
            _iapPlatformStoreFacade.Initialized += IapPlatformStoreFacadeOnInitialized;
            _iapPlatformStoreFacade.InitializationFailed += IapPlatformStoreFacadeOnInitializationFailed;
        }

        void IService.Update() { }

        void IService.Dispose() { }

        #endregion
    }
}
