#if UNITY_EDITOR

using System;
using System.Reflection;

namespace ZombieBattleground.Editor.Runtime
{
    public static class GameViewWrapper
    {
        private static readonly Type _type = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.GameView");
        private static readonly FieldInfo _mGizmosField;
        private static readonly MethodInfo _getMainGameViewMethod;

        static GameViewWrapper()
        {
            _getMainGameViewMethod = _type.GetMethod("GetMainGameView", BindingFlags.NonPublic | BindingFlags.Static);
            _mGizmosField = _type.GetField("m_Gizmos", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public static object GetMainGameView()
        {
            return _getMainGameViewMethod.Invoke(null, null);
        }

        public static bool SetIsGizmosEnabled(object gameView)
        {
            return (bool) _mGizmosField.GetValue(gameView);
        }

        public static void SetIsGizmosEnabled(object gameView, bool isGizmosEnabled)
        {
            _mGizmosField.SetValue(gameView, isGizmosEnabled);
        }
    }
}

#endif
