using System;
using System.Numerics;

namespace Loom.ZombieBattleground.BackendCommunication
{
    [Serializable]
    public class UserDataModel
    {
        public string UserId;

        public BigInteger UserIdNumber;

        public byte[] PrivateKey;

        public bool IsValid;

        public bool IsRegistered;

        public string Email;

        public string Password;

        public string GUID;

        public string AccessToken;

        public UserDataModel(string userId, BigInteger userIdNumber, byte[] privateKey)
        {
            UserId = userId;
            UserIdNumber = userIdNumber;
            PrivateKey = privateKey;
        }

        public override string ToString()
        {
            return $"(UserId: {UserId}, Email: {Email}, IsValid: {IsValid})";
        }
    }
}
