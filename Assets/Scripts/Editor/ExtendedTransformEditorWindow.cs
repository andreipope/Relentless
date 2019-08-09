using System;
using UnityEditor;
using UnityEngine;

namespace Loom.ZombieBattleground.Editor
{
    public class ExtendedTransformEditorWindow : EditorWindow
    {
        [MenuItem("Window/ZombieBattleground/Open Extended Transform Editor Window")]
        private static void OpenWindow()
        {
            ExtendedTransformEditorWindow window = GetWindow<ExtendedTransformEditorWindow>();
            window.Show();
        }

        private void OnEnable()
        {
            autoRepaintOnSceneChange = true;
            titleContent = new GUIContent("Transform Editor");
        }

        private void OnGUI()
        {
            Transform[] transforms = Selection.GetTransforms(SelectionMode.Editable);
            if (transforms == null || transforms.Length == 0)
            {
                GUILayout.Label("Select a Transform");
                return;
            }

            if (transforms.Length > 1)
            {
                GUILayout.Label("Multi-object editing is not supported");
                return;
            }

            Transform transform = transforms[0];
            EditorGUI.BeginChangeCheck();
            transform.position = EditorGUILayout.Vector3Field("World Position", transform.position);
            transform.eulerAngles = EditorGUILayout.Vector3Field("World Rotation", transform.eulerAngles);
            bool changed = EditorGUI.EndChangeCheck();
            if (changed)
            {
                UnityEditor.EditorUtility.SetDirty(transform);
            }
        }
    }
}
