#if UNITY_EDITOR

using System;
using System.Reflection;
using UnityEngine;

namespace ZombieBattleground.Editor.Runtime
{
    internal static class GUIClipWrapper
    {
        private static readonly Type _type;
        private static readonly Action<Rect, Vector2, Vector2, bool> _pushAction;
        private static readonly Action _popAction;
        private static readonly Action _reapplyAction;
        private static readonly Func<Rect> _getTopRectFunc;

        static GUIClipWrapper()
        {
            _type = typeof(GUI).Assembly.GetType("UnityEngine.GUIClip", true);

            MethodInfo method = _type
                .GetMethod("Push",
                    BindingFlags.NonPublic | BindingFlags.Static,
                    null,
                    new[]
                    {
                        typeof(Rect), typeof(Vector2), typeof(Vector2), typeof(bool)
                    },
                    null);
            _pushAction = (Action<Rect, Vector2, Vector2, bool>) Delegate.CreateDelegate(typeof(Action<Rect, Vector2, Vector2, bool>), method);

            method = _type
                .GetMethod("Pop", BindingFlags.NonPublic | BindingFlags.Static);
            _popAction = (Action) Delegate.CreateDelegate(typeof(Action), method);

            method = _type
                .GetMethod("Reapply", BindingFlags.NonPublic | BindingFlags.Static);
            _reapplyAction = (Action) Delegate.CreateDelegate(typeof(Action), method);

            method = _type
                .GetMethod("GetTopRect", BindingFlags.NonPublic | BindingFlags.Static);
            _getTopRectFunc = (Func<Rect>) Delegate.CreateDelegate(typeof(Func<Rect>), method);
        }

        public static void Push(Rect screenRect, Vector2 scrollOffset, Vector2 renderOffset, bool resetOffset)
        {
            _pushAction(screenRect, scrollOffset, renderOffset, resetOffset);
        }

        public static void Pop()
        {
            _popAction();
        }

        public static Rect GetTopRect()
        {
            return _getTopRectFunc();
        }

        public static void Reapply()
        {
            _reapplyAction();
        }
    }
}

#endif
