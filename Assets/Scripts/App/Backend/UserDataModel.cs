namespace Loom.ZombieBattleground.BackendCommunication
{
    public class UserDataModel
    {
        public string UserId;

        public byte[] PrivateKey;

        public bool IsValid;

        public UserDataModel(string userId, byte[] privateKey)
        {
            UserId = userId;
            PrivateKey = privateKey;
        }
    }
}
