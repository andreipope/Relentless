using System.Collections.Generic;
using UnityEngine.Purchasing.Extension;

namespace Loom.ZombieBattleground.Iap
{
    /// <summary>
    /// A simple purchasing module that provides a Loom web Marketplace store.
    /// </summary>
    public class LoomWebMarketplacePurchasingModule : AbstractPurchasingModule
    {
        public IReadOnlyList<IapMarketplaceProduct> MarketplaceProducts { get; }

        public LoomWebMarketplacePurchasingModule(IReadOnlyList<IapMarketplaceProduct> marketplaceProducts)
        {
            MarketplaceProducts = marketplaceProducts;
        }

        public override void Configure()
        {
            RegisterStore("LoomWebMarketplace", new LoomWebMarketplaceStore(MarketplaceProducts));
        }
    }
}
