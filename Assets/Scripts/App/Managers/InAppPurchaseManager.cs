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

    public class InAppPurchaseManager : IInAppPurchaseManager, IStoreListener, IService
    {

        #region Variable Field

        private static IStoreController m_StoreController;          // The Unity Purchasing system.
        private static IExtensionProvider m_StoreExtensionProvider;   // The store-specific Purchasing subsystems.
                                                                      // Product identifiers for all products capable of being purchased: 
                                                                      // "convenience" general identifiers for use with Purchasing, and their store-specific identifier 
                                                                      // counterparts for use with and outside of Unity Purchasing. Define store-specific identifiers 
                                                                      // also on each platform's publisher dashboard (iTunes Connect, Google Play Developer Console, etc.)

        public const string PRODUCT_BOOSTER_PACK_1 = "booster_pack_1";
        public const string PRODUCT_BOOSTER_PACK_2 = "booster_pack_2";
        public const string PRODUCT_BOOSTER_PACK_5 = "booster_pack_5";
        public const string PRODUCT_BOOSTER_PACK_10 = "booster_pack_10";

        public event Action<PurchaseEventArgs> ProcessPurchaseAction;

        #endregion

        #region IInAppPurchaseManager

        public bool IsInitialized()
        {
            // Only say we are initialized if both the Purchasing references are set.
            return m_StoreController != null && m_StoreExtensionProvider != null;
        }

        public void InitializePurchasing()
        {
            // If we have already connected to Purchasing ...
            if (IsInitialized())
            {
                // ... we are done here.
                return;
            }

            // Create a builder, first passing in a suite of Unity provided stores.
            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

            // Add a product to sell / restore by way of its identifier, associating the general identifier
            // with its store-specific identifiers.
            builder.AddProduct(PRODUCT_BOOSTER_PACK_1, ProductType.Consumable);
            builder.AddProduct(PRODUCT_BOOSTER_PACK_2, ProductType.Consumable);
            builder.AddProduct(PRODUCT_BOOSTER_PACK_5, ProductType.Consumable);
            builder.AddProduct(PRODUCT_BOOSTER_PACK_10, ProductType.Consumable);

            UnityPurchasing.Initialize(this, builder);

        }

        public void BuyProductID(string productId)
        {
            // If Purchasing has been initialized ...
            if (IsInitialized())
            {
                // ... look up the Product reference with the general product identifier and the Purchasing 
                // system's products collection.
                Product product = m_StoreController.products.WithID(productId);

                // If the look up found a product for this device's store and that product is ready to be sold ... 
                if (product != null && product.availableToPurchase)
                {
                    Debug.Log(string.Format("Purchasing product asychronously: '{0}'", product.definition.id));
                    // ... buy the product. Expect a response either through ProcessPurchase or OnPurchaseFailed 
                    // asynchronously.
                    m_StoreController.InitiatePurchase(product);
                }
                // Otherwise ...
                else
                {
                    // ... report the product look-up failure situation  
                    Debug.Log("BuyProductID: FAIL. Not purchasing product, either is not found or is not available for purchase");
                }
            }
            // Otherwise ...
            else
            {
                // ... report the fact Purchasing has not succeeded initializing yet. Consider waiting longer or 
                // retrying initiailization.
                Debug.Log("BuyProductID FAIL. Not initialized.");
            }
        }

        #endregion

        #region IService

        public void Init()
        {
            // If we haven't set up the Unity Purchasing reference
            if (m_StoreController == null)
            {
                // Begin to configure our connection to Purchasing
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
            // Purchasing has succeeded initializing. Collect our Purchasing references.
            Debug.Log("OnInitialized: PASS");

            // Overall Purchasing system, configured with products for this application.
            m_StoreController = controller;
            // Store specific subsystem, for accessing device-specific store features.
            m_StoreExtensionProvider = extensions;
        }

        public void OnInitializeFailed(InitializationFailureReason error)
        {
            // Purchasing set-up has not succeeded. Check error for reason. Consider sharing this reason with the user.
            Debug.Log("OnInitializeFailed InitializationFailureReason:" + error);
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            // A product purchase attempt did not succeed. Check failureReason for more detail. Consider sharing 
            // this reason with the user to guide their troubleshooting actions.
            Debug.Log(string.Format("OnPurchaseFailed: FAIL. Product: '{0}', PurchaseFailureReason: {1}", product.definition.storeSpecificId, failureReason));
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
        {

            if (String.Equals(args.purchasedProduct.definition.id, PRODUCT_BOOSTER_PACK_1, StringComparison.Ordinal))
            {
                Debug.Log(string.Format("ProcessPurchase: PASS. Product: '{0}'", args.purchasedProduct.definition.id));
            }
            else
            if (String.Equals(args.purchasedProduct.definition.id, PRODUCT_BOOSTER_PACK_2, StringComparison.Ordinal))
            {
                Debug.Log(string.Format("ProcessPurchase: PASS. Product: '{0}'", args.purchasedProduct.definition.id));
            }
            else
            if (String.Equals(args.purchasedProduct.definition.id, PRODUCT_BOOSTER_PACK_5, StringComparison.Ordinal))
            {
                Debug.Log(string.Format("ProcessPurchase: PASS. Product: '{0}'", args.purchasedProduct.definition.id));
            }
            else
            if (String.Equals(args.purchasedProduct.definition.id, PRODUCT_BOOSTER_PACK_10, StringComparison.Ordinal))
            {
                Debug.Log(string.Format("ProcessPurchase: PASS. Product: '{0}'", args.purchasedProduct.definition.id));
            }
            else
            {
                Debug.Log(string.Format("ProcessPurchase: FAIL. Unrecognized product: '{0}'", args.purchasedProduct.definition.id));
            }
            
            ProcessPurchaseAction?.Invoke( args );

            return PurchaseProcessingResult.Complete;
        }

        #endregion

    }

}