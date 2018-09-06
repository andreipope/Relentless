using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Opencoding.Console.Editor
{
	[InitializeOnLoad] 
	public class DetectUpgrade
	{
		static DetectUpgrade()
		{
			EditorApplication.delayCall += ShowDeleteMessage;
		}

		private static void ShowDeleteMessage()
		{ 
			if (Directory.Exists("Assets/Opencoding/3rdParty/XcodeAPI"))
			{
				EditorUtility.DisplayDialog("Important Upgrade Information",
					"This version of TouchConsole Pro changes the way Xcode projects are built to reduce conflicts with other plugins.\n\n" +
					"This upgrade process will automatically delete the old system.\n\n" +
					"You MUST now do a clean build of your Xcode project or you will get errors.",
					"Ok");
				AssetDatabase.DeleteAsset("Assets/Opencoding/3rdParty/XcodeAPI");
				AssetDatabase.Refresh();
			}

		    if (File.Exists("Assets/Plugins/Android/opencodingconsole.jar"))
		    {
                EditorUtility.DisplayDialog("Important Upgrade Information",
                    "This version of TouchConsole Pro changes how the Android support code is supplied. The old opencodingconsole.jar file will be deleted automatically.",
                    "Ok");
                AssetDatabase.DeleteAsset("Assets/Plugins/Android/opencodingconsole.jar");
                AssetDatabase.Refresh();
            }

            if(!EditorPrefs.GetBool("Opencoding/TouchConsolePro/android-support-v4-warning-seen", false))
            { 
		        var androidSupportJars = AssetDatabase.FindAssets("android-support-v4");
                
                if (androidSupportJars.Length == 2)
                { 
                    var includedVersionIndex = Array.FindIndex(androidSupportJars,
                        guid =>
                        {
                            var path = AssetDatabase.GUIDToAssetPath(guid);
                            return
                                path.IndexOf("Opencoding", 0, StringComparison.InvariantCultureIgnoreCase) != -1 &&
                                path.ToLower().EndsWith(".jar");
                        }
                    ); 
                     
                    if (includedVersionIndex != -1)
                    {
                        if (EditorUtility.DisplayDialog("Important Information", 
                            "There are two copies of the android-support-v4 library in your project. This could cause you problems when you build for Android. Do you want to delete the one that is supplied with TouchConsole Pro?",
                            "Yes", "No - never show this again"))
                        {
                            var assetPath = AssetDatabase.GUIDToAssetPath(androidSupportJars[includedVersionIndex]);
                            AssetDatabase.MoveAssetToTrash(assetPath);
                            AssetDatabase.Refresh();
                        }
                        else
                        {
                            EditorPrefs.SetBool("Opencoding/TouchConsolePro/android-support-v4-warning-seen", true);
                        }
                    }
                }
            }
		}
	}
}