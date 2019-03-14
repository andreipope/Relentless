using System;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DG.Tweening;
using log4net;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Security;
using Object = UnityEngine.Object;
using Convert = System.Convert;
using Newtonsoft.Json;

namespace Loom.ZombieBattleground
{
    public class ShopWithNavigationPage : IUIElement
    {
        private static readonly ILog Log = Logging.GetLog(nameof(ShopWithNavigationPage));

        private IUIManager _uiManager;
        
        private ILoadObjectsManager _loadObjectsManager;
        
        private GameObject _selfPage;

        private List<Button> _itemButtonList;

        private List<TextMeshProUGUI> _textItemNameList,
                                      _textItemPriceList;

        private ShopData _shopData;
        
        #region IUIElement
        
        public void Init()
        {
            _uiManager = GameClient.Get<IUIManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();

            _itemButtonList = new List<Button>();
            _textItemNameList = new List<TextMeshProUGUI>();
            _textItemPriceList = new List<TextMeshProUGUI>();

            InitPurchaseLogic();
            LoadShopData();         
        }

        public void Update()
        {
        }

        public void Show()
        {
            _selfPage = Object.Instantiate(
                _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Pages/MyShopPage"));
            _selfPage.transform.SetParent(_uiManager.Canvas.transform, false);
            
            LoadItems();

            UpdatePageScaleToMatchResolution();
            
            _uiManager.DrawPopup<SideMenuPopup>(SideMenuPopup.MENU.SHOP);
            _uiManager.DrawPopup<AreaBarPopup>();
        }
        
        public void Hide()
        {
            Dispose();
        
            if (_selfPage == null)
                return;
        
            _selfPage.SetActive(false);
            Object.Destroy(_selfPage);
            _selfPage = null;
            
            _uiManager.HidePopup<SideMenuPopup>();
            _uiManager.HidePopup<AreaBarPopup>();
        }
        
        public void Dispose()
        {
            if (_itemButtonList != null)
                _itemButtonList.Clear();  
                
            if (_textItemNameList != null)
                _textItemNameList.Clear();  
                
            if (_textItemPriceList != null)
                _textItemPriceList.Clear();  
        }
        
        #endregion
        
        #region UI Handler
        
        private void BuyButtonHandler( int id )
        {
            #if UNITY_IOS || UNITY_ANDROID && !UNITY_EDITOR
            _uiManager.DrawPopup<LoadingFiatPopup>("Activating purchase . . .");
            _inAppPurchaseManager.BuyProductID( _shopData.ProductID[id] );
            #else
            _uiManager.GetPopup<QuestionPopup>().ConfirmationReceived += ConfirmRedirectMarketplaceLink;
            _uiManager.DrawPopup<QuestionPopup>("Do you want to redirect to marketplace webpage?"); 
            #endif
        }
        
        private void ConfirmRedirectMarketplaceLink(bool status)
        {
            _uiManager.GetPopup<QuestionPopup>().ConfirmationReceived -= ConfirmRedirectMarketplaceLink;
            if(status)
            {
                Application.OpenURL(Constants.MarketPlaceLink);
            }
        }
        
#endregion
        
        private void UpdatePageScaleToMatchResolution()
        {
            float screenRatio = (float)Screen.width/Screen.height;
            if(screenRatio < 1.76f)
            {
                _selfPage.transform.localScale = Vector3.one * 0.93f;
            }
        } 
        
        private void LoadItems()
        {
            string path = "Panel_Content/Group_Packs";
            
            _itemButtonList.Clear();
            _textItemNameList.Clear();
            _textItemPriceList.Clear();
            
            for(int i=0; i<_shopData.NumberOfItems; ++i)
            {
                int index = i;
                Button button = _selfPage.transform.Find($"{path}/Node_Pack_{i}/Button_Pack").GetComponent<Button>();
                _itemButtonList.Add(button);
                button.onClick.AddListener(()=>
                {
                    BuyButtonHandler(index);
                });
                button.onClick.AddListener(PlayClickSound);
                
                TextMeshProUGUI textName = _selfPage.transform.Find($"{path}/Node_Pack_{i}/Text_PackName").GetComponent<TextMeshProUGUI>();
                textName.text = _shopData.ItemNames[i];
                _textItemNameList.Add(textName);
                
                TextMeshProUGUI textPrice = _selfPage.transform.Find($"{path}/Node_Pack_{i}/Text_Price").GetComponent<TextMeshProUGUI>();
                textPrice.text = _shopData.ItemCosts[i];
                _textItemPriceList.Add(textPrice);
            }
        }
        
        private void LoadShopData()
        {
            _shopData = JsonConvert.DeserializeObject<ShopData>(_loadObjectsManager.GetObjectByPath<TextAsset>("Data/shop_data").text);            
        }

#region Purchasing Logic

#if UNITY_IOS || UNITY_ANDROID
        private FiatValidationDataGoogleStore _fiatValidationDataGoogleStore;
        private FiatValidationDataAppleStore _fiatValidationDataAppleStore; 
#endif
        
        private IInAppPurchaseManager _inAppPurchaseManager;

        private FiatBackendManager _fiatBackendManager;

        private FiatPlasmaManager _fiatPlasmaManager;
        
        private Action _finishRequestPack;

        public void InitPurchaseLogic()
        {
            _fiatBackendManager = GameClient.Get<FiatBackendManager>();
            _fiatPlasmaManager = GameClient.Get<FiatPlasmaManager>();
            
            _inAppPurchaseManager = GameClient.Get<IInAppPurchaseManager>();
#if UNITY_IOS || UNITY_ANDROID
            _inAppPurchaseManager.ProcessPurchaseAction += OnProcessPurchase;
            _finishRequestPack = OnFinishRequestPack;
#endif
        }
        
#if UNITY_IOS || UNITY_ANDROID
        private async void RequestFiatValidationGoogle()
        {            
            _uiManager.DrawPopup<LoadingFiatPopup>($"{nameof(RequestFiatValidationGoogle)}");
            
            FiatBackendManager.FiatValidationResponse response = null;
            try
            {
                response = await _fiatBackendManager.CallFiatValidationGoogle
                (
                    _fiatValidationDataGoogleStore.productId,
                    _fiatValidationDataGoogleStore.purchaseToken,
                    _fiatValidationDataGoogleStore.transactionId,
                    _fiatValidationDataGoogleStore.storeName
                );        
            }
            catch(Exception e)
            {
                Log.Info($"{nameof(RequestFiatValidationGoogle)} failed: {e.Message}");
                _uiManager.DrawPopup<WarningPopup>($"{nameof(RequestFiatValidationGoogle)} failed\n{e.Message}\nPlease try again");
                WarningPopup popup = _uiManager.GetPopup<WarningPopup>();
                popup.ConfirmationReceived += WarningPopupRequestFiatValidationGoogle;
                _uiManager.HidePopup<LoadingFiatPopup>();
                return;
            }  
            
            _uiManager.HidePopup<LoadingFiatPopup>();
            RequestFiatTransaction();         
        }
        
        private void WarningPopupRequestFiatValidationGoogle()
        {
            WarningPopup popup = _uiManager.GetPopup<WarningPopup>();
            popup.ConfirmationReceived -= WarningPopupRequestFiatValidationGoogle;

            RequestFiatValidationGoogle();
        }

        private async void RequestFiatValidationApple()
        {
            _uiManager.DrawPopup<LoadingFiatPopup>($"{nameof(RequestFiatValidationApple)}");
            
            FiatBackendManager.FiatValidationResponse response = null;
            try
            {
                response = await _fiatBackendManager.CallFiatValidationApple
                (
                    _fiatValidationDataAppleStore.productId,
                    _fiatValidationDataAppleStore.transactionId,
                    _fiatValidationDataAppleStore.receiptData,
                    _fiatValidationDataAppleStore.storeName
                );    
            }
            catch(Exception e)
            {
                Log.Info($"{nameof(RequestFiatValidationApple)} failed: {e.Message}");
                _uiManager.DrawPopup<WarningPopup>($"{nameof(RequestFiatValidationApple)} failed\n{e.Message}\nPlease try again");
                WarningPopup popup = _uiManager.GetPopup<WarningPopup>();
                popup.ConfirmationReceived += WarningPopupRequestFiatValidationApple;
                _uiManager.HidePopup<LoadingFiatPopup>();
                return;
            }  
            
            _uiManager.HidePopup<LoadingFiatPopup>();
            RequestFiatTransaction();     
        }
        
        private void WarningPopupRequestFiatValidationApple()
        {
            WarningPopup popup = _uiManager.GetPopup<WarningPopup>();
            popup.ConfirmationReceived -= WarningPopupRequestFiatValidationApple;

            RequestFiatValidationApple();
        }
        
        private async void RequestFiatTransaction()
        {
            _uiManager.DrawPopup<LoadingFiatPopup>($"{nameof(RequestFiatTransaction)}");
            
            List<FiatBackendManager.FiatTransactionResponse> recordList = null;
            try
            {
                recordList = await _fiatBackendManager.CallFiatTransaction();
            }
            catch(Exception e)
            {
                Log.Info($"{nameof(RequestFiatTransaction)} failed: {e.Message}");
                _uiManager.DrawPopup<WarningPopup>($"{nameof(RequestFiatTransaction)} failed\n{e.Message}\nPlease try again");
                WarningPopup popup = _uiManager.GetPopup<WarningPopup>();
                popup.ConfirmationReceived += WarningPopupRequestFiatTransaction;
                _uiManager.HidePopup<LoadingFiatPopup>();
                return;
            }
            
            recordList.Sort( (FiatBackendManager.FiatTransactionResponse resA, FiatBackendManager.FiatTransactionResponse resB)=>
            {
                return resB.TxID - resA.TxID;
            });
            string log = "TxID: ";
            foreach( var i in recordList)
            {
                log += i.TxID + ", ";
            }
            Log.Debug(log);
            _uiManager.HidePopup<LoadingFiatPopup>();
            RequestPack(recordList);            
        }
        
        private void WarningPopupRequestFiatTransaction()
        {
            WarningPopup popup = _uiManager.GetPopup<WarningPopup>();
            popup.ConfirmationReceived -= WarningPopupRequestFiatTransaction;

            RequestFiatTransaction();
        }
        
        private async void RequestPack(List<FiatBackendManager.FiatTransactionResponse> sortedRecordList)
        {            
            Log.Debug("<color=green>START REQUEST for packs</color>");
            List<FiatBackendManager.FiatTransactionResponse> requestList = new List<FiatBackendManager.FiatTransactionResponse>();
            for (int i = 0; i < sortedRecordList.Count; ++i)
            {
                requestList.Add(sortedRecordList[i]);
            }

            for (int i = 0; i < requestList.Count; ++i)
            {
                FiatBackendManager.FiatTransactionResponse record = requestList[i];
                
                _uiManager.DrawPopup<LoadingFiatPopup>($"Request Pack UserId: {record.UserId}, TxID: {record.TxID}");

                string eventResponse = "";

                eventResponse = await _fiatPlasmaManager.CallRequestPacksContract(record);
                Log.Debug($"<color=green>Contract [requestPacks] success call.</color>");
                Log.Debug($"<color=green>EVENT RESPONSE: {eventResponse}</color>");
                if (!string.IsNullOrEmpty(eventResponse))
                {
                    Log.Debug("<color=green>FINISH REQUEST for packs</color>");
                    await _fiatBackendManager.CallFiatClaim
                    (
                        record.UserId,
                        new List<int>
                        {
                            record.TxID
                        }
                    );                    
                }
            }
            
            _finishRequestPack();
        }

        private async void OnFinishRequestPack()
        {
            Log.Debug("SUCCESSFULLY REQUEST for packs");
            await _uiManager.GetPage<PackOpenerPageWithNavigationBar>().RetrievePackBalanceAmount((int)Enumerators.MarketplaceCardPackType.Booster);
            _uiManager.DrawPopup<LoadingFiatPopup>($"Successfully request for pack(s)");
            await Task.Delay(TimeSpan.FromSeconds(1f));
            _uiManager.HidePopup<LoadingFiatPopup>();
            GameClient.Get<IAppStateManager>().ChangeAppState(Enumerators.AppState.PACK_OPENER);
        }
        
        private void OnProcessPurchase(PurchaseEventArgs args)
        {
            _uiManager.HidePopup<LoadingFiatPopup>();
            Product product = args.purchasedProduct;

            Log.Debug("OnProcessPurchase");
            Log.Debug($"productId {product.definition.id}");
            Log.Debug($"receipt {args.purchasedProduct.receipt}");
            Log.Debug($"transactionID {product.transactionID}");
            Log.Debug($"storeSpecificId {product.definition.storeSpecificId}");

#if UNITY_ANDROID
            _fiatValidationDataGoogleStore = new FiatValidationDataGoogleStore();      
            _fiatValidationDataGoogleStore.productId = product.definition.id;
            _fiatValidationDataGoogleStore.purchaseToken = ParsePurchaseTokenFromPlayStoreReceipt(args.purchasedProduct.receipt);
            _fiatValidationDataGoogleStore.transactionId = product.transactionID;
            _fiatValidationDataGoogleStore.storeName = "GooglePlay";

            RequestFiatValidationGoogle();  
#elif UNITY_IOS
            _fiatValidationDataAppleStore = new FiatValidationDataAppleStore();
            _fiatValidationDataAppleStore.productId = product.definition.id;
            _fiatValidationDataAppleStore.transactionId = ParseTransactionIdentifierFromAppStoreReceipt(args);
            _fiatValidationDataAppleStore.receiptData = ParsePayloadFromAppStoreReceipt(args.purchasedProduct.receipt);
            _fiatValidationDataAppleStore.storeName = "AppleStore";
            RequestFiatValidationApple();
#endif
        }
        
        private string ParseTransactionIdentifierFromAppStoreReceipt(PurchaseEventArgs e)
        {
            var validator = new CrossPlatformValidator(GooglePlayTangle.Data(),
                    AppleTangle.Data(), Application.identifier);
        
            var result = validator.Validate(e.purchasedProduct.receipt);
            Log.Info("Receipt is valid. Contents:");
            int count = 0;
            foreach (IPurchaseReceipt productReceipt in result) {
                Log.Info($"productReceipt {count}");
                ++count;
                Log.Info($"productReceipt.productID: {productReceipt.productID}");
                Log.Info($"productReceipt.purchaseDate: {productReceipt.purchaseDate}");
                Log.Info($"productReceipt.transactionID: {productReceipt.transactionID}");

                if (productReceipt is AppleInAppPurchaseReceipt apple) {
                    Log.Info($"apple.originalTransactionIdentifier: {apple.originalTransactionIdentifier}");
                    Log.Info($"apple.subscriptionExpirationDate {apple.subscriptionExpirationDate}");
                    Log.Info($"apple.cancellationDate: {apple.cancellationDate}");
                    Log.Info($"apple.quantity: {apple.quantity}");
                }

                return productReceipt.transactionID;
            }
            return "";
        }

        private string ParsePayloadFromAppStoreReceipt(string receiptString)
        {
            string payload = "";   
            try
            {
                IAPReceipt2 receipt = JsonConvert.DeserializeObject<IAPReceipt2>(receiptString);                
                
                Log.Debug("IAPReceipt");
                string log = "";
                log += "receipt.TransactionID: " + receipt.TransactionID;
                log += "\n";
                log += "receipt.Store: " + receipt.Store;
                log += "\n";
                log += "Payload: " + receipt.Payload;
                Log.Debug(log);
                payload = receipt.Payload;

                string logText = "";
                string payloadToCut = payload;
                Log.Debug("PAYLOAD START");
                int count = 0;
                while( !string.IsNullOrEmpty(payloadToCut))
                {
                    int lengthAmount = 200;
                    if(payloadToCut.Length > lengthAmount)
                    {
                        logText = payloadToCut.Substring(0, lengthAmount);
                        payloadToCut = payloadToCut.Substring(lengthAmount);
                    }
                    else
                    {
                        logText = payloadToCut;
                        payloadToCut = "";
                    }
                    ++count;
                    Log.Debug( $"{count}: {logText}");
                }
                Log.Debug("PAYLOAD END");
            }
            catch
            {
                Log.Info("Cannot deserialize args.purchasedProduct.receipt");
            }
            return payload;
        }

        private string ParsePurchaseTokenFromPlayStoreReceipt( string receiptString  )
        {
            string payload = "";   
            string purchaseToken = "";
            
            try
            {
                IAPReceipt2 receipt = JsonConvert.DeserializeObject<IAPReceipt2>(receiptString);                
                
                Log.Debug("IAPReceipt");
                string log = "";
                log += "receipt.TransactionID: " + receipt.TransactionID;
                log += "\n";
                log += "receipt.Store: " + receipt.Store;
                log += "\n";
                log += "Payload: " + receipt.Payload;
                Log.Debug(log);
                payload = receipt.Payload;
            }
            catch
            {
                Log.Info("Cannot deserialize args.purchasedProduct.receipt");
            }
            
            if( !string.IsNullOrEmpty(payload) )
            {
                string json = "";
                try
                {
                    ReceiptPayloadStr rPayload = JsonConvert.DeserializeObject<ReceiptPayloadStr>(payload);
                    Log.Debug("json: " + rPayload.json);
                    json = rPayload.json;
                }
                catch
                {
                    Log.Info("Cannot deserialize payload str");
                }
                
                if (!string.IsNullOrEmpty(json))
                {
                    try
                    {
                        ReceiptJSON rJson = JsonConvert.DeserializeObject<ReceiptJSON>(json);
                        purchaseToken = rJson.purchaseToken;
                        Log.Debug("purchaseToken: " + purchaseToken);
                    }
                    catch
                    {
                        Log.Info("Cannot deserialize rJson");
                    }
                }
            }

            return purchaseToken;
        }
        
        public class FiatValidationDataGoogleStore
        {
            public string productId;
            public string purchaseToken;
            public string transactionId;
            public string storeName;
        }
        
        public class FiatValidationDataAppleStore
        {
            public string productId;
            public string transactionId;
            public string receiptData;
            public string storeName;
        }

        public class IAPReceipt
        {
            public string Store;
            public string TransactionID;
            public ReceiptPayload Payload;
        }
        public class IAPReceipt2
        {
            public string Store;
            public string TransactionID;
            public string Payload;
        }
        public class ReceiptPayload
        {
            public ReceiptJSON json;
        }
         public class ReceiptPayloadStr
        {
            public string json;
        }
        public class ReceiptJSON
        {
            public string productId;
            public string purchaseToken;
        }
#endif
        
#endregion
        
#region Util

        public void PlayClickSound()
        {
            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
        }
        
        public void OpenAlertDialog(string msg)
        {
            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.CHANGE_SCREEN, Constants.SfxSoundVolume,
                false, false, true);
            _uiManager.DrawPopup<WarningPopup>(msg);
        }

#endregion
    }
}
