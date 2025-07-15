#if UNITY_EDITOR
using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

public static class CustomRenderTexture_EditorHandler
{
    [MenuItem("Tools/Update CustomRenderTextures")]
    static void UpdateCustomTextures()
    {
        List<CustomRenderTexture> crts = GetAllCustomRenderTextures();
        foreach (CustomRenderTexture crt in crts)
        {
            crt.Initialize();
            crt.Update();
            EditorApplication.QueuePlayerLoopUpdate();
        }
    }

    public static List<CustomRenderTexture> GetAllCustomRenderTextures()
    {
        var result = new List<CustomRenderTexture>();

        string[] guids = AssetDatabase.FindAssets("t:CustomRenderTexture");

        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);

            var crt = AssetDatabase.LoadAssetAtPath<CustomRenderTexture>(path);
            if (crt != null)
                result.Add(crt);
        }

        return result;
    }
}
#endif
