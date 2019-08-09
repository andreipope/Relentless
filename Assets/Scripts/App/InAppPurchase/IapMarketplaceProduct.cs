using System;
using JetBrains.Annotations;
using UnityEngine.Purchasing;

namespace Loom.ZombieBattleground.Iap
{
    public sealed class IapMarketplaceProduct
    {
        public ProductDefinition Definition { get; }

        public ProductMetadata Metadata { get; }

        public IapMarketplaceProductExtraMetadata ExtraMetadata { get; }

        public IapMarketplaceProduct(
            ProductDefinition definition,
            ProductMetadata metadata,
            IapMarketplaceProductExtraMetadata extraMetadata)
        {
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
            Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            ExtraMetadata = extraMetadata ?? throw new ArgumentNullException(nameof(extraMetadata));
        }
    }
}
