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

        private FiatBackendManager.FiatProduct _productData;

        private string _currencyPrefix;

        private List<FiatBackendManager.FiatTransactionResponse> _cacheFiatTransactionRecordList;
        
        #region IUIElement
        
        public void Init()
        {
            _uiManager = GameClient.Get<IUIManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();

            _itemButtonList = new List<Button>();
            _textItemNameList = new List<TextMeshProUGUI>();
            _textItemPriceList = new List<TextMeshProUGUI>();
            _cacheFiatTransactionRecordList = new List<FiatBackendManager.FiatTransactionResponse>();

            InitPurchaseLogic();
            LoadProductData();         
        }

        public void Update()
        {
        }

        public void Show()
        {
            _selfPage = Object.Instantiate(
                _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Pages/MyShopPage"));
            _selfPage.transform.SetParent(_uiManager.Canvas.transform, false);            

            _cacheFiatTransactionRecordList.Clear();
            UpdatePageScaleToMatchResolution();
            
            _uiManager.DrawPopup<SideMenuPopup>(SideMenuPopup.MENU.SHOP);
            _uiManager.DrawPopup<AreaBarPopup>();
            
            if(!CheckIfProductDataAvailable())
            {
                ReloadProductData();
                _uiManager.DrawPopup<WarningPopup>($"Cannot load product data\n Please try again");                
                return;
            }
            LoadItems();
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
                
            _cacheFiatTransactionRecordList.Clear();
        }
        
        #endregion
        
        #region UI Handler
        
        private void BuyButtonHandler( int id )
        {
            #if UNITY_IOS || UNITY_ANDROID && !UNITY_EDITOR
            _uiManager.DrawPopup<LoadingFiatPopup>("Activating Purchase...");
            _inAppPurchaseManager.BuyProductID(_productData.packs[id].store_id);
            #else
            _uiManager.GetPopup<QuestionPopup>().ConfirmationReceived += ConfirmRedirectMarketplaceLink;
            _uiManager.DrawPopup<QuestionPopup>("Would you like to visit the Marketplace website?"); 
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
        
        private bool CheckIfProductDataAvailable()
        {
            return _productData != null;
        }
        
        private async void ReloadProductData()
        {
            _uiManager.DrawPopup<LoadingFiatPopup>($"Loading products data");
            await LoadProductData();
            _uiManager.HidePopup<LoadingFiatPopup>();
            GameClient.Get<IAppStateManager>().ChangeAppState(Enumerators.AppState.MAIN_MENU);
        }

        private void LoadItems()
        {
            string path = "Panel_Content/Mask/Group_Packs";
            
            _itemButtonList.Clear();
            _textItemNameList.Clear();
            _textItemPriceList.Clear();

            int maxPackObject = _selfPage.transform.Find(path).childCount;
            
            for (int i = 0; i < _productData.packs.Length && i < maxPackObject; ++i)
            {
                int index = i;
                
                Button button = _selfPage.transform.Find($"{path}/Node_Pack_{i}/Button_Pack").GetComponent<Button>();
                button.transform.parent.gameObject.SetActive(true);
                _itemButtonList.Add(button);
                button.onClick.AddListener(()=>
                {
                    PlayClickSound();
                    BuyButtonHandler(index);
                });

                FiatBackendManager.FiatProductPack packData = _productData.packs[i];
                
                TextMeshProUGUI textName = _selfPage.transform.Find($"{path}/Node_Pack_{i}/Text_PackName").GetComponent<TextMeshProUGUI>();
                textName.text = packData.display_name;
                _textItemNameList.Add(textName);
                
                TextMeshProUGUI textPrice = _selfPage.transform.Find($"{path}/Node_Pack_{i}/Text_Price").GetComponent<TextMeshProUGUI>();
                textPrice.text = _currencyPrefix + (packData.price / (float)_productData.unit_percent).ToString("n2");
                _textItemPriceList.Add(textPrice);
            }
        }
        
        private async Task LoadProductData()
        {
            FiatBackendManager.FiatProductResponse fiatProductResponse;
            try
            {
                fiatProductResponse = await _fiatBackendManager.CallFiatProducts();
            }
            catch(Exception e)
            {
                Log.Info($"{nameof(_fiatBackendManager.CallFiatProducts)} failed: {e.Message}");                
                return;
            }

            try
            {
#if UNITY_ANDROID
                _productData = fiatProductResponse.products.First(x => x.store == "PlayStore");
#elif UNITY_IOS
                _productData = fiatProductResponse.products.First(x => x.store == "AppStore");
#else
                _productData = fiatProductResponse.products.First(x => x.store == "MarketPlace"); 
#endif
            }
            catch(Exception e)
            {
                Log.Info($"Parsing product data failed: {e.Message}");                
                return;
            }

            _currencyPrefix = string.Equals(_productData.currency, "USD") ? "$" : "";
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
            _inAppPurchaseManager.PurchaseFailedOrCanceled += OnPurchaseFailedOrCanceled;
            _finishRequestPack = OnFinishRequestPack;
#endif
        }
        
#if UNITY_IOS || UNITY_ANDROID
        private async void RequestFiatValidationGoogle()
        {
            Log.Info($"{nameof(RequestFiatValidationGoogle)}");
            _uiManager.DrawPopup<LoadingFiatPopup>("Processing payment...");
            
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
                _uiManager.DrawPopup<WarningPopup>("Something went wrong.\nPlease try again.");
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
            Log.Info($"{nameof(RequestFiatValidationApple)}");
            _uiManager.DrawPopup<LoadingFiatPopup>("Processing payment...");
            
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
                _uiManager.DrawPopup<WarningPopup>("Something went wrong.\nPlease try again.");
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
        
        public async void RequestFiatTransaction(bool showLoadingPopup = true)
        {
            Log.Info($"{nameof(RequestFiatTransaction)}");
            try
            {
                if (showLoadingPopup)
                {
                    _uiManager.DrawPopup<LoadingFiatPopup>("Processing payment...");
                }

                List<FiatBackendManager.FiatTransactionResponse> recordList = null;
                try
                {
                    recordList = await _fiatBackendManager.CallFiatTransaction();
                }
                catch (Exception e)
                {
                    Log.Info($"{nameof(_fiatBackendManager.CallFiatTransaction)} failed: {e.Message}");
                    throw new Exception($"{nameof(_fiatBackendManager.CallFiatTransaction)} failed: {e.Message}");
                }

                recordList.Sort((FiatBackendManager.FiatTransactionResponse resA, FiatBackendManager.FiatTransactionResponse resB) =>
                {
                   return resB.TxID - resA.TxID;
                });
                string log = "TxID: ";
                foreach (var i in recordList)
                {
                    log += i.TxID + ", ";
                }
                Log.Debug(log);
                if (showLoadingPopup)
                {
                    _uiManager.HidePopup<LoadingFiatPopup>();
                }
                if (recordList.Count > 0)
                {
                    _cacheFiatTransactionRecordList = recordList.ToList();
                    RequestPack(recordList);
                }
            }
            catch(Exception e)
            {
                Log.Info($"{nameof(RequestFiatTransaction)} failed: {e.Message}");
                _uiManager.HidePopup<LoadingFiatPopup>();
                _uiManager.GetPopup<QuestionPopup>().ConfirmationReceived += WarningPopupRequestFiatTransaction;                
                _uiManager.DrawPopup<QuestionPopup>("Something went wrong.\nPlease try again.");
            }
        }
        
        private void WarningPopupRequestFiatTransaction(bool status)
        {
            _uiManager.GetPopup<QuestionPopup>().ConfirmationReceived -= WarningPopupRequestFiatTransaction;              

            if(status)
            {
                RequestFiatTransaction();
            }
        }
        
        private async void RequestPack(List<FiatBackendManager.FiatTransactionResponse> recordList)
        {            
            Log.Debug("START REQUEST for packs");  
            Log.Debug($"recordList: {recordList.Count}");

            try
            {
                for (int i = 0; i < recordList.Count; ++i)
                {
                    FiatBackendManager.FiatTransactionResponse record = recordList[i];

                    Log.Debug($"Request Pack UserId: {record.UserId}, TxID: {record.TxID}");
                    _uiManager.DrawPopup<LoadingFiatPopup>("Fetching your packs...");

                    string eventResponse = "";

                    try
                    {
                        eventResponse = await _fiatPlasmaManager.CallRequestPacksContract(record);
                        Log.Debug($"Contract [requestPacks] success call.");
                    }
                    catch (Exception e)
                    {                        
                        Log.Debug($"Contract [requestPacks] failed");
                        Log.Debug($"e: {e.Message}");
                    }
                    
                    Log.Debug($"EVENT RESPONSE: {eventResponse}");
                    _cacheFiatTransactionRecordList.Remove(record);

                    if(!string.IsNullOrEmpty(eventResponse))
                    {
                        Log.Debug($"CallFiatClaim for UserId:{record.UserId} TxID:{record.TxID}");
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

                _finishRequestPack?.Invoke();
            }
            catch(Exception e)
            {
                Log.Info($"{nameof(RequestPack)} failed: {e.Message}");
                _uiManager.HidePopup<LoadingFiatPopup>();
                _uiManager.GetPopup<QuestionPopup>().ConfirmationReceived += WarningPopupRequestPack;                
                _uiManager.DrawPopup<QuestionPopup>("Something went wrong.\nPlease try again.");
            }
        }
        
        private void WarningPopupRequestPack(bool status)
        {
            _uiManager.GetPopup<QuestionPopup>().ConfirmationReceived -= WarningPopupRequestPack;        
            
            if(status && _cacheFiatTransactionRecordList.Count > 0)
            {
                RequestPack(_cacheFiatTransactionRecordList);
            }               
        }

        private async void OnFinishRequestPack()
        {
            Log.Debug("SUCCESSFULLY REQUEST for packs");
            await _uiManager.GetPage<PackOpenerPageWithNavigationBar>().RetrievePackBalanceAmount((int)Enumerators.MarketplaceCardPackType.Booster);
            _uiManager.DrawPopup<LoadingFiatPopup>($"Successfully request for pack(s).");
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

        private void OnPurchaseFailedOrCanceled()
        {
            _uiManager.HidePopup<LoadingFiatPopup>();
            OpenAlertDialog("Purchasing failed or canceled.");
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
