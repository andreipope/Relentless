using System.Threading.Tasks;
using Loom.Client;
using Loom.Google.Protobuf;

namespace Loom.ZombieBattleground.BackendCommunication
{
    public class ThreadedContractCallProxyWrapper : IContractCallProxy
    {
        private readonly IContractCallProxy _wrappedProxy;

        public IContractCallProxy WrappedProxy => _wrappedProxy;

        public ThreadedContractCallProxyWrapper(IContractCallProxy wrappedProxy)
        {
            _wrappedProxy = wrappedProxy;
        }

        public Task CallAsync(string method, IMessage args)
        {
            return Task.Run(() => _wrappedProxy.CallAsync(method, args));
        }

        public Task<T> CallAsync<T>(string method, IMessage args) where T : IMessage, new()
        {
            return Task.Run(() => _wrappedProxy.CallAsync<T>(method, args));
        }

        public Task<T> StaticCallAsync<T>(string method, IMessage args) where T : IMessage, new()
        {
            return Task.Run(() => _wrappedProxy.StaticCallAsync<T>(method, args));
        }

        public void Dispose()
        {
            _wrappedProxy?.Dispose();
        }
    }
}
