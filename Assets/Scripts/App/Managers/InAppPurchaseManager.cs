using System;
using System.CodeDom;
using System.Collections.Generic;
using Loom.ZombieBattleground;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Common;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Security;
using Convert = System.Convert;
using Object = UnityEngine.Object;

namespace Loom.ZombieBattleground
{

    public class InAppPurchaseManager : IInAppPurchaseManager,
#if UNITY_IOS || UNITY_ANDROID
                                        IStoreListener, 
#endif
                                        IService
    {

#if UNITY_IOS || UNITY_ANDROID
        #region Variable Field

        private static readonly ILog Log = Logging.GetLog(nameof(InAppPurchaseManager));
        private static IStoreController m_StoreController;       
        private static IExtensionProvider m_StoreExtensionProvider;

        public event Action<PurchaseEventArgs> ProcessPurchaseAction;

        #endregion

        #region IInAppPurchaseManager

        public bool IsInitialized()
        {
            return m_StoreController != null && m_StoreExtensionProvider != null;
        }

        public void InitializePurchasing()
        {
            if (IsInitialized())
            {
                return;
            }
            
            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

            builder.AddProduct(Constants.PRODUCT_BOOSTER_PACK_1, ProductType.Consumable);
            builder.AddProduct(Constants.PRODUCT_BOOSTER_PACK_2, ProductType.Consumable);
            builder.AddProduct(Constants.PRODUCT_BOOSTER_PACK_5, ProductType.Consumable);
            builder.AddProduct(Constants.PRODUCT_BOOSTER_PACK_10, ProductType.Consumable);

            UnityPurchasing.Initialize(this, builder);

        }

        public void BuyProductID(string productId)
        {
            if (IsInitialized())
            {
                Product product = m_StoreController.products.WithID(productId);

                if (product != null && product.availableToPurchase)
                {
                    Log.Info(string.Format("Purchasing product asychronously: '{0}'", product.definition.id));
                    m_StoreController.InitiatePurchase(product);
                }
                else
                { 
                    Log.Info("BuyProductID: FAIL. Not purchasing product, either is not found or is not available for purchase");
                }
            }
            else
            {
                Log.Info("BuyProductID FAIL. Not initialized.");
            }
        }

        #endregion

        #region IService

        public void Init()
        {
            if (m_StoreController == null)
            {
                InitializePurchasing();
            }
        }

        public void Update()
        {
        }

        void IService.Dispose()
        {
        }

        #endregion

        #region IStoreListener

        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            Log.Info("OnInitialized: PASS");

            m_StoreController = controller;
            m_StoreExtensionProvider = extensions;
        }

