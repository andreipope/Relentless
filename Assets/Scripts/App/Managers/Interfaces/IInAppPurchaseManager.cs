using System;
using UnityEngine.Purchasing;
using Loom.ZombieBattleground.Common;

namespace Loom.ZombieBattleground
{

    public interface IInAppPurchaseManager
    {
#if UNITY_IOS || UNITY_ANDROID
        event Action<PurchaseEventArgs> ProcessPurchaseAction;

        event Action PurchaseFailedOrCanceled;

        bool IsInitialized();

        void InitializePurchasing();
#endif
        void BuyProductID(string productId);
     
    }

}