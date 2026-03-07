#if UNITY_EDITOR
using UnityEditor;
using System.Reflection;

public static class SerializedPropertyExtension_GetMemberInfo
{
    public static MemberInfo GetMemberInfo(this SerializedProperty property,
        bool cacheObjectInfo = true,
        BindingFlags bindings = BindingFlags.Instance | BindingFlags.Static |
        BindingFlags.NonPublic | BindingFlags.Public)
    {
        return ReflectionTools.GetMemberInfo(
            property.serializedObject.targetObject, property.propertyPath, cacheObjectInfo, bindings);
    }

    public static MemberInfo GetMemberInfo(this SerializedProperty property, BindingFlags bindings)
    {
        return ReflectionTools.GetMemberInfo(
            property.serializedObject.targetObject, property.propertyPath, true, bindings);
    }
}
#endif
