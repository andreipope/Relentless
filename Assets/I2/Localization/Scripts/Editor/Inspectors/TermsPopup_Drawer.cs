using UnityEngine;
using UnityEditor;
using System.Collections;

namespace I2.Loc
{
	[CustomPropertyDrawer (typeof (TermsPopup))]
	public class TermsPopup_Drawer : PropertyDrawer 
	{
		public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) 
		{
			EditorGUI.BeginChangeCheck ();

			var Terms = LocalizationManager.GetTermsList ();
			Terms.Sort(System.StringComparer.OrdinalIgnoreCase);
			Terms.Add ("<none>");
			var newIndex = EditorGUI.Popup (position, label.text, Terms.IndexOf (property.stringValue), Terms.ToArray());

			if (EditorGUI.EndChangeCheck ())
				property.stringValue = (newIndex < 0 || newIndex == Terms.Count - 1) ? string.Empty : Terms [newIndex];
		}
	}
}