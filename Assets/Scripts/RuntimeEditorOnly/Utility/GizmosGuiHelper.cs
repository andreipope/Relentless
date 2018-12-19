#if UNITY_EDITOR

using System;
using UnityEditor;
using UnityEngine;

namespace ZombieBattleground.Editor.Runtime
{
    public static class GizmosGuiHelper
    {
        private static bool _isSceneView;
        private static Rect _topRect;

        public static bool CanRenderGui()
        {
            return GizmosManager.DrawGizmos &&
                (RenderTexture.active == null ||
                    RenderTexture.active.name.StartsWith("SceneView", StringComparison.Ordinal) ||
                    RenderTexture.active.name.StartsWith("TempBuffer", StringComparison.Ordinal));
        }

        public static void EndGizmosGui()
        {
            if (_isSceneView)
            {
                Handles.EndGUI();
            }
            else
            {
                GUIClipWrapper.Pop();
                GUIClipWrapper.Push(_topRect, Vector2.zero, Vector2.zero, false);
            }

            GL.PopMatrix();
        }

        public static void StartGizmosGui()
        {
            _isSceneView = Camera.current.name == "SceneCamera";
            _topRect = new Rect();
            GL.PushMatrix();
            if (_isSceneView)
            {
                GUIClipWrapper.Reapply();
            }
            else
            {
                _topRect = GUIClipWrapper.GetTopRect();
                Rect newRect = _topRect;
                newRect.y = 0f;
                GUIClipWrapper.Pop();
                GUIClipWrapper.Push(newRect, Vector2.zero, Vector2.zero, false);
            }
        }
    }
}

#endif
