using System;
using UnityEngine.Purchasing;

namespace Loom.ZombieBattleground.Iap
{
    public sealed class IapMarketplaceProduct
    {
        public ProductDefinition Definition { get; }

        public ProductMetadata Metadata { get; }

        public IapMarketplaceProduct(ProductDefinition definition, ProductMetadata metadata)
        {
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
            Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
        }
    }
}
