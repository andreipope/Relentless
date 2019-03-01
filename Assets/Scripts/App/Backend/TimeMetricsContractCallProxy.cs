using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using log4net;
using Loom.Client;
using Loom.Google.Protobuf;
using Newtonsoft.Json;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Loom.ZombieBattleground.BackendCommunication
{
    public class TimeMetricsContractCallProxy : IContractCallProxy
    {
        private static readonly ILog Log = Logging.GetLog(nameof(TimeMetricsContractCallProxy));

        private readonly Dictionary<string, CallRoundaboutData> _methodToCallRoundabouts = new Dictionary<string, CallRoundaboutData>();

        private readonly IDataManager _dataManager;

        public const string CallMetricsFileName = "CallMetrics.json";

        public Contract Contract { get; }

        public bool EnableLogs { get; set; }

        public bool StoreCallMetrics { get; set; }

        public bool LoadCallMetricsOnStartup { get; set; }

        public IReadOnlyDictionary<string, CallRoundaboutData> MethodToCallRoundabouts => _methodToCallRoundabouts;

        public int AverageRoundabout { get; private set; }

        public TimeMetricsContractCallProxy(Contract contract, bool enableLogs, bool storeCallMetrics)
        {
            _dataManager = GameClient.Get<IDataManager>();
            Contract = contract ?? throw new ArgumentNullException(nameof(contract));
            EnableLogs = enableLogs;
            StoreCallMetrics = storeCallMetrics;

            if (StoreCallMetrics)
            {
                Application.quitting += ApplicationOnQuitting;
                if (LoadCallMetricsOnStartup)
                {
                    _dataManager = GameClient.Get<IDataManager>();
                    string path = _dataManager.GetPersistentDataPath(CallMetricsFileName);
                    if (File.Exists(path))
                    {
                        try
                        {
                            string json = File.ReadAllText(path);
                            _methodToCallRoundabouts =
                                JsonConvert.DeserializeObject<Dictionary<string, CallRoundaboutData>>(json)
                                ?? new Dictionary<string, CallRoundaboutData>();
                        }
                        catch (Exception e)
                        {
                            Helpers.ExceptionReporter.LogException(e);
                            Log.Warn("Unable to read call metrics: " + e);
                        }
                    }
                }
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
            Stopwatch stopwatch = Stopwatch.StartNew();
            bool timedOut = false;
            try
            {
                stopwatch.Restart();
                await task;
            }
            catch (TimeoutException e)
            {
                timedOut = true;
                throw;
            }
            finally
            {
                long elapsedMilliseconds = stopwatch.ElapsedMilliseconds;
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

            if (EnableLogs)
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

                if (timedOut)
                {
                    Log.Warn(log);
                }
                else
                {
                    Log.Debug(log);
                }
            }
        }

        public void Dispose()
        {
            if (StoreCallMetrics)
            {
                Application.quitting -= ApplicationOnQuitting;
            }
        }

        private void ApplicationOnQuitting()
        {
            string json = _dataManager.SerializeToJson(_methodToCallRoundabouts, true);
            string path = _dataManager.GetPersistentDataPath(CallMetricsFileName);
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
