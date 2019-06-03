namespace Loom.ZombieBattleground.Iap
{
    public class RawPurchaseReceipt
    {
        public string Store { get; }

        public string TransactionId { get; }

        public string Payload { get; }

        public RawPurchaseReceipt(string store, string transactionId, string payload)
        {
            Store = store;
            TransactionId = transactionId;
            Payload = payload;
        }
    }
}
