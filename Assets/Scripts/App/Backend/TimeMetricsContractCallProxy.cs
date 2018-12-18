using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Loom.Client;
using Loom.Google.Protobuf;

namespace Loom.ZombieBattleground.BackendCommunication
{
    public class TimeMetricsContractCallProxy : IContractCallProxy
    {
        private readonly Stopwatch _stopwatch = new Stopwatch();
        private readonly Dictionary<string, CallRoundaboutData> _methodToCallRoundabouts = new Dictionary<string, CallRoundaboutData>();

        public Contract Contract { get; }

        public IReadOnlyDictionary<string, CallRoundaboutData> MethodToCallRoundabouts => _methodToCallRoundabouts;

        public int AverageRoundabout { get; private set; }

        public TimeMetricsContractCallProxy(Contract contract)
        {
            Contract = contract ?? throw new ArgumentNullException(nameof(contract));
        }

        public async Task CallAsync(string method, IMessage args)
        {
            Task call = Contract.CallAsync(method, args);
            await LoggingCall(method, false, call);
        }

        public async Task<T> CallAsync<T>(string method, IMessage args) where T : IMessage, new()
        {
            Task<T> call = Contract.CallAsync<T>(method, args);
            await LoggingCall(method, false, call);
            return await call;
        }

        public async Task<T> StaticCallAsync<T>(string method, IMessage args) where T : IMessage, new()
        {
            Task<T> call = Contract.StaticCallAsync<T>(method, args);
            await LoggingCall(method, true, call);
            return await call;
        }

        private async Task LoggingCall(string method, bool isStatic, Task task)
        {
            bool timedOut = false;
            try
            {
                _stopwatch.Restart();
                await task;
            }
            catch (TimeoutException)
            {
                timedOut = true;
                throw;
            }
            finally
            {
                long elapsedMilliseconds = _stopwatch.ElapsedMilliseconds;
                if (!timedOut)
                {
                    timedOut = elapsedMilliseconds >= Contract.Client.Configuration.CallTimeout;
                }
                LogCallTime(method, elapsedMilliseconds, isStatic, timedOut);
            }
        }

        private void LogCallTime(string method, long callRoundabout, bool isStatic, bool timedOut)
        {
            if (!_methodToCallRoundabouts.TryGetValue(method, out CallRoundaboutData roundaboutData))
            {
                roundaboutData = new CallRoundaboutData();
                _methodToCallRoundabouts.Add(method, roundaboutData);
            }

            roundaboutData.Add(callRoundabout);

            long roundaboutSum = 0;
            long totalEntries = 0;
            foreach (var methodToCallRoundabout in _methodToCallRoundabouts)
            {
                List<long> roundabouts = methodToCallRoundabout.Value.Roundabouts;
                for (int i = 0; i < roundabouts.Count; i++)
                {
                    long roundabout = roundabouts[i];
                    roundaboutSum += roundabout;
                }

                totalEntries += roundabouts.Count;
            }

            AverageRoundabout = (int) (roundaboutSum / (double) totalEntries);

            string log =
                $"{(isStatic ? "Static call" : "Call")} to '{method}' finished in {callRoundabout} ms" +
                //$", average call roundabout {roundaboutData.CallAverageRoundabout} ms" +
                $"{(timedOut ? ", timed out!" : "")}";

            File.AppendAllText("calltimes.txt", log + "\r\n");

            if (timedOut)
            {
                log = "<color=red>" + log + "</color>";
            }

            UnityEngine.Debug.Log(log);
        }

        public class CallRoundaboutData
        {
            public List<long> Roundabouts { get; } = new List<long>();

            public int CallAverageRoundabout { get; private set; }

            public void Add(long roundabout)
            {
                Roundabouts.Add(roundabout);

                long roundaboutSum = 0;
                for (int i = 0; i < Roundabouts.Count; i++)
                {
                    long entry = Roundabouts[i];
                    roundaboutSum += entry;
                }

                CallAverageRoundabout = (int) (roundaboutSum / (double) Roundabouts.Count);
            }
        }
    }
}
