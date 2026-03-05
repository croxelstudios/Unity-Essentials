#if UNITY_EDITOR
using UnityEditor;

public static class SerializedPropertyExtension_GetArrayElementIndex
{
    public static int GetArrayElementIndex(this SerializedProperty prop)
    {
        string path = prop.propertyPath;
        const string token = "Array.data[";
        int i = path.LastIndexOf(token);
        if (i == -1) return -1;
        i += token.Length;
        int j = path.IndexOf(']', i);
        if (j == -1) return -1;
        string num = path.Substring(i, j - i);
        if (int.TryParse(num, out var idx)) return idx;
        return -1;
    }
}
#endif
