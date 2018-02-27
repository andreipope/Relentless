using UnityEngine;
using UnityEditor;
using System.Collections;

namespace I2
{
	[CustomPropertyDrawer(typeof(RenameAttribute))]
	public class RenameDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent propContent) 
		{
			var atr = (RenameAttribute) attribute;
			var content = new GUIContent(atr.Name, atr.Tooltip);
			position.xMin += atr.HorizSpace;
			EditorGUI.PropertyField(position, property, content);
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return base.GetPropertyHeight (property, label);
		}
	}
}