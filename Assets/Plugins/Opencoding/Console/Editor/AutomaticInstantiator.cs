using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Opencoding.Console;
using Opencoding.Console.Editor;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

[InitializeOnLoad] 
class AutomaticInstantiator
{
	static AutomaticInstantiator()
	{
		EditorApplication.playmodeStateChanged += PlaymodeStateChanged;
	}

	private static void PlaymodeStateChanged()
	{
		if (EditorApplication.isPlaying)
		{
			if (!DebugConsoleEditorSettings.AutomaticallyLoadConsoleInEditor)
				return;

			if (Object.FindObjectOfType<DebugConsole>() == null)
			{
				var debugConsolePath = Path.Combine(DebugConsoleEditorSettings.OpencodingDirectoryLocation, "Console/Prefabs/DebugConsole.prefab");
				var prefab = AssetDatabase.LoadMainAssetAtPath(debugConsolePath);
				if (prefab == null)
				{
					Debug.LogWarning("Couldn't load DebugConsole as the DebugConsole prefab couldn't be found at " + debugConsolePath + ". If you have moved the OpenCoding folder, please update the location in DebugConsoleEditorSettings.");
					return;
				}
				var go = Object.Instantiate(prefab);
				go.name = "DebugConsole (Automatically Instantiated)";
			}
		}
	}
}