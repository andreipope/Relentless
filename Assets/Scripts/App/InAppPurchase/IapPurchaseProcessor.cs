using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using log4net;
using Loom.Client;
using OneOf;
using OneOf.Types;
using UnityEngine.Purchasing;

#if UNITY_ANDROID || UNITY_IOS
using Loom.Newtonsoft.Json;
using UnityEngine.Assertions;
using UnityEngine.Purchasing.Security;
#endif

namespace Loom.ZombieBattleground.Iap
{
    /// <summary>
    /// Implements the part of purchase flow responsible for communication with Marketplace and PlasmaChain.
    /// </summary>
    public class IapPurchaseProcessor
    {
        private static readonly ILog Log = Logging.GetLog(nameof(IapPurchaseProcessor));

        private readonly AuthFiatApiFacade _authFiatApiFacade;
        private readonly PlasmaChainBackendFacade _plasmaChainBackendFacade;
        private readonly DAppChainClient _plasmaChainClient;
        private readonly PurchaseStateChangedHandler _setStateAction;

        public IapPurchaseProcessor(
            AuthFiatApiFacade authFiatApiFacade,
            PlasmaChainBackendFacade plasmaChainBackendFacade,
            DAppChainClient plasmaChainClient,
            PurchaseStateChangedHandler stateAction)
        {
            _authFiatApiFacade = authFiatApiFacade;
            _plasmaChainBackendFacade = plasmaChainBackendFacade;
            _plasmaChainClient = plasmaChainClient;
            _setStateAction = stateAction;
        }

        public async Task<OneOf<Success, IapPurchaseProcessingError, IapException>> ProcessPurchase(string receipt, Product product)
        {
#if UNITY_ANDROID || UNITY_IOS
            FiatValidationData fiatValidationData;
            try
            {
#if UNITY_ANDROID
                GooglePlayReceipt googlePlayReceipt = IapReceiptParser.ParseGooglePlayReceipt(receipt);
                Log.Debug($"{nameof(ProcessPurchase)}: GooglePlayReceipt:\n" +
                    JsonUtility.PrettyPrint(JsonConvert.SerializeObject(googlePlayReceipt)));
                fiatValidationData =
                    new FiatValidationDataPlayStore(
                        googlePlayReceipt.productID,
                        googlePlayReceipt.transactionID,
                        googlePlayReceipt.purchaseToken
                    );
#elif UNITY_IOS
                AppleReceipt appleReceipt = IapReceiptParser.ParseAppleReceipt(receipt);
                Log.Debug($"{nameof(ProcessPurchase)}: AppleReceipt:\n" + JsonUtility.PrettyPrint(JsonConvert.SerializeObject(appleReceipt)));
                AppleInAppPurchaseReceipt matchingReceipt =
                    product == null ?
                        appleReceipt.inAppPurchaseReceipts[0] :
                        appleReceipt.inAppPurchaseReceipts.SingleOrDefault(r => r.transactionID == product.transactionID);

                if (matchingReceipt == null)
                    throw new KeyNotFoundException($"Receipt for transactionID {product?.transactionID ?? "null"} not found");

                fiatValidationData =
                    new FiatValidationDataAppStore(
                        matchingReceipt.productID,
                        matchingReceipt.transactionID,
                        IapReceiptParser.ParseRawReceipt(receipt, "AppleAppStore").Payload
                    );
#endif
            }
            catch (Exception e)
            {
                Log.Info($"{nameof(ProcessPurchase)} failed: " + e);
                return new IapException("Failed to process transaction", e);
            }

            return await RequestFiatValidation(fiatValidationData);
#else
            await Task.CompletedTask;
            return IapPurchaseProcessingError.UnsupportedPlatform;
#endif
        }

