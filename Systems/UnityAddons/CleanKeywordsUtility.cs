#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public static class CleanKeywordsUtility
{
    [MenuItem("Tools/Clean Material Keywords")]
    public static void CleanMaterialKeywords()
    {
        for (int i = 0; i < Selection.objects.Length; i++)
        {
            if (Selection.objects[i] is Shader shader)
            {
                Material[] materials = FindMaterialsWithShader(shader);
                foreach (Material mat in materials)
                {
                    CleanKeywords(mat);
                    EditorUtility.SetDirty(mat);
                }
                AssetDatabase.SaveAssets();
                Debug.Log("Cleaned material keywords in all materials using the shader: " + shader.name);
            }
            else if (Selection.objects[i] is Material material)
            {
                CleanKeywords(material);
                EditorUtility.SetDirty(material);
                AssetDatabase.SaveAssets();
                Debug.Log("Cleaned material keywords in material: " + material.name);
            }
            else Debug.LogWarning("Please select a Material or Shader asset in the Project view.");
        }
    }

    static void CleanKeywords(Material material)
    {
        material.shaderKeywords = null;
    }

    static Material[] FindMaterialsWithShader(Shader shader)
    {
        List<Material> result = new List<Material>();
        string[] guids = AssetDatabase.FindAssets("t:Material");

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);

            if (mat != null && mat.shader == shader)
                result.Add(mat);
        }

        return result.ToArray();
    }
}
#endif
