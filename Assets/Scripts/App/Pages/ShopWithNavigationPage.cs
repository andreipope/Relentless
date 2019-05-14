using System;
using System.Text;
using System.IO;
using System.Collections;
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

        private List<FiatBackendManager.FiatTransactionResponse> _fiatTransactionRecordQueqe;
        
        public enum State
        {
            None = -1,
            WaitForInput = 0,
            Purchasing = 1,
            RequestFiatValidation = 2,
            RequestFiatTransaction = 3,
            RequestPack = 4,
            WaitForRequestPackResponse = 5,
            RequestFiatClaim = 6,
            TransitionToPackOpener = 7,
        }
        
        private State _state;

        private Coroutine _requestPackTimeoutCoroutine;

        private const int RequestPackTimeout = 20;
        
        private readonly Queue<Action> _executeOnMainThread = new Queue<Action>();
        
        #region IUIElement
        
        public void Init()
        {
            _uiManager = GameClient.Get<IUIManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();

            _itemButtonList = new List<Button>();
            _textItemNameList = new List<TextMeshProUGUI>();
            _textItemPriceList = new List<TextMeshProUGUI>();
            _fiatTransactionRecordQueqe = new List<FiatBackendManager.FiatTransactionResponse>();

            InitPurchaseLogic();
            LoadProductData();
            
            _state = State.None;
        }

        public void Update()
        {
            if (_selfPage != null)
            {
                while (_executeOnMainThread.Count > 0)
                {
                    _executeOnMainThread.Dequeue().Invoke();
                }
            }
            
            if (_state == State.RequestPack)
            {
                if (_fiatTransactionRecordQueqe.Count > 0)
                {                    
                    RequestPack(_fiatTransactionRecordQueqe[0]);
                }
                else
                {
                    ChangeState
                    (
                        _selfPage == null ? State.None : State.WaitForInput
                    );
                }
            }
        }

        public void Show()
        {
            _selfPage = Object.Instantiate(
                _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Pages/MyShopPage"));
            _selfPage.transform.SetParent(_uiManager.Canvas.transform, false);            

            _fiatTransactionRecordQueqe.Clear();
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
            ResetState();
            
            if 
            (
                !string.IsNullOrEmpty
                (
                    GameClient.Get<BackendDataControlMediator>().UserDataModel.AccessToken
                )
            )
            {
                _uiManager.GetPage<ShopWithNavigationPage>().RequestFiatTransaction();
            }
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
            
            if (_requestPackTimeoutCoroutine != null)
            {
                MainApp.Instance.StopCoroutine(_requestPackTimeoutCoroutine);
            }
            _requestPackTimeoutCoroutine = null;
        }
        
        public void Dispose()
        {
            if (_itemButtonList != null)
                _itemButtonList.Clear();  
                
            if (_textItemNameList != null)
                _textItemNameList.Clear();  
                
            if (_textItemPriceList != null)
                _textItemPriceList.Clear();  
                
            _fiatTransactionRecordQueqe.Clear();
        }
        
        #endregion
        
        #region UI Handler
        
        private void BuyButtonHandler( int id )
        {
            #if UNITY_IOS || UNITY_ANDROID && !UNITY_EDITOR
            ChangeState(State.Purchasing);            
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
        
        private void ResetState()
        {
            _state = State.None;
            ChangeState(State.WaitForInput);
        }

        private void ChangeState(State newState)
        {
            #if UNITY_IOS || UNITY_ANDROID
            if (_selfPage == null)
            {
                GameClient.Get<IAppStateManager>().ChangeAppState(Enumerators.AppState.SHOP);
            }
            
            if(_state == newState)
                return;

            Log.Info($"ChangeState: prev:{_state.ToString()} next:{newState.ToString()}");
            
            if (_requestPackTimeoutCoroutine != null)
            {
                MainApp.Instance.StopCoroutine(_requestPackTimeoutCoroutine);
            }
            _requestPackTimeoutCoroutine = null;
            
            _state = newState;
            switch(_state)
            {
                case State.WaitForInput:
                    _uiManager.HidePopup<LoadingFiatPopup>();
                    break;
                case State.Purchasing:
                    _uiManager.DrawPopup<LoadingFiatPopup>("Activating Purchase...");
                    break;
                case State.RequestFiatValidation:
                    _uiManager.DrawPopup<LoadingFiatPopup>("Processing payment...");
                    break;
                case State.RequestFiatTransaction:
                    _uiManager.DrawPopup<LoadingFiatPopup>("Fetching your packs");
                    RequestFiatTransaction();
                    break;
                case State.RequestPack:
                    _uiManager.DrawPopup<LoadingFiatPopup>("Fetching your packs.");                                        
                    break;
                case State.WaitForRequestPackResponse:
                    _uiManager.DrawPopup<LoadingFiatPopup>("Fetching your packs..");
                    _requestPackTimeoutCoroutine = MainApp.Instance.StartCoroutine(RequestPackTimeoutAsync());
                    break;
                case State.RequestFiatClaim:
                    _uiManager.DrawPopup<LoadingFiatPopup>("Fetching your packs...");
                    break;
                case State.TransitionToPackOpener:
                    OnFinishRequestPack();
                    break;      
                default:
                    break;
            }
            #endif
        }

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

        public void InitPurchaseLogic()
        {
            _fiatBackendManager = GameClient.Get<FiatBackendManager>();
            _fiatPlasmaManager = GameClient.Get<FiatPlasmaManager>();
            
            _inAppPurchaseManager = GameClient.Get<IInAppPurchaseManager>();
#if UNITY_IOS || UNITY_ANDROID
            _inAppPurchaseManager.ProcessPurchaseAction += OnProcessPurchase;
            _inAppPurchaseManager.PurchaseFailedOrCanceled += OnPurchaseFailedOrCanceled;

            _fiatPlasmaManager.OnRequestPackSuccess += OnRequestPackSuccess;
            _fiatPlasmaManager.OnRequestPackFailed += OnRequestPackFailed;
#endif
        }
        
#if UNITY_IOS || UNITY_ANDROID
        private async void RequestFiatValidationGoogle()
        {
            Log.Info($"{nameof(RequestFiatValidationGoogle)}");
            ChangeState(State.RequestFiatValidation);
            
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

            ChangeState(State.RequestFiatTransaction);
        }
        
        private void WarningPopupRequestFiatValidationGoogle()
        {            
            _uiManager.GetPopup<WarningPopup>().ConfirmationReceived -= WarningPopupRequestFiatValidationGoogle;
            RequestFiatValidationGoogle();
        }

        private async void RequestFiatValidationApple()
        {
            Log.Info($"{nameof(RequestFiatValidationApple)}");
            ChangeState(State.RequestFiatValidation);
            
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
            
            ChangeState(State.RequestFiatTransaction);
        }
        
        private void WarningPopupRequestFiatValidationApple()
        {            
            _uiManager.GetPopup<WarningPopup>().ConfirmationReceived -= WarningPopupRequestFiatValidationApple;
            RequestFiatValidationApple();
        }
        
        public async void RequestFiatTransaction()
        {
            Log.Info($"{nameof(RequestFiatTransaction)}");
            
            bool success;
            List<FiatBackendManager.FiatTransactionResponse> recordList = new List<FiatBackendManager.FiatTransactionResponse>();
            try
            {    
                try
                {
                    recordList = await _fiatBackendManager.CallFiatTransaction();
                }
                catch (Exception e)
                {
                    Log.Info($"{nameof(_fiatBackendManager.CallFiatTransaction)} failed: {e.Message}");
                    throw new Exception($"{nameof(_fiatBackendManager.CallFiatTransaction)} failed: {e.Message}");
                }
                success = true;
            }
            catch(Exception e)
            {
                Log.Info($"{nameof(RequestFiatTransaction)} failed: {e.Message}");
                success = false;                
            }
            
            if(success)
            {
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
                
                if (recordList.Count > 0)
                {
                    _fiatTransactionRecordQueqe = recordList.ToList();
                    ChangeState(State.RequestPack);
                }
                else
                {
                    ChangeState(State.WaitForInput);
                }
            }
            else
            {
                _uiManager.HidePopup<LoadingFiatPopup>();
                _uiManager.GetPopup<QuestionPopup>().ConfirmationReceived += WarningPopupRequestFiatTransaction;                
                _uiManager.DrawPopup<QuestionPopup>("Something went wrong.\nPlease try again.");
            }
        }
        
        private void WarningPopupRequestFiatTransaction(bool status)
        {
            _uiManager.GetPopup<QuestionPopup>().ConfirmationReceived -= WarningPopupRequestFiatTransaction;

            ChangeState(State.WaitForInput);
            if(status)
            {
                ChangeState(State.RequestFiatTransaction);
            }
        }
        
        private async void RequestPack(FiatBackendManager.FiatTransactionResponse record)
        {
            Log.Debug($"{nameof(RequestPack)} UserId: {record.UserId}, TxID: {record.TxID}");
            ChangeState(State.WaitForRequestPackResponse);
            bool success;
            bool isRequestFiatClaim = false;
            try
            {  
                try
                {
                    await _fiatPlasmaManager.CallRequestPacksContract(record);
                    Log.Debug($"Contract [requestPacks] success call.");
                }
                catch (Exception e)
                {                        
                    //If a record was already claimed for pack, contract request would also failed at this point
                    Log.Debug($"Contract [requestPacks] failed");
                    Log.Debug($"e: {e.Message}");
                    if
                    (
                        e.Message.Contains("reverted") ||
                        e.Message.Contains("already exists")
                    )
                    {
                        isRequestFiatClaim = true;                        
                    }
                }
                
                
                success = true;
            }
            catch(Exception e)
            {
                Log.Info($"{nameof(RequestPack)} failed: {e.Message}");
                success = false;
            }
            
            if(success)
            {
                if(isRequestFiatClaim)
                {
                    RequestFiatClaim
                    (
                        record.UserId,
                        record.TxID
                    );
                }
                _fiatTransactionRecordQueqe.Remove(record);
            }
            else
            {
                _uiManager.HidePopup<LoadingFiatPopup>();
                _uiManager.GetPopup<QuestionPopup>().ConfirmationReceived += WarningPopupRequestPack;                
                _uiManager.DrawPopup<QuestionPopup>("Something went wrong.\nPlease try again.");
            }
        }

        private IEnumerator RequestPackTimeoutAsync()
        {
            WaitForSeconds wait = new WaitForSeconds(1f);
            for(int i = 0; i < RequestPackTimeout && _state == State.WaitForRequestPackResponse; ++i)
            {
                yield return new WaitForSeconds(1f);
            }
            if(_state == State.WaitForRequestPackResponse)
            {
                Log.Info($"{nameof(RequestPack)} timeout");
                _uiManager.HidePopup<LoadingFiatPopup>();
                _uiManager.GetPopup<QuestionPopup>().ConfirmationReceived += WarningPopupRequestPack;                
                _uiManager.DrawPopup<QuestionPopup>("Something went wrong.\nPlease try again.");
            }
        }
        
        private void WarningPopupRequestPack(bool status)
        {
            _uiManager.GetPopup<QuestionPopup>().ConfirmationReceived -= WarningPopupRequestPack;

            if(status)
            {
                ChangeState(State.RequestFiatTransaction);
            }           
        }
        
        private void OnRequestPackSuccess(FiatPlasmaManager.ContractRequest contractRequest)
        {
            Log.Debug($"{nameof(_fiatPlasmaManager.OnRequestPackSuccess)}");
            _executeOnMainThread.Enqueue(() => 
            {  
                ChangeState(State.RequestFiatClaim);                
                RequestFiatClaim
                (
                    contractRequest.UserId,
                    contractRequest.TxID
                );
            });            
        }
        
        private void OnRequestPackFailed()
        {
            Log.Info($"{nameof(_fiatPlasmaManager.OnRequestPackFailed)} failed");
            _executeOnMainThread.Enqueue(() => 
            {
                ChangeState(State.WaitForInput); 
                _uiManager.HidePopup<LoadingFiatPopup>();
                _uiManager.GetPopup<QuestionPopup>().ConfirmationReceived += WarningPopupRequestPack;                
                _uiManager.DrawPopup<QuestionPopup>("Something went wrong.\nPlease try again.");
            });            
        }
        
        public async void RequestFiatClaim(int userId, int txId)
        {
            Log.Debug($"{nameof(RequestFiatClaim)} for UserID:{userId} TxID:{txId}");
            bool success;
            try
            {
                await _fiatBackendManager.CallFiatClaim
                (
                    userId,
                    new List<int>
                    {
                        txId
                    }
                );

                success = true;                
            }
            catch (Exception e)
            {
                Log.Debug($"{nameof(_fiatBackendManager.CallFiatClaim)} failed. e:{e.Message}");
                success = false;
                
            }
            
            if(success)
            {
                FiatBackendManager.FiatTransactionResponse record = _fiatTransactionRecordQueqe.Find(x => x.TxID == txId);
                if (record != null)
                {
                    _fiatTransactionRecordQueqe.Remove(record);
                }
                
                ChangeState
                (
                    _fiatTransactionRecordQueqe.Count > 0 ? State.RequestPack : State.TransitionToPackOpener
                );
            }
            else
            {
                ChangeState(State.WaitForInput);
                _uiManager.HidePopup<LoadingFiatPopup>();
                _uiManager.GetPopup<QuestionPopup>().ConfirmationReceived += WarningPopupFiatClaim;                
                _uiManager.DrawPopup<QuestionPopup>("Something went wrong.\nPlease try again.");
            }
        }
        
        private void WarningPopupFiatClaim(bool status)
        {
            _uiManager.GetPopup<QuestionPopup>().ConfirmationReceived -= WarningPopupFiatClaim;
            
            if(status)
            {
                ChangeState(State.RequestFiatTransaction);
            }  
        }

        private async void OnFinishRequestPack()
        {
            Log.Debug("SUCCESSFULLY REQUEST for packs");
            await _uiManager.GetPage<PackOpenerPageWithNavigationBar>().RetrievePackBalanceAmount((int)Enumerators.MarketplaceCardPackType.Booster);
            _uiManager.DrawPopup<LoadingFiatPopup>($"Successfully request for pack(s).");
            await Task.Delay(TimeSpan.FromSeconds(1f));
            _uiManager.HidePopup<LoadingFiatPopup>();
            await Task.Delay(TimeSpan.FromSeconds(0.2f));
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
            ChangeState(State.WaitForInput);
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
