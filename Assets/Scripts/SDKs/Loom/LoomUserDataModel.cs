public class LoomUserDataModel
{
    public string UserId;
    public byte[] PrivateKey;
    public bool IsValid;

    public LoomUserDataModel(string userId, byte[] privateKey) {
        UserId = userId;
        PrivateKey = privateKey;
    }
}