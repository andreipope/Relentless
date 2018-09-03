namespace LoomNetwork.CZB.BackendCommunication
{
    public class UserDataModel
    {
        public string UserId;

        public string BetaKey;

        public byte[] PrivateKey;

        public bool IsValid;

        public UserDataModel(string userId, string betaKey, byte[] privateKey)
        {
            UserId = userId;
            BetaKey = betaKey;
            PrivateKey = privateKey;
        }
    }
}
