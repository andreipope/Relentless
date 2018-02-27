using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace I2.Loc
{
	[InitializeOnLoad]
	public class UpgradeManager
	{
        static bool mAlreadyCheckedPlugins = false;

		static UpgradeManager()
		{
			I2.I2AboutWindow.PluginsVersion["I2 Localization"] = LocalizationManager.GetVersion();
			EditorApplication.update += AutoCheckPlugins;
		}

		public static void AutoCheckPlugins()
		{
			CheckPlugins ();
		}

		public static void CheckPlugins( bool bForce = false )
		{
			EditorApplication.update -= AutoCheckPlugins;

            if (mAlreadyCheckedPlugins && !bForce)
                return;
            mAlreadyCheckedPlugins = true;
			
			EnablePlugins(bForce);
			CreateLanguageSources();
			CreateScriptLocalization();
		}

		const string EditorPrefs_AutoEnablePlugins = "I2Loc AutoEnablePlugins";

		[MenuItem( "Tools/I2 Localization/Enable Plugins/Force Detection", false, 0 )]
		public static void ForceCheckPlugins()
		{
			CheckPlugins( true );
		}

		[MenuItem( "Tools/I2 Localization/Enable Plugins/Enable Auto Detection", false, 1 )]
		public static void EnableAutoCheckPlugins()
		{
			EditorPrefs.SetBool(EditorPrefs_AutoEnablePlugins, true);
		}
		[MenuItem( "Tools/I2 Localization/Enable Plugins/Enable Auto Detection", true)]
		public static bool ValidEnableAutoCheckPlugins()
		{
			return !EditorPrefs.GetBool(EditorPrefs_AutoEnablePlugins, true);
		}


		[MenuItem( "Tools/I2 Localization/Enable Plugins/Disable Auto Detection", false, 2 )]
		public static void DisableAutoCheckPlugins()
		{
			EditorPrefs.SetBool(EditorPrefs_AutoEnablePlugins, false);
		}
		[MenuItem( "Tools/I2 Localization/Enable Plugins/Disable Auto Detection", true)]
		public static bool ValidDisableAutoCheckPlugins()
		{
			return EditorPrefs.GetBool(EditorPrefs_AutoEnablePlugins, true);
		}



		
		public static void EnablePlugins( bool bForce = false )
		{
			if (!bForce)
			{
				bool AutoEnablePlugins = EditorPrefs.GetBool(EditorPrefs_AutoEnablePlugins, true);
				if (!AutoEnablePlugins)
					return;
			}
			//var tar = System.Enum.GetValues(typeof(BuildTargetGroup));
			foreach (BuildTargetGroup target in System.Enum.GetValues(typeof(BuildTargetGroup)))
				if (target!=BuildTargetGroup.Unknown && !target.HasAttributeOfType<System.ObsoleteAttribute>())
				{
					EnablePluginsOnPlatform( target );
				}
		}

		static void EnablePluginsOnPlatform( BuildTargetGroup Platform )
		{
			string Settings = PlayerSettings.GetScriptingDefineSymbolsForGroup(Platform );
			
			bool HasChanged = false;
			List<string> symbols = new List<string>( Settings.Split(';'));
			
			HasChanged |= UpdateSettings("NGUI",  "NGUIDebug",  	  		 "", ref symbols);
			HasChanged |= UpdateSettings("DFGUI", "dfPanel", 	  		 "", ref symbols);
			HasChanged |= UpdateSettings("TK2D",  "tk2dTextMesh", 		 "", ref symbols);
			HasChanged |= UpdateSettings("TextMeshPro_Pre53",  "TMPro.TextMeshProFont", 		 "", ref symbols);
			HasChanged |= UpdateSettings("TextMeshPro",  "TMPro.TMP_FontAsset", 		 "", ref symbols);
			HasChanged |= UpdateSettings("SVG",  "SVGImporter.SVGAsset", 		 "", ref symbols);

			if (HasChanged)
			{
				Settings = string.Empty;
				for (int i=0,imax=symbols.Count; i<imax; ++i)
				{
					if (i>0) Settings += ";";
					Settings += symbols[i];
				}
				PlayerSettings.SetScriptingDefineSymbolsForGroup(Platform, Settings );
			}
		}
		
		static bool UpdateSettings( string mPlugin, string mType, string AssemblyType, ref List<string> symbols)
		{
			try
			{
				bool hasPluginClass = false;
				System.Reflection.Assembly assembly = /*typeof(Localize).Assembly;*/(string.IsNullOrEmpty(AssemblyType) ? typeof(Localize).Assembly : System.Reflection.Assembly.Load(AssemblyType));
				if (assembly!=null && assembly.GetType(mType)!=null)
					hasPluginClass = true;
				
				bool hasPluginDef = (symbols.IndexOf(mPlugin)>=0);
				
				if (hasPluginClass != hasPluginDef)
				{
					if (hasPluginClass) symbols.Add(mPlugin);
					else symbols.Remove(mPlugin);
					return true;
				}
			}
			catch(System.Exception)
			{
			}
			return false;
			
		}
		
        [MenuItem( "Tools/I2 Localization/Create I2Languages", false, 1)]
		public static void CreateLanguageSources()
		{
			if (LocalizationManager.GlobalSources==null || LocalizationManager.GlobalSources.Length==0)
				return;
			
			Object GlobalSource = Resources.Load(LocalizationManager.GlobalSources[0]);
			if (GlobalSource!=null)
				return;
			
			string PluginPath = GetI2LocalizationPath();
			string ResourcesFolder = PluginPath.Substring(0, PluginPath.Length-"/Localization".Length) + "/Resources";
			
            string fullresFolder = Application.dataPath + ResourcesFolder.Replace("Assets","");
            if (!System.IO.Directory.Exists(fullresFolder))
                System.IO.Directory.CreateDirectory(fullresFolder);
			//string guid = AssetDatabase.AssetPathToGUID(/*ResourcesFolder*/);
			/*if (string.IsNullOrEmpty(guid)) // Folder doesn't exist
			{
				AssetDatabase.CreateFolder(PluginPath, "Resources");
			}*/
			
			GameObject go = new GameObject(LocalizationManager.GlobalSources[0]);
			go.AddComponent<LanguageSource>();
			PrefabUtility.CreatePrefab(ResourcesFolder + "/" + LocalizationManager.GlobalSources[0] + ".prefab", go);
			GameObject.DestroyImmediate(go);
			
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}
		
		static void CreateScriptLocalization()
		{
			string[] assets = AssetDatabase.FindAssets("ScriptLocalization");
			if (assets.Length>0)
				return;
			
			string ScriptsFolder = GetI2LocalizationPath() + "/Scripts";
			
			string ScriptText = @"// This class is Auto-Generated by the Script Tool in the Language Source
using UnityEngine;

namespace I2.Loc
{
	public static class ScriptLocalization
	{
		public static string Get( string Term ) { return Get( Term, false, 0); }
		public static string Get( string Term, bool FixForRTL ) { return Get( Term, FixForRTL, 0); }
		public static string Get( string Term, bool FixForRTL, int maxLineLengthForRTL ) { return LocalizationManager.GetTermTranslation(Term, FixForRTL, maxLineLengthForRTL); }
	}
}";
			
			System.IO.File.WriteAllText(ScriptsFolder + "/ScriptLocalization.cs", ScriptText);
			
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}
		
		public static string GetI2LocalizationPath()
		{
			string[] assets = AssetDatabase.FindAssets("LocalizationManager");
			if (assets.Length==0)
				return string.Empty;
			
			string PluginPath = AssetDatabase.GUIDToAssetPath(assets[0]);
			PluginPath = PluginPath.Substring(0, PluginPath.Length - "/Scripts/LocalizationManager.cs".Length);
			
			return PluginPath;
		}
	}

	public static class UpgradeManagerHelper
	{
		public static bool HasAttributeOfType<T>(this System.Enum enumVal) where T:System.Attribute
		{
			var type = enumVal.GetType();
			var memInfo = type.GetMember(enumVal.ToString());
			var attributes = memInfo[0].GetCustomAttributes(typeof(T), false);
			return attributes.Length > 0;
		}
	}
}