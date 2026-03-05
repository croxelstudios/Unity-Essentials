#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Reflection;

public static class SerializedPropertyExtension_GetMemberInfo
{
    public static MemberInfo GetMemberInfo(this SerializedProperty property)
    {
        return ReflectionTools.GetMemberInfo(
            property.serializedObject.targetObject, property.propertyPath);
    }
}
#endif
