using UnityEngine.Purchasing;

namespace Loom.ZombieBattleground.Iap
{
    public class IapPlatformStorePurchaseError
    {
        public Product Product { get; }

        public PurchaseFailureReason FailureReason { get; }

        public IapPlatformStorePurchaseError(Product product, PurchaseFailureReason failureReason)
        {
            Product = product;
            FailureReason = failureReason;
        }

        public override string ToString()
        {
            return $"{nameof(Product)}: {Product.definition.storeSpecificId}, {nameof(FailureReason)}: {FailureReason}";
        }
    }
}
