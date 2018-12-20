using System.Threading.Tasks;
using Loom.Client;
using Loom.Client.Protobuf;
using Loom.Google.Protobuf;
using UnityEngine;

namespace Loom.ZombieBattleground.BackendCommunication
{
    public class LoggingNonceTxMiddleware : NonceTxMiddleware
    {
        public LoggingNonceTxMiddleware(byte[] publicKey, DAppChainClient client) : base(publicKey, client)
        {
        }

        public override async Task<byte[]> Handle(byte[] txData)
        {
            var nonce = await this.Client.GetNonceAsyncNonBlocking(this.publicKeyHex);
            var tx = new NonceTx
            {
                Inner = ByteString.CopyFrom(txData),
                Sequence = nonce + 1
            };

            Debug.Log($"NonceLog: Node returned nonce {nonce}, using nonce {tx.Sequence}");

            return tx.ToByteArray();
        }
    }
}
