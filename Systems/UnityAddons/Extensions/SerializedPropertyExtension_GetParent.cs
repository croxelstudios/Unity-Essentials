#if UNITY_EDITOR
using System;
using UnityEditor;

public static class SerializedPropertyExtension_GetParent
{
    public static SerializedProperty GetParent(this SerializedProperty prop)
    {
        string propertyPath = prop.propertyPath;
        const string marker = ".Array.data";
        int idx = propertyPath.IndexOf(marker, StringComparison.Ordinal);
        if (idx == -1) return null;
        return prop.serializedObject.FindProperty(propertyPath.Substring(0, idx));
    }
}
#endif
