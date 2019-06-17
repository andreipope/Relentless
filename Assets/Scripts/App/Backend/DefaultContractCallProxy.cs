using System.Threading.Tasks;
using Loom.Client;
using Loom.Google.Protobuf;

namespace Loom.ZombieBattleground.BackendCommunication
{
    public class DefaultContractCallProxy<TChainEvent> : IContractCallProxy
    {
        public Contract<TChainEvent> Contract { get; }

        public DefaultContractCallProxy(Contract<TChainEvent> contract)
        {
            Contract = contract;
        }

        public async Task CallAsync(string method, IMessage args)
        {
            await Contract.CallAsync(method, args);
        }

        public async Task<T> CallAsync<T>(string method, IMessage args) where T : IMessage, new()
        {
            return await Contract.CallAsync<T>(method, args);
        }

        public async Task<T> StaticCallAsync<T>(string method, IMessage args) where T : IMessage, new()
        {
            return await Contract.StaticCallAsync<T>(method, args);
        }

        public void Dispose()
        {
        }
    }
}
