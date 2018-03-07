using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

namespace I2.Loc
{
	public partial class LocalizationEditor
	{
		void OnGUI_Warning_SourceInScene()
		{
			if (mLanguageSource.UserAgreesToHaveItOnTheScene) return;
			
			LanguageSource source = (LanguageSource)target;
			if (LocalizationManager.IsGlobalSource(source.name) && !GUITools.ObjectExistInScene(source.gameObject))
				return;
			
			string Text = @"Its advised to only use the source in I2\Localization\Resources\I2Languages.prefab

That works as a GLOBAL source accessible in ALL scenes. That’s why its recommended to add all your translations there.

You don't need to instantiate that prefab into the scene, just click the prefab and add the Data.

Only use Sources in the scene when the localization is meant to be ONLY used there.
However, that's not advised and is only used in the Examples to keep them separated from your project localization.

Furthermore, having a source in the scene require that any Localize component get a reference to that source to work properly. By dragging the source into the field at the bottom of the Localize component.";
			EditorGUILayout.HelpBox(Text, MessageType.Warning);
			
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("Keep as is"))
			{
				SerializedProperty Agree = serializedObject.FindProperty("UserAgreesToHaveItOnTheScene");
				Agree.boolValue = true;
			}
			
			GUILayout.FlexibleSpace();
			
			if (GUILayout.Button("Open the Global Source"))
			{
				GameObject Prefab = (Resources.Load(LocalizationManager.GlobalSources[0]) as GameObject);
				Selection.activeGameObject = Prefab;
			}
			
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("Delete this and open the Global Source"))
			{
				EditorApplication.CallbackFunction Callback = null;
				EditorApplication.update += Callback = ()=>
				{
					EditorApplication.update -= Callback;

					if (source.GetComponents<Component>().Length<=2)
					{
						Debug.Log ("Deleting GameObject '" + source.name + "' and Openning the "+LocalizationManager.GlobalSources[0]+".prefab");
						DestroyImmediate (source.gameObject);
					}
					else
					{
						Debug.Log ("Deleting the LanguageSource inside GameObject " + source.name + " and Openning the "+LocalizationManager.GlobalSources[0] +".prefab");
						DestroyImmediate (source);
					}

					GameObject Prefab = (Resources.Load(LocalizationManager.GlobalSources[0]) as GameObject);
					Selection.activeGameObject = Prefab;
				};
			}
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			
			GUILayout.Space(10);
		}

		private bool bSourceInsidePluginsFolder = true;
		public void OnGUI_Warning_SourceInsidePluginsFolder()
		{
			if (!bSourceInsidePluginsFolder || mLanguageSource.UserAgreesToHaveItInsideThePluginsFolder)
				return;
			
			LanguageSource source = (LanguageSource)target;
			if (!LocalizationManager.IsGlobalSource(source.name) || GUITools.ObjectExistInScene(source.gameObject))
			{
				bSourceInsidePluginsFolder = false;
				return;
			}

			string pluginPath = UpgradeManager.GetI2LocalizationPath();
			string assetPath = AssetDatabase.GetAssetPath(source);

			if (!assetPath.StartsWith(pluginPath, StringComparison.OrdinalIgnoreCase))
			{
				bSourceInsidePluginsFolder = false;
				return;
			}
			
			string Text = @"Its advised to move this Global Source to a folder outside the plugin.
For example (Assets/I2/Resources) instead of (Assets/I2/Localization/Resources)

That way upgrading the plugin its as easy as deleting the I2/Localization and I2/Common folders and reinstalling. 

Do you want the plugin to automatically move the LanguageSource to a folder outside the plugin?";
			EditorGUILayout.HelpBox(Text, MessageType.Warning);

			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("Keep as is"))
			{
				SerializedProperty Agree = serializedObject.FindProperty("UserAgreesToHaveItInsideThePluginsFolder");
				Agree.boolValue = true;
				bSourceInsidePluginsFolder = true;
			}

			GUILayout.FlexibleSpace();

			if (GUILayout.Button("Ask me later"))
			{
				bSourceInsidePluginsFolder = false;
			}

			GUILayout.FlexibleSpace();
			if (GUILayout.Button("Move to the Recommended Folder"))
				EditorApplication.delayCall += MoveGlobalSource;
			
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();

			GUILayout.Space(10);		
		}
		private static void MoveGlobalSource()
		{
			EditorApplication.delayCall -= MoveGlobalSource;

			string pluginPath = UpgradeManager.GetI2LocalizationPath();
			string assetPath = AssetDatabase.GetAssetPath(mLanguageSource);

			string I2Path = pluginPath.Substring(0, pluginPath.Length-"/Localization".Length);
			string newPath = I2Path + "/Resources/" + mLanguageSource.name + ".prefab";

			string fullresFolder = Application.dataPath + I2Path.Replace("Assets","") + "/Resources";
			bool folderExists = System.IO.Directory.Exists (fullresFolder);
			
			if (!folderExists)
				AssetDatabase.CreateFolder(I2Path, "Resources");
			AssetDatabase.MoveAsset(assetPath, newPath);
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();

			var prefab = AssetDatabase.LoadAssetAtPath(newPath, typeof(GameObject)) as GameObject;
			Selection.activeGameObject = prefab;

			Debug.Log("LanguageSource moved to:" + newPath);
			ShowInfo("Please, ignore some console warnings/errors produced by this operation, everything worked fine. In a new release those warnings will be cleared");
		}

		public static void DelayedDestroySource()
		{

		}
	}
}