        public void OnInitializeFailed(InitializationFailureReason error)
        {
            Log.Info("OnInitializeFailed InitializationFailureReason:" + error);
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            Log.Info(string.Format("OnPurchaseFailed: FAIL. Product: '{0}', PurchaseFailureReason: {1}", product.definition.storeSpecificId, failureReason));
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
        {           
            switch (args.purchasedProduct.definition.id)
            {
                case Constants.PRODUCT_BOOSTER_PACK_1:
                case Constants.PRODUCT_BOOSTER_PACK_2:
                case Constants.PRODUCT_BOOSTER_PACK_5:
                case Constants.PRODUCT_BOOSTER_PACK_10:
                    Log.Info(string.Format("ProcessPurchase: PASS. Product: '{0}'", args.purchasedProduct.definition.id));
                    break;
                default:
                    Log.Info(string.Format("ProcessPurchase: FAIL. Unrecognized product: '{0}'", args.purchasedProduct.definition.id));
                    break;
            }
            
            ProcessPurchaseAction?.Invoke( args );

            return PurchaseProcessingResult.Complete;
        }

        #endregion
        
#else
        public void Init()
        {
        }
        
        public void Update()
        {
        }
        
        void IService.Dispose()
        {
        }
        
        public void BuyProductID(string productId)
        {
            //Buy Product Command for other platform           
        }
#endif

    }
 
#if UNITY_IOS || UNITY_ANDROID   
    public class AppleTangle
    {
        private static byte[] data = Convert.FromBase64String("Di84PWs6NC00f05OUlsed1BdEA8xowPNFXcWJPbA8IuHMOdgIuj1A3tAIXJVbqh/t/pKXDUuvX+5DbS/vioV7ld5qkg3wMpVsxB+mMl5c0EefX8OvD8cDjM4NxS4drjJMz8/PzZgDrw/Lzg9ayMeOrw/Ng68PzoOQX+Wpsfv9FiiGlUv7p2F2iUU/SENCGQOXA81Djc4PWs6OC08a20PLVBaHl1RUFpXSldRUE0eUVgeS01bRA68P0gOMDg9ayMxPz/BOjo9PD+Ayk2l0OxaMfVHcQrmnADHRsFV9lxSWx5NSl9QWl9MWh5KW0xTTR5fOdJDB721bR7tBvqPgaRxNFXBFcL+XQ1JyQQ5EmjV5DEfMOSETSdxi0lJEF9OTlJbEF1RUxFfTk5SW11fIa/lIHlu1TvTYEe6E9UInGlya9JZsTaKHsn1khIeUU6IAT8Osol98bFNv174JWU3EayMxnp2zl4GoCvLMzg3FLh2uMkzPz87Oz49vD8/PmJOUlsebFFRSh59fw4gKTMOCA4KDDg9ayMwOig6KhXuV3mqSDfAylWzGA4aOD1rOjUtI39OTlJbHn1bTEoa3NXviU7hMXvfGfTPU0bT2YspKZbiQBwL9Bvr5zHoVeqcGh0vyZ+S9ydMy2Mw60FhpcwbPYRrsXNjM89aCx0rdStnI42qyciioPFuhP9mbpWdT6x5bWv/kRF/jcbF3U7z2J1yRx5fTU1LU1tNHl9dXVtOSl9QXVseX1BaHl1bTEpXWFddX0pXUVAeTmeZOzdCKX5oLyBK7Ym1HQV5netROjgtPGttDy0OLzg9azo0LTR/Tk4RDr/9ODYVOD87Ozk8PA6/iCS/jQ68OoUOvD2dnj08Pzw8PzwOMzg3Oz49vD8xPg68PzQ8vD8/Ptqvlzc2FTg/Ozs5PD8oIFZKSk5NBBERSQMYWR60DVTJM7zx4NWdEcdtVGVaSlZRTFdKRw8oDio4PWs6PS0zf07nCEH/uWvnmaeHDHzF5utPoECfbFJbHndQXRAPGA4aOD1rOjUtI39ObFtSV19QXVseUVAeSlZXTR5dW0woDio4PWs6PS0zf05OUlsebFFRSrw/Pjg3FLh2uMldWjs/Dr/MDhQ4jw5m0mQ6DLJWjbEj4FtNwVlgW4KLBJPKMTA+rDWPHygQSusCM+VcKHfmSKENKlufSar3Ezw9Pz4/nbw/CKdyE0aJ07Kl4s1JpcxI7EkOcf8UuHa4yTM/Pzs7Pg5cDzUONzg9a0xfXUpXXVseTUpfSltTW1BKTRAOHlFYHkpWWx5KVltQHl9OTlJXXV+roEQymnm1ZeooCQ31+jFz8CpX7zgOMTg9ayMtPz/BOjsOPT8/wQ4jSldYV11fSlseXEceX1BHHk5fTEpXWFddX0pXUVAef0tKVlFMV0pHD7Unt+DHdVLLOZUcDjzWJgDGbjftiSWDrXwaLBT5MSOIc6JgXfZ1vikSHl1bTEpXWFddX0pbHk5RUlddRxB+mMl5c0E2YA4hOD1rIx06Jg4oIbu9uyWnA3kJzJelfrAS6o+uLOYLDA8KDg0IZCkzDQsODA4HDA8KDk5SWx59W0xKV1hXXV9KV1FQHn9LbpS06+Tawu43OQmOS0sf");
        private static int[] order = new int[] { 9,22,25,32,13,33,17,8,43,28,50,42,51,57,36,15,21,55,47,55,30,50,53,54,55,30,55,40,45,52,30,37,45,36,35,56,36,58,45,45,51,56,55,54,51,48,53,53,56,53,52,56,55,55,59,56,58,58,59,59,60 };
        private static int key = 62;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
            if (IsPopulated == false)
                return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
    
    public class GooglePlayTangle
    {
        private static byte[] data = Convert.FromBase64String("");
        private static int[] order = new int[] {  };
        private static int key = 0;

        public static readonly bool IsPopulated = false;

        public static byte[] Data() {
            if (IsPopulated == false)
                return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
    
    public class UnityChannelTangle
    {
        private static byte[] data = Convert.FromBase64String("");
        private static int[] order = new int[] {  };
        private static int key = 0;

        public static readonly bool IsPopulated = false;

        public static byte[] Data() {
            if (IsPopulated == false)
                return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
#endif

}
