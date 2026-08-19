#if UNITY_EDITOR
using UnityEditor;

public static class SerializedPropertyExtension_GetParent
{
    public static SerializedProperty GetParent(this SerializedProperty property)
    {
        string path = property.propertyPath;
        path = path.Replace(".Array.data[", "[");

        int lastDot = path.LastIndexOf('.');

        return (lastDot < 0) ? property.serializedObject.GetIterator() :
            property.serializedObject.GetSerializedProperty(path[..lastDot]);
    }

    public static SerializedProperty FindProperty(this SerializedProperty property, string propertyPath)
    {
        bool isRoot = property.propertyPath.IsNullOrEmpty();
        return isRoot ? property.serializedObject.FindProperty(propertyPath) :
            property.FindPropertyRelative(propertyPath);
    }
}
#endif
