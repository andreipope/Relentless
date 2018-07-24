using UnityEditor;
using UnityEngine;

public class LoomEditor : Editor 
{
    [MenuItem("LoomX/Clear Player Prefs")]
    public static void ClearPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
    }
}
