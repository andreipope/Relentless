using UnityEditor;
using UnityEngine;
using System.Collections;

namespace I2.Loc
{
	[CustomEditor(typeof(SetLanguage))]
	public class SetLanguageInspector : Editor
	{
		public SetLanguage setLan;
		public SerializedProperty mProp_Language;

		public void OnEnable()
		{
			setLan = (SetLanguage)target;
			mProp_Language = serializedObject.FindProperty("_Language");
		}

		public override void OnInspectorGUI()
		{
			string[] Languages;
			LanguageSource source = setLan.mSource;
			if (source==null)
			{
				LocalizationManager.UpdateSources();
				Languages = LocalizationManager.GetAllLanguages().ToArray();
				System.Array.Sort(Languages);
			}
			else
			{
				Languages = source.GetLanguages().ToArray();
				System.Array.Sort(Languages);
			}

			int index = System.Array.IndexOf(Languages, mProp_Language.stringValue);

			GUI.changed = false;
			index = EditorGUILayout.Popup("Language", index, Languages);
			if (GUI.changed)
			{
				if (index<0 || index>=Languages.Length)
					mProp_Language.stringValue = string.Empty;
				else
					mProp_Language.stringValue = Languages[index];
				GUI.changed = false;
				serializedObject.ApplyModifiedProperties();
			}

			GUILayout.Space(5);
			if (setLan.mSource==null) GUI.contentColor = Color.Lerp (Color.gray, Color.yellow, 0.1f);
			source = EditorGUILayout.ObjectField("Language Source:", source, typeof(LanguageSource), true) as LanguageSource;
			GUI.contentColor = Color.white;

			if (GUI.changed)
				setLan.mSource = source;

			serializedObject.ApplyModifiedProperties();
		}
	}
}