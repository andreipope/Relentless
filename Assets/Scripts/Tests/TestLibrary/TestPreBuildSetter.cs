using NUnit.Framework;
using UnityEngine;

namespace Loom.ZombieBattleground.Test
{
#if UNITY_EDITOR
    [UnityEditor.InitializeOnLoad]
#endif
    public static class TestPreBuildSetter
    {
        private static bool _isRegistered;

        static TestPreBuildSetter()
        {
            Register();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
#if UNITY_EDITOR
        [UnityEditor.Callbacks.DidReloadScripts]
#endif
        private static void Register()
        {
            if (_isRegistered)
                return;

            _isRegistered = true;
            UnitTestDetector.CheckRequested += Update;

#if UNITY_EDITOR
            UnityEditor.EditorApplication.playModeStateChanged += change =>
            {
                Update();
            };
#endif
        }

        private static void Update()
        {
            UnitTestDetector.SetRunningUnitTest(TestContext.CurrentTestExecutionContext != null);
        }
    }
}
