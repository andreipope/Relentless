using System;
using System.Numerics;
using Loom.Client;

namespace Loom.ZombieBattleground.BackendCommunication
{
    [Serializable]
    public class UserDataModel
    {
        public string UserId { get; }

        public BigInteger UserIdNumber { get; }

        public byte[] PrivateKey { get; }

        public byte[] PublicKey { get; }

        public Address Address { get; }

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
            PublicKey = CryptoUtils.PublicKeyFromPrivateKey(PrivateKey);
            Address = Address.FromPublicKey(PublicKey);
        }

        public override string ToString()
        {
            return $"(UserId: {UserId}, Email: {Email}, IsValid: {IsValid})";
        }
    }
}
