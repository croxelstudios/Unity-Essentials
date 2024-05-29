#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public static class ScriptableObjectUtils
{
    public static void CreateScriptableObjectAsset<T>(string defaultWindowName, string defaultAssetName, string defaultLocation = "Assets") where T : ScriptableObject
    {
        string path = EditorUtility.SaveFilePanel("Create new " + defaultWindowName, defaultLocation, defaultAssetName, "asset");
        if (path.StartsWith(Application.dataPath))
        {
            path = "Assets" + path.Substring(Application.dataPath.Length);
            AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<T>(), path);
        }
        else Debug.Log("Can't create asset outside of project");
    }
}
#endif
