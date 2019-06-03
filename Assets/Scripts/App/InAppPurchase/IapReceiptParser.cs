using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine.Purchasing.Security;

namespace Loom.ZombieBattleground.Iap
{
    public static class IapReceiptParser
    {
        public static GooglePlayReceipt ParseGooglePlayReceipt(string receiptJson)
        {
            try
            {
                RawPurchaseReceipt rawPurchaseReceipt = ParseRawReceipt(receiptJson, "GooglePlay");
                GooglePlayReceiptWrapper wrapper = JsonConvert.DeserializeObject<GooglePlayReceiptWrapper>(rawPurchaseReceipt.Payload);
                Dictionary<string, object> dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(wrapper.Json);
                dictionary.TryGetValue("orderId", out object orderId);
                dictionary.TryGetValue("packageName", out object packageName);
                dictionary.TryGetValue("productId", out object productId);
                dictionary.TryGetValue("purchaseToken", out object purchaseToken);
                dictionary.TryGetValue("purchaseTime", out object purchaseTimeRaw);
                dictionary.TryGetValue("purchaseState", out object purchaseStateRaw);
                DateTime purchaseTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds((long) purchaseTimeRaw);
                GooglePurchaseState purchaseState = (GooglePurchaseState) (long) purchaseStateRaw;
                return new GooglePlayReceipt(
                    (string) productId,
                    (string) orderId,
                    (string) packageName,
                    (string) purchaseToken,
                    purchaseTime,
                    purchaseState
                );
            }
            catch (Exception e)
            {
                throw new IapException("Failed to parse Google Play receipt", e);
            }
        }

        public static AppleReceipt ParseAppleReceipt(string receiptJson)
        {
            RawPurchaseReceipt rawPurchaseReceipt = ParseRawReceipt(receiptJson, "AppleAppStore");

            AppleReceipt appleReceipt = new AppleReceiptParser().Parse(Convert.FromBase64String(rawPurchaseReceipt.Payload));
            return appleReceipt;
        }

        public static RawPurchaseReceipt ParseRawReceipt(string receiptString, string expectedStore)
        {
            RawPurchaseReceipt rawPurchaseReceipt = JsonConvert.DeserializeObject<RawPurchaseReceipt>(receiptString);
#if !UNITY_EDITOR
            if (rawPurchaseReceipt.Store != expectedStore)
                throw new IapException($"Unexpected store {rawPurchaseReceipt.Store}, expecting {expectedStore}");
#endif

            return rawPurchaseReceipt;
        }

        private class GooglePlayReceiptWrapper
        {
            [JsonProperty("json")]
            public string Json { get; set; }
        }
    }
}
