using TMPro;
using UnityEditor;
using UnityEditor.UI;

[CustomEditor(typeof(ButtonTextOffset))]
public class ButtonTextOffsetEditor : ButtonEditor
{
    public override void OnInspectorGUI()
    {
        ButtonTextOffset component = (ButtonTextOffset)target;
        base.OnInspectorGUI();

        component.buttonText = (TextMeshProUGUI)EditorGUILayout.ObjectField("Text", component.buttonText, typeof(TextMeshProUGUI), true);
        component.textOffset = EditorGUILayout.FloatField("Text Offset", component.textOffset);
    }
}
