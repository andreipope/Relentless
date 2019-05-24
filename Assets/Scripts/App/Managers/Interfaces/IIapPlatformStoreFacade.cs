using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.Purchasing;
using OneOf;
using OneOf.Types;

namespace Loom.ZombieBattleground.Iap
{
    public delegate void PurchaseFailedHandler(Product product, PurchaseFailureReason failureReason);

    public delegate void ProcessPurchaseHandler(PurchaseEventArgs args, Action<PurchaseProcessingResult> setPurchaseProcessingResult);

    /// <summary>
    /// Provides a more convenient interface for Unity IAP.
    /// </summary>
    public interface IIapPlatformStoreFacade
    {
        event PurchaseFailedHandler PurchaseFailed;

        event ProcessPurchaseHandler ProcessingPurchase;

        event Action Initialized;

        event Action<InitializationFailureReason> InitializationFailed;

        IapInitializationState InitializationState { get; }

        IStoreController StoreController { get; }

        /// <summary>
        /// Begins the IAP initialization process.
        /// Listen for the <see cref="Initialized"/> and <see cref="InitializationFailed"/> events to get the result.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        void BeginInitialization(List<IapMarketplaceProduct> products);

        OneOf<Success, IapPlatformStorePurchaseError> InitiatePurchase(string productId);
    }
}
