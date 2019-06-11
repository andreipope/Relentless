using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Callbacks;
#if UNITY_5_3_OR_NEWER && !(UNITY_5_0 || UNITY_5_1 || UNITY_5_2)
using UnityEditor.SceneManagement;
#endif
using UnityEngine;

namespace Opencoding.Console.Editor
{
	static class BuildPostProcessor
	{
		[PostProcessScene]
		public static void OnPostprocessScene()
		{
			if (EditorApplication.isPlaying)
				return;

			var debugConsoles = UnityEngine.Object.FindObjectsOfType<DebugConsole>();
		    if (debugConsoles.Length > 1)
		    {
#if UNITY_5_0 || (UNITY_5_1 || UNITY_5_2)
                throw new InvalidOperationException("More than one debug console in the scene " +
		                                            EditorApplication.currentScene);
#else
                throw new InvalidOperationException("More than one debug console in the scene " + 
                    EditorSceneManager.GetActiveScene().name);
#endif
            }

            if (debugConsoles.Length == 0)
				return;

			var debugConsole = debugConsoles[0];
			if(debugConsole.Settings.OnlyInDevBuilds && !EditorUserBuildSettings.development)
				UnityEngine.Object.DestroyImmediate(debugConsole.gameObject);

		    bool isEnabledByDefine = String.IsNullOrEmpty(debugConsole.Settings.EnableIfDefined) ||
		                             EditorUserBuildSettings.activeScriptCompilationDefines.Contains(
		                                 debugConsole.Settings.EnableIfDefined);

		    if (isEnabledByDefine && !String.IsNullOrEmpty(debugConsole.Settings.DisableIfDefined) && EditorUserBuildSettings.activeScriptCompilationDefines.Contains(
                                    debugConsole.Settings.DisableIfDefined))
		    {
		        isEnabledByDefine = false;
		    }

            if (!isEnabledByDefine)
				UnityEngine.Object.DestroyImmediate(debugConsole.gameObject);

			if (debugConsole.Settings.AutoSetVersion)
			{
#if UNITY_IOS || (UNITY_ANDROID || !UNITY_5)
				debugConsole.Settings.GameVersion = PlayerSettings.bundleVersion;
#else
				debugConsole.Settings.GameVersion = Application.version;
#endif
				EditorUtility.SetDirty(debugConsole.Settings);
			}

#if UNITY_ANDROID
		    //ConformAndroidManifest();
#endif
		}

	    private static void ConformAndroidManifest()
        {
            var manifestFilePath = "Assets/Plugins/Android/OpenCoding/AndroidManifest.xml";
            var text = File.ReadAllText(manifestFilePath);
	        var regEx = new Regex("android:authorities=(\".*?\")");
	        var newContent = regEx.Replace(text, "android:authorities=\"" + PlayerSettings.applicationIdentifier + ".fileprovider\"");
            if(newContent != text)
                File.WriteAllText(manifestFilePath, newContent);
	    }
	}

	
}
