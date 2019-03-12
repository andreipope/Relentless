#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class EditorEventsHandler : MonoBehaviour
{
    private void Update()
    {
        if (EditorApplication.isCompiling && EditorApplication.isPlaying)
        {
            EditorApplication.isPlaying = false;
            EditorApplication.isPaused = false;
        }
    }
}
#endif
