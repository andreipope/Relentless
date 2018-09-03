using UnityEditor;
using UnityEditor.UI;

namespace LoomNetwork.CZB.Editor
{
    [CustomEditor(typeof(ButtonShiftingContent))]
    public class ButtonShiftingContentEditor : ButtonEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("ShiftedChild"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("ShiftValue"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("ShiftOnHighlight"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("ShiftOnPress"));
            serializedObject.ApplyModifiedProperties();
        }
    }
}
