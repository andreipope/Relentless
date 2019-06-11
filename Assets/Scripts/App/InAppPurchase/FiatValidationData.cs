using Newtonsoft.Json;

namespace Loom.ZombieBattleground.Iap
{
    public abstract class FiatValidationData
    {
        [JsonProperty("productId")]
        public string ProductId { get; }

        [JsonProperty("transactionId")]
        public string TransactionId { get; }

        [JsonProperty("storeName")]
        public abstract string StoreName { get; }

        protected FiatValidationData(string productId, string transactionId)
        {
            ProductId = productId;
            TransactionId = transactionId;
        }
    }

    public class FiatValidationDataPlayStore : FiatValidationData
    {
        [JsonProperty("purchaseToken")]
        public string PurchaseToken { get; }

        public override string StoreName => "GooglePlay";

        public FiatValidationDataPlayStore(string productId, string transactionId, string purchaseToken) : base(productId, transactionId)
        {
            PurchaseToken = purchaseToken;
        }
    }

    public class FiatValidationDataAppStore : FiatValidationData
    {
        [JsonProperty("receiptData")]
        public string ReceiptData { get; }

        public override string StoreName => "AppleStore";

        public FiatValidationDataAppStore(string productId, string transactionId, string receiptData) : base(productId, transactionId)
        {
            ReceiptData = receiptData;
        }
    }
}
