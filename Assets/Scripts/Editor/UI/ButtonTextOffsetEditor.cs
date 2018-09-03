using TMPro;
using UnityEditor;
using UnityEditor.UI;

[CustomEditor(typeof(ButtonTextOffset))]
public class ButtonTextOffsetEditor : ButtonEditor
{
    public override void OnInspectorGUI()
    {
        ButtonTextOffset component = (ButtonTextOffset) target;
        base.OnInspectorGUI();

        component.ButtonText =
            (TextMeshProUGUI) EditorGUILayout.ObjectField("Text", component.ButtonText, typeof(TextMeshProUGUI), true);
        component.TextOffset = EditorGUILayout.FloatField("Text Offset", component.TextOffset);
    }
}
