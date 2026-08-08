#if UNITY_EDITOR
using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

[InitializeOnLoad]
public static class CustomRenderTexture_EditorHandler
{
    static CustomRenderTexture_EditorHandler()
    {
        EditorApplication.delayCall += Initialization;
    }

    static void Initialization()
    {
        EditorApplication.delayCall -= Initialization;

        List<CustomRenderTexture> crts = GetAllCustomRenderTextures();
        foreach (CustomRenderTexture crt in crts)
        {
            if (crt.initializationMode != CustomRenderTextureUpdateMode.OnDemand)
                crt.Initialize();
            if (crt.updateMode != CustomRenderTextureUpdateMode.OnDemand)
                crt.Update();
        }
    }

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
        List<CustomRenderTexture> result = new List<CustomRenderTexture>();

        string[] guids = AssetDatabase.FindAssets("t:CustomRenderTexture");

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);

            CustomRenderTexture crt = AssetDatabase.LoadAssetAtPath<CustomRenderTexture>(path);
            if (crt != null)
                result.Add(crt);
        }

        return result;
    }
}
#endif
