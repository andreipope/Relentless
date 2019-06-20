using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace Loom.ZombieBattleground.Editor.Tools
{
    public static class AssetBundleFolderValidator
    {
        public static List<AssetImporter> GetAssetImportersForAssetsInFolder(string folder)
        {
            string[] assetGuids =
                AssetDatabase
                    .FindAssets("t:Object", new []{ folder })
                    .Distinct()
                    .ToArray();

            List<AssetImporter> importers = new List<AssetImporter>(assetGuids.Length);
            foreach (string assetGuid in assetGuids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(assetGuid);
                importers.Add(AssetImporter.GetAtPath(assetPath));
            }

            return importers;
        }

        public static List<AssetImporter> ForceAssetBundleInFolder(string folder, string assetBundleName)
        {
            List<AssetImporter> importers = GetAssetImportersForAssetsInFolder(folder);
            List<AssetImporter> modified = new List<AssetImporter>();

            AssetDatabase.StartAssetEditing();
            foreach (AssetImporter assetImporter in importers)
            {
                if (assetImporter.assetBundleName != assetBundleName)
                {
                    modified.Add(assetImporter);
                    assetImporter.assetBundleName = assetBundleName;
                    UnityEditor.EditorUtility.SetDirty(assetImporter);
                    UnityEditor.EditorUtility.SetDirty(assetImporter);
                }
            }
            AssetDatabase.StopAssetEditing();

            return modified;
        }
    }
}
