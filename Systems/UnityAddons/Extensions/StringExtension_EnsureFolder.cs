#if UNITY_EDITOR
using System.IO;
using UnityEditor;

public static class StringExtension_EnsureFolder
{
    public static void EnsureAssetFolder(this string folderPath)
    {
        if (folderPath.IsNullOrEmpty() ||
            folderPath == "Assets" ||
            AssetDatabase.IsValidFolder(folderPath))
            return;

        string parent = Path.GetDirectoryName(folderPath)?.Replace("\\", "/");
        string folderName = Path.GetFileName(folderPath);

        EnsureAssetFolder(parent);

        if (!AssetDatabase.IsValidFolder(folderPath))
            AssetDatabase.CreateFolder(parent, folderName);
    }
}
#endif
