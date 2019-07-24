using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using log4net;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

namespace Loom.ZombieBattleground.Iap
{
    /// <summary>
    /// Store implementation that uses product data from the Marketplace API.
    /// Only retrieving product information is supported, purchasing will fail.
    /// </summary>
    public class LoomWebMarketplaceStore : IStore
    {
        private static readonly ILog Log = Logging.GetLog(nameof(LoomWebMarketplaceStore));

        private IStoreCallback _callback;

        public IReadOnlyList<IapMarketplaceProduct> MarketplaceProducts { get; }

        public LoomWebMarketplaceStore(IReadOnlyList<IapMarketplaceProduct> marketplaceProducts)
        {
            MarketplaceProducts = marketplaceProducts;
        }

        public void Initialize(IStoreCallback callback)
        {
            _callback = callback;
        }

        public void RetrieveProducts(ReadOnlyCollection<ProductDefinition> products)
        {
            List<ProductDescription> productDescriptions = new List<ProductDescription>();
            foreach (ProductDefinition productDefinition in products)
            {
                IapMarketplaceProduct marketplaceProduct =
                    MarketplaceProducts.Single(product => productDefinition.storeSpecificId == product.Definition.storeSpecificId);

                ProductDescription productDescription = new ProductDescription(
                    marketplaceProduct.Definition.storeSpecificId,
                    marketplaceProduct.Metadata,
                    null,
                    null,
                    marketplaceProduct.Definition.type
                );

                productDescriptions.Add(productDescription);
            }

            _callback.OnProductsRetrieved(productDescriptions);
        }

        public void Purchase(ProductDefinition product, string developerPayload)
        {
            _callback.OnPurchaseFailed(
                new PurchaseFailureDescription(
                    product.storeSpecificId,
                    PurchaseFailureReason.PurchasingUnavailable,
                    "Not supported by Web Marketplace"
                )
            );
        }

        public void FinishTransaction(ProductDefinition product, string transactionId)
        {
            throw new NotSupportedException("Not supported by Web Marketplace");
        }
    }
}
