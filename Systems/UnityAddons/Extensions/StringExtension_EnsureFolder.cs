#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

public static class StringExtension_EnsureFolder
{
    public static bool EnsureAssetFolder(this string folderPath)
    {
        if (string.IsNullOrWhiteSpace(folderPath))
            return false;

        if (AssetDatabase.IsValidFolder(folderPath))
            return true;

        folderPath = folderPath.Replace("\\", "/");
        if (!folderPath.StartsWith("Assets", StringComparison.Ordinal))
        {
            Debug.LogError($"Folder path must be inside Assets/. Current value: {folderPath}");
            return false;
        }

        string[] parts = folderPath.Split('/');
        string current = parts[0];
        for (int i = 1; i < parts.Length; i++)
        {
            string next = $"{current}/{parts[i]}";
            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(current, parts[i]);
            current = next;
        }

        return true;
    }
}
#endif
