using System.Threading.Tasks;
using Loom.Client;
using Loom.Google.Protobuf;

namespace Loom.ZombieBattleground.BackendCommunication
{
    public class ThreadedTimeMetricsContractCallProxy : TimeMetricsContractCallProxy
    {
        public ThreadedTimeMetricsContractCallProxy(Contract contract, bool enableConsoleLogs, bool storeCallMetrics) : base(contract, enableConsoleLogs, storeCallMetrics)
        {
        }

        public override Task CallAsync(string method, IMessage args)
        {
            return Task.Run(() => base.CallAsync(method, args));
        }

        public override Task<T> CallAsync<T>(string method, IMessage args)
        {
            return Task.Run(() => base.CallAsync<T>(method, args));
        }

        public override Task<T> StaticCallAsync<T>(string method, IMessage args)
        {
            return Task.Run(() => base.StaticCallAsync<T>(method, args));
        }
    }
}
