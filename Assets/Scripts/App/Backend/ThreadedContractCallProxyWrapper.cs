using System.Threading.Tasks;
using Loom.Client;
using Loom.Google.Protobuf;

namespace Loom.ZombieBattleground.BackendCommunication
{
    public class ThreadedContractCallProxyWrapper : IContractCallProxy
    {
        private readonly IContractCallProxy _wrapperProxy;

        public ThreadedContractCallProxyWrapper(IContractCallProxy wrapperProxy)
        {
            _wrapperProxy = wrapperProxy;
        }

        public Task CallAsync(string method, IMessage args)
        {
            return Task.Run(() => _wrapperProxy.CallAsync(method, args));
        }

        public Task<T> CallAsync<T>(string method, IMessage args) where T : IMessage, new()
        {
            return Task.Run(() => _wrapperProxy.CallAsync<T>(method, args));
        }

        public Task<T> StaticCallAsync<T>(string method, IMessage args) where T : IMessage, new()
        {
            return Task.Run(() => _wrapperProxy.StaticCallAsync<T>(method, args));
        }

        public void Dispose()
        {
            _wrapperProxy?.Dispose();
        }
    }
}
