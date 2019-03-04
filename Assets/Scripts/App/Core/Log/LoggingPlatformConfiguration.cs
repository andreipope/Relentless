using UnityEngine;

#if UNITY_EDITOR
using UnityEditor.Callbacks;
#endif

namespace Loom.ZombieBattleground
{
    public static class LoggingPlatformConfiguration
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
#if UNITY_EDITOR
        [DidReloadScripts]
#endif
        public static void Setup()
        {
            Logging.Setup();

#if UNITY_EDITOR && !FORCE_ENABLE_ALL_LOGS
            // Disable non-essential logs in Editor
            Logger backendFacadeRpc = Logging.GetLogger(nameof(BackendFacade) + "Rpc");
            backendFacadeRpc.Level = Level.Warn;

            Logger timeMetricsContractCallProxy = Logging.GetLogger(nameof(TimeMetricsContractCallProxy));
            timeMetricsContractCallProxy.Level = Level.Warn;
#endif
        }
    }
}
