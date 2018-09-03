using System.Collections.Generic;

#if !UNITY_CLOUD_BUILD

namespace UnityEngine.CloudBuild
{
    public abstract class BuildManifestObject : ScriptableObject
    {
        // Tries to get a manifest value - returns true if key was found and could be cast to type T, false otherwise.
        public abstract bool TryGetValue<T>(string key, out T result);

        // Retrieve a manifest value or throw an exception if the given key isn't found.
        public abstract T GetValue<T>(string key);

        // Sets the value for a given key.
        public abstract void SetValue(string key, object value);

        // Copy values from a dictionary. ToString() will be called on dictionary values before being stored.
        public abstract void SetValues(Dictionary<string, object> sourceDict);

        // Remove all key/value pairs
        public abstract void ClearValues();

        // Returns a Dictionary that represents the current BuildManifestObject
        public abstract Dictionary<string, object> ToDictionary();

        // Returns a JSON formatted string that represents the current BuildManifestObject
        public abstract string ToJson();
    }
}

#endif
