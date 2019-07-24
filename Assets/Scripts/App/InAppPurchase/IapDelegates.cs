using OneOf;

namespace Loom.ZombieBattleground.Iap
{
    public delegate void PurchaseStateChangedHandler(
        IapPurchaseState state,
        OneOf<IapPlatformStorePurchaseError, IapPurchaseProcessingError, IapException>? failure);
}
