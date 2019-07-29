using System;
using System.Collections;
using System.Globalization;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Loom.ZombieBattleground.Editor {
    public static class TagManager {
        private const string kTagManagerPath = "ProjectSettings/TagManager.asset";
        private static SerializedObject _tagManager;
        private static SerializedProperty _tags;
        private static SerializedProperty _layers;
        private static readonly CultureInfo _invariantCulture = CultureInfo.InvariantCulture;
        private static readonly bool _isUnity5OrNewer;

        static TagManager() {
#if UNITY_5_0 || UNITY_5_1 || UNITY_5_2 || UNITY_5_3 || UNITY_5_3_OR_NEWER
            _isUnity5OrNewer = true;
#else
            _isUnity5OrNewer = false;
#endif
            UpdateManager();
        }

        private static void UpdateManager() {
            _tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath(kTagManagerPath)[0]);
            _tags = _tagManager.FindProperty("tags");
            _layers = _tagManager.FindProperty("layers");
        }

        private static void SaveManager() {
#if UNITY_5_6_OR_NEWER
            _tagManager.UpdateIfRequiredOrScript();
#else
            _tagManager.UpdateIfDirtyOrScript();
#endif
            _tagManager.ApplyModifiedProperties();
        }

        public static bool IsTagExists(string tag) {
            UpdateManager();

            IEnumerator it = _tags.GetEnumerator();

            while (it.MoveNext()) {
                SerializedProperty prop = it.Current as SerializedProperty;
                if (prop == null || prop.type != "string") {
                    continue;
                }

                if (prop.stringValue == tag) {
                    return true;
                }
            }

            return false;
        }

        public static void AddTag(string tag) {
            UpdateManager();

            if (IsTagExists(tag)) {
                return;
            }

            int newIndex = _tags.arraySize - 1;
            _tags.InsertArrayElementAtIndex(newIndex);
            SerializedProperty newTag = _tags.GetArrayElementAtIndex(newIndex);
            newTag.stringValue = tag.Trim();

            SaveManager();
        }

        public static string GetLayer(int layerNumber) {
            UpdateManager();

            SerializedProperty layer = GetLayerProperty(layerNumber);
            return layer != null ? layer.stringValue : null;
        }

        public static bool SetLayer(int layerNumber, string layerName) {
            UpdateManager();

            SerializedProperty layer = GetLayerProperty(layerNumber);
            if (layer == null) {
                return false;
            }

            layer.stringValue = layerName.Trim();
            SaveManager();

            return true;
        }

        public static bool IsLayerExists(string layerName) {
            for (int i = 7; i <= 31; i++) {
                if (GetLayer(i) == layerName) {
                    return true;
                }
            }

            return false;
        }

        public static int GetFreeLayer(LayerSearchDirection searchDirection) {
            if (searchDirection == LayerSearchDirection.FirstToLast) {
                for (int i = 7; i <= 31; i++) {
                    if (GetLayer(i) == "") {
                        return i;
                    }
                }
            }
            else {
                for (int i = 31; i >= 7; i--) {
                    if (GetLayer(i) == "") {
                        return i;
                    }
                }
            }

            return -1;
        }

        public static void AddLayer(string layerName,
            LayerSearchDirection searchDirection = LayerSearchDirection.LastToFirst) {
            if (IsLayerExists(layerName)) {
                return;
            }

            int freeLayer = GetFreeLayer(searchDirection);
            SetLayer(freeLayer, layerName);
        }

        private static SerializedProperty GetLayerProperty(int layerNumber) {
            return _isUnity5OrNewer ?
                _layers.GetArrayElementAtIndex(layerNumber) :
                _tagManager.FindProperty("User Layer " + layerNumber.ToString(_invariantCulture));
        }

        public enum LayerSearchDirection {
            LastToFirst,
            FirstToLast,
        }
    }
}
