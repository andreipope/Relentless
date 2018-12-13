using System;
using UnityEngine.Purchasing;
using Loom.ZombieBattleground.Common;

namespace Loom.ZombieBattleground
{

    public interface IInAppPurchaseManager
    {
        event Action<PurchaseEventArgs> ProcessPurchaseAction;

        bool IsInitialized();

        void InitializePurchasing();

        void BuyProductID(string productId);
    }

}