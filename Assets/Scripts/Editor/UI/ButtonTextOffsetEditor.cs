using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using TMPro;

[CustomEditor(typeof(ButtonTextOffset))]
public class ButtonTextOffsetEditor : UnityEditor.UI.ButtonEditor
{
    public override void OnInspectorGUI()
    {

        ButtonTextOffset component = (ButtonTextOffset)target;
        base.OnInspectorGUI();

        component.buttonText = (TextMeshProUGUI)EditorGUILayout.ObjectField("Text", component.buttonText, typeof(TextMeshProUGUI), true);
        component.textOffset = EditorGUILayout.FloatField("Text Offset", component.textOffset);

    }
}