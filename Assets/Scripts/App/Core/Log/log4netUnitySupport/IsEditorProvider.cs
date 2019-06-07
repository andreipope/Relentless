#if LOG4NET_UNITY

using UnityEngine;

namespace log4netUnitySupport
{
    internal static class IsEditorProvider
    {
        public static bool IsEditor { get; }

        static IsEditorProvider() {
            IsEditor = Application.isEditor;
        }

        [RuntimeInitializeOnLoadMethod]
        public static void Init() {
        }
    }
}

#endif
