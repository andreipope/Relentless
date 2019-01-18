using System;
using System.CodeDom;
using System.Collections.Generic;
using Loom.ZombieBattleground;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Common;
using UnityEngine;
using UnityEngine.Purchasing;
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
                    Debug.Log(string.Format("Purchasing product asychronously: '{0}'", product.definition.id));
                    m_StoreController.InitiatePurchase(product);
                }
                else
                { 
                    Debug.Log("BuyProductID: FAIL. Not purchasing product, either is not found or is not available for purchase");
                }
            }
            else
            {
                Debug.Log("BuyProductID FAIL. Not initialized.");
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
            Debug.Log("OnInitialized: PASS");

            m_StoreController = controller;
            m_StoreExtensionProvider = extensions;
        }

        public void OnInitializeFailed(InitializationFailureReason error)
        {
            Debug.Log("OnInitializeFailed InitializationFailureReason:" + error);
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            Debug.Log(string.Format("OnPurchaseFailed: FAIL. Product: '{0}', PurchaseFailureReason: {1}", product.definition.storeSpecificId, failureReason));
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
        {           
            switch (args.purchasedProduct.definition.id)
            {
                case Constants.PRODUCT_BOOSTER_PACK_1:
                case Constants.PRODUCT_BOOSTER_PACK_2:
                case Constants.PRODUCT_BOOSTER_PACK_5:
                case Constants.PRODUCT_BOOSTER_PACK_10:
                    Debug.Log(string.Format("ProcessPurchase: PASS. Product: '{0}'", args.purchasedProduct.definition.id));
                    break;
                default:
                    Debug.Log(string.Format("ProcessPurchase: FAIL. Unrecognized product: '{0}'", args.purchasedProduct.definition.id));
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

}