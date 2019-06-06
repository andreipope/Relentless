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
    public class CustomContractCallProxy : IContractCallProxy
    {
        private static readonly ILog Log = Logging.GetLog(nameof(CustomContractCallProxy));
        private static readonly ILog CallExecutionLog = Logging.GetLog("CallExecutionTrace");

        private readonly Dictionary<string, CallRoundaboutData> _methodToCallRoundabouts = new Dictionary<string, CallRoundaboutData>();

        private readonly IDataManager _dataManager;

        public const string CallMetricsFileName = "CallMetrics.json";

        public RawChainEventContract Contract { get; }

        public bool EnableLogs { get; set; }

        public bool StoreCallMetrics { get; set; }

        public bool LoadCallMetricsOnStartup { get; set; }

        public IReadOnlyDictionary<string, CallRoundaboutData> MethodToCallRoundabouts => _methodToCallRoundabouts;

        public int AverageRoundabout { get; private set; }

        private readonly Dictionary<int, double> _callNumberToExecutionTimestamp = new Dictionary<int, double>();

        public CustomContractCallProxy(RawChainEventContract contract, bool enableLogs, bool storeCallMetrics)
        {
            _dataManager = GameClient.Get<IDataManager>();
            Contract = contract ?? throw new ArgumentNullException(nameof(contract));
            EnableLogs = enableLogs;
            StoreCallMetrics = storeCallMetrics;

            NotifyingDAppChainClientCallExecutor callExecutor = Contract.Client.CallExecutor as NotifyingDAppChainClientCallExecutor;
            if (callExecutor == null)
                throw new Exception($"{nameof(NotifyingDAppChainClientCallExecutor)} is expected");

            callExecutor.CallStarting += (callNumber, callContext) =>
            {
                if (EnableLogs)
                {
                    string callType = callContext.IsStatic ? " (static)" : "";
                    CallExecutionLog.Debug($"Executing call{callType} #{callNumber} ({callContext.CalledMethodName})");
                }

                _callNumberToExecutionTimestamp.Add(callNumber, Utilites.GetTimestamp());
            };

            callExecutor.CallFinished += (callNumber, callContext, exception) =>
            {
                double callStartTimestamp = _callNumberToExecutionTimestamp[callNumber];
                _callNumberToExecutionTimestamp.Remove(callNumber);
                int elapsedMilliseconds = (int) ((Utilites.GetTimestamp() - callStartTimestamp) * 1000);
                bool timedOut = elapsedMilliseconds >= Contract.Client.Configuration.CallTimeout;
                LogCallTime(callContext.CalledMethodName, elapsedMilliseconds, callContext.IsStatic, timedOut);

                if (EnableLogs)
                {
                    string callType = callContext.IsStatic ? " (static)" : "";
                    string message = $"Finished executing call{callType} #{callNumber} ({callContext.CalledMethodName}) in {elapsedMilliseconds} ms";
                    if (timedOut)
                    {
                        message += ", timed out!";
                    }

                    if (exception != null)
                    {
                        message += $" with exception: {exception.Message}";
                    }

                    if (timedOut || exception is TimeoutException)
                    {
                        CallExecutionLog.Warn(message, exception);
                    }
                    else
                    {
                        CallExecutionLog.Debug(message, exception);
                    }
                }
            };

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
                            Helpers.ExceptionReporter.SilentReportException(e);
                            Log.Warn("Unable to read call metrics: " + e);
                        }
                    }
                }
            }
        }

        public virtual async Task CallAsync(string method, IMessage args)
        {
            if (EnableLogs)
            {
                CallExecutionLog.Debug($"Queuing call ({method})");
            }

            Task call = Contract.CallAsync(method, args);
            await call;
        }

        public virtual async Task<T> CallAsync<T>(string method, IMessage args) where T : IMessage, new()
        {
            if (EnableLogs)
            {
                CallExecutionLog.Debug($"Queuing call ({method})");
            }

            Task<T> call = Contract.CallAsync<T>(method, args);
            return await call;
        }

        public virtual async Task<T> StaticCallAsync<T>(string method, IMessage args) where T : IMessage, new()
        {
            if (EnableLogs)
            {
                CallExecutionLog.Debug($"Queuing call (static) ({method})");
            }

            Task<T> call = Contract.StaticCallAsync<T>(method, args);
            return await call;
        }

        public void Dispose()
        {
            if (StoreCallMetrics)
            {
                Application.quitting -= ApplicationOnQuitting;
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
