namespace Loom.ZombieBattleground.BackendCommunication
{
    public class UserDataModel
    {
        public string UserId;

        public byte[] PrivateKey;

        public bool IsValid;

        public bool IsRegistered;

        public string Email;

        public string Password;

        public UserDataModel(string userId, byte[] privateKey)
        {
            UserId = userId;
            PrivateKey = privateKey;
        }
    }
}
