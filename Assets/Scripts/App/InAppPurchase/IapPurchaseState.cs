namespace Loom.ZombieBattleground.Iap
{
    public enum IapPurchaseState
    {
        Undefined,
        Failed,
        StorePurchaseInitiated,
        StorePurchaseProcessing,
        RequestingFiatValidation,
        RequestingFiatTransaction,
        RequestingPack,
        Finished
    }
}