        /// <summary>
        /// Sends transaction data to Marketplace for validation and registration, proceeds to the next step on success.
        /// </summary>
        /// <param name="fiatValidationData"></param>
        /// <returns></returns>
        private async Task<OneOf<Success, IapPurchaseProcessingError, IapException>> RequestFiatValidation(FiatValidationData fiatValidationData)
        {
            Log.Info($"{nameof(RequestFiatValidation)}");
            SetState(IapPurchaseState.RequestingFiatValidation, null);

            AuthFiatApiFacade.ValidationResponse validationResponse;
            try
            {
                validationResponse = await _authFiatApiFacade.RegisterTransactionAndValidate(fiatValidationData);
            }
            catch (Exception e)
            {
                Log.Info($"{nameof(RequestFiatValidation)} failed: " + e);
                return IapPurchaseProcessingError.ValidationFailed;
            }

            return await RequestFiatTransaction(validationResponse.txId);
        }

        /// <summary>
        /// Checks if the transaction is registered on Marketplace, proceeds to the next step on success.
        /// </summary>
        /// <param name="txId"></param>
        /// <returns></returns>
        public async Task<OneOf<Success, IapPurchaseProcessingError, IapException>> RequestFiatTransaction(int txId)
        {
            Log.Info($"{nameof(RequestFiatTransaction)}(int txId = {txId})");
            SetState(IapPurchaseState.RequestingFiatTransaction, null);
            AuthFiatApiFacade.TransactionResponse matchingTx;
            try
            {
                List<AuthFiatApiFacade.TransactionResponse> recordList = await _authFiatApiFacade.ListPendingTransactions();
                recordList.Sort((resA, resB) => resB.TxID - resA.TxID);
                Log.Debug($"{nameof(RequestFiatTransaction)}: received TxIDs " + Utilites.FormatCallLogList(recordList.Select(tr => tr.TxID)));
                matchingTx = recordList.SingleOrDefault(record => record.TxID == txId);
                if (matchingTx == null)
                    return IapPurchaseProcessingError.TxNotRegistered;
            }
            catch (Exception e)
            {
                Log.Info($"{nameof(RequestFiatTransaction)} failed: " + e);
                return new IapException("Failed to process transaction", e);
            }

            return await RequestPack(matchingTx);
        }

        /// <summary>
        /// Checks the pack on PlasmaChain, proceeds to the next step on success.
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        private async Task<OneOf<Success, IapPurchaseProcessingError, IapException>> RequestPack(AuthFiatApiFacade.TransactionResponse record)
        {
            Log.Debug($"{nameof(RequestPack)}(UserId: {record.UserId}, TxID: {record.TxID})");
            SetState(IapPurchaseState.RequestingPack, null);

            try
            {
                // Claim pack on PlasmaChain
                await _plasmaChainBackendFacade.ClaimPacks(_plasmaChainClient, record);
            }
            catch (TxCommitException e)
            {
                // If the pack was already claimed, call is expected to fail. We can safely ignore this.
                Log.Debug($"{nameof(_plasmaChainBackendFacade.ClaimPacks)} failed, this is expected: " + e);
            }
            catch (IapException e)
            {
                return e;
            }
            catch (Exception e)
            {
                Log.Info($"{nameof(RequestPack)} failed: " + e);
                return new IapException("Failed to request pack", e);
            }

            // Once pack is claimed on PlasmaChain, its record can be removed from Marketplace
            return await AuthClaim(record.UserId, record.TxID);
        }

        /// <summary>
        /// Checks if the transaction is registered on Marketplace, proceeds to the next step on success.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="txId"></param>
        /// <returns></returns>
        private async Task<OneOf<Success, IapPurchaseProcessingError, IapException>> AuthClaim(int userId, int txId)
        {
            Log.Debug($"{nameof(AuthClaim)}(userID = {userId}, txID = {txId})");

            try
            {
                await _authFiatApiFacade.Claim(userId, new[] { txId });
            }
            catch (Exception e)
            {
                Log.Info($"{nameof(AuthClaim)} failed: " + e);
                return new IapException("Claiming pack on Marketplace failed", e);
            }

            return new Success();
        }

        private void SetState(IapPurchaseState state, OneOf<IapPlatformStorePurchaseError, IapPurchaseProcessingError, IapException>? failure)
        {
            _setStateAction.Invoke(state, failure);
        }
    }
}
