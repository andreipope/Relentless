using UnityEditor;
using UnityEngine;

namespace Opencoding.Console.Editor
{
	internal static class DebugConsoleEditorSettings
	{
		// Change this if you move the Opencoding directory to a different location
		private static string _opencodingDirectoryLocation = "Assets/Opencoding";

		public static bool AutomaticallyLoadConsoleInEditor { get; private set; }

		public static string OpencodingDirectoryLocation { get { return _opencodingDirectoryLocation; } }

		static DebugConsoleEditorSettings()
		{
			AutomaticallyLoadConsoleInEditor = EditorPrefs.GetBool("TouchConsolePro/AutomaticallyLoadConsoleInEditor", false);	
		}

		[PreferenceItem("TouchConsole Pro")]
		private static void SettingsOnGUI()
		{
			bool value = GUILayout.Toggle(AutomaticallyLoadConsoleInEditor, "Automatically load in editor");
			if (AutomaticallyLoadConsoleInEditor != value)
			{
				AutomaticallyLoadConsoleInEditor = value;
				EditorPrefs.SetBool("TouchConsolePro/AutomaticallyLoadConsoleInEditor", AutomaticallyLoadConsoleInEditor);
			}
			GUILayout.Label(
				"This places the DebugConsole prefab into the scene automatically for you when you play in the editor. This has no effect on built versions of your game.", "helpbox");
		}

	}

}