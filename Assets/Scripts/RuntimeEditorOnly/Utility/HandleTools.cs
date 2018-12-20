#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace ZombieBattleground.Editor.Runtime {
    public static class HandleTools {
        private static readonly Vector3[] _wireframeBoxArrayCacheSingle = {
            new Vector3(-1f, -1f, -1f),
            new Vector3(-1f, 1f, -1f),
            new Vector3(1f, 1f, -1f),
            new Vector3(1f, -1f, -1f),
            new Vector3(1f, -1f, -1f),
            new Vector3(-1f, -1f, -1f),
            new Vector3(-1f, -1f, 1f),
            new Vector3(-1f, 1f, 1f),
            new Vector3(1f, 1f, 1f),
            new Vector3(1f, -1f, 1f),
            new Vector3(-1f, -1f, 1f)
        };

        private static readonly Vector3[] _wireframeBox2DArrayCacheSingle = {
            new Vector3(-1f, -1f, 0f),
            new Vector3(-1f, 1f, 0f),
            new Vector3(1f, 1f, 0f),
            new Vector3(1f, -1f, 0f),
            new Vector3(-1f, -1f, 0f)
        };

        private static readonly Vector3[] _drawSolidRectangleWithOutlinCacheSingle = {
            new Vector3(-1f, -1f, 0f),
            new Vector3(-1f, 1f, 0f),
            new Vector3(1f, 1f, 0f),
            new Vector3(1f, -1f, 0f),
        };

        public static void DrawBounds(Bounds bounds, Quaternion rotation) {
            Matrix4x4 matrix = Handles.matrix;
            Handles.matrix = Matrix4x4.TRS(bounds.center, rotation, bounds.size * 0.5f);
            DrawWireframeBoxSingle();
            Handles.matrix = matrix;
        }

        public static void DrawBounds2D(Bounds bounds, Quaternion rotation) {
            Matrix4x4 matrix = Handles.matrix;
            Handles.matrix = Matrix4x4.TRS(bounds.center, rotation, bounds.size * 0.5f);
            Handles.DrawPolyLine(_wireframeBox2DArrayCacheSingle);
            Handles.matrix = matrix;
        }

        public static void DrawVerticalWorldRect(Vector3 pointStart, Vector3 pointEnd) {
            Bounds bounds = new Bounds(pointStart, Vector3.zero);
            bounds.Encapsulate(pointStart);
            bounds.Encapsulate(pointEnd);
            Vector3 size = bounds.size;
            size.z = 0f;
            size.y = 50000f;
            bounds.size = size;

            DrawBounds(bounds, Quaternion.identity);
        }

        public static void DrawSolidRectangleWithOutline(Bounds bounds, Color faceColor, Color outlineColor) {
            _drawSolidRectangleWithOutlinCacheSingle[0] = new Vector3(bounds.center.x - bounds.extents.x, bounds.center.y - bounds.extents.y, bounds.center.z);
            _drawSolidRectangleWithOutlinCacheSingle[1] = new Vector3(bounds.center.x - bounds.extents.x, bounds.center.y + bounds.extents.y, bounds.center.z);
            _drawSolidRectangleWithOutlinCacheSingle[2] = new Vector3(bounds.center.x + bounds.extents.x, bounds.center.y + bounds.extents.y, bounds.center.z);
            _drawSolidRectangleWithOutlinCacheSingle[3] = new Vector3(bounds.center.x + bounds.extents.x, bounds.center.y - bounds.extents.y, bounds.center.z);

            Handles.DrawSolidRectangleWithOutline(_drawSolidRectangleWithOutlinCacheSingle, faceColor, outlineColor);
        }

        public static Rect ClipRectToScreen(Rect rect)
        {
            if (rect.x < 0)
            {
                rect.x = 0f;
            }
            if (rect.y < 0)
            {
                rect.y = 0f;
            }
            if (rect.xMax > Screen.width)
            {
                rect.x -= rect.xMax - Screen.width;
            }
            if (rect.yMax > Screen.height)
            {
                rect.y -= rect.yMax - Screen.height;
            }

            return rect;
        }

        private static void DrawWireframeBoxSingle() {
            Handles.DrawPolyLine(_wireframeBoxArrayCacheSingle);
            Handles.DrawLine(_wireframeBoxArrayCacheSingle[1], _wireframeBoxArrayCacheSingle[6]);
            Handles.DrawLine(_wireframeBoxArrayCacheSingle[2], _wireframeBoxArrayCacheSingle[7]);
            Handles.DrawLine(_wireframeBoxArrayCacheSingle[3], _wireframeBoxArrayCacheSingle[8]);
        }
    }
}

#endif
