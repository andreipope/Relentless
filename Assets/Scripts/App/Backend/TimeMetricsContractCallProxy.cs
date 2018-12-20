using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Loom.Client;
using Loom.Google.Protobuf;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Loom.ZombieBattleground.BackendCommunication
{
    public class TimeMetricsContractCallProxy : IContractCallProxy
    {
        private readonly Stopwatch _stopwatch = new Stopwatch();
        private readonly Dictionary<string, CallRoundaboutData> _methodToCallRoundabouts = new Dictionary<string, CallRoundaboutData>();

        public Contract Contract { get; }

        public bool EnableConsoleLogs { get; set; }

        public bool SaveMetricsToJsonOnExit { get; set; }

        public IReadOnlyDictionary<string, CallRoundaboutData> MethodToCallRoundabouts => _methodToCallRoundabouts;

        public int AverageRoundabout { get; private set; }

        public TimeMetricsContractCallProxy(Contract contract, bool enableConsoleLogs, bool saveMetricsToJsonOnExit)
        {
            Contract = contract ?? throw new ArgumentNullException(nameof(contract));
            EnableConsoleLogs = enableConsoleLogs;
            SaveMetricsToJsonOnExit = saveMetricsToJsonOnExit;

            if (SaveMetricsToJsonOnExit)
            {
                Application.quitting += ApplicationOnQuitting;
            }
        }

        public virtual async Task CallAsync(string method, IMessage args)
        {
            Task call = Contract.CallAsync(method, args);
            await LoggingCall(method, false, call);
        }

        public virtual async Task<T> CallAsync<T>(string method, IMessage args) where T : IMessage, new()
        {
            Task<T> call = Contract.CallAsync<T>(method, args);
            await LoggingCall(method, false, call);
            return await call;
        }

        public virtual async Task<T> StaticCallAsync<T>(string method, IMessage args) where T : IMessage, new()
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
                LogCallTime(method, (int) elapsedMilliseconds, isStatic, timedOut);
            }
        }

        private void LogCallTime(string method, int callRoundabout, bool isStatic, bool timedOut)
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
                List<int> roundabouts = methodToCallRoundabout.Value.Roundabouts;
                for (int i = 0; i < roundabouts.Count; i++)
                {
                    long roundabout = roundabouts[i];
                    roundaboutSum += roundabout;
                }

                totalEntries += roundabouts.Count;
            }

            AverageRoundabout = (int) (roundaboutSum / (double) totalEntries);


            if (EnableConsoleLogs)
            {
                string log =
                    $"{(isStatic ? "Static call" : "Call")} to '{method}' finished in {callRoundabout} ms" +
                    $"{(timedOut ? ", timed out!" : "")}";

#if UNITY_EDITOR
                if (timedOut)
                {
                    log = "<color=red>" + log + "</color>";
                }
#endif

                Debug.Log(log);
            }
        }

        public void Dispose()
        {
            if (SaveMetricsToJsonOnExit)
            {
                Application.quitting -= ApplicationOnQuitting;
            }
        }

        private void ApplicationOnQuitting()
        {
            IDataManager dataManager = GameClient.Get<IDataManager>();
            string json = dataManager.SerializeToJson(_methodToCallRoundabouts, true);
            string path = dataManager.GetPersistentDataPath("CallMetrics.json");
            File.WriteAllText(path, json, Encoding.UTF8);
        }

        public class CallRoundaboutData
        {
            public List<int> Roundabouts { get; } = new List<int>();

            public int Average { get; private set; }

            public int Min { get; private set; } = int.MaxValue;

            public int Max { get; private set; } = int.MinValue;

            public void Add(int roundabout)
            {
                Roundabouts.Add(roundabout);

                long roundaboutSum = 0;
                for (int i = 0; i < Roundabouts.Count; i++)
                {
                    long entry = Roundabouts[i];
                    roundaboutSum += entry;
                }

                Average = (int) (roundaboutSum / (double) Roundabouts.Count);

                if (roundabout < Min)
                {
                    Min = roundabout;
                }

                if (roundabout > Max)
                {
                    Max = roundabout;
                }
            }
        }
    }
}
